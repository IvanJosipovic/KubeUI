using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Notifications;
using Avalonia.Platform.Storage;
using Avalonia.Threading;
using Dock.Model.Core;
using FluentAvalonia.UI.Controls;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Infrastructure.DependencyInjection;
using KubeUI.Avalonia.Infrastructure.Docking;
using KubeUI.Avalonia.Infrastructure.Platform;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Avalonia.Services.Settings;
using KubeUI.Kubernetes;
using KubeUI.Kubernetes;
using KubeUI.Testing.Kubernetes.Infrastructure;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;
using Microsoft.Extensions.Hosting;
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
        Dispatcher.UIThread.ShutdownFinished += OnDispatcherShutdownFinished;
    }

    private void OnDispatcherShutdownFinished(object? sender, EventArgs e)
    {
        Dispatcher.UIThread.ShutdownFinished -= OnDispatcherShutdownFinished;
        Dispose();
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
        services.AddSingleton<IHostApplicationLifetime, KubernetesTestHostApplicationLifetime>();
        var kubeConfigPath = Path.Combine(Path.GetTempPath(), $"kubeui-avalonia-{Guid.NewGuid():N}.config");

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
            overrides.AddKubernetesTestRuntime();
            overrides.Replace(ServiceDescriptor.Singleton<ISettingsService, TestSettingsService>());
            overrides.Replace(ServiceDescriptor.Singleton<ISettingsPersistence, TestSettingsPersistence>());
            overrides.Replace(ServiceDescriptor.Singleton<IPlatformServices, TestPlatformServices>());
            overrides.Replace(ServiceDescriptor.Singleton<TimeProvider>(new TestTimeProvider(DateTimeOffset.UnixEpoch)));
            overrides.RemoveAll<IClusterSettingsStore>();
            overrides.AddSingleton(sp => sp.GetRequiredService<ISettingsService>().Clusters);
            overrides.RemoveAll<ClusterWorkspaceCatalog>();
            overrides.AddSingleton(sp =>
            {
                _ = sp.GetRequiredService<IClusterRuntime>();
                return ActivatorUtilities.CreateInstance<ClusterWorkspaceCatalog>(sp);
            });
            overrides.Replace(ServiceDescriptor.Singleton(dialog.Object));
            overrides.Replace(ServiceDescriptor.Singleton(notifications.Object));
            overrides.Replace(ServiceDescriptor.Singleton<IFactory>(sp =>
            {
                var factory = new DockFactory(sp, sp.GetRequiredService<ILogger<DockFactory>>());
                var layout = factory.CreateLayout();
                factory.InitLayout(layout);
                return factory;
            }));
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

    private sealed class TestSettingsPersistence : ISettingsPersistence
    {
        public string SettingsDirectory => Path.Combine(Path.GetTempPath(), "kubeui-tests");

        public bool EnsureDirectoryExists() => true;

        public SettingsPersistenceData Load() => new();

        public void Save(SettingsPersistenceData data)
        {
        }
    }

    private sealed class TestPlatformServices : IPlatformServices
    {
        public TopLevel GetRequiredTopLevel() => throw new InvalidOperationException("Test platform does not expose a top level.");

        public Task<bool> LaunchUriAsync(Uri uri) => Task.FromResult(false);

        public Task<IReadOnlyList<IStorageFile>> OpenFilePickerAsync(FilePickerOpenOptions options)
            => Task.FromResult<IReadOnlyList<IStorageFile>>([]);

        public Task<IReadOnlyList<IStorageFolder>> OpenFolderPickerAsync(FolderPickerOpenOptions options)
            => Task.FromResult<IReadOnlyList<IStorageFolder>>([]);
    }

    private sealed class TestTimeProvider : TimeProvider
    {
        private readonly DateTimeOffset _utcNow;

        public TestTimeProvider(DateTimeOffset utcNow)
        {
            _utcNow = utcNow;
        }

        public override DateTimeOffset GetUtcNow() => _utcNow;
    }

}

internal static class TestAppClusterRuntimeExtensions
{
    public static Task<T> CreateAsync<T>(this IClusterRuntime runtime, T item, CancellationToken cancellationToken = default)
        where T : class, k8s.IKubernetesObject<k8s.Models.V1ObjectMeta>, new()
    {
        using var client = runtime.Client!.GetGenericClient<T>();
        return string.IsNullOrEmpty(item.Metadata?.NamespaceProperty)
            ? client.CreateAsync(item, cancellationToken)
            : client.CreateNamespacedAsync(item, item.Metadata!.NamespaceProperty, cancellationToken);
    }

    public static async Task<T> ReplaceAsync<T>(this IClusterRuntime runtime, T item, CancellationToken cancellationToken = default)
        where T : class, k8s.IKubernetesObject<k8s.Models.V1ObjectMeta>, new()
    {
        using var client = runtime.Client!.GetGenericClient<T>();
        var current = string.IsNullOrEmpty(item.Metadata?.NamespaceProperty)
            ? await client.ReadAsync<T>(item.Metadata!.Name, cancellationToken).ConfigureAwait(false)
            : await client.ReadNamespacedAsync<T>(item.Metadata.NamespaceProperty, item.Metadata.Name, cancellationToken).ConfigureAwait(false);
        item.Metadata.ResourceVersion = current.Metadata.ResourceVersion;
        return string.IsNullOrEmpty(item.Metadata.NamespaceProperty)
            ? await client.ReplaceAsync(item, item.Metadata.Name, cancellationToken).ConfigureAwait(false)
            : await client.ReplaceNamespacedAsync(item, item.Metadata.NamespaceProperty, item.Metadata.Name, cancellationToken).ConfigureAwait(false);
    }

    public static Task DeleteAsync<T>(this IClusterRuntime runtime, T item, CancellationToken cancellationToken = default)
        where T : class, k8s.IKubernetesObject<k8s.Models.V1ObjectMeta>, new()
    {
        using var client = runtime.Client!.GetGenericClient<T>();
        return string.IsNullOrEmpty(item.Metadata?.NamespaceProperty)
            ? client.DeleteAsync<T>(item.Metadata!.Name, cancellationToken)
            : client.DeleteNamespacedAsync<T>(item.Metadata.NamespaceProperty, item.Metadata.Name, cancellationToken);
    }
}
