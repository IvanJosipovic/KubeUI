using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Kubernetes;

namespace KubeUI.Testing.Kubernetes.Infrastructure;

public sealed class KubernetesTestKubeConfigPathProvider : IKubeConfigPathProvider
{
    public KubernetesTestKubeConfigPathProvider(string path)
    {
        DefaultPath = Path.GetFullPath(path);
    }

    public string DefaultPath { get; }
}

public sealed class KubernetesTestHostApplicationLifetime : IHostApplicationLifetime
{
    public CancellationToken ApplicationStarted => CancellationToken.None;
    public CancellationToken ApplicationStopping => CancellationToken.None;
    public CancellationToken ApplicationStopped => CancellationToken.None;
    public void StopApplication()
    {
    }
}

public sealed class KubernetesTestSettingsStore : IClusterSettingsStore
{
    private readonly List<string> _kubeConfigPaths;
    private readonly Dictionary<IClusterRuntime, IReadOnlyCollection<string>> _namespaces = [];

    public KubernetesTestSettingsStore(params string[] kubeConfigPaths)
    {
        _kubeConfigPaths = [.. kubeConfigPaths];
    }

    public IReadOnlyCollection<string> KubeConfigPaths => _kubeConfigPaths;

    public void AddKubeConfigPath(string path)
    {
        if (!_kubeConfigPaths.Contains(path, StringComparer.Ordinal))
        {
            _kubeConfigPaths.Add(path);
        }
    }

    public IReadOnlyCollection<string> GetClusterNamespaces(IClusterRuntime cluster) =>
        _namespaces.TryGetValue(cluster, out var namespaces) ? namespaces : [];

    public void SetClusterNamespaces(IClusterRuntime cluster, params string[] namespaces) =>
        _namespaces[cluster] = namespaces;
}

public static class KubernetesTestServiceProvider
{
    public static ServiceProvider Build(KubernetesTestSettingsStore settings)
        => Build((IClusterSettingsStore)settings);

    public static ServiceProvider Build(IClusterSettingsStore settings, Action<IServiceCollection>? configure = null)
    {
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddDebug().SetMinimumLevel(LogLevel.Information));
        services.AddSingleton<IHostApplicationLifetime, KubernetesTestHostApplicationLifetime>();
        services.AddSingleton(settings);
        services.AddKubeUIKubernetesServices();
        configure?.Invoke(services);
        var provider = services.BuildServiceProvider();
        RegisterYamlModels(provider.GetRequiredService<KubernetesModelCatalog>());
        return provider;
    }

    private static void RegisterYamlModels(KubernetesModelCatalog catalog)
    {
        Register<V1Pod>(catalog);
        Register<V1Namespace>(catalog);
        Register<V1ServiceAccount>(catalog);
        Register<V1Secret>(catalog);
        Register<V1ClusterRole>(catalog);
        Register<V1ClusterRoleBinding>(catalog);
        Register<V1RoleBinding>(catalog);
    }

    private static void Register<T>(KubernetesModelCatalog catalog)
        where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        catalog.Register(GroupApiVersionKind.From<T>(), typeof(T));
    }
}
