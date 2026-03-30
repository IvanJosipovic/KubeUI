using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Net.WebSockets;
using k8s;
using k8s.Models;

namespace KubeUI.Kubernetes;

public sealed class PrometheusQueryClient(ILogger<PrometheusQueryClient> logger) : IPrometheusQueryClient
{
    private readonly ILogger<PrometheusQueryClient> _logger = logger;
    private HttpClient? _httpClient;
    private string? _endpointKey;
    private Uri? _baseAddress;

    public Task PrepareAsync(Cluster cluster, ResolvedPrometheusEndpoint endpoint, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(cluster);
        ArgumentNullException.ThrowIfNull(endpoint);

        EnsureTransport(cluster, endpoint);
        return Task.CompletedTask;
    }

    public async Task<PrometheusClientQueryRangeResponse?> QueryRangeAsync(
        Cluster cluster,
        ResolvedPrometheusEndpoint endpoint,
        string query,
        DateTimeOffset start,
        DateTimeOffset end,
        int stepSeconds,
        CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(cluster);
        ArgumentNullException.ThrowIfNull(endpoint);

        EnsureTransport(cluster, endpoint);
        var url = BuildPrometheusUrl(endpoint, query, start, end, stepSeconds);

        using var response = await _httpClient!.GetAsync(url, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PrometheusClientQueryRangeResponse>(cancellationToken: cancellationToken).ConfigureAwait(false);
    }

    public Task ResetAsync()
    {
        _httpClient?.Dispose();
        _httpClient = null;
        _endpointKey = null;
        _baseAddress = null;

        return Task.CompletedTask;
    }

    private void EnsureTransport(Cluster cluster, ResolvedPrometheusEndpoint endpoint)
    {
        var endpointKey = CreateEndpointKey(cluster, endpoint);
        if (_httpClient != null && string.Equals(_endpointKey, endpointKey, StringComparison.Ordinal))
        {
            return;
        }

        ResetAsync().GetAwaiter().GetResult();

        _baseAddress = GetPrometheusBaseAddress(endpoint);
        _httpClient = CreateHttpClient(cluster, endpoint, _baseAddress);

        if (!string.IsNullOrWhiteSpace(endpoint.BearerToken))
        {
            _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", endpoint.BearerToken);
        }

        if (string.IsNullOrWhiteSpace(endpoint.DirectUrl))
        {
            if (string.IsNullOrWhiteSpace(endpoint.Namespace)
                || string.IsNullOrWhiteSpace(endpoint.ServiceName)
                || endpoint.ServicePort is not > 0)
            {
                throw new InvalidOperationException("Prometheus service endpoint is incomplete.");
            }

            _logger.LogDebug(
                "Prepared Prometheus transport for cluster {Name} via direct Kubernetes port-forward stream to service {Namespace}/{Service}:{Port}.",
                cluster.Name,
                endpoint.Namespace,
                endpoint.ServiceName,
                endpoint.ServicePort);
        }
        else
        {
            _logger.LogDebug(
                "Prepared Prometheus transport for cluster {Name} using direct URL {DirectUrl}.",
                cluster.Name,
                endpoint.DirectUrl);
        }

        _endpointKey = endpointKey;
    }

    private string BuildPrometheusUrl(ResolvedPrometheusEndpoint endpoint, string query, DateTimeOffset start, DateTimeOffset end, int stepSeconds)
    {
        return $"{_baseAddress!.ToString().TrimEnd('/')}/api/v1/query_range?query={Uri.EscapeDataString(query)}&start={start.ToUnixTimeSeconds()}&end={end.ToUnixTimeSeconds()}&step={stepSeconds}";
    }

    private static Uri GetPrometheusBaseAddress(ResolvedPrometheusEndpoint endpoint)
    {
        if (!string.IsNullOrWhiteSpace(endpoint.DirectUrl))
        {
            return new Uri(endpoint.DirectUrl.TrimEnd('/') + endpoint.PathPrefix, UriKind.Absolute);
        }

        var scheme = endpoint.UseHttps ? "https" : "http";
        return new Uri($"{scheme}://prometheus.internal{endpoint.PathPrefix}", UriKind.Absolute);
    }

    private HttpClient CreateHttpClient(Cluster cluster, ResolvedPrometheusEndpoint endpoint, Uri baseAddress)
    {
        if (!string.IsNullOrWhiteSpace(endpoint.DirectUrl))
        {
            return new HttpClient
            {
                BaseAddress = baseAddress,
                Timeout = Timeout.InfiniteTimeSpan,
            };
        }

        var handler = new SocketsHttpHandler
        {
            ConnectCallback = (_, cancellationToken) => ConnectToServiceAsync(cluster, endpoint, cancellationToken),
            PooledConnectionIdleTimeout = TimeSpan.FromMinutes(1),
            ConnectTimeout = Timeout.InfiniteTimeSpan,
        };

        return new HttpClient(handler, disposeHandler: true)
        {
            BaseAddress = baseAddress,
            Timeout = Timeout.InfiniteTimeSpan,
        };
    }

    private async ValueTask<Stream> ConnectToServiceAsync(Cluster cluster, ResolvedPrometheusEndpoint endpoint, CancellationToken cancellationToken)
    {
        var (podName, podPort) = await ResolveTargetPodAsync(cluster, endpoint, cancellationToken).ConfigureAwait(false);
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

        var webSocket = await cluster.Client!.WebSocketNamespacedPodPortForwardAsync(podName, endpoint.Namespace!, [podPort], "v4.channel.k8s.io", cancellationToken: linkedCts.Token).ConfigureAwait(false);
        var demux = new StreamDemuxer(webSocket, StreamType.PortForward);
        demux.Start();
        var stream = demux.GetStream((byte?)0, (byte?)0);

        return new PortForwardTunnelStream(stream, demux, webSocket);
    }

    private async Task<(string PodName, int PodPort)> ResolveTargetPodAsync(Cluster cluster, ResolvedPrometheusEndpoint endpoint, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(endpoint.Namespace)
            || string.IsNullOrWhiteSpace(endpoint.ServiceName)
            || endpoint.ServicePort is not > 0)
        {
            throw new InvalidOperationException("Prometheus service endpoint is incomplete.");
        }

        var service = cluster.GetResource<V1Service>(endpoint.Namespace, endpoint.ServiceName)
            ?? throw new InvalidOperationException($"Prometheus service {endpoint.Namespace}/{endpoint.ServiceName} was not found.");
        var servicePort = service.Spec?.Ports?.FirstOrDefault(x => x.Port == endpoint.ServicePort.Value)
            ?? throw new InvalidOperationException($"Prometheus service {endpoint.Namespace}/{endpoint.ServiceName} does not expose port {endpoint.ServicePort}.");

        await cluster.SeedResource<V1EndpointSlice>(true).ConfigureAwait(false);
        cancellationToken.ThrowIfCancellationRequested();

        var endpointSlice = cluster.GetResourceList<V1EndpointSlice>()
            .FirstOrDefault(x => x.Namespace() == service.Namespace() && x.GetLabel("kubernetes.io/service-name") == service.Name());
        if (endpointSlice == null)
        {
            throw new InvalidOperationException($"No endpoint slices found for Prometheus service {endpoint.Namespace}/{endpoint.ServiceName}.");
        }

        var portData = endpointSlice.Ports?.FirstOrDefault(y => y.Name == servicePort.Name);
        if (portData?.Port == null)
        {
            throw new InvalidOperationException($"No matching endpoint port found for Prometheus service {endpoint.Namespace}/{endpoint.ServiceName}.");
        }

        var podName = endpointSlice.Endpoints?
            .Where(x => x.Conditions?.Ready == true && x.Conditions?.Serving == true && string.Equals(x.TargetRef?.Kind, "Pod", StringComparison.Ordinal))
            .Select(x => x.TargetRef!.Name)
            .FirstOrDefault();
        if (string.IsNullOrWhiteSpace(podName))
        {
            throw new InvalidOperationException($"No ready Prometheus pods found for service {endpoint.Namespace}/{endpoint.ServiceName}.");
        }

        return (podName, (int)portData.Port.Value);
    }

    private static string CreateEndpointKey(Cluster cluster, ResolvedPrometheusEndpoint endpoint)
    {
        return string.Join(
            "|",
            cluster.Name,
            endpoint.ProviderKind,
            endpoint.Namespace ?? string.Empty,
            endpoint.ServiceName ?? string.Empty,
            endpoint.ServicePort?.ToString() ?? string.Empty,
            endpoint.DirectUrl ?? string.Empty,
            endpoint.UseHttps,
            endpoint.PathPrefix,
            endpoint.BearerToken ?? string.Empty);
    }

    private sealed class PortForwardTunnelStream(Stream stream, StreamDemuxer demuxer, WebSocket webSocket) : Stream
    {
        private readonly Stream _stream = stream;
        private readonly StreamDemuxer _demuxer = demuxer;
        private readonly WebSocket _webSocket = webSocket;

        public override bool CanRead => _stream.CanRead;
        public override bool CanSeek => false;
        public override bool CanWrite => _stream.CanWrite;
        public override long Length => throw new NotSupportedException();
        public override long Position
        {
            get => throw new NotSupportedException();
            set => throw new NotSupportedException();
        }

        public override void Flush() => _stream.Flush();
        public override Task FlushAsync(CancellationToken cancellationToken) => _stream.FlushAsync(cancellationToken);
        public override int Read(byte[] buffer, int offset, int count) => _stream.Read(buffer, offset, count);
        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => _stream.ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();
        public override int Read(Span<byte> buffer) => _stream.Read(buffer);
        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default) => _stream.ReadAsync(buffer, cancellationToken);
        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
        public override void SetLength(long value) => throw new NotSupportedException();
        public override void Write(byte[] buffer, int offset, int count) => _stream.Write(buffer, offset, count);
        public override Task WriteAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken) => _stream.WriteAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();
        public override void Write(ReadOnlySpan<byte> buffer) => _stream.Write(buffer);
        public override ValueTask WriteAsync(ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default) => _stream.WriteAsync(buffer, cancellationToken);

        protected override void Dispose(bool disposing)
        {
            if (disposing)
            {
                _stream.Dispose();
                _demuxer.Dispose();
                _webSocket.Dispose();
            }

            base.Dispose(disposing);
        }

        public override async ValueTask DisposeAsync()
        {
            await _stream.DisposeAsync().ConfigureAwait(false);
            _demuxer.Dispose();
            _webSocket.Dispose();
            await base.DisposeAsync().ConfigureAwait(false);
        }
    }
}
