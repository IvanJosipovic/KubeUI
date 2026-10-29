using System.Diagnostics.CodeAnalysis;
using System.Text.Json;
using System.Text.Json.Serialization.Metadata;
using k8s;
using KubeUI.Kubernetes.Serialization;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace KubeUI.Kubernetes;

public static partial class KubeUIKubernetesServiceCollectionExtensions
{
    /// <summary>Adds KubeUI Kubernetes services and configures Kubernetes JSON serialization.</summary>
    /// <param name="services">Service collection to configure.</param>
    /// <param name="jsonAllowReflection">Whether Kubernetes JSON serialization may use reflection instead of generated metadata.</param>
    /// <param name="yamlAllowReflection">Whether YAML serialization may inspect runtime model properties.</param>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection AddKubeUIKubernetesServices(
        this IServiceCollection services,
        bool jsonAllowReflection,
        bool yamlAllowReflection)
    {
        EnsureJsonReflectionIsAvailable(jsonAllowReflection);
        services.AddSingleton<IKubernetesJsonSerializer>(provider =>
            new KubernetesJsonSerializer(CreateKubeUIKubernetesJsonOptions(
                jsonAllowReflection, provider.GetService<ILoggerFactory>()?.CreateLogger("KubeUI.KubernetesJson"))));
        services.AddSingleton<IThreadDispatcher, ImmediateThreadDispatcher>();
        services.AddSingleton<IKubeConfigPathProvider, DefaultKubeConfigPathProvider>();
        services.AddSingleton<IPodLogSessionResolver, PodLogSessionResolver>();
        services.AddSingleton<IPodLogStreamClient, PodLogStreamClient>();
        services.AddSingleton<KubernetesModelCatalog>();
        services.AddTransient<ClusterModelCatalog>();
        services.AddSingleton<IKubernetesYamlSerializer>(_ => new KubernetesYamlSerializer(yamlAllowReflection));
        services.AddSingleton<IAksClusterService, AksClusterService>();
        services.AddSingleton<IAzureMonitorWorkspaceService, AzureMonitorWorkspaceService>();
        services.AddTransient<IMetricsService, MetricsService>();
        services.AddTransient<IPrometheusQueryClient, PrometheusQueryClient>();
        services.AddSingleton<IPrometheusProvider, OperatorPrometheusProvider>();
        services.AddSingleton<IPrometheusProvider, OpenShiftPrometheusProvider>();
        services.AddSingleton<IPrometheusProvider, ManualPrometheusProvider>();
        services.AddSingleton<IPrometheusProvider, ExternalPrometheusProvider>();
        services.AddSingleton<IPrometheusProvider, AzureMonitorPrometheusProvider>();
        services.AddSingleton<IHelmClientFactory, HelmClientFactory>();
        services.AddSingleton<IHelmService, HelmService>();
        services.AddTransient<Cluster>();
        services.AddTransient<IClusterRuntime>(sp => sp.GetRequiredService<Cluster>());
        services.AddSingleton<ClusterManager>();
        services.AddSingleton<IClusterRuntimeCatalog>(sp => sp.GetRequiredService<ClusterManager>());
        services.AddHostedService<ClusterManagerStartupService>();
        services.AddOptions<ClusterAuthorizationOptions>();
        return services;
    }

    private static JsonSerializerOptions CreateKubeUIKubernetesJsonOptions(bool allowReflection, ILogger? logger)
    {
        var options = new KubernetesClientConfiguration().JsonSerializerOptions;
        options.TypeInfoResolver = CreateKubeUIKubernetesJsonResolver(allowReflection, logger);
        return options;
    }

    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2026",
        Justification = "Reflection is retained as an explicit opt-in mode, guarded by JsonSerializer.IsReflectionEnabledByDefault; source-generated mode remains the default.")]
    private static IJsonTypeInfoResolver CreateKubeUIKubernetesJsonResolver(bool allowReflection, ILogger? logger)
    {
        var reflectionResolver = new DefaultJsonTypeInfoResolver
        {
            Modifiers =
            {
                jsonTypeInfo =>
                {
                    if (jsonTypeInfo.OriginatingResolver is DefaultJsonTypeInfoResolver
                        && logger is not null)
                    {
                        LogTypeSerializedUsingReflection(logger, jsonTypeInfo.Type);
                    }
                }
            }
        };

        return allowReflection
            ? JsonTypeInfoResolver.Combine(
                KubernetesJsonStaticContext.Default,
                SourceGenerationContext.Default,
                reflectionResolver)
            : JsonTypeInfoResolver.Combine(
                KubernetesJsonStaticContext.Default,
                SourceGenerationContext.Default);
    }

    /// <summary>
    /// Configures the logger used to report Kubernetes types that still fall back to
    /// reflection-based serialization. Types covered by the generated metadata never reach
    /// this path, so a reported type is one that still needs a source-generated registration.
    /// </summary>
    /// <param name="services">Application service provider.</param>
    public static void ConfigureKubeUIKubernetesJsonLogging(this IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(services);
        _ = services.GetService<IKubernetesJsonSerializer>();
    }

    internal static void EnsureJsonReflectionIsAvailable(bool allowReflection)
    {
        if (allowReflection && !JsonSerializer.IsReflectionEnabledByDefault)
        {
            throw new InvalidOperationException(
                "Kubernetes JSON reflection cannot be enabled because reflection-based JSON serialization is unavailable.");
        }
    }

    [LoggerMessage(
        Level = LogLevel.Critical,
        Message = "Type is serialized using reflection: {Type}")]
    private static partial void LogTypeSerializedUsingReflection(ILogger logger, Type type);
}
