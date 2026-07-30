using Microsoft.Extensions.DependencyInjection;

namespace KubeUI.Testing;

public sealed class KubernetesTestClusterScope : IDisposable
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
        _services.Dispose();
    }
}
