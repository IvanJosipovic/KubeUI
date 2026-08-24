using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using KubeUI.Avalonia.Tests.Infra;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.Clusters.Settings;

public sealed class ClusterSettingsViewModelTests
{
    [AvaloniaFact]
    public async Task cluster_settings_default_debug_container_image_is_busybox()
    {
        var services = Application.Current.GetTestServices();
        var workspace = services.GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        var settings = services.GetRequiredService<ISettingsService>();
        settings.Settings.GetClusterSettings(workspace.Runtime).DebugContainerImage.ShouldBe(ClusterSettings.DefaultDebugContainerImage);
    }

    [AvaloniaFact]
    public async Task changing_debug_container_image_updates_persisted_cluster_settings()
    {
        var services = Application.Current.GetTestServices();
        var workspace = services.GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        var viewModel = services.GetRequiredService<ClusterSettingsViewModel>();

        viewModel.Initialize(workspace);
        await TestApplicationExtensions.WaitForUiAsync();

        viewModel.DebugContainerImage = "example.com/debug:1";

        services.GetRequiredService<ISettingsService>()
            .Settings
            .GetClusterSettings(workspace.Runtime)
            .DebugContainerImage
            .ShouldBe("example.com/debug:1");
    }
}
