using System.Net.Http.Headers;
using System.Net.Http.Json;

namespace KubeUI.Kubernetes;

internal sealed class PrometheusQueryClient(ILogger<PrometheusQueryClient> logger) : IPrometheusQueryClient
{
    private readonly ILogger<PrometheusQueryClient> _logger = logger;
    private HttpClient? _httpClient;
    private PortForwarder? _portForwarder;
    private string? _endpointKey;

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
        _portForwarder?.Dispose();
        _portForwarder = null;

        _httpClient?.Dispose();
        _httpClient = null;
        _endpointKey = null;

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

        _httpClient = new HttpClient
        {
            Timeout = Timeout.InfiniteTimeSpan,
        };

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

            _portForwarder = cluster.AddServicePortForward(endpoint.Namespace, endpoint.ServiceName, endpoint.ServicePort.Value);
            _portForwarder.Start();

            if (!string.Equals(_portForwarder.Status, "Active", StringComparison.Ordinal) || _portForwarder.LocalPort <= 0)
            {
                throw new InvalidOperationException($"Prometheus port-forward did not start successfully. Status: {_portForwarder.Status}");
            }

            _logger.LogDebug(
                "Prepared Prometheus transport for cluster {Name} via local port-forward {LocalPort}.",
                cluster.Name,
                _portForwarder.LocalPort);
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
        var baseUrl = GetPrometheusBaseUrl(endpoint);
        return $"{baseUrl}/api/v1/query_range?query={Uri.EscapeDataString(query)}&start={start.ToUnixTimeSeconds()}&end={end.ToUnixTimeSeconds()}&step={stepSeconds}";
    }

    private string GetPrometheusBaseUrl(ResolvedPrometheusEndpoint endpoint)
    {
        if (!string.IsNullOrWhiteSpace(endpoint.DirectUrl))
        {
            return endpoint.DirectUrl.TrimEnd('/') + endpoint.PathPrefix;
        }

        if (_portForwarder?.LocalPort is not > 0)
        {
            throw new InvalidOperationException("Prometheus port-forward is not active.");
        }

        var scheme = endpoint.UseHttps ? "https" : "http";
        return $"{scheme}://localhost:{_portForwarder.LocalPort}{endpoint.PathPrefix}";
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
}
