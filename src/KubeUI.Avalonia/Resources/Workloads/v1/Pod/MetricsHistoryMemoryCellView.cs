using KubeUI.Avalonia.Infrastructure.Threading;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Pod;

public sealed class MetricsHistoryMemoryCellView : PodMetricsHistoryCellBase
{
    public MetricsHistoryMemoryCellView(
        IUiRefreshClock refreshClock,
        TimeProvider timeProvider,
        PodPrometheusHistoryIndex prometheusHistoryIndex)
        : base(refreshClock, timeProvider, prometheusHistoryIndex)
    {
    }

    protected override bool IsMemoryMetric => true;
}
