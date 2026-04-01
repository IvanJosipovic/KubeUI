using KubeUI.Avalonia.Features.Clusters.Overview.ViewModels;
using KubeUI.Avalonia.Features.Clusters.Workspace.ViewModels;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;
using Humanizer;
using k8s;
using k8s.Models;
using LiveChartsCore.Defaults;
using Microsoft.Extensions.Logging;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;

namespace KubeUI.Avalonia.Features.Clusters.Overview.ViewModels;

internal static class ClusterOverviewBackend
{
    public static async Task<ClusterOverviewSnapshot> LoadClusterOverviewSnapshotAsync(
        ClusterWorkspaceViewModel cluster,
        ClusterScope selectedScope,
        MetricTabViewModel? selectedMode,
        MetricTimeRangeOption? selectedTimeRange,
        ILogger logger)
    {
        List<V1Node> nodes = GetClusterScopeNodes(cluster, selectedScope);
        HashSet<string> nodeNames = nodes
            .Select(static node => node.Name())
            .Where(static name => !string.IsNullOrWhiteSpace(name))
            .ToHashSet(StringComparer.Ordinal);
        List<V1Pod> pods = GetClusterScopePods(cluster, selectedScope, nodeNames);

        decimal cpuUsage = 0;
        decimal cpuRequests = 0;
        decimal cpuLimits = 0;
        decimal cpuCapacity = 0;
        decimal cpuAllocatable = 0;
        long memoryUsageBytes = 0;
        long memoryRequestsBytes = 0;
        long memoryLimitsBytes = 0;
        long memoryCapacityBytes = 0;
        long memoryAllocatableBytes = 0;

        foreach (var metric in cluster.NodeMetrics.Where(metric => nodeNames.Count == 0 || nodeNames.Contains(metric.Name())))
        {
            if (metric.Usage.TryGetValue("cpu", out var cpuUsageQuantity))
            {
                cpuUsage += cpuUsageQuantity.ToDecimal();
            }

            if (metric.Usage.TryGetValue("memory", out var memoryUsageQuantity))
            {
                memoryUsageBytes += memoryUsageQuantity.ToInt64();
            }
        }

        foreach (var node in nodes)
        {
            if (node.Status?.Capacity?.TryGetValue("cpu", out var nodeCpuCapacity) == true)
            {
                cpuCapacity += nodeCpuCapacity.ToDecimal();
            }

            if (node.Status?.Allocatable?.TryGetValue("cpu", out var nodeCpuAllocatable) == true)
            {
                cpuAllocatable += nodeCpuAllocatable.ToDecimal();
            }

            if (node.Status?.Capacity?.TryGetValue("memory", out var nodeMemoryCapacity) == true)
            {
                memoryCapacityBytes += nodeMemoryCapacity.ToInt64();
            }

            if (node.Status?.Allocatable?.TryGetValue("memory", out var nodeMemoryAllocatable) == true)
            {
                memoryAllocatableBytes += nodeMemoryAllocatable.ToInt64();
            }
        }

        foreach (var container in pods.SelectMany(static pod => pod.Spec?.Containers ?? []))
        {
            if (container.Resources?.Requests?.TryGetValue("cpu", out var cpuRequest) == true)
            {
                cpuRequests += cpuRequest.ToDecimal();
            }

            if (container.Resources?.Limits?.TryGetValue("cpu", out var cpuLimit) == true)
            {
                cpuLimits += cpuLimit.ToDecimal();
            }

            if (container.Resources?.Requests?.TryGetValue("memory", out var memoryRequest) == true)
            {
                memoryRequestsBytes += memoryRequest.ToInt64();
            }

            if (container.Resources?.Limits?.TryGetValue("memory", out var memoryLimit) == true)
            {
                memoryLimitsBytes += memoryLimit.ToInt64();
            }
        }

        string trendTitle = selectedMode?.Title ?? "CPU";
        IReadOnlyList<ClusterOverviewMetricSeries> trendSeries = [];
        IReadOnlyList<ClusterOverviewMetricSeries> cpuUsageSeries = [];
        IReadOnlyList<ClusterOverviewMetricSeries> memoryUsageSeries = [];
        if (cluster.MetricsServiceType == KubeUI.Kubernetes.MetricsServiceType.Prometheus)
        {
            trendSeries = trendTitle switch
            {
                "Memory" => await LoadClusterRingChartAsync(cluster, selectedScope, selectedTimeRange, "Memory Trend", ("workloadMemoryUsage", "Usage")).ConfigureAwait(false),
                "Pods" => await LoadClusterRingChartAsync(cluster, selectedScope, selectedTimeRange, "Pods Trend", ("podUsage", "Usage")).ConfigureAwait(false),
                _ => await LoadClusterRingChartAsync(cluster, selectedScope, selectedTimeRange, "CPU Trend", ("cpuUsage", "Usage")).ConfigureAwait(false),
            };

            cpuUsageSeries = await LoadClusterRingChartAsync(cluster, selectedScope, selectedTimeRange, "CPU Usage", ("cpuUsage", "Usage")).ConfigureAwait(false);
            memoryUsageSeries = await LoadClusterRingChartAsync(cluster, selectedScope, selectedTimeRange, "Memory Usage", ("workloadMemoryUsage", "Usage")).ConfigureAwait(false);
        }

        decimal effectiveCpuUsage = cpuUsage;
        long effectiveMemoryUsageBytes = memoryUsageBytes;

        if (cluster.MetricsServiceType == KubeUI.Kubernetes.MetricsServiceType.Prometheus)
        {
            double latestCpuUsage = GetLatestSeriesValue(cpuUsageSeries, "Usage");
            if (latestCpuUsage > 0)
            {
                effectiveCpuUsage = (decimal)latestCpuUsage;
            }

            double latestMemoryUsage = GetLatestSeriesValue(memoryUsageSeries, "Usage");
            if (latestMemoryUsage > 0)
            {
                effectiveMemoryUsageBytes = (long)Math.Round(latestMemoryUsage);
            }
        }

        double cpuUsageDisplay = (double)effectiveCpuUsage;
        double memoryUsageDisplay = effectiveMemoryUsageBytes;
        double cpuFrame = cpuAllocatable > 0 ? (double)cpuAllocatable : (double)cpuCapacity;
        double memoryFrame = memoryAllocatableBytes > 0 ? memoryAllocatableBytes : memoryCapacityBytes;
        decimal podCapacity = SumNodePodCapacity(nodes);
        decimal podAllocatableCapacity = SumNodePodAllocatableCapacity(nodes);
        double podFrame = podAllocatableCapacity > 0 ? (double)podAllocatableCapacity : (double)podCapacity;

        if (cluster.MetricsServiceType != KubeUI.Kubernetes.MetricsServiceType.Prometheus)
        {
            trendSeries = trendTitle switch
            {
                "Memory" => CreateClusterSeries(
                    ("Usage", (long)ClusterOverviewRingChartViewModel.ClampToFrame(memoryUsageDisplay, memoryFrame)),
                    ("Requests", memoryRequestsBytes),
                    ("Limits", memoryLimitsBytes)),
                "Pods" => CreateClusterSeries(("Usage", (decimal)ClusterOverviewRingChartViewModel.ClampToFrame((double)pods.Count, podFrame))),
                _ => CreateClusterSeries(
                    ("Usage", (decimal)ClusterOverviewRingChartViewModel.ClampToFrame(cpuUsageDisplay, cpuFrame)),
                    ("Requests", cpuRequests),
                    ("Limits", cpuLimits)),
            };
        }

        return new ClusterOverviewSnapshot(
            effectiveCpuUsage,
            cpuRequests,
            cpuLimits,
            cpuCapacity,
            cpuAllocatable,
            effectiveMemoryUsageBytes,
            memoryRequestsBytes,
            memoryLimitsBytes,
            memoryCapacityBytes,
            memoryAllocatableBytes,
            pods.Count,
            podCapacity,
            podAllocatableCapacity,
            trendSeries,
            CreateClusterSeries(
                ("Usage", (decimal)ClusterOverviewRingChartViewModel.ClampToFrame(cpuUsageDisplay, cpuFrame)),
                ("Requests", (decimal)ClusterOverviewRingChartViewModel.ClampToFrame((double)cpuRequests, cpuFrame)),
                ("Limits", (decimal)ClusterOverviewRingChartViewModel.ClampToFrame((double)cpuLimits, cpuFrame))),
            CreateClusterSeries(
                ("Usage", (long)ClusterOverviewRingChartViewModel.ClampToFrame(memoryUsageDisplay, memoryFrame)),
                ("Requests", (long)ClusterOverviewRingChartViewModel.ClampToFrame(memoryRequestsBytes, memoryFrame)),
                ("Limits", (long)ClusterOverviewRingChartViewModel.ClampToFrame(memoryLimitsBytes, memoryFrame))),
            CreateClusterSeries(
                ("Usage", (decimal)ClusterOverviewRingChartViewModel.ClampToFrame((double)pods.Count, podFrame))),
            CreateCpuLegendSeries((decimal)cpuUsageDisplay, cpuRequests, cpuLimits, cpuAllocatable, cpuCapacity),
            CreateMemoryLegendSeries(memoryUsageDisplay, memoryRequestsBytes, memoryLimitsBytes, memoryAllocatableBytes, memoryCapacityBytes),
            CreatePodLegendSeries(pods.Count, podAllocatableCapacity, podCapacity));
    }

    public static List<V1Node> GetClusterScopeNodes(ClusterWorkspaceViewModel cluster, ClusterScope scope)
    {
        List<V1Node> nodes = cluster.GetResourceList<V1Node>()?.ToList() ?? [];
        return scope switch
        {
            ClusterScope.MasterNodes => nodes.Where(static node => IsMasterNode(node)).ToList(),
            ClusterScope.WorkerNodes => nodes.Where(static node => !IsMasterNode(node)).ToList(),
            _ => nodes,
        };
    }

    public static List<V1Pod> GetClusterScopePods(ClusterWorkspaceViewModel cluster, ClusterScope scope, ISet<string> nodeNames)
    {
        List<V1Pod> pods = cluster.GetResourceList<V1Pod>()?.ToList() ?? [];
        if (scope == ClusterScope.All || nodeNames.Count == 0)
        {
            return pods;
        }

        return pods.Where(pod => !string.IsNullOrWhiteSpace(pod.Spec?.NodeName) && nodeNames.Contains(pod.Spec!.NodeName!)).ToList();
    }

    private static double GetLatestSeriesValue(IReadOnlyList<ClusterOverviewMetricSeries> series, string name)
    {
        for (int i = 0; i < series.Count; i++)
        {
            if (!string.Equals(series[i].Name, name, StringComparison.Ordinal) || series[i].Points.Count == 0)
            {
                continue;
            }

            double? value = series[i].Points[^1].Value;
            if (value.HasValue && value.Value > 0)
            {
                return value.Value;
            }
        }

        return 0;
    }

    private static IReadOnlyList<ClusterOverviewMetricSeries> CreateClusterSeries(params (string Name, decimal Value)[] items)
    {
        List<ClusterOverviewMetricSeries> series = new(items.Length);
        DateTime now = DateTime.Now;

        foreach (var item in items)
        {
            if (item.Value <= 0)
            {
                continue;
            }

            series.Add(new ClusterOverviewMetricSeries(item.Name, [new DateTimePoint(now, (double)item.Value)]));
        }

        return series;
    }

    private static IReadOnlyList<ClusterOverviewMetricSeries> CreateClusterSeries(params (string Name, long Value)[] items)
    {
        List<ClusterOverviewMetricSeries> series = new(items.Length);
        DateTime now = DateTime.Now;

        foreach (var item in items)
        {
            if (item.Value <= 0)
            {
                continue;
            }

            series.Add(new ClusterOverviewMetricSeries(item.Name, [new DateTimePoint(now, item.Value)]));
        }

        return series;
    }

    private static async Task<IReadOnlyList<ClusterOverviewMetricSeries>> LoadClusterRingChartAsync(
        ClusterWorkspaceViewModel cluster,
        ClusterScope scope,
        MetricTimeRangeOption? selectedTimeRange,
        string title,
        params (string QueryName, string Label)[] queries)
    {
        try
        {
            string nodeMatcher = string.Join(
                "|",
                GetClusterScopeNodes(cluster, scope)
                    .Select(static node => node.Name())
                    .Where(static name => !string.IsNullOrWhiteSpace(name))
                    .Select(static name => Regex.Escape(name!)));

            KubeUI.Kubernetes.MetricRequest request = new()
            {
                Category = KubeUI.Kubernetes.MetricCategory.Cluster,
                RangeSeconds = selectedTimeRange?.RangeSeconds ?? 3600,
                StepSeconds = 60,
                Queries = queries.Select(query => new KubeUI.Kubernetes.MetricQueryDefinition
                {
                    Name = query.QueryName,
                    Options = new Dictionary<string, string>(StringComparer.Ordinal)
                    {
                        ["nodes"] = string.IsNullOrWhiteSpace(nodeMatcher) ? ".*" : nodeMatcher,
                    },
                }).ToArray(),
            };

            KubeUI.Kubernetes.MetricResultSet result = await cluster.RequestMetricsAsync(request).ConfigureAwait(false);
            if (result.IsEmpty)
            {
                return [];
            }

            List<ClusterOverviewMetricSeries> series = new(queries.Length);

            foreach (var query in queries)
            {
                if (!result.Metrics.TryGetValue(query.QueryName, out var metricSeries))
                {
                    continue;
                }

                foreach (var item in metricSeries)
                {
                    DateTimePoint[] points = item.Points.Select(static value => new DateTimePoint(value.Timestamp.LocalDateTime, value.Value)).ToArray();
                    if (points.Length == 0)
                    {
                        continue;
                    }

                    series.Add(new ClusterOverviewMetricSeries(query.Label, points));
                }
            }

            return series;
        }
        catch
        {
            return [];
        }
    }

    private static decimal SumNodePodCapacity(IReadOnlyList<V1Node> nodes)
    {
        decimal total = 0;
        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].Status?.Capacity?.TryGetValue("pods", out var quantity) == true)
            {
                total += quantity.ToDecimal();
            }
        }

        return total;
    }

    private static decimal SumNodePodAllocatableCapacity(IReadOnlyList<V1Node> nodes)
    {
        decimal total = 0;
        for (int i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].Status?.Allocatable?.TryGetValue("pods", out var quantity) == true)
            {
                total += quantity.ToDecimal();
            }
        }

        return total;
    }

    private static IReadOnlyList<ClusterOverviewMetricSeries> CreatePodLegendSeries(int podCount, decimal podAllocatableCapacity, decimal podCapacity)
    {
        DateTime now = DateTime.Now;
        return
        [
            new ClusterOverviewMetricSeries("Usage", [new DateTimePoint(now, podCount)]),
            new ClusterOverviewMetricSeries("Allocatable Capacity", [new DateTimePoint(now, (double)Math.Max(0, podAllocatableCapacity))]),
            new ClusterOverviewMetricSeries("Capacity", [new DateTimePoint(now, (double)Math.Max(0, podCapacity))]),
        ];
    }

    private static IReadOnlyList<ClusterOverviewMetricSeries> CreateCpuLegendSeries(decimal cpuUsage, decimal cpuRequests, decimal cpuLimits, decimal cpuAllocatable, decimal cpuCapacity)
    {
        DateTime now = DateTime.Now;
        return
        [
            new ClusterOverviewMetricSeries("Usage", [new DateTimePoint(now, (double)Math.Max(0, cpuUsage))]),
            new ClusterOverviewMetricSeries("Requests", [new DateTimePoint(now, (double)Math.Max(0, cpuRequests))]),
            new ClusterOverviewMetricSeries("Limits", [new DateTimePoint(now, (double)Math.Max(0, cpuLimits))]),
            new ClusterOverviewMetricSeries("Allocatable Capacity", [new DateTimePoint(now, (double)Math.Max(0, cpuAllocatable))]),
            new ClusterOverviewMetricSeries("Capacity", [new DateTimePoint(now, (double)Math.Max(0, cpuCapacity))]),
        ];
    }

    private static IReadOnlyList<ClusterOverviewMetricSeries> CreateMemoryLegendSeries(double memoryUsageBytes, long memoryRequestsBytes, long memoryLimitsBytes, long memoryAllocatableBytes, long memoryCapacityBytes)
    {
        DateTime now = DateTime.Now;
        return
        [
            new ClusterOverviewMetricSeries("Usage", [new DateTimePoint(now, memoryUsageBytes)]),
            new ClusterOverviewMetricSeries("Requests", [new DateTimePoint(now, memoryRequestsBytes)]),
            new ClusterOverviewMetricSeries("Limits", [new DateTimePoint(now, memoryLimitsBytes)]),
            new ClusterOverviewMetricSeries("Allocatable Capacity", [new DateTimePoint(now, memoryAllocatableBytes)]),
            new ClusterOverviewMetricSeries("Capacity", [new DateTimePoint(now, memoryCapacityBytes)]),
        ];
    }

    private static bool IsMasterNode(V1Node node)
    {
        var labels = node.Metadata?.Labels;
        if (labels == null)
        {
            return false;
        }

        return labels.ContainsKey("node-role.kubernetes.io/master")
            || labels.ContainsKey("node-role.kubernetes.io/control-plane");
    }
}

internal sealed record ClusterOverviewSnapshot(
    decimal CpuUsage,
    decimal CpuRequests,
    decimal CpuLimits,
    decimal CpuCapacity,
    decimal CpuAllocatable,
    long MemoryUsageBytes,
    long MemoryRequestsBytes,
    long MemoryLimitsBytes,
    long MemoryCapacityBytes,
    long MemoryAllocatableBytes,
    int PodCount,
    decimal PodCapacity,
    decimal PodAllocatableCapacity,
    IReadOnlyList<ClusterOverviewMetricSeries> TrendSeries,
    IReadOnlyList<ClusterOverviewMetricSeries> CpuSeries,
    IReadOnlyList<ClusterOverviewMetricSeries> MemorySeries,
    IReadOnlyList<ClusterOverviewMetricSeries> PodSeries,
    IReadOnlyList<ClusterOverviewMetricSeries> CpuLegendSeries,
    IReadOnlyList<ClusterOverviewMetricSeries> MemoryLegendSeries,
    IReadOnlyList<ClusterOverviewMetricSeries> PodLegendSeries);
