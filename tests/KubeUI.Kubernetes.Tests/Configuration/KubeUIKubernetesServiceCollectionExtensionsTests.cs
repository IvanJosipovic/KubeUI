using k8s;
using k8s.Models;
using KubeUI.Kubernetes.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace KubeUI.Kubernetes.Tests.Configuration;

public sealed class KubeUIKubernetesServiceCollectionExtensionsTests
{
    [Fact]
    public void ConfigureKubeUIKubernetesJsonLogging_remains_available_for_existing_consumers()
    {
        using var services = new ServiceCollection().BuildServiceProvider();

        services.ConfigureKubeUIKubernetesJsonLogging();
    }

    [Theory]
    [InlineData(typeof(V1Status))]
    [InlineData(typeof(V1DeleteOptions))]
    [InlineData(typeof(V1Patch))]
    [InlineData(typeof(GenericKubernetesObject))]
    [InlineData(typeof(KubernetesObject))]
    [InlineData(typeof(KubernetesList<GenericKubernetesObject>))]
    [InlineData(typeof(KubernetesList<V1Pod>))]
    [InlineData(typeof(KubernetesList<Corev1Event>))]
    public void generated_json_metadata_covers_protocol_payload_types(Type payloadType)
    {
        using var services = new ServiceCollection()
            .AddKubeUIKubernetesServices(jsonAllowReflection: false, yamlAllowReflection: false)
            .BuildServiceProvider();
        var serializer = services.GetRequiredService<IKubernetesJsonSerializer>();
        KubernetesClientConfiguration configuration = new();
        serializer.ConfigureClient(configuration);

        Should.NotThrow(() => configuration.JsonSerializerOptions.GetTypeInfo(payloadType));
    }

    [Fact]
    public void json_resolver_uses_reflection_fallback_only_when_allowed()
    {
        using var services = new ServiceCollection()
            .AddKubeUIKubernetesServices(jsonAllowReflection: false, yamlAllowReflection: false)
            .BuildServiceProvider();
        var serializer = services.GetRequiredService<IKubernetesJsonSerializer>();
        Should.Throw<NotSupportedException>(() =>
            serializer.Serialize(new Uri("https://example.com")));

        if (System.Text.Json.JsonSerializer.IsReflectionEnabledByDefault)
        {
            using var reflectionServices = new ServiceCollection()
                .AddKubeUIKubernetesServices(jsonAllowReflection: true, yamlAllowReflection: false)
                .BuildServiceProvider();
            reflectionServices.GetRequiredService<IKubernetesJsonSerializer>().Serialize(new Uri("https://example.com"))
                .ShouldBe("\"https://example.com/\"");
            return;
        }

        Should.Throw<InvalidOperationException>(() =>
            new ServiceCollection().AddKubeUIKubernetesServices(jsonAllowReflection: true, yamlAllowReflection: false))
            .Message.ShouldContain("reflection-based JSON serialization is unavailable");
    }

    [Fact]
    public void disabling_json_source_generation_requires_reflection_support()
    {
        if (System.Text.Json.JsonSerializer.IsReflectionEnabledByDefault)
        {
            Should.NotThrow(() =>
                KubeUIKubernetesServiceCollectionExtensions.EnsureJsonReflectionIsAvailable(allowReflection: true));
            return;
        }

        Should.Throw<InvalidOperationException>(() =>
            KubeUIKubernetesServiceCollectionExtensions.EnsureJsonReflectionIsAvailable(allowReflection: true))
            .Message.ShouldContain("reflection-based JSON serialization is unavailable");
    }
}
