using k8s.Models;
using KubeUI.Testing;
using Shouldly;

namespace KubeUI.Kubernetes.Tests;

public sealed class ClusterResourceUpdateTests
{
    [Fact]
    public async Task test_runtime_update_replaces_cached_pod_spec_record()
    {
        TestClusterRuntime cluster = new();
        V1Pod pod = new()
        {
            Metadata = new V1ObjectMeta
            {
                Name = "pod-a",
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

        await cluster.AddOrUpdateResource(pod);
        await cluster.AddPodEphemeralDebugContainer(pod, "app", "example.com/debug:1");

        V1Pod updated = cluster.GetResource<V1Pod>("default", "pod-a").ShouldNotBeNull();
        updated.Spec.ShouldNotBeNull();
        updated.Spec.EphemeralContainers.ShouldNotBeNull();
        updated.Spec.EphemeralContainers.Count.ShouldBe(1);
        updated.Spec.EphemeralContainers[0].Image.ShouldBe("example.com/debug:1");
        updated.Spec.EphemeralContainers[0].TargetContainerName.ShouldBe("app");
    }
}
