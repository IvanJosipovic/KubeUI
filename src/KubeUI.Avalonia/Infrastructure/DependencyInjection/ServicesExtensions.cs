using Avalonia.Controls.Templates;
using Avalonia.Logging;
using KubeUI.AI.Agents;
using KubeUI.AI.Configuration;
using KubeUI.AI.Permissions;
using KubeUI.Avalonia.Features.AI;
using KubeUI.Avalonia.Features.Crossplane;
using KubeUI.Avalonia.Features.Crossplane.MRDiffDetection;
using KubeUI.Avalonia.Features.Resources.List.Controls;
using KubeUI.Avalonia.Features.Resources.Yaml;
using KubeUI.Avalonia.Infrastructure.Logging;
using KubeUI.Avalonia.Infrastructure.Platform;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Avalonia.Infrastructure.Threading;
using KubeUI.Avalonia.Resources;
using KubeUI.Avalonia.Resources.Workloads.v1.Pod.Services;
using KubeUI.Avalonia.Resources.Workloads.v1.Pod.ViewModels;
using KubeUI.Avalonia.Services.Icons;
using KubeUI.Avalonia.Services.Settings;
using KubeUI.Avalonia.Shell.Main;
using KubeUI.Avalonia.Shell.Navigation;
using KubeUI.Kubernetes;
using Microsoft.Extensions.Http.Resilience;
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
        services.AddHttpClient(MainViewModel.GitHubReleasesHttpClientName, client =>
        {
            client.DefaultRequestHeaders.UserAgent.ParseAdd("KubeUI");
            client.DefaultRequestHeaders.Accept.ParseAdd("application/vnd.github+json");
        }).AddStandardResilienceHandler();
        services.AddSingleton<IPlatformServices, AvaloniaPlatformServices>();
        services.AddSingleton<IUiRefreshClock, AvaloniaUiRefreshClock>();
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<Instrumentation>();
        services.AddSingleton<IYamlValidationService, YamlSyntaxValidationService>();
        services.AddSingleton<ILogSink, LogSink>();
        services.AddSingleton<ViewLocator>();
        services.AddSingleton<IPodLogExportService, PodLogExportService>();
        services.AddSingleton<Func<PodLogsViewModel>>(sp =>
            () => sp.GetRequiredService<PodLogsViewModel>());
        services.AddSingleton<IPodLogsLauncher, PodLogsLauncher>();
        services.AddSingleton<IDataTemplate>(sp => sp.GetRequiredService<ViewLocator>());
        services.AddSingleton<ISettingsPersistence, FileSettingsPersistence>();
        services.AddSingleton<ISettingsService, SettingsService>();
        services.AddSingleton<IResourceIconService, ResourceIconService>();
        services.AddTransient(static serviceProvider => new CustomResourceConfig(serviceProvider));
        services.AddSingleton<INavigationSectionProvider, CrossplaneNavigationSectionProvider>();
        services.AddSingleton<NavigationSectionCatalog>();
        services.AddSingleton<CrossplaneDiffLogParser>();
        services.AddTransient<CrossplaneProviderLogMonitor>();
        services.AddSingleton<IAgentRegistry>(sp => new AcpAgentRegistry(
            AcpAgentDefaults.Definitions,
            sp.GetRequiredService<IAgentPermissionService>()));
        services.AddSingleton<IAgentPermissionService, AvaloniaAgentPermissionService>();
        services.AddSingleton<IAgentContextService, AgentContextService>();
        services.AddSingleton<IClusterSettingsStore>(sp => sp.GetRequiredService<ISettingsService>());
        return services;
    }

    [GenerateServiceRegistrations(AssignableTo = typeof(Window), Lifetime = ServiceLifetime.Transient, AsSelf = true, AsImplementedInterfaces = false, AssemblyNameFilter = "KubeUI.Avalonia")]
    [GenerateServiceRegistrations(AssignableTo = typeof(UserControl), Lifetime = ServiceLifetime.Transient, AsSelf = true, AsImplementedInterfaces = false, AssemblyNameFilter = "KubeUI.Avalonia")]
    [GenerateServiceRegistrations(AssignableTo = typeof(ViewModelBase), Lifetime = ServiceLifetime.Transient, AsSelf = true, AsImplementedInterfaces = false, AssemblyNameFilter = "KubeUI.Avalonia")]
    [GenerateServiceRegistrations(AssignableTo = typeof(ViewBase<>), Lifetime = ServiceLifetime.Transient, AsSelf = true, AsImplementedInterfaces = false, AssemblyNameFilter = "KubeUI.Avalonia")]
    [GenerateServiceRegistrations(AssignableTo = typeof(RefreshingCellTextBlock), Lifetime = ServiceLifetime.Transient, AsSelf = true, AsImplementedInterfaces = false, AssemblyNameFilter = "KubeUI.Avalonia")]
    private static partial IServiceCollection AddKubeUIShellGeneratedServices(this IServiceCollection services);
}
