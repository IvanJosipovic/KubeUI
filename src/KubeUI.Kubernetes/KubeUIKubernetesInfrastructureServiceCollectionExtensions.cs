using System.Text.Json.Serialization.Metadata;
using k8s;
using KubernetesCRDModelGen;
using KubeUI.Client;
using KubeUI.Client.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace KubeUI;

public static class KubeUIKubernetesInfrastructureServiceCollectionExtensions
{
    private static int _isJsonConfigured;

    public static IServiceCollection AddKubeUIKubernetesServices(this IServiceCollection services)
    {
        services.AddTransient<ModelCache>();
        services.AddSingleton<IKubernetesYamlSerializer, KubernetesYamlSerializer>();
        services.AddTransient<Cluster>();
        services.AddTransient<IClusterRuntime>(sp => sp.GetRequiredService<Cluster>());
        services.AddSingleton<ClusterManager>();
        services.AddSingleton<IClusterRuntimeCatalog>(sp => sp.GetRequiredService<ClusterManager>());
        services.AddSingleton<IGenerator, Generator>();
        return services;
    }

    public static void ConfigureKubeUIKubernetesJson(this IServiceProvider services)
    {
        if (Interlocked.Exchange(ref _isJsonConfigured, 1) == 1)
        {
            return;
        }

        var logger = services.GetService<ILoggerFactory>()?.CreateLogger("KubeUI.KubernetesJson");

        KubernetesJson.AddJsonOptions(options =>
        {
            options.TypeInfoResolver = JsonTypeInfoResolver.Combine(
                CustomSourceGenerationContext.Default,
                new DefaultJsonTypeInfoResolver
                {
                    Modifiers =
                    {
                        jsonTypeInfo =>
                        {
                            if (jsonTypeInfo.Type?.Namespace?.StartsWith("KubeUI.Models", StringComparison.Ordinal) == true)
                            {
                                foreach (var prop in jsonTypeInfo.Properties)
                                {
                                    prop.IsRequired = false;
                                }
                            }

                            if (jsonTypeInfo.OriginatingResolver is DefaultJsonTypeInfoResolver)
                            {
                                logger?.LogDebug("Type is serialized using reflection: {Type}", jsonTypeInfo.Type);
                            }
                        }
                    }
                });
        });
    }
}
