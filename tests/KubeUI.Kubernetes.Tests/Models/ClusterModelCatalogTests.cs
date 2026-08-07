using k8s.Models;
using KubernetesClient.Informer.Client;
using Shouldly;

namespace KubeUI.Kubernetes.Tests.Models;

public sealed class ClusterModelCatalogTests
{
    [Fact]
    public void BuiltInModelLookupsAreAvailableThroughClusterModelCatalog()
    {
        var sharedCatalog = new KubernetesModelCatalog();
        var catalog = new ClusterModelCatalog(sharedCatalog);

        catalog.GetResourceType(string.Empty, V1Pod.KubeApiVersion, V1Pod.KubeKind)
            .ShouldBe(typeof(V1Pod));
        catalog.GetResourceType(new GroupApiVersionKind(string.Empty, V1Pod.KubeApiVersion, V1Pod.KubeKind, "pods"))
            .ShouldBe(typeof(V1Pod));
        catalog.GetDocumentation(typeof(V1Pod)).ShouldNotBeNull();
    }

    [Fact]
    public void ClusterCatalogsShareBuiltInCatalogButKeepSeparateCrdCaches()
    {
        var sharedCatalog = new KubernetesModelCatalog();
        var first = new ClusterModelCatalog(sharedCatalog);
        var second = new ClusterModelCatalog(sharedCatalog);

        first.SharedCatalog.ShouldBeSameAs(sharedCatalog);
        second.SharedCatalog.ShouldBeSameAs(sharedCatalog);
        first.CrdModels.ShouldNotBeSameAs(second.CrdModels);
    }
}
