using Avalonia;
using Avalonia.Markup.Xaml;
using KubeUI.Client;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Dock.Model.Core;
using KubernetesCRDModelGen;
using Avalonia.Controls;
using CommunityToolkit.Mvvm.ComponentModel;
using KubeUI.ViewModels;
using KubeUI.Views;
using Microsoft.Extensions.Hosting;
using HanumanInstitute.MvvmDialogs;
using Moq;
using Avalonia.Controls.Notifications;

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

        builder.Services.AddLogging();

        // Services
        builder.Services.AddServices();

        var dialog = new Mock<IDialogService>();
        builder.Services.AddSingleton<IDialogService>(dialog.Object);

        var notifications = new Mock<INotificationManager>();
        builder.Services.AddSingleton<INotificationManager>(notifications.Object);


        Host = builder.Build();

        Host.Services.GetRequiredService<ISettingsService>().Settings = new Settings();

        Resources[typeof(IServiceProvider)] = Host.Services;
        _ = Host.RunAsync();

        AvaloniaXamlLoader.Load(this);
    }
}
