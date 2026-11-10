using Avalonia.Headless.XUnit;
using k8s.Models;
using KubeUI.App.Tests.Infra;
using KubeUI.Features.Cilium;
using Shouldly;

namespace KubeUI.Features.Cilium.Tests.Hubble;

public sealed class CiliumClusterExtensionsTests
{
    [AvaloniaFact]
    public async Task IsCiliumInstalledAsync_seeds_crd_cache_and_matches_cilium_group()
    {
        using var workspace = await Application.Current.CreateClusterAsync(config => config.Resources =
        [
            new V1CustomResourceDefinition
            {
                Metadata = new V1ObjectMeta { Name = "networkpolicies.cilium.io" },
                Spec = new V1CustomResourceDefinitionSpec { Group = "cilium.io" },
            },
        ]);

        var isInstalled = await workspace.Runtime.IsCiliumInstalledAsync(TestContext.Current.CancellationToken);

        isInstalled.ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task IsCiliumInstalledAsync_rejects_dot_boundary_subgroups_that_share_the_prefix()
    {
        using var workspace = await Application.Current.CreateClusterAsync(config => config.Resources =
        [
            new V1CustomResourceDefinition
            {
                Metadata = new V1ObjectMeta { Name = "networkpolicies.xcilium.io" },
                Spec = new V1CustomResourceDefinitionSpec { Group = "xcilium.io" },
            },
            new V1CustomResourceDefinition
            {
                Metadata = new V1ObjectMeta { Name = "networkpolicies.notcilium.io" },
                Spec = new V1CustomResourceDefinitionSpec { Group = "notcilium.io" },
            },
        ]);

        var isInstalled = await workspace.Runtime.IsCiliumInstalledAsync(TestContext.Current.CancellationToken);

        isInstalled.ShouldBeFalse();
        workspace.Runtime.IsCiliumInstalled().ShouldBeFalse();
    }
}
