using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using k8s;
using k8s.Models;

namespace KubeUI.Client;

public partial class PortForwarder : ObservableObject, IEquatable<PortForwarder>, IDisposable
{
    private readonly ICluster _cluster;
    private readonly TcpListener _listener;

    public string Name { get; private set; }

    public string Namespace { get; }

    public int ContainerPort { get; private set; }

    public string Type { get; private set; }

    [ObservableProperty]
    private int _localPort;

    [ObservableProperty]
    private string _status = "Initializing";


    private bool _isDisposing;

    public PortForwarder(ICluster cluster, string @namespace, int localPort = 0)
    {
        _cluster = cluster;
        Namespace = @namespace;
        LocalPort = localPort;

        _listener = new TcpListener(IPAddress.Loopback, localPort);
    }

    public void SetPod(string podName, int containerPort)
    {
        Name = podName;
        ContainerPort = containerPort;
        Type = "Pod";
    }

    public void SetService(string serviceName, int containerPort)
    {
        Name = serviceName;
        ContainerPort = containerPort;
        Type = "Service";
    }

    public void Start()
    {
        if (LocalPort != 0 && !IsPortAvailable(LocalPort))
        {
            Status = "Local port is busy";
            return;
        }

        _listener.Start();

        _listener.BeginAcceptSocket(new AsyncCallback(ClientConnected), null);
        LocalPort = ((IPEndPoint)_listener.LocalEndpoint).Port;
        Status = "Active";
    }

    public void Stop()
    {
        _isDisposing = true;
        Status = "Inactive";
    }

    private void ClientConnected(IAsyncResult result)
    {
        var socket = _listener.EndAcceptSocket(result);
        Task.Run(async () => await HandleConnection(socket));
        _listener.BeginAcceptSocket(new AsyncCallback(ClientConnected), null);
    }

    private async Task HandleConnection(Socket socket)
    {
        var podName = Name;
        if (Type == "Service")
        {
            var service = await _cluster.GetObjectAsync<V1Service>(Namespace, Name);

            var pods = await _cluster.GetObjectDictionaryAsync<V1Pod>();

            var random = new Random();

            var pod = pods.Where(x => service.Spec.Selector.All(y => x.Value.Metadata.Labels.ContainsKey(y.Key) && x.Value.Metadata.Labels[y.Key] == y.Value))
                .Select(x => x.Value)
                .OrderBy(x => random.Next())
                .FirstOrDefault();

            if (pod == null)
            {
                Status = "No pods found for service";
                socket.Close();
                return;
            }

            podName = pod.Name();
        }

        using var webSocket = await _cluster.Client.WebSocketNamespacedPodPortForwardAsync(podName, Namespace, new int[] { ContainerPort }, "v4.channel.k8s.io");
        using var demux = new StreamDemuxer(webSocket, StreamType.PortForward);
        demux.Start();

        await using var stream = demux.GetStream((byte?)0, (byte?)0);

        var read = Task.Run(() =>
        {
            var buffer = new byte[4096];
            while (!_isDisposing && SocketConnected(socket))
            {
                try
                {
                    int bytesReceived = socket.Receive(buffer);
                    stream.Write(buffer, 0, bytesReceived);
                }
                catch (Exception) { }
            }
        });

        var write = Task.Run(() =>
        {
            var buffer = new byte[4096];
            while (!_isDisposing && SocketConnected(socket))
            {
                try
                {
                    var bytesReceived = stream.Read(buffer, 0, 4096);
                    socket.Send(buffer, bytesReceived, 0);
                }
                catch (Exception) { }
            }
        });

        await read;
        await write;

        socket.Close();
    }

    private static bool IsPortAvailable(int port)
    {
        var properties = IPGlobalProperties.GetIPGlobalProperties();
        var activeTcpListeners = properties.GetActiveTcpListeners();
        return !activeTcpListeners.Any(x => x.Port == port);
    }

    private static bool SocketConnected(Socket s)
    {
        bool part1 = s.Poll(1000, SelectMode.SelectRead);
        bool part2 = (s.Available == 0);
        return !part1 || !part2;
    }

    public bool Equals(PortForwarder? other)
    {
        return other != null && other.Name == Name && other.Namespace == Namespace && other.ContainerPort == ContainerPort && other.Type == Type;
    }

    public void Dispose()
    {
        _isDisposing = true;
    }
}
