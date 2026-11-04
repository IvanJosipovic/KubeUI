using KubeUI.Features.Clusters.Workspace;
using ServiceScan.SourceGenerator;

namespace KubeUI.Features.Clusters.DependencyInjection;

public static partial class KubeUIClustersFeatureServiceCollectionExtensions
{
    public static IServiceCollection AddKubeUIClustersFeature(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<ClusterWorkspacePreferences>();
        services.AddSingleton<ClusterWorkspaceCatalog>();
        services.AddKubeUIClustersGeneratedServices();
        services.AddViewFactories(GetViewFactories());
        return services;
    }

    [GenerateServiceRegistrations(AssignableTo = typeof(UserControl), Lifetime = ServiceLifetime.Transient, AsSelf = true, AsImplementedInterfaces = false, AssemblyNameFilter = "KubeUI.Features.Clusters")]
    [GenerateServiceRegistrations(AssignableTo = typeof(ViewModelBase), Lifetime = ServiceLifetime.Transient, AsSelf = true, AsImplementedInterfaces = false, AssemblyNameFilter = "KubeUI.Features.Clusters")]
    [GenerateServiceRegistrations(AssignableTo = typeof(ViewBase<>), Lifetime = ServiceLifetime.Transient, AsSelf = true, AsImplementedInterfaces = false, AssemblyNameFilter = "KubeUI.Features.Clusters")]
    private static partial IServiceCollection AddKubeUIClustersGeneratedServices(this IServiceCollection services);

    [ScanForTypes(AssignableTo = typeof(IViewFactoryDefinition), Handler = nameof(ViewFactoryDiscovery.Create))]
    private static partial IViewFactory[] GetViewFactories();
}
