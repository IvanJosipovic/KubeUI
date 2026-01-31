using Avalonia;
using Avalonia.Controls.Notifications;
using Avalonia.Markup.Xaml;
using HanumanInstitute.MvvmDialogs;
using KubeUI;
using KubeUI.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;

namespace KubeUI.Tests.Infra;

public class TestApp : Application
{
    public static IHost Host { get; private set; } = null!;

    public override void Initialize()
    {
        var builder = Microsoft.Extensions.Hosting.Host.CreateEmptyApplicationBuilder(new()
        {
            ApplicationName = "KubeUI.Desktop",
            Configuration = new ConfigurationManager(),
            ContentRootPath = Directory.GetCurrentDirectory(),
        });


        // Services
        builder.Services.AddServices();
        builder.Services.AddLogging();

        var dialog = new Mock<IDialogService>();
        builder.Services.AddSingleton<IDialogService>(dialog.Object);

        var notifications = new Mock<INotificationManager>();
        builder.Services.AddSingleton<INotificationManager>(notifications.Object);

        builder.Services.AddSingleton<ServiceDescriptor[]>([.. builder.Services]);

        Host = builder.Build();

        Host.Services.GetRequiredService<ISettingsService>().Settings = new Settings();

        Resources[typeof(IServiceProvider)] = Host.Services;
        _ = Host.RunAsync();

        AvaloniaXamlLoader.Load(this);
    }
}
