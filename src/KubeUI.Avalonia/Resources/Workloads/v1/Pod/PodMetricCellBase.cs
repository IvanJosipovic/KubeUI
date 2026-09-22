using k8s.Models;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Features.Resources.List.Controls;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Avalonia.Infrastructure.Threading;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Pod;

public abstract class PodMetricCellBase : RefreshingCellTextBlock, IInitializeCluster
{
    public ClusterWorkspace? Cluster { get; private set; }

    protected PodMetricCellBase(IUiRefreshClock refreshClock)
        : base(refreshClock)
    {
    }

    public void Initialize(ClusterWorkspace cluster)
    {
        Cluster = cluster;
        RefreshText();
    }

    protected sealed override string ResolveText(object? dataContext)
    {
        if (Cluster == null || dataContext is not V1Pod pod)
        {
            return string.Empty;
        }

        var metric = Cluster.Runtime.PodMetrics.FirstOrDefault(metric =>
            metric.Name() == pod.Name() && metric.Namespace() == pod.Namespace());

        return metric == null ? string.Empty : FormatMetric(metric);
    }

    protected abstract string FormatMetric(PodMetrics metric);
}
