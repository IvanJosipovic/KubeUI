using System.Collections.ObjectModel;
using System.Linq;
using Avalonia.Threading;
using Humanizer;
using k8s.Models;
using KubeUI.Avalonia.Features.Clusters.Workspace.ViewModels;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;
using KubeUI.Kubernetes;
using LiveChartsCore.Defaults;
using Microsoft.Extensions.Logging.Abstractions;

namespace KubeUI.Avalonia.Features.Clusters.Overview.ViewModels;

public sealed partial class ClusterViewModel
{
    [ObservableProperty]
    public partial ClusterOverviewChartPanelViewModel ClusterTrendChart { get; set; } = new()
    {
        Title = "Cluster CPU",
        EmptyText = "Trend data is unavailable for the selected backend.",
    };

    public ObservableCollection<ClusterScopeOption> ClusterOverviewScopeOptions { get; } = [];

    [ObservableProperty]
    public partial ClusterScopeOption? SelectedClusterOverviewScope { get; set; }

    public ObservableCollection<MetricTabViewModel> ClusterOverviewModes { get; } = [];

    [ObservableProperty]
    public partial MetricTabViewModel? SelectedClusterOverviewMode { get; set; }

    public ObservableCollection<MetricTimeRangeOption> ClusterOverviewTimeRangeOptions { get; } = [];

    [ObservableProperty]
    public partial MetricTimeRangeOption? SelectedClusterOverviewTimeRange { get; set; }

    public ClusterOverviewRingChartViewModel ClusterCpuChart { get; } = new();

    public ClusterOverviewRingChartViewModel ClusterMemoryChart { get; } = new();

    public ClusterOverviewRingChartViewModel ClusterPodChart { get; } = new();

    private async Task RefreshOverviewChartsAsync()
    {
        if (_cluster == null)
        {
            return;
        }

        InitializeClusterOverviewOptions();

        var snapshot = await ClusterOverviewBackend.LoadClusterOverviewSnapshotAsync(
            _cluster,
            SelectedClusterOverviewScope?.Scope ?? ClusterScope.All,
            SelectedClusterOverviewMode,
            SelectedClusterOverviewTimeRange,
            NullLogger.Instance).ConfigureAwait(false);

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            ClusterTrendChart.SetSeries(snapshot.TrendSeries);
            ClusterCpuChart.SetSeries(snapshot.CpuSeries, static value => value <= 0 ? "n/a" : $"{value:F2}c", snapshot.CpuLegendSeries, title: "CPU");
            ClusterMemoryChart.SetSeries(snapshot.MemorySeries, static value => value <= 0 ? "n/a" : value.Bytes().Humanize(), snapshot.MemoryLegendSeries, title: "Memory");
            ClusterPodChart.SetSeries(snapshot.PodSeries, legendSnapshots: snapshot.PodLegendSeries, innerRadius: 34, title: "Pods");
        });
    }

    partial void OnSelectedClusterOverviewScopeChanged(ClusterScopeOption? value)
    {
        _ = RefreshData();
    }

    partial void OnSelectedClusterOverviewModeChanged(MetricTabViewModel? value)
    {
        SyncClusterOverviewModeSelection();
        _ = RefreshData();
    }

    partial void OnSelectedClusterOverviewTimeRangeChanged(MetricTimeRangeOption? value)
    {
        _ = RefreshData();
    }

    private void InitializeClusterOverviewOptions()
    {
        if (ClusterOverviewScopeOptions.Count == 0)
        {
            ClusterOverviewScopeOptions.Add(new ClusterScopeOption("All Nodes", ClusterScope.All));
            ClusterOverviewScopeOptions.Add(new ClusterScopeOption("Worker Nodes", ClusterScope.WorkerNodes));
            ClusterOverviewScopeOptions.Add(new ClusterScopeOption("Master Nodes", ClusterScope.MasterNodes));
        }

        if (ClusterOverviewModes.Count == 0)
        {
            ClusterOverviewModes.Add(new MetricTabViewModel { Title = "CPU", IsSelected = true });
            ClusterOverviewModes.Add(new MetricTabViewModel { Title = "Memory" });
        }

        if (ClusterOverviewTimeRangeOptions.Count == 0)
        {
            ClusterOverviewTimeRangeOptions.Add(new MetricTimeRangeOption("1h", 3600));
            ClusterOverviewTimeRangeOptions.Add(new MetricTimeRangeOption("2h", 7200));
            ClusterOverviewTimeRangeOptions.Add(new MetricTimeRangeOption("4h", 14400));
            ClusterOverviewTimeRangeOptions.Add(new MetricTimeRangeOption("24h", 86400));
        }

        SelectedClusterOverviewScope ??= ClusterOverviewScopeOptions[0];
        SelectedClusterOverviewMode ??= ClusterOverviewModes[0];
        SelectedClusterOverviewTimeRange ??= ClusterOverviewTimeRangeOptions[0];
        SyncClusterOverviewModeSelection();
    }

    private void SyncClusterOverviewModeSelection()
    {
        for (var i = 0; i < ClusterOverviewModes.Count; i++)
        {
            ClusterOverviewModes[i].IsSelected = ReferenceEquals(ClusterOverviewModes[i], SelectedClusterOverviewMode);
        }
    }

    private ClusterOverviewSnapshot CreateClusterOverviewSnapshot(ClusterWorkspaceViewModel cluster)
    {
        IReadOnlyList<V1Node> nodes = cluster.GetResourceList<V1Node>() ?? [];
        IReadOnlyList<V1Pod> pods = cluster.GetResourceList<V1Pod>() ?? [];

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

        foreach (var metric in cluster.NodeMetrics)
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

        double cpuFrame = cpuAllocatable > 0 ? (double)cpuAllocatable : (double)cpuCapacity;
        double memoryFrame = memoryAllocatableBytes > 0 ? memoryAllocatableBytes : memoryCapacityBytes;
        decimal podCapacity = SumNodePodCapacity(nodes);
        decimal podAllocatableCapacity = SumNodePodAllocatableCapacity(nodes);
        double podFrame = podAllocatableCapacity > 0 ? (double)podAllocatableCapacity : (double)podCapacity;

        return new ClusterOverviewSnapshot(
            CreateClusterSeries(
                ("Usage", (decimal)ClusterOverviewRingChartViewModel.ClampToFrame((double)cpuUsage, cpuFrame)),
                ("Requests", (decimal)ClusterOverviewRingChartViewModel.ClampToFrame((double)cpuRequests, cpuFrame)),
                ("Limits", (decimal)ClusterOverviewRingChartViewModel.ClampToFrame((double)cpuLimits, cpuFrame))),
            CreateCpuLegendSeries(cpuUsage, cpuRequests, cpuLimits, cpuAllocatable, cpuCapacity),
            CreateClusterSeries(
                ("Usage", (long)ClusterOverviewRingChartViewModel.ClampToFrame((double)memoryUsageBytes, memoryFrame)),
                ("Requests", (long)ClusterOverviewRingChartViewModel.ClampToFrame((double)memoryRequestsBytes, memoryFrame)),
                ("Limits", (long)ClusterOverviewRingChartViewModel.ClampToFrame((double)memoryLimitsBytes, memoryFrame))),
            CreateMemoryLegendSeries((double)memoryUsageBytes, memoryRequestsBytes, memoryLimitsBytes, memoryAllocatableBytes, memoryCapacityBytes),
            CreateClusterSeries(
                ("Usage", (decimal)ClusterOverviewRingChartViewModel.ClampToFrame((double)pods.Count, podFrame))),
            CreatePodLegendSeries(pods.Count, podAllocatableCapacity, podCapacity));
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

    private static IReadOnlyList<ClusterOverviewMetricSeries> CreatePodLegendSeries(int podCount, decimal podAllocatableCapacity, decimal podCapacity)
    {
        DateTime now = DateTime.Now;
        return
        [
            new ClusterOverviewMetricSeries("Usage", [new DateTimePoint(now, podCount)]),
            new ClusterOverviewMetricSeries("Allocatable Capacity", [new DateTimePoint(now, (double)Math.Max(0, podAllocatableCapacity))], true),
            new ClusterOverviewMetricSeries("Capacity", [new DateTimePoint(now, (double)Math.Max(0, podCapacity))], true),
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
            new ClusterOverviewMetricSeries("Allocatable Capacity", [new DateTimePoint(now, (double)Math.Max(0, cpuAllocatable))], true),
            new ClusterOverviewMetricSeries("Capacity", [new DateTimePoint(now, (double)Math.Max(0, cpuCapacity))], true),
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
            new ClusterOverviewMetricSeries("Allocatable Capacity", [new DateTimePoint(now, memoryAllocatableBytes)], true),
            new ClusterOverviewMetricSeries("Capacity", [new DateTimePoint(now, memoryCapacityBytes)], true),
        ];
    }

    private static decimal SumNodePodCapacity(IReadOnlyList<V1Node> nodes)
    {
        decimal total = 0;
        for (var i = 0; i < nodes.Count; i++)
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
        for (var i = 0; i < nodes.Count; i++)
        {
            if (nodes[i].Status?.Allocatable?.TryGetValue("pods", out var quantity) == true)
            {
                total += quantity.ToDecimal();
            }
        }

        return total;
    }

    private sealed record ClusterOverviewSnapshot(
        IReadOnlyList<ClusterOverviewMetricSeries> CpuSeries,
        IReadOnlyList<ClusterOverviewMetricSeries> CpuLegendSeries,
        IReadOnlyList<ClusterOverviewMetricSeries> MemorySeries,
        IReadOnlyList<ClusterOverviewMetricSeries> MemoryLegendSeries,
        IReadOnlyList<ClusterOverviewMetricSeries> PodSeries,
        IReadOnlyList<ClusterOverviewMetricSeries> PodLegendSeries);

    private void DisposeOverviewCharts()
    {
        ClusterCpuChart.Dispose();
        ClusterMemoryChart.Dispose();
        ClusterPodChart.Dispose();
    }
}
