using KubeUI.Avalonia.Infrastructure.Threading;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Pod;

public sealed class MetricsHistoryCPUCellView : PodMetricsHistoryCellBase
{
    public MetricsHistoryCPUCellView(
        IUiRefreshClock refreshClock,
        TimeProvider timeProvider)
        : base(refreshClock, timeProvider)
    {
    }

    protected override bool IsMemoryMetric => false;
}
