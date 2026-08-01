using Avalonia.Headless.XUnit;
using k8s.Models;
using KubeUI.Avalonia.Tests.Infra;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.Workloads.Pod;

public sealed class PodDebugContainerTests
{
    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task adding_debug_container_uses_cluster_image_and_target_container(KubernetesBackend backend)
    {
        var services = (Application.Current as TestApp)?.Services ?? throw new InvalidOperationException("Test services are not initialized.");
        await using var scope = await KubernetesTestWorkspaceScope.CreateAsync(services, backend);
        var runtime = scope.ScenarioHarness.Cluster;
        var workspace = scope.Workspace;
        await workspace.Connect();
        await runtime.SeedResource<V1Pod>(true);
        var settings = services.GetRequiredService<ISettingsService>();
        settings.Settings.GetClusterSettings(workspace.Runtime).DebugContainerImage = "example.com/debug:1";

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
                        Image = "example.com/app:1",
                    },
                ],
            },
        };

        await workspace.Runtime.AddOrUpdateResource(pod);
        await TestWait.UntilAsync(
            () => runtime.GetResource<V1Pod>("default", "pod-1") is not null,
            TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken);
        V1Pod currentPod = runtime.GetResource<V1Pod>("default", "pod-1").ShouldNotBeNull();
        await workspace.Runtime.AddPodEphemeralDebugContainer(currentPod, "app", settings.Settings.GetClusterSettings(workspace.Runtime).DebugContainerImage);

        await TestWait.UntilAsync(
            () => runtime.GetResource<V1Pod>("default", "pod-1")?.Spec?.EphemeralContainers?.Count == 1,
            TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken);

        V1Pod updated = runtime.GetResource<V1Pod>("default", "pod-1").ShouldNotBeNull();
        updated.Spec.EphemeralContainers.ShouldNotBeNull();
        updated.Spec.EphemeralContainers.Count.ShouldBe(1);
        updated.Spec.EphemeralContainers[0].Image.ShouldBe("example.com/debug:1");
        updated.Spec.EphemeralContainers[0].TargetContainerName.ShouldBe("app");
    }
}
