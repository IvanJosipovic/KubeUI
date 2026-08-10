using System.Xml;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubernetesCRDModelGen;
using KubeUI.Kubernetes.Serialization;
using Shouldly;

namespace KubeUI.Kubernetes.Tests.Models;

public class ClusterCrdModelCatalogTests
{
    [Fact]
    public void AddToCache_PopulatesTypeCache()
    {
        var cache = new ClusterCrdModelCatalog();

        cache.AddToCache(typeof(V1Pod).Assembly, new XmlDocument());

        cache.GetResourceType(string.Empty, V1Pod.KubeApiVersion, V1Pod.KubeKind).ShouldBe(typeof(V1Pod));
        cache.GetResourceType(new GroupApiVersionKind(string.Empty, V1Pod.KubeApiVersion, V1Pod.KubeKind, "pods"))
            .ShouldBe(typeof(V1Pod));
    }

    [Fact]
    public void GetYamlTypeMap_IsLazyAndInvalidatedWhenAssemblyAdded()
    {
        var cache = new ClusterCrdModelCatalog();

        var firstMap = cache.GetYamlTypeMap();
        firstMap.ShouldBeEmpty();

        cache.AddToCache(typeof(V1Pod).Assembly, new XmlDocument());

        var secondMap = cache.GetYamlTypeMap();
        secondMap.ShouldNotBeSameAs(firstMap);
        secondMap["v1/Pod"].ShouldBe(typeof(V1Pod));
    }

    [Fact]
    public void AddToCache_IgnoresDuplicateAssemblies()
    {
        var cache = new ClusterCrdModelCatalog();
        var xml1 = new XmlDocument();
        var xml2 = new XmlDocument();
        xml1.LoadXml($"<doc><members><member name=\"T:{typeof(V1Pod).FullName}\"><summary>first</summary></member></members></doc>");
        xml2.LoadXml($"<doc><members><member name=\"T:{typeof(V1Pod).FullName}\"><summary>second</summary></member></members></doc>");

        cache.AddToCache(typeof(V1Pod).Assembly, xml1);
        cache.AddToCache(typeof(V1Pod).Assembly, xml2);

        cache.GetDocumentation(typeof(V1Pod))?.SelectSingleNode("summary")?.InnerText.ShouldBe("first");
        cache.GetResourceType(string.Empty, V1Pod.KubeApiVersion, V1Pod.KubeKind).ShouldBe(typeof(V1Pod));
    }

    [Fact]
    public void GetDocumentation_ReturnsXmlForCachedMember()
    {
        var cache = CreateModelCache();

        var documentation = cache.GetDocumentation(typeof(V1Pod).GetProperty(nameof(V1Pod.Spec))!);

        documentation.ShouldNotBeNull();
        documentation.SelectSingleNode("summary")?.InnerText.ShouldContain("Specification of the desired behavior of the pod.");
    }

    [Fact]
    public void GetDocumentation_ReturnsXmlForCachedType()
    {
        var cache = CreateModelCache();

        var documentation = cache.GetDocumentation(typeof(V1Pod));

        documentation.ShouldNotBeNull();
        documentation.SelectSingleNode("summary")?.InnerText.ShouldContain("Pod is a collection of containers");
    }

    [Fact]
    public void GetDocumentation_ReturnsXmlForGeneratedNestedCrdMember()
    {
        var cache = new ClusterCrdModelCatalog();
        var xml = new XmlDocument();
        xml.LoadXml($$"""
            <doc>
              <assembly>
                <name>{{typeof(TestDocParent).Assembly.GetName().Name}}</name>
              </assembly>
              <members>
                <member name="P:{{typeof(TestDocParent.TestDocSpec).FullName!.Replace('+', '.')}}.SomeString">
                  <summary>Some string description.</summary>
                </member>
              </members>
            </doc>
            """);

        cache.AddToCache(typeof(TestDocParent).Assembly, xml);

        var nestedProperty = typeof(TestDocParent.TestDocSpec).GetProperty(nameof(TestDocParent.TestDocSpec.SomeString));
        nestedProperty.ShouldNotBeNull();

        var documentation = cache.GetDocumentation(nestedProperty);

        documentation.ShouldNotBeNull();
        documentation.SelectSingleNode("summary")?.InnerText.ShouldContain("Some string description.");
    }

    private static ClusterCrdModelCatalog CreateModelCache()
    {
        var cache = new ClusterCrdModelCatalog();
        var xml = new XmlDocument();
        using var stream = typeof(Generator).Assembly.GetManifestResourceStream("runtime.KubernetesClient.xml");
        stream.ShouldNotBeNull();
        xml.Load(stream);
        cache.AddToCache(typeof(V1Pod).Assembly, xml);
        return cache;
    }

    public sealed class TestDocParent
    {
        public sealed class TestDocSpec
        {
            public string SomeString { get; set; } = string.Empty;
        }
    }

    [Fact]
    public void TestGenerateAssemblyMaterializesPropertyDescriptionsInXmlDocumentation()
    {
        const string modelNamespace = "KubernetesCRDModelGen.Tests.Models";
        var yaml = """
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
          properties:
            apiVersion:
              type: string
            kind:
              type: string
            metadata:
              type: object
            spec:
              type: object
              description: Widget desired state.
              properties:
                size:
                  type: string
                  description: Size of the widget.
                enabled:
                  type: boolean
                  description: Whether the widget is enabled.
""";

        var crd = (V1CustomResourceDefinition)KubernetesYaml.LoadAllFromString(yaml)[0];
        var generator = new Generator();
        var result = generator.GenerateAssembly(crd, modelNamespace);
        using var unloadHandle = result.UnloadHandle;
        var xml = result.XmlDocumentation;

        xml.ShouldNotBeNull();
        xml!.SelectSingleNode("/doc/members/member[@name='P:KubernetesCRDModelGen.Tests.Models.example.com.V1WidgetSpec.Size']/summary")
            ?.InnerText.ShouldBe("Size of the widget.");
        xml.SelectSingleNode("/doc/members/member[@name='P:KubernetesCRDModelGen.Tests.Models.example.com.V1WidgetSpec.Enabled']/summary")
            ?.InnerText.ShouldBe("Whether the widget is enabled.");
        xml.SelectSingleNode("/doc/members/member[@name='P:KubernetesCRDModelGen.Tests.Models.example.com.V1Widget.Spec']/summary")
            ?.InnerText.ShouldBe("Widget desired state.");
    }

    [Fact]
    public void RemoveCustomResourceDefinition_UnloadsGeneratedAssembly()
    {
        const string modelNamespace = "KubernetesCRDModelGen.Tests.Models";
        var yaml = """
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
          properties:
            apiVersion:
              type: string
            kind:
              type: string
            metadata:
              type: object
            spec:
              type: object
              properties:
                size:
                  type: string
""";

        var crd = (V1CustomResourceDefinition)KubernetesYaml.LoadAllFromString(yaml)[0];
        var generator = new Generator();
        var result = generator.GenerateAssembly(crd, modelNamespace);

        result.Success.ShouldBeTrue();
        result.Assembly.ShouldNotBeNull();
        result.XmlDocumentation.ShouldNotBeNull();
        result.UnloadHandle.ShouldNotBeNull();

        var cache = new ClusterCrdModelCatalog();
        cache.ReplaceCustomResourceDefinition(crd, result.Assembly!, result.XmlDocumentation!, result.UnloadHandle);

        cache.GetYamlTypeMap()["example.com/v1/Widget"].ShouldNotBeNull();

        result.UnloadHandle!.IsUnloaded.ShouldBeFalse();

        cache.RemoveCustomResourceDefinition(crd);

        result.UnloadHandle.IsUnloaded.ShouldBeTrue();
        cache.CheckIfCRDExists(crd).ShouldBeFalse();
        cache.GetYamlTypeMap().ShouldNotContainKey("example.com/v1/Widget");
    }

}
