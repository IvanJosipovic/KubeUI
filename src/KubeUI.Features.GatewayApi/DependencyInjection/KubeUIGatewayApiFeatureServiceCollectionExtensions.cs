using KubeUI.Features.Resources;
using ServiceScan.SourceGenerator;

namespace KubeUI.Features.GatewayApi.DependencyInjection;

/// <summary>Registers Gateway API resource configurations.</summary>
public static partial class KubeUIGatewayApiFeatureServiceCollectionExtensions
{
    /// <summary>Adds Gateway API resource configurations.</summary>
    /// <param name="services">Service collection to configure.</param>
    /// <returns>The same service collection for chaining.</returns>
    public static IServiceCollection AddKubeUIGatewayApiFeature(this IServiceCollection services)
    {
        ArgumentNullException.ThrowIfNull(services);
        return services.AddKubeUIGatewayApiResourceConfigs();
    }

    [GenerateServiceRegistrations(AssignableTo = typeof(ResourceConfigBase<>), Lifetime = ServiceLifetime.Transient, AsSelf = true, AsImplementedInterfaces = true, AssemblyNameFilter = "KubeUI.Features.GatewayApi")]
    private static partial IServiceCollection AddKubeUIGatewayApiResourceConfigs(this IServiceCollection services);
}
