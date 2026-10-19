using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Resources;
using ServiceScan.SourceGenerator;

namespace KubeUI.Avalonia.Infrastructure.DependencyInjection;

public static partial class KubeUIAvaloniaResourceServiceCollectionExtensions
{
    public static IServiceCollection AddKubeUIAvaloniaResourceServices(this IServiceCollection services)
    {
        services.AddSingleton<ClusterWorkspaceCatalog>();
        services.AddKubeUIAvaloniaCustomResourceConfigs();
        return services.AddKubeUIAvaloniaResourceGeneratedServices();
    }

    [GenerateServiceRegistrations(
        AssignableTo = typeof(CustomResourceConfig),
        Lifetime = ServiceLifetime.Transient,
        ExcludeByTypeName = "KubeUI.Avalonia.Resources.CustomResourceConfig,KubeUI.Avalonia.Features.GatewayApi.GatewayApiCustomResourceConfig",
        AssemblyNameFilter = "KubeUI.Avalonia")]
    private static partial IServiceCollection AddKubeUIAvaloniaCustomResourceConfigs(this IServiceCollection services);

    [GenerateServiceRegistrations(AssignableTo = typeof(ResourceConfigBase<>), Lifetime = ServiceLifetime.Transient, AsSelf = true, AsImplementedInterfaces = true, AssemblyNameFilter = "KubeUI.Avalonia")]
    private static partial IServiceCollection AddKubeUIAvaloniaResourceGeneratedServices(this IServiceCollection services);
}
