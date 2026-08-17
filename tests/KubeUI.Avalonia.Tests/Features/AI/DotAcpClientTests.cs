using System.Threading.Channels;
using dotacp.protocol;
using KubeUI.AI.Acp;
using KubeUI.AI.Agents;
using KubeUI.AI.Configuration;
using KubeUI.AI.Permissions;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.AI;

public sealed class DotAcpClientTests
{
    [Fact]
    public async Task registry_refresh_keeps_available_agents_and_removes_missing_executables()
    {
        var registry = new AcpAgentRegistry
        ([
            new AcpAgentDefinition
            {
                Id = "available",
                Name = "Available",
                Executable = Environment.ProcessPath!
            },
            new AcpAgentDefinition
            {
                Id = "missing",
                Name = "Missing",
                Executable = "executable-that-does-not-exist-kubeui-test"
            }
        ]);

        registry.Agents.Select(agent => agent.Id).ShouldBe(["available"]);
    }

    [Fact]
    public async Task session_update_maps_tool_progress_to_domain_events()
    {
        var events = Channel.CreateUnbounded<AgentEvent>();
        using var client = new DotAcpClient(events.Writer);

        await client.SessionUpdateAsync(new SessionNotification
        {
            Update = new SessionUpdateToolCallUpdate
            {
                Title = "kubernetes.get",
                Status = ToolCallStatus.InProgress
            }
        });

        var result = await events.Reader.ReadAsync();
        result.ShouldBeOfType<AgentToolStartedEvent>().Tool.Name.ShouldBe("kubernetes.get");
    }

    [Fact]
    public async Task session_update_maps_tool_call_start_to_domain_event()
    {
        var events = Channel.CreateUnbounded<AgentEvent>();
        using var client = new DotAcpClient(events.Writer);

        await client.SessionUpdateAsync(new SessionNotification
        {
            Update = new ToolCall
            {
                Title = "kubernetes.list",
                RawInput = new { Kind = "Pod" }
            }
        });

        var result = await events.Reader.ReadAsync().AsTask().WaitAsync(TimeSpan.FromSeconds(1));
        result.ShouldBeOfType<AgentToolStartedEvent>().Tool.Name.ShouldBe("kubernetes.list");
    }

    [Fact]
    public async Task session_update_maps_assistant_message_and_thought_to_domain_events()
    {
        var events = Channel.CreateUnbounded<AgentEvent>();
        using var client = new DotAcpClient(events.Writer);

        await client.SessionUpdateAsync(new SessionNotification
        {
            Update = new SessionUpdateAgentMessageChunk
            {
                Content = new TextContent { Text = "response" }
            }
        });
        await client.SessionUpdateAsync(new SessionNotification
        {
            Update = new SessionUpdateAgentThoughtChunk
            {
                Content = new TextContent { Text = "thinking" }
            }
        });

        (await events.Reader.ReadAsync()).ShouldBeOfType<AgentMessageEvent>().Message.Text.ShouldBe("response");
        (await events.Reader.ReadAsync()).ShouldBeOfType<AgentStatusEvent>().Text.ShouldBe("thinking");
    }

    [Fact]
    public async Task session_update_maps_completed_and_failed_tools_to_completion_events()
    {
        var events = Channel.CreateUnbounded<AgentEvent>();
        using var client = new DotAcpClient(events.Writer);

        await client.SessionUpdateAsync(new SessionNotification
        {
            Update = new SessionUpdateToolCallUpdate
            {
                Title = "kubernetes.get",
                Status = ToolCallStatus.Completed,
                RawOutput = new { Name = "pod-a" }
            }
        });
        await client.SessionUpdateAsync(new SessionNotification
        {
            Update = new SessionUpdateToolCallUpdate
            {
                Title = "kubernetes.logs",
                Status = ToolCallStatus.Failed,
                RawOutput = new { Error = "unavailable" }
            }
        });

        var completed = (await events.Reader.ReadAsync()).ShouldBeOfType<AgentToolCompletedEvent>();
        completed.Result.Name.ShouldBe("kubernetes.get");
        completed.Result.Succeeded.ShouldBeTrue();
        completed.Result.Output.ShouldContain("pod-a");

        var failed = (await events.Reader.ReadAsync()).ShouldBeOfType<AgentToolCompletedEvent>();
        failed.Result.Name.ShouldBe("kubernetes.logs");
        failed.Result.Succeeded.ShouldBeFalse();
        failed.Result.Output.ShouldContain("unavailable");
    }

    [Fact]
    public async Task session_update_maps_an_already_completed_tool_call_to_a_completion_event()
    {
        var events = Channel.CreateUnbounded<AgentEvent>();
        using var client = new DotAcpClient(events.Writer);

        await client.SessionUpdateAsync(new SessionNotification
        {
            Update = new ToolCall
            {
                Title = "kubernetes.get",
                Status = ToolCallStatus.Completed,
                RawOutput = new { Name = "pod-a" }
            }
        });

        var result = (await events.Reader.ReadAsync()).ShouldBeOfType<AgentToolCompletedEvent>();
        result.Result.Name.ShouldBe("kubernetes.get");
        result.Result.Succeeded.ShouldBeTrue();
        result.Result.Output.ShouldContain("pod-a");
    }

    [Fact]
    public async Task session_update_maps_user_and_session_metadata_variants_to_domain_events()
    {
        var events = Channel.CreateUnbounded<AgentEvent>();
        using var client = new DotAcpClient(events.Writer);

        await client.SessionUpdateAsync(new SessionNotification
        {
            Update = new SessionUpdateUserMessageChunk
            {
                Content = new TextContent { Text = "user message" }
            }
        });
        await client.SessionUpdateAsync(new SessionNotification
        {
            Update = new AvailableCommandsUpdate
            {
                AvailableCommands = [new AvailableCommand { Name = "help" }]
            }
        });
        await client.SessionUpdateAsync(new SessionNotification
        {
            Update = new ConfigOptionUpdate
            {
                ConfigOptions = [new SessionConfigBoolean { Name = "model" }]
            }
        });
        await client.SessionUpdateAsync(new SessionNotification
        {
            Update = new CurrentModeUpdate { CurrentModeId = new SessionModeId("plan") }
        });
        await client.SessionUpdateAsync(new SessionNotification
        {
            Update = new SessionInfoUpdate { Title = "Investigating pod" }
        });

        (await events.Reader.ReadAsync()).ShouldBeOfType<AgentMessageEvent>().Message.ShouldBe(new AgentMessage("user", "user message"));
        (await events.Reader.ReadAsync()).ShouldBeOfType<AgentStatusEvent>().Text.ShouldBe("Available commands: help");
        (await events.Reader.ReadAsync()).ShouldBeOfType<AgentStatusEvent>().Text.ShouldBe("Configuration options: model");
        (await events.Reader.ReadAsync()).ShouldBeOfType<AgentStatusEvent>().Text.ShouldBe("Current mode: plan");
        (await events.Reader.ReadAsync()).ShouldBeOfType<AgentStatusEvent>().Text.ShouldBe("Investigating pod");
    }

    [Fact]
    public async Task permission_request_selects_allow_option_when_permission_service_allows()
    {
        var events = Channel.CreateUnbounded<AgentEvent>();
        using var client = new DotAcpClient(events.Writer, new AllowAgentPermissionService());

        var response = await client.RequestPermissionAsync(new RequestPermissionRequest
        {
            Options =
            [
                new PermissionOption { Kind = PermissionOptionKind.RejectOnce, Name = "Deny", OptionId = "deny" },
                new PermissionOption { Kind = PermissionOptionKind.AllowOnce, Name = "Allow", OptionId = "allow" }
            ],
            ToolCall = new ToolCallUpdate { Title = "write file", Kind = ToolKind.Edit }
        });

        response.Outcome.ShouldBeOfType<SelectedPermissionOutcome>().OptionId.ToString().ShouldBe("allow");
        (await events.Reader.ReadAsync()).ShouldBeOfType<AgentPermissionRequestedEvent>().Request.Action.ShouldBe("write file");
    }

    [Fact]
    public async Task permission_request_describes_tool_when_title_is_missing()
    {
        var events = Channel.CreateUnbounded<AgentEvent>();
        using var client = new DotAcpClient(events.Writer, new AllowAgentPermissionService());

        await client.RequestPermissionAsync(new RequestPermissionRequest
        {
            Options =
            [
                new PermissionOption { Kind = PermissionOptionKind.AllowOnce, Name = "Allow", OptionId = "allow" }
            ],
            ToolCall = new ToolCallUpdate
            {
                Kind = ToolKind.Execute,
                RawInput = new { Command = "kubectl get pods" }
            }
        });

        var request = (await events.Reader.ReadAsync()).ShouldBeOfType<AgentPermissionRequestedEvent>().Request;
        request.Action.ShouldBe("Execute");
        request.Resource.ShouldContain("kubectl get pods");
    }

    [Fact]
    public async Task read_only_tool_permission_is_auto_allowed_without_prompt()
    {
        var events = Channel.CreateUnbounded<AgentEvent>();
        using var client = new DotAcpClient(events.Writer);

        var response = await client.RequestPermissionAsync(new RequestPermissionRequest
        {
            Options =
            [
                new PermissionOption { Kind = PermissionOptionKind.AllowOnce, Name = "Allow", OptionId = "allow" },
                new PermissionOption { Kind = PermissionOptionKind.RejectOnce, Name = "Deny", OptionId = "deny" }
            ],
            ToolCall = new ToolCallUpdate
            {
                Title = "kubeui_list_clusters",
                Kind = ToolKind.Other
            }
        });

        response.Outcome.ShouldBeOfType<SelectedPermissionOutcome>().OptionId.ToString().ShouldBe("allow");
        events.Reader.TryRead(out _).ShouldBeFalse();
    }

    [Fact]
    public async Task permission_request_uses_mcp_tool_metadata_to_describe_the_tool()
    {
        var events = Channel.CreateUnbounded<AgentEvent>();
        using var client = new DotAcpClient(events.Writer, trustedMcpServers: new HashSet<string>(StringComparer.Ordinal) { "kubeui" });

        await client.SessionUpdateAsync(new SessionNotification
        {
            Update = new ToolCall
            {
                ToolCallId = "connect",
                Kind = ToolKind.Execute,
                RawInput = new { server = "kubeui", tool = "kubeui_connect_cluster" },
                Meta = new Dictionary<string, object> { ["is_mcp_tool_call"] = true }
            }
        });
        events.Reader.TryRead(out _).ShouldBeTrue();

        var response = await client.RequestPermissionAsync(new RequestPermissionRequest
        {
            Options =
            [
                new PermissionOption { Kind = PermissionOptionKind.AllowOnce, Name = "Allow", OptionId = "allow" },
                new PermissionOption { Kind = PermissionOptionKind.RejectOnce, Name = "Deny", OptionId = "deny" }
            ],
            ToolCall = new ToolCallUpdate
            {
                ToolCallId = "connect",
                Kind = ToolKind.Execute,
            }
        });

        response.Outcome.ShouldBeOfType<SelectedPermissionOutcome>().OptionId.ToString().ShouldBe("allow");
        events.Reader.TryRead(out _).ShouldBeFalse();
    }

    [Fact]
    public async Task external_mcp_tool_requires_permission_even_when_it_is_read_only()
    {
        var events = Channel.CreateUnbounded<AgentEvent>();
        using var client = new DotAcpClient(events.Writer, new AllowAgentPermissionService(), new HashSet<string>(StringComparer.Ordinal) { "kubeui" });

        await client.SessionUpdateAsync(new SessionNotification
        {
            Update = new ToolCall
            {
                ToolCallId = "external",
                Kind = ToolKind.Execute,
                RawInput = new { server = "filesystem", tool = "list_directory", arguments = new { path = "." } },
                Meta = new Dictionary<string, object> { ["is_mcp_tool_call"] = true }
            }
        });
        events.Reader.TryRead(out _).ShouldBeTrue();

        var response = await client.RequestPermissionAsync(new RequestPermissionRequest
        {
            Options =
            [
                new PermissionOption { Kind = PermissionOptionKind.AllowOnce, Name = "Allow", OptionId = "allow" },
                new PermissionOption { Kind = PermissionOptionKind.RejectOnce, Name = "Deny", OptionId = "deny" }
            ],
            ToolCall = new ToolCallUpdate { ToolCallId = "external", Kind = ToolKind.Execute }
        });

        response.Outcome.ShouldBeOfType<SelectedPermissionOutcome>().OptionId.ToString().ShouldBe("allow");
        var permission = (await events.Reader.ReadAsync()).ShouldBeOfType<AgentPermissionRequestedEvent>().Request;
        permission.Action.ShouldBe("MCP filesystem/list_directory");
        permission.IsDestructive.ShouldBeFalse();
        permission.RequiresApproval.ShouldBeTrue();
    }

    [Fact]
    public async Task session_update_maps_plan_and_usage_progress_to_domain_events()
    {
        var events = Channel.CreateUnbounded<AgentEvent>();
        using var client = new DotAcpClient(events.Writer);

        await client.SessionUpdateAsync(new SessionNotification
        {
            Update = new Plan
            {
                Entries = [new PlanEntry { Content = "Inspect pod" }]
            }
        });
        await client.SessionUpdateAsync(new SessionNotification
        {
            Update = new UsageUpdate { Used = 12, Size = 100 }
        });

        var planEvent = (await events.Reader.ReadAsync()).ShouldBeOfType<AgentPlanChangedEvent>();
        planEvent.Plan.Steps.ShouldBe(["Inspect pod"]);
        var usageEvent = (await events.Reader.ReadAsync()).ShouldBeOfType<AgentUsageChangedEvent>();
        usageEvent.Usage.ShouldBe(new AgentUsage(0, 0, 12));
    }

    [Fact]
    public async Task acp_process_exposes_stderr_without_mixing_it_into_protocol_output()
    {
        var (command, arguments) = GetEchoCommand("kubeui-stderr", standardError: true);
        await using var process = new AcpProcess(
            new AcpAgentDefinition
            {
                Id = "stderr-test",
                Name = "stderr test",
                Executable = command,
                Arguments = arguments
            },
            new AgentSessionOptions());
        var error = new TaskCompletionSource<string>(TaskCreationOptions.RunContinuationsAsynchronously);
        process.ErrorReceived += line => error.TrySetResult(line);

        await process.StartAsync(CancellationToken.None);
        var message = await error.Task.WaitAsync(TimeSpan.FromSeconds(5));

        message.ShouldNotBeNullOrWhiteSpace();
        process.Input.ShouldNotBeNull();
        process.Output.ShouldNotBeNull();
    }

    [Fact]
    public async Task terminal_callbacks_start_capture_wait_and_release_a_permissioned_process()
    {
        var (command, arguments) = GetEchoCommand("kubeui-terminal");
        var events = Channel.CreateUnbounded<AgentEvent>();
        using var client = new DotAcpClient(events.Writer, new AllowAgentPermissionService());
        var terminal = await client.CreateTerminalAsync(new CreateTerminalRequest
        {
            Command = command,
            Args = arguments,
            OutputByteLimit = 1024
        });

        var completed = await client.WaitForTerminalExitAsync(new WaitForTerminalExitRequest { TerminalId = terminal.TerminalId });
        var output = await client.TerminalOutputAsync(new TerminalOutputRequest { TerminalId = terminal.TerminalId });

        completed.ExitCode.ShouldBe(0u);
        output.Output.ShouldContain("kubeui-terminal");
        await client.ReleaseTerminalAsync(new ReleaseTerminalRequest { TerminalId = terminal.TerminalId });
    }

    private static (string Command, string[] Arguments) GetEchoCommand(string text, bool standardError = false)
    {
        var commandLine = standardError ? $"echo {text} 1>&2" : $"echo {text}";
        return OperatingSystem.IsWindows()
            ? (Environment.GetEnvironmentVariable("ComSpec") ?? "cmd.exe", ["/c", commandLine])
            : ("/bin/sh", ["-c", commandLine]);
    }

    [Fact]
    public async Task file_callbacks_apply_permission_and_read_line_limits()
    {
        var path = Path.Combine(Path.GetTempPath(), $"kubeui-acp-{Guid.NewGuid():N}.txt");
        try
        {
            await File.WriteAllTextAsync(path, "first\nsecond\nthird");
            using var client = new DotAcpClient(
                Channel.CreateUnbounded<AgentEvent>().Writer,
                new AllowAgentPermissionService());

            var response = await client.ReadTextFileAsync(new ReadTextFileRequest { Path = path, Line = 1, Limit = 1 });
            response.Content.ShouldBe("second");

            await client.WriteTextFileAsync(new WriteTextFileRequest { Path = path, Content = "updated" });
            (await File.ReadAllTextAsync(path)).ShouldBe("updated");
        }
        finally
        {
            File.Delete(path);
        }
    }

    [Fact]
    public async Task file_callbacks_deny_access_when_permission_service_denies()
    {
        using var client = new DotAcpClient(Channel.CreateUnbounded<AgentEvent>().Writer);

        await Should.ThrowAsync<UnauthorizedAccessException>(() => client.ReadTextFileAsync(
            new ReadTextFileRequest { Path = Path.Combine(Path.GetTempPath(), "not-read.txt") }));
    }

    private sealed class AllowAgentPermissionService : IAgentPermissionService
    {
        public Task<AgentPermissionResult> RequestPermissionAsync(AgentPermissionRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(new AgentPermissionResult(true));
    }
}
