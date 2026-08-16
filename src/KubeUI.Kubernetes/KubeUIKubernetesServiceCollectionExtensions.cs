using System.Text.Json.Serialization.Metadata;
using k8s;
using KubernetesCRDModelGen;
using KubeUI.Kubernetes.Serialization;

namespace KubeUI.Kubernetes;

public static class KubeUIKubernetesServiceCollectionExtensions
{
    private static readonly object _jsonConfigurationLock = new();
    private static bool _isJsonConfigured;
    private static ILogger? _jsonLogger;

    public static IServiceCollection AddKubeUIKubernetesServices(this IServiceCollection services)
    {
        ConfigureKubeUIKubernetesJson();
        services.AddSingleton<IThreadDispatcher, ImmediateThreadDispatcher>();
        services.AddSingleton<IKubeConfigPathProvider, DefaultKubeConfigPathProvider>();
        services.AddSingleton<KubernetesModelCatalog>();
        services.AddTransient<ClusterModelCatalog>();
        services.AddSingleton<IKubernetesYamlSerializer, KubernetesYamlSerializer>();
        services.AddSingleton<IAksClusterService, AksClusterService>();
        services.AddTransient<Cluster>();
        services.AddTransient<IClusterRuntime>(sp => sp.GetRequiredService<Cluster>());
        services.AddSingleton<ClusterManager>();
        services.AddSingleton<IClusterRuntimeCatalog>(sp => sp.GetRequiredService<ClusterManager>());
        services.AddHostedService<ClusterManagerStartupService>();
        services.AddSingleton<IGenerator, Generator>();
        return services;
    }

    private static void ConfigureKubeUIKubernetesJson()
    {
        lock (_jsonConfigurationLock)
        {
            if (_isJsonConfigured)
            {
                return;
            }

            KubernetesJson.AddJsonOptions(options =>
            {
                if (options.IsReadOnly)
                {
                    return;
                }

                try
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
                }
                catch (InvalidOperationException) when (options.IsReadOnly)
                {
                    // KubernetesJson owns a process-wide options instance. It may have
                    // been frozen by an earlier serialization before services are registered.
                }
            });

            _isJsonConfigured = true;
        }
    }

    public static void ConfigureKubeUIKubernetesJsonLogging(this IServiceProvider services)
    {
        _jsonLogger ??= services.GetService<ILoggerFactory>()?.CreateLogger("KubeUI.KubernetesJson");
    }
}
