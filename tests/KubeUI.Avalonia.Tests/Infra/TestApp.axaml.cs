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
using KubeUI.Kubernetes;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Moq;

namespace KubeUI.Avalonia.Tests.Infra;

public class TestApp : Application
{
    public IServiceProvider? Services { get; private set; }
    public static Mock<IDialogManager>? DialogManagerMock { get; private set; }
    public static ContentDialogSettings? LastContentDialogSettings { get; private set; }

    public static TopLevel TopLevel { get; private set; }

    public override void Initialize()
    {
        AvaloniaXamlLoader.Load(this);
        ResetServices();
    }

    public static void ResetForTest()
    {
        Dispatcher.UIThread.Invoke(() =>
        {
            if (Application.Current is TestApp app)
            {
                app.ResetServices();
            }
        });
    }

    public static void CleanupAfterTest()
    {
        Dispatcher.UIThread.Invoke(() =>
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
        Services = provider;
        ApplyResources(provider);
        InitializeDockFactory(provider);
    }

    private ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();

        services.AddKubeUIAvaloniaServices();
        services.AddKubeUIKubernetesServices();
        services.AddLogging();

        services.AddSingleton<ISettingsService, TestSettingsService>();
        services.AddSingleton<IClusterSettingsStore>(sp => sp.GetRequiredService<ISettingsService>().Clusters);

        LastContentDialogSettings = null;

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
        services.AddSingleton<IDialogService>(dialog.Object);

        var notifications = new Mock<INotificationManager>();
        services.AddSingleton<INotificationManager>(notifications.Object);
        services.AddSingleton<IFactory>(sp => Dispatcher.UIThread.Invoke(() => (IFactory)new DockFactory(sp.GetRequiredService<ILogger<DockFactory>>())));
        services.AddSingleton<ServiceDescriptor[]>([.. services]);

        var provider = services.BuildServiceProvider();
        provider.ConfigureKubeUIKubernetesJson();
        return provider;
    }

    private void ApplyResources(ServiceProvider provider)
    {
        Resources[typeof(IServiceProvider)] = provider;
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
        if (Services is IDisposable disposable)
        {
            disposable.Dispose();
        }

        Resources.Remove(typeof(IServiceProvider));
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
}



