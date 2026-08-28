using KubeUI.AI.Agents;
using KubeUI.Avalonia.Features.AI;
using KubeUI.Avalonia.Infrastructure.Mcp;
using KubeUI.Avalonia.Options;
using KubeUI.Avalonia.Services.Settings;
using System.Threading.Channels;
using Moq;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.AI;

public sealed class AgentChatViewModelTests
{
    [Fact]
    public async Task send_adds_user_message_and_streamed_agent_message()
    {
        await using var session = new TestSession("session-1", [new AgentMessageEvent(new AgentMessage("assistant", "Pod is healthy."))]);
        var agent = new TestAgent(session);
        var vm = new AgentChatViewModel(new TestRegistry(agent)) { Prompt = " Diagnose pod " };

        await vm.SendCommand.ExecuteAsync(null);

        vm.Messages.Select(message => message.Text).ShouldBe([" Diagnose pod ".Trim(), "Pod is healthy."]);
        session.Prompt.ShouldEndWith("User request:\nDiagnose pod");
        await vm.DisposeAsync();
    }

    [Fact]
    public async Task send_adds_kubeui_operating_guidance_to_agent_prompt()
    {
        await using var session = new TestSession("session-guidance", []);
        var vm = new AgentChatViewModel(new TestRegistry(new TestAgent(session))) { Prompt = "List pods" };

        await vm.SendCommand.ExecuteAsync(null);

        session.Prompt.ShouldContain("KubeUI MCP");
        session.Prompt.ShouldContain("FullApiVersion");
        session.Prompt.ShouldContain("pkg.crossplane.io/v1");
        session.Prompt.ShouldContain("ask for approval");
        session.Prompt.ShouldContain("List pods");
        await vm.DisposeAsync();
    }

    [Fact]
    public async Task send_coalesces_consecutive_assistant_chunks_into_one_message()
    {
        await using var session = new TestSession("session-stream", [
            new AgentMessageEvent(new AgentMessage("assistant", "I")),
            new AgentMessageEvent(new AgentMessage("assistant", "’m ")),
            new AgentMessageEvent(new AgentMessage("assistant", "checking the cluster.")),
            new AgentTurnCompletedEvent()
        ]);
        var vm = new AgentChatViewModel(new TestRegistry(new TestAgent(session))) { Prompt = "Inspect" };

        await vm.SendCommand.ExecuteAsync(null);

        vm.Messages.Select(message => message.Text).ShouldBe(["Inspect", "I’m checking the cluster."]);
        await vm.DisposeAsync();
    }

    [Fact]
    public async Task send_displays_streamed_message_before_agent_turn_completes()
    {
        await using var session = new StreamingTestSession();
        var vm = new AgentChatViewModel(new TestRegistry(new StreamingTestAgent(session))) { Prompt = "Inspect" };
        var sendTask = vm.SendCommand.ExecuteAsync(null);

        await session.FirstChunkSent.Task;

        vm.Messages.Select(message => message.Text).ShouldBe(["Inspect", "The response has started."]);
        sendTask.IsCompleted.ShouldBeFalse();

        session.Complete();
        await sendTask;
        await vm.DisposeAsync();
    }

    [Fact]
    public async Task send_renders_normalized_agent_activity_events()
    {
        await using var session = new TestSession("session-activity", [
            new AgentToolStartedEvent(new AgentToolCall("kubernetes.get", "{\"kind\":\"Pod\"}")),
            new AgentToolCompletedEvent(new AgentToolResult("kubernetes.get", true)),
            new AgentPlanChangedEvent(new AgentPlan(["Inspect pod"])),
            new AgentPermissionRequestedEvent(new AgentPermissionRequest("read_file", "pod.yaml")),
            new AgentUsageChangedEvent(new AgentUsage(2, 3, 5)),
            new AgentStatusEvent("Inspecting cluster"),
            new AgentTurnCompletedEvent()
        ]);
        var vm = new AgentChatViewModel(new TestRegistry(new TestAgent(session))) { Prompt = "Inspect" };

        await vm.SendCommand.ExecuteAsync(null);

        vm.Messages.Select(message => (message.Role, message.Text)).ShouldBe([
            ("You", "Inspect"),
            ("Tool", "▸ kubernetes.get {\"kind\":\"Pod\"}"),
            ("Tool", "✓ kubernetes.get"),
            ("Plan", "• Inspect pod"),
            ("Permission", "read_file: pod.yaml"),
        ]);
        await vm.DisposeAsync();
    }

    [Fact]
    public async Task blank_prompt_does_not_create_session()
    {
        await using var session = new TestSession("unused", []);
        var agent = new TestAgent(session);
        var vm = new AgentChatViewModel(new TestRegistry(agent)) { Prompt = "  " };

        await vm.SendCommand.ExecuteAsync(null);

        agent.CreateSessionCount.ShouldBe(0);
        vm.Messages.ShouldBeEmpty();
        await vm.DisposeAsync();
    }

    [Fact]
    public async Task send_stops_reading_events_at_turn_completion_without_disposing_session()
    {
        await using var session = new TestSession("session-2", [
            new AgentMessageEvent(new AgentMessage("assistant", "Done.")),
            new AgentTurnCompletedEvent()
        ], keepOpenAfterCompletion: true);
        var vm = new AgentChatViewModel(new TestRegistry(new TestAgent(session))) { Prompt = "Check pod" };

        await vm.SendCommand.ExecuteAsync(null);

        vm.IsBusy.ShouldBeFalse();
        vm.Messages.Select(message => message.Text).ShouldBe(["Check pod", "Done."]);
        session.WasDisposed.ShouldBeFalse();
        await vm.DisposeAsync();
    }

    [Fact]
    public async Task send_passes_selected_kubernetes_context_to_agent_session()
    {
        await using var session = new TestSession("session-3", []);
        var agent = new TestAgent(session);
        var context = new AgentContext
        {
            Namespace = "default",
            SelectedResources = [new KubernetesResourceReference("v1", "Pod", "api", "default")]
        };
        var vm = new AgentChatViewModel(new TestRegistry(agent)) { Prompt = "Diagnose", Context = context };

        await vm.SendCommand.ExecuteAsync(null);

        agent.Options!.Context.ShouldBe(context);
        await vm.DisposeAsync();
    }

    [Fact]
    public async Task send_passes_all_selected_kubernetes_resources_to_agent_session()
    {
        await using var session = new TestSession("session-multiple-context", []);
        var agent = new TestAgent(session);
        var context = new AgentContext
        {
            Namespace = "default",
            SelectedResources =
            [
                new KubernetesResourceReference("v1", "Pod", "api", "default"),
                new KubernetesResourceReference("v1", "Pod", "worker", "default")
            ]
        };
        var vm = new AgentChatViewModel(new TestRegistry(agent)) { Prompt = "Inspect", Context = context };

        await vm.SendCommand.ExecuteAsync(null);

        agent.Options!.Context!.SelectedResources.ShouldBe(context.SelectedResources);
        await vm.DisposeAsync();
    }

    [Fact]
    public async Task selected_agent_follows_persisted_settings()
    {
        await using var firstSession = new TestSession("first-session", []);
        await using var secondSession = new TestSession("second-session", []);
        var first = new TestAgent(firstSession, "first", "First");
        var second = new TestAgent(secondSession, "second", "Second");
        var settings = new Settings { SelectedAgentId = "second" };
        var settingsService = new Mock<ISettingsService>();
        settingsService.SetupGet(service => service.Settings).Returns(settings);
        var vm = new AgentChatViewModel(new TestRegistry(first, second), settingsService.Object);

        vm.SelectedAgent.ShouldBeSameAs(second);
        settings.SelectedAgentId = "first";
        vm.SelectedAgent.ShouldBeSameAs(first);

        await vm.DisposeAsync();
    }

    [Fact]
    public async Task cancel_command_cancels_the_active_agent_turn()
    {
        await using var session = new BlockingTestSession();
        var vm = new AgentChatViewModel(new TestRegistry(new BlockingTestAgent(session))) { Prompt = "Stop" };
        var sendTask = vm.SendCommand.ExecuteAsync(null);
        await session.Started.Task;

        await vm.CancelCommand.ExecuteAsync(null);
        await sendTask;

        session.CancelCalled.ShouldBeTrue();
        vm.IsBusy.ShouldBeFalse();
        vm.Messages.Select(message => message.Text).ShouldContain("Canceled");
        await vm.DisposeAsync();
    }

    [Fact]
    public async Task send_surfaces_agent_startup_errors_and_clears_busy_state()
    {
        var vm = new AgentChatViewModel(new TestRegistry(new FailingAgent("Authentication required.")))
        {
            Prompt = "Connect"
        };

        await vm.SendCommand.ExecuteAsync(null);

        vm.IsBusy.ShouldBeFalse();
        vm.Messages.Select(message => message.Text).ShouldBe(["Connect", "Authentication required."]);
        await vm.DisposeAsync();
    }

    [Fact]
    public async Task send_passes_the_bound_mcp_endpoint_to_the_agent_session()
    {
        await using var session = new TestSession("session-bound-endpoint", []);
        var agent = new TestAgent(session);
        var settings = new Settings { McpServerEnabled = true, McpServerPort = 62888 };
        var settingsService = new Mock<ISettingsService>();
        settingsService.SetupGet(service => service.Settings).Returns(settings);
        var mcpServerState = new McpServerState { BoundPort = 54321 };
        var vm = new AgentChatViewModel(
            new TestRegistry(agent),
            settingsService.Object,
            mcpServerState: mcpServerState) { Prompt = "List pods" };

        await vm.SendCommand.ExecuteAsync(null);

        agent.Options!.McpEndpoint.ShouldBe("http://127.0.0.1:54321/mcp");
        await vm.DisposeAsync();
    }

    private sealed class TestRegistry(params IAgent[] agents) : IAgentRegistry
    {
        public IReadOnlyList<IAgent> Agents => agents;
    }

    private sealed class TestAgent(TestSession session, string id = "test", string name = "Test") : IAgent
    {
        public int CreateSessionCount { get; private set; }
        public AgentSessionOptions? Options { get; private set; }
        public string Id => id;
        public string Name => name;
        public AgentCapabilities Capabilities => AgentCapabilities.None;
        public Task<IAgentSession> CreateSessionAsync(AgentSessionOptions options, CancellationToken cancellationToken = default)
        {
            CreateSessionCount++;
            Options = options;
            return Task.FromResult<IAgentSession>(session);
        }

    }

    private sealed class BlockingTestAgent(BlockingTestSession session) : IAgent
    {
        public string Id => "blocking";
        public string Name => "Blocking";
        public AgentCapabilities Capabilities => AgentCapabilities.None;
        public Task<IAgentSession> CreateSessionAsync(AgentSessionOptions options, CancellationToken cancellationToken = default)
            => Task.FromResult<IAgentSession>(session);
    }

    private sealed class FailingAgent(string message) : IAgent
    {
        public string Id => "failing";
        public string Name => "Failing";
        public AgentCapabilities Capabilities => AgentCapabilities.None;

        public Task<IAgentSession> CreateSessionAsync(AgentSessionOptions options, CancellationToken cancellationToken = default)
            => Task.FromException<IAgentSession>(new InvalidOperationException(message));
    }

    private sealed class BlockingTestSession : IAgentSession
    {
        private readonly TaskCompletionSource _canceled = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public bool CancelCalled { get; private set; }
        public string Id => "blocking-session";
        public IAsyncEnumerable<AgentEvent> Events => EmptyEvents();

        public async Task PromptAsync(string prompt, CancellationToken cancellationToken = default)
        {
            Started.TrySetResult();
            await _canceled.Task.WaitAsync(cancellationToken);
        }

        public Task CancelAsync(CancellationToken cancellationToken = default)
        {
            CancelCalled = true;
            _canceled.TrySetResult();
            return Task.CompletedTask;
        }

        public ValueTask DisposeAsync()
        {
            _canceled.TrySetResult();
            return ValueTask.CompletedTask;
        }

        private static async IAsyncEnumerable<AgentEvent> EmptyEvents()
        {
            await Task.CompletedTask;
            yield break;
        }
    }

    private sealed class TestSession(string id, IReadOnlyList<AgentEvent> events, bool keepOpenAfterCompletion = false) : IAgentSession
    {
        private readonly TaskCompletionSource _neverCompletes = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public string Id => id;
        public string? Prompt { get; private set; }
        public bool WasDisposed { get; private set; }
        public IAsyncEnumerable<AgentEvent> Events => ReadEventsAsync();

        private async IAsyncEnumerable<AgentEvent> ReadEventsAsync()
        {
            foreach (var item in events)
            {
                yield return item;
                await Task.Yield();
            }

            if (keepOpenAfterCompletion)
                await _neverCompletes.Task;
        }

        public Task PromptAsync(string prompt, CancellationToken cancellationToken = default)
        {
            Prompt = prompt;
            return Task.CompletedTask;
        }

        public Task CancelAsync(CancellationToken cancellationToken = default) => Task.CompletedTask;
        public ValueTask DisposeAsync()
        {
            WasDisposed = true;
            _neverCompletes.TrySetResult();
            return ValueTask.CompletedTask;
        }
    }

    private sealed class StreamingTestAgent(StreamingTestSession session) : IAgent
    {
        public string Id => "streaming";
        public string Name => "Streaming";
        public AgentCapabilities Capabilities => AgentCapabilities.None;

        public Task<IAgentSession> CreateSessionAsync(AgentSessionOptions options, CancellationToken cancellationToken = default)
            => Task.FromResult<IAgentSession>(session);
    }

    private sealed class StreamingTestSession : IAgentSession
    {
        private readonly Channel<AgentEvent> _events = Channel.CreateUnbounded<AgentEvent>();
        private readonly TaskCompletionSource _completion = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource FirstChunkSent { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public string Id => "streaming-session";
        public IAsyncEnumerable<AgentEvent> Events => _events.Reader.ReadAllAsync();

        public async Task PromptAsync(string prompt, CancellationToken cancellationToken = default)
        {
            await _events.Writer.WriteAsync(
                new AgentMessageEvent(new AgentMessage("assistant", "The response has started.")),
                cancellationToken);
            FirstChunkSent.TrySetResult();
            await _completion.Task.WaitAsync(cancellationToken);
            await _events.Writer.WriteAsync(new AgentTurnCompletedEvent(), cancellationToken);
        }

        public Task CancelAsync(CancellationToken cancellationToken = default)
        {
            _completion.TrySetResult();
            return Task.CompletedTask;
        }

        public ValueTask DisposeAsync()
        {
            _completion.TrySetResult();
            _events.Writer.TryComplete();
            return ValueTask.CompletedTask;
        }

        public void Complete() => _completion.TrySetResult();
    }
}
