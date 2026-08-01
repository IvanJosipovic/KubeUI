using Microsoft.Extensions.DependencyInjection;

namespace KubeUI.Testing.Kubernetes.Infrastructure;

public sealed class KubernetesTestClusterScope : IDisposable, IAsyncDisposable
{
    private readonly ServiceProvider _services;

    public KubernetesTestClusterScope()
    {
        _services = KubernetesTestServiceProvider.Build(new KubernetesTestSettingsStore());
        Cluster = _services.GetRequiredService<Cluster>();
    }

    public Cluster Cluster { get; }

    public void Dispose()
    {
        DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    public ValueTask DisposeAsync()
        => _services.DisposeAsync();
}
