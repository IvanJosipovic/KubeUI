using k8s;
using k8s.Models;
using KubeUI.Features.Resources.Visualization;
using KubeUI.Kubernetes.Resources.Relationships;
using Shouldly;

namespace KubeUI.Features.Resources.Tests.Visualization;

public sealed class VisualizationIncrementalUpdateCoordinatorTests
{
    [Fact]
    public void begin_serializes_updates_until_completion()
    {
        using var coordinator = CreateCoordinator();

        coordinator.TryBegin().ShouldBeTrue();
        coordinator.TryBegin().ShouldBeFalse();
        coordinator.IsRunning.ShouldBeTrue();

        coordinator.Complete();
        coordinator.IsRunning.ShouldBeFalse();
        coordinator.TryBegin().ShouldBeTrue();
    }

    [Fact]
    public void enqueue_keeps_latest_change_per_resource_and_clears_on_dispose()
    {
        var coordinator = CreateCoordinator();
        var pod = Pod("namespace-a", "pod-a");
        var replacement = Pod("namespace-a", "pod-a");
        var key = new ResourceKey("v1", "Pod", "namespace-a", "pod-a");

        coordinator.Enqueue(key, pod, replaceExisting: false);
        coordinator.Enqueue(key, replacement, replaceExisting: true);

        coordinator.PendingCount.ShouldBe(1);
        coordinator.TryTakePending(out var pending).ShouldBeTrue();
        pending.ShouldNotBeNull();
        pending.Resource.ShouldBeSameAs(replacement);
        pending.ReplaceExisting.ShouldBeTrue();
        coordinator.TryTakePending(out _).ShouldBeFalse();

        coordinator.Enqueue(key, pod, replaceExisting: false);
        coordinator.Dispose();

        coordinator.PendingCount.ShouldBe(0);
        coordinator.TryBegin().ShouldBeFalse();
    }

    [Fact]
    public void merge_rejects_unrelated_namespaced_resource_for_selected_namespaces()
    {
        using var coordinator = CreateCoordinator();
        var delta = new ResourceRelationshipGraph([Pod("namespace-b", "pod-b")], []);

        var merged = coordinator.MergeAdditionDelta(
            ResourceRelationshipGraph.Empty,
            delta,
            Pod("namespace-b", "pod-b"),
            replaceExisting: false,
            new HashSet<string>(["namespace-a"], StringComparer.Ordinal),
            rootResource: null,
            selector: null,
            out var requiresRebuild);

        merged.ShouldBeNull();
        requiresRebuild.ShouldBeFalse();
    }

    [Fact]
    public void merge_preserves_pending_references_and_seed_prerequisites()
    {
        using var coordinator = CreateCoordinator();
        var pod = Pod("namespace-a", "pod-a");
        var pending = new UnresolvedResourceReference("apps", "v1", "Deployment", "namespace-a", "deployment-a");
        var prerequisite = new ResourceSeedPrerequisite(new GroupApiVersionKind("apps", "v1", "Deployment", "deployments"));
        var current = new ResourceRelationshipGraph(
            [],
            [],
            new HashSet<UnresolvedResourceReference>([pending]),
            new HashSet<ResourceSeedPrerequisite>([prerequisite]));
        var delta = new ResourceRelationshipGraph([pod], []);

        var merged = coordinator.MergeAdditionDelta(
            current,
            delta,
            pod,
            replaceExisting: false,
            new HashSet<string>(["namespace-a"], StringComparer.Ordinal),
            rootResource: null,
            selector: null,
            out var requiresRebuild);

        requiresRebuild.ShouldBeFalse();
        merged.ShouldNotBeNull();
        merged.Resources.Select(resource => resource.Name()).ShouldBe(["pod-a"]);
        merged.PendingReferences.ShouldContain(pending);
        merged.RequiredSeedPrerequisites.ShouldContain(prerequisite);
    }

    [Fact]
    public void merge_prefers_the_fresher_delta_copy_for_resources_already_in_the_graph()
    {
        using var coordinator = CreateCoordinator();
        var stale = Pod("namespace-a", "pod-a");
        stale.Metadata!.ResourceVersion = "1";
        stale.Metadata.Labels = new Dictionary<string, string> { ["version"] = "1" };
        var fresh = Pod("namespace-a", "pod-a");
        fresh.Metadata!.ResourceVersion = "2";
        fresh.Metadata.Labels = new Dictionary<string, string> { ["version"] = "2" };
        var unrelated = Pod("namespace-a", "unrelated");
        var current = new ResourceRelationshipGraph([stale, unrelated], []);
        var delta = new ResourceRelationshipGraph([fresh], []);

        var merged = coordinator.MergeAdditionDelta(
            current,
            delta,
            fresh,
            replaceExisting: false,
            new HashSet<string>(["namespace-a"], StringComparer.Ordinal),
            rootResource: null,
            selector: null,
            out var requiresRebuild);

        requiresRebuild.ShouldBeFalse();
        merged.ShouldNotBeNull();
        merged.Resources.Count.ShouldBe(2);
        merged.Resources[0].Metadata!.Labels["version"].ShouldBe("2");
        merged.Resources[1].Name().ShouldBe("unrelated");
    }

    [Fact]
    public void merge_requires_rebuild_when_root_scope_has_no_relationship_to_resource()
    {
        using var coordinator = CreateCoordinator();
        var root = Pod("namespace-a", "root");
        var unrelated = Pod("namespace-a", "unrelated");

        var merged = coordinator.MergeAdditionDelta(
            ResourceRelationshipGraph.Empty,
            new ResourceRelationshipGraph([unrelated], []),
            unrelated,
            replaceExisting: false,
            new HashSet<string>(StringComparer.Ordinal),
            root,
            selector: null,
            out var requiresRebuild);

        merged.ShouldBeNull();
        requiresRebuild.ShouldBeTrue();
    }

    [Fact]
    public void merge_replaces_existing_resource_identity()
    {
        using var coordinator = CreateCoordinator();
        var original = Pod("namespace-a", "pod-a");
        var replacement = Pod("namespace-a", "pod-a");
        replacement.Metadata!.Labels = new Dictionary<string, string> { ["version"] = "2" };
        var current = new ResourceRelationshipGraph([original], []);
        var delta = new ResourceRelationshipGraph([replacement], []);

        var merged = coordinator.MergeAdditionDelta(
            current,
            delta,
            replacement,
            replaceExisting: true,
            new HashSet<string>(["namespace-a"], StringComparer.Ordinal),
            rootResource: null,
            selector: null,
            out var requiresRebuild);

        requiresRebuild.ShouldBeFalse();
        merged.ShouldNotBeNull();
        merged.Resources.Count.ShouldBe(1);
        merged.Resources[0].Metadata!.Labels["version"].ShouldBe("2");
    }

    [Fact]
    public async Task build_addition_delta_uses_stored_resource_snapshot()
    {
        var store = new VisualizationResourceStore();
        var pod = Pod("namespace-a", "pod-a");
        var key = new ResourceKey("v1", "Pod", "namespace-a", "pod-a");
        store.Upsert(key, pod);
        using var coordinator = new VisualizationIncrementalUpdateCoordinator(store, new ResourceRelationshipBuilder());

        var delta = await coordinator.BuildAdditionDeltaAsync(
            key,
            new HashSet<string>(["namespace-a"], StringComparer.Ordinal),
            hideNoise: true,
            CancellationToken.None);

        delta.Resources.Select(resource => resource.Name()).ShouldBe(["pod-a"]);
    }

    [Fact]
    public void merge_applies_selector_projection_to_incremental_result()
    {
        using var coordinator = CreateCoordinator();
        var pod = Pod("namespace-a", "pod-a");
        pod.Metadata!.Annotations = new Dictionary<string, string> { ["app"] = "selected" };
        var unrelated = Pod("namespace-a", "unrelated");
        ResourceAnnotationSelector selector = new(new Dictionary<string, string> { ["app"] = "selected" });

        var merged = coordinator.MergeAdditionDelta(
            ResourceRelationshipGraph.Empty,
            new ResourceRelationshipGraph([pod, unrelated], []),
            pod,
            replaceExisting: false,
            new HashSet<string>(StringComparer.Ordinal),
            rootResource: null,
            selector,
            out var requiresRebuild);

        requiresRebuild.ShouldBeFalse();
        merged.ShouldNotBeNull();
        merged.Resources.Select(resource => resource.Name()).ShouldBe(["pod-a"]);
    }

    [Fact]
    public void clearing_pending_changes_discards_queued_resources()
    {
        using var coordinator = CreateCoordinator();
        coordinator.Enqueue(new ResourceKey("v1", "Pod", "namespace-a", "pod-a"), Pod("namespace-a", "pod-a"), false);

        coordinator.ClearPending();

        coordinator.PendingCount.ShouldBe(0);
        coordinator.TryTakePending(out _).ShouldBeFalse();
    }

    [Fact]
    public void merge_projects_incremental_result_to_root_resource_scope()
    {
        using var coordinator = CreateCoordinator();
        var root = Pod("namespace-a", "root");
        var child = Pod("namespace-a", "child");
        var rootIdentity = new ResourceIdentity("v1", "Pod", "namespace-a", "root", "uid-root");
        var childIdentity = new ResourceIdentity("v1", "Pod", "namespace-a", "child", "uid-child");
        var current = new ResourceRelationshipGraph([root], []);
        var delta = new ResourceRelationshipGraph(
            [child],
            [new ResourceRelationship(rootIdentity, childIdentity, ResourceRelationshipKind.Owner)]);

        var merged = coordinator.MergeAdditionDelta(
            current,
            delta,
            child,
            replaceExisting: false,
            new HashSet<string>(StringComparer.Ordinal),
            root,
            selector: null,
            out var requiresRebuild);

        requiresRebuild.ShouldBeFalse();
        merged.ShouldNotBeNull();
        merged.Resources.Select(resource => resource.Name()).ShouldBe(["root", "child"]);
    }

    private static VisualizationIncrementalUpdateCoordinator CreateCoordinator()
        => new(new VisualizationResourceStore(), new ResourceRelationshipBuilder());

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
}
