using System.Net.WebSockets;
using k8s;

namespace KubeUI.Testing;

public sealed class FakeKubernetesWebSocketBuilder(Func<Uri, WebSocket> socketFactory) : WebSocketBuilder
{
    public List<Uri> ConnectedUris { get; } = [];

    public override Task<WebSocket> BuildAndConnectAsync(Uri uri, CancellationToken cancellationToken)
    {
        cancellationToken.ThrowIfCancellationRequested();
        ConnectedUris.Add(uri);
        return Task.FromResult(socketFactory(uri));
    }
}

public sealed class FakeKubernetesWebSocket : WebSocket
{
    public override WebSocketCloseStatus? CloseStatus => null;

    public override string? CloseStatusDescription => null;

    public override WebSocketState State { get; } = WebSocketState.Open;

    public override string? SubProtocol => null;

    public override void Abort()
    {
    }

    public override Task CloseAsync(WebSocketCloseStatus closeStatus, string? statusDescription, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public override Task CloseOutputAsync(WebSocketCloseStatus closeStatus, string? statusDescription, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public override void Dispose()
    {
    }

    public override Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken)
    {
        return Task.FromResult(new WebSocketReceiveResult(0, WebSocketMessageType.Close, true));
    }

    public override ValueTask<ValueWebSocketReceiveResult> ReceiveAsync(Memory<byte> buffer, CancellationToken cancellationToken)
    {
        return ValueTask.FromResult(new ValueWebSocketReceiveResult(0, WebSocketMessageType.Close, true));
    }

    public override Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
    {
        return Task.CompletedTask;
    }

    public override ValueTask SendAsync(ReadOnlyMemory<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
    {
        return ValueTask.CompletedTask;
    }
}
