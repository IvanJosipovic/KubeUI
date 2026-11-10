using Avalonia.Controls.Notifications;
using Dock.Model.Core;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using KubeUI.App.Infrastructure.Dialogs;
using KubeUI.App.Infrastructure.Docking;
using KubeUI.App.Infrastructure.Mcp;
using KubeUI.App.Infrastructure.Platform;
using KubeUI.App.Infrastructure.Threading;
using KubeUI.App.Options;
using KubeUI.App.Shell.Navigation;
using KubeUI.Features.AI.Mcp;
using KubeUI.Features.Cilium;
using KubeUI.Features.Clusters.Workspace;
using KubeUI.Features.Resources.List;
using KubeUI.Kubernetes;
using KubeUI.Kubernetes.Resources.Relationships;
using KubeUI.Kubernetes.Serialization;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace KubeUI.App.Infrastructure.DependencyInjection;

public static class KubeUIAvaloniaServiceCollectionExtensions
{
    /// <summary>Activity source name emitted by the Kubernetes runtime for application telemetry.</summary>
    public const string KubernetesActivitySourceName = "com.KubeUI.Kubernetes";

    /// <summary>Registers application services with serialization policies captured from Settings.</summary>
    /// <param name="services">Service collection to configure.</param>
    /// <param name="features">Preview feature settings for this application instance.</param>
    /// <param name="configureOverrides">Optional application service overrides.</param>
    /// <returns>The configured service collection.</returns>
    public static IServiceCollection AddKubeUIAppServices(
        this IServiceCollection services,
        FeatureSettings features,
        Action<IServiceCollection>? configureOverrides = null)
    {
        services.AddKubeUIAvaloniaServices();
        services.AddKubeUIKubernetesServices(jsonAllowReflection: !features.DisableJsonSerializationReflection);
        services.AddKubeUICiliumFeature();
        services.Replace(ServiceDescriptor.Singleton<IThreadDispatcher>(AvaloniaScheduler.Instance));
        services.AddKubeUIDialogServices();
        services.AddSingleton<IMcpClusterSession, McpClusterSession>();
        services.AddSingleton<IResourceRelationshipBuilder, ResourceRelationshipBuilder>();
        services.AddSingleton<McpServerState>();
        services.AddSingleton<IMcpServerState>(sp => sp.GetRequiredService<McpServerState>());
        services.AddSingleton<McpRequestIdentityAccessor>();
        services.AddSingleton<IMcpCredentialStore, McpSettingsCredentialStore>();
        services.AddSingleton<IMcpCredentialService, McpCredentialService>();
        services.AddSingleton(TimeProvider.System);
        services.AddSingleton<IResourceNavigationService>(sp => new NavigationDocumentService(
            sp.GetRequiredService<IResourceListDocumentFactory>(),
            sp.GetRequiredService<ILogger<NavigationDocumentService>>(),
            sp.GetRequiredService<ClusterWorkspaceCatalog>(),
            () => sp.GetRequiredService<IFactory>()));

        configureOverrides?.Invoke(services);

        return services;
    }

    /// <summary>
    /// Configures the logger used to report Kubernetes types that still fall back to
    /// reflection-based serialization. This forces the <see cref="IKubernetesJsonSerializer"/>
    /// to be instantiated so any reflection-based serialization warnings get logged.
    /// </summary>
    /// <param name="services">Application service provider.</param>
    public static void ConfigureKubeUIKubernetesJsonLogging(this IServiceProvider services)
    {
        ArgumentNullException.ThrowIfNull(services);
        _ = services.GetService<IKubernetesJsonSerializer>();
    }

    public static IServiceCollection AddKubeUIDialogServices(this IServiceCollection services)
    {
        services.TryAdd(ServiceDescriptor.Singleton<IDialogFactory, FluentDialogFactory>(_ => (FluentDialogFactory)new DialogFactory().AddFluent()));
        services.TryAdd(ServiceDescriptor.Singleton<IDialogManager, DialogManager>(x => new MyDialogManager(
            dialogFactory: x.GetRequiredService<IDialogFactory>(),
            logger: x.GetRequiredService<ILogger<DialogManager>>())));
        services.TryAdd(ServiceDescriptor.Singleton<IDialogService, DialogService>(x => new DialogService(x.GetRequiredService<IDialogManager>())));

        services.TryAdd(ServiceDescriptor.Singleton(sp => Dispatcher.UIThread.Invoke(() => (IFactory)new DockFactory(sp, sp.GetRequiredService<ILogger<DockFactory>>()))));
        services.TryAdd(ServiceDescriptor.Singleton(sp => Dispatcher.UIThread.Invoke(() => (INotificationManager)new WindowNotificationManager(sp.GetRequiredService<IPlatformServices>().GetRequiredTopLevel()) { MaxItems = 4 })));
        return services;
    }
}
