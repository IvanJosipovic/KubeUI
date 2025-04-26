using Avalonia.Controls.Templates;
using Avalonia.Logging;
using Dock.Model.Core;
using KubernetesCRDModelGen;
using KubeUI.Client;
using KubeUI.Resources;
using KubeUI.Views;

namespace KubeUI;
public static partial class ServicesExtensions
{
    //[GenerateServiceRegistrations(AssignableTo = typeof(UserControl), Lifetime = ServiceLifetime.Transient, AsSelf = true, AsImplementedInterfaces = true)]
    //[GenerateServiceRegistrations(AssignableTo = typeof(ObservableObject), Lifetime = ServiceLifetime.Transient, AsSelf = true, AsImplementedInterfaces = false)]
    [GenerateServiceRegistrations(AssignableTo = typeof(ViewModelBase), Lifetime = ServiceLifetime.Transient, AsSelf = true, AsImplementedInterfaces = false)]
    [GenerateServiceRegistrations(AssignableTo = typeof(MyViewBase<>), Lifetime = ServiceLifetime.Transient, AsSelf = true, AsImplementedInterfaces = false)]
    [GenerateServiceRegistrations(AssignableTo = typeof(ResourceConfigBase<>), Lifetime = ServiceLifetime.Transient, AsImplementedInterfaces = false)]

    [GenerateServiceRegistrations(AssignableTo = typeof(ICluster), Lifetime = ServiceLifetime.Transient, AsSelf = false, AsImplementedInterfaces = true)]

    [GenerateServiceRegistrations(AssignableTo = typeof(ModelCache), Lifetime = ServiceLifetime.Transient)]


    [GenerateServiceRegistrations(AssignableTo = typeof(Instrumentation), Lifetime = ServiceLifetime.Singleton)]
    [GenerateServiceRegistrations(AssignableTo = typeof(ILogSink), Lifetime = ServiceLifetime.Singleton, AsImplementedInterfaces = true)]
    [GenerateServiceRegistrations(AssignableTo = typeof(ISettingsService), Lifetime = ServiceLifetime.Singleton, AsImplementedInterfaces = true)]
    [GenerateServiceRegistrations(AssignableTo = typeof(IDataTemplate), Lifetime = ServiceLifetime.Singleton, AsSelf = true)]
    [GenerateServiceRegistrations(AssignableTo = typeof(ClusterManager), Lifetime = ServiceLifetime.Singleton)]
    [GenerateServiceRegistrations(AssignableTo = typeof(IFactory), Lifetime = ServiceLifetime.Singleton)]

    [GenerateServiceRegistrations(FromAssemblyOf = typeof(IGenerator), AssignableTo = typeof(IGenerator), Lifetime = ServiceLifetime.Singleton)]

    public static partial IServiceCollection AddServices(this IServiceCollection services);
}
