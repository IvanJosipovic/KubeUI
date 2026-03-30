using System.Net.Http.Headers;
using System.Net.Http.Json;
using k8s;
using k8s.Models;
using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace KubeUI.Kubernetes;

public sealed partial class MetricsService : ObservableObject, IMetricsService
{
    private static readonly TimeSpan s_prometheusRetryBaseDelay = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan s_prometheusRequestTimeout = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan s_prometheusCacheDuration = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan s_prometheusFailureCooldown = TimeSpan.FromMinutes(1);

    private readonly ILogger<MetricsService> _logger;
    private readonly IClusterSettingsStore _settings;
    private readonly IReadOnlyDictionary<PrometheusProviderKind, IPrometheusProvider> _prometheusProviders;
    private readonly ResiliencePipeline<HttpResponseMessage> _prometheusQueryPipeline;
    private readonly object _metricsRequestSync = new();
    private Cluster? _cluster;
    private HttpClient? _prometheusHttpClient;
    private PortForwarder? _prometheusServicePortForward;
    private PeriodicTimer? _metricsRefreshTimer;
    private CancellationTokenSource? _metricsRefreshCancellationTokenSource;
    private ResolvedPrometheusEndpoint? _resolvedPrometheusEndpoint;
    private IPrometheusProvider? _resolvedPrometheusProvider;
    private MetricsServiceType _configuredMetricsServiceType = MetricsServiceType.None;
    private DateTimeOffset? _prometheusUnavailableUntilUtc;
    private bool _prometheusFailureLogged;
    private Dictionary<string, CachedMetricResult> _metricResultCache = new(StringComparer.Ordinal);
    private Dictionary<string, Task<MetricResultSet>> _inflightMetricRequests = new(StringComparer.Ordinal);

    public MetricsService(ILogger<MetricsService> logger, IClusterSettingsStore settings, IEnumerable<IPrometheusProvider> prometheusProviders)
    {
        _logger = logger;
        _settings = settings;
        _prometheusProviders = prometheusProviders.ToDictionary(static x => x.Kind);
        _prometheusQueryPipeline = new ResiliencePipelineBuilder<HttpResponseMessage>
        {
            Name = nameof(MetricsService),
            InstanceName = "PrometheusQueryRange",
        }
        .AddRetry(new HttpRetryStrategyOptions
        {
            MaxRetryAttempts = 3,
            Delay = s_prometheusRetryBaseDelay,
            BackoffType = DelayBackoffType.Exponential,
            UseJitter = true,
        })
        .Build();
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

        var kube = cluster.Client as k8s.Kubernetes;
        if (kube == null)
        {
            return;
        }

        var settings = NormalizeLegacySettings(_settings.GetClusterMetricsSettings(cluster));
        var configuredType = settings.MetricsServiceType;
        _configuredMetricsServiceType = configuredType;

        if (configuredType == MetricsServiceType.None)
        {
            return;
        }

        if (configuredType == MetricsServiceType.KubernetesMetricsServer)
        {
            await StartKubernetesMetricsAsync(cluster, kube).ConfigureAwait(false);
            return;
        }

        if (configuredType is MetricsServiceType.Prometheus or MetricsServiceType.Auto)
        {
            var promReady = await TryStartPrometheusAsync(cluster, kube, settings).ConfigureAwait(false);
            if (promReady)
            {
                return;
            }

            if (configuredType == MetricsServiceType.Auto)
            {
                await StartKubernetesMetricsAsync(cluster, kube).ConfigureAwait(false);
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

    public Task StopAsync()
    {
        _metricsRefreshCancellationTokenSource?.Cancel();
        _metricsRefreshCancellationTokenSource?.Dispose();
        _metricsRefreshCancellationTokenSource = null;

        _metricsRefreshTimer?.Dispose();
        _metricsRefreshTimer = null;

        _prometheusServicePortForward?.Dispose();
        _prometheusServicePortForward = null;

        _prometheusHttpClient?.Dispose();
        _prometheusHttpClient = null;

        _resolvedPrometheusEndpoint = null;
        _resolvedPrometheusProvider = null;

        PodMetrics.Clear();
        NodeMetrics.Clear();
        IsMetricsAvailable = false;
        ActiveMetricsBackend = ActiveMetricsBackend.None;
        _configuredMetricsServiceType = MetricsServiceType.None;
        _prometheusUnavailableUntilUtc = null;
        _prometheusFailureLogged = false;
        lock (_metricsRequestSync)
        {
            _metricResultCache.Clear();
            _inflightMetricRequests.Clear();
        }
        _cluster = null;

        return Task.CompletedTask;
    }

    public async Task<MetricResultSet> RequestMetricsAsync(MetricRequest request, CancellationToken cancellationToken = default)
    {
        if (_cluster == null
            || _prometheusHttpClient == null
            || _resolvedPrometheusProvider == null
            || _resolvedPrometheusEndpoint == null
            || ActiveMetricsBackend.Type != MetricsServiceType.Prometheus)
        {
            return MetricResultSet.Empty;
        }

        var (start, end, stepSeconds) = ResolveTimeRange(request);
        if (_prometheusUnavailableUntilUtc is { } unavailableUntil && unavailableUntil > DateTimeOffset.UtcNow)
        {
            return MetricResultSet.Empty;
        }

        var cacheKey = CreateMetricRequestCacheKey(request, start, end, stepSeconds);
        Task<MetricResultSet>? inflightTask;

        lock (_metricsRequestSync)
        {
            if (_metricResultCache.TryGetValue(cacheKey, out var cached)
                && DateTimeOffset.UtcNow - cached.TimestampUtc < s_prometheusCacheDuration)
            {
                return cached.Result;
            }

            if (_inflightMetricRequests.TryGetValue(cacheKey, out inflightTask))
            {
                goto AwaitInflight;
            }

            inflightTask = LoadMetricResultSetAsync(request, start, end, stepSeconds, cancellationToken);
            _inflightMetricRequests[cacheKey] = inflightTask;
        }

        try
        {
            var result = await inflightTask.ConfigureAwait(false);
            lock (_metricsRequestSync)
            {
                _metricResultCache[cacheKey] = new CachedMetricResult(DateTimeOffset.UtcNow, result);
            }

            return result;
        }
        finally
        {
            lock (_metricsRequestSync)
            {
                _inflightMetricRequests.Remove(cacheKey);
            }
        }

AwaitInflight:
        return await inflightTask.ConfigureAwait(false);
    }

    private async Task<MetricResultSet> LoadMetricResultSetAsync(
        MetricRequest request,
        DateTimeOffset start,
        DateTimeOffset end,
        int stepSeconds,
        CancellationToken cancellationToken)
    {
        var tasks = request.Queries.Select(query => LoadMetricSeriesAsync(query, request.Category, start, end, stepSeconds, request.Frames, cancellationToken));
        var loaded = await Task.WhenAll(tasks).ConfigureAwait(false);

        return new MetricResultSet
        {
            Metrics = loaded.ToDictionary(static x => x.Name, static x => x.Series, StringComparer.Ordinal),
        };
    }

    private async Task<(string Name, IReadOnlyList<MetricSeries> Series)> LoadMetricSeriesAsync(
        MetricQueryDefinition query,
        MetricCategory category,
        DateTimeOffset start,
        DateTimeOffset end,
        int stepSeconds,
        int frames,
        CancellationToken cancellationToken)
    {
        var promQuery = _resolvedPrometheusProvider!.BuildQuery(category, query.Name, query.Options);
        var result = await ExecuteQueryRangeAsync(promQuery, start, end, stepSeconds, cancellationToken).ConfigureAwait(false);

        if (result == null || !string.Equals(result.Status, "success", StringComparison.Ordinal))
        {
            return (query.Name, []);
        }

        return (query.Name, NormalizeResultSet(query.Name, result, frames));
    }

    private async Task<bool> TryStartPrometheusAsync(Cluster cluster, k8s.Kubernetes kube, ClusterMetricsSettings settings)
    {
        try
        {
            var resolved = await ResolvePrometheusEndpointAsync(kube, settings, CancellationToken.None).ConfigureAwait(false);
            if (resolved.Endpoint == null || resolved.Provider == null)
            {
                return false;
            }

            _resolvedPrometheusEndpoint = resolved.Endpoint;
            _resolvedPrometheusProvider = resolved.Provider;
            _prometheusHttpClient = CreatePrometheusHttpClient(resolved.Endpoint);

            if (!string.IsNullOrWhiteSpace(resolved.Endpoint.DirectUrl))
            {
                ActiveMetricsBackend = ActiveMetricsBackend.Prometheus(resolved.Provider.Kind);
                IsMetricsAvailable = true;
                return true;
            }

            if (string.IsNullOrWhiteSpace(resolved.Endpoint.Namespace)
                || string.IsNullOrWhiteSpace(resolved.Endpoint.ServiceName)
                || resolved.Endpoint.ServicePort is not > 0)
            {
                return false;
            }

            _prometheusServicePortForward = cluster.AddServicePortForward(
                resolved.Endpoint.Namespace,
                resolved.Endpoint.ServiceName,
                resolved.Endpoint.ServicePort.Value);
            _prometheusServicePortForward.Start();

            ActiveMetricsBackend = ActiveMetricsBackend.Prometheus(resolved.Provider.Kind);
            IsMetricsAvailable = true;
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Unable to initialize Prometheus metrics for cluster {Name}", cluster.Name);
            _resolvedPrometheusEndpoint = null;
            _resolvedPrometheusProvider = null;
            _prometheusHttpClient?.Dispose();
            _prometheusHttpClient = null;
            _prometheusServicePortForward?.Dispose();
            _prometheusServicePortForward = null;
            ActiveMetricsBackend = ActiveMetricsBackend.None;
            return false;
        }
    }

    private async Task StartKubernetesMetricsAsync(Cluster cluster, k8s.Kubernetes kube)
    {
        if (!await CanUseKubernetesMetricsServerAsync(cluster, kube).ConfigureAwait(false))
        {
            ActiveMetricsBackend = ActiveMetricsBackend.None;
            return;
        }

        ActiveMetricsBackend = ActiveMetricsBackend.KubernetesMetricsServer;
        IsMetricsAvailable = true;
        _metricsRefreshCancellationTokenSource = new CancellationTokenSource();
        _metricsRefreshTimer = new PeriodicTimer(TimeSpan.FromSeconds(30));

        await SyncKubernetesMetricsAsync(cluster, _metricsRefreshCancellationTokenSource.Token).ConfigureAwait(false);

        _ = Task.Run(async () =>
        {
            try
            {
                while (_metricsRefreshTimer != null
                    && await _metricsRefreshTimer.WaitForNextTickAsync(_metricsRefreshCancellationTokenSource.Token).ConfigureAwait(false))
                {
                    await SyncKubernetesMetricsAsync(cluster, _metricsRefreshCancellationTokenSource.Token).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }, _metricsRefreshCancellationTokenSource.Token);
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

            return (await selectedProvider.TryResolveServiceAsync(kube, settings, cancellationToken).ConfigureAwait(false), selectedProvider);
        }

        foreach (var provider in GetProviderResolutionOrder())
        {
            try
            {
                var endpoint = await provider.TryResolveServiceAsync(kube, settings, cancellationToken).ConfigureAwait(false);
                if (endpoint != null)
                {
                    return (endpoint, provider);
                }
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Prometheus provider {Provider} did not resolve for cluster {Name}", provider.Kind, _cluster?.Name);
            }
        }

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
            _ => 99,
        };

        return _prometheusProviders.Values.OrderBy(GetRank).ThenBy(static x => x.Name, StringComparer.Ordinal);
    }

    private HttpClient CreatePrometheusHttpClient(ResolvedPrometheusEndpoint endpoint)
    {
        var client = new HttpClient
        {
            Timeout = Timeout.InfiniteTimeSpan,
        };

        if (!string.IsNullOrWhiteSpace(endpoint.BearerToken))
        {
            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", endpoint.BearerToken);
        }

        return client;
    }

    private async Task<PrometheusClientQueryRangeResponse?> ExecuteQueryRangeAsync(string query, DateTimeOffset start, DateTimeOffset end, int stepSeconds, CancellationToken cancellationToken)
    {
        var url = BuildPrometheusUrl(query, start, end, stepSeconds);
        Exception? lastException = null;

        try
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(s_prometheusRequestTimeout);

            using var response = await _prometheusQueryPipeline.ExecuteAsync(
                async token => await _prometheusHttpClient!.GetAsync(url, token).ConfigureAwait(false),
                timeoutCts.Token).ConfigureAwait(false);

            response.EnsureSuccessStatusCode();
            HandlePrometheusQuerySuccess();
            return await response.Content.ReadFromJsonAsync<PrometheusClientQueryRangeResponse>(cancellationToken: cancellationToken).ConfigureAwait(false);
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            lastException = ex;
        }
        catch (HttpRequestException ex)
        {
            lastException = ex;
        }

        await HandlePrometheusQueryFailureAsync(lastException).ConfigureAwait(false);
        return null;
    }

    private string BuildPrometheusUrl(string query, DateTimeOffset start, DateTimeOffset end, int stepSeconds)
    {
        var baseUrl = GetPrometheusBaseUrl();
        return $"{baseUrl}/api/v1/query_range?query={Uri.EscapeDataString(query)}&start={start.ToUnixTimeSeconds()}&end={end.ToUnixTimeSeconds()}&step={stepSeconds}";
    }

    private string GetPrometheusBaseUrl()
    {
        if (_resolvedPrometheusEndpoint == null)
        {
            throw new InvalidOperationException("Prometheus endpoint has not been resolved.");
        }

        if (!string.IsNullOrWhiteSpace(_resolvedPrometheusEndpoint.DirectUrl))
        {
            return _resolvedPrometheusEndpoint.DirectUrl.TrimEnd('/') + _resolvedPrometheusEndpoint.PathPrefix;
        }

        if (_prometheusServicePortForward?.LocalPort is not > 0)
        {
            throw new InvalidOperationException("Prometheus port-forward is not active.");
        }

        var scheme = _resolvedPrometheusEndpoint.UseHttps ? "https" : "http";
        return $"{scheme}://localhost:{_prometheusServicePortForward.LocalPort}{_resolvedPrometheusEndpoint.PathPrefix}";
    }

    private static ClusterMetricsSettings NormalizeLegacySettings(ClusterMetricsSettings settings)
    {
        if (settings.PrometheusProviderKind == null && !string.IsNullOrWhiteSpace(settings.PrometheusDirectUrl))
        {
            settings.PrometheusProviderKind = PrometheusProviderKind.External;
        }

        if (settings.PrometheusProviderKind == null && !string.IsNullOrWhiteSpace(settings.PrometheusServerUrl))
        {
            settings.PrometheusDirectUrl = settings.PrometheusServerUrl;
            settings.PrometheusProviderKind = PrometheusProviderKind.External;
        }

        if (settings.MetricsServiceType == MetricsServiceType.Prometheus && settings.PrometheusProviderKind == null)
        {
            settings.PrometheusProviderKind = !string.IsNullOrWhiteSpace(settings.PrometheusDirectUrl)
                ? PrometheusProviderKind.External
                : PrometheusProviderKind.Manual;
        }

        return settings;
    }

    private static (DateTimeOffset Start, DateTimeOffset End, int StepSeconds) ResolveTimeRange(MetricRequest request)
    {
        var stepSeconds = request.StepSeconds ?? 60;
        var rawEnd = request.End ?? DateTimeOffset.UtcNow;
        var normalizedEnd = DateTimeOffset.FromUnixTimeSeconds(rawEnd.ToUnixTimeSeconds() / stepSeconds * stepSeconds);
        var rawStart = request.Start ?? normalizedEnd.AddSeconds(-(request.RangeSeconds ?? 3600));
        var normalizedStart = DateTimeOffset.FromUnixTimeSeconds(rawStart.ToUnixTimeSeconds() / stepSeconds * stepSeconds);
        return (normalizedStart, normalizedEnd, stepSeconds);
    }

    private static string CreateMetricRequestCacheKey(MetricRequest request, DateTimeOffset start, DateTimeOffset end, int stepSeconds)
    {
        var queryKeys = request.Queries
            .OrderBy(static x => x.Name, StringComparer.Ordinal)
            .Select(static query => $"{query.Name}:{string.Join(",", query.Options.OrderBy(static option => option.Key, StringComparer.Ordinal).Select(static option => $"{option.Key}={option.Value}"))}");

        return $"{request.Category}|{start.ToUnixTimeSeconds()}|{end.ToUnixTimeSeconds()}|{stepSeconds}|{request.Frames}|{string.Join("|", queryKeys)}";
    }

    private void HandlePrometheusQuerySuccess()
    {
        _prometheusUnavailableUntilUtc = null;
        _prometheusFailureLogged = false;
    }

    private async Task HandlePrometheusQueryFailureAsync(Exception? exception)
    {
        if (_prometheusUnavailableUntilUtc is { } unavailableUntil && unavailableUntil > DateTimeOffset.UtcNow)
        {
            return;
        }

        _prometheusUnavailableUntilUtc = DateTimeOffset.UtcNow.Add(s_prometheusFailureCooldown);

        if (!_prometheusFailureLogged)
        {
            _prometheusFailureLogged = true;
            _logger.LogWarning(exception, "Prometheus metrics are temporarily unavailable for cluster {Name}. Suppressing Prometheus requests for {CooldownSeconds} seconds.", _cluster?.Name, (int)s_prometheusFailureCooldown.TotalSeconds);
        }

        if (_configuredMetricsServiceType != MetricsServiceType.Auto
            || _cluster?.Client is not k8s.Kubernetes kube
            || ActiveMetricsBackend.Type != MetricsServiceType.Prometheus)
        {
            return;
        }

        try
        {
            _logger.LogInformation("Falling back to Kubernetes Metrics Server for cluster {Name} after Prometheus query failure.", _cluster.Name);
            await StartKubernetesMetricsAsync(_cluster, kube).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Unable to fall back to Kubernetes Metrics Server for cluster {Name}", _cluster?.Name);
        }
    }

    private static IReadOnlyList<MetricSeries> NormalizeResultSet(string metricName, PrometheusClientQueryRangeResponse response, int frames)
    {
        if (response.Data.Result.Length == 0)
        {
            return [];
        }

        return response.Data.Result
            .Select(result => new MetricSeries
            {
                Name = metricName,
                Labels = new Dictionary<string, string>(result.Metric, StringComparer.Ordinal),
                Points = NormalizeSeries(result.Values, frames),
            })
            .ToArray();
    }

    private static IReadOnlyList<MetricPoint> NormalizeSeries(IList<(DateTimeOffset Timestamp, double Value)> values, int frames)
    {
        if (values.Count == 0)
        {
            return [];
        }

        var points = values
            .OrderBy(static x => x.Timestamp)
            .Select(static x => new MetricPoint(x.Timestamp, x.Value))
            .ToList();

        if (frames <= 0 || points.Count == 0)
        {
            return points;
        }

        var normalized = new List<MetricPoint>(points);
        var cursor = points[0].Timestamp;
        var end = points[^1].Timestamp;

        while (cursor < end)
        {
            cursor = cursor.AddMinutes(1);
            if (!normalized.Any(x => x.Timestamp == cursor))
            {
                normalized.Add(new MetricPoint(cursor, 0));
            }
        }

        normalized.Sort(static (a, b) => a.Timestamp.CompareTo(b.Timestamp));

        while (normalized.Count < frames)
        {
            normalized.Insert(0, new MetricPoint(normalized[0].Timestamp.AddMinutes(-1), 0));
        }

        return normalized;
    }

    private async Task SyncKubernetesMetricsAsync(Cluster cluster, CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();

            var nodeMetricsList = await cluster.Client!.GetKubernetesNodesMetricsAsync().ConfigureAwait(false);
            NodeMetrics.Clear();
            foreach (var item in ((IEnumerable)nodeMetricsList.Items).OfType<NodeMetrics>())
            {
                NodeMetrics.Add(item);
            }

            cancellationToken.ThrowIfCancellationRequested();

            var podMetricsList = await cluster.Client!.GetKubernetesPodsMetricsAsync().ConfigureAwait(false);
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

    private async Task<bool> CanUseKubernetesMetricsServerAsync(Cluster cluster, k8s.Kubernetes kube)
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
        var apiGroups = await cluster.Client!.Apis.GetAPIVersionsAsync().ConfigureAwait(false);

        return apiGroups.Groups.Any(g => g.Name == "metrics.k8s.io")
            && podResponse.Status.Allowed
            && nodeResponse.Status.Allowed;
    }

    private sealed record CachedMetricResult(DateTimeOffset TimestampUtc, MetricResultSet Result);
}
