using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using k8s.Models;
using KubeUI.Avalonia.Tests.Infra;

namespace KubeUI.Avalonia.Tests.Features.Resources.Properties;

public sealed class ResourcePropertiesViewThreadingTests
{
    [AvaloniaFact]
    public async Task ClearItems_is_safe_when_invoked_from_background_thread()
    {
        await using var scope = await KubernetesTestWorkspaceScope.CreateAsync(TestApp.CurrentServices!);
        var workspace = scope.Workspace;
        await workspace.Connect();

        var services = TestApp.CurrentServices ?? throw new InvalidOperationException("Test services are not initialized.");
        var viewModel = services.GetRequiredService<ResourcePropertiesViewModel<V1Pod>>();
        viewModel.Initialize(workspace, new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "pod-1",
                NamespaceProperty = "default",
            }
        });

        var view = new TestableResourcePropertiesView
        {
            DataContext = viewModel,
        };

        var window = new Window
        {
            Content = view
        };

        window.Show();
        Dispatcher.UIThread.RunJobs();
        Dispatcher.UIThread.RunJobs();

        // Close the window to detach the view from the visual tree
        window.Close();
        Dispatcher.UIThread.RunJobs();

        // Invoke ClearItems via a public wrapper from a background thread to simulate the race.
        await Task.Run(() =>
        {
            view.InvokeClear();
        }, TestContext.Current.CancellationToken);

        // If we reach here without throwing, the invocation was handled safely.
    }
}

internal sealed class TestableResourcePropertiesView : ResourcePropertiesView<V1Pod>
{
    public void InvokeClear() => ClearItems();
}
