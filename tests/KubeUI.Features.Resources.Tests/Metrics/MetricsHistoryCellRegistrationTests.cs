using Avalonia.Headless.XUnit;
using KubeUI.Features.Resources.ResourceConfigs.Workloads.v1.Pod;
using Shouldly;

namespace KubeUI.Features.Resources.Tests.Metrics;

public sealed class MetricsHistoryCellRegistrationTests
{
    [AvaloniaFact]
    public void pod_history_cells_resolve_with_their_shared_index()
    {
        var services = Application.Current.GetTestServices();

        services.GetRequiredService<PodPrometheusHistoryIndex>()
            .ShouldNotBeNull();

        services.GetRequiredService<MetricsHistoryCPUCellView>()
            .ShouldBeOfType<MetricsHistoryCPUCellView>();
        services.GetRequiredService<MetricsHistoryMemoryCellView>()
            .ShouldBeOfType<MetricsHistoryMemoryCellView>();
    }
}
