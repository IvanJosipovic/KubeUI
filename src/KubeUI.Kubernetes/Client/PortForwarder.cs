using System.Buffers;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using DynamicData.Kernel;
using k8s.Models;
using Microsoft.Extensions.Logging;

#pragma warning disable RCS1075
namespace KubeUI.Kubernetes;

public partial class PortForwarder : ObservableObject, IEquatable<PortForwarder>, IDisposable
{
    private readonly IClusterRuntime _cluster;
    private readonly TcpListener _listener;
    private readonly IPortForwardSessionFactory _sessionFactory;
    private readonly ILogger _logger;
    private readonly HashSet<Socket> _activeSockets = [];
    private readonly Lock _activeSocketsGate = new();

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
        : this(cluster, @namespace, localPort, null, Microsoft.Extensions.Logging.Abstractions.NullLogger.Instance)
    {
    }

    public PortForwarder(IClusterRuntime cluster, string @namespace, int localPort, IPortForwardSessionFactory? sessionFactory, ILogger logger)
    {
        _cluster = cluster;
        Namespace = @namespace;
        LocalPort = localPort;
        _sessionFactory = sessionFactory ?? new KubernetesPortForwardSessionFactory(cluster);
        _logger = logger;

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
            _ = HandleConnection(socket);

            if (!_isDisposing)
            {
                _listener.BeginAcceptSocket(ClientConnected, null);
            }
        }
        catch (ObjectDisposedException ex)
        {
            _logger.LogDebug(ex, "Port forward listener disposed while accepting connection");
        }
        catch (InvalidOperationException ex)
        {
            _logger.LogDebug(ex, "Port forward listener invalid operation while accepting connection");
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
                var service = _cluster.GetResourceSourceCache<V1Service>()
                    .Lookup(new ResourceCacheKey(Namespace, Name))
                    .ValueOrDefault();
                if (service is null)
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

            var read = CopySocketToStream(socket, session is IDuplexPortForwardSession duplex ? duplex.WriteStream : stream);
            var write = CopyStreamToSocket(socket, stream);

            await Task.WhenAll(read, write).ConfigureAwait(false);
        }
        catch (Exception ex) when (!_isDisposing)
        {
            _logger.LogDebug(ex, "Port forward connection failed for {Type} {Namespace}/{Name}:{Port}", Type, Namespace, Name, Port);
            Status = "Port forward failed";
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

        return string.IsNullOrEmpty(servicePort.Name) && ports.Count == 1
            ? ports[0]
            : null;
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

    private static async Task CopySocketToStream(Socket socket, Stream stream)
    {
        var pipe = new Pipe();
        var writeTask = FillPipeFromSocket(socket, pipe.Writer);
        var readTask = ReadPipeToStream(pipe.Reader, stream);

        await Task.WhenAll(writeTask, readTask).ConfigureAwait(false);
    }

    private static async Task FillPipeFromSocket(Socket socket, PipeWriter writer)
    {
        const int BufferSize = 4096;
        try
        {
            while (true)
            {
                Memory<byte> memory = writer.GetMemory(BufferSize);
                int bytesReceived;
                try
                {
                    bytesReceived = await socket.ReceiveAsync(memory, SocketFlags.None).ConfigureAwait(false);
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (SocketException)
                {
                    break;
                }

                if (bytesReceived <= 0)
                {
                    break;
                }

                writer.Advance(bytesReceived);

                var result = await writer.FlushAsync().ConfigureAwait(false);
                if (result.IsCompleted || result.IsCanceled)
                {
                    break;
                }
            }
        }
        catch (Exception)
        {
            // Ignore errors during copy
        }
        finally
        {
            await writer.CompleteAsync().ConfigureAwait(false);
        }
    }

    private static async Task ReadPipeToStream(PipeReader reader, Stream stream)
    {
        try
        {
            while (true)
            {
                var result = await reader.ReadAsync().ConfigureAwait(false);
                var buffer = result.Buffer;

                if (buffer.Length > 0)
                {
                    foreach (var segment in buffer)
                    {
                        await stream.WriteAsync(segment).ConfigureAwait(false);
                    }
                }

                reader.AdvanceTo(buffer.End);

                if (result.IsCompleted || result.IsCanceled)
                {
                    break;
                }
            }
        }
        catch (Exception)
        {
            // Ignore errors during copy
        }
        finally
        {
            await reader.CompleteAsync().ConfigureAwait(false);
        }
    }

    private static async Task CopyStreamToSocket(Socket socket, Stream stream)
    {
        var pipe = new Pipe();
        var readTask = FillPipeFromStream(stream, pipe.Writer);
        var writeTask = ReadPipeToSocket(pipe.Reader, socket);

        await Task.WhenAll(readTask, writeTask).ConfigureAwait(false);
    }

    private static async Task FillPipeFromStream(Stream stream, PipeWriter writer)
    {
        const int BufferSize = 4096;
        try
        {
            while (true)
            {
                Memory<byte> memory = writer.GetMemory(BufferSize);
                int bytesRead;
                try
                {
                    bytesRead = await stream.ReadAsync(memory).ConfigureAwait(false);
                }
                catch (ObjectDisposedException)
                {
                    break;
                }
                catch (IOException)
                {
                    break;
                }

                if (bytesRead <= 0)
                {
                    break;
                }

                writer.Advance(bytesRead);

                var result = await writer.FlushAsync().ConfigureAwait(false);
                if (result.IsCompleted || result.IsCanceled)
                {
                    break;
                }
            }
        }
        catch (Exception)
        {
            // Ignore errors during copy
        }
        finally
        {
            await writer.CompleteAsync().ConfigureAwait(false);
        }
    }

    private static async Task ReadPipeToSocket(PipeReader reader, Socket socket)
    {
        try
        {
            while (true)
            {
                var result = await reader.ReadAsync().ConfigureAwait(false);
                var buffer = result.Buffer;

                if (buffer.Length > 0)
                {
                    foreach (var segment in buffer)
                    {
                        try
                        {
                            socket.Send(segment.Span, SocketFlags.None);
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

                reader.AdvanceTo(buffer.End);

                if (result.IsCompleted || result.IsCanceled)
                {
                    break;
                }
            }
        }
        catch (Exception)
        {
            // Ignore errors during copy
        }
        finally
        {
            await reader.CompleteAsync().ConfigureAwait(false);
        }
    }

    public bool Equals(PortForwarder? other)
    {
        return other != null
            && other.Name == Name
            && other.Namespace == Namespace
            && other.Port == Port
            && other.Type == Type;
    }

    public void Dispose()
    {
        Stop();
    }

    public override bool Equals(object? obj)
    {
        return Equals(obj as PortForwarder);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(Name, Namespace, Port, Type);
    }
}
#pragma warning restore RCS1075
