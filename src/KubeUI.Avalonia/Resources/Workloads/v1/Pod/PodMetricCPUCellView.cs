using k8s.Models;
using KubeUI.Avalonia.Infrastructure.Threading;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Pod;

public sealed class PodMetricCPUCellView : PodMetricCellBase
{
    public PodMetricCPUCellView(IUiRefreshClock refreshClock)
        : base(refreshClock)
    {
    }

    protected override string FormatMetric(PodMetrics metric)
        => $"{metric.Containers.Sum(container => container.Usage["cpu"].ToDecimal()):F3}c";
}
