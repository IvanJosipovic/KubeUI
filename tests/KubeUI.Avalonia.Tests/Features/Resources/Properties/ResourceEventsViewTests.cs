using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;
using KubeUI.Avalonia.Tests.Infra;

namespace KubeUI.Avalonia.Tests.Features.Resources.Properties;

public sealed class ResourceEventsViewTests : AvaloniaTestBase
{
    [AvaloniaFact]
    public async Task detached_resource_events_view_does_not_throw_when_data_context_changes()
    {
        var workspace = new TestCluster().CreateWorkspace();
        await workspace.EnsureWorkspaceStateInitializedAsync();
        _ = TestApp.CurrentServices ?? throw new InvalidOperationException("Test services are not initialized.");

        var view = new ResourceEventsView();
        view.Initialize(workspace);

        var window = new Window
        {
            Content = view,
        };

        window.Show();
        view.DataContext = new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "pod-1",
                NamespaceProperty = "default",
            }
        };
        Dispatcher.UIThread.RunJobs();

        window.Content = null;
        window.Close();
        Dispatcher.UIThread.RunJobs();

        view.DataContext = new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "pod-2",
                NamespaceProperty = "default",
            }
        };

        Dispatcher.UIThread.RunJobs();
    }
}
