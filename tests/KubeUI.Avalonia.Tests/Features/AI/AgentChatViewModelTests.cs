using System.Text.Json;
using System.Threading.Channels;
using KubeUI.AI.Agents;
using KubeUI.Avalonia.Features.AI;
using KubeUI.Avalonia.Infrastructure.Mcp;
using Microsoft.Extensions.Logging;
using Moq;
using Shouldly;
using SvcSystems.AI.AgentClientProtocol;
using SvcSystems.AI.AgentClientProtocol.Agent.Models;
using SvcSystems.AI.AgentClientProtocol.Models;

namespace KubeUI.Avalonia.Tests.Features.AI;

public sealed class AgentChatViewModelTests
{
    [Fact]
    public async Task send_adds_user_message_and_streamed_agent_message()
    {
        await using var session = new TestSession("session-1", [AssistantChunk("Pod is healthy.")]);
        var agent = new TestAgent(session);
        var vm = CreateViewModel(new TestRegistry(agent), prompt: " Diagnose pod ");

        await vm.SendCommand.ExecuteAsync(null);

        vm.Messages.Select(message => message.Text).ShouldBe([" Diagnose pod ".Trim(), "Pod is healthy."]);
        session.Prompt.ShouldEndWith("User request:\nDiagnose pod");
        await vm.DisposeAsync();
    }

    [Fact]
    public async Task send_logs_acp_session_creation_exception()
    {
        var exception = new InvalidOperationException("ACP client initialization failed.");
        await using var session = new TestSession("failed-session", []);
        var logger = new Mock<ILogger<AgentChatViewModel>>();
        var vm = CreateViewModel(
            new TestRegistry(new TestAgent(session, sessionCreationException: exception)),
            prompt: "Connect to cluster",
            logger: logger.Object);

        await vm.SendCommand.ExecuteAsync(null);

        vm.Messages.Last().Text.ShouldContain(exception.Message);
        logger.Verify(log => log.Log(
            LogLevel.Error,
            It.IsAny<EventId>(),
            It.Is<It.IsAnyType>((state, _) => state.ToString()!.Contains("agent test", StringComparison.Ordinal)),
            exception,
            It.IsAny<Func<It.IsAnyType, Exception?, string>>()), Times.Once);
        await vm.DisposeAsync();
    }

    [Fact]
    public async Task send_adds_kubeui_operating_guidance_to_agent_prompt()
    {
        await using var session = new TestSession("session-guidance", []);
        var vm = CreateViewModel(new TestRegistry(new TestAgent(session)), prompt: "List pods");

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
            AssistantChunk("I"), AssistantChunk("’m "), AssistantChunk("checking the cluster."), TurnCompleted()
        ]);
        var vm = CreateViewModel(new TestRegistry(new TestAgent(session)), prompt: "Inspect");

        await vm.SendCommand.ExecuteAsync(null);

        vm.Messages.Select(message => message.Text).ShouldBe(["Inspect", "I’m checking the cluster."]);
        await vm.DisposeAsync();
    }

    [Fact]
    public async Task send_displays_streamed_message_before_agent_turn_completes()
    {
        await using var session = new StreamingTestSession();
        var vm = CreateViewModel(new TestRegistry(new StreamingTestAgent(session)), prompt: "Inspect");
        var sendTask = vm.SendCommand.ExecuteAsync(null);

        await session.FirstChunkSent.Task;

        vm.Messages.Select(message => message.Text).ShouldBe(["Inspect", "The response has started."]);
        sendTask.IsCompleted.ShouldBeFalse();

        session.Complete();
        await sendTask;
        await vm.DisposeAsync();
    }

    [Fact]
    public async Task send_renders_protocol_agent_activity_events()
    {
        await using var session = new TestSession("session-activity", [
            new SessionUpdateEvent(new SessionUpdate(new ToolCall { ToolCallId = "call-1", Title = "kubernetes.get" })),
            new SessionUpdateEvent(new SessionUpdate(new ToolCallUpdate { ToolCallId = "call-1", Status = "completed", Title = "kubernetes.get" })),
            new SessionUpdateEvent(new SessionUpdate(new Plan { Entries = [new PlanEntry { Content = "Inspect pod", Priority = "high", Status = "pending" }] })),
            TurnCompleted()
        ]);
        var vm = CreateViewModel(new TestRegistry(new TestAgent(session)), prompt: "Inspect");

        await vm.SendCommand.ExecuteAsync(null);

        vm.Messages.Select(message => (message.Role, message.Text)).ShouldBe([
            ("You", "Inspect"),
            ("Tool", "▸ kubernetes.get"),
            ("Tool", "✓ kubernetes.get"),
            ("Plan", "• Inspect pod"),
        ]);
        await vm.DisposeAsync();
    }

    [Fact]
    public async Task send_renders_tool_call_raw_input()
    {
        await using var session = new TestSession("session-tool-input", [
            new SessionUpdateEvent(new SessionUpdate(new ToolCall
            {
                ToolCallId = "call-1",
                Title = "kubernetes.get",
                RawInput = JsonDocument.Parse("""{"namespace":"default"}""").RootElement
            })),
            TurnCompleted()
        ]);
        var vm = CreateViewModel(new TestRegistry(new TestAgent(session)), prompt: "Inspect");

        await vm.SendCommand.ExecuteAsync(null);

        vm.Messages.Select(message => message.Text).ShouldBe([
            "Inspect",
            """▸ kubernetes.get {"namespace":"default"}""",
        ]);
        await vm.DisposeAsync();
    }

    [Fact]
    public async Task send_omits_tool_input_when_agent_does_not_supply_it()
    {
        await using var session = new TestSession("session-no-tool-input", [
            new SessionUpdateEvent(new SessionUpdate(new ToolCall { ToolCallId = "call-1", Title = "kubernetes.get" })),
            TurnCompleted()
        ]);
        var vm = CreateViewModel(new TestRegistry(new TestAgent(session)), prompt: "Inspect");

        await vm.SendCommand.ExecuteAsync(null);

        vm.Messages.Select(message => message.Text).ShouldBe([
            "Inspect",
            "▸ kubernetes.get",
        ]);
        await vm.DisposeAsync();
    }

    [Fact]
    public async Task blank_prompt_does_not_create_session()
    {
        await using var session = new TestSession("unused", []);
        var agent = new TestAgent(session);
        var vm = CreateViewModel(new TestRegistry(agent), prompt: "  ");

        await vm.SendCommand.ExecuteAsync(null);

        agent.CreateSessionCount.ShouldBe(0);
        vm.Messages.ShouldBeEmpty();
        await vm.DisposeAsync();
    }

    [Fact]
    public async Task send_stops_reading_events_at_turn_completion_without_disposing_session()
    {
        await using var session = new TestSession("session-2", [
            AssistantChunk("Done."), TurnCompleted()
        ], keepOpenAfterCompletion: true);
        var vm = CreateViewModel(new TestRegistry(new TestAgent(session)), prompt: "Check pod");

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
        var vm = CreateViewModel(new TestRegistry(agent), prompt: "Diagnose", context: context);

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
        var vm = CreateViewModel(new TestRegistry(agent), prompt: "Inspect", context: context);

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
        var vm = CreateViewModel(new TestRegistry(first, second), settingsService.Object);

        vm.SelectedAgent.ShouldBeSameAs(second);
        settings.SelectedAgentId = "first";
        vm.SelectedAgent.ShouldBeSameAs(first);

        await vm.DisposeAsync();
    }

    [Fact]
    public async Task cancel_command_cancels_the_active_agent_turn()
    {
        await using var session = new BlockingTestSession();
        var vm = CreateViewModel(new TestRegistry(new BlockingTestAgent(session)), prompt: "Stop");
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
        var vm = CreateViewModel(
            new TestRegistry(new FailingAgent("Authentication required.")),
            prompt: "Connect");

        await vm.SendCommand.ExecuteAsync(null);

        vm.IsBusy.ShouldBeFalse();
        vm.Messages.Select(message => message.Text).ShouldBe(["Connect", "Authentication required."]);
        await vm.DisposeAsync();
    }

    [Fact]
    public async Task send_passes_the_configured_mcp_endpoint_to_the_agent_session()
    {
        await using var session = new TestSession("session-bound-endpoint", []);
        var agent = new TestAgent(session);
        var settings = new Settings { McpServerEnabled = true, McpServerPort = 62888 };
        var settingsService = new Mock<ISettingsService>();
        settingsService.SetupGet(service => service.Settings).Returns(settings);
        var mcpServerState = new McpServerState();
        mcpServerState.SetBoundPort(54321);
        var vm = CreateViewModel(
            new TestRegistry(agent),
            settingsService.Object,
            new AgentContextService(),
            mcpServerState,
            prompt: "List pods");

        await vm.SendCommand.ExecuteAsync(null);

        agent.Options!.McpEndpoint.ShouldBe("http://127.0.0.1:54321/mcp");
        await vm.DisposeAsync();
    }

    [Fact]
    public async Task send_omits_the_mcp_endpoint_when_the_server_is_disabled()
    {
        await using var session = new TestSession("session-disabled-endpoint", []);
        var agent = new TestAgent(session);
        var settings = new Settings { McpServerEnabled = false, McpServerPort = 62888 };
        var settingsService = new Mock<ISettingsService>();
        settingsService.SetupGet(service => service.Settings).Returns(settings);
        var vm = CreateViewModel(new TestRegistry(agent), settingsService.Object, prompt: "List pods");

        await vm.SendCommand.ExecuteAsync(null);

        agent.Options!.McpEndpoint.ShouldBeNull();
        await vm.DisposeAsync();
    }

    [Fact]
    public async Task send_uses_a_session_scoped_mcp_bearer_token_and_revokes_it_on_disposal()
    {
        await using var session = new TestSession("session-mcp-auth", []);
        var agent = new TestAgent(session);
        var settings = new Settings { McpServerEnabled = true, McpServerPort = 62888 };
        var settingsService = new Mock<ISettingsService>();
        settingsService.SetupGet(service => service.Settings).Returns(settings);
        var credentials = new McpCredentialService(settingsService.Object);
        var serverState = new McpServerState();
        serverState.SetBoundPort(62888);
        var vm = new AgentChatViewModel(
            new TestRegistry(agent),
            settingsService.Object,
            new AgentContextService(),
            serverState,
            credentials)
        {
            Prompt = "List pods"
        };

        await vm.SendCommand.ExecuteAsync(null);

        agent.Options!.McpEndpoint.ShouldBe("http://127.0.0.1:62888/mcp");
        agent.Options.McpHeaders.ShouldContainKey("Authorization");
        var authorization = agent.Options.McpHeaders["Authorization"];
        authorization.ShouldStartWith("Bearer kui_");
        var token = authorization["Bearer ".Length..];
        credentials.Authenticate(token).ShouldNotBeNull();
        agent.Options.ToString().ShouldNotContain(token);

        await vm.DisposeAsync();

        credentials.Authenticate(token).ShouldBeNull();
    }

    private static AgentChatViewModel CreateViewModel(
        IAgentRegistry registry,
        ISettingsService? settingsService = null,
        IAgentContextService? contextService = null,
        IMcpServerState? mcpServerState = null,
        string? prompt = null,
        AgentContext? context = null,
        ILogger<AgentChatViewModel>? logger = null)
    {
        settingsService ??= CreateSettingsService(new Settings());
        var viewModel = new AgentChatViewModel(
            registry,
            settingsService,
            contextService ?? new AgentContextService(),
            mcpServerState ?? new McpServerState(),
            logger: logger);
        viewModel.Prompt = prompt ?? string.Empty;
        viewModel.Context = context;
        return viewModel;
    }

    private static ISettingsService CreateSettingsService(Settings settings)
    {
        var settingsService = new Mock<ISettingsService>();
        settingsService.SetupGet(service => service.Settings).Returns(settings);
        return settingsService.Object;
    }

    private sealed class TestRegistry(params IAgent[] agents) : IAgentRegistry
    {
        public IReadOnlyList<IAgent> Agents => agents;
    }

    private sealed class TestAgent(
        TestSession session,
        string id = "test",
        string name = "Test",
        Exception? sessionCreationException = null) : IAgent
    {
        public int CreateSessionCount { get; private set; }
        public AgentSessionOptions? Options { get; private set; }
        public string Id => id;
        public string Name => name;
        public Task<IAgentSession> CreateSessionAsync(AgentSessionOptions options, CancellationToken cancellationToken = default)
        {
            CreateSessionCount++;
            Options = options;
            if (sessionCreationException is not null)
                return Task.FromException<IAgentSession>(sessionCreationException);
            return Task.FromResult<IAgentSession>(session);
        }

    }

    private sealed class BlockingTestAgent(BlockingTestSession session) : IAgent
    {
        public string Id => "blocking";
        public string Name => "Blocking";
        public Task<IAgentSession> CreateSessionAsync(AgentSessionOptions options, CancellationToken cancellationToken = default)
            => Task.FromResult<IAgentSession>(session);
    }

    private sealed class FailingAgent(string message) : IAgent
    {
        public string Id => "failing";
        public string Name => "Failing";

        public Task<IAgentSession> CreateSessionAsync(AgentSessionOptions options, CancellationToken cancellationToken = default)
            => Task.FromException<IAgentSession>(new InvalidOperationException(message));
    }

    private sealed class BlockingTestSession : IAgentSession
    {
        private readonly TaskCompletionSource _canceled = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource Started { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public bool CancelCalled { get; private set; }
        public string Id => "blocking-session";
        public IAsyncEnumerable<SessionEvent> Events => EmptyEvents();

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

        private static async IAsyncEnumerable<SessionEvent> EmptyEvents()
        {
            await Task.CompletedTask;
            yield break;
        }
    }

    private sealed class TestSession(string id, IReadOnlyList<SessionEvent> events, bool keepOpenAfterCompletion = false) : IAgentSession
    {
        private readonly TaskCompletionSource _neverCompletes = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public string Id => id;
        public string? Prompt { get; private set; }
        public bool WasDisposed { get; private set; }
        public IAsyncEnumerable<SessionEvent> Events => ReadEventsAsync();

        private async IAsyncEnumerable<SessionEvent> ReadEventsAsync()
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

        public Task<IAgentSession> CreateSessionAsync(AgentSessionOptions options, CancellationToken cancellationToken = default)
            => Task.FromResult<IAgentSession>(session);
    }

    private sealed class StreamingTestSession : IAgentSession
    {
        private readonly Channel<SessionEvent> _events = Channel.CreateUnbounded<SessionEvent>();
        private readonly TaskCompletionSource _completion = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public TaskCompletionSource FirstChunkSent { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public string Id => "streaming-session";
        public IAsyncEnumerable<SessionEvent> Events => _events.Reader.ReadAllAsync();

        public async Task PromptAsync(string prompt, CancellationToken cancellationToken = default)
        {
            await _events.Writer.WriteAsync(
                AssistantChunk("The response has started."),
                cancellationToken);
            FirstChunkSent.TrySetResult();
            await _completion.Task.WaitAsync(cancellationToken);
            await _events.Writer.WriteAsync(TurnCompleted(), cancellationToken);
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

    private static SessionEvent AssistantChunk(string text)
        => new SessionUpdateEvent(new SessionUpdate(new SessionUpdateVariant1AgentMessageChunk
        {
            Content = new ContentBlock(new TextContent { Text = text })
        }));

    private static SessionEvent TurnCompleted()
        => new Completed(new PromptResponse { StopReason = "end_turn" });
}
