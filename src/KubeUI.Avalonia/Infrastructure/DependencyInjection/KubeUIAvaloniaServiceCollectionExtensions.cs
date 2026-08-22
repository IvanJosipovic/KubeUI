using Avalonia.Controls.Notifications;
using Dock.Model.Core;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using KubeUI.Avalonia.Infrastructure.Dialogs;
using KubeUI.Avalonia.Infrastructure.Docking;
using KubeUI.Avalonia.Infrastructure.Platform;
using KubeUI.Avalonia.Infrastructure.Threading;
using KubeUI.Kubernetes;
using KubeUI.Avalonia.Infrastructure.Mcp;
using KubeUI.Avalonia.Shell.Navigation;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace KubeUI.Avalonia.Infrastructure.DependencyInjection;

public static class KubeUIAvaloniaServiceCollectionExtensions
{
    public static IServiceCollection AddKubeUIAppServices(this IServiceCollection services, Action<IServiceCollection>? configureOverrides = null)
    {
        services.AddKubeUIAvaloniaServices();
        services.AddKubeUIKubernetesServices();
        services.Replace(ServiceDescriptor.Singleton<IThreadDispatcher>(AvaloniaScheduler.Instance));
        services.AddKubeUIDialogServices();
        services.AddSingleton<IMcpClusterSession, McpClusterSession>();
        services.AddSingleton<IResourceNavigationService>(sp => new NavigationDocumentService(
            sp,
            sp.GetRequiredService<ILogger<NavigationDocumentService>>(),
            sp.GetRequiredService<ClusterWorkspaceCatalog>(),
            () => sp.GetRequiredService<IFactory>()));

        configureOverrides?.Invoke(services);

        return services;
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
