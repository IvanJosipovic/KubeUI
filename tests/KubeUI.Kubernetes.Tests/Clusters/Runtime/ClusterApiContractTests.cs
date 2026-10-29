using System.Net;
using System.Text.Json.Nodes;
using k8s;
using k8s.Autorest;
using k8s.Models;
using Shouldly;

namespace KubeUI.Kubernetes.Tests.Clusters.Runtime;

[Trait("Category", "Kind")]
public sealed class ClusterApiContractTests
{
    [Theory, KubernetesBackendData]
    public async Task ConfigMapMergePatchMatchesKubernetesBehavior(KubernetesBackend backend)
    {
        await using var harness = await new TestClusterGenerator().CreateAsync(
            new TestClusterConfig { Type = backend },
            TestContext.Current.CancellationToken);
        await harness.Cluster.Connect();
        using var client = harness.Cluster.Client!.GetGenericClient<V1ConfigMap>();

        var created = await client.CreateNamespacedAsync(
            new V1ConfigMap
            {
                ApiVersion = V1ConfigMap.KubeApiVersion,
                Kind = V1ConfigMap.KubeKind,
                Metadata = new V1ObjectMeta { Name = "merge-patch-contract", NamespaceProperty = "default" },
                Data = new Dictionary<string, string> { ["keep"] = "before", ["remove"] = "value" },
            },
            "default",
            TestContext.Current.CancellationToken);

        var patched = await client.PatchNamespacedAsync<V1ConfigMap>(
            new V1Patch("{\"data\":{\"keep\":\"after\",\"remove\":null}}", V1Patch.PatchType.MergePatch),
            "default",
            created.Name(),
            TestContext.Current.CancellationToken);

        patched.Data.ShouldContainKeyAndValue("keep", "after");
        patched.Data.ShouldNotContainKey("remove");
        patched.Metadata.ResourceVersion.ShouldNotBe(created.Metadata.ResourceVersion);
        var deleted = await client.DeleteNamespacedAsync<V1ConfigMap>(
            "default",
            created.Name(),
            TestContext.Current.CancellationToken);
        deleted.Kind.ShouldBe("Status");
    }

    [Theory, KubernetesBackendData]
    public async Task InvalidPodReturnsKubernetesStatusAndFieldCause(KubernetesBackend backend)
    {
        await using var harness = await new TestClusterGenerator().CreateAsync(
            new TestClusterConfig { Type = backend },
            TestContext.Current.CancellationToken);
        await harness.Cluster.Connect();
        using var client = harness.Cluster.Client!.GetGenericClient<V1Pod>();

        var exception = await Should.ThrowAsync<HttpOperationException>(() => client.CreateNamespacedAsync(
            new V1Pod
            {
                ApiVersion = V1Pod.KubeApiVersion,
                Kind = V1Pod.KubeKind,
                Metadata = new V1ObjectMeta { Name = "invalid-pod-contract", NamespaceProperty = "default" },
                Spec = new V1PodSpec { Containers = [new V1Container { Name = "app" }] },
            },
            "default",
            TestContext.Current.CancellationToken));

        exception.Response.StatusCode.ShouldBe(HttpStatusCode.UnprocessableEntity);
        var status = JsonNode.Parse(exception.Response.Content)!.AsObject();
        status["kind"]!.GetValue<string>().ShouldBe("Status");
        status["status"]!.GetValue<string>().ShouldBe("Failure");
        status["reason"]!.GetValue<string>().ShouldBe("Invalid");
        status["code"]!.GetValue<int>().ShouldBe(422);
        status["details"]!["causes"]!.AsArray()
            .ShouldContain(cause => cause!["field"]!.GetValue<string>() == "spec.containers[0].image");
    }
}
