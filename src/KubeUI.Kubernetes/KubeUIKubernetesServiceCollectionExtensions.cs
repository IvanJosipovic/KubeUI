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
    private static readonly Lock _jsonConfigurationLock = new();
    private static bool _isJsonConfigured;
    private static bool? _configuredJsonAllowReflection;
    private static ILogger? _jsonLogger;

    /// <summary>Adds KubeUI Kubernetes services and configures Kubernetes JSON serialization.</summary>
    /// <param name="services">Service collection to configure.</param>
    /// <param name="jsonAllowReflection">Whether Kubernetes JSON serialization may use reflection instead of generated metadata.</param>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection AddKubeUIKubernetesServices(
        this IServiceCollection services,
        bool jsonAllowReflection = false)
    {
        ConfigureKubeUIKubernetesJson(jsonAllowReflection);
        services.AddSingleton<IThreadDispatcher, ImmediateThreadDispatcher>();
        services.AddSingleton<IKubeConfigPathProvider, DefaultKubeConfigPathProvider>();
        services.AddSingleton<IPodLogSessionResolver, PodLogSessionResolver>();
        services.AddSingleton<IPodLogStreamClient, PodLogStreamClient>();
        services.AddSingleton<KubernetesModelCatalog>();
        services.AddTransient<ClusterModelCatalog>();
        services.AddSingleton<IKubernetesYamlSerializer, KubernetesYamlSerializer>();
        services.AddSingleton<IAksClusterService, AksClusterService>();
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

    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2026",
        Justification = "KubeUI combines its generated JSON context with KubernetesClient's generated source context; CRD payloads use GenericKubernetesObject and JsonElement extension data.")]
    private static void ConfigureKubeUIKubernetesJson(bool allowReflection)
    {
        lock (_jsonConfigurationLock)
        {
            if (_isJsonConfigured)
            {
                if (_configuredJsonAllowReflection != allowReflection)
                {
                    throw new InvalidOperationException(
                        "Kubernetes JSON serialization was already configured with a different reflection setting. " +
                        "Configure this setting once at application startup.");
                }

                return;
            }

            EnsureJsonReflectionIsAvailable(allowReflection);
            if (!allowReflection)
            {
                EnsureResolverCovers(KubernetesJsonRequiredTypes);
            }

            KubernetesJson.AddJsonOptions(options =>
            {
                if (options.IsReadOnly)
                {
                    return;
                }

                try
                {
                    options.TypeInfoResolver = CreateKubeUIKubernetesJsonResolver(allowReflection, _jsonLogger);
                }
                catch (InvalidOperationException) when (options.IsReadOnly)
                {
                    // KubernetesJson owns a process-wide options instance. It may have
                    // been frozen by an earlier serialization before services are registered.
                }
            });

            _isJsonConfigured = true;
            _configuredJsonAllowReflection = allowReflection;
        }
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
    /// Types KubernetesClient serializes that must be covered by generated metadata so a
    /// trimmed Release build never falls back to the reflection serializer.
    /// </summary>
    private static IReadOnlyList<Type> KubernetesJsonRequiredTypes { get; } =
    [
        typeof(k8s.Models.V1Pod),
        typeof(k8s.Models.V1Deployment),
        typeof(k8s.Models.V1Service),
        typeof(k8s.Models.V1ConfigMap),
        typeof(k8s.Models.V1Secret),
        typeof(k8s.Models.V1Namespace),
        typeof(k8s.Models.V1Node),
        typeof(k8s.Models.Corev1Event),
        typeof(k8s.Models.V1Status),
        typeof(k8s.Models.V1DeleteOptions),
        typeof(k8s.Models.V1Patch),
        typeof(GenericKubernetesObject),
        typeof(KubernetesObject),
        typeof(k8s.Models.KubernetesList<k8s.Models.V1Pod>),
        typeof(k8s.Models.KubernetesList<k8s.Models.Corev1Event>)
    ];

    /// <summary>
    /// Configures the logger used to report Kubernetes types that still fall back to
    /// reflection-based serialization. Types covered by the generated metadata never reach
    /// this path, so a reported type is one that still needs a source-generated registration.
    /// </summary>
    /// <param name="services">Application service provider.</param>
    public static void ConfigureKubeUIKubernetesJsonLogging(this IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(services);
        _jsonLogger ??= services.GetService<ILoggerFactory>()?.CreateLogger("KubeUI.KubernetesJson");
    }

    /// <summary>
    /// Gets the JSON type metadata resolver that KubeUI installs for Kubernetes payloads.
    /// </summary>
    /// <returns>The generated resolver, optionally followed by a reflection fallback.</returns>
    [UnconditionalSuppressMessage(
        "Trimming",
        "IL2026",
        Justification = "This test-visible resolver factory creates a reflection resolver only when the caller explicitly allows reflection.")]
    internal static IJsonTypeInfoResolver GetKubeUIKubernetesJsonResolver(bool allowReflection = false)
    {
        EnsureJsonReflectionIsAvailable(allowReflection);
        if (allowReflection)
        {
            return JsonTypeInfoResolver.Combine(
                KubernetesJsonStaticContext.Default,
                SourceGenerationContext.Default,
                new DefaultJsonTypeInfoResolver());
        }

        return JsonTypeInfoResolver.Combine(
            KubernetesJsonStaticContext.Default,
            SourceGenerationContext.Default);
    }

    internal static void EnsureJsonReflectionIsAvailable(bool allowReflection)
    {
        if (allowReflection && !JsonSerializer.IsReflectionEnabledByDefault)
        {
            throw new InvalidOperationException(
                "Kubernetes JSON reflection cannot be enabled because reflection-based JSON serialization is unavailable.");
        }
    }

    /// <summary>
    /// Verifies that the combined resolver can supply metadata for every type KubeUI
    /// serializes. A missing type would silently fall back to reflection, which is
    /// unavailable in a trimmed Release build.
    /// </summary>
    /// <param name="types">Types to verify.</param>
    /// <exception cref="InvalidOperationException">A type cannot be resolved by the generated metadata.</exception>
    internal static void EnsureResolverCovers(IEnumerable<Type> types)
    {
        ArgumentNullException.ThrowIfNull(types);

        var resolver = GetKubeUIKubernetesJsonResolver();
        var options = new JsonSerializerOptions
        {
            TypeInfoResolver = resolver
        };

        foreach (var type in types)
        {
            if (resolver.GetTypeInfo(type, options) is null)
            {
                throw new InvalidOperationException(
                    $"The generated Kubernetes JSON metadata does not cover '{type.FullName}'. " +
                    "Add the type to KubernetesJsonStaticContext so trimmed builds do not fall back to reflection.");
            }
        }
    }

    [LoggerMessage(
        Level = LogLevel.Critical,
        Message = "Type is serialized using reflection: {Type}")]
    private static partial void LogTypeSerializedUsingReflection(ILogger logger, Type type);
}
