using Avalonia.Headless.XUnit;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Infra;

public sealed class ApplicationServiceResolutionTests
{
    [AvaloniaFact]
    public void TestAppBuilder_initializes_services()
    {
        Application.Current.GetTestServices().ShouldNotBeNull();
        Application.Current.ShouldBeAssignableTo<KubeUI.App.App>();
    }

    [AvaloniaFact]
    public async Task TestClusterGenerator_creates_isolated_fake_workspaces()
    {
        using var first = await Application.Current.CreateClusterAsync(
            config => config.Type = KubernetesBackend.Fake,
            connect: false);
        using var second = await Application.Current.CreateClusterAsync(
            config => config.Type = KubernetesBackend.Fake,
            connect: false);

        first.ShouldNotBeSameAs(second);
        first.Runtime.ShouldNotBeSameAs(second.Runtime);
        first.Runtime.Name.ShouldNotBe(second.Runtime.Name);
    }

    [AvaloniaFact]
    public async Task CreateClusterAsync_resets_the_shared_config_between_clusters()
    {
        // First cluster poisons the shared TestClusterConfig singleton; the second cluster
        // must not inherit ThrowOnConnect from it, so it connects successfully.
        var poisoned = await Application.Current.CreateClusterAsync(
            config =>
            {
                config.Type = KubernetesBackend.Fake;
                config.ThrowOnConnect = true;
            });

        poisoned.Runtime.Connected.ShouldBeFalse();

        using var workspace = await Application.Current.CreateClusterAsync(
            config => config.Type = KubernetesBackend.Fake);

        workspace.Runtime.Connected.ShouldBeTrue();
    }
}
