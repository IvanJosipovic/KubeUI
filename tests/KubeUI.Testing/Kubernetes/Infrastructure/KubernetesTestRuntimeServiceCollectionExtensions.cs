using KubeUI.Testing.Kubernetes.Bootstrap;
using Microsoft.Extensions.DependencyInjection;

namespace KubeUI.Testing.Kubernetes.Infrastructure;

public static class KubernetesTestRuntimeServiceCollectionExtensions
{
    public static IServiceCollection AddKubernetesTestRuntime(this IServiceCollection services)
    {
        services.AddSingleton<TestClusterGenerator>();
        services.AddSingleton<TestClusterGeneratorCleanup>();
        services.AddSingleton<TestClusterConfig>();
        services.AddSingleton<IClusterRuntime>(sp =>
        {
            _ = sp.GetRequiredService<TestClusterGeneratorCleanup>();
            var config = sp.GetRequiredService<TestClusterConfig>();
            var generator = sp.GetRequiredService<TestClusterGenerator>();
            var manager = sp.GetRequiredService<ClusterManager>();
            var cluster = generator.CreateAsync(config).GetAwaiter().GetResult();
            manager.AddCluster(cluster.Cluster);
            return cluster.Cluster;
        });

        return services;
    }
}

internal sealed class TestClusterGeneratorCleanup(TestClusterGenerator generator) : IAsyncDisposable
{
    public ValueTask DisposeAsync() => generator.ResetAsync();
}
