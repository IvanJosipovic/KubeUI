using KubeUI.Testing.Kubernetes.Bootstrap;
using Microsoft.Extensions.DependencyInjection;

namespace KubeUI.Testing.Kubernetes.Infrastructure;

public static class KubernetesTestRuntimeServiceCollectionExtensions
{
    public static IServiceCollection AddKubernetesTestRuntime(this IServiceCollection services)
    {
        services.AddSingleton<TestClusterGenerator>();
        services.AddSingleton<TestClusterConfig>();
        services.AddTransient<IClusterRuntime>(sp =>
        {
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
