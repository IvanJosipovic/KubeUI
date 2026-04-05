using System.Net;
using System.Net.Sockets;
using k8s.Models;

#pragma warning disable RCS1075
namespace KubeUI.Kubernetes;

public partial class PortForwarder : ObservableObject, IEquatable<PortForwarder>, IDisposable
{
    private readonly IClusterRuntime _cluster;
    private readonly TcpListener _listener;
    private readonly IPortForwardSessionFactory _sessionFactory;
    private readonly HashSet<Socket> _activeSockets = [];
    private readonly object _activeSocketsGate = new();

    public string Name { get; private set; }

    public string Namespace { get; }

    public int Port { get; private set; }

    public string Type { get; private set; }

    [ObservableProperty]
    public partial int LocalPort { get; set; }

    [ObservableProperty]
    public partial string Status { get; set; } = "Initializing";

    [ObservableProperty]
    public partial int Connections { get; set; }

    private bool _isDisposing;

    public PortForwarder(IClusterRuntime cluster, string @namespace, int localPort = 0)
        : this(cluster, @namespace, localPort, null)
    {
    }

    public PortForwarder(IClusterRuntime cluster, string @namespace, int localPort, IPortForwardSessionFactory? sessionFactory)
    {
        _cluster = cluster;
        Namespace = @namespace;
        LocalPort = localPort;
        _sessionFactory = sessionFactory ?? new KubernetesPortForwardSessionFactory(cluster);

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
        try
        {
            _listener.Start();
            _listener.BeginAcceptSocket(ClientConnected, null);
            LocalPort = ((IPEndPoint)_listener.LocalEndpoint).Port;
            Status = "Active";
        }
        catch (SocketException)
        {
            Status = "Local port is busy";

            try
            {
                _listener.Stop();
            }
            catch
            {
            }
        }
        catch (ObjectDisposedException)
        {
            Status = "Inactive";
        }
    }

    public void Stop()
    {
        _isDisposing = true;
        Status = "Inactive";

        Socket[] sockets;
        lock (_activeSocketsGate)
        {
            sockets = _activeSockets.ToArray();
        }

        foreach (var socket in sockets)
        {
            try
            {
                socket.Shutdown(SocketShutdown.Both);
            }
            catch
            {
            }

            try
            {
                socket.Close();
            }
            catch
            {
            }
        }

        try
        {
            _listener.Stop();
        }
        catch
        {
        }
    }

    private void ClientConnected(IAsyncResult result)
    {
        if (_isDisposing)
        {
            return;
        }

        try
        {
            var socket = _listener.EndAcceptSocket(result);
            lock (_activeSocketsGate)
            {
                _activeSockets.Add(socket);
            }
            _ = Task.Run(() => HandleConnection(socket));

            if (!_isDisposing)
            {
                _listener.BeginAcceptSocket(ClientConnected, null);
            }
        }
        catch (ObjectDisposedException)
        {
        }
        catch (InvalidOperationException)
        {
        }
    }

    private async Task HandleConnection(Socket socket)
    {
        Connections++;
        try
        {
            var podName = Name;
            var podPort = Port;
            if (Type == "Service")
            {
                var service = _cluster.GetResource<V1Service>(Namespace, Name);
                if (service == null)
                {
                    Status = "Service not found";
                    return;
                }

                var servicePort = FindServicePort(service, Port);
                if (servicePort == null)
                {
                    Status = "No port found for Service";
                    return;
                }

                await _cluster.SeedResource<V1EndpointSlice>(true).ConfigureAwait(false);
                var endpointSlice = FindEndpointSlice(service);
                if (endpointSlice == null)
                {
                    Status = "No endpoint slices found for Service";
                    return;
                }

                var portData = FindEndpointSlicePort(endpointSlice, servicePort);
                if (portData == null || portData.Port == null)
                {
                    Status = "No port found for Service";
                    return;
                }

                if (!TrySelectPod(endpointSlice, out var selectedPod))
                {
                    Status = "No pods found for Service";
                    return;
                }

                podName = selectedPod;
                podPort = (int)portData.Port;
            }

            using var session = await _sessionFactory.CreateAsync(podName, Namespace, podPort).ConfigureAwait(false);
            var stream = session.Stream;

            var read = Task.Run(() => CopySocketToStream(socket, stream));
            var write = Task.Run(() => CopyStreamToSocket(socket, stream));

            await Task.WhenAll(read, write).ConfigureAwait(false);
        }
        catch
        {
            if (!_isDisposing)
            {
                Status = "Port forward failed";
            }
        }
        finally
        {
            lock (_activeSocketsGate)
            {
                _activeSockets.Remove(socket);
            }

            try
            {
                socket.Shutdown(SocketShutdown.Both);
            }
            catch
            {
            }

            try
            {
                socket.Close();
            }
            catch
            {
            }

            Connections--;
        }
    }

    private static V1ServicePort? FindServicePort(V1Service? service, int port)
    {
        var ports = service?.Spec?.Ports;
        if (ports == null)
        {
            return null;
        }

        foreach (var candidate in ports)
        {
            if (candidate.Port == port)
            {
                return candidate;
            }
        }

        return null;
    }

    private V1EndpointSlice? FindEndpointSlice(V1Service service)
    {
        var endpointSlices = _cluster.GetResourceList<V1EndpointSlice>();

        foreach (var endpointSlice in endpointSlices)
        {
            if (endpointSlice.Namespace() == service.Namespace()
                && endpointSlice.GetLabel("kubernetes.io/service-name") == service.Name())
            {
                return endpointSlice;
            }
        }

        return null;
    }

    private static Discoveryv1EndpointPort? FindEndpointSlicePort(V1EndpointSlice endpointSlice, V1ServicePort servicePort)
    {
        var ports = endpointSlice.Ports;
        if (ports == null)
        {
            return null;
        }

        foreach (var candidate in ports)
        {
            if (!string.IsNullOrEmpty(servicePort.Name) && string.Equals(candidate.Name, servicePort.Name, StringComparison.Ordinal))
            {
                return candidate;
            }

            if (string.IsNullOrEmpty(servicePort.Name) && candidate.Port == servicePort.Port)
            {
                return candidate;
            }
        }

        return null;
    }

    private static bool TrySelectPod(V1EndpointSlice endpointSlice, out string podName)
    {
        podName = string.Empty;

        var endpoints = endpointSlice.Endpoints;
        if (endpoints == null)
        {
            return false;
        }

        string? selectedPod = null;
        var eligiblePods = 0;

        foreach (var endpoint in endpoints)
        {
            var targetRef = endpoint?.TargetRef;
            var conditions = endpoint?.Conditions;

            if (targetRef?.Kind != "Pod"
                || string.IsNullOrEmpty(targetRef.Name)
                || conditions?.Ready != true
                || conditions.Serving != true)
            {
                continue;
            }

            eligiblePods++;
            if (Random.Shared.Next(eligiblePods) == 0)
            {
                selectedPod = targetRef.Name;
            }
        }

        if (selectedPod == null)
        {
            return false;
        }

        podName = selectedPod;
        return true;
    }

    private static void CopySocketToStream(Socket socket, Stream stream)
    {
        var buffer = new byte[4096];

        while (true)
        {
            int bytesReceived;
            try
            {
                bytesReceived = socket.Receive(buffer);
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            catch (SocketException)
            {
                return;
            }

            if (bytesReceived <= 0)
            {
                return;
            }

            try
            {
                stream.Write(buffer, 0, bytesReceived);
            }
            catch (IOException)
            {
                return;
            }
            catch (ObjectDisposedException)
            {
                return;
            }
        }
    }

    private static void CopyStreamToSocket(Socket socket, Stream stream)
    {
        var buffer = new byte[4096];

        while (true)
        {
            int bytesReceived;
            try
            {
                bytesReceived = stream.Read(buffer, 0, buffer.Length);
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            catch (IOException)
            {
                return;
            }

            if (bytesReceived <= 0)
            {
                return;
            }

            try
            {
                socket.Send(buffer, 0, bytesReceived, SocketFlags.None);
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            catch (SocketException)
            {
                return;
            }
        }
    }

    public bool Equals(PortForwarder? other)
    {
        return other != null && other.Name == Name && other.Namespace == Namespace && other.Port == Port && other.Type == Type;
    }

    public void Dispose()
    {
        Stop();
    }
}
#pragma warning restore RCS1075

