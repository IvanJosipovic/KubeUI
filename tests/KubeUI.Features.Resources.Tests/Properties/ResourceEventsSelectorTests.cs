using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using k8s.Models;
using KubeUI.Features.Resources.Properties.Controls;
using KubeUI.Testing.Utilities;
using Shouldly;

namespace KubeUI.Features.Resources.Tests.Properties;

public sealed class ResourceEventsSelectorTests
{
    [Fact]
    public void Event_timestamp_resolution_prefers_legacy_timestamp_then_event_time_then_creation()
    {
        var lastTimestamp = DateTime.UtcNow.AddMinutes(-3);
        var eventTime = DateTime.UtcNow.AddMinutes(-2);
        var creationTime = DateTime.UtcNow.AddMinutes(-1);
        var @event = new Corev1Event
        {
            Metadata = new() { CreationTimestamp = creationTime },
            EventTime = eventTime,
            LastTimestamp = lastTimestamp
        };

        KubernetesEventTimestampResolver.Resolve(@event).ShouldBe(lastTimestamp);
        @event.LastTimestamp = null;
        KubernetesEventTimestampResolver.Resolve(@event).ShouldBe(eventTime);
        @event.EventTime = null;
        KubernetesEventTimestampResolver.Resolve(@event).ShouldBe(creationTime);
        @event.Metadata = null;
        KubernetesEventTimestampResolver.Resolve(@event).ShouldBeNull();
    }

    [AvaloniaFact]
    public async Task Resource_events_resume_cache_updates_after_detach_and_reattach_without_data_context_change()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        await workspace.Runtime.Permissions.UpdatePermissionsAllNamespaceAsync<Corev1Event>(Verb.List);
        await workspace.Runtime.Permissions.UpdatePermissionsAllNamespaceAsync<Corev1Event>(Verb.Watch);
        await workspace.Runtime.SeedResource<Corev1Event>(true);
        var resource = new V1Deployment
        {
            Metadata = new() { Name = "web", NamespaceProperty = "default", Uid = "resource-uid" }
        };
        var view = new ResourceEventsView { DataContext = resource };
        view.Initialize(workspace);
        using var window = Application.Current.CreateTestWindow(content: view);
        window.Show();

        await workspace.Runtime.AddOrUpdateResource(Event("before-detach", "before", "resource-uid"));
        await TestWait.UntilAsync(
            () => view.Items.Any(item => item.Message == "before"),
            timeoutMs: 5_000,
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());

        window.Content = null;
        await workspace.Runtime.AddOrUpdateResource(Event("while-detached", "after", "resource-uid"));
        await TestWait.UntilAsync(
            () => workspace.Runtime.GetResourceSourceCache<Corev1Event>().Items.Any(item => item.Metadata?.Name == "while-detached"),
            timeoutMs: 5_000,
            cancellationToken: TestContext.Current.CancellationToken);
        window.Content = view;

        await TestWait.UntilAsync(
            () => view.Items.Any(item => item.Message == "after"),
            timeoutMs: 5_000,
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
        view.DataContext.ShouldBeSameAs(resource);
    }

    private static Corev1Event Event(string name, string message, string resourceUid)
        => new()
        {
            Metadata = new() { Name = name, NamespaceProperty = "default", CreationTimestamp = DateTime.UtcNow },
            Message = message,
            LastTimestamp = DateTime.UtcNow,
            InvolvedObject = new() { Uid = resourceUid }
        };

    [AvaloniaFact]
    public async Task SelectRecentEvents_sorts_and_limits_to_five()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        await workspace.Runtime.Permissions.UpdatePermissionsAllNamespaceAsync<Corev1Event>(Verb.List);
        await workspace.Runtime.Permissions.UpdatePermissionsAllNamespaceAsync<Corev1Event>(Verb.Watch);
        await workspace.Runtime.SeedResource<Corev1Event>(true);

        var resource = new V1Deployment
        {
            Metadata = new()
            {
                Name = "my-deployment",
                NamespaceProperty = "default",
                Uid = "resource-uid",
            }
        };

        var now = new DateTime(2026, 3, 29, 0, 40, 0, DateTimeKind.Utc);

        for (var i = 0; i < 6; i++)
        {
            await workspace.Runtime.AddOrUpdateResource(new Corev1Event
            {
                Metadata = new()
                {
                    Name = $"event-{i}",
                    NamespaceProperty = "default",
                    CreationTimestamp = now.AddMinutes(-i - 1),
                },
                Message = $"message-{i}",
                Type = i == 0 ? "Warning" : "Normal",
                Count = i + 1,
                LastTimestamp = now.AddMinutes(-i - 1),
                Source = new()
                {
                    Component = "kubelet",
                    Host = $"node-{i}",
                },
                InvolvedObject = new()
                {
                    Uid = "resource-uid",
                    FieldPath = $"spec.containers{{container-{i}}}",
                }
            });
        }

        await workspace.Runtime.AddOrUpdateResource(new Corev1Event
        {
            Metadata = new()
            {
                Name = "other-event",
                NamespaceProperty = "default",
                CreationTimestamp = now,
            },
            Message = "other",
            InvolvedObject = new()
            {
                Uid = "other-uid",
            }
        });

        await TestWait.UntilAsync(
            () => workspace.Runtime.GetResourceSourceCache<Corev1Event>().Items.Count == 7,
            TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken);

        var results = ResourceEventsSelector.SelectRecentEvents(
            workspace.Runtime.GetResourceSourceCache<Corev1Event>().Items,
            resource,
            now);

        results.Length.ShouldBe(5);
        results[0].Message.ShouldBe("message-0");
        results[0].Source.ShouldBe("kubelet node-0");
        results[0].Count.ShouldBe(1);
        results[0].SubObject.ShouldBe("spec.containers{container-0}");
        results[0].LastSeen.ShouldStartWith("1m0s ago");
        results[0].IsWarning.ShouldBeTrue();

        results[4].Message.ShouldBe("message-4");
        results[4].LastSeen.ShouldStartWith("5m0s ago");
    }
}
