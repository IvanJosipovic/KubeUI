using System.Threading.Channels;
using dotacp.client;
using dotacp.protocol;
using KubeUI.AI.Agents;
using KubeUI.AI.Permissions;

namespace KubeUI.AI.Acp;

// dotacp owns protocol dispatch. This callback is the sole protocol-to-domain seam.
internal sealed class DotAcpClient : IAcpClient, IDisposable
{
    private readonly ChannelWriter<AgentEvent> _events;
    private readonly AcpPermissionHandler _permissionHandler;
    private readonly AcpFileSystemHandler _fileSystemHandler;
    private readonly AcpTerminalHandler _terminalHandler;

    public DotAcpClient(
        ChannelWriter<AgentEvent> events,
        IAgentPermissionService? permissionService = null,
        IReadOnlySet<string>? trustedMcpServers = null,
        IReadOnlySet<string>? fileSystemRoots = null)
    {
        _events = events;
        var effectivePermissionService = permissionService ?? new DenyByDefaultAgentPermissionService();
        _permissionHandler = new AcpPermissionHandler(events, effectivePermissionService, trustedMcpServers);
        _fileSystemHandler = new AcpFileSystemHandler(effectivePermissionService, fileSystemRoots);
        _terminalHandler = new AcpTerminalHandler(effectivePermissionService);
    }

    public void OnDisconnected(Connection connection)
    {
        _permissionHandler.Clear();
        _terminalHandler.Dispose();
        _events.TryComplete(new IOException("ACP agent disconnected."));
    }

    public Task<ReadTextFileResponse> ReadTextFileAsync(ReadTextFileRequest request, CancellationToken cancellationToken = default)
        => _fileSystemHandler.ReadTextFileAsync(request, cancellationToken);

    public Task<WriteTextFileResponse> WriteTextFileAsync(WriteTextFileRequest request, CancellationToken cancellationToken = default)
        => _fileSystemHandler.WriteTextFileAsync(request, cancellationToken);

    public Task<RequestPermissionResponse> RequestPermissionAsync(RequestPermissionRequest request, CancellationToken cancellationToken = default)
        => _permissionHandler.RequestAsync(request, cancellationToken);

    public Task SessionUpdateAsync(SessionNotification notification, CancellationToken cancellationToken = default)
    {
        if (_permissionHandler.TrackToolCall(notification.Update)
            && AcpMapper.ToAgentEvent(notification.Update) is { } agentEvent)
            _events.TryWrite(agentEvent);
        return Task.CompletedTask;
    }

    public Task<CreateTerminalResponse> CreateTerminalAsync(CreateTerminalRequest request, CancellationToken cancellationToken = default)
        => _terminalHandler.CreateAsync(request, cancellationToken);

    public Task<KillTerminalResponse> KillTerminalAsync(KillTerminalRequest request, CancellationToken cancellationToken = default)
        => _terminalHandler.KillAsync(request, cancellationToken);

    public Task<TerminalOutputResponse> TerminalOutputAsync(TerminalOutputRequest request, CancellationToken cancellationToken = default)
        => _terminalHandler.OutputAsync(request, cancellationToken);

    public Task<ReleaseTerminalResponse> ReleaseTerminalAsync(ReleaseTerminalRequest request, CancellationToken cancellationToken = default)
        => _terminalHandler.ReleaseAsync(request, cancellationToken);

    public Task<WaitForTerminalExitResponse> WaitForTerminalExitAsync(WaitForTerminalExitRequest request, CancellationToken cancellationToken = default)
        => _terminalHandler.WaitAsync(request, cancellationToken);

    public Task<object> ExtMethodAsync(string method, object request, CancellationToken cancellationToken = default)
        => Task.FromException<object>(new NotSupportedException($"ACP extension method '{method}' is not supported."));

    public Task ExtNotificationAsync(string method, object notification, CancellationToken cancellationToken = default)
        => Task.FromException(new NotSupportedException($"ACP extension notification '{method}' is not supported."));

    public void Dispose()
    {
        _permissionHandler.Clear();
        _terminalHandler.Dispose();
    }
}
