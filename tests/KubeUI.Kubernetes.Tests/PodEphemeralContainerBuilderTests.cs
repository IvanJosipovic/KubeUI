using k8s.Models;
using KubeUI.Avalonia.Options;
using KubeUI.Kubernetes;
using Shouldly;

namespace KubeUI.Kubernetes.Tests;

public sealed class PodEphemeralContainerBuilderTests
{
    [Fact]
    public void WithDebugContainer_appends_an_ephemeral_container_with_default_shell_command()
    {
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

        V1Pod updated = PodEphemeralContainerBuilder.WithDebugContainer(pod, "app", ClusterSettings.DefaultDebugContainerImage);

        updated.ShouldNotBeSameAs(pod);
        updated.Spec.EphemeralContainers.ShouldNotBeNull();
        updated.Spec.EphemeralContainers.Count.ShouldBe(1);
        updated.Spec.EphemeralContainers[0].Image.ShouldBe(ClusterSettings.DefaultDebugContainerImage);
        updated.Spec.EphemeralContainers[0].TargetContainerName.ShouldBe("app");
        updated.Spec.EphemeralContainers[0].Command.ShouldBe(["sh"]);
        updated.Spec.EphemeralContainers[0].Stdin.ShouldBe(true);
        updated.Spec.EphemeralContainers[0].StdinOnce.ShouldBe(true);
        updated.Spec.EphemeralContainers[0].Tty.ShouldBe(true);
    }

    [Fact]
    public void WithDebugContainer_accepts_pod_level_debugging_without_target_container()
    {
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

        V1Pod updated = PodEphemeralContainerBuilder.WithDebugContainer(pod, null, "example.com/debug:1");

        updated.Spec.EphemeralContainers.ShouldNotBeNull();
        updated.Spec.EphemeralContainers[0].TargetContainerName.ShouldBeNull();
        updated.Spec.EphemeralContainers[0].Image.ShouldBe("example.com/debug:1");
    }
}
