using k8s.Models;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Features.Resources.Metrics.Controls;
using KubeUI.Avalonia.Infrastructure.Threading;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Pod;

/// <summary>Provides Pod metric history and summed container limits to the shared history cell.</summary>
public abstract class PodMetricsHistoryCellBase : MetricsHistoryCellBase<V1Pod>
{
    protected PodMetricsHistoryCellBase(IUiRefreshClock refreshClock, TimeProvider timeProvider)
        : base(refreshClock, timeProvider)
    {
    }

    protected override MetricResultSet CaptureMetricsServerHistory(
        ClusterWorkspace cluster,
        V1Pod pod,
        DateTimeOffset start,
        DateTimeOffset end)
    {
        Dictionary<DateTimeOffset, (double Cpu, double Memory)> samples = [];
        foreach (var metric in cluster.Runtime.PodMetrics)
        {
            if (!string.Equals(metric.Name(), pod.Name(), StringComparison.Ordinal)
                || !string.Equals(metric.Namespace(), pod.Namespace(), StringComparison.Ordinal)
                || !metric.Timestamp.HasValue)
            {
                continue;
            }

            var timestamp = new DateTimeOffset(DateTime.SpecifyKind(metric.Timestamp.Value, DateTimeKind.Utc));
            if (timestamp < start || timestamp > end)
            {
                continue;
            }

            double cpu = 0;
            double memory = 0;
            if (metric.Containers != null)
            {
                foreach (var container in metric.Containers)
                {
                    if (container.Usage?.TryGetValue("cpu", out var cpuQuantity) == true)
                    {
                        cpu += (double)cpuQuantity.ToDecimal();
                    }

                    if (container.Usage?.TryGetValue("memory", out var memoryQuantity) == true)
                    {
                        memory += memoryQuantity.ToInt64();
                    }
                }
            }

            samples[timestamp] = (cpu, memory);
        }

        return CreateMetricsResult(
            samples.Select(static sample => new MetricPoint(sample.Key, sample.Value.Cpu)).ToArray(),
            samples.Select(static sample => new MetricPoint(sample.Key, sample.Value.Memory)).ToArray());
    }

    protected override MetricRequest CreatePrometheusRequest(V1Pod pod, DateTimeOffset end)
    {
        Dictionary<string, string> options = new(StringComparer.Ordinal)
        {
            ["namespace"] = pod.Namespace() ?? string.Empty,
            ["pods"] = System.Text.RegularExpressions.Regex.Escape(pod.Name() ?? string.Empty),
            ["selector"] = "pod, namespace",
        };
        return CreateMetricRequest(MetricCategory.Pods, options, end);
    }

    protected override double? GetMetricLimit(V1Pod pod)
    {
        double total = 0;
        var found = false;
        var key = IsMemoryMetric ? "memory" : "cpu";
        foreach (var container in pod.Spec?.Containers ?? [])
        {
            if (container.Resources?.Limits?.TryGetValue(key, out var quantity) != true || quantity == null)
            {
                continue;
            }

            total += IsMemoryMetric ? quantity.ToInt64() : (double)quantity.ToDecimal();
            found = true;
        }

        return found && total > 0 ? total : null;
    }

    private static MetricResultSet CreateMetricsResult(
        IReadOnlyList<MetricPoint> cpu,
        IReadOnlyList<MetricPoint> memory)
    {
        return new MetricResultSet
        {
            Metrics = new Dictionary<string, IReadOnlyList<MetricSeries>>(StringComparer.Ordinal)
            {
                ["cpuUsage"] = [new MetricSeries { Name = "cpuUsage", Points = cpu }],
                ["memoryUsage"] = [new MetricSeries { Name = "memoryUsage", Points = memory }],
            },
        };
    }
}
