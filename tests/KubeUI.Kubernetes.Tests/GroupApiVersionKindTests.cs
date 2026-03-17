using k8s.Models;
using KubernetesClient.Informer.Client;
using Shouldly;

namespace KubeUI.Kubernetes.Tests;

public class GroupApiVersionKindTests
{
    [Fact]
    public void FromPod_UsesExpectedKind()
    {
        GroupApiVersionKind.From<V1Pod>().Kind.ShouldBe("Pod");
    }
}
