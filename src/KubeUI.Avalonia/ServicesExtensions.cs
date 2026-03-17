using Avalonia.Controls.Templates;
using Avalonia.Logging;
using KubeUI.Client;
using KubeUI.ViewModels;
using Microsoft.Extensions.Hosting;
using ServiceScan.SourceGenerator;

namespace KubeUI;

public static partial class KubeUIShellServiceCollectionExtensions
{
    public static IServiceCollection AddKubeUIAvaloniaServices(this IServiceCollection services)
    {
        services.AddKubeUIPresentationServices();
        return services.AddKubeUIShellServices();
    }

    public static IServiceCollection AddKubeUIShellServices(this IServiceCollection services)
    {
        services.AddKubeUIShellGeneratedServices();
        services.AddSingleton<Instrumentation>();
        services.AddSingleton<ILogSink, LogSink>();
        services.AddSingleton<ViewLocator>();
        services.AddSingleton<IDataTemplate>(sp => sp.GetRequiredService<ViewLocator>());
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<IHostApplicationLifetime, Host>();
        return services;
    }

    [GenerateServiceRegistrations(AssignableTo = typeof(Window), Lifetime = ServiceLifetime.Transient, AsSelf = true, AsImplementedInterfaces = false, AssemblyNameFilter = "KubeUI.Avalonia")]
    [GenerateServiceRegistrations(AssignableTo = typeof(UserControl), Lifetime = ServiceLifetime.Transient, AsSelf = true, AsImplementedInterfaces = false, AssemblyNameFilter = "KubeUI.Avalonia")]
    [GenerateServiceRegistrations(AssignableTo = typeof(ViewModelBase), Lifetime = ServiceLifetime.Transient, AsSelf = true, AsImplementedInterfaces = false, AssemblyNameFilter = "KubeUI.Avalonia")]
    private static partial IServiceCollection AddKubeUIShellGeneratedServices(this IServiceCollection services);
}

