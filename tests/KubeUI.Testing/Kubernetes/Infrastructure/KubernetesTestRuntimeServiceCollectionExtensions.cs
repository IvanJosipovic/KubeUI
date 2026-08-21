using KubeUI.Testing.Kubernetes.Bootstrap;
using KubernetesClient.Informer.Client;
using k8s.Models;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using KubeUI.Kubernetes;

namespace KubeUI.Testing.Kubernetes.Infrastructure;

public static class KubernetesTestRuntimeServiceCollectionExtensions
{
    public static IServiceCollection AddKubernetesTestRuntime(this IServiceCollection services)
    {
        services.RemoveAll<KubernetesModelCatalog>();
        services.AddSingleton(sp =>
        {
            var catalog = new KubernetesModelCatalog();
            catalog.Register(GroupApiVersionKind.From<V1Pod>(), typeof(V1Pod));
            return catalog;
        });
        services.AddSingleton<TestClusterGenerator>();
        services.AddSingleton<TestClusterGeneratorCleanup>();
        services.AddSingleton<TestClusterConfig>();
        services.AddSingleton(sp =>
            sp.GetService<ClusterManager>()!.Clusters.Single());

        return services;
    }
}

public sealed class TestClusterGeneratorCleanup : IAsyncDisposable
{
    private readonly TestClusterGenerator _generator;
    private int _disposed;

    public TestClusterGeneratorCleanup(TestClusterGenerator generator)
    {
        _generator = generator;
        AppDomain.CurrentDomain.ProcessExit += OnProcessExit;
    }

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposed, 1) != 0)
        {
            return;
        }

        AppDomain.CurrentDomain.ProcessExit -= OnProcessExit;
        await _generator.ResetAsync().ConfigureAwait(false);
    }

    private void OnProcessExit(object? sender, EventArgs e)
    {
        DisposeAsync().AsTask().GetAwaiter().GetResult();
    }
}
