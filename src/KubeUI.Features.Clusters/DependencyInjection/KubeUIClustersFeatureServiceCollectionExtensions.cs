using KubeUI.Features.Clusters.Catalog;
using KubeUI.Features.Clusters.Error;
using KubeUI.Features.Clusters.Settings;
using KubeUI.Features.Clusters.Workspace;

namespace KubeUI.Features.Clusters.DependencyInjection;

public static class KubeUIClustersFeatureServiceCollectionExtensions
{
    public static IServiceCollection AddKubeUIClustersFeature(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<ClusterWorkspacePreferences>();
        services.AddSingleton<ClusterWorkspaceCatalog>();
        services.AddTransient<ClusterListView>();
        services.AddTransient<ClusterListViewModel>();
        services.AddTransient<ClusterErrorView>();
        services.AddTransient<ClusterErrorViewModel>();
        services.AddTransient<ClusterSettingsView>();
        services.AddTransient<ClusterSettingsViewModel>();
        return services;
    }
}
