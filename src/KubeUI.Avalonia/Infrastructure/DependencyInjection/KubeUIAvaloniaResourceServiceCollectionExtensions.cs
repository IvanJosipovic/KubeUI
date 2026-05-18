using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Resources;
using ServiceScan.SourceGenerator;

namespace KubeUI.Avalonia.Infrastructure.DependencyInjection;

public static partial class KubeUIAvaloniaResourceServiceCollectionExtensions
{
    public static IServiceCollection AddKubeUIAvaloniaResourceServices(this IServiceCollection services)
    {
        services.AddSingleton<ClusterWorkspaceCatalog>();
        return services.AddKubeUIAvaloniaResourceGeneratedServices();
    }

    [GenerateServiceRegistrations(AssignableTo = typeof(ResourceConfigBase<>), Lifetime = ServiceLifetime.Transient, AsSelf = true, AsImplementedInterfaces = false, AssemblyNameFilter = "KubeUI.Avalonia")]
    [GenerateServiceRegistrations(AssignableTo = typeof(CRDResourceConfig<>), Lifetime = ServiceLifetime.Transient, AsSelf = true, AsImplementedInterfaces = false, AssemblyNameFilter = "KubeUI.Avalonia")]
    private static partial IServiceCollection AddKubeUIAvaloniaResourceGeneratedServices(this IServiceCollection services);
}
