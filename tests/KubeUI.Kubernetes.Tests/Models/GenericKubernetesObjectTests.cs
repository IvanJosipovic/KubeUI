using System.Text.Json;
using k8s;
using KubeUI.Kubernetes;
using KubernetesClient.Informer.Client;
using Shouldly;

namespace KubeUI.Kubernetes.Tests.Models;

public sealed class GenericKubernetesObjectTests
{
    [Fact]
    public void Catalog_RegistersCrdAgainstSharedJsonObjectWithoutAssembly()
    {
        var catalog = new ClusterModelCatalog(new KubernetesModelCatalog());
        var kind = new GroupApiVersionKind("example.com", "v1", "Widget", "widgets");

        catalog.RegisterCustomResourceDefinition(kind);

        catalog.Contains(kind).ShouldBeTrue();
    }

    [Fact]
    public void Parse_PreservesUnknownCrdFieldsAndMetadata()
    {
        var resource = KubernetesJson.Deserialize<GenericKubernetesObject>("""
            {
              "apiVersion": "example.com/v1",
              "kind": "Widget",
              "metadata": { "name": "one", "namespace": "default" },
              "spec": { "size": 3, "enabled": true }
            }
            """);

        resource.ApiVersion.ShouldBe("example.com/v1");
        resource.Kind.ShouldBe("Widget");
        resource.Metadata.Name.ShouldBe("one");
        resource.Properties["spec"].GetProperty("size").GetInt32().ShouldBe(3);
        resource.Properties["spec"].GetProperty("enabled").GetBoolean().ShouldBeTrue();
    }

    [Fact]
    public void JsonExtensionData_RoundTripsCompleteDocument()
    {
        var options = new JsonSerializerOptions();
        var resource = KubernetesJson.Deserialize<GenericKubernetesObject>("{\"apiVersion\":\"example.com/v1\",\"kind\":\"Widget\",\"metadata\":{\"name\":\"one\"},\"spec\":{\"value\":\"kept\"}}");

        var json = JsonSerializer.Serialize(resource, options);
        var roundTripped = JsonSerializer.Deserialize<GenericKubernetesObject>(json, options);

        roundTripped.ShouldNotBeNull();
        roundTripped!.Properties["spec"].GetProperty("value").GetString().ShouldBe("kept");
    }
}
