using KubeUI.Resources;
using KubeUI.ViewModels;
using ServiceScan.SourceGenerator;

namespace KubeUI;

public static partial class KubeUIPresentationServiceCollectionExtensions
{
    public static IServiceCollection AddKubeUIPresentationServices(this IServiceCollection services)
    {
        return services.AddKubeUIPresentationGeneratedServices();
    }

    [GenerateServiceRegistrations(AssignableTo = typeof(UserControl), Lifetime = ServiceLifetime.Transient, AsSelf = true, AsImplementedInterfaces = false, AssemblyNameFilter = "KubeUI.Presentation")]
    [GenerateServiceRegistrations(AssignableTo = typeof(ViewModelBase), Lifetime = ServiceLifetime.Transient, AsSelf = true, AsImplementedInterfaces = false, AssemblyNameFilter = "KubeUI.Presentation")]
    [GenerateServiceRegistrations(AssignableTo = typeof(ResourceConfigBase<>), Lifetime = ServiceLifetime.Transient, AsSelf = false, AsImplementedInterfaces = false, AssemblyNameFilter = "KubeUI.Presentation")]
    [GenerateServiceRegistrations(AssignableTo = typeof(CRDResourceConfig<>), Lifetime = ServiceLifetime.Transient, AsSelf = true, AsImplementedInterfaces = false, AssemblyNameFilter = "KubeUI.Presentation")]
    private static partial IServiceCollection AddKubeUIPresentationGeneratedServices(this IServiceCollection services);
}
