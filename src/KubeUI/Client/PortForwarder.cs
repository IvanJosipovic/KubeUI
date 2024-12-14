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

    public int Port { get; private set; }

    public string Type { get; private set; }

    [ObservableProperty]
    public partial int LocalPort { get; set; }

    [ObservableProperty]
    public partial string Status { get; set; } = "Initializing";

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
        Port = containerPort;
        Type = "Pod";
    }

    public void SetService(string serviceName, int servicePort)
    {
        Name = serviceName;
        Port = servicePort;
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
        var podPort = Port;
        if (Type == "Service")
        {
            var service = await _cluster.GetObjectAsync<V1Service>(Namespace, Name);
            var port = service.Spec.Ports.First(x => x.Port == Port);
            var endpoint = await _cluster.GetObjectAsync<V1Endpoints>(Namespace, Name);

            var random = new Random();

            var subset = endpoint.Subsets.First(x => x.Ports.Any(y => y.Name == port.Name));
            var portData = subset.Ports.First(y => y.Name == port.Name);

            var pod = subset.Addresses
                .Select(x => x.TargetRef.Name)
                .OrderBy(x => random.Next())
                .FirstOrDefault();

            if (pod == null)
            {
                Status = "No pods found for Service";
                socket.Close();
                return;
            }
            else
            {
                Status = "Active";
            }

            podName = pod;
            podPort = portData.Port;
        }

        using var webSocket = await _cluster.Client.WebSocketNamespacedPodPortForwardAsync(podName, Namespace, [podPort], "v4.channel.k8s.io");
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
        return other != null && other.Name == Name && other.Namespace == Namespace && other.Port == Port && other.Type == Type;
    }

    public void Dispose()
    {
        _isDisposing = true;
    }
}
