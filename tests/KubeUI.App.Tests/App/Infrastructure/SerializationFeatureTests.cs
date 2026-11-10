using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using Avalonia.Headless.XUnit;
using k8s;
using k8s.Models;
using KubeUI.Desktop;
using KubeUI.Features.Resources;
using KubeUI.Kubernetes.Serialization;
using Microsoft.Extensions.Hosting;
using Shouldly;

namespace KubeUI.App.Tests.App.Infrastructure;

public sealed class SerializationFeatureTests
{
    [Fact]
    public void source_generation_contexts_do_not_register_duplicate_root_types()
    {
        var kubeUiTypes = GetExplicitMetadataTypes(KubernetesJsonStaticContext.Default);
        kubeUiTypes.ShouldNotBeEmpty();

        // The SDK context exposes transitive metadata too; KubeUI should only root types it does not already cover.
        var duplicates = kubeUiTypes
            .Where(type => SourceGenerationContext.Default.GetTypeInfo(type) is not null)
            .OrderBy(type => type.FullName, StringComparer.Ordinal)
            .ToArray();

        duplicates.ShouldBeEmpty(
            $"KubeUI roots types already covered by KubernetesClient metadata: {string.Join(", ", duplicates.Select(type => type.FullName))}");
    }

    [AvaloniaFact]
    public async Task generated_json_metadata_covers_registered_resource_types_for_json_and_yaml()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        using var host = CreateHost(disableJsonReflection: true);
        var jsonSerializer = host.Services.GetRequiredService<IKubernetesJsonSerializer>();
        var yamlSerializer = host.Services.GetRequiredService<IKubernetesYamlSerializer>();
        KubernetesClientConfiguration configuration = new();
        jsonSerializer.ConfigureClient(configuration);
        var resources = workspace.GetResourceConfigs();
        resources.ShouldNotBeEmpty();

        foreach (var registered in Application.Current.GetRequiredTestService<IEnumerable<IResourceConfig>>())
        {
            if (!registered.IsCustomResource)
            {
                resources.ShouldContain(resource => resource.Kind == registered.Kind);
            }
        }

        foreach (var resource in resources)
        {
            workspace.Runtime.ModelCatalog.TryGetResourceType(resource.Kind, out var modelType)
                .ShouldBeTrue($"No resource model registered for {resource.Kind}.");
            var metadata = configuration.JsonSerializerOptions.GetTypeInfo(modelType);
            var model = JsonSerializer.Deserialize("{}", metadata)
                .ShouldBeAssignableTo<IKubernetesObject<V1ObjectMeta>>();
            model.ApiVersion = string.IsNullOrEmpty(resource.Kind.Group)
                ? resource.Kind.ApiVersion
                : $"{resource.Kind.Group}/{resource.Kind.ApiVersion}";
            model.Kind = resource.Kind.Kind;
            model.Metadata = new V1ObjectMeta { Name = "metadata-coverage-probe" };

            var jsonRestored = JsonSerializer.Deserialize(jsonSerializer.Serialize(model), metadata)
                .ShouldBeAssignableTo<IKubernetesObject<V1ObjectMeta>>();
            var yamlRestored = yamlSerializer.Deserialize(yamlSerializer.Serialize(model), modelType)
                .ShouldBeAssignableTo<IKubernetesObject<V1ObjectMeta>>();
            AssertProbe(model, jsonRestored);
            AssertProbe(model, yamlRestored);
        }
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void startup_settings_control_json_and_yaml_reflection_together(bool disableJsonReflection)
    {
        using var host = CreateHost(disableJsonReflection);
        AssertCapabilities(host, disableJsonReflection);
    }

    [Fact]
    public void default_settings_enable_reflection()
    {
        using var host = Program.CreateHostBuilder(
            [], includeOptionalServices: false, settingsOverride: new SettingsPersistenceData()).Build();
        AssertCapabilities(host, disableJsonReflection: false);
    }

    [Fact]
    public void application_instances_keep_independent_serialization_settings()
    {
        SettingsPersistenceData settings = new();
        using var reflectionHost = Program.CreateHostBuilder(
            [], includeOptionalServices: false, settingsOverride: settings).Build();
        settings.Settings.PreviewFeatures.DisableJsonSerializationReflection = true;
        using var generatedHost = Program.CreateHostBuilder(
            [], includeOptionalServices: false, settingsOverride: settings).Build();

        AssertCapabilities(reflectionHost, disableJsonReflection: false);
        AssertCapabilities(generatedHost, disableJsonReflection: true);
        AssertCapabilities(reflectionHost, disableJsonReflection: false);
    }

    private static IHost CreateHost(bool disableJsonReflection)
    {
        SettingsPersistenceData settings = new();
        settings.Settings.PreviewFeatures.DisableJsonSerializationReflection = disableJsonReflection;
        return Program.CreateHostBuilder([], includeOptionalServices: false, settingsOverride: settings).Build();
    }

    private static HashSet<Type> GetExplicitMetadataTypes(JsonSerializerContext context)
        => context.GetType()
            .GetCustomAttributesData()
            .Where(attribute => attribute.AttributeType == typeof(JsonSerializableAttribute))
            .Select(attribute => attribute.ConstructorArguments[0].Value)
            .OfType<Type>()
            .ToHashSet();

    private static void AssertCapabilities(IHost host, bool disableJsonReflection)
    {
        var jsonSerializer = host.Services.GetRequiredService<IKubernetesJsonSerializer>();
        var yamlSerializer = host.Services.GetRequiredService<IKubernetesYamlSerializer>();
        KubernetesClientConfiguration clientConfiguration = new();
        jsonSerializer.ConfigureClient(clientConfiguration);
        var resource = CreateResource();
        V1Pod pod = new()
        {
            ApiVersion = V1Pod.KubeApiVersion,
            Kind = V1Pod.KubeKind,
            Metadata = new V1ObjectMeta { Name = "generated-pod" },
        };
        jsonSerializer.Deserialize<V1Pod>(jsonSerializer.Serialize(pod)).Metadata.Name.ShouldBe("generated-pod");
        var podYaml = yamlSerializer.Serialize(pod);
        GetRootYamlMappingKeys(podYaml).ShouldBe(["apiVersion", "kind", "metadata"]);
        yamlSerializer.Deserialize<V1Pod>(podYaml).Metadata.Name.ShouldBe("generated-pod");
        if (disableJsonReflection)
        {
            Should.Throw<NotSupportedException>(() => Utilities.CloneObject(resource, jsonSerializer));
            Should.Throw<NotSupportedException>(() => jsonSerializer.Serialize(resource));
            Should.Throw<NotSupportedException>(() => jsonSerializer.Deserialize<ReflectionOnlyResource>(Json));
            Should.Throw<NotSupportedException>(() => yamlSerializer.Serialize(resource));
            Should.Throw<NotSupportedException>(() => yamlSerializer.Deserialize<ReflectionOnlyResource>(Yaml));
            Should.Throw<NotSupportedException>(() => KubernetesJson.Serialize(resource, clientConfiguration.JsonSerializerOptions));
            Should.Throw<NotSupportedException>(() => KubernetesJson.Deserialize<ReflectionOnlyResource>(Json, clientConfiguration.JsonSerializerOptions));
            return;
        }

        if (!JsonSerializer.IsReflectionEnabledByDefault)
        {
            Should.Throw<InvalidOperationException>(() => yamlSerializer.Serialize(resource));
            Should.Throw<InvalidOperationException>(() => jsonSerializer.Serialize(resource));
            return;
        }

        AssertResource(Utilities.CloneObject(resource, jsonSerializer));
        AssertResource(jsonSerializer.Deserialize<ReflectionOnlyResource>(jsonSerializer.Serialize(resource)));
        AssertResource(yamlSerializer.Deserialize<ReflectionOnlyResource>(yamlSerializer.Serialize(resource)));
        AssertResource(yamlSerializer.Deserialize<ReflectionOnlyResource>(Yaml));
        AssertResource(KubernetesJson.Deserialize<ReflectionOnlyResource>(
            KubernetesJson.Serialize(resource, clientConfiguration.JsonSerializerOptions),
            clientConfiguration.JsonSerializerOptions));
    }

    private static void AssertProbe(IKubernetesObject<V1ObjectMeta> expected, IKubernetesObject<V1ObjectMeta> actual)
    {
        actual.ApiVersion.ShouldBe(expected.ApiVersion);
        actual.Kind.ShouldBe(expected.Kind);
        actual.Metadata.Name.ShouldBe(expected.Metadata.Name);
    }

    private static string[] GetRootYamlMappingKeys(string yaml)
    {
        return yaml.Split('\n')
            .Where(static line => line.Length > 0 && !char.IsWhiteSpace(line[0]))
            .Select(static line => line[..line.IndexOf(':')])
            .ToArray();
    }

    private const string Json = """
        {"apiVersion":"preview.kubeui.test/v1","kind":"ReflectionProbe","metadata":{"name":"reflection-probe"},"spec":{"replicas":3}}
        """;

    private const string Yaml = """
        apiVersion: preview.kubeui.test/v1
        kind: ReflectionProbe
        metadata:
          name: reflection-probe
        spec:
          replicas: 3
        """;

    private static ReflectionOnlyResource CreateResource() => new()
    {
        ApiVersion = "preview.kubeui.test/v1",
        Kind = "ReflectionProbe",
        Metadata = new V1ObjectMeta { Name = "reflection-probe" },
        Spec = new ReflectionOnlySpec { Replicas = 3 },
    };

    private static void AssertResource(ReflectionOnlyResource resource)
    {
        resource.ApiVersion.ShouldBe("preview.kubeui.test/v1");
        resource.Kind.ShouldBe("ReflectionProbe");
        resource.Metadata.Name.ShouldBe("reflection-probe");
        resource.Spec.Replicas.ShouldBe(3);
    }

    public sealed class ReflectionOnlyResource : IKubernetesObject<V1ObjectMeta>
    {
        public ReflectionOnlySpec Spec { get; set; } = new();
        public V1ObjectMeta Metadata { get; set; } = new();
        public string Kind { get; set; } = string.Empty;
        public string ApiVersion { get; set; } = string.Empty;
    }

    public sealed class ReflectionOnlySpec
    {
        public int Replicas { get; set; }
    }

}
