using System.Collections.Frozen;
using k8s.Models;
using KubeUI.Kubernetes;
using KubeUI.Kubernetes.Serialization;
using Shouldly;
using YamlDotNet.Core;

namespace KubeUI.Kubernetes.Tests.Configuration;

public class KubernetesYamlTests
{
    [Fact]
    public void Deserialize_IgnoresUnknownProperty_WhenStrictIsFalse()
    {
        var yaml = """
            apiVersion: v1
            kind: Pod
            metadata:
              name: test
              namespace: default
            spec:
              unknownField: value
            """.ReplaceLineEndings("\n");

        var pod = KubernetesYaml.Deserialize<V1Pod>(yaml, strict: false);

        pod.ShouldNotBeNull();
        pod.Metadata.Name.ShouldBe("test");
    }

    [Fact]
    public void Deserialize_ThrowsForUnknownProperty_WhenStrictIsTrue()
    {
        var yaml = """
            apiVersion: v1
            kind: Pod
            metadata:
              name: test
              namespace: default
            spec:
              unknownField: value
            """.ReplaceLineEndings("\n");

        Should.Throw<YamlException>(() => KubernetesYaml.Deserialize<V1Pod>(yaml, strict: true));
    }

    [Fact]
    public void Deserialize_ThrowsForDuplicateKeys_WhenStrictIsTrue()
    {
        var yaml = """
            apiVersion: v1
            kind: Pod
            metadata:
              name: test
              name: other
            """.ReplaceLineEndings("\n");

        Should.Throw<YamlException>(() => KubernetesYaml.Deserialize<V1Pod>(yaml, strict: true));
    }

    [Fact]
    public void Deserialize_ByType_UsesStrictMode()
    {
        var yaml = """
            apiVersion: v1
            kind: Pod
            metadata:
              name: test
              namespace: default
            spec:
              unknownField: value
            """.ReplaceLineEndings("\n");

        Should.Throw<YamlException>(() => KubernetesYaml.Deserialize(yaml, typeof(V1Pod), strict: true));
    }

    [Fact]
    public void LoadAllFromStringAcceptsFrozenTypeMap()
    {
        var typeMap = new Dictionary<string, Type>
        {
            ["v1/Pod"] = typeof(V1Pod),
        }.ToFrozenDictionary(StringComparer.Ordinal);

        var objects = KubernetesYaml.LoadAllFromString("""
            apiVersion: v1
            kind: Pod
            metadata:
              name: test
            """.ReplaceLineEndings("\n"), typeMap, strict: true);

        objects.Single().ShouldBeOfType<V1Pod>();
    }

    [Fact]
    public void LoadAllFromStringUsesAllAvailableKubernetesModelTypes()
    {
        var catalog = new KubernetesModelCatalog();
        catalog.Register(
            new KubernetesClient.Informer.Client.GroupApiVersionKind("", "v1", "PodTemplate", "podtemplates"),
            typeof(V1PodTemplate));

        var objects = KubernetesYaml.LoadAllFromString("""
            apiVersion: v1
            kind: PodTemplate
            metadata:
              name: test
            template:
              metadata:
                labels:
                  app: test
            """.ReplaceLineEndings("\n"), catalog.GetYamlTypeMap());

        objects.Single().ShouldBeOfType<V1PodTemplate>();
    }

}
