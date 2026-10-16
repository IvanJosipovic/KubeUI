using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization.Metadata;
using k8s;
using KubeUI.Kubernetes.Serialization;

namespace KubeUI.Kubernetes;

public static class KubeUIKubernetesServiceCollectionExtensions
{
    private static readonly object _jsonConfigurationLock = new();
    private static bool _isJsonConfigured;

    public static IServiceCollection AddKubeUIKubernetesServices(this IServiceCollection services)
    {
        ConfigureKubeUIKubernetesJson();
        services.AddSingleton<IThreadDispatcher, ImmediateThreadDispatcher>();
        services.AddSingleton<IKubeConfigPathProvider, DefaultKubeConfigPathProvider>();
        services.AddSingleton<IPodLogSessionResolver, PodLogSessionResolver>();
        services.AddSingleton<IPodLogStreamClient, PodLogStreamClient>();
        services.AddSingleton<KubernetesModelCatalog>();
        services.AddTransient<ClusterModelCatalog>();
        services.AddSingleton<IKubernetesYamlSerializer, KubernetesYamlSerializer>();
        services.AddSingleton<IAksClusterService, AksClusterService>();
        services.AddTransient<Cluster>();
        services.AddTransient<IClusterRuntime>(sp => sp.GetRequiredService<Cluster>());
        services.AddSingleton<ClusterManager>();
        services.AddSingleton<IClusterRuntimeCatalog>(sp => sp.GetRequiredService<ClusterManager>());
        services.AddHostedService<ClusterManagerStartupService>();
        return services;
    }

    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2026",
        Justification = "KubeUI combines its generated JSON context with KubernetesClient's generated source context; CRD payloads use GenericKubernetesObject and JsonElement extension data.")]
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
                        KubernetesJsonStaticContext.Default,
                        SourceGenerationContext.Default);
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

    /// <summary>
    /// Compatibility shim for the removed reflection-fallback JSON logging configuration.
    /// JSON serialization now uses generated metadata and emits no reflection-fallback logs.
    /// </summary>
    /// <param name="services">Application service provider.</param>
    [Obsolete("JSON reflection-fallback logging was removed. Generated JSON metadata is configured automatically.")]
    public static void ConfigureKubeUIKubernetesJsonLogging(this IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(services);
    }

}
