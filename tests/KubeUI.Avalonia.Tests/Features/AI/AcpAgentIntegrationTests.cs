using System.Collections.Concurrent;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Text;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using dotacp.protocol;
using KubeUI.AI.Acp;
using KubeUI.AI.Agents;
using KubeUI.AI.Configuration;
using KubeUI.AI.Diagnostics;
using StreamJsonRpc;
using Shouldly;
using DomainAgentCapabilities = KubeUI.AI.Agents.AgentCapabilities;

namespace KubeUI.Avalonia.Tests.Features.AI;

public sealed class AcpAgentIntegrationTests
{
    [Fact]
    public void acp_error_formatter_preserves_code_and_structured_data()
    {
        var exception = new AcpException(
            -32602,
            "Invalid params",
            new { type = new { errors = new[] { "Expected http" } } });

        AcpErrorFormatter.Format(exception).ShouldBe(
            "ACP error -32602: Invalid params. Details: {\"type\":{\"errors\":[\"Expected http\"]}}");

        var remoteException = new StreamJsonRpc.RemoteInvocationException(
            "Invalid params",
            -32602,
            new { headers = new { errors = new[] { "Expected object" } } });

        AcpErrorFormatter.Format(remoteException).ShouldBe(
            "ACP error -32602: Invalid params. Details: {\"headers\":{\"errors\":[\"Expected object\"]}}");
    }

    [Fact]
    public async Task acp_session_creation_exposes_structured_error_details()
    {
        await using var process = new InMemoryAcpProcess(
            sessionCreationException: new AcpException(-32602, "Invalid params", new { type = "http" }));
        var agent = new AcpAgent(
            new AcpAgentDefinition { Id = "copilot", Name = "GitHub Copilot", Executable = "copilot" },
            () => process);
        var exception = await Should.ThrowAsync<InvalidOperationException>(
            () => agent.CreateSessionAsync(new AgentSessionOptions()));

        exception.Message.ShouldContain("ACP error -32000: Invalid params.");
        exception.Message.ShouldContain("AcpException");
    }

    [Fact]
    public void copilot_mcp_server_uses_http_configuration_with_empty_headers()
    {
        var json = JObject.Parse(JsonConvert.SerializeObject(
            AcpMcpServerFactory.Create(
                new AcpAgent(new AcpAgentDefinition { Id = "copilot", Name = "GitHub Copilot", Executable = "copilot" }),
                "http://127.0.0.1:62888/mcp")));

        json["type"]!.Value<string>().ShouldBe("http");
        json["url"]!.Value<string>().ShouldBe("http://127.0.0.1:62888/mcp");
        json["headers"]!.ShouldBeOfType<JArray>().ShouldBeEmpty();
    }

    [Fact]
    public async Task acp_agent_completes_initialize_session_prompt_stream_cancel_and_shutdown()
    {
        await using var process = new InMemoryAcpProcess();
        var agent = new AcpAgent(
            new AcpAgentDefinition { Id = "fake", Name = "Fake ACP", Executable = "unused" },
            () => process,
            new AllowPermissionService());
        await using var session = await agent.CreateSessionAsync(new AgentSessionOptions());
        agent.Capabilities.ShouldBe(
            DomainAgentCapabilities.FileSystem
            | DomainAgentCapabilities.Terminal
            | DomainAgentCapabilities.Permissions
            | DomainAgentCapabilities.Mcp
            | DomainAgentCapabilities.Plans
            | DomainAgentCapabilities.Usage);
        await session.PromptAsync("hello");

        var events = new List<AgentEvent>();
        await foreach (var item in session.Events)
        {
            events.Add(item);
            if (item is AgentTurnCompletedEvent)
                break;
        }

        agent.Id.ShouldBe("fake");
        events.OfType<AgentMessageEvent>().Single().Message.Text.ShouldBe("fake response");
        await session.CancelAsync();
        await process.CancelReceived.WaitAsync(TimeSpan.FromSeconds(5));
        process.WasDisposed.ShouldBeFalse();
        await session.DisposeAsync();
        process.WasDisposed.ShouldBeTrue();
        process.EmitDiagnostic("after-dispose");
    }

    [Fact]
    public async Task acp_disconnect_completes_session_events_with_an_error()
    {
        await using var process = new InMemoryAcpProcess();
        var agent = new AcpAgent(
            new AcpAgentDefinition { Id = "fake", Name = "Fake ACP", Executable = "unused" },
            () => process,
            new AllowPermissionService());

        await using var session = await agent.CreateSessionAsync(new AgentSessionOptions());
        process.Disconnect();

        await Should.ThrowAsync<IOException>(async () =>
        {
            await foreach (var _ in session.Events)
            {
            }
        });
    }

    [Fact]
    public async Task acp_agent_can_restart_after_disposing_a_session()
    {
#pragma warning disable CA2000 // Each process is owned and disposed by the ACP session created below.
        var processes = new Queue<InMemoryAcpProcess>([new(), new()]);
#pragma warning restore CA2000
        var agent = new AcpAgent(
            new AcpAgentDefinition { Id = "fake", Name = "Fake ACP", Executable = "unused" },
            () => processes.Dequeue(),
            new AllowPermissionService());

        await using (var first = await agent.CreateSessionAsync(new AgentSessionOptions()))
        {
            first.Id.ShouldBe("fake-session");
        }

        await using var second = await agent.CreateSessionAsync(new AgentSessionOptions());
        second.Id.ShouldBe("fake-session");
        processes.ShouldBeEmpty();
    }

    [Fact]
    public async Task acp_agent_rejects_malformed_protocol_messages()
    {
        var agent = new AcpAgent(
            new AcpAgentDefinition { Id = "malformed", Name = "Malformed ACP", Executable = "unused" },
            () => new MalformedAcpProcess());
        using var timeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));

        await Should.ThrowAsync<Exception>(() => agent.CreateSessionAsync(new AgentSessionOptions(), timeout.Token));
    }

    [Fact]
    public async Task acp_agent_authenticates_when_the_runtime_advertises_an_agent_auth_method()
    {
        await using var process = new InMemoryAcpProcess(requiresAuthentication: true);
        var agent = new AcpAgent(
            new AcpAgentDefinition { Id = "auth", Name = "Authenticated ACP", Executable = "unused" },
            () => process,
            new AllowPermissionService());

        await using var session = await agent.CreateSessionAsync(new AgentSessionOptions());

        await process.AuthenticationReceived.WaitAsync(TimeSpan.FromSeconds(5));
        session.Id.ShouldBe("fake-session");
    }

    [Fact]
    public async Task acp_agent_uses_the_configured_authentication_method()
    {
        await using var process = new InMemoryAcpProcess(
            requiresAuthentication: true,
            authenticationMethodIds: ["api-key", "chat-gpt"]);
        var agent = new AcpAgent(
            new AcpAgentDefinition
            {
                Id = "codex",
                Name = "Codex",
                Executable = "unused",
                AuthenticationMethodId = "chat-gpt"
            },
            () => process,
            new AllowPermissionService());

        await using var session = await agent.CreateSessionAsync(new AgentSessionOptions());

        await process.AuthenticationReceived.WaitAsync(TimeSpan.FromSeconds(5));
        process.AuthenticationMethodId.ShouldBe("chat-gpt");
        session.Id.ShouldBe("fake-session");
    }

    [Fact]
    public async Task acp_session_registers_only_the_kubeui_mcp_server()
    {
        await using var process = new InMemoryAcpProcess();
        var agent = new AcpAgent(
            new AcpAgentDefinition { Id = "fake", Name = "Fake ACP", Executable = "unused" },
            () => process,
            new AllowPermissionService());

        await using var session = await agent.CreateSessionAsync(new AgentSessionOptions
        {
            McpEndpoint = "http://127.0.0.1:62888/mcp"
        });

        var mcpServers = process.NewSessionRequest!.McpServers!.ToArray();
        mcpServers.Length.ShouldBe(1);
        mcpServers[0].ShouldBeOfType<McpServerHttp>();
        var server = (McpServerHttp)mcpServers[0];
        server.Name.ShouldBe("kubeui");
        server.Url.ShouldBe("http://127.0.0.1:62888/mcp");
    }

    [Fact]
    public async Task acp_lifecycle_emits_expected_diagnostic_activities()
    {
        var names = new ConcurrentBag<string>();
        using var listener = new ActivityListener
        {
            ShouldListenTo = source => source.Name == AgentActivitySource.SourceName,
            Sample = static (ref ActivityCreationOptions<ActivityContext> _) => ActivitySamplingResult.AllData,
            ActivityStopped = activity => names.Add(activity.OperationName)
        };
        ActivitySource.AddActivityListener(listener);

        await using var process = new InMemoryAcpProcess();
        var agent = new AcpAgent(
            new AcpAgentDefinition { Id = "traced", Name = "Traced ACP", Executable = "unused" },
            () => process,
            new AllowPermissionService());
        await using var session = await agent.CreateSessionAsync(new AgentSessionOptions());
        await session.PromptAsync("hello");
        await session.CancelAsync();
        await session.DisposeAsync();

        names.ShouldContain("ai.agent.start");
        names.ShouldContain("ai.agent.initialize");
        names.ShouldContain("ai.session.create");
        names.ShouldContain("ai.session.prompt");
        names.ShouldContain("ai.session.cancel");
        names.ShouldContain("ai.agent.stop");
    }

    private sealed class AllowPermissionService : KubeUI.AI.Permissions.IAgentPermissionService
    {
        public Task<AgentPermissionResult> RequestPermissionAsync(
            AgentPermissionRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(new AgentPermissionResult(true));
    }

    private sealed class MalformedAcpProcess : IAcpProcess
    {
        private readonly MemoryStream _output = new(Encoding.UTF8.GetBytes("not valid json\n"));

        public event Action<string>? ErrorReceived { add { } remove { } }
        public event Action? Exited { add { } remove { } }
        public Stream Input => Stream.Null;
        public Stream Output => _output;

        public Task StartAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            return Task.CompletedTask;
        }

        public Task StopAsync(CancellationToken cancellationToken) => Task.CompletedTask;

        public ValueTask DisposeAsync()
        {
            _output.Dispose();
            return ValueTask.CompletedTask;
        }
    }

    private sealed class InMemoryAcpProcess(
        bool requiresAuthentication = false,
        IReadOnlyList<string>? authenticationMethodIds = null,
        AcpException? sessionCreationException = null) : IAcpProcess
    {
        private readonly Pipe _clientToServer = new();
        private readonly Pipe _serverToClient = new();
        private JsonRpc? _server;
        private FakeAcpServer? _fakeServer;
        private Stream? _clientInput;
        private Stream? _clientOutput;

        public bool WasDisposed { get; private set; }
        public Task CancelReceived => _fakeServer?.CancelReceived.Task ?? Task.CompletedTask;
        public Task AuthenticationReceived => _fakeServer?.AuthenticationReceived.Task ?? Task.CompletedTask;
        public string? AuthenticationMethodId => _fakeServer?.AuthenticationMethodId;
        public NewSessionRequest? NewSessionRequest => _fakeServer?.NewSessionRequest;
        public event Action<string>? ErrorReceived;
        public event Action? Exited;
        public Stream Input => _clientInput ?? throw new InvalidOperationException("Process has not started.");
        public Stream Output => _clientOutput ?? throw new InvalidOperationException("Process has not started.");

        public Task StartAsync(CancellationToken cancellationToken)
        {
            cancellationToken.ThrowIfCancellationRequested();
            _clientInput = _clientToServer.Writer.AsStream(leaveOpen: true);
            _clientOutput = _serverToClient.Reader.AsStream(leaveOpen: true);
#pragma warning disable CA2000 // The JsonRpc server owns the handler and formatter for the in-memory process lifetime.
            var handler = new NewLineDelimitedMessageHandler(
                _serverToClient.Writer.AsStream(leaveOpen: true),
                _clientToServer.Reader.AsStream(leaveOpen: true),
                new JsonMessageFormatter());
            _server = new JsonRpc(handler);
#pragma warning restore CA2000
            _fakeServer = new FakeAcpServer(_server, requiresAuthentication, authenticationMethodIds, sessionCreationException);
            _server.AddLocalRpcTarget(_fakeServer);
            _server.StartListening();
            return Task.CompletedTask;
        }

        public void Disconnect()
        {
            _server?.Dispose();
            Exited?.Invoke();
        }

        public void EmitDiagnostic(string message) => ErrorReceived?.Invoke(message);

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _server?.Dispose();
            await _clientToServer.Writer.CompleteAsync().ConfigureAwait(false);
            await _clientToServer.Reader.CompleteAsync().ConfigureAwait(false);
            await _serverToClient.Writer.CompleteAsync().ConfigureAwait(false);
            await _serverToClient.Reader.CompleteAsync().ConfigureAwait(false);
        }

        public async ValueTask DisposeAsync()
        {
            if (WasDisposed)
                return;
            WasDisposed = true;
            try { await StopAsync(CancellationToken.None).ConfigureAwait(false); }
            catch (Exception exception) { ErrorReceived?.Invoke(exception.Message); }
            _clientInput?.Dispose();
            _clientOutput?.Dispose();
        }
    }

    private sealed class FakeAcpServer(
        JsonRpc rpc,
        bool requiresAuthentication,
        IReadOnlyList<string>? authenticationMethodIds,
        AcpException? sessionCreationException)
    {
        public TaskCompletionSource CancelReceived { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public TaskCompletionSource AuthenticationReceived { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public string? AuthenticationMethodId { get; private set; }
        public NewSessionRequest? NewSessionRequest { get; private set; }

        [JsonRpcMethod("initialize", UseSingleObjectParameterDeserialization = true)]
        public Task<InitializeResponse> InitializeAsync(InitializeRequest _)
            => Task.FromResult(new InitializeResponse
            {
                ProtocolVersion = 1,
                AuthMethods = requiresAuthentication
                    ? [.. (authenticationMethodIds ?? ["test-auth"]).Select(id => new AuthMethodAgent
                    {
                        Id = new AuthMethodId(id),
                        Name = id
                    })]
                    : [],
                AgentCapabilities = new dotacp.protocol.AgentCapabilities
                {
                    McpCapabilities = new McpCapabilities()
                }
            });

        [JsonRpcMethod("authenticate", UseSingleObjectParameterDeserialization = true)]
        public Task<AuthenticateResponse> AuthenticateAsync(AuthenticateRequest request)
        {
            AuthenticationMethodId = request.MethodId.ToString();
            AuthenticationReceived.TrySetResult();
            return Task.FromResult(new AuthenticateResponse());
        }

        [JsonRpcMethod("session/new", UseSingleObjectParameterDeserialization = true)]
        public Task<NewSessionResponse> NewSessionAsync(NewSessionRequest request)
        {
            if (sessionCreationException is not null)
                throw sessionCreationException;
            NewSessionRequest = request;
            return Task.FromResult(new NewSessionResponse { SessionId = new SessionId("fake-session") });
        }

        [JsonRpcMethod("session/prompt", UseSingleObjectParameterDeserialization = true)]
        public async Task<PromptResponse> PromptAsync(PromptRequest request)
        {
            await rpc.NotifyWithParameterObjectAsync("session/update", new SessionNotification
            {
                SessionId = request.SessionId,
                Update = new SessionUpdateAgentMessageChunk
                {
                    Content = new TextContent { Text = "fake response" }
                }
            }).ConfigureAwait(false);
            return new PromptResponse { StopReason = StopReason.EndTurn };
        }

        [JsonRpcMethod("session/cancel", UseSingleObjectParameterDeserialization = true)]
        public Task CancelAsync(CancelNotification _)
        {
            CancelReceived.TrySetResult();
            return Task.CompletedTask;
        }
    }
}
