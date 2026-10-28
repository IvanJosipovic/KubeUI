using k8s;
using k8s.Models;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using System.Text.Json.Serialization.Metadata;

namespace KubeUI.Kubernetes.Tests.Configuration;

public sealed class KubeUIKubernetesServiceCollectionExtensionsTests
{
    [Fact]
    public void ConfigureKubeUIKubernetesJsonLogging_remains_available_for_existing_consumers()
    {
        using var services = new ServiceCollection().BuildServiceProvider();

        services.ConfigureKubeUIKubernetesJsonLogging();
    }

    [Fact]
    public void generated_json_metadata_covers_every_required_kubernetes_type()
    {
        Should.NotThrow(() =>
            KubeUIKubernetesServiceCollectionExtensions.EnsureResolverCovers(
            [
                typeof(V1Pod),
                typeof(V1Deployment),
                typeof(V1Service),
                typeof(V1ConfigMap),
                typeof(V1Secret),
                typeof(V1Namespace),
                typeof(V1Node),
                typeof(Corev1Event),
                typeof(V1Status),
                typeof(V1DeleteOptions),
                typeof(V1Patch),
                typeof(GenericKubernetesObject),
                typeof(KubernetesObject),
                typeof(KubernetesList<V1Pod>),
                typeof(KubernetesList<Corev1Event>)
            ]));
    }

    [Fact]
    public void resolver_guard_reports_a_type_without_generated_metadata()
    {
        var exception = Should.Throw<InvalidOperationException>(() =>
            KubeUIKubernetesServiceCollectionExtensions.EnsureResolverCovers([typeof(Uri)]));

        exception.Message.ShouldContain("Uri");
        exception.Message.ShouldContain("KubernetesJsonStaticContext");
    }

    [Fact]
    public void json_resolver_uses_reflection_fallback_only_when_allowed()
    {
        var generatedResolver = KubeUIKubernetesServiceCollectionExtensions.GetKubeUIKubernetesJsonResolver();
        var generatedOptions = new System.Text.Json.JsonSerializerOptions { TypeInfoResolver = generatedResolver };
        Should.Throw<NotSupportedException>(() =>
            System.Text.Json.JsonSerializer.Serialize(new Uri("https://example.com"), generatedOptions));

        if (System.Text.Json.JsonSerializer.IsReflectionEnabledByDefault)
        {
            var resolver = KubeUIKubernetesServiceCollectionExtensions.GetKubeUIKubernetesJsonResolver(allowReflection: true);
            var options = new System.Text.Json.JsonSerializerOptions { TypeInfoResolver = resolver };
            System.Text.Json.JsonSerializer.Serialize(new Uri("https://example.com"), options)
                .ShouldBe("\"https://example.com/\"");
            return;
        }

        Should.Throw<InvalidOperationException>(() =>
            KubeUIKubernetesServiceCollectionExtensions.GetKubeUIKubernetesJsonResolver(allowReflection: true))
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
