using System.Diagnostics;
using System.Threading.Channels;
using dotacp.client;
using dotacp.protocol;
using KubeUI.AI.Agents;
using KubeUI.AI.Configuration;
using KubeUI.AI.Diagnostics;
using KubeUI.AI.Permissions;
using DomainAgentCapabilities = KubeUI.AI.Agents.AgentCapabilities;
using ProtocolAuthMethodAgent = dotacp.protocol.AuthMethodAgent;
using ProtocolInitializeResponse = dotacp.protocol.InitializeResponse;
using ProtocolNewSessionResponse = dotacp.protocol.NewSessionResponse;

namespace KubeUI.AI.Acp;

public sealed class AcpAgent : IAgent
{
    private readonly AcpAgentDefinition _definition;
    private readonly Func<IAcpProcess>? _processFactory;
    private readonly IAgentPermissionService _permissionService;

    public AcpAgent(AcpAgentDefinition definition, IAgentPermissionService? permissionService = null)
    {
        _definition = definition;
        _permissionService = permissionService ?? new DenyByDefaultAgentPermissionService();
    }

    internal AcpAgent(AcpAgentDefinition definition, Func<IAcpProcess> processFactory, IAgentPermissionService? permissionService = null)
    {
        _definition = definition;
        _processFactory = processFactory;
        _permissionService = permissionService ?? new DenyByDefaultAgentPermissionService();
    }

    public string Id => _definition.Id;
    public string Name => _definition.Name;
    public DomainAgentCapabilities Capabilities { get; private set; }

    public event Action<string>? DiagnosticReceived;

    public async Task<IAgentSession> CreateSessionAsync(AgentSessionOptions options, CancellationToken cancellationToken = default)
    {
        using var activity = AgentActivitySource.Source.StartActivity("ai.agent.start");
        activity?.SetTag("agent.id", Id);
        var process = _processFactory?.Invoke() ?? CreateProcess(options);
        process.ErrorReceived += OnProcessError;
        Connection? connection = null;
        var events = Channel.CreateUnbounded<AgentEvent>();
        try
        {
            await process.StartAsync(cancellationToken).ConfigureAwait(false);
                connection = Connection.RunClient(new DotAcpClient(events.Writer, _permissionService, options.TrustedMcpServers), process.Input, process.Output)
                ?? throw new InvalidOperationException("Unable to create ACP connection.");
            ProtocolInitializeResponse initialize;
            using (var initializeActivity = AgentActivitySource.Source.StartActivity("ai.agent.initialize"))
            {
                initializeActivity?.SetTag("agent.id", Id);
                initializeActivity?.SetTag("agent.protocol", "acp");
                initialize = await connection.InitializeAsync(new InitializeRequest
                {
                    ProtocolVersion = 1,
                    ClientInfo = new Implementation { Name = "KubeUI", Version = "1.0" },
                    ClientCapabilities = new ClientCapabilities
                    {
                        Fs = new FileSystemCapabilities { ReadTextFile = true, WriteTextFile = true },
                        Terminal = true
                    }
                }, cancellationToken).ConfigureAwait(false);
            }
            Capabilities = MapCapabilities(initialize);
            var authenticationMethods = initialize.AuthMethods?.OfType<ProtocolAuthMethodAgent>().ToArray() ?? [];
            var agentAuthentication = authenticationMethods.FirstOrDefault(method =>
                    string.Equals(method.Id.ToString(), _definition.AuthenticationMethodId, StringComparison.Ordinal))
                ?? authenticationMethods.FirstOrDefault();
            if (agentAuthentication is not null)
            {
                using var authenticateActivity = AgentActivitySource.Source.StartActivity("ai.agent.authenticate");
                authenticateActivity?.SetTag("agent.id", Id);
                authenticateActivity?.SetTag("agent.protocol", "acp");
                await connection.AuthenticateAsync(new AuthenticateRequest { MethodId = agentAuthentication.Id }, cancellationToken).ConfigureAwait(false);
            }
            ProtocolNewSessionResponse session;
            using (var sessionActivity = AgentActivitySource.Source.StartActivity("ai.session.create"))
            {
                sessionActivity?.SetTag("agent.id", Id);
                sessionActivity?.SetTag("agent.protocol", "acp");
                session = await connection.NewSessionAsync(new NewSessionRequest
                {
                    Cwd = options.WorkingDirectory ?? Environment.CurrentDirectory,
                    McpServers = string.IsNullOrWhiteSpace(options.McpEndpoint)
                        ? []
                        : [new McpServerHttp { Name = "kubeui", Url = options.McpEndpoint }]
                }, cancellationToken).ConfigureAwait(false);
                sessionActivity?.SetTag("agent.session.id", session.SessionId.ToString());
            }
            return new AcpAgentSession(
                session.SessionId.ToString(),
                session.SessionId,
                connection,
                events,
                options.Context,
                process,
                () => process.ErrorReceived -= OnProcessError);
        }
        catch
        {
            connection?.Dispose();
            await process.DisposeAsync().ConfigureAwait(false);
            process.ErrorReceived -= OnProcessError;
            throw;
        }
    }

    private void OnProcessError(string message) => DiagnosticReceived?.Invoke(message);

    private IAcpProcess CreateProcess(AgentSessionOptions options) => new AcpProcess(_definition, options);

    private static DomainAgentCapabilities MapCapabilities(dotacp.protocol.InitializeResponse result)
    {
        // These capabilities are provided by the KubeUI ACP client callbacks;
        // MCP is additionally gated by the agent's negotiated advertisement.
        var capabilities = DomainAgentCapabilities.FileSystem
            | DomainAgentCapabilities.Terminal
            | DomainAgentCapabilities.Permissions
            | DomainAgentCapabilities.Plans
            | DomainAgentCapabilities.Usage;
        if (result.AgentCapabilities?.McpCapabilities != null) capabilities |= DomainAgentCapabilities.Mcp;
        return capabilities;
    }

}
