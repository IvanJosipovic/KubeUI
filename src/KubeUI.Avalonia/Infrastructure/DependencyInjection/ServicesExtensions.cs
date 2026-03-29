using Avalonia.Controls.Templates;
using Avalonia.Logging;
using KubeUI.Avalonia.Controls.DataGridFilters;
using KubeUI.Avalonia.Infrastructure.Logging;
using KubeUI.Kubernetes;
using KubeUI.Avalonia.Infrastructure.Presentation;
using Microsoft.Extensions.Hosting;
using ServiceScan.SourceGenerator;

namespace KubeUI.Avalonia.Infrastructure.DependencyInjection;

public static partial class KubeUIShellServiceCollectionExtensions
{
    public static IServiceCollection AddKubeUIAvaloniaServices(this IServiceCollection services)
    {
        services.AddKubeUIAvaloniaResourceServices();
        return services.AddKubeUIShellServices();
    }

    public static IServiceCollection AddKubeUIShellServices(this IServiceCollection services)
    {
        services.AddKubeUIShellGeneratedServices();
        services.AddSingleton<Instrumentation>();
        services.AddSingleton<IYamlValidationService, YamlSyntaxValidationService>();
        services.AddSingleton<ILogSink, LogSink>();
        services.AddSingleton<ViewLocator>();
        services.AddSingleton<DataGridColumnFilterService>();
        services.AddSingleton<DataGridColumnFilterFlyoutFactory>();
        services.AddSingleton<IDataTemplate>(sp => sp.GetRequiredService<ViewLocator>());
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<IClusterSettingsStore>(sp => sp.GetRequiredService<ISettingsService>());
        services.AddSingleton<IHostApplicationLifetime, KubeUI.Avalonia.Infrastructure.Hosting.Host>();
        return services;
    }

    [GenerateServiceRegistrations(AssignableTo = typeof(Window), Lifetime = ServiceLifetime.Transient, AsSelf = true, AsImplementedInterfaces = false, AssemblyNameFilter = "KubeUI.Avalonia")]
    [GenerateServiceRegistrations(AssignableTo = typeof(UserControl), Lifetime = ServiceLifetime.Transient, AsSelf = true, AsImplementedInterfaces = false, AssemblyNameFilter = "KubeUI.Avalonia")]
    [GenerateServiceRegistrations(AssignableTo = typeof(ViewModelBase), Lifetime = ServiceLifetime.Transient, AsSelf = true, AsImplementedInterfaces = false, AssemblyNameFilter = "KubeUI.Avalonia")]
    private static partial IServiceCollection AddKubeUIShellGeneratedServices(this IServiceCollection services);
}




