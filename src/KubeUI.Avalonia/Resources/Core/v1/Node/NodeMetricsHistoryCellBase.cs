using k8s.Models;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Features.Resources.Metrics.Controls;
using KubeUI.Avalonia.Infrastructure.Threading;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Resources.Core.v1.Node;

/// <summary>Provides Node metric history and allocatable limits to the shared history cell.</summary>
public abstract class NodeMetricsHistoryCellBase : MetricsHistoryCellBase<V1Node>
{
    protected NodeMetricsHistoryCellBase(IUiRefreshClock refreshClock, TimeProvider timeProvider)
        : base(refreshClock, timeProvider)
    {
    }

    protected override MetricResultSet CaptureMetricsServerHistory(
        ClusterWorkspace cluster,
        V1Node node,
        DateTimeOffset start,
        DateTimeOffset end)
    {
        List<MetricPoint> cpu = [];
        List<MetricPoint> memory = [];
        foreach (var metric in cluster.Runtime.NodeMetrics)
        {
            if (!string.Equals(metric.Name(), node.Name(), StringComparison.Ordinal)
                || !metric.Timestamp.HasValue)
            {
                continue;
            }

            var timestamp = new DateTimeOffset(DateTime.SpecifyKind(metric.Timestamp.Value, DateTimeKind.Utc));
            if (timestamp < start || timestamp > end)
            {
                continue;
            }

            if (metric.Usage?.TryGetValue("cpu", out var cpuQuantity) == true)
            {
                cpu.Add(new MetricPoint(timestamp, (double)cpuQuantity.ToDecimal()));
            }

            if (metric.Usage?.TryGetValue("memory", out var memoryQuantity) == true)
            {
                memory.Add(new MetricPoint(timestamp, memoryQuantity.ToInt64()));
            }
        }

        return new MetricResultSet
        {
            Metrics = new Dictionary<string, IReadOnlyList<MetricSeries>>(StringComparer.Ordinal)
            {
                ["cpuUsage"] =
                [
                    new MetricSeries
                    {
                        Name = "cpuUsage",
                        Labels = new Dictionary<string, string>(StringComparer.Ordinal) { ["node"] = node.Name() },
                        Points = cpu,
                    },
                ],
                ["memoryUsage"] =
                [
                    new MetricSeries
                    {
                        Name = "memoryUsage",
                        Labels = new Dictionary<string, string>(StringComparer.Ordinal) { ["node"] = node.Name() },
                        Points = memory,
                    },
                ],
            },
        };
    }

    protected override MetricRequest CreatePrometheusRequest(V1Node node, DateTimeOffset end)
    {
        Dictionary<string, string> options = new(StringComparer.Ordinal)
        {
            ["nodes"] = System.Text.RegularExpressions.Regex.Escape(node.Name() ?? string.Empty),
        };
        return CreateMetricRequest(MetricCategory.Nodes, options, end);
    }

    protected override double? GetMetricLimit(V1Node node)
    {
        var key = IsMemoryMetric ? "memory" : "cpu";
        ResourceQuantity? quantity = null;
        if (node.Status?.Allocatable?.TryGetValue(key, out var allocatable) == true)
        {
            quantity = allocatable;
        }
        else if (node.Status?.Capacity?.TryGetValue(key, out var capacity) == true)
        {
            quantity = capacity;
        }

        if (quantity == null)
        {
            return null;
        }

        var limit = IsMemoryMetric ? quantity.ToInt64() : (double)quantity.ToDecimal();
        return limit > 0 ? limit : null;
    }

    protected override bool MatchesSeries(V1Node node, MetricSeries series)
    {
        return series.Labels.TryGetValue("node", out var name)
            && string.Equals(name, node.Name(), StringComparison.Ordinal);
    }
}
