using Microsoft.Extensions.Http.Resilience;
using Polly;

namespace KubeUI.Kubernetes;

internal sealed class PrometheusMetricQueries(
    ILogger logger,
    IPrometheusQueryClient queryClient,
    TimeProvider timeProvider)
{
    private static readonly TimeSpan s_retryBaseDelay = TimeSpan.FromSeconds(1);
    private static readonly TimeSpan s_requestTimeout = TimeSpan.FromSeconds(15);
    private static readonly TimeSpan s_cacheDuration = TimeSpan.FromMinutes(1);
    private readonly ResiliencePipeline<HttpResponseMessage> _pipeline = new ResiliencePipelineBuilder<HttpResponseMessage>
    {
        Name = nameof(PrometheusMetricQueries),
        InstanceName = "PrometheusQueryRange",
    }
    .AddRetry(new HttpRetryStrategyOptions
    {
        MaxRetryAttempts = 3,
        Delay = s_retryBaseDelay,
        BackoffType = DelayBackoffType.Exponential,
        UseJitter = true,
    })
    .AddTimeout(new Polly.Timeout.TimeoutStrategyOptions { Timeout = s_requestTimeout })
    .Build();
    private readonly Lock _sync = new();
    private readonly Dictionary<string, CachedResult> _cache = new(StringComparer.Ordinal);
    private readonly Dictionary<string, Task<MetricResultSet>> _inflight = new(StringComparer.Ordinal);

    public async Task<MetricResultSet> RequestAsync(
        Cluster cluster,
        ResolvedPrometheusEndpoint endpoint,
        IPrometheusProvider provider,
        MetricRequest request,
        CancellationToken lifecycleToken,
        CancellationToken cancellationToken)
    {
        var (start, end, stepSeconds) = ResolveTimeRange(request);
        var key = CreateCacheKey(request, start, end, stepSeconds);
        Task<MetricResultSet>? task;
        var ownsRequest = false;
        lock (_sync)
        {
            if (_cache.TryGetValue(key, out var cached) && timeProvider.GetUtcNow() - cached.Timestamp < s_cacheDuration)
                return cached.Result;
            if (!_inflight.TryGetValue(key, out task))
            {
                task = LoadAsync(cluster, endpoint, provider, request, start, end, stepSeconds, lifecycleToken, cancellationToken);
                _inflight[key] = task;
                ownsRequest = true;
            }
        }

        if (!ownsRequest) return await task!.ConfigureAwait(false);

        try
        {
            var result = await task.ConfigureAwait(false);
            if (!result.HadRequestFailures)
            {
                lock (_sync) _cache[key] = new CachedResult(timeProvider.GetUtcNow(), result);
            }
            return result;
        }
        finally
        {
            lock (_sync) _inflight.Remove(key);
        }
    }

    public async Task StopAsync()
    {
        Task[] requests;
        lock (_sync) requests = _inflight.Values.Distinct().ToArray();
        if (requests.Length > 0)
        {
            try { await Task.WhenAll(requests).ConfigureAwait(false); }
            catch (OperationCanceledException) { }
            catch (Exception ex) { logger.LogDebug(ex, "Ignoring in-flight Prometheus request failure while stopping metrics."); }
        }
        await queryClient.ResetAsync().ConfigureAwait(false);
        lock (_sync)
        {
            _cache.Clear();
            _inflight.Clear();
        }
    }

    private async Task<MetricResultSet> LoadAsync(Cluster cluster, ResolvedPrometheusEndpoint endpoint, IPrometheusProvider provider, MetricRequest request, DateTimeOffset start, DateTimeOffset end, int stepSeconds, CancellationToken lifecycleToken, CancellationToken cancellationToken)
    {
        using var linked = CancellationTokenSource.CreateLinkedTokenSource(lifecycleToken, cancellationToken);
        var loaded = await Task.WhenAll(request.Queries.Select(query => LoadSeriesAsync(cluster, endpoint, provider, query, request.Category, start, end, stepSeconds, request.Frames, linked.Token))).ConfigureAwait(false);
        return new MetricResultSet
        {
            Metrics = loaded.ToDictionary(static item => item.Name, static item => item.Series, StringComparer.Ordinal),
            HadRequestFailures = loaded.Any(static item => item.Failed),
        };
    }

    private async Task<(string Name, IReadOnlyList<MetricSeries> Series, bool Failed)> LoadSeriesAsync(Cluster cluster, ResolvedPrometheusEndpoint endpoint, IPrometheusProvider provider, MetricQueryDefinition definition, MetricCategory category, DateTimeOffset start, DateTimeOffset end, int stepSeconds, int frames, CancellationToken cancellationToken)
    {
        var query = provider.BuildQuery(category, definition.Name, definition.Options);
        logger.LogDebug("Requesting Prometheus metric {Metric} for cluster {Cluster} over {Start:u} to {End:u} at {StepSeconds}s step.", definition.Name, cluster.Name, start, end, stepSeconds);
        var response = await ExecuteAsync(cluster, endpoint, provider, query, start, end, stepSeconds, cancellationToken).ConfigureAwait(false);
        if (response == null || !string.Equals(response.Status, "success", StringComparison.Ordinal))
        {
            if (response != null) logger.LogWarning("Prometheus returned status {Status} for metric {Metric} on cluster {Cluster}. Error type: {ErrorType}. Error: {Error}", response.Status, definition.Name, cluster.Name, response.ErrorType, response.Error);
            return (definition.Name, [], true);
        }
        var series = NormalizeResultSet(definition.Name, response, frames, stepSeconds);
        if (series.Count == 0) logger.LogDebug("Prometheus returned no series for metric {Metric} on cluster {Cluster}.", definition.Name, cluster.Name);
        return (definition.Name, series, false);
    }

    private async Task<PrometheusClientQueryRangeResponse?> ExecuteAsync(Cluster cluster, ResolvedPrometheusEndpoint endpoint, IPrometheusProvider provider, string query, DateTimeOffset start, DateTimeOffset end, int stepSeconds, CancellationToken cancellationToken)
    {
        try
        {
            PrometheusClientQueryRangeResponse? response = null;
            _ = await _pipeline.ExecuteAsync(async token =>
            {
                response = await queryClient.QueryRangeAsync(cluster, endpoint, query, start, end, stepSeconds, token).ConfigureAwait(false)
                    ?? throw new HttpRequestException("Prometheus query returned no response.");
                return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
            }, cancellationToken).ConfigureAwait(false);
            return response;
        }
        catch (OperationCanceledException ex) when (!cancellationToken.IsCancellationRequested)
        {
            logger.LogWarning(ex, "Prometheus query failed for cluster {Cluster} using provider {Provider} after resilience retries.", cluster.Name, provider.Kind);
        }
        catch (Exception ex) when (ex is not OperationCanceledException)
        {
            logger.LogWarning(ex, "Prometheus query failed for cluster {Cluster} using provider {Provider} after resilience retries.", cluster.Name, provider.Kind);
        }
        return null;
    }

    private (DateTimeOffset Start, DateTimeOffset End, int StepSeconds) ResolveTimeRange(MetricRequest request)
    {
        var step = request.StepSeconds ?? 60;
        var rawEnd = request.End ?? timeProvider.GetUtcNow();
        var end = DateTimeOffset.FromUnixTimeSeconds(rawEnd.ToUnixTimeSeconds() / step * step);
        var rawStart = request.Start ?? end.AddSeconds(-(request.RangeSeconds ?? 3600));
        var start = DateTimeOffset.FromUnixTimeSeconds(rawStart.ToUnixTimeSeconds() / step * step);
        return (start, end, step);
    }

    private static string CreateCacheKey(MetricRequest request, DateTimeOffset start, DateTimeOffset end, int step)
    {
        var queries = request.Queries.OrderBy(static query => query.Name, StringComparer.Ordinal)
            .Select(static query => $"{query.Name}:{string.Join(",", query.Options.OrderBy(static option => option.Key, StringComparer.Ordinal).Select(static option => $"{option.Key}={option.Value}"))}");
        return $"{request.Category}|{start.ToUnixTimeSeconds()}|{end.ToUnixTimeSeconds()}|{step}|{request.Frames}|{string.Join("|", queries)}";
    }

    private static IReadOnlyList<MetricSeries> NormalizeResultSet(string name, PrometheusClientQueryRangeResponse response, int frames, int step)
    {
        if (response.Data.Result.Length == 0) return [];
        return response.Data.Result.Select(result => new MetricSeries
        {
            Name = name,
            Labels = new Dictionary<string, string>(result.Metric, StringComparer.Ordinal),
            Points = NormalizeSeries(result.Values, frames, step),
        }).ToArray();
    }

    private static IReadOnlyList<MetricPoint> NormalizeSeries(IList<(DateTimeOffset Timestamp, double Value)> values, int frames, int step)
    {
        if (values.Count == 0) return [];
        var points = values.OrderBy(static item => item.Timestamp).Select(static item => new MetricPoint(item.Timestamp, item.Value)).ToList();
        if (frames <= 0) return points;
        var normalized = new List<MetricPoint>(points);
        var cursor = points[0].Timestamp;
        var end = points[^1].Timestamp;
        while (cursor < end)
        {
            cursor = cursor.AddSeconds(step);
            if (!normalized.Any(point => point.Timestamp == cursor)) normalized.Add(new MetricPoint(cursor, 0));
        }
        normalized.Sort(static (left, right) => left.Timestamp.CompareTo(right.Timestamp));
        while (normalized.Count < frames) normalized.Insert(0, new MetricPoint(normalized[0].Timestamp.AddSeconds(-step), 0));
        return normalized;
    }

    private sealed record CachedResult(DateTimeOffset Timestamp, MetricResultSet Result);
}
