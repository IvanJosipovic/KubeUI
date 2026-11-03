using KubeUI.Features.Helm.Charts;
using KubeUI.Features.Helm.Helm;
using KubeUI.Features.Helm.Releases;
using KubeUI.Features.Helm.Repositories;
using ServiceScan.SourceGenerator;

namespace KubeUI.Features.Helm.DependencyInjection;

/// <summary>Registers Helm feature services and views.</summary>
public static partial class KubeUIHelmFeatureServiceCollectionExtensions
{
    /// <summary>Adds Helm feature services to the service collection.</summary>
    /// <param name="services">Service collection to configure.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddKubeUIHelmFeature(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<IHelmListNotifier, HelmListNotifier>();
        services.AddTransient<IHelmDocumentOpener, HelmDocumentOpener>();
        services.AddTransient<Func<HelmChartsViewModel>>(sp => () => sp.GetRequiredService<HelmChartsViewModel>());
        services.AddTransient<Func<HelmReleaseEditorViewModel>>(sp => () => sp.GetRequiredService<HelmReleaseEditorViewModel>());
        services.AddTransient<Func<HelmRepositoryEditorViewModel>>(sp => () => sp.GetRequiredService<HelmRepositoryEditorViewModel>());
        services.AddTransient<IHelmReleasePreviewService, HelmReleasePreviewService>();
        
        // Helm service infrastructure
        services.AddSingleton<IHelmClientFactory, HelmClientFactory>();
        services.AddSingleton<IHelmService, HelmService>();
        
        return services.AddKubeUIHelmGeneratedServices();
    }

    [GenerateServiceRegistrations(AssignableTo = typeof(UserControl), Lifetime = ServiceLifetime.Transient, AsSelf = true, AsImplementedInterfaces = false, AssemblyNameFilter = "KubeUI.Features.Helm")]
    [GenerateServiceRegistrations(AssignableTo = typeof(ViewModelBase), Lifetime = ServiceLifetime.Transient, AsSelf = true, AsImplementedInterfaces = false, AssemblyNameFilter = "KubeUI.Features.Helm")]
    [GenerateServiceRegistrations(AssignableTo = typeof(ViewBase<>), Lifetime = ServiceLifetime.Transient, AsSelf = true, AsImplementedInterfaces = false, AssemblyNameFilter = "KubeUI.Features.Helm")]
    private static partial IServiceCollection AddKubeUIHelmGeneratedServices(this IServiceCollection services);
}
