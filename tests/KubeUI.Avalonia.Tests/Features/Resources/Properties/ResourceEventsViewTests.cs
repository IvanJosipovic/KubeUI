using System.Reflection;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;
using KubeUI.Avalonia.Tests.Infra;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.Resources.Properties;

public sealed class ResourceEventsViewTests
{
    [AvaloniaFact]
    public void event_card_uses_compact_event_layout_and_theme_foreground()
    {
        var createEventCard = typeof(ResourceEventsView).GetMethod(
            "CreateEventCard",
            BindingFlags.Static | BindingFlags.NonPublic);
        createEventCard.ShouldNotBeNull();

        var card = createEventCard.Invoke(
            null,
            [new ResourceEventItem(
                "Container started",
                "kubelet r720",
                1,
                "spec.containers{rclone}",
                "32m ago",
                false)]).ShouldBeOfType<Border>();

        var window = new Window { Content = card };
        window.Show();
        Dispatcher.UIThread.RunJobs();

        var headline = window.GetVisualDescendants()
            .OfType<TextBlock>()
            .Single(text => text.Text == "Container started");

        headline.Foreground.ShouldNotBeNull();

        window.GetVisualDescendants()
            .OfType<PropertyItem>()
            .Count()
            .ShouldBe(4);

        window.Close();
        Dispatcher.UIThread.RunJobs();
    }

    [AvaloniaFact]
    public async Task pre_attach_refresh_does_not_throw_when_dispatcher_flushes()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        var services = Application.Current.GetTestServices();

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

        await TestApplicationExtensions.WaitForUiAsync();

        var window = new Window
        {
            Content = view,
        };

        window.Show();
        await TestApplicationExtensions.WaitForUiAsync();

        window.Content = null;
        window.Close();
        await TestApplicationExtensions.WaitForUiAsync();
    }

    [AvaloniaFact]
    public async Task detached_resource_events_view_does_not_throw_when_data_context_changes()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        var services = Application.Current.GetTestServices();

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
        await TestApplicationExtensions.WaitForUiAsync();

        window.Content = null;
        window.Close();
        await TestApplicationExtensions.WaitForUiAsync();

        view.DataContext = new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "pod-2",
                NamespaceProperty = "default",
            }
        };

        await TestApplicationExtensions.WaitForUiAsync();
    }

    [AvaloniaFact]
    public async Task refresh_keeps_a_stable_items_source_instance()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        var services = Application.Current.GetTestServices();

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

        await TestApplicationExtensions.WaitForUiAsync();

        var itemsBeforeRefresh = view.Items;

        var refreshMethod = typeof(ResourceEventsView).GetMethod("Refresh", BindingFlags.Instance | BindingFlags.NonPublic);
        refreshMethod.ShouldNotBeNull();
        refreshMethod.Invoke(view, null);

        await TestApplicationExtensions.WaitForUiAsync();

        view.Items.ShouldBeSameAs(itemsBeforeRefresh);

        window.Content = null;
        window.Close();
    }

    [AvaloniaFact]
    public async Task queued_update_during_teardown_does_not_throw()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        var services = Application.Current.GetTestServices();

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
        await TestApplicationExtensions.WaitForUiAsync();

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

        await TestApplicationExtensions.WaitForUiAsync();
    }
}
