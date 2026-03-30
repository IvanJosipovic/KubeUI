using System.Net.Http.Json;
using k8s;
using k8s.Models;

namespace KubeUI.Kubernetes;

public partial class Cluster
{
    private HttpClient? _metricsHttpClient;
    private PortForwarder? _prometheusServicePortForward;
    private PeriodicTimer? _metricsRefreshTimer;
    private CancellationTokenSource? _metricsRefreshCancellationTokenSource;

    [ObservableProperty]
    public partial ObservableCollection<PodMetrics> PodMetrics { get; set; } = [];

    [ObservableProperty]
    public partial ObservableCollection<NodeMetrics> NodeMetrics { get; set; } = [];

    [ObservableProperty]
    public partial bool IsMetricsAvailable { get; set; }

    [ObservableProperty]
    public partial MetricsServiceType MetricsServiceType { get; set; } = MetricsServiceType.None;

    public async Task<PrometheusClientQueryRangeResponse?> GetPrometheusMetrics(string query, DateTimeOffset start, DateTimeOffset end, string step = "1")
    {
        if (_metricsHttpClient == null || MetricsServiceType is not (MetricsServiceType.Prometheus or MetricsServiceType.PrometheusExternal))
        {
            return null;
        }

        var url = $"{GetPrometheusUrl()}/api/v1/query_range?query={Uri.EscapeDataString(query)}&start={start.ToUnixTimeSeconds()}&end={end.ToUnixTimeSeconds()}&step={Uri.EscapeDataString(step)}";
        using var response = await _metricsHttpClient.GetAsync(url).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<PrometheusClientQueryRangeResponse>().ConfigureAwait(false);
    }

    private async Task SyncKubernetesMetricsAsync(CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var nodeMetricsList = await Client.GetKubernetesNodesMetricsAsync().ConfigureAwait(false);
            NodeMetrics.Clear();
            foreach (var item in ((IEnumerable)nodeMetricsList.Items).OfType<NodeMetrics>())
            {
                NodeMetrics.Add(item);
            }

            cancellationToken.ThrowIfCancellationRequested();

            var podMetricsList = await Client.GetKubernetesPodsMetricsAsync().ConfigureAwait(false);
            PodMetrics.Clear();
            foreach (var item in ((IEnumerable)podMetricsList.Items).OfType<PodMetrics>())
            {
                PodMetrics.Add(item);
            }
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating Kubernetes metrics");
        }
    }

    private async Task InitMetrics()
    {
        StopMetrics();

        var kube = Client as k8s.Kubernetes;
        if (kube == null)
        {
            return;
        }

        var clusterMetricsSettings = _settings.GetClusterMetricsSettings(this);
        var configuredType = clusterMetricsSettings.MetricsServiceType;
        var runtimeType = configuredType == MetricsServiceType.Auto
            ? await DetectMetricsServiceTypeAsync(kube, clusterMetricsSettings).ConfigureAwait(false)
            : configuredType;

        MetricsServiceType = runtimeType;

        switch (runtimeType)
        {
            case MetricsServiceType.None:
            case MetricsServiceType.Auto:
                return;
            case MetricsServiceType.KubernetesMetricsServer:
                if (!await CanUseKubernetesMetricsServerAsync(kube).ConfigureAwait(false))
                {
                    MetricsServiceType = MetricsServiceType.None;
                    return;
                }

                IsMetricsAvailable = true;
                _metricsRefreshCancellationTokenSource = new CancellationTokenSource();
                _metricsRefreshTimer = new PeriodicTimer(TimeSpan.FromSeconds(30));

                await SyncKubernetesMetricsAsync(_metricsRefreshCancellationTokenSource.Token).ConfigureAwait(false);

                _ = Task.Run(async () =>
                {
                    try
                    {
                        while (_metricsRefreshTimer != null
                            && await _metricsRefreshTimer.WaitForNextTickAsync(_metricsRefreshCancellationTokenSource.Token).ConfigureAwait(false))
                        {
                            await SyncKubernetesMetricsAsync(_metricsRefreshCancellationTokenSource.Token).ConfigureAwait(false);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                    }
                }, _metricsRefreshCancellationTokenSource.Token);

                return;
            case MetricsServiceType.Prometheus:
                if (!HasPrometheusServiceConfiguration(clusterMetricsSettings))
                {
                    _logger.LogWarning("Prometheus metrics are configured for cluster {Name} but the service configuration is incomplete.", Name);
                    MetricsServiceType = MetricsServiceType.None;
                    return;
                }

                _metricsHttpClient = new HttpClient();
                _prometheusServicePortForward = AddServicePortForward(
                    clusterMetricsSettings.PrometheusServiceNamespace!,
                    clusterMetricsSettings.PrometheusServiceName!,
                    clusterMetricsSettings.PrometheusServicePort!.Value);
                _prometheusServicePortForward.Start();
                return;
            case MetricsServiceType.PrometheusExternal:
                if (string.IsNullOrWhiteSpace(clusterMetricsSettings.PrometheusServerUrl))
                {
                    _logger.LogWarning("External Prometheus metrics are configured for cluster {Name} but the server URL is missing.", Name);
                    MetricsServiceType = MetricsServiceType.None;
                    return;
                }

                _metricsHttpClient = new HttpClient();
                return;
            case MetricsServiceType.AzureManagedPrometheus:
                _logger.LogWarning("Azure Managed Prometheus is not implemented yet for cluster {Name}.", Name);
                MetricsServiceType = MetricsServiceType.None;
                return;
            default:
                MetricsServiceType = MetricsServiceType.None;
                return;
        }
    }

    private async Task<MetricsServiceType> DetectMetricsServiceTypeAsync(k8s.Kubernetes kube, ClusterMetricsSettings clusterMetricsSettings)
    {
        try
        {
            await SeedResource<V1Service>(true).ConfigureAwait(false);

            foreach (var service in GetResourceList<V1Service>())
            {
                if (service.Metadata?.Labels?.TryGetValue("operated-prometheus", out var value) != true
                    || !string.Equals(value, "true", StringComparison.Ordinal))
                {
                    continue;
                }

                var port = service.Spec?.Ports?.FirstOrDefault(x => string.Equals(x.Name, "http-web", StringComparison.Ordinal))
                    ?? service.Spec?.Ports?.FirstOrDefault();

                if (port?.Port == null)
                {
                    continue;
                }

                clusterMetricsSettings.MetricsServiceType = MetricsServiceType.Prometheus;
                clusterMetricsSettings.PrometheusServiceName = service.Name();
                clusterMetricsSettings.PrometheusServiceNamespace = service.Namespace();
                clusterMetricsSettings.PrometheusServicePort = port.Port;
                _settings.Persist();

                return MetricsServiceType.Prometheus;
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Unable to auto-detect Prometheus for cluster {Name}", Name);
        }

        try
        {
            if (await CanUseKubernetesMetricsServerAsync(kube).ConfigureAwait(false))
            {
                clusterMetricsSettings.MetricsServiceType = MetricsServiceType.KubernetesMetricsServer;
                _settings.Persist();
                return MetricsServiceType.KubernetesMetricsServer;
            }
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Unable to auto-detect Kubernetes metrics server for cluster {Name}", Name);
        }

        return MetricsServiceType.None;
    }

    private static bool HasPrometheusServiceConfiguration(ClusterMetricsSettings clusterMetricsSettings)
    {
        return !string.IsNullOrWhiteSpace(clusterMetricsSettings.PrometheusServiceNamespace)
            && !string.IsNullOrWhiteSpace(clusterMetricsSettings.PrometheusServiceName)
            && clusterMetricsSettings.PrometheusServicePort is > 0;
    }

    private string GetPrometheusUrl()
    {
        var clusterMetricsSettings = _settings.GetClusterMetricsSettings(this);

        return MetricsServiceType switch
        {
            MetricsServiceType.Prometheus => $"http://localhost:{_prometheusServicePortForward?.LocalPort ?? throw new InvalidOperationException("Prometheus port-forward is not active.")}",
            MetricsServiceType.PrometheusExternal => clusterMetricsSettings.PrometheusServerUrl?.TrimEnd('/') ?? throw new InvalidOperationException("Prometheus server URL is not configured."),
            _ => throw new NotSupportedException($"Metrics provider {MetricsServiceType} does not expose Prometheus queries."),
        };
    }

    private async Task<bool> CanUseKubernetesMetricsServerAsync(k8s.Kubernetes kube)
    {
        var podReview = new V1SelfSubjectAccessReview
        {
            ApiVersion = V1SelfSubjectAccessReview.KubeGroup + "/" + V1SelfSubjectAccessReview.KubeApiVersion,
            Kind = V1SelfSubjectAccessReview.KubeKind,
            Spec = new()
            {
                ResourceAttributes = new()
                {
                    Group = "metrics.k8s.io",
                    Resource = "pods",
                    Verb = "list"
                }
            }
        };

        var nodeReview = new V1SelfSubjectAccessReview
        {
            ApiVersion = V1SelfSubjectAccessReview.KubeGroup + "/" + V1SelfSubjectAccessReview.KubeApiVersion,
            Kind = V1SelfSubjectAccessReview.KubeKind,
            Spec = new()
            {
                ResourceAttributes = new()
                {
                    Group = "metrics.k8s.io",
                    Resource = "nodes",
                    Verb = "list"
                }
            }
        };

        var podResponse = await kube.CreateSelfSubjectAccessReviewAsync(podReview).ConfigureAwait(false);
        var nodeResponse = await kube.CreateSelfSubjectAccessReviewAsync(nodeReview).ConfigureAwait(false);
        var apiGroups = await Client.Apis.GetAPIVersionsAsync().ConfigureAwait(false);

        return apiGroups.Groups.Any(g => g.Name == "metrics.k8s.io")
            && podResponse.Status.Allowed
            && nodeResponse.Status.Allowed;
    }
}
