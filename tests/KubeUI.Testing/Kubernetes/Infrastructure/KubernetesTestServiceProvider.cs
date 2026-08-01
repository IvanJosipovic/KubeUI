using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

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
        services.AddSingleton<IClusterSettingsStore>(settings);
        services.AddKubeUIKubernetesServices();
        configure?.Invoke(services);
        return services.BuildServiceProvider();
    }
}
