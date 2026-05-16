using System.Net.Http.Headers;

namespace KubeUI.Kubernetes;

public sealed partial class PrometheusQueryClient(ILogger<PrometheusQueryClient> logger) : IPrometheusQueryClient
{
    private readonly ILogger<PrometheusQueryClient> _logger = logger;
    private HttpClient? _httpClient;
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

        if (!string.IsNullOrWhiteSpace(endpoint.DirectUrl))
        {
            return await QueryDirectAsync(endpoint, query, start, end, stepSeconds, cancellationToken).ConfigureAwait(false);
        }

        return await QueryViaKubernetesProxyAsync(cluster, endpoint, query, start, end, stepSeconds, cancellationToken).ConfigureAwait(false);
    }

    public Task ResetAsync()
    {
        _httpClient?.Dispose();
        _httpClient = null;
        _endpointKey = null;

        return Task.CompletedTask;
    }

    private void EnsureTransport(Cluster cluster, ResolvedPrometheusEndpoint endpoint)
    {
        var endpointKey = CreateEndpointKey(cluster, endpoint);
        if (string.Equals(_endpointKey, endpointKey, StringComparison.Ordinal))
        {
            return;
        }

        ResetAsync().GetAwaiter().GetResult();

        if (!string.IsNullOrWhiteSpace(endpoint.DirectUrl))
        {
            _httpClient = CreateDirectHttpClient(endpoint);

            if (!string.IsNullOrWhiteSpace(endpoint.BearerToken))
            {
                _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", endpoint.BearerToken);
            }
        }

        LogPreparedTransport(cluster, endpoint);
        _endpointKey = endpointKey;
    }

    private void LogPreparedTransport(Cluster cluster, ResolvedPrometheusEndpoint endpoint)
    {
        if (string.IsNullOrWhiteSpace(endpoint.DirectUrl))
        {
            if (string.IsNullOrWhiteSpace(endpoint.Namespace)
                || string.IsNullOrWhiteSpace(endpoint.ServiceName)
                || endpoint.ServicePort is not > 0)
            {
                throw new InvalidOperationException("Prometheus service endpoint is incomplete.");
            }

            _logger.LogDebug(
                "Prepared Prometheus transport for cluster {Name} using Kubernetes service proxy to service {Namespace}/{Service}:{Port}.",
                cluster.Name,
                endpoint.Namespace,
                endpoint.ServiceName,
                endpoint.ServicePort);
            return;
        }

        _logger.LogDebug(
            "Prepared Prometheus transport for cluster {Name} using direct URL {DirectUrl}.",
            cluster.Name,
            endpoint.DirectUrl);
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
