using System.Reflection;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using k8s.Models;
using KubeUI.Avalonia.Features.Clusters.Workspace.ViewModels;
using KubeUI.Avalonia.Features.Resources.Properties.ViewModels;
using KubeUI.Avalonia.Features.Resources.Properties.Views;
using KubeUI.Avalonia.Tests.Infra;
using Microsoft.Extensions.DependencyInjection;
using Xunit;

namespace KubeUI.Avalonia.Tests.Features.Resources.Properties;

public sealed class ResourcePropertiesViewThreadingTests : AvaloniaTestBase
{
    [AvaloniaFact]
    public async Task ClearItems_is_safe_when_invoked_from_background_thread()
    {
        var workspace = new TestCluster().CreateWorkspace();
        await workspace.EnsureWorkspaceStateInitializedAsync();

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
        });

        // If we reach here without throwing, the invocation was handled safely.
    }
}

internal sealed class TestableResourcePropertiesView : ResourcePropertiesView
{
    public void InvokeClear() => ClearItems();
}
