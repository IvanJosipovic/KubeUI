using Avalonia.Controls.Templates;
using Avalonia.Logging;
using KubeUI.Features.AI.Chat;
using KubeUI.Features.AI.DependencyInjection;
using KubeUI.Features.Cilium.Hubble;
using KubeUI.Features.Cilium;
using KubeUI.Features.Clusters.Catalog;
using KubeUI.Features.Clusters.DependencyInjection;
using KubeUI.Features.Clusters.Error;
using KubeUI.Features.Clusters.Settings;
using KubeUI.Features.Resources.DependencyInjection;
using KubeUI.Features.Clusters.Workspace;
using KubeUI.Features.Crossplane.DependencyInjection;
using KubeUI.Features.Crossplane.MRDiffDetection;
using KubeUI.Features.GatewayApi.DependencyInjection;
using KubeUI.Features.Helm.DependencyInjection;
using KubeUI.Features.Helm;
using KubeUI.Features.Helm.Charts;
using KubeUI.Features.Helm.Releases;
using KubeUI.Features.Helm.Repositories;
using KubeUI.Features.Resources.List;
using KubeUI.Features.Resources.List.Controls;
using KubeUI.Features.Resources.Visualization;
using KubeUI.Features.Resources.Yaml;
using KubeUI.Features.Resources.Workloads.v1.Deployment;
using KubeUI.Features.Resources.Workloads.v1.Pod;
using KubeUI.Features.Resources.Workloads.v1.Pod.Views;
using KubeUI.App.Infrastructure.Logging;
using KubeUI.App.Infrastructure.Platform;
using KubeUI.App.Infrastructure.Presentation;
using KubeUI.App.Infrastructure.Threading;
using KubeUI.Features.Resources;
using KubeUI.Features.Resources.Workloads.v1.Pod.Services;
using KubeUI.Features.Resources.Workloads.v1.Pod.ViewModels;
using KubeUI.App.Services.FeatureFlags;
using KubeUI.App.Services.AI;
using KubeUI.Features.Resources.Icons;
using KubeUI.App.Services.Settings;
using KubeUI.App.Shell.Main;
using KubeUI.App.Shell.Navigation;
using KubeUI.Kubernetes;
using Microsoft.Extensions.Http.Resilience;
using ServiceScan.SourceGenerator;
using KubeUI.Features.Resources.Settings;

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
        services.AddSingleton<IViewFactory[]>([..GetViewFactories(),
            CreateViewFactory<AgentChatView>(),
            CreateViewFactory<ClusterListView>(),
            CreateViewFactory<ClusterErrorView>(),
            CreateViewFactory<ClusterSettingsView>(),
            CreateViewFactory<FlowListView>(),
            CreateViewFactory<ServiceMapView>(),
            CreateViewFactory<ServiceMapNodePropertiesView>(),
            CreateViewFactory<MRDiffDetectionView>(),
            CreateViewFactory<HelmChartsView>(),
            CreateViewFactory<HelmReleaseEditorView>(),
            CreateViewFactory<HelmReleasesView>(),
            CreateViewFactory<HelmRepositoriesView>(),
            CreateViewFactory<HelmRepositoryEditorView>(),
            CreateViewFactory<ResourceListView>(),
            CreateViewFactory<VisualizationView>(),
            CreateViewFactory<ResourceYamlView>(),
            CreateViewFactory<DeploymentRolloutHistoryView>(),
            CreateViewFactory<PodConsoleView>(),
            CreateViewFactory<PortForwarderListView>(),
            CreateViewFactory<PodLogsView>()]);
        services.AddSingleton<ViewLocator>();
        services.AddSingleton<Func<PodLogsViewModel>>(sp =>
            () => sp.GetRequiredService<PodLogsViewModel>());
        services.AddSingleton<IDataTemplate>(sp => sp.GetRequiredService<ViewLocator>());
        services.AddSingleton<ISettingsPersistence, FileSettingsPersistence>();
        services.AddSingleton<SettingsService>();
        services.AddSingleton<ISettingsService>(sp => sp.GetRequiredService<SettingsService>());
        services.AddSingleton<IResourceAppearanceSettings>(sp => sp.GetRequiredService<SettingsService>());
        services.AddSingleton<IClusterPreferenceStore>(sp => sp.GetRequiredService<ISettingsService>() as IClusterPreferenceStore
            ?? throw new InvalidOperationException("The settings service must provide cluster preferences."));
        services.AddSingleton<IFeatureProvider, SettingsFeatureProvider>();
        services.AddSingleton<IFeatureClient, FeatureClient>();
        services.AddTransient(static serviceProvider => new CustomResourceConfig(serviceProvider));
        services.AddSingleton<INavigationSectionProvider>(sp => new HelmNavigationSectionProvider(
            sp.GetRequiredService<IFeatureClient>().GetBooleanValue(FeatureFlagKeys.Helm, false)));
        services.AddSingleton<INavigationSectionProvider>(sp => new HubbleNavigationSectionProvider(
            sp.GetRequiredService<IFeatureClient>().GetBooleanValue(FeatureFlagKeys.Hubble, false)));
        services.AddTransient<Func<VisualizationViewModel>>(sp => () => sp.GetRequiredService<VisualizationViewModel>());
        services.AddTransient<Func<ResourceYamlViewModel>>(sp => () => sp.GetRequiredService<ResourceYamlViewModel>());
        services.AddSingleton<NavigationSectionCatalog>();
        services.AddKubeUIAIFeature();
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
    [GenerateServiceRegistrations(AssignableTo = typeof(RefreshingCellTextBlock), Lifetime = ServiceLifetime.Transient, AsSelf = true, AsImplementedInterfaces = false, AssemblyNameFilter = "KubeUI.App")]
    private static partial IServiceCollection AddKubeUIShellGeneratedServices(this IServiceCollection services);

    [ScanForTypes(AssignableTo = typeof(IViewFactoryDefinition), Handler = nameof(CreateViewFactory))]
    private static partial IViewFactory[] GetViewFactories();

    private static IViewFactory CreateViewFactory<TView>()
        where TView : Control, IViewFactoryDefinition =>
        new DiscoveredViewFactory(TView.CanCreate, serviceProvider => serviceProvider.GetRequiredService<TView>());

}
