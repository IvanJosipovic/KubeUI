using System.Text.Json;
using k8s;
using k8s.KubeConfigModels;
using k8s.Models;
using KubeUI.Kubernetes.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using YamlDotNet.Core;

namespace KubeUI.Kubernetes.Tests.Configuration;

public sealed class KubernetesYamlTests : IDisposable
{
    private readonly ServiceProvider _services;
    private readonly IKubernetesYamlSerializer _serializer;

    public KubernetesYamlTests()
    {
        _services = new ServiceCollection()
            .AddKubeUIKubernetesServices(jsonAllowReflection: false)
            .BuildServiceProvider();
        _serializer = _services.GetRequiredService<IKubernetesYamlSerializer>();
    }

    public void Dispose() => _services.Dispose();

    [Fact]
    public void pod_round_trips_generated_json_metadata_and_kubernetes_converters()
    {
        V1Pod pod = new()
        {
            ApiVersion = "v1",
            Kind = "Pod",
            Metadata = new V1ObjectMeta { Name = "metadata-pod", NamespaceProperty = "default" },
            Spec = new V1PodSpec
            {
                Containers =
                [
                    new V1Container
                    {
                        Name = "app",
                        Image = "nginx:1.27",
                        Resources = new V1ResourceRequirements
                        {
                            Limits = new Dictionary<string, ResourceQuantity> { ["memory"] = new("128Mi") },
                        },
                    },
                ],
            },
        };

        var yaml = _serializer.Serialize(pod);
        yaml.ShouldContain("name: metadata-pod");
        var restored = _serializer.Deserialize<V1Pod>(yaml);

        restored.Metadata.Name.ShouldBe("metadata-pod");
        restored.Spec.Containers.ShouldHaveSingleItem().Image.ShouldBe("nginx:1.27");
        restored.Spec.Containers[0].Resources.Limits["memory"].ToString().ShouldBe("128Mi");
    }

    [Fact]
    public void runtime_type_deserialization_uses_the_resolved_json_contract_directly()
    {
        const string yaml = """
            apiVersion: v1
            kind: Pod
            metadata:
              name: runtime-type-pod
            """;

        var restored = _serializer.Deserialize(yaml, typeof(V1Pod)).ShouldBeOfType<V1Pod>();

        restored.Metadata.Name.ShouldBe("runtime-type-pod");
    }

    [Fact]
    public void secret_round_trips_binary_data()
    {
        V1Secret secret = new()
        {
            ApiVersion = "v1",
            Kind = "Secret",
            Metadata = new V1ObjectMeta { Name = "test-secret" },
            Data = new Dictionary<string, byte[]> { ["payload"] = [0, 1, 2, 255] },
        };

        var restored = _serializer.Deserialize<V1Secret>(_serializer.Serialize(secret));

        restored.Metadata.Name.ShouldBe("test-secret");
        restored.Data["payload"].ShouldBe([0, 1, 2, 255]);
    }

    [Fact]
    public void generic_resource_round_trips_json_extension_data()
    {
        using var spec = JsonDocument.Parse("""{"replicas":3,"settings":{"enabled":true,"values":[1,"two",null]}}""");
        GenericKubernetesObject resource = new()
        {
            ApiVersion = "example.com/v1",
            Kind = "Widget",
            Metadata = new V1ObjectMeta { Name = "test" },
            Properties = new Dictionary<string, JsonElement> { ["spec"] = spec.RootElement.Clone() },
        };

        var restored = _serializer.Deserialize<GenericKubernetesObject>(_serializer.Serialize(resource));

        restored.ApiVersion.ShouldBe("example.com/v1");
        restored.Kind.ShouldBe("Widget");
        restored.Metadata.Name.ShouldBe("test");
        restored.Properties["spec"].GetProperty("replicas").GetInt32().ShouldBe(3);
        restored.Properties["spec"].GetProperty("settings").GetProperty("enabled").GetBoolean().ShouldBeTrue();
    }

    [Fact]
    public void strict_mode_rejects_unknown_model_fields_while_lenient_mode_ignores_them()
    {
        const string yaml = """
            apiVersion: v1
            kind: Pod
            metadata:
              name: test
            unknownField: true
            """;

        _serializer.Deserialize<V1Pod>(yaml).Metadata.Name.ShouldBe("test");
        Should.Throw<JsonException>(() => _serializer.Deserialize<V1Pod>(yaml, strict: true));
    }

    [Fact]
    public void lenient_mode_uses_the_last_value_for_duplicate_mapping_keys()
    {
        const string yaml = """
            value: first
            value: second
            """;

        var restored = _serializer.Deserialize<Dictionary<string, object>>(yaml);

        restored["value"].ShouldBe("second");
        Should.Throw<YamlException>(() => _serializer.Deserialize<Dictionary<string, object>>(yaml, strict: true));
    }

    [Fact]
    public void strict_mode_resolves_yaml_merge_keys_and_allows_explicit_overrides()
    {
        const string yaml = """
            apiVersion: v1
            kind: Pod
            metadata:
              name: merge-pod
              labels: &labels
                app: kubeui
                environment: test
              annotations:
                app: override
                <<: *labels
            """;

        var pod = _serializer.Deserialize<V1Pod>(yaml, strict: true);

        pod.Metadata.Labels["app"].ShouldBe("kubeui");
        pod.Metadata.Annotations["environment"].ShouldBe("test");
        pod.Metadata.Annotations["app"].ShouldBe("override");
    }

    [Fact]
    public void strict_parser_rejects_duplicate_yaml_mapping_keys()
    {
        const string yaml = """
            apiVersion: v1
            kind: Pod
            metadata:
              name: first
              name: second
            """;

        Should.Throw<YamlException>(() => _serializer.Deserialize<V1Pod>(yaml, strict: true));
    }

    [Fact]
    public void mixed_kind_documents_are_resolved_and_round_trip_as_a_stream()
    {
        V1Pod pod = new()
        {
            ApiVersion = "v1",
            Kind = "Pod",
            Metadata = new V1ObjectMeta { Name = "multi-pod" },
        };
        V1Namespace ns = new()
        {
            ApiVersion = "v1",
            Kind = "Namespace",
            Metadata = new V1ObjectMeta { Name = "multi-namespace" },
        };

        var yaml = _serializer.SerializeAll([pod, ns]);
        var restored = _serializer.LoadAllFromString(yaml, key => key switch
        {
            "v1/Pod" => typeof(V1Pod),
            "v1/Namespace" => typeof(V1Namespace),
            _ => throw new KeyNotFoundException(key),
        });

        restored.Count.ShouldBe(2);
        restored[0].ShouldBeOfType<V1Pod>().Metadata.Name.ShouldBe("multi-pod");
        restored[1].ShouldBeOfType<V1Namespace>().Metadata.Name.ShouldBe("multi-namespace");
    }

    [Fact]
    public void load_all_skips_empty_and_comment_only_documents_between_resources()
    {
        const string yaml = """
            apiVersion: v1
            kind: ConfigMap
            metadata:
              name: before
            ---
            # Helm templates may emit comment-only documents.
            ---
            apiVersion: v1
            kind: ServiceAccount
            metadata:
              name: after
            """;
        var restored = _serializer.LoadAllFromString(yaml, key => key switch
        {
            "v1/ConfigMap" => typeof(V1ConfigMap),
            "v1/ServiceAccount" => typeof(V1ServiceAccount),
            _ => throw new KeyNotFoundException(key),
        });

        restored.Count.ShouldBe(2);
        restored[0].ShouldBeOfType<V1ConfigMap>().Metadata.Name.ShouldBe("before");
        restored[1].ShouldBeOfType<V1ServiceAccount>().Metadata.Name.ShouldBe("after");
    }

    [Fact]
    public void load_all_requires_kubernetes_document_identity()
    {
        Should.Throw<FormatException>(() => _serializer.LoadAllFromString(
            "metadata:\n  name: missing-identity",
            static _ => typeof(GenericKubernetesObject)));
    }

    [Fact]
    public void kubeconfig_round_trips_through_the_application_json_contract()
    {
        NamedExtension extension = new()
        {
            Name = "sample",
            Extension = new Dictionary<string, object>
            {
                ["enabled"] = true,
                ["attempts"] = 3,
                ["ratio"] = 1.25d,
                ["optional"] = null!,
                ["options"] = new Dictionary<string, object> { ["mode"] = "strict" },
                ["servers"] = new object[] { "one", "two" },
            },
        };
        K8SConfiguration config = new()
        {
            CurrentContext = "dev",
            Contexts = [new Context
            {
                Name = "dev",
                ContextDetails = new ContextDetails { Cluster = "dev", User = "dev" },
                Extensions = [extension],
            }],
            Clusters = [new k8s.KubeConfigModels.Cluster { Name = "dev", ClusterEndpoint = new ClusterEndpoint { Server = "https://example.test" } }],
            Users = [new User { Name = "dev", UserCredentials = new UserCredentials { Token = "token" } }],
            Extensions = [extension],
        };

        var yaml = _serializer.Serialize(config);
        var restored = _serializer.Deserialize<K8SConfiguration>(yaml);

        restored.CurrentContext.ShouldBe("dev");
        restored.Contexts.ShouldHaveSingleItem().ContextDetails.Cluster.ShouldBe("dev");
        restored.Clusters.ShouldHaveSingleItem().ClusterEndpoint.Server.ShouldBe("https://example.test");
        restored.Users.ShouldHaveSingleItem().UserCredentials.Token.ShouldBe("token");
        AssertExtension(restored.Extensions.ShouldHaveSingleItem());
        AssertExtension(restored.Contexts.Single().Extensions.ShouldHaveSingleItem());
    }

    [Fact]
    public void standalone_named_extension_uses_the_application_json_contract()
    {
        NamedExtension extension = new()
        {
            Name = "sample",
            Extension = new Dictionary<string, object>
            {
                ["enabled"] = true,
                ["nested"] = new Dictionary<string, object> { ["mode"] = "strict" },
            },
        };

        var yaml = _serializer.Serialize(extension);
        yaml.ShouldContain("name: sample");
        yaml.ShouldContain("mode: strict");

        var restored = _serializer.Deserialize(yaml, typeof(NamedExtension)).ShouldBeOfType<NamedExtension>();
        var values = ((object)restored.Extension).ShouldBeOfType<Dictionary<object, object>>();
        values["enabled"].ShouldBe(true);
        values["nested"].ShouldBeOfType<Dictionary<object, object>>()["mode"].ShouldBe("strict");
    }

    [Fact]
    public void kubeconfig_file_loader_preserves_relative_path_base()
    {
        using var directory = new TemporaryDirectory();
        var path = Path.Combine(directory.Path, "config");
        File.WriteAllText(path, _serializer.Serialize(new K8SConfiguration { Kind = "Config" }));

        var config = KubeConfigFileLoader.Load(path, _serializer);

        config.FileName.ShouldBe(Path.GetFullPath(path));
    }

    private static void AssertExtension(NamedExtension extension)
    {
        extension.Name.ShouldBe("sample");
        var values = ((object)extension.Extension).ShouldBeOfType<Dictionary<object, object>>();
        values["enabled"].ShouldBe(true);
        values["attempts"].ShouldBe(3L);
        values["ratio"].ShouldBe(1.25d);
        values["optional"].ShouldBeNull();
        var options = values["options"].ShouldBeOfType<Dictionary<object, object>>();
        options["mode"].ShouldBe("strict");
        values["servers"].ShouldBeOfType<List<object>>().ShouldBe(new object[] { "one", "two" });
    }
}
