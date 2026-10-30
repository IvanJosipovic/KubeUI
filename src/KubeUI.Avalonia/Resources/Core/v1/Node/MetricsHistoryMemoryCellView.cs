using KubeUI.Avalonia.Infrastructure.Threading;

namespace KubeUI.Avalonia.Resources.Core.v1.Node;

public sealed class MetricsHistoryMemoryCellView : NodeMetricsHistoryCellBase
{
    public MetricsHistoryMemoryCellView(IUiRefreshClock refreshClock, TimeProvider timeProvider)
        : base(refreshClock, timeProvider)
    {
    }

    protected override bool IsMemoryMetric => true;
}
