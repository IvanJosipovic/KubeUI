using KubeUI.Features.Cilium.Hubble;
using KubeUI.Features.Cilium.Hubble.Controls;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace KubeUI.Features.Cilium;

public static class KubeUICiliumServiceCollectionExtensions
{
    /// <summary>Registers Cilium runtime services and Hubble views.</summary>
    /// <param name="services">Service collection to configure.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddKubeUICiliumFeature(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);

        services.AddCilium();
        services.AddTransient<FlowListViewModel>();
        services.AddTransient<ServiceMapViewModel>();
        services.AddTransient<ServiceMapNodePropertiesViewModel>();
        services.AddTransient<Func<FlowListViewModel>>(sp => () => sp.GetRequiredService<FlowListViewModel>());
        services.AddTransient<Func<ServiceMapViewModel>>(sp => () => sp.GetRequiredService<ServiceMapViewModel>());
        services.AddTransient<FlowListView>();
        services.AddTransient<ServiceMapView>();
        services.AddTransient<ServiceMapNodePropertiesView>();
        services.AddTransient<ServiceMapGraphControl>();
        return services;
    }

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
