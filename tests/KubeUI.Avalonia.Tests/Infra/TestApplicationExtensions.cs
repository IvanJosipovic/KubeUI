using Avalonia.Controls;

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
        Action<TestClusterConfig>? configure = null,
        bool connect = true)
    {
        var services = application.GetTestServices();
        configure?.Invoke(services.GetRequiredService<TestClusterConfig>());

        var cluster = services.GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        if (connect)
        {
            await cluster.Connect();
        }

        return cluster;
    }

    public static T GetRequiredTestService<T>(this Application? application)
        where T : class
    {
        var service = application.GetTestServices().GetRequiredService<T>();
        return service;
    }

    public static TestWindow CreateTestWindow(
        this Application? application,
        double width = 1200,
        double height = 800,
        object? content = null)
    {
        ArgumentNullException.ThrowIfNull(application);
        return new TestWindow
        {
            Width = width,
            Height = height,
            Content = content,
        };
    }

    public sealed class TestWindow : Window, IDisposable
    {
        public void Dispose()
        {
            Content = null;
            Close();
        }
    }
}
