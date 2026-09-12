using Avalonia.Controls;
using Avalonia.Threading;

namespace KubeUI.Avalonia.Tests.Infra;

internal static class TestApplicationExtensions
{
    public static async Task WaitForUiAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();
        Dispatcher.UIThread.RunJobs();
        var completion = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        Dispatcher.UIThread.Post(
            static state => ((TaskCompletionSource)state!).TrySetResult(),
            completion,
            DispatcherPriority.Background);
        await completion.Task.WaitAsync(cancellationToken);
        cancellationToken.ThrowIfCancellationRequested();
    }

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
        var config = services.GetRequiredService<TestClusterConfig>();
        configure?.Invoke(config);
        if (config.Type == KubernetesBackend.Fake)
        {
            config.Name = $"fake-{Guid.NewGuid():N}";
        }

        var cluster = await services.GetRequiredService<TestClusterGenerator>()
            .CreateAsync(config, TestContext.Current.CancellationToken);
        services.GetRequiredService<ClusterManager>().AddCluster(cluster.Cluster);

        var workspace = services.GetRequiredService<ClusterWorkspaceCatalog>().GetCluster(cluster.Cluster.Name)
            ?? throw new InvalidOperationException($"Cluster workspace '{cluster.Cluster.Name}' was not created.");
        if (connect)
        {
            await workspace.Connect();
        }

        return workspace;
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
