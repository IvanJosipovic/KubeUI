using k8s;
using k8s.Models;
using Avalonia.Headless.XUnit;
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
    public void selecting_namespace_preserves_non_reference_cluster_scoped_resource()
    {
        var pod = Pod("namespace-a", "pod-a");
        var clusterRole = ClusterResource("rbac.authorization.k8s.io/v1", "ClusterRole", "read-pods", "cluster-role-uid");
        var relationship = new ResourceRelationship(Identity(clusterRole), Identity(pod), ResourceRelationshipKind.Rbac);

        var selected = ResourceGraphProjection.ToSelectedNamespaces(
            new ResourceRelationshipGraph([pod, clusterRole], [relationship]),
            new HashSet<string>(["namespace-a"], StringComparer.Ordinal));

        selected.Resources.Select(resource => resource.Name()).ShouldBe(["pod-a", "read-pods"]);
        selected.Relationships.ShouldContain(relationship);
    }

    [Fact]
    public void incremental_namespace_projection_preserves_non_reference_cluster_scoped_resource()
    {
        var pod = Pod("namespace-a", "pod-a");
        var clusterRole = ClusterResource("rbac.authorization.k8s.io/v1", "ClusterRole", "read-pods", "cluster-role-uid");
        var relationship = new ResourceRelationship(Identity(clusterRole), Identity(pod), ResourceRelationshipKind.Rbac);

        var selected = ResourceGraphProjection.ToSelectedNamespacesIncremental(
            new ResourceRelationshipGraph([pod, clusterRole], [relationship]),
            new HashSet<string>(["namespace-a"], StringComparer.Ordinal),
            new HashSet<ResourceIdentity> { Identity(pod) });

        selected.Resources.Select(resource => resource.Name()).ShouldBe(["pod-a", "read-pods"]);
        selected.Relationships.ShouldContain(relationship);
    }

    [Fact]
    public void selecting_namespace_preserves_crossplane_owner_ancestors_without_descending()
    {
        var deployment = Pod("crossplane-system", "provider-databricks");
        var providerRevision = ClusterResource("pkg.crossplane.io/v1", "ProviderRevision", "provider-databricks", "provider-revision-uid");
        var managedResourceDefinition = ClusterResource(
            "apiextensions.crossplane.io/v1alpha1",
            "ManagedResourceDefinition",
            "apps.apps.databricks.crossplane.io",
            "mrd-uid");
        var customResourceDefinition = ClusterResource(
            "apiextensions.k8s.io/v1",
            "CustomResourceDefinition",
            "apps.apps.databricks.crossplane.io",
            "crd-uid");

        ResourceRelationshipGraph graph = new(
            [deployment, providerRevision, managedResourceDefinition, customResourceDefinition],
            [
                new ResourceRelationship(Identity(providerRevision), Identity(deployment), ResourceRelationshipKind.Owner),
                new ResourceRelationship(Identity(providerRevision), Identity(managedResourceDefinition), ResourceRelationshipKind.Owner),
                new ResourceRelationship(Identity(managedResourceDefinition), Identity(customResourceDefinition), ResourceRelationshipKind.Owner),
            ]);

        var selected = ResourceGraphProjection.ToSelectedNamespaces(
            graph,
            new HashSet<string>(["crossplane-system"], StringComparer.Ordinal));

        selected.Resources.Select(resource => resource.Kind).ShouldBe(["Pod", "ProviderRevision"]);
    }

    [Fact]
    public void selecting_namespace_does_not_descend_from_an_owner_in_another_namespace()
    {
        var selected = Pod("envoy-gateway-system", "selected");
        var kustomization = ClusterResource("kustomize.toolkit.fluxcd.io/v1", "Kustomization", "envoy-gateway", "kustomization-uid");
        var unrelatedChild = Pod("other-namespace", "unrelated-child");

        ResourceRelationshipGraph graph = new(
            [selected, kustomization, unrelatedChild],
            [
                new ResourceRelationship(Identity(kustomization), Identity(selected), ResourceRelationshipKind.Owner),
                new ResourceRelationship(Identity(kustomization), Identity(unrelatedChild), ResourceRelationshipKind.Owner),
            ]);

        var projected = ResourceGraphProjection.ToSelectedNamespaces(
            graph,
            new HashSet<string>(["envoy-gateway-system"], StringComparer.Ordinal));

        projected.Resources.Select(resource => resource.Name()).ShouldBe(["selected", "envoy-gateway"]);
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
        store.Remove(childKey, replacement).ShouldBeTrue();
        store.HasOwnerReferencesTo(owner).ShouldBeFalse();
        store.Upsert(childKey, child);
        store.Upsert(childKey, replacement);

        store.HasOwnerReferencesTo(owner).ShouldBeFalse();
        store.Snapshot().ShouldContain(replacement);
    }

    [Fact]
    public void resource_store_removes_owner_index_and_clear_removes_remaining_resources()
    {
        var owner = Pod("namespace-a", "owner");
        owner.Metadata!.Uid = "owner-uid";
        var child = Pod("namespace-a", "child");
        child.Metadata!.OwnerReferences = [new V1OwnerReference { Uid = "owner-uid", Name = "owner", Kind = "Pod", ApiVersion = "v1" }];
        var ownerKey = new ResourceKey("v1", "Pod", "namespace-a", "owner");
        var childKey = new ResourceKey("v1", "Pod", "namespace-a", "child");
        var store = new VisualizationResourceStore();

        store.Replace(
            new Dictionary<ResourceKey, IKubernetesObject<V1ObjectMeta>> { [ownerKey] = owner },
            new Dictionary<string, HashSet<ResourceKey>>());
        store.Count.ShouldBe(1);

        store.Upsert(ownerKey, owner);
        store.Upsert(childKey, child);
        store.Remove(childKey, child).ShouldBeTrue();
        store.HasOwnerReferencesTo(owner).ShouldBeFalse();
        store.TryGet(childKey, out _).ShouldBeFalse();

        store.Clear();
        store.Count.ShouldBe(0);
        store.Snapshot().ShouldBeEmpty();
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

    [Fact]
    public async Task build_coordinator_passes_pending_request_version_not_live_version()
    {
        List<(int Request, int Version)> started = [];
        var firstStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var secondStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseSecond = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        using VisualizationBuildCoordinator<int> coordinator = new(async (request, version, cancellationToken) =>
        {
            started.Add((request, version));
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
        coordinator.Invalidate();
        await secondStarted.Task;
        releaseSecond.SetResult();
        await TestWait.UntilAsync(() => !coordinator.IsPendingOrRunning, TimeSpan.FromSeconds(5));

        started.ShouldBe([(1, 1), (2, 2)]);
    }

    [AvaloniaFact]
    public async Task seed_planner_resolves_pending_reference_to_a_configured_kind()
    {
        var cluster = await Application.Current.CreateClusterAsync(config => config.Type = KubernetesBackend.Fake);
        await cluster.Connect();
        var pending = new UnresolvedResourceReference(string.Empty, "v1", V1Pod.KubeKind, "default", "pending");

        var required = VisualizationSeedPlanner.FindRequiredSeedKinds(
            new ResourceRelationshipGraph(
                [],
                [],
                SeedPrerequisites: new HashSet<ResourceSeedPrerequisite>
                {
                    new(GroupApiVersionKind.From<V1Node>(), allowServedVersionFallback: true),
                }),
            new HashSet<UnresolvedResourceReference> { pending },
            cluster);

        required.ShouldContain(GroupApiVersionKind.From<V1Node>());
        required.ShouldContain(GroupApiVersionKind.From<V1Pod>());
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

    private static GenericKubernetesObject ClusterResource(string apiVersion, string kind, string name, string uid)
        => new()
        {
            ApiVersion = apiVersion,
            Kind = kind,
            Metadata = new V1ObjectMeta { Name = name, Uid = uid },
        };

    private static ResourceIdentity Identity(IKubernetesObject<V1ObjectMeta> resource)
        => new(
            resource.ApiVersion!,
            resource.Kind!,
            resource.Namespace(),
            resource.Name()!,
            resource.Uid());
}
