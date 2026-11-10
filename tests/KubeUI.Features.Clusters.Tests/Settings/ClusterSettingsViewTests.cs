using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.VisualTree;
using Shouldly;

namespace KubeUI.Features.Clusters.Tests.Settings;

public sealed class ClusterSettingsViewTests
{
    [AvaloniaFact]
    public async Task Manual_prometheus_endpoint_modes_render_as_separate_expandable_sections()
    {
        var settings = new Moq.Mock<IClusterPreferenceStore>();
        var azureMonitor = new Moq.Mock<IAzureMonitorWorkspaceService>();
        using var viewModel = new ClusterSettingsViewModel(settings.Object, azureMonitor.Object)
        {
            ClusterSettings = new ClusterSettings
            {
                MetricsSettings = new ClusterMetricsSettings
                {
                    MetricsServiceType = MetricsServiceType.Prometheus,
                    PrometheusProviderKind = PrometheusProviderKind.Manual,
                },
            },
        };
        var view = new ClusterSettingsView { DataContext = viewModel };

        using var window = Application.Current.CreateTestWindow(content: view);
        window.Show();
        await TestApplicationExtensions.WaitForUiAsync(TestContext.Current.CancellationToken);

        var sections = view.GetVisualDescendants()
            .OfType<ExpandableSection>()
            .Select(static section => section.Header?.ToString())
            .ToArray();

        sections.ShouldContain("Kubernetes Service (Port Forward)");
        sections.ShouldContain("Direct URL");
        sections.ShouldContain("Authentication");
    }
}
