using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using KubeUI.Avalonia.Tests.Infra;
using KubeUI.Testing;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.Clusters.Settings;

public sealed class ClusterSettingsViewModelTests : AvaloniaTestBase
{
    [AvaloniaFact]
    public void cluster_settings_default_debug_container_image_is_busybox()
    {
        using var scope = new KubernetesTestClusterScope();
        using var workspace = ActivatorUtilities.CreateInstance<ClusterWorkspace>(TestApp.CurrentServices!, scope.Cluster);

        var settings = TestApp.CurrentServices!.GetRequiredService<ISettingsService>();
        settings.Settings.GetClusterSettings(workspace.Runtime).DebugContainerImage.ShouldBe(ClusterSettings.DefaultDebugContainerImage);
    }

    [AvaloniaFact]
    public void changing_debug_container_image_updates_persisted_cluster_settings()
    {
        using var scope = new KubernetesTestClusterScope();
        using var workspace = ActivatorUtilities.CreateInstance<ClusterWorkspace>(TestApp.CurrentServices!, scope.Cluster);
        var viewModel = ActivatorUtilities.CreateInstance<ClusterSettingsViewModel>(
            TestApp.CurrentServices ?? throw new InvalidOperationException("Test services are not initialized."));

        viewModel.Initialize(workspace);
        Dispatcher.UIThread.RunJobs();

        viewModel.DebugContainerImage = "example.com/debug:1";

        TestApp.CurrentServices!.GetRequiredService<ISettingsService>()
            .Settings
            .GetClusterSettings(workspace.Runtime)
            .DebugContainerImage
            .ShouldBe("example.com/debug:1");
    }
}
