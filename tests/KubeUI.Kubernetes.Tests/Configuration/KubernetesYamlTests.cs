using System.Collections.Frozen;
using System.Text;
using System.Text.Json;
using k8s.Models;
using KubeUI.Kubernetes.Serialization;
using Shouldly;
using YamlDotNet.Core;

namespace KubeUI.Kubernetes.Tests.Configuration;

[CollectionDefinition(KubernetesYamlCollection.Name, DisableParallelization = true)]
public sealed class KubernetesYamlCollection
{
    public const string Name = "Kubernetes YAML";
}

[Collection(KubernetesYamlCollection.Name)]
public class KubernetesYamlTests
{
    [Fact]
    public void UseStaticContext_TogglesYamlBuildersAndDefaultsToReflection()
    {
        var previousValue = KubernetesYaml.UseStaticContext;
        try
        {
            KubernetesYaml.UseStaticContext = false;
            KubernetesYaml.UseStaticContext.ShouldBeFalse();
            var reflectionDeserializer = KubernetesYaml.GetDeserializer();
            var reflectionSerializer = KubernetesYaml.Serializer;

            KubernetesYaml.UseStaticContext = true;
            var staticDeserializer = KubernetesYaml.GetDeserializer();
            var staticSerializer = KubernetesYaml.Serializer;

            ReferenceEquals(staticDeserializer, reflectionDeserializer).ShouldBeFalse();
            ReferenceEquals(staticSerializer, reflectionSerializer).ShouldBeFalse();

            var pod = KubernetesYaml.Deserialize<V1Pod>("""
                apiVersion: v1
                kind: Pod
                metadata:
                  name: test
                """.ReplaceLineEndings("\n"));
            pod.Metadata.Name.ShouldBe("test");
            KubernetesYaml.Serialize(pod).ShouldContain("name: test");
        }
        finally
        {
            KubernetesYaml.UseStaticContext = previousValue;
        }
    }

    [Fact]
    public void StaticContext_KnowsEveryKubernetesModelCatalogType()
    {
        var catalog = new KubernetesModelCatalog();
        var context = new KubernetesYamlStaticContext();

        foreach (var modelType in catalog.GetYamlTypeMap().Values.Distinct())
        {
            context.IsKnownType(modelType).ShouldBeTrue(modelType.FullName);
        }
    }

    [Fact]
    public void JsonContext_KnowsEveryYamlApiModelExceptListsAndWatchEvents()
    {
        var yamlContext = new KubernetesYamlStaticContext();
        var modelTypes = typeof(V1Pod).Assembly.GetTypes()
            .Where(yamlContext.IsKnownType)
            .Where(IsYamlApiModel)
            .Where(type => !IsListOrWatchModel(type))
            .ToArray();

        var missingModelTypes = modelTypes
            .Where(modelType => KubernetesJsonStaticContext.Default.GetTypeInfo(modelType) is null)
            .Select(modelType => modelType.FullName)
            .ToArray();

        missingModelTypes.ShouldBeEmpty(string.Join(Environment.NewLine, missingModelTypes));

        KubernetesJsonStaticContext.Default.GetTypeInfo(typeof(GenericKubernetesObject))
            .ShouldNotBeNull(nameof(GenericKubernetesObject));
        KubernetesJsonStaticContext.Default.GetTypeInfo(typeof(k8s.KubernetesObject))
            .ShouldNotBeNull(nameof(k8s.KubernetesObject));
        KubernetesJsonStaticContext.Default.GetTypeInfo(typeof(IList<V1EphemeralContainer>))
            .ShouldNotBeNull(nameof(IList<V1EphemeralContainer>));
    }

    private static bool IsYamlApiModel(Type type)
    {
        return type.Namespace == "k8s.Models";
    }

    private static bool IsListOrWatchModel(Type type)
    {
        return type.Name.EndsWith("List", StringComparison.Ordinal)
            || type.Name.Contains("WatchEvent", StringComparison.Ordinal);
    }

    [Fact]
    public void GenericKubernetesObject_RoundTripsJsonExtensionDataThroughYaml()
    {
        using var specDocument = JsonDocument.Parse("""{"replicas":3,"settings":{"enabled":true,"values":[1,"two",null]}}""");
        var resource = new GenericKubernetesObject
        {
            ApiVersion = "example.com/v1",
            Kind = "Widget",
            Metadata = new V1ObjectMeta { Name = "test" },
            Properties = new Dictionary<string, JsonElement>
            {
                ["spec"] = specDocument.RootElement.Clone(),
            },
        };

        var yaml = KubernetesYaml.Serialize(resource);
        var restored = KubernetesYaml.Deserialize<GenericKubernetesObject>(yaml);

        restored.ShouldNotBeNull();
        restored.Properties["spec"].GetProperty("replicas").GetInt32().ShouldBe(3);
        restored.Properties["spec"].GetProperty("settings").GetProperty("enabled").GetBoolean().ShouldBeTrue();
        restored.Properties["spec"].GetProperty("settings").GetProperty("values")[1].GetString().ShouldBe("two");
        restored.Properties["spec"].GetProperty("settings").GetProperty("values")[2].ValueKind.ShouldBe(JsonValueKind.Null);
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void KubeConfigNamedExtension_RoundTripsDynamicExtensionValues(bool useStaticContext)
    {
        using var contextScope = new StaticContextScope(useStaticContext);
        using var jsonDocument = JsonDocument.Parse("""{"items":["first",2]}""");
        var extension = new k8s.KubeConfigModels.NamedExtension
        {
            Name = "sample",
            Extension = new Dictionary<string, object>
            {
                ["enabled"] = true,
                ["disabled"] = false,
                ["attempts"] = 3,
                ["fraction"] = 1.25d,
                ["stringBoolean"] = "true",
                ["stringInteger"] = "3",
                ["stringFraction"] = "1.5",
                ["stringNull"] = "null",
                ["stringEmpty"] = "",
                ["optional"] = null!,
                ["options"] = new Dictionary<string, object> { ["mode"] = "strict" },
                ["servers"] = new object[] { "one", "two" },
                ["json"] = jsonDocument.RootElement.Clone(),
            },
        };

        var yaml = KubernetesYaml.Serialize(extension);
        var restored = KubernetesYaml.Deserialize<k8s.KubeConfigModels.NamedExtension>(yaml);

        restored.Name.ShouldBe("sample");
        var restoredExtension = (Dictionary<object, object>)restored.Extension;
        restoredExtension.ShouldContainKey("enabled");
        restoredExtension["enabled"].ShouldBe(true);
        restoredExtension["disabled"].ShouldBe(false);
        restoredExtension["attempts"].ShouldBe(3L);
        restoredExtension["fraction"].ShouldBe(1.25d);
        restoredExtension["stringBoolean"].ShouldBeOfType<string>().ShouldBe("true");
        restoredExtension["stringInteger"].ShouldBeOfType<string>().ShouldBe("3");
        restoredExtension["stringFraction"].ShouldBeOfType<string>().ShouldBe("1.5");
        restoredExtension["stringNull"].ShouldBeOfType<string>().ShouldBe("null");
        restoredExtension["stringEmpty"].ShouldBeOfType<string>().ShouldBeEmpty();
        restoredExtension["optional"].ShouldBeNull();
        ((Dictionary<object, object>)restoredExtension["options"])["mode"].ShouldBe("strict");
        ((List<object>)restoredExtension["servers"]).ShouldBe(new object[] { "one", "two" });
        ((Dictionary<object, object>)restoredExtension["json"])["items"].ShouldBeOfType<List<object>>();
    }

    [Fact]
    public void KubeConfigNamedExtension_IgnoresUnknownFields()
    {
        using var staticContext = new StaticContextScope();
        var yaml = """
            name: sample
            extension: true
            futureField: ignored
            """.ReplaceLineEndings("\n");

        var restored = KubernetesYaml.Deserialize<k8s.KubeConfigModels.NamedExtension>(yaml, strict: true);

        restored.Name.ShouldBe("sample");
        ((object)restored.Extension).ShouldBe(true);
    }

    [Fact]
    public void KubeConfigNamedExtension_RejectsNonStringExtensionMapKeys()
    {
        using var staticContext = new StaticContextScope();
        var extension = new k8s.KubeConfigModels.NamedExtension
        {
            Name = "sample",
            Extension = new Dictionary<object, object> { [1] = "invalid" },
        };

        Should.Throw<YamlException>(() => KubernetesYaml.Serialize(extension));
    }

    [Fact]
    public void KubeConfigNamedExtension_RejectsUnsupportedExtensionValues()
    {
        using var staticContext = new StaticContextScope();
        var extension = new k8s.KubeConfigModels.NamedExtension
        {
            Name = "sample",
            Extension = new object(),
        };

        Should.Throw<YamlException>(() => KubernetesYaml.Serialize(extension));
    }

    [Fact]
    public void ResourceQuantityConverter_RoundTripsPodResourceLimits()
    {
        var pod = new V1Pod
        {
            Metadata = new V1ObjectMeta { Name = "resource-limits-test" },
            Spec = new V1PodSpec
            {
                Containers =
                [
                    new V1Container
                    {
                        Name = "app",
                        Resources = new V1ResourceRequirements
                        {
                            Limits = new Dictionary<string, ResourceQuantity>
                            {
                                ["cpu"] = new ResourceQuantity(500m, -3, ResourceQuantity.SuffixFormat.DecimalSI),
                            },
                        },
                    },
                ],
            },
        };

        var yaml = KubernetesYaml.Serialize(pod);
        var restored = KubernetesYaml.Deserialize<V1Pod>(yaml);

        restored.Spec.Containers[0].Resources.Limits["cpu"].ToString().ShouldBe("500m");
    }

    [Fact]
    public void KubernetesYaml_RoundTripsSecretBinaryData()
    {
        var secret = new V1Secret
        {
            Metadata = new V1ObjectMeta { Name = "credentials" },
            Type = "Opaque",
            Data = new Dictionary<string, byte[]>
            {
                ["token"] = [0, 1, 127, 255],
                ["empty"] = [],
            },
        };

        var yaml = KubernetesYaml.Serialize(secret);
        var restored = KubernetesYaml.Deserialize<V1Secret>(yaml, strict: true);

        restored.Data["token"].ShouldBe(new byte[] { 0, 1, 127, 255 });
        restored.Data["empty"].ShouldBeEmpty();
        yaml.ShouldContain("AAF//w==");
    }

    [Fact]
    public void KubernetesYaml_RejectsInvalidSecretBinaryData()
    {
        var yaml = """
            apiVersion: v1
            kind: Secret
            metadata:
              name: credentials
            data:
              token: not-base64!
            """.ReplaceLineEndings("\n");

        Should.Throw<YamlException>(() => KubernetesYaml.Deserialize<V1Secret>(yaml, strict: true));
    }

    [Fact]
    public void KubernetesYaml_RejectsNonScalarSecretBinaryData()
    {
        var yaml = """
            apiVersion: v1
            kind: Secret
            metadata:
              name: credentials
            data:
              token:
                nested: value
            """.ReplaceLineEndings("\n");

        Should.Throw<YamlException>(() => KubernetesYaml.Deserialize<V1Secret>(yaml, strict: true));
    }

    [Fact]
    public void KubernetesYaml_HandlesNullBinaryScalarAsEmptyBytes()
    {
        using var writer = new StringWriter();
        var emitter = new YamlDotNet.Core.Emitter(writer);
        emitter.Emit(new YamlDotNet.Core.Events.StreamStart());
        emitter.Emit(new YamlDotNet.Core.Events.DocumentStart());

        KubernetesYaml.Serializer.SerializeValue(emitter, null, typeof(byte[]));
        emitter.Emit(new YamlDotNet.Core.Events.DocumentEnd(true));
        emitter.Emit(new YamlDotNet.Core.Events.StreamEnd());

        KubernetesYaml.Deserialize<byte[]>(writer.ToString(), strict: true).ShouldBeEmpty();
    }

    [Fact]
    public void KubernetesYaml_PreservesYamlLikeStringsInStringDictionaries()
    {
        var configMap = new V1ConfigMap
        {
            Metadata = new V1ObjectMeta { Name = "app-settings" },
            Data = new Dictionary<string, string>
            {
                ["boolean"] = "true",
                ["integer"] = "3",
                ["null"] = "null",
                ["colon"] = "a: b",
            },
        };

        var yaml = KubernetesYaml.Serialize(configMap);
        var restored = KubernetesYaml.Deserialize<V1ConfigMap>(yaml, strict: true);

        restored.Data.ShouldBe(configMap.Data);
    }

    [Fact]
    public void KubernetesYaml_RoundTripsServiceTargetPortAsNumberAndName()
    {
        var service = new V1Service
        {
            Metadata = new V1ObjectMeta { Name = "web" },
            Spec = new V1ServiceSpec
            {
                Ports =
                [
                    new V1ServicePort { Name = "http", Port = 80, TargetPort = 8080 },
                    new V1ServicePort { Name = "metrics", Port = 9090, TargetPort = "metrics" },
                ],
            },
        };

        var yaml = KubernetesYaml.Serialize(service);
        var restored = KubernetesYaml.Deserialize<V1Service>(yaml, strict: true);

        restored.Spec.Ports[0].TargetPort.ToString().ShouldBe("8080");
        restored.Spec.Ports[1].TargetPort.ToString().ShouldBe("metrics");
    }

    [Fact]
    public void KubernetesYaml_RejectsInvalidResourceQuantity()
    {
        Should.Throw<YamlException>(() => KubernetesYaml.Deserialize<ResourceQuantity>("not-a-quantity", strict: true));
    }

    [Fact]
    public void KubernetesYaml_RejectsEmptyResourceQuantityInResourceMap()
    {
        using var staticContext = new StaticContextScope();
        var yaml = """
            apiVersion: v1
            kind: Pod
            metadata:
              name: invalid-quantity
            spec:
              containers:
                - name: app
                  resources:
                    limits:
                      cpu: ""
            """.ReplaceLineEndings("\n");

        Should.Throw<YamlException>(() => KubernetesYaml.Deserialize<V1Pod>(yaml, strict: true));
    }

    private sealed class StaticContextScope : IDisposable
    {
        private readonly bool _previousValue = KubernetesYaml.UseStaticContext;

        public StaticContextScope(bool useStaticContext = true)
        {
            KubernetesYaml.UseStaticContext = useStaticContext;
        }

        public void Dispose()
        {
            KubernetesYaml.UseStaticContext = _previousValue;
        }
    }

    [Fact]
    public void KubernetesYaml_RoundTripsKubernetesTimestamp()
    {
        var timestamp = new DateTime(2025, 1, 2, 3, 4, 5, DateTimeKind.Utc);
        var resource = new V1Namespace
        {
            Metadata = new V1ObjectMeta { Name = "timestamp-test", CreationTimestamp = timestamp },
        };

        var yaml = KubernetesYaml.Serialize(resource);
        var restored = KubernetesYaml.Deserialize<V1Namespace>(yaml, strict: true);

        restored.Metadata.CreationTimestamp.ShouldBe(timestamp);
    }

    [Fact]
    public void KubernetesYaml_RoundTripsDateTimeOffsetScalar()
    {
        var timestamp = new DateTimeOffset(2025, 1, 2, 3, 4, 5, TimeSpan.FromHours(-7));

        var yaml = KubernetesYaml.Serialize(timestamp);
        var restored = KubernetesYaml.Deserialize<DateTimeOffset>(yaml, strict: true);

        restored.ShouldBe(timestamp);
    }

    [Fact]
    public void KubernetesYaml_RejectsInvalidDateTimeOffsetScalars()
    {
        Should.Throw<YamlException>(() => KubernetesYaml.Deserialize<DateTimeOffset>("not-a-timestamp", strict: true));
        Should.Throw<YamlException>(() => KubernetesYaml.Deserialize<DateTimeOffset>("timestamp: 2025-01-02", strict: true));
    }

    [Fact]
    public void LoadAllFromString_DeserializesMultipleStrictDocuments()
    {
        var typeMap = new Dictionary<string, Type>
        {
            ["v1/ConfigMap"] = typeof(V1ConfigMap),
            ["v1/Secret"] = typeof(V1Secret),
        };
        var yaml = """
            apiVersion: v1
            kind: ConfigMap
            metadata:
              name: app-settings
            data:
              enabled: "true"
              retries: "3"
            ---
            apiVersion: v1
            kind: Secret
            metadata:
              name: credentials
            type: Opaque
            data:
              token: dG9rZW4=
            """.ReplaceLineEndings("\n");

        var resources = KubernetesYaml.LoadAllFromString(yaml, typeMap, strict: true);

        resources.Count.ShouldBe(2);
        var configMap = resources[0].ShouldBeOfType<V1ConfigMap>();
        configMap.Data["enabled"].ShouldBe("true");
        configMap.Data["retries"].ShouldBe("3");
        var secret = resources[1].ShouldBeOfType<V1Secret>();
        Encoding.UTF8.GetString(secret.Data["token"]).ShouldBe("token");
    }

    [Fact]
    public async Task SerializeAll_LoadAllFromStreamAsync_RoundTripsMultipleResources()
    {
        var resources = new object[]
        {
            new V1Namespace { ApiVersion = "v1", Kind = "Namespace", Metadata = new V1ObjectMeta { Name = "team-a" } },
            new V1ConfigMap
            {
                ApiVersion = "v1",
                Kind = "ConfigMap",
                Metadata = new V1ObjectMeta { Name = "settings", NamespaceProperty = "team-a" },
                Data = new Dictionary<string, string> { ["enabled"] = "true" },
            },
        };
        var typeMap = new Dictionary<string, Type>
        {
            ["v1/Namespace"] = typeof(V1Namespace),
            ["v1/ConfigMap"] = typeof(V1ConfigMap),
        };
        var yaml = KubernetesYaml.SerializeAll(resources);
        await using var stream = new MemoryStream(Encoding.UTF8.GetBytes(yaml));

        var restored = await KubernetesYaml.LoadAllFromStreamAsync(stream, typeMap, strict: true);

        restored.Count.ShouldBe(2);
        restored[0].ShouldBeOfType<V1Namespace>().Metadata.Name.ShouldBe("team-a");
        restored[1].ShouldBeOfType<V1ConfigMap>().Data["enabled"].ShouldBe("true");
    }

    [Fact]
    public void PodYaml_RoundTripsCalicoContainerIdAnnotation()
    {
        const string annotationKey = "cni.projectcalico.org/containerID";
        const string containerId = "1f38475382e1f66aff44a9cf15ab057b7303637def73b0b20423aaedd97f3886";
        var pod = new V1Pod
        {
            ApiVersion = "v1",
            Kind = "Pod",
            Metadata = new V1ObjectMeta
            {
                Annotations = new Dictionary<string, string> { [annotationKey] = containerId },
            },
        };

        var yaml = KubernetesYaml.Serialize(pod);
        var restored = KubernetesYaml.Deserialize<V1Pod>(yaml);

        restored.Metadata.Annotations[annotationKey].ShouldBe(containerId);
        yaml.ShouldContain($"{annotationKey}: {containerId}");
    }

    [Fact]
    public void Deserialize_PodExpandsYamlMergeKeysInMetadataMaps()
    {
        var yaml = """
            apiVersion: v1
            kind: Pod
            metadata:
              name: merged-metadata
              labels: &commonLabels
                app: kubeui
                environment: test
              annotations:
                <<: *commonLabels
                cni.projectcalico.org/containerID: 0123456789abcdef
            """.ReplaceLineEndings("\n");

        var pod = KubernetesYaml.Deserialize<V1Pod>(yaml, strict: true);

        pod.Metadata.Labels["app"].ShouldBe("kubeui");
        pod.Metadata.Labels["environment"].ShouldBe("test");
        pod.Metadata.Annotations["app"].ShouldBe("kubeui");
        pod.Metadata.Annotations["cni.projectcalico.org/containerID"].ShouldBe("0123456789abcdef");
    }

    [Fact]
    public void JsonBackedYamlConverter_RoundTripsPodSecurityContext()
    {
        var pod = new V1Pod
        {
            Metadata = new V1ObjectMeta { Name = "security-context-test" },
            Spec = new V1PodSpec
            {
                SecurityContext = new V1PodSecurityContext { RunAsUser = 1000 },
            },
        };

        var yaml = KubernetesYaml.Serialize(pod);
        var restored = KubernetesYaml.Deserialize<V1Pod>(yaml);

        restored.Spec.SecurityContext.RunAsUser.ShouldBe(1000);
    }

    [Fact]
    public void JsonBackedYamlConverter_RoundTripsContainerRestartExitCodes()
    {
        var pod = new V1Pod
        {
            Metadata = new V1ObjectMeta { Name = "restart-rules-test" },
            Spec = new V1PodSpec
            {
                Containers =
                [
                    new V1Container
                    {
                        Name = "app",
                        RestartPolicyRules =
                        [
                            new V1ContainerRestartRule
                            {
                                Action = "Restart",
                                ExitCodes = new V1ContainerRestartRuleOnExitCodes
                                {
                                    OperatorProperty = "In",
                                    Values = [7, 9],
                                },
                            },
                        ],
                    },
                ],
            },
        };

        var yaml = KubernetesYaml.Serialize(pod);
        var restored = KubernetesYaml.Deserialize<V1Pod>(yaml);

        restored.Spec.Containers[0].RestartPolicyRules[0].ExitCodes.Values.ShouldBe(new int?[] { 7, 9 });
    }

    [Fact]
    public void JsonBackedYamlConverter_RoundTripsLinuxContainerUser()
    {
        var pod = new V1Pod
        {
            Metadata = new V1ObjectMeta { Name = "container-user-test" },
            Status = new V1PodStatus
            {
                ContainerStatuses =
                [
                    new V1ContainerStatus
                    {
                        Name = "app",
                        User = new V1ContainerUser
                        {
                            Linux = new V1LinuxContainerUser { Uid = 1000, Gid = 2000 },
                        },
                    },
                ],
            },
        };

        var yaml = KubernetesYaml.Serialize(pod);
        var restored = KubernetesYaml.Deserialize<V1Pod>(yaml);

        restored.Status.ContainerStatuses[0].User.Linux.Uid.ShouldBe(1000);
        restored.Status.ContainerStatuses[0].User.Linux.Gid.ShouldBe(2000);
    }

    [Fact]
    public void JsonBackedYamlConverter_RoundTripsStatusListMetadata()
    {
        var metadata = new V1ListMeta { ResourceVersion = "42" };

        var yaml = KubernetesYaml.Serialize(metadata);
        var restored = KubernetesYaml.Deserialize<V1ListMeta>(yaml);

        restored.ResourceVersion.ShouldBe("42");
    }

    [Fact]
    public void JsonBackedYamlConverter_RoundTripsPodDisruptionBudgetStatus()
    {
        var pdb = new V1PodDisruptionBudget
        {
            Metadata = new V1ObjectMeta { Name = "pdb-test" },
            Status = new V1PodDisruptionBudgetStatus
            {
                CurrentHealthy = 2,
                DesiredHealthy = 3,
                DisruptionsAllowed = 1,
                ExpectedPods = 4,
            },
        };

        var yaml = KubernetesYaml.Serialize(pdb);
        var restored = KubernetesYaml.Deserialize<V1PodDisruptionBudget>(yaml);

        restored.Status.CurrentHealthy.ShouldBe(2);
        restored.Status.DesiredHealthy.ShouldBe(3);
        restored.Status.DisruptionsAllowed.ShouldBe(1);
        restored.Status.ExpectedPods.ShouldBe(4);
    }

    [Fact]
    public void CustomResourceDefinitionYaml_PreservesJsonExtensionData()
    {
        var resource = KubernetesYaml.Deserialize<GenericKubernetesObject>(
            KubeUI.Testing.Kubernetes.Fixtures.KubernetesTestData.CustomResourceDefinitionYaml);

        resource.Properties.Keys.ShouldContain("spec", string.Join(",", resource.Properties.Keys));
        resource.Properties["spec"].GetProperty("group").GetString().ShouldBe("kubeui.com");
        resource.Properties["spec"].GetProperty("versions")[0].GetProperty("name").GetString().ShouldBe("v1beta1");
        resource.Properties["spec"].GetProperty("names").GetProperty("plural").GetString().ShouldBe("tests");
    }

    [Fact]
    public void KubeConfig_RoundTripsConnectionAndCredentialFields()
    {
        var config = new k8s.KubeConfigModels.K8SConfiguration
        {
            CurrentContext = "sample",
            Clusters = [new k8s.KubeConfigModels.Cluster
            {
                Name = "cluster",
                ClusterEndpoint = new k8s.KubeConfigModels.ClusterEndpoint { Server = "https://127.0.0.1" },
            }],
            Contexts = [new k8s.KubeConfigModels.Context
            {
                Name = "sample",
                ContextDetails = new k8s.KubeConfigModels.ContextDetails
                {
                    Cluster = "cluster",
                    User = "user",
                    Namespace = "my-app",
                },
            }],
            Users = [new k8s.KubeConfigModels.User
            {
                Name = "user",
                UserCredentials = new k8s.KubeConfigModels.UserCredentials { Token = "fake-token" },
            }],
        };

        var yaml = KubernetesYaml.Serialize(config);
        var restored = KubernetesYaml.Deserialize<k8s.KubeConfigModels.K8SConfiguration>(yaml);

        restored.CurrentContext.ShouldBe("sample");
        restored.Clusters.Single().ClusterEndpoint.Server.ShouldBe("https://127.0.0.1");
        restored.Contexts.Single().ContextDetails.Cluster.ShouldBe("cluster");
        restored.Contexts.Single().ContextDetails.Namespace.ShouldBe("my-app");
        restored.Users.Single().UserCredentials.Token.ShouldBe("fake-token");
    }

    [Fact]
    public void LoadAllFromString_PreservesRbacRulesForTrimmedDeserializer()
    {
        var typeMap = new Dictionary<string, Type>
        {
            ["v1/Namespace"] = typeof(V1Namespace),
            ["v1/ServiceAccount"] = typeof(V1ServiceAccount),
            ["v1/Secret"] = typeof(V1Secret),
            ["rbac.authorization.k8s.io/v1/ClusterRole"] = typeof(V1ClusterRole),
            ["rbac.authorization.k8s.io/v1/ClusterRoleBinding"] = typeof(V1ClusterRoleBinding),
            ["rbac.authorization.k8s.io/v1/RoleBinding"] = typeof(V1RoleBinding),
        };
        var resources = KubernetesYaml.LoadAllFromString(
            KubeUI.Testing.Kubernetes.Fixtures.KubernetesTestData.LimitedAccessWithNamespaceFallback,
            typeMap);

        var developer = resources.OfType<k8s.Models.V1ClusterRole>().Single(role => role.Name() == "developer");
        developer.Rules.ShouldNotBeEmpty();
        developer.Rules.ShouldContain(rule => rule.Verbs.Contains("delete"));
        var binding = resources.OfType<V1RoleBinding>().Single(item => item.Name() == "my-serviceaccount-developer");
        binding.RoleRef.Name.ShouldBe("developer");
        string.Join("|", binding.Subjects.Select(subject => $"{subject.Kind}:{subject.Name}:{subject.NamespaceProperty}"))
            .ShouldBe("ServiceAccount:my-serviceaccount:my-app");
    }

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
