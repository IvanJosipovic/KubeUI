using KubeUI.Features.Clusters.Workspace;
using KubeUI.Features.Resources;
using KubeUI.Features.Resources.Icons;
using KubeUI.Features.Resources.List;
using KubeUI.Features.Resources.Properties;
using KubeUI.Features.Resources.Visualization;
using KubeUI.Features.Resources.Workloads.v1.Pod.Services;
using KubeUI.Features.Resources.Workloads.v1.Pod.ViewModels;
using KubeUI.Features.Resources.Yaml;
using ServiceScan.SourceGenerator;

namespace KubeUI.Features.Resources.DependencyInjection;

public static partial class KubeUIResourcesFeatureServiceCollectionExtensions
{
    public static IServiceCollection AddKubeUIResourcesFeature(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IClusterCustomResourceConfigFactory, CustomResourceConfigFactory>();
        services.AddSingleton<ResourceIconService>();
        services.AddSingleton<IResourceIconService>(sp => sp.GetRequiredService<ResourceIconService>());
        services.AddSingleton<IResourceVisualizationService, ResourceVisualizationService>();
        services.AddSingleton<IResourceListDocumentFactory>(static _ => new ResourceListDocumentFactory());
        services.AddSingleton<IYamlValidationService, YamlSyntaxValidationService>();
        services.AddTransient(typeof(ResourceListViewModel<>));
        services.AddTransient<Func<VisualizationViewModel>>(sp => () => sp.GetRequiredService<VisualizationViewModel>());
        services.AddTransient<Func<ResourceYamlViewModel>>(sp => () => sp.GetRequiredService<ResourceYamlViewModel>());
        services.AddTransient<Func<PodLogsViewModel>>(sp => () => sp.GetRequiredService<PodLogsViewModel>());
        services.AddTransient(static serviceProvider => new CustomResourceConfig(serviceProvider));
        services.AddTransient<IPodLogExportService, PodLogExportService>();
        services.AddTransient<IPodLogsLauncher, PodLogsLauncher>();
        services.AddViewFactories(GetViewFactories());
        services.AddKubeUIResourcesGeneratedServices();
        services.AddKubeUIResourceConfigServices();
        services.AddKubeUIResourcesCustomConfigServices();
        return services;
    }

    [GenerateServiceRegistrations(AssignableTo = typeof(UserControl), Lifetime = ServiceLifetime.Transient, AsSelf = true, AsImplementedInterfaces = false, AssemblyNameFilter = "KubeUI.Features.Resources")]
    [GenerateServiceRegistrations(AssignableTo = typeof(ViewModelBase), Lifetime = ServiceLifetime.Transient, AsSelf = true, AsImplementedInterfaces = false, AssemblyNameFilter = "KubeUI.Features.Resources")]
    [GenerateServiceRegistrations(AssignableTo = typeof(ViewBase<>), Lifetime = ServiceLifetime.Transient, AsSelf = true, AsImplementedInterfaces = false, AssemblyNameFilter = "KubeUI.Features.Resources")]
    [GenerateServiceRegistrations(AssignableTo = typeof(RefreshingCellTextBlock), Lifetime = ServiceLifetime.Transient, AsSelf = true, AsImplementedInterfaces = false, AssemblyNameFilter = "KubeUI.Features.Resources")]
    private static partial IServiceCollection AddKubeUIResourcesGeneratedServices(this IServiceCollection services);

    [GenerateServiceRegistrations(AssignableTo = typeof(ResourceConfigBase<>), Lifetime = ServiceLifetime.Transient, AsSelf = true, AsImplementedInterfaces = true, AssemblyNameFilter = "KubeUI.Features.Resources")]
    private static partial IServiceCollection AddKubeUIResourceConfigServices(this IServiceCollection services);

    [GenerateServiceRegistrations(AssignableTo = typeof(CustomResourceConfig), Lifetime = ServiceLifetime.Transient, ExcludeByTypeName = "KubeUI.Features.Resources.CustomResourceConfig", AssemblyNameFilter = "KubeUI.Features.Resources")]
    private static partial IServiceCollection AddKubeUIResourcesCustomConfigServices(this IServiceCollection services);

    [ScanForTypes(AssignableTo = typeof(IViewFactoryDefinition), Handler = nameof(ViewFactoryDiscovery.Create))]
    private static partial IViewFactory[] GetViewFactories();
}
