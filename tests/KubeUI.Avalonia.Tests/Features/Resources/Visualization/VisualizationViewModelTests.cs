using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Features.Resources.Visualization;
using KubeUI.Kubernetes.Resources.Relationships;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.Resources.Visualization;

public sealed class VisualizationViewModelTests
{
    [Fact]
    public void selecting_another_namespace_removes_resources_from_previous_selection()
    {
        var podA = Pod("namespace-a", "pod-a");
        var podB = Pod("namespace-b", "pod-b");
        var graph = Graph(podA, podB);

        var selectedA = ResourceGraphProjection.ToSelectedNamespaces(
            graph,
            new HashSet<string>(["namespace-a"], StringComparer.Ordinal));
        var selectedB = ResourceGraphProjection.ToSelectedNamespaces(
            graph,
            new HashSet<string>(["namespace-b"], StringComparer.Ordinal));

        selectedA.Resources.Select(resource => resource.Name()).ShouldBe(["pod-a"]);
        selectedB.Resources.Select(resource => resource.Name()).ShouldBe(["pod-b"]);
    }

    [Fact]
    public void clearing_namespace_selection_returns_empty_graph()
    {
        var graph = Graph(Pod("namespace-a", "pod-a"));

        var selected = ResourceGraphProjection.ToSelectedNamespaces(
            graph,
            new HashSet<string>(StringComparer.Ordinal));

        selected.Resources.ShouldBeEmpty();
        selected.Relationships.ShouldBeEmpty();
    }

    [Fact]
    public void selecting_namespace_preserves_related_cluster_scoped_resource()
    {
        var pod = Pod("namespace-a", "pod-a");
        V1Node node = new()
        {
            ApiVersion = "v1",
            Kind = "Node",
            Metadata = new V1ObjectMeta { Name = "node-a", Uid = "node-uid" },
        };
        var podIdentity = Identity(pod);
        var nodeIdentity = Identity(node);
        ResourceRelationshipGraph graph = new(
            [pod, node],
            [new ResourceRelationship(nodeIdentity, podIdentity, ResourceRelationshipKind.Reference)]);

        var selected = ResourceGraphProjection.ToSelectedNamespaces(
            graph,
            new HashSet<string>(["namespace-a"], StringComparer.Ordinal));

        selected.Resources.Select(resource => resource.Name()).ShouldBe(["pod-a", "node-a"]);
        selected.Relationships.Count.ShouldBe(1);
    }

    [Fact]
    public void projection_preserves_pending_references_and_seed_prerequisites()
    {
        var pod = Pod("namespace-a", "pod-a");
        var pending = new UnresolvedResourceReference("apps", "v1", "Deployment", "namespace-a", "deployment-a");
        var prerequisite = new ResourceSeedPrerequisite(new GroupApiVersionKind("apps", "v1", "Deployment", "deployments"));
        ResourceRelationshipGraph graph = new(
            [pod],
            [],
            new HashSet<UnresolvedResourceReference>([pending]),
            new HashSet<ResourceSeedPrerequisite>([prerequisite]));

        var projected = ResourceGraphProjection.ToSelectedNamespaces(
            graph,
            new HashSet<string>(["namespace-a"], StringComparer.Ordinal));

        projected.PendingReferences.ShouldBe([pending]);
        projected.RequiredSeedPrerequisites.ShouldBe([prerequisite]);
    }

    [Fact]
    public void root_projection_includes_ancestors_and_descendants_but_excludes_unrelated_resources()
    {
        var root = Pod("namespace-a", "root");
        var parent = Pod("namespace-a", "parent");
        var child = Pod("namespace-a", "child");
        var unrelated = Pod("namespace-a", "unrelated");
        ResourceIdentity rootIdentity = Identity(root);
        ResourceIdentity parentIdentity = Identity(parent);
        ResourceIdentity childIdentity = Identity(child);

        ResourceRelationshipGraph graph = new(
            [root, parent, child, unrelated],
            [
                new ResourceRelationship(parentIdentity, rootIdentity, ResourceRelationshipKind.Owner),
                new ResourceRelationship(rootIdentity, childIdentity, ResourceRelationshipKind.Reference),
            ]);

        var projected = ResourceGraphProjection.ToRootResource(graph, root);

        projected.Resources.Select(resource => resource.Name()).ShouldBe(["root", "parent", "child"]);
        projected.Relationships.Count.ShouldBe(2);
    }

    [Fact]
    public void display_filter_preserves_graph_metadata_while_filtering_resources_and_edges()
    {
        var pod = Pod("namespace-a", "pod-a");
        var service = new V1Service
        {
            ApiVersion = "v1",
            Kind = "Service",
            Metadata = new V1ObjectMeta
            {
                NamespaceProperty = "namespace-a",
                Name = "service-a",
                Uid = "uid-service-a",
            },
        };
        var pending = new UnresolvedResourceReference("apps", "v1", "Deployment", "namespace-a", "deployment-a");
        ResourceRelationshipGraph graph = new(
            [pod, service],
            [new ResourceRelationship(Identity(service), Identity(pod), ResourceRelationshipKind.Reference)],
            new HashSet<UnresolvedResourceReference>([pending]),
            new HashSet<ResourceSeedPrerequisite>());

        var filtered = ResourceGraphDisplayFilter.Apply(
            graph,
            new HashSet<string>(["Pod"], StringComparer.Ordinal),
            showNotReadyOnly: false);

        filtered.Resources.Select(resource => resource.Kind).ShouldBe(["Pod"]);
        filtered.Relationships.ShouldBeEmpty();
        filtered.PendingReferences.ShouldBe([pending]);
    }

    [Fact]
    public void resource_store_replaces_resource_and_owner_index_atomically()
    {
        var owner = Pod("namespace-a", "owner");
        owner.Metadata!.Uid = "owner-uid";
        var child = Pod("namespace-a", "child");
        child.Metadata!.OwnerReferences = [new V1OwnerReference { Uid = "owner-uid", Name = "owner", Kind = "Pod", ApiVersion = "v1" }];
        var replacement = Pod("namespace-a", "child");
        replacement.Metadata!.OwnerReferences = [];
        var store = new VisualizationResourceStore();
        var ownerKey = new ResourceKey("v1", "Pod", "namespace-a", "owner");
        var childKey = new ResourceKey("v1", "Pod", "namespace-a", "child");

        store.Upsert(ownerKey, owner);
        store.Upsert(childKey, child);
        store.HasOwnerReferencesTo(owner).ShouldBeTrue();
        store.Upsert(childKey, replacement);

        store.HasOwnerReferencesTo(owner).ShouldBeFalse();
        store.Snapshot().ShouldContain(replacement);
    }

    [Fact]
    public async Task build_coordinator_cancels_superseded_request_and_publishes_latest_request()
    {
        List<int> started = [];
        var firstStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var secondStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseSecond = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using VisualizationBuildCoordinator<int> coordinator = new(async (request, _, cancellationToken) =>
        {
            started.Add(request);
            if (request == 1)
            {
                firstStarted.SetResult();
                var canceled = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
                using CancellationTokenRegistration registration = cancellationToken.Register(canceled.SetResult);
                await canceled.Task;
                cancellationToken.ThrowIfCancellationRequested();
            }
            else
            {
                secondStarted.SetResult();
                await releaseSecond.Task;
            }
        });

        coordinator.Enqueue(1);
        await firstStarted.Task;
        coordinator.Enqueue(2);
        await secondStarted.Task;
        releaseSecond.SetResult();
        await TestWait.UntilAsync(() => !coordinator.IsPendingOrRunning, TimeSpan.FromSeconds(5));

        started.ShouldBe([1, 2]);
    }

    private static ResourceRelationshipGraph Graph(params V1Pod[] pods)
        => new(pods, []);

    private static V1Pod Pod(string namespaceName, string name)
        => new()
        {
            ApiVersion = "v1",
            Kind = "Pod",
            Metadata = new V1ObjectMeta
            {
                NamespaceProperty = namespaceName,
                Name = name,
                Uid = $"uid-{name}",
            },
        };

    private static ResourceIdentity Identity(IKubernetesObject<V1ObjectMeta> resource)
        => new(
            resource.ApiVersion!,
            resource.Kind!,
            resource.Namespace(),
            resource.Name()!,
            resource.Uid());
}
