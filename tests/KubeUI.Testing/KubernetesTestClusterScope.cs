using Microsoft.Extensions.DependencyInjection;

namespace KubeUI.Testing;

public sealed class KubernetesTestClusterScope : IDisposable, IAsyncDisposable
{
    private readonly ServiceProvider _services;

    public KubernetesTestClusterScope()
    {
        _services = KubernetesTestServices.Build(new KubernetesTestSettingsStore());
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
