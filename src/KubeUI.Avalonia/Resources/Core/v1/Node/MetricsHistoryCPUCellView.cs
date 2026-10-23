using KubeUI.Avalonia.Infrastructure.Threading;

namespace KubeUI.Avalonia.Resources.Core.v1.Node;

public sealed class MetricsHistoryCPUCellView : NodeMetricsHistoryCellBase
{
    public MetricsHistoryCPUCellView(IUiRefreshClock refreshClock, TimeProvider timeProvider)
        : base(refreshClock, timeProvider)
    {
    }

    protected override bool IsMemoryMetric => false;
}
