using System.Threading.Channels;
using dotacp.client;
using dotacp.protocol;
using KubeUI.AI.Agents;
using KubeUI.AI.Permissions;

namespace KubeUI.AI.Acp;

// dotacp owns protocol dispatch. This callback is the sole protocol-to-domain seam.
internal sealed class DotAcpClient : IAcpClient
{
    private readonly ChannelWriter<AgentEvent> _events;
    private readonly IAgentPermissionService _permissionService;
    private readonly AcpPermissionHandler _permissionHandler;
    private readonly AcpTerminalHandler _terminalHandler;

    public DotAcpClient(
        ChannelWriter<AgentEvent> events,
        IAgentPermissionService? permissionService = null,
        IReadOnlySet<string>? trustedMcpServers = null)
    {
        _events = events;
        _permissionService = permissionService ?? new DenyByDefaultAgentPermissionService();
        _permissionHandler = new AcpPermissionHandler(events, _permissionService, trustedMcpServers);
        _terminalHandler = new AcpTerminalHandler(_permissionService);
    }

    public void OnDisconnected(Connection connection)
    {
        _terminalHandler.Dispose();
        _events.TryComplete(new IOException("ACP agent disconnected."));
    }

    public async Task<ReadTextFileResponse> ReadTextFileAsync(ReadTextFileRequest request, CancellationToken cancellationToken = default)
    {
        var permission = await _permissionService.RequestPermissionAsync(
            new AgentPermissionRequest("read_file", request.Path), cancellationToken).ConfigureAwait(false);
        if (!permission.Allowed)
            throw new UnauthorizedAccessException(permission.Reason ?? $"Reading '{request.Path}' was denied.");

        var content = await File.ReadAllTextAsync(request.Path, cancellationToken).ConfigureAwait(false);
        var lines = content.Split('\n');
        var start = request.Line is null ? 0 : checked((int)request.Line.Value);
        var limit = request.Limit is null ? lines.Length - start : checked((int)request.Limit.Value);
        var selected = lines.Skip(start).Take(Math.Max(0, limit));
        return new ReadTextFileResponse { Content = string.Join('\n', selected) };
    }

    public async Task<WriteTextFileResponse> WriteTextFileAsync(WriteTextFileRequest request, CancellationToken cancellationToken = default)
    {
        var permission = await _permissionService.RequestPermissionAsync(
            new AgentPermissionRequest("write_file", request.Path, IsDestructive: true), cancellationToken).ConfigureAwait(false);
        if (!permission.Allowed)
            throw new UnauthorizedAccessException(permission.Reason ?? $"Writing '{request.Path}' was denied.");

        await File.WriteAllTextAsync(request.Path, request.Content, cancellationToken).ConfigureAwait(false);
        return new WriteTextFileResponse();
    }

    public Task<RequestPermissionResponse> RequestPermissionAsync(RequestPermissionRequest request, CancellationToken cancellationToken = default)
        => _permissionHandler.RequestAsync(request, cancellationToken);

    public Task SessionUpdateAsync(SessionNotification notification, CancellationToken cancellationToken = default)
    {
        _permissionHandler.TrackToolCall(notification.Update);
        if (AcpMapper.ToAgentEvent(notification.Update) is { } agentEvent)
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
        => Task.FromResult<object>(new { });

    public Task ExtNotificationAsync(string method, object notification, CancellationToken cancellationToken = default)
        => Task.CompletedTask;
}
