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
            TestClusterConfig config = sp.GetRequiredService<TestClusterConfig>();
            TestClusterGenerator generator = sp.GetRequiredService<TestClusterGenerator>();
            ClusterManager manager = sp.GetRequiredService<ClusterManager>();
            TestCluster cluster = generator.CreateAsync(config).GetAwaiter().GetResult();
            manager.AddCluster(cluster.Cluster);
            return cluster.Cluster;
        });

        return services;
    }
}
