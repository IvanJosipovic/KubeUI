using Avalonia.Headless.XUnit;
using KubeUI.Avalonia;
using KubeUI.Testing.Kubernetes.Scenarios;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Infra;

public sealed class ApplicationServiceResolutionTests
{
    [AvaloniaFact]
    public void TestAppBuilder_initializes_services()
    {
        Application.Current.GetTestServices().ShouldNotBeNull();
        Application.Current.ShouldBeAssignableTo<App>();
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
}
