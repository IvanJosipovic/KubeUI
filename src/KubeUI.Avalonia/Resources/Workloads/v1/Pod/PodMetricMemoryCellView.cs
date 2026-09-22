using Humanizer;
using k8s.Models;
using KubeUI.Avalonia.Infrastructure.Threading;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Pod;

public sealed class PodMetricMemoryCellView : PodMetricCellBase
{
    public PodMetricMemoryCellView(IUiRefreshClock refreshClock)
        : base(refreshClock)
    {
    }

    protected override string FormatMetric(PodMetrics metric)
    {
        try
        {
            var usageBytes = metric.Containers.Sum(container => container.Usage["memory"].ToInt64());
            return usageBytes.Bytes().Humanize();
        }
        catch
        {
            return string.Empty;
        }
    }
}
