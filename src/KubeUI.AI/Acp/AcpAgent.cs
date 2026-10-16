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

    public async Task<IAgentSession> CreateSessionAsync(AgentSessionOptions options, CancellationToken cancellationToken = default)
    {
        using var activity = AgentActivitySource.Source.StartActivity("ai.agent.start");
        activity?.SetTag("agent.id", Id);
        var process = _processFactory?.Invoke() ?? CreateProcess(options);
        Connection? connection = null;
        DotAcpClient? client = null;
        var sensitiveValues = GetSensitiveValues(options.McpHeaders);
        var events = Channel.CreateUnbounded<AgentEvent>();
        try
        {
            await process.StartAsync(cancellationToken).ConfigureAwait(false);
            client = new DotAcpClient(events.Writer, _permissionService, options.TrustedMcpServers, options.FileSystemRoots);
            connection = Connection.RunClient(client, process.Input, process.Output)
                ?? throw new InvalidOperationException("Unable to create ACP connection.");
            ProtocolInitializeResponse initialize;
            using (var initializeActivity = AgentActivitySource.Source.StartActivity("ai.agent.initialize"))
            {
                initializeActivity?.SetTag("agent.id", Id);
                initializeActivity?.SetTag("agent.protocol", "acp");
                initialize = await connection.InitializeAsync(new InitializeRequest
                {
                    ProtocolVersion = ProtocolMeta.Version,
                    ClientInfo = new Implementation { Name = "KubeUI", Version = "1.0" },
                    ClientCapabilities = new ClientCapabilities
                    {
                        Fs = new FileSystemCapabilities { ReadTextFile = true, WriteTextFile = true },
                        Terminal = true
                    }
                }, cancellationToken).ConfigureAwait(false);
            }
            Capabilities = MapCapabilities(initialize);
            var useHttpMcp = !string.IsNullOrWhiteSpace(options.McpEndpoint)
                && initialize.AgentCapabilities?.McpCapabilities?.Http == true;
            if (!useHttpMcp)
                options.McpCredentialLease?.Dispose();
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
                    McpServers = !useHttpMcp
                        ? []
                        : [new McpServerHttp
                        {
                            Name = "kubeui",
                            Url = options.McpEndpoint,
                            Headers = [.. options.McpHeaders.Select(header => new HttpHeader { Name = header.Key, Value = header.Value })]
                        }]
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
                client,
                useHttpMcp ? options.McpCredentialLease : null,
                sensitiveValues);
        }
        catch (Exception exception)
        {
            options.McpCredentialLease?.Dispose();
            RecordException(activity, exception, sensitiveValues);
            var formattedException = AcpErrorFormatter.IsAcpError(exception)
                ? AcpErrorFormatter.Format(exception)
                : null;
            if (formattedException is not null)
                formattedException = RedactSensitiveValues(formattedException, sensitiveValues);
            if (connection is not null)
            {
                var completion = connection.Completion;
                connection.Dispose();
                try
                {
                    await completion.ConfigureAwait(false);
                }
                catch (Exception completionException)
                {
                    RecordException(activity, completionException, sensitiveValues);
                    // Preserve original ACP failure while observing connection shutdown failure.
                }
            }
            await process.DisposeAsync().ConfigureAwait(false);
            client?.Dispose();
            if (formattedException is not null)
                throw new InvalidOperationException(formattedException, sensitiveValues.Length == 0 ? exception : null);
            if (sensitiveValues.Length > 0)
            {
                var safeDiagnostic = RedactSensitiveValues(exception.ToString(), sensitiveValues);
                throw new InvalidOperationException(
                    "ACP session creation failed; sensitive MCP connection data was omitted.",
                    new InvalidOperationException(safeDiagnostic));
            }
            throw;
        }
    }

    private IAcpProcess CreateProcess(AgentSessionOptions options) => new AcpProcess(_definition, options);

    private static void RecordException(Activity? activity, Exception exception, IReadOnlyList<string> sensitiveValues)
    {
        if (activity is null)
            return;

        var message = RedactSensitiveValues(exception.Message, sensitiveValues);
        activity.SetStatus(ActivityStatusCode.Error, message);
        var tags = new ActivityTagsCollection
        {
            ["exception.type"] = exception.GetType().FullName,
            ["exception.message"] = message
        };
        activity.AddEvent(new ActivityEvent("exception", tags: tags));
    }

    private static string RedactSensitiveValues(string value, IReadOnlyList<string> sensitiveValues)
    {
        foreach (var sensitiveValue in sensitiveValues)
            value = value.Replace(sensitiveValue, "[redacted]", StringComparison.Ordinal);
        return value;
    }

    private static string[] GetSensitiveValues(IReadOnlyDictionary<string, string> headers)
    {
        var values = new HashSet<string>(StringComparer.Ordinal);
        foreach (var header in headers.Values)
        {
            if (string.IsNullOrEmpty(header))
                continue;
            values.Add(header);
            const string bearerPrefix = "Bearer ";
            if (header.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase)
                && header.Length > bearerPrefix.Length)
                values.Add(header[bearerPrefix.Length..]);
        }
        return [.. values];
    }

    private static DomainAgentCapabilities MapCapabilities(ProtocolInitializeResponse result)
    {
        // These capabilities are provided by the KubeUI ACP client callbacks;
        // MCP is additionally gated by the agent's negotiated advertisement.
        var capabilities = DomainAgentCapabilities.FileSystem
            | DomainAgentCapabilities.Terminal
            | DomainAgentCapabilities.Permissions
            | DomainAgentCapabilities.Plans
            | DomainAgentCapabilities.Usage;
        if (result.AgentCapabilities?.McpCapabilities?.Http == true)
            capabilities |= DomainAgentCapabilities.Mcp;
        return capabilities;
    }

}
