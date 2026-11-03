using k8s.Models;
using KubeUI.Controls.Threading;

namespace KubeUI.Features.Resources.Workloads.v1.Pod;

public sealed class PodMetricCPUCellView : PodMetricCellBase
{
    public PodMetricCPUCellView(IUiRefreshClock refreshClock)
        : base(refreshClock)
    {
    }

    protected override string FormatMetric(PodMetrics metric)
        => $"{metric.Containers.Sum(container => container.Usage["cpu"].ToDecimal()):F3}c";
}
