using KubeUI.Features.Azure.Client.AzureMonitor;
using KubeUI.Kubernetes;
using Microsoft.Extensions.DependencyInjection;

namespace KubeUI.Features.Azure.DependencyInjection;

/// <summary>Registers Azure integration services used by KubeUI features.</summary>
public static class KubeUIAzureFeatureServiceCollectionExtensions
{
    /// <summary>Registers Azure integration services.</summary>
    /// <param name="services">Service collection to configure.</param>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection AddKubeUIAzureFeature(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        services.AddSingleton<IAksClusterService, AksClusterService>();
        services.AddSingleton<IAzureMonitorWorkspaceService, AzureMonitorWorkspaceService>();
        return services;
    }
}
