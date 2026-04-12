using Avalonia.Headless.XUnit;
using k8s.Models;
using KubeUI.Avalonia.Services.Settings;
using KubeUI.Avalonia.Tests.Infra;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.Workloads.Pod;

public sealed class PodDebugContainerTests : AvaloniaTestBase
{
    [AvaloniaFact]
    public async Task adding_debug_container_uses_cluster_image_and_target_container()
    {
        var runtime = new TestCluster();
        var workspace = runtime.CreateWorkspace();
        var settings = TestApp.CurrentServices!.GetRequiredService<ISettingsService>();
        settings.Settings.GetClusterSettings(workspace).DebugContainerImage = "example.com/debug:1";

        V1Pod pod = new()
        {
            Metadata = new V1ObjectMeta
            {
                Name = "pod-1",
                NamespaceProperty = "default",
            },
            Spec = new V1PodSpec
            {
                Containers =
                [
                    new V1Container
                    {
                        Name = "app",
                    },
                ],
            },
        };

        await workspace.AddOrUpdateResource(pod);
        await workspace.AddPodEphemeralDebugContainer(pod, "app", settings.Settings.GetClusterSettings(workspace).DebugContainerImage);

        V1Pod updated = runtime.GetResource<V1Pod>("default", "pod-1").ShouldNotBeNull();
        updated.Spec.EphemeralContainers.ShouldNotBeNull();
        updated.Spec.EphemeralContainers.Count.ShouldBe(1);
        updated.Spec.EphemeralContainers[0].Image.ShouldBe("example.com/debug:1");
        updated.Spec.EphemeralContainers[0].TargetContainerName.ShouldBe("app");
    }
}
