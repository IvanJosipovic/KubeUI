using Avalonia.Headless.XUnit;
using DynamicData;
using k8s.Models;
using KubeUI.Avalonia.Infrastructure.Mcp;
using KubeUI.Kubernetes;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Infrastructure.Mcp;

public sealed class McpClusterSessionTests
{
    [AvaloniaFact]
    public async Task list_resources_honors_request_cancellation_during_resource_readiness()
    {
        var workspace = await Application.Current.CreateClusterAsync();
        var session = Application.Current.GetTestServices().GetRequiredService<IMcpClusterSession>();
        using var cancellation = new CancellationTokenSource();
        cancellation.Cancel();

        await Should.ThrowAsync<OperationCanceledException>(() => session.ListResourcesAsync(
            workspace.Runtime.Name, "v1", "Pod", "default", 10, cancellation.Token));
    }

    [AvaloniaFact]
    public async Task list_resources_uses_the_real_kubeui_workspace_cache_and_namespace_filter()
    {
        var pod = new V1Pod
        {
            Metadata = new V1ObjectMeta { Name = "api", NamespaceProperty = "default" }
        };
        var otherNamespacePod = new V1Pod
        {
            Metadata = new V1ObjectMeta { Name = "worker", NamespaceProperty = "other" }
        };
        var workspace = await Application.Current.CreateClusterAsync(config =>
        {
            config.Resources = [pod, otherNamespacePod];
        });
        var session = Application.Current.GetTestServices().GetRequiredService<IMcpClusterSession>();

        var page = await session.ListResourcesAsync(
            workspace.Runtime.Name, "v1", "Pod", "default", 10);

        page.Resources.Select(resource => resource.Name).ShouldBe(["api"]);
        page.NextCursor.ShouldBeNull();

        var emptyPage = await session.ListResourcesAsync(
            workspace.Runtime.Name, "v1", "Pod", "missing", 10);
        emptyPage.Resources.ShouldBeEmpty();
        emptyPage.NextCursor.ShouldBeNull();
    }

    [AvaloniaFact]
    public async Task list_resources_pages_by_stable_key_while_informer_cache_changes()
    {
        var firstPod = Pod("a");
        var lastPod = Pod("c");
        var workspace = await Application.Current.CreateClusterAsync(config =>
        {
            config.Resources = [firstPod, lastPod];
        });
        var session = Application.Current.GetTestServices().GetRequiredService<IMcpClusterSession>();

        var firstPage = await session.ListResourcesAsync(
            workspace.Runtime.Name, "v1", "Pod", "default", 1);

        firstPage.Resources.Select(resource => resource.Name).ShouldBe(["a"]);
        firstPage.NextCursor.ShouldNotBeNull();
        await Should.ThrowAsync<ArgumentException>(() => session.ListResourcesAsync(
            workspace.Runtime.Name, "v1", "Pod", "other", 1,
            cursor: firstPage.NextCursor));
        await Should.ThrowAsync<ArgumentException>(() => session.ListResourcesAsync(
            workspace.Runtime.Name, "apps/v1", "Pod", "default", 1,
            cursor: firstPage.NextCursor));
        await Should.ThrowAsync<ArgumentException>(() => session.ListResourcesAsync(
            workspace.Runtime.Name, "v1", "Deployment", "default", 1,
            cursor: firstPage.NextCursor));

        var podContainer = workspace.Runtime.Objects.Values.OfType<ContainerClass<V1Pod>>().Single();
        podContainer.Items.AddOrUpdate(Pod("b"));

        var secondPage = await session.ListResourcesAsync(
            workspace.Runtime.Name, "v1", "Pod", "default", 1,
            cursor: firstPage.NextCursor);
        secondPage.Resources.Select(resource => resource.Name).ShouldBe(["b"]);
        secondPage.NextCursor.ShouldNotBeNull();

        var thirdPage = await session.ListResourcesAsync(
            workspace.Runtime.Name, "v1", "Pod", "default", 1,
            cursor: secondPage.NextCursor);
        thirdPage.Resources.Select(resource => resource.Name).ShouldBe(["c"]);
        thirdPage.NextCursor.ShouldBeNull();
    }

    [AvaloniaFact]
    public async Task list_events_returns_review_details_and_keyset_pages_in_stable_order()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        await workspace.Runtime.Permissions.UpdatePermissionsAllNamespaceAsync<Corev1Event>(Verb.List);
        await workspace.Runtime.Permissions.UpdatePermissionsAllNamespaceAsync<Corev1Event>(Verb.Watch);
        await workspace.Runtime.SeedResource<Corev1Event>(
            waitForReady: true, cancellationToken: TestContext.Current.CancellationToken);
        var now = new DateTime(2026, 10, 13, 12, 0, 0, DateTimeKind.Utc);
        workspace.Runtime.GetResourceSourceCache<Corev1Event>().AddOrUpdate(
        [
            Event("aaa-old", now.AddMinutes(-2)),
            Event("zzz-new", now),
            Event("mmm-middle", now.AddMinutes(-1)),
        ]);
        var session = Application.Current.GetTestServices().GetRequiredService<IMcpClusterSession>();

        var firstPage = await session.ListEventsAsync(
            workspace.Runtime.Name, "default", 2, TestContext.Current.CancellationToken);

        firstPage.Events.Select(item => item.Metadata.Name).ShouldBe(["aaa-old", "mmm-middle"]);
        firstPage.Events[0].Type.ShouldBe("Warning");
        firstPage.Events[0].Reason.ShouldBe("FailedScheduling");
        firstPage.Events[0].Message.ShouldBe("0/3 nodes are available");
        firstPage.Events[0].InvolvedObject.Kind.ShouldBe("Pod");
        firstPage.Events[0].InvolvedObject.Name.ShouldBe("workload");
        firstPage.Events[0].Source.Component.ShouldBe("default-scheduler");
        firstPage.Events[0].Count.ShouldBe(2);
        firstPage.Events[0].LastTimestamp.ShouldBe(now.AddMinutes(-2));
        firstPage.Events[0].Metadata.Annotations["example.com/context"].ShouldBe("preserve-full-event");
        firstPage.NextCursor.ShouldNotBeNull();

        var secondPage = await session.ListEventsAsync(
            workspace.Runtime.Name, "default", 2, TestContext.Current.CancellationToken,
            firstPage.NextCursor);

        secondPage.Events.Select(item => item.Metadata.Name).ShouldBe(["zzz-new"]);
        secondPage.NextCursor.ShouldBeNull();
        await Should.ThrowAsync<ArgumentException>(() => session.ListEventsAsync(
            workspace.Runtime.Name, "other", 2, TestContext.Current.CancellationToken,
            firstPage.NextCursor));
    }

    [AvaloniaFact]
    public async Task list_events_returns_one_hundred_review_ready_items_from_a_five_hundred_event_cache()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        await workspace.Runtime.Permissions.UpdatePermissionsAllNamespaceAsync<Corev1Event>(Verb.List);
        await workspace.Runtime.Permissions.UpdatePermissionsAllNamespaceAsync<Corev1Event>(Verb.Watch);
        await workspace.Runtime.SeedResource<Corev1Event>(
            waitForReady: true, cancellationToken: TestContext.Current.CancellationToken);

        var now = new DateTime(2026, 10, 13, 12, 0, 0, DateTimeKind.Utc);
        var events = new Corev1Event[500];
        for (var index = 0; index < events.Length; index++)
        {
            events[index] = Event($"event-{index:D3}", now.AddMinutes(index));
        }

        workspace.Runtime.GetResourceSourceCache<Corev1Event>().AddOrUpdate(events);
        var session = Application.Current.GetTestServices().GetRequiredService<IMcpClusterSession>();

        var page = await session.ListEventsAsync(
            workspace.Runtime.Name, "default", 100, TestContext.Current.CancellationToken);

        page.Events.Count.ShouldBe(100);
        page.Events[0].Metadata.Name.ShouldBe("event-000");
        page.Events[^1].Metadata.Name.ShouldBe("event-099");
        page.Events.ShouldAllBe(item =>
            item.Type == "Warning"
            && item.Reason == "FailedScheduling"
            && item.Message == "0/3 nodes are available"
            && item.InvolvedObject.Kind == "Pod"
            && item.InvolvedObject.Name == "workload"
            && item.Source.Component == "default-scheduler"
            && item.Count == 2
            && item.LastTimestamp.HasValue);
        page.NextCursor.ShouldNotBeNull();
    }

    [AvaloniaFact]
    public async Task list_events_rejects_invalid_limits_and_resource_cursors()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        var session = Application.Current.GetTestServices().GetRequiredService<IMcpClusterSession>();
        var resourceCursor = McpResourceCursorCodec.Encode(
            workspace.Runtime.Name, "v1", "Pod", "default", "default", "pod", "uid-pod");

        await Should.ThrowAsync<ArgumentOutOfRangeException>(() => session.ListEventsAsync(
            workspace.Runtime.Name, "default", 0, TestContext.Current.CancellationToken));
        await Should.ThrowAsync<ArgumentOutOfRangeException>(() => session.ListEventsAsync(
            workspace.Runtime.Name, "default", 501, TestContext.Current.CancellationToken));
        await Should.ThrowAsync<ArgumentException>(() => session.ListEventsAsync(
            workspace.Runtime.Name, "default", 10, TestContext.Current.CancellationToken,
            resourceCursor));
    }

    [AvaloniaFact]
    public async Task list_resources_rejects_out_of_range_limits_and_unknown_kinds()
    {
        var workspace = await Application.Current.CreateClusterAsync();
        var session = Application.Current.GetTestServices().GetRequiredService<IMcpClusterSession>();

        await Should.ThrowAsync<ArgumentOutOfRangeException>(() => session.ListResourcesAsync(
            workspace.Runtime.Name, "v1", "Pod", null, 0));
        await Should.ThrowAsync<ArgumentOutOfRangeException>(() => session.ListResourcesAsync(
            workspace.Runtime.Name, "v1", "Pod", null, 501));
        await Should.ThrowAsync<InvalidOperationException>(() => session.ListResourcesAsync(
            workspace.Runtime.Name, "v1", "UnknownKind", null, 10));
    }

    private static V1Pod Pod(string name) => new()
    {
        Metadata = new V1ObjectMeta
        {
            Name = name,
            NamespaceProperty = "default",
            Uid = $"uid-{name}",
        },
    };

    private static Corev1Event Event(string name, DateTime timestamp) => new()
    {
        Metadata = new V1ObjectMeta
        {
            Name = name,
            NamespaceProperty = "default",
            Uid = $"uid-{name}",
            Annotations = new Dictionary<string, string>
            {
                ["example.com/context"] = "preserve-full-event",
            },
            CreationTimestamp = timestamp,
        },
        Type = "Warning",
        Reason = "FailedScheduling",
        Message = "0/3 nodes are available",
        InvolvedObject = new V1ObjectReference
        {
            ApiVersion = "v1",
            Kind = "Pod",
            Name = "workload",
            NamespaceProperty = "default",
        },
        Source = new V1EventSource { Component = "default-scheduler" },
        Count = 2,
        LastTimestamp = timestamp,
    };
}
