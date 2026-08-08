using System.Collections.Frozen;
using System.Xml;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubernetesCRDModelGen;
using KubeUI.Kubernetes.Serialization;
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
    public void GetYamlTypeMapIncludesBuiltInModels()
    {
        var catalog = new ClusterModelCatalog(new KubernetesModelCatalog());

        var map = catalog.GetYamlTypeMap();

        map.ShouldContainKey("v1/Pod");
        map["v1/Pod"].ShouldBe(typeof(V1Pod));
    }

    [Fact]
    public void GetYamlTypeMapReusesFrozenMap()
    {
        var catalog = new ClusterModelCatalog(new KubernetesModelCatalog());

        var firstMap = catalog.GetYamlTypeMap();
        var secondMap = catalog.GetYamlTypeMap();

        firstMap.ShouldBeAssignableTo<FrozenDictionary<string, Type>>();
        secondMap.ShouldBeSameAs(firstMap);
    }

    [Fact]
    public void GetYamlTypeMapIncludesCrdModels()
    {
        var crd = (V1CustomResourceDefinition)KubernetesYaml.LoadAllFromString("""
            apiVersion: apiextensions.k8s.io/v1
            kind: CustomResourceDefinition
            metadata:
              name: widgets.example.com
            spec:
              group: example.com
              names:
                plural: widgets
                singular: widget
                kind: Widget
                listKind: WidgetList
              scope: Namespaced
              versions:
                - name: v1
                  served: true
                  storage: true
                  schema:
                    openAPIV3Schema:
                      type: object
            """)[0];
        var generated = new Generator().GenerateAssembly(crd, "KubeUI.Kubernetes.Tests.Models");
        using var unloadHandle = generated.UnloadHandle;

        generated.Success.ShouldBeTrue();
        generated.Assembly.ShouldNotBeNull();
        generated.XmlDocumentation.ShouldNotBeNull();

        var catalog = new ClusterModelCatalog(new KubernetesModelCatalog());
        var builtInMap = catalog.GetYamlTypeMap();
        catalog.CrdModels.ReplaceCustomResourceDefinition(
            crd,
            generated.Assembly!,
            generated.XmlDocumentation!,
            unloadHandle);

        var crdType = catalog.CrdModels.GetResourceType("example.com", "v1", "Widget");
        crdType.ShouldNotBeNull();
        var map = catalog.GetYamlTypeMap();
        map.ShouldNotBeSameAs(builtInMap);
        map["example.com/v1/Widget"].ShouldBe(crdType);
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
