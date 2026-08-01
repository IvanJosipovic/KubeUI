using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using KubeUI.Avalonia.Tests.Infra;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.Clusters.Settings;

public sealed class ClusterSettingsViewModelTests
{
    [AvaloniaFact]
    public void cluster_settings_default_debug_container_image_is_busybox()
    {
        using var scope = new KubernetesTestClusterScope();
        using var workspace = ActivatorUtilities.CreateInstance<ClusterWorkspace>((Application.Current as TestApp)?.Services!, scope.Cluster);

        var settings = (Application.Current as TestApp)?.Services!.GetRequiredService<ISettingsService>();
        settings.Settings.GetClusterSettings(workspace.Runtime).DebugContainerImage.ShouldBe(ClusterSettings.DefaultDebugContainerImage);
    }

    [AvaloniaFact]
    public void changing_debug_container_image_updates_persisted_cluster_settings()
    {
        using var scope = new KubernetesTestClusterScope();
        using var workspace = ActivatorUtilities.CreateInstance<ClusterWorkspace>((Application.Current as TestApp)?.Services!, scope.Cluster);
        var viewModel = ActivatorUtilities.CreateInstance<ClusterSettingsViewModel>(
            (Application.Current as TestApp)?.Services ?? throw new InvalidOperationException("Test services are not initialized."));

        viewModel.Initialize(workspace);
        Dispatcher.UIThread.RunJobs();

        viewModel.DebugContainerImage = "example.com/debug:1";

        (Application.Current as TestApp)?.Services!.GetRequiredService<ISettingsService>()
            .Settings
            .GetClusterSettings(workspace.Runtime)
            .DebugContainerImage
            .ShouldBe("example.com/debug:1");
    }
}
