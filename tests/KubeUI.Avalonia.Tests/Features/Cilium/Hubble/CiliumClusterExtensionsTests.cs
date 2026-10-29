using Avalonia.Headless.XUnit;
using k8s.Models;
using KubeUI.Avalonia.Tests.Infra;
using KubeUI.Cilium;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.Cilium.Hubble;

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
}
