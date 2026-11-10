using KubeUI.Features.Cilium.Hubble;
using KubeUI.Features.Cilium.Hubble.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using ServiceScan.SourceGenerator;

namespace KubeUI.Features.Cilium;

public static partial class KubeUICiliumServiceCollectionExtensions
{
    /// <summary>Registers Cilium runtime services and Hubble views.</summary>
    /// <param name="services">Service collection to configure.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddKubeUICiliumFeature(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddCilium();
        services.AddKubeUICiliumGeneratedServices();
        services.AddViewFactories(GetViewFactories());
        return services;
    }

    [GenerateServiceRegistrations(AssignableTo = typeof(UserControl), Lifetime = ServiceLifetime.Transient, AsSelf = true, AsImplementedInterfaces = false, AssemblyNameFilter = "KubeUI.Features.Cilium")]
    [GenerateServiceRegistrations(AssignableTo = typeof(ViewModelBase), Lifetime = ServiceLifetime.Transient, AsSelf = true, AsImplementedInterfaces = false, AssemblyNameFilter = "KubeUI.Features.Cilium")]
    [GenerateServiceRegistrations(AssignableTo = typeof(ViewBase<>), Lifetime = ServiceLifetime.Transient, AsSelf = true, AsImplementedInterfaces = false, AssemblyNameFilter = "KubeUI.Features.Cilium")]
    private static partial IServiceCollection AddKubeUICiliumGeneratedServices(this IServiceCollection services);

    [ScanForTypes(AssignableTo = typeof(IViewFactoryDefinition), Handler = nameof(ViewFactoryDiscovery.Create))]
    private static partial IViewFactory[] GetViewFactories();

    /// <summary>Registers Cilium and Hubble services.</summary>
    /// <param name="services">Service collection to configure.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddCilium(this IServiceCollection services)
    {
        services.AddOptions<HubbleConnectionOptions>();
        services.TryAddSingleton<HubbleClientFactory>();
        services.TryAddSingleton<IHubbleConnectionFactory>(serviceProvider =>
            serviceProvider.GetRequiredService<HubbleClientFactory>());
        return services;
    }
}
