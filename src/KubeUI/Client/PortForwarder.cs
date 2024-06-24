using System;
using System.Linq;
using System.Net.NetworkInformation;
using System.Net.Sockets;
using System.Net;
using System.Threading.Tasks;
using k8s;
using CommunityToolkit.Mvvm.ComponentModel;

namespace KubeUI.Client
{
    public partial class PortForwarder : ObservableObject, IEquatable<PortForwarder>, IDisposable
    {
        private readonly IKubernetes _client;
        private readonly TcpListener _listener;

        public string PodName { get; }

        public string Namespace { get; }

        public int ContainerPort { get; }

        [ObservableProperty]
        private int _localPort;

        [ObservableProperty]
        private string _status = "Initializing";

        private bool _isDisposing;

        public PortForwarder(IKubernetes client, string @namespace, string podName, int containerPort, int localPort = 0)
        {
            _client = client;
            Namespace = @namespace;
            PodName = podName;
            ContainerPort = containerPort;
            LocalPort = localPort;

            _listener = new TcpListener(IPAddress.Loopback, localPort);
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
            using var webSocket = await _client.WebSocketNamespacedPodPortForwardAsync(PodName, Namespace, new int[] { ContainerPort }, "v4.channel.k8s.io");
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
            return other != null && other.PodName == PodName && other.Namespace == Namespace && other.ContainerPort == ContainerPort;
        }

        public void Dispose()
        {
            _isDisposing = true;
        }
    }
}
