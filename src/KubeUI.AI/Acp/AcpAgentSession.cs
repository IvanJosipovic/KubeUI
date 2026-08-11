using System.Threading.Channels;
using dotacp.protocol;
using dotacp.client;
using KubeUI.AI.Agents;
using KubeUI.AI.Diagnostics;

namespace KubeUI.AI.Acp;

internal sealed class AcpAgentSession : IAgentSession
{
    private readonly SessionId _protocolId;
    private readonly string _id;
    private readonly AgentContext? _context;
    private readonly Connection _connection;
    private readonly Channel<AgentEvent> _events;
    private readonly IAcpProcess? _diagnosticProcess;
    private readonly Action? _onDispose;
    private bool _disposed;

    public AcpAgentSession(
        string id,
        SessionId protocolId,
        Connection connection,
        Channel<AgentEvent> events,
        AgentContext? context = null,
        IAcpProcess? diagnosticProcess = null,
        Action? onDispose = null)
    {
        _id = id;
        _protocolId = protocolId;
        _context = context;
        _connection = connection;
        _events = events;
        _diagnosticProcess = diagnosticProcess;
        _onDispose = onDispose;
        if (_diagnosticProcess is not null)
            _diagnosticProcess.ErrorReceived += DiagnosticProcessOnError;
        if (_diagnosticProcess is not null)
            _diagnosticProcess.Exited += ProcessOnExited;
    }

    public string Id => _id;
    public IAsyncEnumerable<AgentEvent> Events => _events.Reader.ReadAllAsync();

    public async Task PromptAsync(string prompt, CancellationToken cancellationToken = default)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        using var activity = AgentActivitySource.Source.StartActivity("ai.session.prompt");
        activity?.SetTag("agent.protocol", "acp");
        activity?.SetTag("agent.session.id", Id);
        var text = string.IsNullOrWhiteSpace(_context?.ToPromptContext())
            ? prompt
            : $"{prompt}{Environment.NewLine}{Environment.NewLine}KubeUI context:{Environment.NewLine}{_context.ToPromptContext()}";
        try
        {
            await _connection.PromptAsync(new PromptRequest
            {
                SessionId = _protocolId,
                Prompt = [new TextContent { Text = text }]
            }, cancellationToken).ConfigureAwait(false);
            _events.Writer.TryWrite(new AgentTurnCompletedEvent());
        }
        catch (Exception exception)
        {
            _events.Writer.TryComplete(exception);
            throw;
        }
    }

    public async Task CancelAsync(CancellationToken cancellationToken = default)
    {
        using var activity = AgentActivitySource.Source.StartActivity("ai.session.cancel");
        activity?.SetTag("agent.protocol", "acp");
        activity?.SetTag("agent.session.id", Id);
        await _connection.CancelAsync(new CancelNotification { SessionId = _protocolId }, cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask DisposeAsync()
    {
        if (_disposed)
            return;
        _disposed = true;
        if (_diagnosticProcess is not null)
        {
            _diagnosticProcess.ErrorReceived -= DiagnosticProcessOnError;
            _diagnosticProcess.Exited -= ProcessOnExited;
        }
        _onDispose?.Invoke();
        using var activity = AgentActivitySource.Source.StartActivity("ai.agent.stop");
        activity?.SetTag("agent.protocol", "acp");
        _connection.Dispose();
        if (_diagnosticProcess is not null)
            await _diagnosticProcess.DisposeAsync().ConfigureAwait(false);
        _events.Writer.TryComplete();
    }

    private void DiagnosticProcessOnError(string message)
        => _events.Writer.TryWrite(new AgentDiagnosticEvent(message));

    private void ProcessOnExited()
        => _events.Writer.TryComplete(new IOException("ACP agent process exited."));
}
