using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Markup.Xaml;
using HanumanInstitute.MvvmDialogs;
using KubeUI.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
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

        services.AddSingleton<ServiceDescriptor[]>([.. services]);

        Services = services.BuildServiceProvider();

        Resources[typeof(IServiceProvider)] = Services;
    }
}
