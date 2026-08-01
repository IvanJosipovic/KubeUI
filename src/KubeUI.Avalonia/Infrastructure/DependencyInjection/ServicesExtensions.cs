using Avalonia.Controls.Templates;
using Avalonia.Logging;
using KubeUI.Avalonia.Controls.DataGridFilters;
using KubeUI.Avalonia.Features.Resources.Yaml;
using KubeUI.Avalonia.Infrastructure.Logging;
using KubeUI.Avalonia.Infrastructure.Platform;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Avalonia.Infrastructure.Threading;
using KubeUI.Avalonia.Services.Settings;
using KubeUI.Avalonia.Services.Icons;
using KubeUI.Kubernetes;
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
        services.AddSingleton<IPlatformServices, AvaloniaPlatformServices>();
        services.AddSingleton<IUiRefreshClock, AvaloniaUiRefreshClock>();
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<Instrumentation>();
        services.AddSingleton<IYamlValidationService, YamlSyntaxValidationService>();
        services.AddSingleton<ILogSink, LogSink>();
        services.AddSingleton<ViewLocator>();
        services.AddSingleton<DataGridColumnFilterService>();
        services.AddSingleton<DataGridColumnFilterFlyoutFactory>();
        services.AddSingleton<IDataTemplate>(sp => sp.GetRequiredService<ViewLocator>());
        services.AddSingleton<ISettingsPersistence, FileSettingsPersistence>();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<IResourceIconService, ResourceIconService>();
        services.AddSingleton<IClusterSettingsStore>(sp => sp.GetRequiredService<ISettingsService>());
        return services;
    }

    [GenerateServiceRegistrations(AssignableTo = typeof(Window), Lifetime = ServiceLifetime.Transient, AsSelf = true, AsImplementedInterfaces = false, AssemblyNameFilter = "KubeUI.Avalonia")]
    [GenerateServiceRegistrations(AssignableTo = typeof(UserControl), Lifetime = ServiceLifetime.Transient, AsSelf = true, AsImplementedInterfaces = false, AssemblyNameFilter = "KubeUI.Avalonia")]
    [GenerateServiceRegistrations(AssignableTo = typeof(ViewModelBase), Lifetime = ServiceLifetime.Transient, AsSelf = true, AsImplementedInterfaces = false, AssemblyNameFilter = "KubeUI.Avalonia")]
    [GenerateServiceRegistrations(AssignableTo = typeof(ViewBase<>), Lifetime = ServiceLifetime.Transient, AsSelf = true, AsImplementedInterfaces = false, AssemblyNameFilter = "KubeUI.Avalonia")]
    private static partial IServiceCollection AddKubeUIShellGeneratedServices(this IServiceCollection services);
}




