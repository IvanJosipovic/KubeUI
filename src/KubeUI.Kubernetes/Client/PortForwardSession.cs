using System.Net.WebSockets;
using k8s;

namespace KubeUI.Kubernetes;

public interface IPortForwardSessionFactory
{
    Task<IPortForwardSession> CreateAsync(string podName, string @namespace, int port);
}

public interface IPortForwardSession : IDisposable
{
    Stream Stream { get; }
}

internal sealed class KubernetesPortForwardSessionFactory : IPortForwardSessionFactory
{
    private readonly IClusterRuntime _cluster;

    public KubernetesPortForwardSessionFactory(IClusterRuntime cluster)
    {
        _cluster = cluster;
    }

    public async Task<IPortForwardSession> CreateAsync(string podName, string @namespace, int port)
    {
        WebSocket webSocket = await _cluster.Client!.WebSocketNamespacedPodPortForwardAsync(
            podName,
            @namespace,
            [port],
            "v4.channel.k8s.io").ConfigureAwait(false);

        return new KubernetesPortForwardSession(webSocket);
    }
}

internal sealed class KubernetesPortForwardSession : IPortForwardSession
{
    private readonly WebSocket _webSocket;
    private readonly StreamDemuxer _demux;
    private readonly Stream _stream;

    public KubernetesPortForwardSession(WebSocket webSocket)
    {
        _webSocket = webSocket;
        _demux = new StreamDemuxer(_webSocket, StreamType.PortForward);
        _demux.Start();
        _stream = _demux.GetStream((byte?)0, (byte?)0);
    }

    public Stream Stream => _stream;

    public void Dispose()
    {
        _stream.Dispose();
        _demux.Dispose();
        _webSocket.Dispose();
    }
}
