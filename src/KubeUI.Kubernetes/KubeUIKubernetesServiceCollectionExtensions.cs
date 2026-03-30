using System.Text.Json.Serialization.Metadata;
using k8s;
using KubernetesCRDModelGen;
using KubeUI.Kubernetes;
using KubeUI.Kubernetes.Serialization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace KubeUI.Kubernetes;

public static class KubeUIKubernetesServiceCollectionExtensions
{
    private static int _isJsonConfigured;
    private static ILogger? _jsonLogger;

    public static IServiceCollection AddKubeUIKubernetesServices(this IServiceCollection services)
    {
        ConfigureKubeUIKubernetesJson();
        services.AddSingleton<IThreadDispatcher, ImmediateThreadDispatcher>();
        services.AddTransient<ModelCache>();
        services.AddSingleton<IKubernetesYamlSerializer, KubernetesYamlSerializer>();
        services.AddTransient<IPrometheusProvider, OperatorPrometheusProvider>();
        services.AddTransient<IPrometheusProvider, OpenShiftPrometheusProvider>();
        services.AddTransient<IPrometheusProvider, ManualPrometheusProvider>();
        services.AddTransient<IPrometheusProvider, ExternalPrometheusProvider>();
        services.AddTransient<IPrometheusQueryClient, PrometheusQueryClient>();
        services.AddTransient<IMetricsService, MetricsService>();
        services.AddTransient<Cluster>();
        services.AddTransient<IClusterRuntime>(sp => sp.GetRequiredService<Cluster>());
        services.AddSingleton<ClusterManager>();
        services.AddSingleton<IClusterRuntimeCatalog>(sp => sp.GetRequiredService<ClusterManager>());
        services.AddSingleton<IGenerator, Generator>();
        return services;
    }

    private static void ConfigureKubeUIKubernetesJson()
    {
        if (Interlocked.Exchange(ref _isJsonConfigured, 1) == 1)
        {
            return;
        }

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
                                _jsonLogger?.LogDebug("Type is serialized using reflection: {Type}", jsonTypeInfo.Type);
                            }
                        }
                    }
                });
        });
    }

    public static void ConfigureKubeUIKubernetesJsonLogging(this IServiceProvider services)
    {
        _jsonLogger ??= services.GetService<ILoggerFactory>()?.CreateLogger("KubeUI.KubernetesJson");
    }
}



