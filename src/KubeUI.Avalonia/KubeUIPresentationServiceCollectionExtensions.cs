using KubeUI.Resources;
using KubeUI.ViewModels;
using ServiceScan.SourceGenerator;

namespace KubeUI;

public static partial class KubeUIPresentationServiceCollectionExtensions
{
    public static IServiceCollection AddKubeUIPresentationServices(this IServiceCollection services)
    {
        services.AddSingleton<ClusterWorkspaceCatalog>();
        return services.AddKubeUIPresentationGeneratedServices();
    }

    
    
    [GenerateServiceRegistrations(AssignableTo = typeof(ResourceConfigBase<>), Lifetime = ServiceLifetime.Transient, AsSelf = false, AsImplementedInterfaces = false, AssemblyNameFilter = "KubeUI.Avalonia")]
    [GenerateServiceRegistrations(AssignableTo = typeof(CRDResourceConfig<>), Lifetime = ServiceLifetime.Transient, AsSelf = true, AsImplementedInterfaces = false, AssemblyNameFilter = "KubeUI.Avalonia")]
    private static partial IServiceCollection AddKubeUIPresentationGeneratedServices(this IServiceCollection services);
}

