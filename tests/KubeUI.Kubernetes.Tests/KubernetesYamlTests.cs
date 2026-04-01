using k8s.Models;
using KubeUI.Kubernetes.Serialization;
using Shouldly;
using YamlDotNet.Core;

namespace KubeUI.Kubernetes.Tests;

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
}
