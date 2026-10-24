using KubeUI.Cilium.Hubble;
using Microsoft.Extensions.DependencyInjection;

namespace KubeUI.Cilium;

public static class KubeUICiliumServiceCollectionExtensions
{
    public static IServiceCollection AddCilium(this IServiceCollection services)
    {
        services.AddSingleton<HubbleClientFactory>();
        return services;
    }
}