using KubeUI.Kubernetes;
using KubernetesClient.Informer.Client;
using Shouldly;

namespace KubeUI.Kubernetes.Tests.Models;

public sealed class ClusterModelCatalogCustomResourceTests
{
    [Fact]
    public void Register_UsesSharedJsonModelAndPreservesApiKey()
    {
        var catalog = CreateCatalog();
        var kind = CreateKind();

        catalog.RegisterCustomResourceDefinition(kind);

        catalog.IsCustomResource(kind).ShouldBeTrue();
    }

    [Fact]
    public void TryGet_ResolvesResourceKindFromApiVersionAndKind()
    {
        var catalog = CreateCatalog();
        var kind = CreateKind();
        catalog.RegisterCustomResourceDefinition(kind);

        catalog.TryGetResourceKind("example.com/v1", "Widget", out var resolved).ShouldBeTrue();
        resolved.ShouldBe(kind);
    }

    [Fact]
    public void RegisterByDefinitionName_ReplacesPreviousServedStorageKind()
    {
        var catalog = CreateCatalog();
        var original = CreateKind();
        var updated = new GroupApiVersionKind("example.com", "v2", "Widget", "widgets");

        catalog.RegisterCustomResourceDefinition("widgets.example.com", original).ShouldBeNull();
        catalog.RegisterCustomResourceDefinition("widgets.example.com", updated).ShouldBe(original);

        catalog.IsCustomResource(original).ShouldBeFalse();
        catalog.IsCustomResource(updated).ShouldBeTrue();
    }

    [Fact]
    public void RemoveByDefinitionName_RemovesCurrentKind()
    {
        var catalog = CreateCatalog();
        var kind = CreateKind();
        catalog.RegisterCustomResourceDefinition("widgets.example.com", kind);

        catalog.RemoveCustomResourceDefinition("widgets.example.com").ShouldBe(kind);
        catalog.IsCustomResource(kind).ShouldBeFalse();
    }

    [Fact]
    public void Remove_RemovesJsonModel()
    {
        var catalog = CreateCatalog();
        var kind = CreateKind();
        catalog.RegisterCustomResourceDefinition(kind);

        catalog.RemoveCustomResourceDefinition(kind).ShouldBeTrue();
        catalog.IsCustomResource(kind).ShouldBeFalse();
    }

    private static ClusterModelCatalog CreateCatalog() => new(new KubernetesModelCatalog());

    private static GroupApiVersionKind CreateKind() => new("example.com", "v1", "Widget", "widgets");
}
