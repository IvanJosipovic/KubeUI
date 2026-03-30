using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using Dock.Model.Core;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using KubeUI.Avalonia.Infrastructure.Dialogs;
using KubeUI.Avalonia.Infrastructure.Docking;
using KubeUI.Kubernetes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;

namespace KubeUI.Avalonia.Infrastructure.DependencyInjection;

public static class KubeUIAppServiceCollectionExtensions
{
    public static IServiceCollection AddKubeUIAppServices(this IServiceCollection services, Action<IServiceCollection>? configureOverrides = null)
    {
        services.AddKubeUIAvaloniaServices();
        services.AddKubeUIKubernetesServices();
        services.AddKubeUIDialogServices();

        configureOverrides?.Invoke(services);

        services.RemoveAll<ServiceDescriptor[]>();
        services.AddSingleton<ServiceDescriptor[]>([.. services]);
        return services;
    }

    public static IServiceCollection AddKubeUIDialogServices(this IServiceCollection services)
    {
        services.TryAdd(ServiceDescriptor.Singleton<IDialogFactory, FluentDialogFactory>(_ => (FluentDialogFactory)new DialogFactory().AddFluent()));
        services.TryAdd(ServiceDescriptor.Singleton<IDialogManager, DialogManager>(x => new MyDialogManager(
            dialogFactory: x.GetRequiredService<IDialogFactory>(),
            logger: x.GetRequiredService<ILogger<DialogManager>>())));
        services.TryAdd(ServiceDescriptor.Singleton<IDialogService, DialogService>(x => new DialogService(x.GetRequiredService<IDialogManager>())));

        services.TryAdd(ServiceDescriptor.Singleton<IFactory>(sp => Dispatcher.UIThread.Invoke(() => (IFactory)new DockFactory(sp.GetRequiredService<ILogger<DockFactory>>()))));
        services.TryAdd(ServiceDescriptor.Singleton<INotificationManager>(_ => Dispatcher.UIThread.Invoke(() => (INotificationManager)new WindowNotificationManager(App.TopLevel) { MaxItems = 4 })));
        return services;
    }
}
