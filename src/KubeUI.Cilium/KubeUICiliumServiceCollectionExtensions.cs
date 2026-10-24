using KubeUI.Cilium.Hubble;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace KubeUI.Cilium;

public static class KubeUICiliumServiceCollectionExtensions
{
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
