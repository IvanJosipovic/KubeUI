using KubeUI.Avalonia.Infrastructure.Threading;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Pod;

public sealed class MetricsHistoryMemoryCellView : PodMetricsHistoryCellBase
{
    public MetricsHistoryMemoryCellView(
        IUiRefreshClock refreshClock,
        TimeProvider timeProvider)
        : base(refreshClock, timeProvider)
    {
    }

    protected override bool IsMemoryMetric => true;
}
