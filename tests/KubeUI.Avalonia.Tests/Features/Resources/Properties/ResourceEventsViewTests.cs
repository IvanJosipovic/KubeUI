using System.Reflection;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;
using KubeUI.Avalonia.Tests.Infra;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.Resources.Properties;

public sealed class ResourceEventsViewTests
{
    [AvaloniaFact]
    public async Task pre_attach_refresh_does_not_throw_when_dispatcher_flushes()
    {
        var services = Application.Current.GetTestServices();
        ClusterWorkspace workspace = services.GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        await workspace.Connect();

        var view = ActivatorUtilities.CreateInstance<ResourceEventsView>(services);
        view.Initialize(workspace);
        view.DataContext = new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "pod-1",
                NamespaceProperty = "default",
            }
        };

        Dispatcher.UIThread.RunJobs();

        var window = new Window
        {
            Content = view,
        };

        window.Show();
        Dispatcher.UIThread.RunJobs();

        window.Content = null;
        window.Close();
        Dispatcher.UIThread.RunJobs();
    }

    [AvaloniaFact]
    public async Task detached_resource_events_view_does_not_throw_when_data_context_changes()
    {
        var services = Application.Current.GetTestServices();
        ClusterWorkspace workspace = services.GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        await workspace.Connect();

        var view = ActivatorUtilities.CreateInstance<ResourceEventsView>(services);
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

    [AvaloniaFact]
    public async Task refresh_keeps_a_stable_items_source_instance()
    {
        var services = Application.Current.GetTestServices();
        ClusterWorkspace workspace = services.GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        await workspace.Connect();

        var view = ActivatorUtilities.CreateInstance<ResourceEventsView>(services);
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

        var itemsBeforeRefresh = view.Items;

        var refreshMethod = typeof(ResourceEventsView).GetMethod("Refresh", BindingFlags.Instance | BindingFlags.NonPublic);
        refreshMethod.ShouldNotBeNull();
        refreshMethod.Invoke(view, null);

        Dispatcher.UIThread.RunJobs();

        view.Items.ShouldBeSameAs(itemsBeforeRefresh);

        window.Content = null;
        window.Close();
    }

    [AvaloniaFact]
    public async Task queued_update_during_teardown_does_not_throw()
    {
        var services = Application.Current.GetTestServices();
        ClusterWorkspace workspace = services.GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        await workspace.Connect();

        var view = ActivatorUtilities.CreateInstance<ResourceEventsView>(services);
        view.Initialize(workspace);
        view.DataContext = new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "pod-1",
                NamespaceProperty = "default",
            }
        };

        var window = new Window
        {
            Content = view,
        };

        window.Show();
        Dispatcher.UIThread.RunJobs();

        var updateItemsMethod = typeof(ResourceEventsView).GetMethod("UpdateItems", BindingFlags.Instance | BindingFlags.NonPublic);
        updateItemsMethod.ShouldNotBeNull();

        var pendingItems =
            new[]
            {
                new ResourceEventItem(
                    "Failed to pull image",
                    "kubelet",
                    3,
                    "spec.containers{app}",
                    "1 minute ago",
                    true)
            };

        Dispatcher.UIThread.Post(
            () => updateItemsMethod.Invoke(view, new object[] { pendingItems }),
            DispatcherPriority.Background);

        window.Content = null;
        window.Close();

        Dispatcher.UIThread.RunJobs();
    }
}
