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
            return sp.GetRequiredService<ClusterManager>().Clusters.Single();
        });

        return services;
    }
}

public sealed class TestClusterGeneratorCleanup(TestClusterGenerator generator) : IAsyncDisposable
{
    public ValueTask DisposeAsync() => generator.ResetAsync();
}
