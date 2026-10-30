using System.Text.Json;
using KubeUI.Avalonia.Features.Helm;
using KubeUI.Kubernetes.Serialization;
using Shouldly;
using YamlDotNet.Core;

namespace KubeUI.Avalonia.Tests.Features.Helm;

public sealed class HelmValuesYamlTests : IDisposable
{
    private readonly KubernetesYamlSerializer _yamlSerializer = new(allowReflection: true);

    public void Dispose() => _yamlSerializer.Dispose();

    [Fact]
    public void Values_roundtrip_preserves_nested_json_types_and_quoted_strings()
    {
        using var source = JsonDocument.Parse("""
            {"replicas":3,"enabled":true,"server":{"name":"true","number":"123","ports":[80,443],"unset":null},"ratio":1.25}
            """);
        var yaml = HelmValuesYaml.Serialize(source.RootElement, _yamlSerializer);
        var parsed = HelmValuesYaml.Parse(yaml, _yamlSerializer);
        using var result = JsonDocument.Parse(k8s.KubernetesJson.Serialize(parsed));

        result.RootElement.GetProperty("replicas").GetInt32().ShouldBe(3);
        result.RootElement.GetProperty("enabled").GetBoolean().ShouldBeTrue();
        result.RootElement.GetProperty("ratio").GetDouble().ShouldBe(1.25);
        var server = result.RootElement.GetProperty("server");
        server.GetProperty("name").GetString().ShouldBe("true");
        server.GetProperty("number").GetString().ShouldBe("123");
        server.GetProperty("ports").EnumerateArray().Select(item => item.GetInt32()).ShouldBe([80, 443]);
        server.GetProperty("unset").ValueKind.ShouldBe(JsonValueKind.Null);
    }

    [Fact]
    public void Values_resolve_anchors_and_merge_keys_with_explicit_overrides()
    {
        var parsed = HelmValuesYaml.Parse("""
            defaults: &defaults
              replicas: 2
              image: nginx
            server:
              <<: *defaults
              image: apache
            """, _yamlSerializer);

        using var result = JsonDocument.Parse(k8s.KubernetesJson.Serialize(parsed));

        var server = result.RootElement.GetProperty("server");
        server.GetProperty("replicas").GetInt32().ShouldBe(2);
        server.GetProperty("image").GetString().ShouldBe("apache");
    }

    [Fact]
    public void Values_reject_duplicate_keys()
    {
        var exception = Should.Throw<YamlException>(() => HelmValuesYaml.Parse("""
            replicas: 1
            replicas: 2
            """, _yamlSerializer));

        exception.Message.ShouldContain("replicas");
    }

    [Fact]
    public void Values_reject_non_mapping_documents()
        => Should.Throw<YamlException>(() => HelmValuesYaml.Parse("- one\n- two", _yamlSerializer));

    [Fact]
    public void Blank_values_parse_as_empty_mapping()
        => HelmValuesYaml.Parse(string.Empty, _yamlSerializer).ShouldBeEmpty();
}
