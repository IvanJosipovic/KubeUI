using k8s.Models;
using KubeUI.Controls.Presentation;
using KubeUI.Controls.Threading;
using KubeUI.Features.Clusters.Workspace;
using KubeUI.Features.Resources.List.Controls;

namespace KubeUI.Features.Resources.Workloads.v1.Pod;

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
