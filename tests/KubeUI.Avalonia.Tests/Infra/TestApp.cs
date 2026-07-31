using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Controls.Notifications;
using Avalonia.Threading;
using Dock.Model.Core;
using FluentAvalonia.UI.Controls;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Avalonia.Styles;
using KubeUI.Testing;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Moq;

namespace KubeUI.Avalonia.Tests.Infra;

public class TestApp : Application, IServiceProviderHost
{
    public static IServiceProvider? CurrentServices => (Application.Current as TestApp)?.Services;
    public static Mock<IDialogManager>? DialogManagerMock => (Application.Current as TestApp)?.DialogManager;
    public static INotification? LastNotification => (Application.Current as TestApp)?.Notification;
    public static ContentDialogSettings? LastContentDialogSettings => (Application.Current as TestApp)?.ContentDialogSettings;

    public IServiceProvider? Services { get; private set; }
    public Mock<IDialogManager>? DialogManager { get; private set; }
    public INotification? Notification { get; private set; }
    public ContentDialogSettings? ContentDialogSettings { get; private set; }
    private string? _testKubeConfigPath;

    IServiceProvider IServiceProviderHost.Services => Services ?? throw new InvalidOperationException("Test services are not initialized.");

    public override void Initialize()
    {
        ApplicationThemeStyles.AddTo(Styles);
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

    public static async Task CleanupAfterTestAsync()
    {
        TestApp? app = null;
        await RunOnUiThreadAsync(() =>
        {
            app = Application.Current as TestApp;
            app?.CloseOpenWindows();
            return Task.CompletedTask;
        });

        if (app is not null)
        {
            await app.DisposeServicesAsync().ConfigureAwait(false);
        }
    }

    private void ResetServices()
    {
        CloseOpenWindows();
        DisposeServices();

        var provider = BuildServiceProvider();
        Services = provider;
        ApplyResources(provider);
        InitializeDockFactory(provider);
    }

    private ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        string kubeConfigPath = Path.Combine(Path.GetTempPath(), $"kubeui-avalonia-{Guid.NewGuid():N}.config");
        _testKubeConfigPath = kubeConfigPath;

        ContentDialogSettings = null;
        Notification = null;

        var dialogManager = new Mock<IDialogManager>();
        dialogManager.SetupGet(x => x.Logger).Returns((ILogger<IDialogManager>?)null);
        dialogManager.SetupProperty(x => x.AllowConcurrentDialogs);
        dialogManager
            .Setup(x => x.ShowFrameworkDialogAsync(It.IsAny<System.ComponentModel.INotifyPropertyChanged?>(), It.IsAny<ContentDialogSettings>(), It.IsAny<Func<object?, string>?>()))
            .Callback<System.ComponentModel.INotifyPropertyChanged?, ContentDialogSettings, Func<object?, string>?>((_, settings, _) => ContentDialogSettings = settings)
            .ReturnsAsync(FAContentDialogResult.Primary);
        DialogManager = dialogManager;

        var dialog = new Mock<IDialogService>();
        dialog.SetupGet(x => x.DialogManager).Returns(dialogManager.Object);

        var notifications = new Mock<INotificationManager>();
        notifications
            .Setup(x => x.Show(It.IsAny<INotification>()))
            .Callback<INotification>(notification => Notification = notification);

        services.AddKubeUIAppServices(overrides =>
        {
            overrides.Replace(ServiceDescriptor.Singleton<ISettingsService, TestSettingsService>());
            overrides.RemoveAll<IClusterSettingsStore>();
            overrides.AddSingleton<IClusterSettingsStore>(sp => sp.GetRequiredService<ISettingsService>().Clusters);
            overrides.Replace(ServiceDescriptor.Singleton<IDialogService>(dialog.Object));
            overrides.Replace(ServiceDescriptor.Singleton<INotificationManager>(notifications.Object));
            overrides.Replace(ServiceDescriptor.Singleton<IFactory>(sp => new DockFactory(sp, sp.GetRequiredService<ILogger<DockFactory>>())));
            overrides.Replace(ServiceDescriptor.Singleton<IKubeConfigPathProvider>(
                new KubernetesTestKubeConfigPathProvider(kubeConfigPath)));
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
        if (Services is IAsyncDisposable asyncDisposable)
        {
            Task.Run(() => asyncDisposable.DisposeAsync().AsTask()).GetAwaiter().GetResult();
        }
        else if (Services is IDisposable disposable)
        {
            disposable.Dispose();
        }

        if (_testKubeConfigPath is { } path)
        {
            File.Delete(path);
            _testKubeConfigPath = null;
        }

        Services = null;
    }

    private async Task DisposeServicesAsync()
    {
        if (Services is IAsyncDisposable asyncDisposable)
        {
            await asyncDisposable.DisposeAsync();
        }
        else if (Services is IDisposable disposable)
        {
            disposable.Dispose();
        }

        if (_testKubeConfigPath is { } path)
        {
            File.Delete(path);
            _testKubeConfigPath = null;
        }

        Services = null;
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

    private static async Task RunOnUiThreadAsync(Func<Task> action)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            await action();
            return;
        }

        await Dispatcher.UIThread.InvokeAsync(action);
    }
}
