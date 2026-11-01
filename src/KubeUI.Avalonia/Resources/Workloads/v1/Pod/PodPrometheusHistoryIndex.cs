using System.Text;
using KubeUI.Avalonia.Features.Resources.Metrics.Controls;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Pod;

/// <summary>Shares indexed Prometheus history results between recycled Pod metric cells.</summary>
public sealed class PodPrometheusHistoryIndex
{
    private const int MaximumIndexedResultSets = 4;
    private readonly Lock _sync = new();
    private readonly Dictionary<string, Lazy<Task<PodMetricHistoryIndexData>>> _indexes = new(StringComparer.Ordinal);
    private readonly Queue<string> _indexOrder = new();
    private int _indexBuildCount;

    /// <summary>Creates a bounded cache for aggregated Pod Prometheus histories.</summary>
    public PodPrometheusHistoryIndex()
    {
    }

    internal int IndexBuildCount => Volatile.Read(ref _indexBuildCount);

    internal async Task<MetricHistoryData> GetHistoryAsync(
        MetricResultSet result,
        string? namespaceName,
        string? podName,
        CancellationToken cancellationToken)
    {
        ArgumentNullException.ThrowIfNull(result);

        // The cache is keyed by the content of the result rather than by its identity, so an
        // equivalent result produced by a different request still reuses the existing index.
        var cacheKey = CreateCacheKey(result);
        Lazy<Task<PodMetricHistoryIndexData>> lazyIndex;
        lock (_sync)
        {
            if (!_indexes.TryGetValue(cacheKey, out lazyIndex!))
            {
                lazyIndex = new Lazy<Task<PodMetricHistoryIndexData>>(
                    () => Task.Run(() => BuildIndex(result)),
                    LazyThreadSafetyMode.ExecutionAndPublication);
                _indexes.Add(cacheKey, lazyIndex);
                _indexOrder.Enqueue(cacheKey);
                if (_indexOrder.Count > MaximumIndexedResultSets)
                {
                    _indexes.Remove(_indexOrder.Dequeue());
                }
            }
        }

        var index = await lazyIndex.Value.WaitAsync(cancellationToken).ConfigureAwait(false);
        var key = new PodMetricHistoryKey(namespaceName ?? string.Empty, podName ?? string.Empty);
        var podHistory = index.ByPod.TryGetValue(key, out var history)
            ? history
            : MetricHistoryData.Empty;

        if (index.UnlabeledHistory.Cpu.Count == 0 && index.UnlabeledHistory.Memory.Count == 0)
        {
            return podHistory;
        }

        return Merge(index.UnlabeledHistory, podHistory);
    }

    private static string CreateCacheKey(MetricResultSet result)
    {
        var builder = new StringBuilder();
        foreach (var metric in result.Metrics.OrderBy(static metric => metric.Key, StringComparer.Ordinal))
        {
            builder.Append(metric.Key).Append('|');
            foreach (var series in metric.Value.OrderBy(static series => series.Name, StringComparer.Ordinal))
            {
                builder.Append(series.Name).Append(':');
                foreach (var label in series.Labels.OrderBy(static label => label.Key, StringComparer.Ordinal))
                {
                    builder.Append(label.Key).Append('=').Append(label.Value).Append(',');
                }

                // The point values are part of the index, so they have to be part of the key. Without
                // them a result with the same series and labels but different samples would reuse a
                // stale index and report the previous values.
                foreach (var point in series.Points)
                {
                    builder.Append(point.Timestamp.ToUnixTimeMilliseconds()).Append('=').Append(point.Value).Append(',');
                }

                builder.Append(';');
            }
        }

        return builder.ToString();
    }

    private PodMetricHistoryIndexData BuildIndex(MetricResultSet result)
    {
        Interlocked.Increment(ref _indexBuildCount);
        Dictionary<PodMetricHistoryKey, MutablePodMetricHistory> byPod = [];
        MutablePodMetricHistory unlabeled = new();
        AddSeries(result, "cpuUsage", isMemory: false, byPod, unlabeled);
        AddSeries(result, "memoryUsage", isMemory: true, byPod, unlabeled);

        Dictionary<PodMetricHistoryKey, MetricHistoryData> histories = new(byPod.Count);
        foreach (var (key, history) in byPod)
        {
            histories.Add(key, history.ToHistoryData());
        }

        return new PodMetricHistoryIndexData(histories, unlabeled.ToHistoryData());
    }

    private static void AddSeries(
        MetricResultSet result,
        string metricName,
        bool isMemory,
        Dictionary<PodMetricHistoryKey, MutablePodMetricHistory> byPod,
        MutablePodMetricHistory unlabeled)
    {
        if (!result.Metrics.TryGetValue(metricName, out var series))
        {
            return;
        }

        foreach (var metricSeries in series)
        {
            var hasPod = metricSeries.Labels.TryGetValue("pod", out var podName);
            var hasNamespace = metricSeries.Labels.TryGetValue("namespace", out var namespaceName);
            if (hasPod != hasNamespace)
            {
                continue;
            }

            MutablePodMetricHistory history;
            if (hasPod)
            {
                var key = new PodMetricHistoryKey(namespaceName!, podName!);
                if (byPod.TryGetValue(key, out var existing))
                {
                    history = existing;
                }
                else
                {
                    history = new MutablePodMetricHistory();
                    byPod.Add(key, history);
                }
            }
            else
            {
                history = unlabeled;
            }

            var values = isMemory ? history.Memory : history.Cpu;
            foreach (var point in metricSeries.Points)
            {
                values.TryGetValue(point.Timestamp, out var total);
                values[point.Timestamp] = total + point.Value;
            }
        }
    }

    private static MetricHistoryData Merge(MetricHistoryData shared, MetricHistoryData pod)
    {
        return new MetricHistoryData(
            MergePoints(shared.Cpu, pod.Cpu),
            MergePoints(shared.Memory, pod.Memory));
    }

    private static IReadOnlyList<MetricPoint> MergePoints(
        IReadOnlyList<MetricPoint> shared,
        IReadOnlyList<MetricPoint> pod)
    {
        Dictionary<DateTimeOffset, double> values = new(shared.Count + pod.Count);
        foreach (var point in shared)
        {
            values[point.Timestamp] = point.Value;
        }

        foreach (var point in pod)
        {
            values.TryGetValue(point.Timestamp, out var total);
            values[point.Timestamp] = total + point.Value;
        }

        return values
            .OrderBy(static item => item.Key)
            .Select(static item => new MetricPoint(item.Key, item.Value))
            .ToArray();
    }

    private sealed record PodMetricHistoryIndexData(
        IReadOnlyDictionary<PodMetricHistoryKey, MetricHistoryData> ByPod,
        MetricHistoryData UnlabeledHistory);

    private readonly record struct PodMetricHistoryKey(string NamespaceName, string PodName);

    private sealed class MutablePodMetricHistory
    {
        public Dictionary<DateTimeOffset, double> Cpu { get; } = [];

        public Dictionary<DateTimeOffset, double> Memory { get; } = [];

        public MetricHistoryData ToHistoryData()
        {
            return new MetricHistoryData(ToPoints(Cpu), ToPoints(Memory));
        }

        private static IReadOnlyList<MetricPoint> ToPoints(Dictionary<DateTimeOffset, double> values)
        {
            return values
                .OrderBy(static item => item.Key)
                .Select(static item => new MetricPoint(item.Key, item.Value))
                .ToArray();
        }
    }
}
