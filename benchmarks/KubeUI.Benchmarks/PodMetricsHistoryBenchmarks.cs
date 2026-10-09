using BenchmarkDotNet.Attributes;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Metrics.Controls;
using KubeUI.Avalonia.Resources.Workloads.v1.Pod;
using KubeUI.Kubernetes;

namespace KubeUI.Benchmarks;

[MemoryDiagnoser]
[BenchmarkCategory("ResourceList", "Metrics")]
public class PodMetricsHistoryBenchmarks
{
    private MetricResultSet _result = null!;
    private V1Pod[] _cells = null!;
    private PodPrometheusHistoryIndex _index = null!;

    [Params(100, 500, 1_000)]
    public int PodCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        DateTimeOffset end = DateTimeOffset.UtcNow;
        _cells = new V1Pod[PodCount * 2];
        for (var podIndex = 0; podIndex < PodCount; podIndex++)
        {
            V1Pod pod = new()
            {
                Metadata = new V1ObjectMeta
                {
                    Name = $"pod-{podIndex:D4}",
                    NamespaceProperty = "benchmark",
                },
            };
            _cells[podIndex * 2] = pod;
            _cells[(podIndex * 2) + 1] = pod;
        }

        _result = CreateResult(PodCount, end);
        _index = new PodPrometheusHistoryIndex();
    }

    [IterationSetup(Target = nameof(BuildOnceAndReadIndexedHistory))]
    public void ResetIndex()
    {
        _index = new PodPrometheusHistoryIndex();
    }

    [IterationSetup(Target = nameof(ReadWarmedIndexForRecycledCells))]
    public void WarmIndex()
    {
        _index = new PodPrometheusHistoryIndex();
        _index.GetHistoryAsync(
            _result,
            _cells[0].Namespace(),
            _cells[0].Name(),
            CancellationToken.None).GetAwaiter().GetResult();
    }

    [Benchmark(Baseline = true)]
    public async Task<int> ReaggregateNamespaceForEachRecycledCell()
    {
        var histories = await Task.WhenAll(_cells.Select(pod => MetricsHistoryAggregator.AggregateAsync(
            _result,
            pod,
            MatchesPod,
            CancellationToken.None)));
        return CountPoints(histories);
    }

    [Benchmark]
    public async Task<int> BuildOnceAndReadIndexedHistory()
    {
        var histories = await Task.WhenAll(_cells.Select(pod => _index.GetHistoryAsync(
            _result,
            pod.Namespace(),
            pod.Name(),
            CancellationToken.None)));
        return CountPoints(histories);
    }

    [Benchmark]
    public async Task<int> ReadWarmedIndexForRecycledCells()
    {
        var histories = await Task.WhenAll(_cells.Select(pod => _index.GetHistoryAsync(
            _result,
            pod.Namespace(),
            pod.Name(),
            CancellationToken.None)));
        return CountPoints(histories);
    }

    private static int CountPoints(IEnumerable<MetricHistoryData> histories)
    {
        return histories.Sum(static history => history.Cpu.Count + history.Memory.Count);
    }

    private static bool MatchesPod(V1Pod pod, MetricSeries series)
    {
        return series.Labels.TryGetValue("pod", out var podName)
            && string.Equals(podName, pod.Name(), StringComparison.Ordinal)
            && series.Labels.TryGetValue("namespace", out var namespaceName)
            && string.Equals(namespaceName, pod.Namespace(), StringComparison.Ordinal);
    }

    private static MetricResultSet CreateResult(int podCount, DateTimeOffset end)
    {
        List<MetricSeries> cpu = new(podCount);
        List<MetricSeries> memory = new(podCount);
        for (var podIndex = 0; podIndex < podCount; podIndex++)
        {
            var podName = $"pod-{podIndex:D4}";
            Dictionary<string, string> labels = new(StringComparer.Ordinal)
            {
                ["pod"] = podName,
                ["namespace"] = "benchmark",
            };
            MetricPoint[] cpuPoints = new MetricPoint[60];
            MetricPoint[] memoryPoints = new MetricPoint[60];
            for (var pointIndex = 0; pointIndex < 60; pointIndex++)
            {
                var timestamp = end.AddMinutes(pointIndex - 60);
                cpuPoints[pointIndex] = new MetricPoint(timestamp, (podIndex % 10 + 1) / 100d);
                memoryPoints[pointIndex] = new MetricPoint(timestamp, (podIndex % 10 + 1) * 1024d);
            }

            cpu.Add(new MetricSeries { Name = "cpuUsage", Labels = labels, Points = cpuPoints });
            memory.Add(new MetricSeries { Name = "memoryUsage", Labels = labels, Points = memoryPoints });
        }

        return new MetricResultSet
        {
            Metrics = new Dictionary<string, IReadOnlyList<MetricSeries>>(StringComparer.Ordinal)
            {
                ["cpuUsage"] = cpu,
                ["memoryUsage"] = memory,
            },
        };
    }
}
