using System.Net.Http.Headers;

namespace KubeUI.Kubernetes;

public sealed partial class PrometheusQueryClient : IPrometheusQueryClient, IDisposable
{
    private readonly ILogger<PrometheusQueryClient> _logger;
    private readonly IAzureMonitorWorkspaceService? _azureMonitorWorkspaceService;
    private readonly Func<HttpMessageHandler>? _directHandlerFactory;
    private readonly SemaphoreSlim _transportGate = new(1, 1);
    private HttpClient? _httpClient;
    private string? _endpointKey;

    public PrometheusQueryClient(ILogger<PrometheusQueryClient> logger)
        : this(logger, null, null)
    {
    }

    public PrometheusQueryClient(ILogger<PrometheusQueryClient> logger, IAzureMonitorWorkspaceService azureMonitorWorkspaceService)
        : this(logger, azureMonitorWorkspaceService, null)
    {
    }

    internal PrometheusQueryClient(
        ILogger<PrometheusQueryClient> logger,
        IAzureMonitorWorkspaceService? azureMonitorWorkspaceService,
        Func<HttpMessageHandler>? directHandlerFactory)
    {
        _logger = logger;
        _azureMonitorWorkspaceService = azureMonitorWorkspaceService;
        _directHandlerFactory = directHandlerFactory;
    }

    public async Task PrepareAsync(Cluster cluster, ResolvedPrometheusEndpoint endpoint, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(cluster);
        ArgumentNullException.ThrowIfNull(endpoint);

        await EnsureTransportAsync(cluster, endpoint, cancellationToken).ConfigureAwait(false);
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

        await EnsureTransportAsync(cluster, endpoint, cancellationToken).ConfigureAwait(false);

        return await QueryDirectAsync(endpoint, query, start, end, stepSeconds, cancellationToken).ConfigureAwait(false);
    }

    public async Task ResetAsync()
    {
        await _transportGate.WaitAsync().ConfigureAwait(false);
        try
        {
            ResetTransport();
        }
        finally
        {
            _transportGate.Release();
        }
    }

    /// <summary>Disposes the cached HTTP transport and transport setup gate.</summary>
    public void Dispose()
    {
        _transportGate.Wait();
        try
        {
            ResetTransport();
        }
        finally
        {
            _transportGate.Release();
            _transportGate.Dispose();
        }
    }

    private async Task EnsureTransportAsync(Cluster cluster, ResolvedPrometheusEndpoint endpoint, CancellationToken cancellationToken)
    {
        await _transportGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var endpointKey = CreateEndpointKey(cluster, endpoint);
            if (string.Equals(_endpointKey, endpointKey, StringComparison.Ordinal))
            {
                return;
            }

            ResetTransport();

            try
            {
                if (!string.IsNullOrWhiteSpace(endpoint.DirectUrl))
                {
                    _httpClient = CreateDirectHttpClient(endpoint, _directHandlerFactory?.Invoke());
                }
                else
                {
                    await cluster.SeedResource<k8s.Models.V1Service>(true, cancellationToken).ConfigureAwait(false);
                    await EnsureServiceAvailableAsync(cluster, endpoint, cancellationToken).ConfigureAwait(false);
                    PrepareServicePortForwardTransport(cluster, endpoint);
                }

                if (_httpClient is not null
                    && !endpoint.UseAzureMonitorAuthentication
                    && !string.IsNullOrWhiteSpace(endpoint.BearerToken))
                {
                    _httpClient.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", endpoint.BearerToken);
                }

                LogPreparedTransport(cluster, endpoint);
                _endpointKey = endpointKey;
            }
            catch
            {
                ResetTransport();
                throw;
            }
        }
        finally
        {
            _transportGate.Release();
        }
    }

    private void ResetTransport()
    {
        _httpClient?.Dispose();
        _httpClient = null;
        _endpointKey = null;
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
                "Prepared Prometheus transport for cluster {Name} using local port-forward to service {Namespace}/{Service}:{Port}.",
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
            endpoint.BearerToken ?? string.Empty,
            endpoint.UseAzureMonitorAuthentication);
    }
}
