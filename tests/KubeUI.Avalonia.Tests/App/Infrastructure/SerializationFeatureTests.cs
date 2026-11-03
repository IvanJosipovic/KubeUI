using System.Text.Json;
using System.Text.Json.Serialization;
using Avalonia.Headless.XUnit;
using k8s;
using k8s.Models;
using KubeUI.App.Services.Settings;
using KubeUI.Desktop;
using KubeUI.Features.Resources;
using KubeUI.Kubernetes.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Shouldly;
using YamlDotNet.Serialization;

namespace KubeUI.Avalonia.Tests.App.Infrastructure;

public sealed class SerializationFeatureTests
{
    [AvaloniaFact]
    public async Task generated_serialization_metadata_covers_all_registered_resource_types()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        using var host = CreateHost(disableJsonReflection: true, disableYamlReflection: true);
        var serializer = host.Services.GetRequiredService<IKubernetesJsonSerializer>();
        var yamlContext = new KubernetesYamlStaticContext();
        var yamlRootTypes = GetYamlSerializableRootTypes();
        var duplicateYamlRootTypes = yamlRootTypes
            .GroupBy(type => type)
            .Where(group => group.Count() > 1)
            .Select(group => group.Key.FullName ?? group.Key.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();
        duplicateYamlRootTypes.ShouldBeEmpty(string.Join(Environment.NewLine, duplicateYamlRootTypes));

        KubernetesClientConfiguration configuration = new();
        serializer.ConfigureClient(configuration);
        var resources = workspace.GetResourceConfigs();
        resources.ShouldNotBeEmpty();
        var nestedKubernetesTypes = new HashSet<Type>();
        nestedKubernetesTypes.Add(typeof(GenericKubernetesObject));
        nestedKubernetesTypes.Add(typeof(KubernetesObject));
        AddNestedKubernetesModelTypes(typeof(k8s.KubeConfigModels.K8SConfiguration), nestedKubernetesTypes);
        AddNestedKubernetesModelTypes(typeof(V1PodTemplate), nestedKubernetesTypes);
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
            yamlRootTypes.ShouldContain(modelType);
            yamlContext.IsKnownType(modelType)
                .ShouldBeTrue($"No generated YAML metadata for {modelType} (resource kind {resource.Kind}).");
            AddNestedKubernetesModelTypes(modelType, nestedKubernetesTypes);

            var watchEventType = typeof(Watcher<>).GetNestedType("WatchEvent")!.MakeGenericType(modelType);
            var hasGeneratedWatchMetadata = KubernetesJsonStaticContext.Default.GetTypeInfo(watchEventType) is not null
                || SourceGenerationContext.Default.GetTypeInfo(watchEventType) is not null;
            hasGeneratedWatchMetadata.ShouldBeTrue(
                $"No generated JSON metadata for {watchEventType} (resource kind {resource.Kind}).");

            var listType = typeof(KubernetesList<>).MakeGenericType(modelType);
            var hasGeneratedListMetadata = KubernetesJsonStaticContext.Default.GetTypeInfo(listType) is not null
                || SourceGenerationContext.Default.GetTypeInfo(listType) is not null;
            hasGeneratedListMetadata.ShouldBeTrue(
                $"No generated JSON metadata for {listType} (resource kind {resource.Kind}).");

            var metadata = configuration.JsonSerializerOptions.GetTypeInfo(modelType);
            var model = JsonSerializer.Deserialize("{}", metadata)
                .ShouldBeAssignableTo<IKubernetesObject<V1ObjectMeta>>();
            model.ApiVersion = string.IsNullOrEmpty(resource.Kind.Group)
                ? resource.Kind.ApiVersion
                : $"{resource.Kind.Group}/{resource.Kind.ApiVersion}";
            model.Kind = resource.Kind.Kind;
            model.Metadata = new V1ObjectMeta { Name = "metadata-coverage-probe" };

            var restored = JsonSerializer.Deserialize(serializer.Serialize(model), metadata)
                .ShouldBeAssignableTo<IKubernetesObject<V1ObjectMeta>>();
            restored.ApiVersion.ShouldBe(model.ApiVersion);
            restored.Kind.ShouldBe(model.Kind);
            restored.Metadata.Name.ShouldBe(model.Metadata.Name);
        }

        var missingNestedTypes = nestedKubernetesTypes
            .Where(type => !yamlContext.IsKnownType(type)
                && KubernetesJsonStaticContext.Default.GetTypeInfo(type) is null)
            .Select(type => type.FullName ?? type.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();
        missingNestedTypes.ShouldBeEmpty(string.Join(Environment.NewLine, missingNestedTypes));

        var missingYamlRootTypes = nestedKubernetesTypes
            .Where(type => !yamlRootTypes.Contains(type))
            .Select(type => type.FullName ?? type.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();
        missingYamlRootTypes.ShouldBeEmpty(string.Join(Environment.NewLine, missingYamlRootTypes));

        var unusedYamlRootTypes = yamlRootTypes
            .Where(type => !nestedKubernetesTypes.Contains(type))
            .Select(type => type.FullName ?? type.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();
        unusedYamlRootTypes.ShouldBeEmpty(string.Join(Environment.NewLine, unusedYamlRootTypes));
    }

    [Fact]
    public void generated_json_contexts_do_not_register_duplicate_root_types()
    {
        var kubeUiTypes = GetJsonSerializableRootTypes(typeof(KubernetesJsonStaticContext));
        var kubernetesClientTypes = GetJsonSerializableRootTypes(typeof(SourceGenerationContext));
        var duplicateTypes = kubeUiTypes
            .Intersect(kubernetesClientTypes)
            .Select(type => type.FullName ?? type.Name)
            .OrderBy(name => name, StringComparer.Ordinal)
            .ToArray();

        duplicateTypes.ShouldBeEmpty(string.Join(Environment.NewLine, duplicateTypes));
    }

    private static Type[] GetJsonSerializableRootTypes(Type contextType)
    {
        return contextType.GetCustomAttributesData()
            .Where(attribute => attribute.AttributeType == typeof(JsonSerializableAttribute))
            .Select(attribute => attribute.ConstructorArguments[0].Value)
            .OfType<Type>()
            .ToArray();
    }

    private static Type[] GetYamlSerializableRootTypes()
    {
        return typeof(KubernetesYamlStaticContext).GetCustomAttributesData()
            .Where(attribute => attribute.AttributeType == typeof(YamlSerializableAttribute))
            .Select(attribute => attribute.ConstructorArguments[0].Value)
            .OfType<Type>()
            .ToArray();
    }

    private static void AddNestedKubernetesModelTypes(Type type, HashSet<Type> modelTypes)
    {
        if (type == typeof(ResourceQuantity)
            || type == typeof(k8s.KubeConfigModels.NamedExtension)
            || type == typeof(V1ContainerRestartRuleOnExitCodes)
            || type == typeof(V1LinuxContainerUser)
            || type == typeof(V1ListMeta)
            || type == typeof(V1PodDisruptionBudgetStatus)
            || type == typeof(V1PodSecurityContext))
        {
            return;
        }

        if (type.IsArray)
        {
            AddNestedKubernetesModelTypes(type.GetElementType()!, modelTypes);
            return;
        }

        if (type.IsGenericType)
        {
            foreach (var argument in type.GetGenericArguments())
            {
                AddNestedKubernetesModelTypes(argument, modelTypes);
            }
        }

        if (type.Assembly != typeof(V1Pod).Assembly
            || type.Namespace is not ("k8s.Models" or "k8s.KubeConfigModels" or "k8s")
            || !modelTypes.Add(type))
        {
            return;
        }

        foreach (var property in type.GetProperties(System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public))
        {
            AddNestedKubernetesModelTypes(property.PropertyType, modelTypes);
        }
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void startup_settings_control_serialization_capabilities(bool disableJsonReflection, bool disableYamlReflection)
    {
        using var host = CreateHost(disableJsonReflection, disableYamlReflection);
        AssertCapabilities(host, disableJsonReflection, disableYamlReflection);
    }

    [Fact]
    public void default_settings_enable_reflection()
    {
        using var host = Program.CreateHostBuilder(
            [], includeOptionalServices: false, settingsOverride: new SettingsPersistenceData()).Build();
        AssertCapabilities(host, disableJsonReflection: false, disableYamlReflection: false);
    }

    [Fact]
    public void application_instances_keep_independent_serialization_settings()
    {
        SettingsPersistenceData settings = new();
        using var reflectionHost = Program.CreateHostBuilder(
            [], includeOptionalServices: false, settingsOverride: settings).Build();
        settings.Settings.PreviewFeatures.DisableJsonSerializationReflection = true;
        settings.Settings.PreviewFeatures.DisableYamlSerializationReflection = true;
        using var generatedHost = Program.CreateHostBuilder(
            [], includeOptionalServices: false, settingsOverride: settings).Build();
        AssertCapabilities(reflectionHost, disableJsonReflection: false, disableYamlReflection: false);
        AssertCapabilities(generatedHost, disableJsonReflection: true, disableYamlReflection: true);
        AssertCapabilities(reflectionHost, disableJsonReflection: false, disableYamlReflection: false);
    }

    private static IHost CreateHost(bool disableJsonReflection, bool disableYamlReflection)
    {
        SettingsPersistenceData settings = new();
        settings.Settings.PreviewFeatures.DisableJsonSerializationReflection = disableJsonReflection;
        settings.Settings.PreviewFeatures.DisableYamlSerializationReflection = disableYamlReflection;
        return Program.CreateHostBuilder([], includeOptionalServices: false, settingsOverride: settings).Build();
    }

    private static void AssertCapabilities(IHost host, bool disableJsonReflection, bool disableYamlReflection)
    {
        var jsonSerializer = host.Services.GetRequiredService<IKubernetesJsonSerializer>();
        var yamlSerializer = host.Services.GetRequiredService<IKubernetesYamlSerializer>();
        KubernetesClientConfiguration clientConfiguration = new();
        jsonSerializer.ConfigureClient(clientConfiguration);
        var resource = CreateResource();
        V1Pod pod = new() { Metadata = new V1ObjectMeta { Name = "generated-pod" } };
        jsonSerializer.Deserialize<V1Pod>(jsonSerializer.Serialize(pod)).Metadata.Name.ShouldBe("generated-pod");
        yamlSerializer.Deserialize<V1Pod>(yamlSerializer.Serialize(pod)).Metadata.Name.ShouldBe("generated-pod");
        if (disableJsonReflection)
        {
            Should.Throw<NotSupportedException>(() => Utilities.CloneObject(resource, jsonSerializer));
            Should.Throw<NotSupportedException>(() => jsonSerializer.Serialize(resource));
            Should.Throw<NotSupportedException>(() => jsonSerializer.Deserialize<ReflectionOnlyResource>(Json));
            Should.Throw<NotSupportedException>(() => KubernetesJson.Serialize(resource, clientConfiguration.JsonSerializerOptions));
            Should.Throw<NotSupportedException>(() => KubernetesJson.Deserialize<ReflectionOnlyResource>(Json, clientConfiguration.JsonSerializerOptions));
        }
        else
        {
            AssertResource(Utilities.CloneObject(resource, jsonSerializer));
            AssertResource(jsonSerializer.Deserialize<ReflectionOnlyResource>(jsonSerializer.Serialize(resource)));
            AssertResource(jsonSerializer.Deserialize<ReflectionOnlyResource>(Json));
            AssertResource(KubernetesJson.Deserialize<ReflectionOnlyResource>(
                KubernetesJson.Serialize(resource, clientConfiguration.JsonSerializerOptions), clientConfiguration.JsonSerializerOptions));
        }

        if (disableYamlReflection)
        {
            // Static metadata has no properties for this unregistered model.
            yamlSerializer.Serialize(resource).Trim().ShouldBe("{}");
            var exception = Should.Throw<YamlDotNet.Core.YamlException>(() => yamlSerializer.Deserialize<ReflectionOnlyResource>(Yaml));
            exception.InnerException.ShouldBeOfType<InvalidOperationException>()
                .Message.ShouldContain("not registered in the YamlDotNet static context");
        }
        else
        {
            AssertResource(yamlSerializer.Deserialize<ReflectionOnlyResource>(yamlSerializer.Serialize(resource)));
            AssertResource(yamlSerializer.Deserialize<ReflectionOnlyResource>(Yaml));
        }
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
        Spec = new ReflectionOnlySpec { Replicas = 3 }
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
        public string ApiVersion { get; set; } = string.Empty;
        public string Kind { get; set; } = string.Empty;
        public V1ObjectMeta Metadata { get; set; } = new();
        public ReflectionOnlySpec Spec { get; set; } = new();
    }

    public sealed class ReflectionOnlySpec
    {
        public int Replicas { get; set; }
    }
}
