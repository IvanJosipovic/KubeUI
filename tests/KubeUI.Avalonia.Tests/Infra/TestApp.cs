using Avalonia;
using Avalonia.Controls.Notifications;
using Dock.Model.Core;
using FluentAvalonia.UI.Controls;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Avalonia.Infrastructure.DependencyInjection;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Avalonia.Infrastructure.Docking;
using KubeUI.Avalonia.Services.Settings;
using KubeUI.Kubernetes;
using KubeUI.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Logging;
using Moq;

namespace KubeUI.Avalonia.Tests.Infra;

public class TestApp : Application, IServiceProviderHost, IDisposable
{
    public IServiceProvider? Services { get; private set; }
    public Mock<IDialogManager>? DialogManager { get; private set; }
    public INotification? Notification { get; private set; }
    public ContentDialogSettings? ContentDialogSettings { get; private set; }
    private int _disposed;

    IServiceProvider IServiceProviderHost.Services => Services ?? throw new InvalidOperationException("Test services are not initialized.");

    internal void InitializeServices()
    {
        var provider = BuildServiceProvider();
        Services = provider;
        ApplyResources(provider);
        InitializeDockFactory(provider);

    }

    public void Dispose()
    {
        if (Interlocked.Exchange(ref _disposed, 1) == 1)
        {
            return;
        }

        if (Services is IAsyncDisposable asyncDisposableServices)
        {
            asyncDisposableServices.DisposeAsync().AsTask().GetAwaiter().GetResult();
        }
        else if (Services is IDisposable disposableServices)
        {
            disposableServices.Dispose();
        }

        Services = null;
        DialogManager = null;
        Notification = null;
        ContentDialogSettings = null;
    }

    private ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();
        services.AddLogging();
        string kubeConfigPath = Path.Combine(Path.GetTempPath(), $"kubeui-avalonia-{Guid.NewGuid():N}.config");

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
            overrides.RemoveAll<ClusterWorkspaceCatalog>();
            overrides.AddTransient<ClusterWorkspaceCatalog>();
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

}
