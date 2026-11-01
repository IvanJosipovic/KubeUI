using System.Net.WebSockets;
using System.Threading.Channels;
using Shouldly;

namespace KubeUI.Kubernetes.Tests;

public sealed class PortForwardSessionTests
{
    [Fact]
    public async Task pending_read_does_not_block_writing_on_same_forwarded_connection()
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(TestContext.Current.CancellationToken);
        timeout.CancelAfter(TimeSpan.FromSeconds(5));
        using var socket = new ReplyWebSocket();
        using var session = new KubernetesPortForwardSession(socket);
        var readBuffer = new byte[2];
        var pendingRead = session.Stream.ReadAsync(readBuffer, timeout.Token).AsTask();
        await session.WriteStream.WriteAsync("go"u8.ToArray(), timeout.Token).AsTask().WaitAsync(timeout.Token);
        var read = await pendingRead.WaitAsync(timeout.Token);
        read.ShouldBe(2);
        readBuffer.ShouldBe("ok"u8.ToArray());
    }

    private sealed class ReplyWebSocket : WebSocket
    {
        private readonly Channel<byte[]> _messages = Channel.CreateUnbounded<byte[]>();
        private readonly CancellationTokenSource _stopped = new();
        private bool _disposed;
        public override WebSocketCloseStatus? CloseStatus => null;
        public override string? CloseStatusDescription => null;
        public override WebSocketState State => WebSocketState.Open;
        public override string? SubProtocol => null;
        public override void Abort() => _stopped.Cancel();
        public override Task CloseAsync(WebSocketCloseStatus closeStatus, string? statusDescription, CancellationToken cancellationToken) => Task.CompletedTask;
        public override Task CloseOutputAsync(WebSocketCloseStatus closeStatus, string? statusDescription, CancellationToken cancellationToken) => Task.CompletedTask;
        public override void Dispose()
        {
            if (_disposed)
                return;
            _disposed = true;
            _stopped.Cancel();
            _stopped.Dispose();
        }
        public override async Task<WebSocketReceiveResult> ReceiveAsync(ArraySegment<byte> buffer, CancellationToken cancellationToken)
        {
            using var linked = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, _stopped.Token);
            var data = await _messages.Reader.ReadAsync(linked.Token);
            data.AsSpan().CopyTo(buffer.AsSpan());
            return new WebSocketReceiveResult(data.Length, WebSocketMessageType.Binary, true);
        }
        public override Task SendAsync(ArraySegment<byte> buffer, WebSocketMessageType messageType, bool endOfMessage, CancellationToken cancellationToken)
        {
            _messages.Writer.TryWrite([0, 0, 0, (byte)'o', (byte)'k']);
            return Task.CompletedTask;
        }
    }
}
