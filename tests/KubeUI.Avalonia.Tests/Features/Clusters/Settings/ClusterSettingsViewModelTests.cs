using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using KubeUI.Avalonia.Features.Clusters.Settings.ViewModels;
using KubeUI.Avalonia.Options;
using KubeUI.Avalonia.Services.Settings;
using KubeUI.Avalonia.Tests.Infra;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.Clusters.Settings;

public sealed class ClusterSettingsViewModelTests : AvaloniaTestBase
{
    [AvaloniaFact]
    public void cluster_settings_default_debug_container_image_is_busybox()
    {
        var runtime = new TestCluster();
        var workspace = runtime.CreateWorkspace();

        var settings = TestApp.CurrentServices!.GetRequiredService<ISettingsService>();
        settings.Settings.GetClusterSettings(workspace).DebugContainerImage.ShouldBe(ClusterSettings.DefaultDebugContainerImage);
    }

    [AvaloniaFact]
    public void changing_debug_container_image_updates_persisted_cluster_settings()
    {
        var runtime = new TestCluster();
        var workspace = runtime.CreateWorkspace();
        var viewModel = ActivatorUtilities.CreateInstance<ClusterSettingsViewModel>(
            TestApp.CurrentServices ?? throw new InvalidOperationException("Test services are not initialized."));

        viewModel.Initialize(workspace);
        Dispatcher.UIThread.RunJobs();

        viewModel.DebugContainerImage = "example.com/debug:1";

        TestApp.CurrentServices!.GetRequiredService<ISettingsService>()
            .Settings
            .GetClusterSettings(workspace)
            .DebugContainerImage
            .ShouldBe("example.com/debug:1");
    }
}
