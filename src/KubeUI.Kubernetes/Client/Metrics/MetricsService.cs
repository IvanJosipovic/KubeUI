using k8s;
using k8s.Models;
using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace KubeUI.Kubernetes;

public sealed partial class MetricsService : ObservableObject, IMetricsService, IDisposable
{
    private static readonly TimeSpan s_prometheusRetryBaseDelay = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan s_prometheusRequestTimeout = TimeSpan.FromSeconds(5);
    private static readonly TimeSpan s_prometheusCacheDuration = TimeSpan.FromSeconds(30);
    private static readonly TimeSpan s_prometheusFailureCooldown = TimeSpan.FromMinutes(1);
    private static readonly TimeSpan s_kubernetesMetricsRetention = TimeSpan.FromHours(1);

    private readonly ILogger<MetricsService> _logger;
    private readonly IClusterSettingsStore _settings;
    private readonly IReadOnlyDictionary<PrometheusProviderKind, IPrometheusProvider> _prometheusProviders;
    private readonly IPrometheusQueryClient _prometheusQueryClient;
    private readonly ResiliencePipeline<HttpResponseMessage> _prometheusQueryPipeline;
    private readonly Lock _metricsRequestSync = new();
    private Cluster? _cluster;
    private CancellationTokenSource? _lifecycleCancellationTokenSource;
    private PeriodicTimer? _metricsRefreshTimer;
    private CancellationTokenSource? _metricsRefreshCancellationTokenSource;
    private Task? _metricsRefreshTask;
    private ResolvedPrometheusEndpoint? _resolvedPrometheusEndpoint;
    private IPrometheusProvider? _resolvedPrometheusProvider;
    private DateTimeOffset? _prometheusUnavailableUntilUtc;
    private bool _prometheusFailureLogged;
    private Dictionary<string, CachedMetricResult> _metricResultCache = new(StringComparer.Ordinal);
    private Dictionary<string, Task<MetricResultSet>> _inflightMetricRequests = new(StringComparer.Ordinal);

    public MetricsService(ILogger<MetricsService> logger, IClusterSettingsStore settings, IEnumerable<IPrometheusProvider> prometheusProviders, IPrometheusQueryClient prometheusQueryClient)
    {
        _logger = logger;
        _settings = settings;
        _prometheusProviders = prometheusProviders.ToDictionary(static x => x.Kind);
        _prometheusQueryClient = prometheusQueryClient;
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
        _lifecycleCancellationTokenSource = new CancellationTokenSource();

        var kube = cluster.Client as k8s.Kubernetes;
        if (kube == null)
        {
            _logger.LogDebug("Skipping metrics initialization for cluster {Name} because the Kubernetes client is not available.", cluster.Name);
            return;
        }

        var settings = NormalizeLegacySettings(_settings.GetClusterMetricsSettings(cluster));
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
            await StartKubernetesMetricsAsync(cluster, kube).ConfigureAwait(false);
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

    public async Task StopAsync()
    {
        if (_cluster != null)
        {
            _logger.LogDebug("Stopping metrics service for cluster {Name}.", _cluster.Name);
        }

        _lifecycleCancellationTokenSource?.Cancel();
        _lifecycleCancellationTokenSource?.Dispose();
        _lifecycleCancellationTokenSource = null;

        var refreshCancellation = _metricsRefreshCancellationTokenSource;
        refreshCancellation?.Cancel();
        _metricsRefreshCancellationTokenSource = null;

        var refreshTimer = _metricsRefreshTimer;
        _metricsRefreshTimer = null;
        refreshTimer?.Dispose();

        if (_metricsRefreshTask is { } refreshTask)
        {
            try
            {
                await refreshTask.ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (refreshCancellation?.IsCancellationRequested == true)
            {
            }
            finally
            {
                _metricsRefreshTask = null;
                refreshCancellation?.Dispose();
            }
        }
        else
        {
            refreshCancellation?.Dispose();
        }

        Task[] inflightRequests;
        lock (_metricsRequestSync)
        {
            inflightRequests = _inflightMetricRequests.Values.Distinct().ToArray();
        }

        if (inflightRequests.Length > 0)
        {
            try
            {
                await Task.WhenAll(inflightRequests).ConfigureAwait(false);
            }
            catch (OperationCanceledException)
            {
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Ignoring in-flight Prometheus request failure while stopping metrics.");
            }
        }

        await _prometheusQueryClient.ResetAsync().ConfigureAwait(false);

        _resolvedPrometheusEndpoint = null;
        _resolvedPrometheusProvider = null;

        PodMetrics.Clear();
        NodeMetrics.Clear();
        IsMetricsAvailable = false;
        ActiveMetricsBackend = ActiveMetricsBackend.None;
        _prometheusUnavailableUntilUtc = null;
        _prometheusFailureLogged = false;
        lock (_metricsRequestSync)
        {
            _metricResultCache.Clear();
            _inflightMetricRequests.Clear();
        }
        _cluster = null;
    }

    public async Task<MetricResultSet> RequestMetricsAsync(MetricRequest request, CancellationToken cancellationToken = default)
    {
        if (_cluster == null
            || _resolvedPrometheusProvider == null
            || _resolvedPrometheusEndpoint == null
            || ActiveMetricsBackend.Type != MetricsServiceType.Prometheus)
        {
            return MetricResultSet.Empty;
        }

        var (start, end, stepSeconds) = ResolveTimeRange(request);
        if (_prometheusUnavailableUntilUtc is { } unavailableUntil && unavailableUntil > DateTimeOffset.UtcNow)
        {
            _logger.LogDebug(
                "Suppressing Prometheus metrics request for cluster {Name} until {UnavailableUntil:u} because Prometheus is in cooldown.",
                _cluster.Name,
                unavailableUntil);
            return new MetricResultSet { HadRequestFailures = true };
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
        using var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(
            _lifecycleCancellationTokenSource?.Token ?? CancellationToken.None,
            cancellationToken);
        var requestCancellationToken = linkedCancellation.Token;
        var tasks = request.Queries.Select(query => LoadMetricSeriesAsync(query, request.Category, start, end, stepSeconds, request.Frames, requestCancellationToken));
        var loaded = await Task.WhenAll(tasks).ConfigureAwait(false);

        return new MetricResultSet
        {
            Metrics = loaded.ToDictionary(static x => x.Name, static x => x.Series, StringComparer.Ordinal),
            HadRequestFailures = loaded.Any(static x => x.HadRequestFailure),
        };
    }

    private async Task<(string Name, IReadOnlyList<MetricSeries> Series, bool HadRequestFailure)> LoadMetricSeriesAsync(
        MetricQueryDefinition query,
        MetricCategory category,
        DateTimeOffset start,
        DateTimeOffset end,
        int stepSeconds,
        int frames,
        CancellationToken cancellationToken)
    {
        var promQuery = _resolvedPrometheusProvider!.BuildQuery(category, query.Name, query.Options);
        _logger.LogDebug(
            "Requesting Prometheus metric {Metric} for cluster {Cluster} over {Start:u} to {End:u} at {StepSeconds}s step.",
            query.Name,
            _cluster?.Name,
            start,
            end,
            stepSeconds);
        var result = await ExecuteQueryRangeAsync(promQuery, start, end, stepSeconds, cancellationToken).ConfigureAwait(false);

        if (result == null || !string.Equals(result.Status, "success", StringComparison.Ordinal))
        {
            if (result != null)
            {
                _logger.LogWarning(
                    "Prometheus returned status {Status} for metric {Metric} on cluster {Cluster}. Error type: {ErrorType}. Error: {Error}",
                    result.Status,
                    query.Name,
                    _cluster?.Name,
                    result.ErrorType,
                    result.Error);
            }

            return (query.Name, [], true);
        }

        var series = NormalizeResultSet(query.Name, result, frames, stepSeconds);
        if (series.Count == 0)
        {
            _logger.LogDebug("Prometheus returned no series for metric {Metric} on cluster {Cluster}.", query.Name, _cluster?.Name);
        }

        return (query.Name, series, false);
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

    private async Task StartKubernetesMetricsAsync(Cluster cluster, k8s.Kubernetes kube)
    {
        _logger.LogDebug("Checking Kubernetes Metrics Server availability for cluster {Name}.", cluster.Name);
        if (!await CanUseKubernetesMetricsServerAsync(cluster, kube, _lifecycleCancellationTokenSource?.Token ?? CancellationToken.None).ConfigureAwait(false))
        {
            ActiveMetricsBackend = ActiveMetricsBackend.None;
            IsMetricsAvailable = false;
            _logger.LogInformation("Kubernetes Metrics Server is not available for cluster {Name}.", cluster.Name);
            return;
        }

        ActiveMetricsBackend = ActiveMetricsBackend.KubernetesMetricsServer;
        IsMetricsAvailable = true;
        _logger.LogInformation("Kubernetes Metrics Server metrics activated for cluster {Name}.", cluster.Name);
        var refreshCancellation = new CancellationTokenSource();
        var refreshTimer = new PeriodicTimer(TimeSpan.FromSeconds(30));
        _metricsRefreshCancellationTokenSource = refreshCancellation;
        _metricsRefreshTimer = refreshTimer;

        await SyncKubernetesMetricsAsync(cluster, refreshCancellation.Token).ConfigureAwait(false);

        _metricsRefreshTask = Task.Run(async () =>
        {
            try
            {
                while (await refreshTimer.WaitForNextTickAsync(refreshCancellation.Token).ConfigureAwait(false))
                {
                    await SyncKubernetesMetricsAsync(cluster, refreshCancellation.Token).ConfigureAwait(false);
                }
            }
            catch (OperationCanceledException) when (refreshCancellation.IsCancellationRequested)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Kubernetes Metrics Server refresh loop failed for cluster {Name}.", cluster.Name);
            }
        }, refreshCancellation.Token);
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

    private async Task<PrometheusClientQueryRangeResponse?> ExecuteQueryRangeAsync(string query, DateTimeOffset start, DateTimeOffset end, int stepSeconds, CancellationToken cancellationToken)
    {
        Exception? lastException = null;
        PrometheusClientQueryRangeResponse? queryResponse = null;

        try
        {
            using var timeoutCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeoutCts.CancelAfter(s_prometheusRequestTimeout);

            _ = await _prometheusQueryPipeline.ExecuteAsync(
                async token =>
                {
                    queryResponse = await _prometheusQueryClient.QueryRangeAsync(_cluster!, _resolvedPrometheusEndpoint!, query, start, end, stepSeconds, token).ConfigureAwait(false)
                        ?? throw new HttpRequestException("Prometheus query returned no response.");

                    return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
                },
                timeoutCts.Token).ConfigureAwait(false);

            HandlePrometheusQuerySuccess();
            return queryResponse;
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            lastException = ex;
        }
        catch (HttpRequestException ex)
        {
            lastException = ex;
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            lastException = ex;
            _logger.LogError(ex, "Unexpected error querying Prometheus for cluster {Cluster} using provider {Provider}.", _cluster?.Name, _resolvedPrometheusProvider?.Kind);
        }

        await HandlePrometheusQueryFailureAsync(lastException).ConfigureAwait(false);
        return null;
    }

    private static ClusterMetricsSettings NormalizeLegacySettings(ClusterMetricsSettings settings)
    {
        var normalized = new ClusterMetricsSettings
        {
            MetricsServiceType = settings.MetricsServiceType,
            PrometheusServerUrl = settings.PrometheusServerUrl,
            PrometheusProviderKind = settings.PrometheusProviderKind,
            PrometheusServiceName = settings.PrometheusServiceName,
            PrometheusServiceNamespace = settings.PrometheusServiceNamespace,
            PrometheusServicePort = settings.PrometheusServicePort,
            PrometheusPathPrefix = settings.PrometheusPathPrefix,
            PrometheusUseHttps = settings.PrometheusUseHttps,
            PrometheusDirectUrl = settings.PrometheusDirectUrl,
            PrometheusBearerToken = settings.PrometheusBearerToken,
            AzureMonitorWorkspaceId = settings.AzureMonitorWorkspaceId,
            AzureMonitorQueryEndpoint = settings.AzureMonitorQueryEndpoint,
        };

        if (normalized.PrometheusProviderKind == null && !string.IsNullOrWhiteSpace(normalized.PrometheusDirectUrl))
        {
            normalized.PrometheusProviderKind = PrometheusProviderKind.External;
        }

        if (normalized.PrometheusProviderKind == null && !string.IsNullOrWhiteSpace(normalized.PrometheusServerUrl))
        {
            normalized.PrometheusDirectUrl = normalized.PrometheusServerUrl;
            normalized.PrometheusProviderKind = PrometheusProviderKind.External;
        }

        if (normalized.MetricsServiceType == MetricsServiceType.Prometheus && normalized.PrometheusProviderKind == null)
        {
            normalized.PrometheusProviderKind = !string.IsNullOrWhiteSpace(normalized.PrometheusDirectUrl)
                ? PrometheusProviderKind.External
                : PrometheusProviderKind.Manual;
        }

        return normalized;
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
        if (_prometheusUnavailableUntilUtc != null)
        {
            _logger.LogInformation("Prometheus metrics recovered for cluster {Name}.", _cluster?.Name);
        }

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

    }

    private static IReadOnlyList<MetricSeries> NormalizeResultSet(string metricName, PrometheusClientQueryRangeResponse response, int frames, int stepSeconds)
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
                Points = NormalizeSeries(result.Values, frames, stepSeconds),
            })
            .ToArray();
    }

    private static IReadOnlyList<MetricPoint> NormalizeSeries(IList<(DateTimeOffset Timestamp, double Value)> values, int frames, int stepSeconds)
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
            cursor = cursor.AddSeconds(stepSeconds);
            if (!normalized.Any(x => x.Timestamp == cursor))
            {
                normalized.Add(new MetricPoint(cursor, 0));
            }
        }

        normalized.Sort(static (a, b) => a.Timestamp.CompareTo(b.Timestamp));

        while (normalized.Count < frames)
        {
            normalized.Insert(0, new MetricPoint(normalized[0].Timestamp.AddSeconds(-stepSeconds), 0));
        }

        return normalized;
    }

    internal async Task SyncKubernetesMetricsAsync(Cluster cluster, CancellationToken cancellationToken)
    {
        try
        {
            cancellationToken.ThrowIfCancellationRequested();
            PruneExpiredMetrics(NodeMetrics, static metric => metric.Timestamp);
            PruneExpiredMetrics(PodMetrics, static metric => metric.Timestamp);

            var nodeMetricsList = await GetMetricsAsync<NodeMetricsList>(cluster, "/apis/metrics.k8s.io/v1beta1/nodes", cancellationToken).ConfigureAwait(false);
            AppendRecentMetrics(
                NodeMetrics,
                nodeMetricsList.Items.OfType<NodeMetrics>(),
                static metric => metric.Name() ?? string.Empty,
                static metric => metric.Timestamp);

            cancellationToken.ThrowIfCancellationRequested();

            var podMetricsList = await GetMetricsAsync<PodMetricsList>(cluster, "/apis/metrics.k8s.io/v1beta1/pods", cancellationToken).ConfigureAwait(false);
            AppendRecentMetrics(
                PodMetrics,
                podMetricsList.Items.OfType<PodMetrics>(),
                static metric => string.Concat(metric.Namespace(), "/", metric.Name()),
                static metric => metric.Timestamp);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating Kubernetes metrics");
        }
    }

    private static async Task<TMetricsList> GetMetricsAsync<TMetricsList>(Cluster cluster, string path, CancellationToken cancellationToken)
    {
        using var request = new HttpRequestMessage(HttpMethod.Get, new Uri(((k8s.Kubernetes)cluster.Client!).BaseUri, path));
        using var response = await ((k8s.Kubernetes)cluster.Client!).SendAuthenticatedAsync(request, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();
        var json = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        return KubernetesJson.Deserialize<TMetricsList>(json)
            ?? throw new InvalidOperationException($"Kubernetes metrics API returned an empty response for '{path}'.");
    }

    private static void PruneExpiredMetrics<TMetric>(
        ObservableCollection<TMetric> storedMetrics,
        Func<TMetric, DateTime?> getTimestamp)
    {
        var cutoffUtc = DateTime.UtcNow - s_kubernetesMetricsRetention;
        for (var i = storedMetrics.Count - 1; i >= 0; i--)
        {
            var timestamp = getTimestamp(storedMetrics[i]);
            if (!timestamp.HasValue || timestamp.Value.ToUniversalTime() < cutoffUtc)
            {
                storedMetrics.RemoveAt(i);
            }
        }
    }

    private static void AppendRecentMetrics<TMetric>(
        ObservableCollection<TMetric> storedMetrics,
        IEnumerable<TMetric> receivedMetrics,
        Func<TMetric, string> getResourceKey,
        Func<TMetric, DateTime?> getTimestamp)
    {
        var cutoffUtc = DateTime.UtcNow - s_kubernetesMetricsRetention;
        HashSet<(string ResourceKey, DateTime TimestampUtc)> knownSamples = [];

        for (var i = storedMetrics.Count - 1; i >= 0; i--)
        {
            var metric = storedMetrics[i];
            var timestampUtc = getTimestamp(metric)!.Value.ToUniversalTime();
            if (!knownSamples.Add((getResourceKey(metric), timestampUtc)))
            {
                storedMetrics.RemoveAt(i);
            }
        }

        foreach (var metric in receivedMetrics)
        {
            var timestamp = getTimestamp(metric);
            if (!timestamp.HasValue)
            {
                continue;
            }

            var timestampUtc = timestamp.Value.ToUniversalTime();
            if (timestampUtc >= cutoffUtc && knownSamples.Add((getResourceKey(metric), timestampUtc)))
            {
                storedMetrics.Add(metric);
            }
        }
    }

    public void Dispose()
    {
        StopAsync().GetAwaiter().GetResult();
    }

    private async Task<bool> CanUseKubernetesMetricsServerAsync(Cluster cluster, k8s.Kubernetes kube, CancellationToken cancellationToken)
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

        var podResponse = await kube.CreateSelfSubjectAccessReviewAsync(podReview, cancellationToken: cancellationToken).ConfigureAwait(false);
        var nodeResponse = await kube.CreateSelfSubjectAccessReviewAsync(nodeReview, cancellationToken: cancellationToken).ConfigureAwait(false);
        var apiGroups = await cluster.Client!.Apis.GetAPIVersionsAsync(cancellationToken).ConfigureAwait(false);
        var apiGroupAvailable = apiGroups.Groups.Any(g => g.Name == "metrics.k8s.io");
        var allowed = apiGroupAvailable
            && podResponse.Status.Allowed
            && nodeResponse.Status.Allowed;

        _logger.LogDebug(
            "Kubernetes Metrics Server detection for cluster {Name}: apiGroupAvailable={ApiGroupAvailable}, podListAllowed={PodAllowed}, nodeListAllowed={NodeAllowed}, result={Result}.",
            cluster.Name,
            apiGroupAvailable,
            podResponse.Status.Allowed,
            nodeResponse.Status.Allowed,
            allowed);

        return allowed;
    }

    private static string DescribeEndpoint(ResolvedPrometheusEndpoint endpoint)
    {
        if (!string.IsNullOrWhiteSpace(endpoint.DirectUrl))
        {
            return $"directUrl={endpoint.DirectUrl}, useHttps={endpoint.UseHttps}, pathPrefix={endpoint.PathPrefix}";
        }

        return $"namespace={endpoint.Namespace}, service={endpoint.ServiceName}, port={endpoint.ServicePort}, useHttps={endpoint.UseHttps}, pathPrefix={endpoint.PathPrefix}";
    }

    private sealed record CachedMetricResult(DateTimeOffset TimestampUtc, MetricResultSet Result);
}
