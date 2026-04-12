using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using Dock.Model.Core;
using KubeUI.Avalonia.Infrastructure.Dialogs;
using KubeUI.Avalonia.Infrastructure.Docking;
using KubeUI.Kubernetes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace KubeUI.Avalonia.Infrastructure.DependencyInjection;

public static class KubeUIAvaloniaServiceCollectionExtensions
{
    public static IServiceCollection AddKubeUIAppServices(this IServiceCollection services, Action<IServiceCollection>? configureOverrides = null)
    {
        services.AddKubeUIAvaloniaServices();
        services.AddKubeUIKubernetesServices();
        services.AddKubeUIContentDialogServices();

        configureOverrides?.Invoke(services);

        services.RemoveAll<ServiceDescriptor[]>();
        services.AddSingleton<ServiceDescriptor[]>([.. services]);
        return services;
    }

    public static IServiceCollection AddKubeUIContentDialogServices(this IServiceCollection services)
    {
        services.TryAddSingleton<IContentDialogService, ContentDialogService>();

        services.TryAdd(ServiceDescriptor.Singleton<IFactory>(sp => Dispatcher.UIThread.Invoke(() => (IFactory)new DockFactory(sp, sp.GetRequiredService<ILogger<DockFactory>>()))));
        services.TryAdd(ServiceDescriptor.Singleton<INotificationManager>(_ => Dispatcher.UIThread.Invoke(() => (INotificationManager)new WindowNotificationManager(App.TopLevel) { MaxItems = 4 })));
        return services;
    }
}
