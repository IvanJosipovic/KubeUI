using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Dock.Model.Core;
using HanumanInstitute.MvvmDialogs;
using KubeUI.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace KubeUI.Tests.Infra;

public class TestApp : Application
{
    public IServiceProvider Services { get; set; }

    public static TopLevel TopLevel { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);

        var services = new ServiceCollection();

        // Services
        services.AddServices();
        services.AddLogging();

        services.AddSingleton<ISettingsService, TestSettingsService>();

        var dialog = new Mock<IDialogService>();
        services.AddSingleton<IDialogService>(dialog.Object);

        var notifications = new Mock<INotificationManager>();
        services.AddSingleton<INotificationManager>(notifications.Object);
        services.AddSingleton<IFactory>(sp => Dispatcher.UIThread.Invoke<IFactory>(() => new DockFactory(sp.GetRequiredService<ILogger<DockFactory>>())));

        services.AddSingleton<ServiceDescriptor[]>([.. services]);

        Services = services.BuildServiceProvider();

        Resources[typeof(IServiceProvider)] = Services;
    }
}
