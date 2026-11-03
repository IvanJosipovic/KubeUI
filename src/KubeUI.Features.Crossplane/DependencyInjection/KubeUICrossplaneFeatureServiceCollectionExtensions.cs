using KubeUI.Features.Crossplane.MRDiffDetection;

namespace KubeUI.Features.Crossplane.DependencyInjection;

/// <summary>Registers Crossplane feature services and views.</summary>
public static partial class KubeUICrossplaneFeatureServiceCollectionExtensions
{
    /// <summary>Adds Crossplane feature services to the service collection.</summary>
    /// <param name="services">Service collection to configure.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddKubeUICrossplaneFeature(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddSingleton<INavigationSectionProvider, CrossplaneNavigationSectionProvider>();
        services.AddSingleton<CrossplaneDiffLogParser>();
        services.AddTransient<CrossplaneProviderLogMonitor>();
        return services.AddKubeUICrossplaneGeneratedServices();
    }

    [GenerateServiceRegistrations(AssignableTo = typeof(UserControl), Lifetime = ServiceLifetime.Transient, AsSelf = true, AsImplementedInterfaces = false, AssemblyNameFilter = "KubeUI.Features.Crossplane")]
    [GenerateServiceRegistrations(AssignableTo = typeof(ViewModelBase), Lifetime = ServiceLifetime.Transient, AsSelf = true, AsImplementedInterfaces = false, AssemblyNameFilter = "KubeUI.Features.Crossplane")]
    [GenerateServiceRegistrations(AssignableTo = typeof(ViewBase<>), Lifetime = ServiceLifetime.Transient, AsSelf = true, AsImplementedInterfaces = false, AssemblyNameFilter = "KubeUI.Features.Crossplane")]
    private static partial IServiceCollection AddKubeUICrossplaneGeneratedServices(this IServiceCollection services);
}
