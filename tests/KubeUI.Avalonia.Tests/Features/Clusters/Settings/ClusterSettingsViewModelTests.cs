using Avalonia;
using Avalonia.Headless.XUnit;
using KubeUI.Avalonia.Features.Clusters.Settings.ViewModels;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Avalonia.Services.Settings;
using KubeUI.Avalonia.Tests.Infra;
using KubeUI.Kubernetes;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.Clusters.Settings;

public sealed class ClusterSettingsViewModelTests : AvaloniaTestBase
{
    [AvaloniaFact]
    public void auto_metrics_mode_shows_detected_runtime_backend()
    {
        var runtime = new TestCluster
        {
            MetricsServiceType = MetricsServiceType.Prometheus,
        };
        var workspace = runtime.CreateWorkspace();
        var settings = Application.Current.GetRequiredService<ISettingsService>().Settings.GetClusterSettings(workspace);
        settings.MetricsServiceType = MetricsServiceType.Auto;

        var viewModel = new ClusterSettingsViewModel();
        viewModel.Initialize(workspace);

        viewModel.ShowDetectedMetricsBackend.ShouldBeTrue();
        viewModel.DetectedMetricsBackendText.ShouldBe("Currently using Prometheus (Prometheus Operator)");

        runtime.MetricsServiceType = MetricsServiceType.KubernetesMetricsServer;

        viewModel.DetectedMetricsBackendText.ShouldBe("Currently using Kubernetes Metrics Server");
    }
}
