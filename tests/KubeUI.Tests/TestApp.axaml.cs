using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Markup.Xaml;
using CommunityToolkit.Mvvm.ComponentModel;
using Dock.Model.Core;
using HanumanInstitute.MvvmDialogs;
using KubernetesCRDModelGen;
using KubeUI.Client;
using KubeUI.ViewModels;
using KubeUI.Views;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Moq;

namespace KubeUI.Tests;

public class TestApp : Application
{
    public static IHost Host { get; private set; }

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
