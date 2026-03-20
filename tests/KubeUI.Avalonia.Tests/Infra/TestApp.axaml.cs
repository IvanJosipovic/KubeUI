using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Dock.Model.Core;
using HanumanInstitute.MvvmDialogs;
using KubeUI.Avalonia;
using KubeUI.Kubernetes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace KubeUI.Avalonia.Tests.Infra;

public class TestApp : Application
{
    public IServiceProvider Services { get; set; }

    public static TopLevel TopLevel { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        var services = new ServiceCollection();

        services.AddKubeUIAvaloniaServices();
        services.AddKubeUIKubernetesServices();
        services.AddLogging();

        services.AddSingleton<ISettingsService, TestSettingsService>();
        services.AddSingleton<IClusterSettingsStore>(sp => sp.GetRequiredService<ISettingsService>().Clusters);

        var dialog = new Mock<IDialogService>();
        services.AddSingleton<IDialogService>(dialog.Object);

        var notifications = new Mock<INotificationManager>();
        services.AddSingleton<INotificationManager>(notifications.Object);
        services.AddSingleton<IFactory>(sp => Dispatcher.UIThread.Invoke(() => (IFactory)new DockFactory(sp.GetRequiredService<ILogger<DockFactory>>())));
        services.AddSingleton<ServiceDescriptor[]>([.. services]);

        var provider = services.BuildServiceProvider();
        provider.ConfigureKubeUIKubernetesJson();

        Services = provider;
        Resources[typeof(IServiceProvider)] = Services;

        Dispatcher.UIThread.Invoke(() =>
        {
            var factory = provider.GetRequiredService<IFactory>();
            var layout = factory.CreateLayout();
            factory.InitLayout(layout);
        });
    }
}



