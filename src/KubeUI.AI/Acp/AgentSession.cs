using System.Diagnostics;
using System.Threading.Channels;
using KubeUI.AI.Agents;
using KubeUI.AI.Diagnostics;
using SvcSystems.AI.AgentClientProtocol;
using SvcSystems.AI.AgentClientProtocol.Models;
using AcpClient = SvcSystems.AI.AgentClientProtocol.Client.Client;

namespace KubeUI.AI.Acp;

internal sealed class AgentSession : IAgentSession
{
    private readonly Session _session;
    private readonly AcpClient _client;
    private readonly TerminalHandler _terminal;
    private readonly AgentContext? _context;
    private readonly Channel<SessionEvent> _events;
    private readonly IDisposable? _mcpCredentialLease;
    private readonly IReadOnlyList<string> _sensitiveValues;
    private readonly AcpToolCallTracker _toolCallTracker = new();
    private bool _disposed;

    public AgentSession(
        Session session, AcpClient client, TerminalHandler terminal, Channel<SessionEvent> events,
        AgentContext? context = null, IDisposable? mcpCredentialLease = null,
        IReadOnlyList<string>? sensitiveValues = null)
    {
        _session = session;
        _client = client;
        _terminal = terminal;
        _events = events;
        _context = context;
        _mcpCredentialLease = mcpCredentialLease;
        _sensitiveValues = sensitiveValues ?? [];
    }

    public string Id => _session.Id;
    public IAsyncEnumerable<SessionEvent> Events => _events.Reader.ReadAllAsync();

    public async Task PromptAsync(string prompt, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        using var activity = AgentActivitySource.Source.StartActivity("ai.session.prompt");
        activity?.SetTag("agent.protocol", "acp");
        activity?.SetTag("agent.session.id", Id);
        var context = _context?.ToPromptContext();
        var text = string.IsNullOrWhiteSpace(context)
            ? prompt
            : $"{prompt}{Environment.NewLine}{Environment.NewLine}KubeUI context:{Environment.NewLine}{context}";
        try
        {
            await foreach (var update in _session.PromptAsync(text, cancellationToken).ConfigureAwait(false))
            {
                if (!_toolCallTracker.ShouldForward(update))
                    continue;
                _events.Writer.TryWrite(update);
            }
        }
        catch (Exception exception)
        {
            var safeException = SanitizeException(exception);
            _events.Writer.TryComplete(safeException);
            throw safeException;
        }
    }

    public async Task CancelAsync(CancellationToken cancellationToken = default)
    {
        using var activity = AgentActivitySource.Source.StartActivity("ai.session.cancel");
        activity?.SetTag("agent.protocol", "acp");
        activity?.SetTag("agent.session.id", Id);
        try
        { await _session.CancelAsync(cancellationToken).ConfigureAwait(false); }
        catch (Exception exception) { throw SanitizeException(exception); }
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;
        _disposed = true;
        using var activity = AgentActivitySource.Source.StartActivity("ai.agent.stop");
        activity?.SetTag("agent.protocol", "acp");

        var exceptions = new List<Exception>();

        try
        {
            await _client.DisposeAsync().ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            var safeException = SanitizeException(exception);
            exceptions.Add(safeException);
            if (activity is not null)
            {
                activity.SetStatus(ActivityStatusCode.Error, safeException.Message);
                activity.AddEvent(new ActivityEvent("exception", tags: new ActivityTagsCollection
                {
                    ["exception.type"] = safeException.GetType().FullName,
                    ["exception.message"] = safeException.Message
                }));
            }
        }

        try
        {
            await _terminal.DisposeAsync().ConfigureAwait(false);
        }
        catch (Exception exception)
        {
            exceptions.Add(exception);
        }

        try
        {
            _mcpCredentialLease?.Dispose();
        }
        catch (Exception exception)
        {
            exceptions.Add(exception);
        }

        _events.Writer.TryComplete();

        if (exceptions.Count > 0)
        {
            throw new AggregateException("One or more errors occurred during disposal", exceptions);
        }
    }

    private Exception SanitizeException(Exception exception)
        => _sensitiveValues.Count == 0 ? exception : new InvalidOperationException(RedactSensitiveValues(exception.Message));

    private string RedactSensitiveValues(string value)
    {
        foreach (var sensitiveValue in _sensitiveValues)
            value = value.Replace(sensitiveValue, "[redacted]", StringComparison.Ordinal);
        return value;
    }
}
