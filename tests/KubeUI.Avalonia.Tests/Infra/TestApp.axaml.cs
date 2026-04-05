using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Avalonia.Markup.Xaml;
using Avalonia.Threading;
using Dock.Model.Core;
using FluentAvalonia.UI.Controls;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using KubeUI.Avalonia;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Avalonia.Infrastructure.DependencyInjection;
using KubeUI.Avalonia.Infrastructure.Docking;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Avalonia.Services.Settings;
using KubeUI.Kubernetes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Moq;

namespace KubeUI.Avalonia.Tests.Infra;

public class TestApp : Application, IServiceProviderHost
{
    public static IServiceProvider? CurrentServices { get; private set; }
    public static Mock<IDialogManager>? DialogManagerMock { get; private set; }
    public static INotification? LastNotification { get; private set; }
    public static ContentDialogSettings? LastContentDialogSettings { get; private set; }

    public static TopLevel TopLevel { get; private set; }

    public IServiceProvider Services => CurrentServices ?? throw new InvalidOperationException("Test services are not initialized.");

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        ResetServices();
    }

    public static void ResetForTest()
    {
        RunOnUiThread(() =>
        {
            if (Application.Current is TestApp app)
            {
                app.ResetServices();
            }
        });
    }

    public static void CleanupAfterTest()
    {
        RunOnUiThread(() =>
        {
            if (Application.Current is TestApp app)
            {
                app.CloseOpenWindows();
                app.DisposeServices();
            }
        });
    }

    private void ResetServices()
    {
        CloseOpenWindows();
        DisposeServices();

        var provider = BuildServiceProvider();
        CurrentServices = provider;
        ApplyResources(provider);
        InitializeDockFactory(provider);
    }

    private ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();

        LastContentDialogSettings = null;
        LastNotification = null;

        var dialogManager = new Mock<IDialogManager>();
        dialogManager.SetupGet(x => x.Logger).Returns((ILogger<IDialogManager>?)null);
        dialogManager.SetupProperty(x => x.AllowConcurrentDialogs);
        dialogManager
            .Setup(x => x.ShowFrameworkDialogAsync(It.IsAny<System.ComponentModel.INotifyPropertyChanged?>(), It.IsAny<ContentDialogSettings>(), It.IsAny<Func<object?, string>?>()))
            .Callback<System.ComponentModel.INotifyPropertyChanged?, ContentDialogSettings, Func<object?, string>?>((_, settings, _) => LastContentDialogSettings = settings)
            .ReturnsAsync(ContentDialogResult.Primary);
        DialogManagerMock = dialogManager;

        var dialog = new Mock<IDialogService>();
        dialog.SetupGet(x => x.DialogManager).Returns(dialogManager.Object);

        var notifications = new Mock<INotificationManager>();
        notifications
            .Setup(x => x.Show(It.IsAny<INotification>()))
            .Callback<INotification>(notification => LastNotification = notification);

        services.AddKubeUIAppServices(overrides =>
        {
            overrides.Replace(ServiceDescriptor.Singleton<ISettingsService, TestSettingsService>());
            overrides.RemoveAll<IClusterSettingsStore>();
            overrides.AddSingleton<IClusterSettingsStore>(sp => sp.GetRequiredService<ISettingsService>().Clusters);
            overrides.Replace(ServiceDescriptor.Singleton<IDialogService>(dialog.Object));
            overrides.Replace(ServiceDescriptor.Singleton<INotificationManager>(notifications.Object));
            overrides.Replace(ServiceDescriptor.Singleton<IFactory>(sp => Dispatcher.UIThread.InvokeAsync(() => (IFactory)new DockFactory(sp, sp.GetRequiredService<ILogger<DockFactory>>())).GetAwaiter().GetResult()));
        });

        var provider = services.BuildServiceProvider();
        provider.ConfigureKubeUIKubernetesJsonLogging();
        return provider;
    }

    private void ApplyResources(ServiceProvider provider)
    {
        Resources["AppearanceSettings"] = provider.GetRequiredService<ISettingsService>().Appearance;
        Resources["DataGridRowHeight"] = Convert.ToDouble(provider.GetRequiredService<ISettingsService>().Appearance.ListRowHeight);
        Resources["DataGridColumnHeaderMinHeight"] = Convert.ToDouble(provider.GetRequiredService<ISettingsService>().Appearance.ListRowHeight + 4m);
        Resources["DataGridFontSize"] = Convert.ToDouble(provider.GetRequiredService<ISettingsService>().Appearance.FontSize);

        foreach (var existingViewLocator in DataTemplates.OfType<ViewLocator>().ToList())
        {
            DataTemplates.Remove(existingViewLocator);
        }

        DataTemplates.Add(provider.GetRequiredService<ViewLocator>());
    }

    private static void InitializeDockFactory(ServiceProvider provider)
    {
        var factory = provider.GetRequiredService<IFactory>();
        var layout = factory.CreateLayout();
        factory.InitLayout(layout);
    }

    private void DisposeServices()
    {
        if (CurrentServices is IDisposable disposable)
        {
            disposable.Dispose();
        }
        CurrentServices = null;
    }

    private void CloseOpenWindows()
    {
        if (ApplicationLifetime is not IClassicDesktopStyleApplicationLifetime desktop)
        {
            return;
        }

        foreach (var window in desktop.Windows.ToList())
        {
            window.Close();
        }
    }

    private static void RunOnUiThread(Action action)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            action();
            return;
        }

        Dispatcher.UIThread.InvokeAsync(action).GetAwaiter().GetResult();
    }
}



