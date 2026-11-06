using Avalonia.Controls.Templates;
using Avalonia.Logging;
using Dock.Model.Core;
using KubeUI.App.Infrastructure;
using KubeUI.App.Infrastructure.Logging;
using KubeUI.App.Infrastructure.Platform;
using KubeUI.App.Infrastructure.Presentation;
using KubeUI.App.Infrastructure.Threading;
using KubeUI.App.Services.AI;
using KubeUI.App.Services.FeatureFlags;
using KubeUI.App.Services.Settings;
using KubeUI.App.Shell.Main;
using KubeUI.App.Shell.Navigation;
using KubeUI.Features.AI.Chat;
using KubeUI.Features.AI.DependencyInjection;
using KubeUI.Features.Azure.DependencyInjection;
using KubeUI.Features.Cilium.Hubble;
using KubeUI.Features.Clusters.DependencyInjection;
using KubeUI.Features.Clusters.Error;
using KubeUI.Features.Clusters.Workspace;
using KubeUI.Features.Crossplane.DependencyInjection;
using KubeUI.Features.GatewayApi.DependencyInjection;
using KubeUI.Features.Helm;
using KubeUI.Features.Helm.DependencyInjection;
using KubeUI.Features.Resources;
using KubeUI.Features.Resources.DependencyInjection;
using KubeUI.Features.Resources.Settings;
using KubeUI.Kubernetes;
using Microsoft.Extensions.Http.Resilience;
using ServiceScan.SourceGenerator;

namespace KubeUI.App.Infrastructure.DependencyInjection;

public static partial class KubeUIShellServiceCollectionExtensions
{
    public static IServiceCollection AddKubeUIAvaloniaServices(this IServiceCollection services)
    {
        services.AddKubeUIResourcesFeature();
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
        services.AddSingleton<IClusterInstrumentation>(sp => sp.GetRequiredService<Instrumentation>());
        services.AddSingleton<ILogSink, LogSink>();
        services.AddSingleton<IApplicationEventSource, ApplicationEventSource>();
        services.AddSingleton<IApplicationTelemetryFlusher, ApplicationTelemetryFlusher>();
        services.AddSingleton<ApplicationShutdownCoordinator>();
        services.AddTransient<Func<ClusterErrorViewModel>>(sp => () => sp.GetRequiredService<ClusterErrorViewModel>());
        services.AddSingleton<IClusterErrorDocumentPresenter>(sp => new ClusterErrorDocumentPresenter(
            () => sp.GetRequiredService<IFactory>(),
            sp.GetRequiredService<Func<ClusterErrorViewModel>>()));
        services.AddSingleton<ApplicationErrorHandler>();
        services.AddViewFactories(GetViewFactories());
        services.AddSingleton<ViewLocator>();
        services.AddSingleton<IDataTemplate>(sp => sp.GetRequiredService<ViewLocator>());
        services.AddSingleton<ISettingsPersistence, FileSettingsPersistence>();
        services.AddSingleton<SettingsService>();
        services.AddSingleton<ISettingsService>(sp => sp.GetRequiredService<SettingsService>());
        services.AddSingleton<IResourceAppearanceSettings>(sp => sp.GetRequiredService<SettingsService>());
        services.AddSingleton(sp => sp.GetRequiredService<ISettingsService>() as IClusterPreferenceStore
            ?? throw new InvalidOperationException("The settings service must provide cluster preferences."));
        services.AddSingleton<IFeatureProvider, SettingsFeatureProvider>();
        services.AddSingleton<IFeatureClient, FeatureClient>();
        services.AddSingleton<INavigationSectionProvider>(sp => new HelmNavigationSectionProvider(
            sp.GetRequiredService<IFeatureClient>().GetBooleanValue(FeatureFlagKeys.Helm, false)));
        services.AddSingleton<INavigationSectionProvider>(sp => new HubbleNavigationSectionProvider(
            sp.GetRequiredService<IFeatureClient>().GetBooleanValue(FeatureFlagKeys.Hubble, false)));
        services.AddSingleton<NavigationSectionCatalog>();
        services.AddKubeUIAIFeature();
        services.AddKubeUIAzureFeature();
        services.AddKubeUIClustersFeature();
        services.AddKubeUICrossplaneFeature();
        services.AddKubeUIGatewayApiFeature();
        services.AddKubeUIHelmFeature();
        services.AddSingleton<IAgentChatSettings, AgentChatSettings>();
        services.AddSingleton<IClusterSettingsStore>(sp => sp.GetRequiredService<ISettingsService>());
        return services;
    }

    [GenerateServiceRegistrations(AssignableTo = typeof(Window), Lifetime = ServiceLifetime.Transient, AsSelf = true, AsImplementedInterfaces = false, AssemblyNameFilter = "KubeUI.App")]
    [GenerateServiceRegistrations(AssignableTo = typeof(UserControl), Lifetime = ServiceLifetime.Transient, AsSelf = true, AsImplementedInterfaces = false, AssemblyNameFilter = "KubeUI.App")]
    [GenerateServiceRegistrations(AssignableTo = typeof(ViewModelBase), Lifetime = ServiceLifetime.Transient, AsSelf = true, AsImplementedInterfaces = false, AssemblyNameFilter = "KubeUI.App")]
    [GenerateServiceRegistrations(AssignableTo = typeof(ViewBase<>), Lifetime = ServiceLifetime.Transient, AsSelf = true, AsImplementedInterfaces = false, AssemblyNameFilter = "KubeUI.App")]
    private static partial IServiceCollection AddKubeUIShellGeneratedServices(this IServiceCollection services);

    [ScanForTypes(AssignableTo = typeof(IViewFactoryDefinition), Handler = nameof(ViewFactoryDiscovery.Create))]
    private static partial IViewFactory[] GetViewFactories();
}
