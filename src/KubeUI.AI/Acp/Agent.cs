using System.Diagnostics;
using System.Threading.Channels;
using KubeUI.AI.Agents;
using KubeUI.AI.Configuration;
using KubeUI.AI.Diagnostics;
using KubeUI.AI.Permissions;
using SvcSystems.AI.AgentClientProtocol;
using SvcSystems.AI.AgentClientProtocol.Client.Models;
using SvcSystems.AI.AgentClientProtocol.Models;
using AcpClient = SvcSystems.AI.AgentClientProtocol.Client.Client;

namespace KubeUI.AI.Acp;

public sealed class Agent : IAgent
{
    private readonly AcpAgentDefinition _definition;
    private readonly IAgentPermissionService _permissionService;

    public Agent(AcpAgentDefinition definition, IAgentPermissionService? permissionService = null)
    {
        _definition = definition;
        _permissionService = permissionService ?? new DenyByDefaultAgentPermissionService();
    }

    public string Id => _definition.Id;
    public string Name => _definition.Name;

    public async Task<IAgentSession> CreateSessionAsync(AgentSessionOptions options, CancellationToken cancellationToken = default)
    {
        using var activity = AgentActivitySource.Source.StartActivity("ai.agent.start");
        activity?.SetTag("agent.id", Id);
        var sensitiveValues = GetSensitiveValues(options.McpHeaders);
        var events = Channel.CreateUnbounded<SessionEvent>();
        var fileSystem = new FileSystemHandler(_permissionService, options.FileSystemRoots);
        var terminal = new TerminalHandler(_permissionService);
        var permission = new PermissionHandler(_permissionService, options.TrustedMcpServers);
        var clientOptions = new ClientOptions
        {
            ClientInfo = new Implementation { Name = "KubeUI", Version = "1.0" },
            FileSystemHandler = fileSystem,
            TerminalHandler = terminal,
            PermissionHandler = permission
        };
        var environment = new Dictionary<string, string?>(_definition.Environment, StringComparer.Ordinal);
        foreach (var name in _definition.EnvironmentVariableNames)
        {
            var value = Environment.GetEnvironmentVariable(name);
            if (value is not null)
                environment[name] = value;
        }
        foreach (var item in options.Environment)
            environment[item.Key] = item.Value;

        AcpClient? client = null;
        var mcpCredentialLease = options.McpCredentialLease;
        var leaseTransferred = false;
        try
        {
            client = await AcpClient.StartAsync(new ProcessOptions
            {
                FileName = ExecutableLocator.Find(_definition.Executable) ?? _definition.Executable,
                Arguments = _definition.Arguments,
                WorkingDirectory = options.WorkingDirectory ?? Environment.CurrentDirectory,
                Environment = environment,
                StandardErrorReceived = message => Trace.TraceWarning("ACP agent {0}: {1}", Id, RedactSensitiveValues(message, sensitiveValues))
            }, clientOptions, cancellationToken).ConfigureAwait(false);

            var agentCapabilities = client.Agent.AgentCapabilities;
            var useHttpMcp = !string.IsNullOrWhiteSpace(options.McpEndpoint)
                && agentCapabilities?.McpCapabilities?.Http == true;
            if (!useHttpMcp)
            {
                mcpCredentialLease?.Dispose();
                mcpCredentialLease = null;
            }

            var authMethods = client.Agent.AuthMethods?
                .Select(static method => method.Value)
                .OfType<AuthMethodAgent>()
                .ToArray();
            var auth = authMethods?.FirstOrDefault(method =>
                    string.Equals(method.Id, _definition.AuthenticationMethodId, StringComparison.Ordinal))
                ?? authMethods?.FirstOrDefault();
            if (auth is not null)
            {
                using var authenticateActivity = AgentActivitySource.Source.StartActivity("ai.agent.authenticate");
                authenticateActivity?.SetTag("agent.id", Id);
                authenticateActivity?.SetTag("agent.protocol", "acp");
                await client.AuthenticateAsync(auth.Id, cancellationToken).ConfigureAwait(false);
            }

            using var sessionActivity = AgentActivitySource.Source.StartActivity("ai.session.create");
            sessionActivity?.SetTag("agent.id", Id);
            sessionActivity?.SetTag("agent.protocol", "acp");
            var session = await client.CreateSessionAsync(new SessionOptions
            {
                WorkingDirectory = options.WorkingDirectory ?? Environment.CurrentDirectory,
                McpServers = useHttpMcp
                    ? [new McpServerHttp
                    {
                        Name = "kubeui",
                        Url = options.McpEndpoint!,
                        Headers = options.McpHeaders.Select(static header => new HttpHeader
                        {
                            Name = header.Key,
                            Value = header.Value
                        }).ToList()
                    }]
                    : []
            }, cancellationToken).ConfigureAwait(false);
            sessionActivity?.SetTag("agent.session.id", session.Id);
            leaseTransferred = true;
            return new AgentSession(session, client, terminal, events, options.Context,
                useHttpMcp ? mcpCredentialLease : null, sensitiveValues);
        }
        catch (Exception exception)
        {
            RecordException(activity, exception, sensitiveValues);
            var exceptions = new List<Exception>();

            if (client is not null)
            {
                try
                {
                    await client.DisposeAsync().ConfigureAwait(false);
                }
                catch (Exception cleanupException)
                {
                    exceptions.Add(SanitizeException(cleanupException, sensitiveValues));
                }
            }

            try
            {
                terminal.Dispose();
            }
            catch (Exception cleanupException)
            {
                exceptions.Add(SanitizeException(cleanupException, sensitiveValues));
            }

            try
            {
                if (!leaseTransferred)
                    mcpCredentialLease?.Dispose();
            }
            catch (Exception cleanupException)
            {
                exceptions.Add(SanitizeException(cleanupException, sensitiveValues));
            }

            if (exceptions.Count > 0)
            {
                throw new AggregateException("One or more errors occurred during session creation cleanup", exceptions);
            }

            throw SanitizeException(exception, sensitiveValues);
        }
    }

    private static void RecordException(Activity? activity, Exception exception, IReadOnlyList<string> sensitiveValues)
    {
        if (activity is null)
            return;
        var message = RedactSensitiveValues(exception.Message, sensitiveValues);
        activity.SetStatus(ActivityStatusCode.Error, message);
        activity.AddEvent(new ActivityEvent("exception", tags: new ActivityTagsCollection
        {
            ["exception.type"] = exception.GetType().FullName,
            ["exception.message"] = message
        }));
    }

    private static Exception SanitizeException(Exception exception, IReadOnlyList<string> sensitiveValues)
        => sensitiveValues.Count == 0 ? exception : new InvalidOperationException(RedactSensitiveValues(exception.ToString(), sensitiveValues));

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
            if (header.StartsWith(bearerPrefix, StringComparison.OrdinalIgnoreCase) && header.Length > bearerPrefix.Length)
                values.Add(header[bearerPrefix.Length..]);
        }
        return [.. values];
    }
}
