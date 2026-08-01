using Avalonia;
using Avalonia.Controls;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Infrastructure.DependencyInjection;
using KubeUI.Testing.Kubernetes.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace KubeUI.Avalonia.Tests.Infra;

internal static class TestApplicationExtensions
{
    public static IServiceProvider GetTestServices(this Application? application)
    {
        return application is IServiceProviderHost host
            ? host.Services
            : throw new InvalidOperationException("Test services are not initialized.");
    }

    public static async Task<ClusterWorkspace> CreateClusterAsync(
        this Application? application,
        ICollection<IDisposable>? disposables = null,
        Action<TestClusterConfig>? configure = null,
        bool connect = true)
    {
        var services = application.GetTestServices();
        configure?.Invoke(services.GetRequiredService<TestClusterConfig>());

        var cluster = services.GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        disposables?.Add(cluster);
        if (connect)
        {
            await cluster.Connect();
        }

        return cluster;
    }

    public static Task<ClusterWorkspace> CreateClusterAsync(
        this Application? application,
        Action<TestClusterConfig> configure,
        ICollection<IDisposable>? disposables = null,
        bool connect = true)
    {
        ArgumentNullException.ThrowIfNull(configure);
        return application.CreateClusterAsync(disposables, configure, connect);
    }

    public static T GetRequiredTestService<T>(
        this Application? application,
        ICollection<IDisposable>? disposables = null)
        where T : class
    {
        var service = application.GetTestServices().GetRequiredService<T>();
        if (service is IDisposable disposable)
        {
            disposables?.Add(disposable);
        }

        return service;
    }

    public static Window CreateTestWindow(
        this ICollection<IDisposable> disposables,
        double width = 1200,
        double height = 800,
        object? content = null)
    {
        ArgumentNullException.ThrowIfNull(disposables);

        var window = new Window
        {
            Width = width,
            Height = height,
            Content = content,
        };
#pragma warning disable CA2000 // Ownership is transferred to the disposable collection.
        disposables.Add(new WindowDisposer(window));
#pragma warning restore CA2000
        return window;
    }

    private sealed class WindowDisposer(Window window) : IDisposable
    {
        public void Dispose()
        {
            window.Content = null;
            window.Close();
        }
    }
}
