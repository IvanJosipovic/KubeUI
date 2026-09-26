using k8s.Models;

namespace KubeUI.Kubernetes;

public sealed partial class MetricsService : ObservableObject, IMetricsService, IDisposable
{

    private readonly ILogger<MetricsService> _logger;
    private readonly IClusterSettingsStore _settings;
    private readonly IReadOnlyDictionary<PrometheusProviderKind, IPrometheusProvider> _prometheusProviders;
    private readonly IPrometheusQueryClient _prometheusQueryClient;
    private readonly KubernetesMetricsCollector _kubernetesMetricsCollector;
    private readonly PrometheusMetricQueries _prometheusMetricQueries;
    private Cluster? _cluster;
    private CancellationTokenSource? _lifecycleCancellationTokenSource;
    private ResolvedPrometheusEndpoint? _resolvedPrometheusEndpoint;
    private IPrometheusProvider? _resolvedPrometheusProvider;

    public MetricsService(ILogger<MetricsService> logger, IClusterSettingsStore settings, IEnumerable<IPrometheusProvider> prometheusProviders, IPrometheusQueryClient prometheusQueryClient)
        : this(logger, settings, prometheusProviders, prometheusQueryClient, TimeProvider.System)
    {
    }

    internal MetricsService(
        ILogger<MetricsService> logger,
        IClusterSettingsStore settings,
        IEnumerable<IPrometheusProvider> prometheusProviders,
        IPrometheusQueryClient prometheusQueryClient,
        TimeProvider timeProvider)
    {
        _logger = logger;
        _settings = settings;
        _prometheusProviders = prometheusProviders.ToDictionary(static x => x.Kind);
        _prometheusQueryClient = prometheusQueryClient;
        _kubernetesMetricsCollector = new KubernetesMetricsCollector(logger, PodMetrics, NodeMetrics);
        _prometheusMetricQueries = new PrometheusMetricQueries(logger, prometheusQueryClient, timeProvider);
    }

    [ObservableProperty]
    public partial ObservableCollection<PodMetrics> PodMetrics { get; set; } = [];

    [ObservableProperty]
    public partial ObservableCollection<NodeMetrics> NodeMetrics { get; set; } = [];

    [ObservableProperty]
    public partial bool IsMetricsAvailable { get; set; }

    [ObservableProperty]
    public partial ActiveMetricsBackend ActiveMetricsBackend { get; set; } = ActiveMetricsBackend.None;

    public async Task InitializeAsync(Cluster cluster)
    {
        ArgumentNullException.ThrowIfNull(cluster);

        await StopAsync().ConfigureAwait(false);
        _cluster = cluster;
        _lifecycleCancellationTokenSource = new CancellationTokenSource();

        var kube = cluster.Client as k8s.Kubernetes;
        if (kube == null)
        {
            _logger.LogDebug("Skipping metrics initialization for cluster {Name} because the Kubernetes client is not available.", cluster.Name);
            return;
        }

        var settings = _settings.GetClusterMetricsSettings(cluster);
        var configuredType = settings.MetricsServiceType;
        _logger.LogInformation(
            "Initializing metrics for cluster {Name}. Configured backend: {MetricsBackend}, configured Prometheus provider: {PrometheusProvider}.",
            cluster.Name,
            configuredType,
            settings.PrometheusProviderKind?.ToString() ?? "auto");

        if (configuredType == MetricsServiceType.None)
        {
            _logger.LogInformation("Metrics are disabled for cluster {Name}.", cluster.Name);
            return;
        }

        if (configuredType == MetricsServiceType.KubernetesMetricsServer)
        {
            _logger.LogInformation("Cluster {Name} is configured to use Kubernetes Metrics Server.", cluster.Name);
            await StartKubernetesMetricsAsync(cluster).ConfigureAwait(false);
            return;
        }

        if (configuredType is MetricsServiceType.Prometheus or MetricsServiceType.Auto)
        {
            _logger.LogInformation("Cluster {Name} will attempt Prometheus metrics initialization.", cluster.Name);
            var promReady = await TryStartPrometheusAsync(cluster, kube, settings).ConfigureAwait(false);
            if (promReady)
            {
                return;
            }

            if (configuredType == MetricsServiceType.Auto)
            {
                _logger.LogError(
                    "Prometheus metrics failed for cluster {Name}; falling back to Kubernetes Metrics Server. Check Prometheus configuration and availability.",
                    cluster.Name);
                await StartKubernetesMetricsAsync(cluster).ConfigureAwait(false);
            }
        }
    }

    public Task<IReadOnlyList<MetricProviderInfo>> GetAvailablePrometheusProvidersAsync()
    {
        IReadOnlyList<MetricProviderInfo> providers = _prometheusProviders.Values
            .OrderBy(static x => x.Name, StringComparer.Ordinal)
            .Select(static x => new MetricProviderInfo(x.Kind, x.Name, x.IsConfigurable))
            .ToArray();

        return Task.FromResult(providers);
    }

    public async Task StopAsync()
    {
        if (_cluster != null)
        {
            _logger.LogDebug("Stopping metrics service for cluster {Name}.", _cluster.Name);
        }

        _lifecycleCancellationTokenSource?.Cancel();
        _lifecycleCancellationTokenSource?.Dispose();
        _lifecycleCancellationTokenSource = null;

        await _kubernetesMetricsCollector.StopAsync().ConfigureAwait(false);

        await _prometheusMetricQueries.StopAsync().ConfigureAwait(false);

        _resolvedPrometheusEndpoint = null;
        _resolvedPrometheusProvider = null;

        PodMetrics.Clear();
        NodeMetrics.Clear();
        IsMetricsAvailable = false;
        ActiveMetricsBackend = ActiveMetricsBackend.None;
        _cluster = null;
    }

    public Task<MetricResultSet> RequestMetricsAsync(MetricRequest request, CancellationToken cancellationToken = default)
    {
        if (_cluster == null
            || _resolvedPrometheusProvider == null
            || _resolvedPrometheusEndpoint == null
            || ActiveMetricsBackend.Type != MetricsServiceType.Prometheus)
        {
            return Task.FromResult(MetricResultSet.Empty);
        }
        return _prometheusMetricQueries.RequestAsync(
            _cluster,
            _resolvedPrometheusEndpoint,
            _resolvedPrometheusProvider,
            request,
            _lifecycleCancellationTokenSource?.Token ?? CancellationToken.None,
            cancellationToken);
    }

    private async Task<bool> TryStartPrometheusAsync(Cluster cluster, k8s.Kubernetes kube, ClusterMetricsSettings settings)
    {
        try
        {
            _logger.LogDebug(
                "Resolving Prometheus endpoint for cluster {Name}. Configured provider: {Provider}.",
                cluster.Name,
                settings.PrometheusProviderKind?.ToString() ?? "auto");
            var resolved = await ResolvePrometheusEndpointAsync(kube, settings, _lifecycleCancellationTokenSource?.Token ?? CancellationToken.None).ConfigureAwait(false);
            if (resolved.Endpoint == null || resolved.Provider == null)
            {
                _logger.LogInformation("No Prometheus endpoint could be resolved for cluster {Name}.", cluster.Name);
                return false;
            }

            _resolvedPrometheusEndpoint = resolved.Endpoint;
            _resolvedPrometheusProvider = resolved.Provider;
            await _prometheusQueryClient.PrepareAsync(cluster, resolved.Endpoint, _lifecycleCancellationTokenSource?.Token ?? CancellationToken.None).ConfigureAwait(false);
            _logger.LogInformation(
                "Resolved Prometheus provider {Provider} for cluster {Name}: {Endpoint}.",
                resolved.Provider.Kind,
                cluster.Name,
                DescribeEndpoint(resolved.Endpoint));
            if (string.IsNullOrWhiteSpace(resolved.Endpoint.Namespace)
                && string.IsNullOrWhiteSpace(resolved.Endpoint.DirectUrl))
            {
                _logger.LogWarning(
                    "Resolved Prometheus provider {Provider} for cluster {Name}, but neither a direct URL nor a service endpoint was available: {Endpoint}.",
                    resolved.Provider.Kind,
                    cluster.Name,
                    DescribeEndpoint(resolved.Endpoint));
                return false;
            }

            if (string.IsNullOrWhiteSpace(resolved.Endpoint.Namespace)
                || string.IsNullOrWhiteSpace(resolved.Endpoint.ServiceName)
                || resolved.Endpoint.ServicePort is not > 0)
            {
                if (string.IsNullOrWhiteSpace(resolved.Endpoint.DirectUrl))
                {
                    _logger.LogWarning(
                        "Resolved Prometheus provider {Provider} for cluster {Name}, but the service endpoint was incomplete: {Endpoint}.",
                        resolved.Provider.Kind,
                        cluster.Name,
                        DescribeEndpoint(resolved.Endpoint));
                    return false;
                }
            }

            ActiveMetricsBackend = ActiveMetricsBackend.Prometheus(resolved.Provider.Kind);
            IsMetricsAvailable = true;
            _logger.LogInformation(
                "Prometheus metrics activated for cluster {Name} using provider {Provider}.",
                cluster.Name,
                resolved.Provider.Kind);
            return true;
        }
        catch (OperationCanceledException) when (_lifecycleCancellationTokenSource?.IsCancellationRequested == true)
        {
            throw;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Unable to initialize Prometheus metrics for cluster {Name}", cluster.Name);
            _resolvedPrometheusEndpoint = null;
            _resolvedPrometheusProvider = null;
            await _prometheusQueryClient.ResetAsync().ConfigureAwait(false);
            ActiveMetricsBackend = ActiveMetricsBackend.None;
            return false;
        }
    }

    private async Task StartKubernetesMetricsAsync(Cluster cluster)
    {
        _logger.LogDebug("Checking Kubernetes Metrics Server availability for cluster {Name}.", cluster.Name);
        if (!await _kubernetesMetricsCollector.StartAsync(cluster, _lifecycleCancellationTokenSource?.Token ?? CancellationToken.None).ConfigureAwait(false))
        {
            ActiveMetricsBackend = ActiveMetricsBackend.None;
            IsMetricsAvailable = false;
            _logger.LogInformation("Kubernetes Metrics Server is not available for cluster {Name}.", cluster.Name);
            return;
        }

        ActiveMetricsBackend = ActiveMetricsBackend.KubernetesMetricsServer;
        IsMetricsAvailable = true;
        _logger.LogInformation("Kubernetes Metrics Server metrics activated for cluster {Name}.", cluster.Name);
    }

    private async Task<(ResolvedPrometheusEndpoint? Endpoint, IPrometheusProvider? Provider)> ResolvePrometheusEndpointAsync(
        k8s.Kubernetes kube,
        ClusterMetricsSettings settings,
        CancellationToken cancellationToken)
    {
        if (settings.PrometheusProviderKind != null)
        {
            if (!_prometheusProviders.TryGetValue(settings.PrometheusProviderKind.Value, out var selectedProvider))
            {
                throw new InvalidOperationException($"Unknown Prometheus provider '{settings.PrometheusProviderKind}'.");
            }

            _logger.LogInformation(
                "Using explicitly configured Prometheus provider {Provider} for cluster {Name}.",
                selectedProvider.Kind,
                _cluster?.Name);
            return (await selectedProvider.TryResolveServiceAsync(kube, settings, cancellationToken).ConfigureAwait(false), selectedProvider);
        }

        foreach (var provider in GetProviderResolutionOrder())
        {
            try
            {
                _logger.LogDebug("Trying Prometheus provider {Provider} for cluster {Name}.", provider.Kind, _cluster?.Name);
                var endpoint = await provider.TryResolveServiceAsync(kube, settings, cancellationToken).ConfigureAwait(false);
                if (endpoint != null)
                {
                    _logger.LogInformation(
                        "Prometheus provider {Provider} matched cluster {Name}: {Endpoint}.",
                        provider.Kind,
                        _cluster?.Name,
                        DescribeEndpoint(endpoint));
                    return (endpoint, provider);
                }

                _logger.LogDebug("Prometheus provider {Provider} did not match cluster {Name}.", provider.Kind, _cluster?.Name);
            }
            catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
            {
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Prometheus provider {Provider} did not resolve for cluster {Name}", provider.Kind, _cluster?.Name);
            }
        }

        _logger.LogInformation("No Prometheus providers matched cluster {Name}.", _cluster?.Name);
        return (null, null);
    }

    private IEnumerable<IPrometheusProvider> GetProviderResolutionOrder()
    {
        static int GetRank(IPrometheusProvider provider) => provider.Kind switch
        {
            PrometheusProviderKind.Operator => 0,
            PrometheusProviderKind.OpenShift => 1,
            PrometheusProviderKind.Manual => 2,
            PrometheusProviderKind.External => 3,
            PrometheusProviderKind.AzureMonitor => 4,
            _ => 99,
        };

        return _prometheusProviders.Values.OrderBy(GetRank).ThenBy(static x => x.Name, StringComparer.Ordinal);
    }

    internal Task SyncKubernetesMetricsAsync(Cluster cluster, CancellationToken cancellationToken) => _kubernetesMetricsCollector.SyncAsync(cluster, cancellationToken);

    public void Dispose()
    {
        StopAsync().GetAwaiter().GetResult();
    }

    private static string DescribeEndpoint(ResolvedPrometheusEndpoint endpoint)
    {
        if (!string.IsNullOrWhiteSpace(endpoint.DirectUrl))
        {
            return $"directUrl={endpoint.DirectUrl}, useHttps={endpoint.UseHttps}, pathPrefix={endpoint.PathPrefix}";
        }

        return $"namespace={endpoint.Namespace}, service={endpoint.ServiceName}, port={endpoint.ServicePort}, useHttps={endpoint.UseHttps}, pathPrefix={endpoint.PathPrefix}";
    }

}
