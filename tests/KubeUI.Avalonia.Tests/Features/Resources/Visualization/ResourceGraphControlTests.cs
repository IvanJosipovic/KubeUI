using Avalonia.Headless.XUnit;
using Avalonia.Controls;
using Avalonia.Threading;
using Avalonia;
using System.Collections.Concurrent;
using System.Diagnostics;
using k8s;
using KubeUI.Avalonia.Features.Resources.Visualization;
using KubeUI.Avalonia.Tests.Infra;
using KubeUI.Kubernetes.Resources.Relationships;
using Shouldly;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Testing;
using Westermo.GraphX.Controls.Controls;

namespace KubeUI.Avalonia.Tests.Features.Resources.Visualization;

public sealed class ResourceGraphControlTests : AvaloniaTestBase
{
    private static async Task WaitForAsync(Func<bool> predicate, int timeoutMs = 5000, CancellationToken cancellationToken = default)
    {
        cancellationToken = cancellationToken == default ? TestContext.Current.CancellationToken : cancellationToken;
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);
        while (DateTime.UtcNow < deadline)
        {
            cancellationToken.ThrowIfCancellationRequested();
            Dispatcher.UIThread.RunJobs();
            if (predicate())
            {
                return;
            }

            await WaitForNextPollAsync(cancellationToken);
        }

        Dispatcher.UIThread.RunJobs();
        predicate().ShouldBeTrue();
    }

    private static async Task WaitForNextPollAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken = cancellationToken == default ? TestContext.Current.CancellationToken : cancellationToken;
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(10));
        await timer.WaitForNextTickAsync(cancellationToken);
    }

    private static async Task WaitForSignalAsync(Task signal, string description)
    {
        DateTime deadline = DateTime.UtcNow.AddSeconds(5);
        while (!signal.IsCompleted)
        {
            Dispatcher.UIThread.RunJobs();
            if (DateTime.UtcNow >= deadline)
            {
                throw new TimeoutException($"Timed out waiting for {description}.");
            }

            await WaitForNextPollAsync();
        }
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task visualizing_namespace_links_namespace_selector_and_can_unlink(KubernetesBackend backend)
    {
        await using var clusterScope = await KubernetesTestWorkspaceScope.CreateAsync(TestApp.CurrentServices!, backend);
        var cluster = clusterScope.Workspace;
        V1Namespace namespaceResource = new() { Metadata = new() { Name = "team-a" } };
        await cluster.Runtime.AddOrUpdateResource(namespaceResource);

        using VisualizationViewModel viewModel = new(new ResourceRelationshipBuilder());
        viewModel.Initialize(cluster, namespaceResource);

        viewModel.IsNamespaceSelectionLinked.ShouldBeFalse();
        viewModel.IsNamespaceSelectorVisible.ShouldBeTrue();
        viewModel.RootResource.ShouldBeNull();
        ReferenceEquals(viewModel.SelectedNamespaces, cluster.SelectedNamespaces).ShouldBeFalse();
        viewModel.SelectedNamespaces.Select(x => x.Name()).ShouldContain("team-a");
        cluster.SelectedNamespaces.Select(x => x.Name()).ShouldNotContain("team-a");

        viewModel.IsNamespaceSelectionLinked = false;
        Dispatcher.UIThread.RunJobs();

        ReferenceEquals(viewModel.SelectedNamespaces, cluster.SelectedNamespaces).ShouldBeFalse();
        viewModel.SelectedNamespaces.Select(x => x.Name()).ShouldContain("team-a");

        cluster.SelectedNamespaces.Add(new V1Namespace { Metadata = new() { Name = "team-b" } });
        Dispatcher.UIThread.RunJobs();

        viewModel.SelectedNamespaces.Select(x => x.Name()).ShouldContain("team-a");
        viewModel.SelectedNamespaces.Select(x => x.Name()).ShouldNotContain("team-b");
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task visualizing_selected_namespace_includes_pvc_already_loaded_before_view_initialization(KubernetesBackend backend)
    {
        await using var clusterScope = await KubernetesTestWorkspaceScope.CreateAsync(TestApp.CurrentServices!, backend);
        var cluster = clusterScope.Workspace;
        V1Namespace namespaceResource = new() { Metadata = new() { Name = "platform-dev-ijosipov" } };
        await clusterScope.ScenarioHarness.CreateDirectAsync(namespaceResource, TestContext.Current.CancellationToken);
        await cluster.Runtime.SeedResource<V1PersistentVolumeClaim>(true);

        V1PersistentVolumeClaim claim = new()
        {
            ApiVersion = V1PersistentVolumeClaim.KubeApiVersion,
            Kind = V1PersistentVolumeClaim.KubeKind,
            Metadata = new()
            {
                Name = "unity-unitycatalog-server-db",
                NamespaceProperty = "platform-dev-ijosipov",
            },
            Spec = new()
            {
                AccessModes = ["ReadWriteOnce"],
                Resources = new() { Requests = new Dictionary<string, ResourceQuantity> { ["storage"] = new("1Gi") } },
                StorageClassName = "default",
            },
        };
        if (backend == KubernetesBackend.Fake)
        {
            cluster.Runtime.GetResourceSourceCache<V1PersistentVolumeClaim>().Edit(cache => cache.AddOrUpdate(claim));
        }
        else
        {
            await clusterScope.ScenarioHarness.CreateDirectAsync(claim, TestContext.Current.CancellationToken);
            await TestWait.UntilAsync(
                () => cluster.Runtime.GetResourceList<V1PersistentVolumeClaim>().Any(resource => resource.Name() == claim.Name()),
                TimeSpan.FromSeconds(10),
                cancellationToken: TestContext.Current.CancellationToken);
        }

        using VisualizationViewModel viewModel = new(new ResourceRelationshipBuilder());
        viewModel.Initialize(cluster, namespaceResource);

        await WaitForAsync(() => viewModel.Graph!.Resources.Any(resource => resource.Name() == claim.Name()));

        viewModel.Graph!.Resources.ShouldContain(resource => resource.Name() == claim.Name());
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task reopening_visualization_keeps_secret_that_arrived_incrementally(KubernetesBackend backend)
    {
        V1Secret secret = new()
        {
            ApiVersion = V1Secret.KubeApiVersion,
            Kind = V1Secret.KubeKind,
            Metadata = new()
            {
                Name = "existing-secret",
                NamespaceProperty = "platform-dev-ijosipov",
            },
        };

        await using var clusterScope = await KubernetesTestWorkspaceScope.CreateAsync(TestApp.CurrentServices!, backend);
        var cluster = clusterScope.Workspace;
        V1Namespace namespaceResource = new() { Metadata = new() { Name = "platform-dev-ijosipov" } };
        await clusterScope.ScenarioHarness.CreateDirectAsync(namespaceResource, TestContext.Current.CancellationToken);

        using (VisualizationViewModel firstView = new(new ResourceRelationshipBuilder()))
        {
            firstView.Initialize(cluster, namespaceResource);
            await clusterScope.ScenarioHarness.CreateDirectAsync(secret, TestContext.Current.CancellationToken);
            await cluster.Runtime.SeedResource<V1Secret>(true);
            await WaitForAsync(() => firstView.Graph!.Resources.Any(resource => resource.Name() == secret.Name()));
        }

        using VisualizationViewModel reopenedView = new(new ResourceRelationshipBuilder());
        reopenedView.Initialize(cluster, namespaceResource);

        await WaitForAsync(() => reopenedView.Graph!.Resources.Any(resource => resource.Name() == secret.Name()));
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task reopening_visualization_from_selected_namespace_as_root_keeps_secret_loaded_during_config_initialization(KubernetesBackend backend)
    {
        V1Secret secret = new()
        {
            ApiVersion = V1Secret.KubeApiVersion,
            Kind = V1Secret.KubeKind,
            Metadata = new()
            {
                Name = "config-loaded-secret",
                NamespaceProperty = "platform-dev-ijosipov",
            },
        };

        await using var clusterScope = await KubernetesTestWorkspaceScope.CreateAsync(TestApp.CurrentServices!, backend);
        var cluster = clusterScope.Workspace;
        V1Namespace namespaceResource = new() { Metadata = new() { Name = "platform-dev-ijosipov" } };
        await clusterScope.ScenarioHarness.CreateDirectAsync(namespaceResource, TestContext.Current.CancellationToken);

        using (VisualizationViewModel firstView = new(new ResourceRelationshipBuilder()))
        {
            firstView.Initialize(cluster);
            firstView.SelectedNamespaces.Add(namespaceResource);
            await clusterScope.ScenarioHarness.CreateDirectAsync(secret, TestContext.Current.CancellationToken);
            await cluster.Runtime.SeedResource<V1Secret>(true);
            await WaitForAsync(() => firstView.Graph!.Resources.Any(resource => resource.Name() == secret.Name()));
        }

        using VisualizationViewModel reopenedView = new(new ResourceRelationshipBuilder());
        reopenedView.Initialize(cluster, namespaceResource);

        await WaitForAsync(() => reopenedView.Graph!.Resources.Any(resource => resource.Name() == secret.Name()));
    }

    [Fact]
    public void root_filter_does_not_expand_downward_from_parents()
    {
        V1Pod grandparent = new() { ApiVersion = "v1", Kind = V1Pod.KubeKind, Metadata = new() { Name = "grandparent", NamespaceProperty = "demo", Uid = "grandparent-uid" } };
        V1Pod parent = new() { ApiVersion = "v1", Kind = V1Pod.KubeKind, Metadata = new() { Name = "parent", NamespaceProperty = "demo", Uid = "parent-uid" } };
        V1Pod root = new() { ApiVersion = "v1", Kind = V1Pod.KubeKind, Metadata = new() { Name = "root", NamespaceProperty = "demo", Uid = "root-uid" } };
        V1Pod child = new() { ApiVersion = "v1", Kind = V1Pod.KubeKind, Metadata = new() { Name = "child", NamespaceProperty = "demo", Uid = "child-uid" } };
        V1Pod sibling = new() { ApiVersion = "v1", Kind = V1Pod.KubeKind, Metadata = new() { Name = "sibling", NamespaceProperty = "demo", Uid = "sibling-uid" } };

        ResourceRelationshipGraph graph = new(
            [grandparent, parent, root, child, sibling],
            [
                new(GetIdentity(grandparent), GetIdentity(parent), ResourceRelationshipKind.Owner),
                new(GetIdentity(parent), GetIdentity(root), ResourceRelationshipKind.Owner),
                new(GetIdentity(root), GetIdentity(parent), ResourceRelationshipKind.Reference),
                new(GetIdentity(parent), GetIdentity(sibling), ResourceRelationshipKind.Owner),
                new(GetIdentity(root), GetIdentity(child), ResourceRelationshipKind.Owner),
            ]);

        ResourceRelationshipGraph filtered = VisualizationViewModel.FilterToRootResource(graph, root);

        filtered.Resources.Select(resource => resource.Name()).ShouldBe(["grandparent", "parent", "root", "child"]);
    }

    [Fact]
    public void root_filter_does_not_include_parents_of_descendants()
    {
        V1Pod root = new() { ApiVersion = "v1", Kind = V1Pod.KubeKind, Metadata = new() { Name = "root", NamespaceProperty = "demo", Uid = "root-uid" } };
        V1Pod child = new() { ApiVersion = "v1", Kind = V1Pod.KubeKind, Metadata = new() { Name = "child", NamespaceProperty = "demo", Uid = "child-uid" } };
        V1Pod childParent = new() { ApiVersion = "v1", Kind = V1Pod.KubeKind, Metadata = new() { Name = "child-parent", NamespaceProperty = "other", Uid = "child-parent-uid" } };

        ResourceRelationshipGraph graph = new(
            [root, child, childParent],
            [
                new(GetIdentity(root), GetIdentity(child), ResourceRelationshipKind.Owner),
                new(GetIdentity(childParent), GetIdentity(child), ResourceRelationshipKind.Owner),
            ]);

        ResourceRelationshipGraph filtered = VisualizationViewModel.FilterToRootResource(graph, root);

        filtered.Resources.Select(resource => resource.Name()).ShouldBe(["root", "child"]);
    }

    [Fact]
    public void endpoint_slice_root_does_not_include_sibling_endpoint_slices()
    {
        V1Service service = new() { ApiVersion = "v1", Kind = V1Service.KubeKind, Metadata = new() { Name = "database", NamespaceProperty = "authentik", Uid = "service-uid" } };
        V1Pod pod = new() { ApiVersion = "v1", Kind = V1Pod.KubeKind, Metadata = new() { Name = "database-0", NamespaceProperty = "authentik", Uid = "pod-uid" } };
        V1Pod siblingPod = new() { ApiVersion = "v1", Kind = V1Pod.KubeKind, Metadata = new() { Name = "database-1", NamespaceProperty = "authentik", Uid = "sibling-pod-uid" } };
        V1EndpointSlice root = new()
        {
            ApiVersion = "discovery.k8s.io/v1",
            Kind = V1EndpointSlice.KubeKind,
            Metadata = new()
            {
                Name = "database-root",
                NamespaceProperty = "authentik",
                Uid = "root-endpoint-slice-uid",
                OwnerReferences = [new() { ApiVersion = "v1", Kind = V1Service.KubeKind, Name = "database", Uid = "service-uid" }],
            },
            Endpoints = [new() { TargetRef = new() { ApiVersion = "v1", Kind = V1Pod.KubeKind, Name = "database-0", NamespaceProperty = "authentik", Uid = "pod-uid" } }],
        };
        V1EndpointSlice sibling = new()
        {
            ApiVersion = "discovery.k8s.io/v1",
            Kind = V1EndpointSlice.KubeKind,
            Metadata = new()
            {
                Name = "database-sibling",
                NamespaceProperty = "authentik",
                Uid = "sibling-endpoint-slice-uid",
                OwnerReferences = [new() { ApiVersion = "v1", Kind = V1Service.KubeKind, Name = "database", Uid = "service-uid" }],
            },
            Endpoints = [new() { TargetRef = new() { ApiVersion = "v1", Kind = V1Pod.KubeKind, Name = "database-1", NamespaceProperty = "authentik", Uid = "sibling-pod-uid" } }],
        };

        ResourceRelationshipGraph graph = new ResourceRelationshipBuilder().Build([service, pod, siblingPod, root, sibling], new HashSet<string>(), hideNoise: true);
        ResourceRelationshipGraph delta = new ResourceRelationshipBuilder().BuildAdditionDelta(
            [service, pod, siblingPod, root, sibling],
            new ResourceKey("discovery.k8s.io/v1", V1EndpointSlice.KubeKind, "authentik", "database-root"),
            new HashSet<string>(),
            hideNoise: true);
        delta.Resources.Select(resource => resource.Name()).ShouldBe(["database", "database-0", "database-root"]);
        ResourceRelationshipGraph filtered = VisualizationViewModel.FilterToRootResource(graph, root);

        filtered.Resources.Select(resource => resource.Name()).ShouldBe(["database", "database-0", "database-root"]);
    }

    [Fact]
    public void resource_type_selection_persists_across_graph_updates_and_selects_new_types()
    {
        V1Pod pod = CreatePod("pod");
        V1Service service = new()
        {
            ApiVersion = "v1",
            Kind = V1Service.KubeKind,
            Metadata = new() { Name = "service", NamespaceProperty = "default", Uid = "service" },
        };
        V1Deployment deployment = new()
        {
            ApiVersion = "apps/v1",
            Kind = V1Deployment.KubeKind,
            Metadata = new() { Name = "deployment", NamespaceProperty = "default", Uid = "deployment" },
        };

        using VisualizationViewModel viewModel = new(new ResourceRelationshipBuilder());
        viewModel.ApplyGraph(new ResourceRelationshipGraph([pod, service], []));

        viewModel.ResourceTypes.ShouldBe([V1Pod.KubeKind, V1Service.KubeKind]);
        viewModel.SelectedResourceTypes.ShouldBe([V1Pod.KubeKind, V1Service.KubeKind]);

        viewModel.SelectedResourceTypes.Remove(V1Pod.KubeKind);
        viewModel.Graph!.Resources.Select(resource => resource.Kind).ShouldBe([V1Service.KubeKind]);

        viewModel.ApplyGraph(new ResourceRelationshipGraph([pod, service, deployment], []));

        viewModel.ResourceTypes.ShouldBe([V1Deployment.KubeKind, V1Pod.KubeKind, V1Service.KubeKind]);
        viewModel.SelectedResourceTypes.ShouldBe([V1Service.KubeKind, V1Deployment.KubeKind]);
        viewModel.Graph!.Resources.Select(resource => resource.Kind).ShouldBe([V1Service.KubeKind, V1Deployment.KubeKind]);
    }

    [Fact]
    public void selected_namespace_keeps_cross_namespace_gitops_application()
    {
        V1Deployment managed = new()
        {
            ApiVersion = "apps/v1",
            Kind = V1Deployment.KubeKind,
            Metadata = new()
            {
                Name = "managed",
                NamespaceProperty = "workload",
                Annotations = new Dictionary<string, string>
                {
                    ["argocd.argoproj.io/tracking-id"] = "demo-app:apps/Deployment:workload/managed",
                },
            },
        };
        V1ConfigMap application = new()
        {
            ApiVersion = "argoproj.io/v1alpha1",
            Kind = "Application",
            Metadata = new() { Name = "demo-app", NamespaceProperty = "argocd" },
        };
        V1Deployment unrelatedManaged = new()
        {
            ApiVersion = "apps/v1",
            Kind = V1Deployment.KubeKind,
            Metadata = new()
            {
                Name = "unrelated-managed",
                NamespaceProperty = "other-workload",
                Annotations = new Dictionary<string, string>
                {
                    ["argocd.argoproj.io/tracking-id"] = "demo-app:apps/Deployment:other-workload/unrelated-managed",
                },
            },
        };
        ResourceRelationshipGraph graph = new ResourceRelationshipBuilder().Build(
            [managed, application, unrelatedManaged],
            new HashSet<string>(),
            hideNoise: true);

        ResourceRelationshipGraph filtered = VisualizationViewModel.FilterToSelectedNamespaces(
            graph,
            new HashSet<string> { "workload" });

        filtered.Resources.Select(resource => resource.Name()).ShouldBe(["managed", "demo-app"]);
        filtered.Relationships.ShouldContain(new ResourceRelationship(
            new("argoproj.io/v1alpha1", "Application", "argocd", "demo-app", null),
            new("apps/v1", V1Deployment.KubeKind, "workload", "managed", null),
            ResourceRelationshipKind.GitOps));
    }

    [Fact]
    public void selected_namespace_does_not_include_unselected_gitops_cluster_scoped_descendant()
    {
        V1Deployment managed = new()
        {
            ApiVersion = "apps/v1",
            Kind = V1Deployment.KubeKind,
            Metadata = new()
            {
                Name = "managed",
                NamespaceProperty = "workload",
                Annotations = new Dictionary<string, string>
                {
                    ["argocd.argoproj.io/tracking-id"] = "demo-app:apps/Deployment:workload/managed",
                },
            },
        };
        V1ClusterRole unrelatedClusterResource = new()
        {
            ApiVersion = "rbac.authorization.k8s.io/v1",
            Kind = V1ClusterRole.KubeKind,
            Metadata = new()
            {
                Name = "unrelated-cluster-resource",
                Annotations = new Dictionary<string, string>
                {
                    ["argocd.argoproj.io/tracking-id"] = "demo-app:rbac.authorization.k8s.io/ClusterRole:unrelated-cluster-resource",
                },
            },
        };
        V1ConfigMap application = new()
        {
            ApiVersion = "argoproj.io/v1alpha1",
            Kind = "Application",
            Metadata = new() { Name = "demo-app", NamespaceProperty = "argocd" },
        };
        ResourceRelationshipGraph graph = new ResourceRelationshipBuilder().Build(
            [managed, unrelatedClusterResource, application],
            new HashSet<string>(),
            hideNoise: true);

        ResourceRelationshipGraph filtered = VisualizationViewModel.FilterToSelectedNamespaces(
            graph,
            new HashSet<string> { "workload" });

        filtered.Resources.Select(resource => resource.Name()).ShouldBe(["managed", "demo-app"]);
        filtered.Resources.Select(resource => resource.Name()).ShouldNotContain("unrelated-cluster-resource");
    }

    [Fact]
    public void incremental_gitops_application_keeps_both_cross_namespace_endpoints()
    {
        TestDynamicResource managed = new()
        {
            ApiVersion = "pkg.crossplane.io/v1",
            Kind = "Provider",
            Metadata = new()
            {
                Name = "provider-databricks",
                NamespaceProperty = "crossplane-system",
                Annotations = new Dictionary<string, string>
                {
                    ["argocd.argoproj.io/tracking-id"] = "crossplane-providers:pkg.crossplane.io/Provider:crossplane-system/provider-databricks",
                },
            },
        };
        V1ConfigMap application = new()
        {
            ApiVersion = "argoproj.io/v1alpha1",
            Kind = "Application",
            Metadata = new() { Name = "crossplane-providers", NamespaceProperty = "argocd" },
        };
        ResourceRelationshipGraph delta = new ResourceRelationshipBuilder().BuildAdditionDelta(
            [managed, application],
            new ResourceKey("argoproj.io/v1alpha1", "Application", "argocd", "crossplane-providers"),
            new HashSet<string> { "crossplane-system" },
            hideNoise: true);

        ResourceRelationshipGraph filtered = VisualizationViewModel.FilterIncrementalDelta(
            delta,
            new HashSet<string> { "crossplane-system" },
            new HashSet<ResourceIdentity> { new("pkg.crossplane.io/v1", "Provider", "crossplane-system", "provider-databricks", null) });

        filtered.Resources.Select(resource => resource.Name()).ShouldBe(["provider-databricks", "crossplane-providers"]);
        filtered.Relationships.ShouldContain(new ResourceRelationship(
            new("argoproj.io/v1alpha1", "Application", "argocd", "crossplane-providers", null),
            new("pkg.crossplane.io/v1", "Provider", "crossplane-system", "provider-databricks", null),
            ResourceRelationshipKind.GitOps));
    }

    [Fact]
    public void not_ready_filter_keeps_only_resources_with_false_conditions()
    {
        V1Pod ready = CreatePod("ready");
        ready.Status = new() { Conditions = [new() { Type = "Ready", Status = "True" }] };
        V1Pod notReady = CreatePod("not-ready");
        notReady.Status = new() { Conditions = [new() { Type = "Ready", Status = "False" }] };
        V1Pod unknown = CreatePod("unknown");
        unknown.Status = new() { Conditions = [new() { Type = "Ready", Status = "Unknown" }] };
        V1Pod withoutConditions = CreatePod("without-conditions");

        using VisualizationViewModel viewModel = new(new ResourceRelationshipBuilder());
        viewModel.ApplyGraph(new ResourceRelationshipGraph([ready, notReady, unknown, withoutConditions], []));

        viewModel.ShowNotReadyOnly = true;

        viewModel.Graph!.Resources.Select(resource => resource.Name()).ShouldBe(["not-ready"]);
    }

    [Fact]
    public void type_filter_preserves_pending_references_and_seed_prerequisites()
    {
        V1Pod pod = CreatePod("pod");
        UnresolvedResourceReference pending = new("apps", "v1", "Deployment", "default", "owner");
        ResourceSeedPrerequisite prerequisite = new(typeof(V1Deployment));

        using VisualizationViewModel viewModel = new(new ResourceRelationshipBuilder());
        viewModel.ApplyGraph(new ResourceRelationshipGraph(
            [pod],
            [],
            new HashSet<UnresolvedResourceReference> { pending },
            new HashSet<ResourceSeedPrerequisite> { prerequisite }));

        viewModel.ShowNotReadyOnly = true;

        viewModel.Graph!.PendingReferences.ShouldContain(pending);
        viewModel.Graph.RequiredSeedPrerequisites.ShouldContain(prerequisite);
    }

    [Fact]
    public void resource_type_is_selected_again_after_disappearing_and_reappearing()
    {
        V1Pod pod = CreatePod("pod");
        V1Service service = new()
        {
            ApiVersion = "v1",
            Kind = V1Service.KubeKind,
            Metadata = new() { Name = "service", NamespaceProperty = "default", Uid = "service" },
        };

        using VisualizationViewModel viewModel = new(new ResourceRelationshipBuilder());
        viewModel.ApplyGraph(new ResourceRelationshipGraph([pod, service], []));
        viewModel.ApplyGraph(new ResourceRelationshipGraph([service], []));
        viewModel.ApplyGraph(new ResourceRelationshipGraph([pod, service], []));

        viewModel.ResourceTypes.ShouldBe([V1Pod.KubeKind, V1Service.KubeKind]);
        viewModel.SelectedResourceTypes.Order(StringComparer.Ordinal).ShouldBe([V1Pod.KubeKind, V1Service.KubeKind]);
        viewModel.Graph!.Resources.ShouldBe([pod, service]);
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task late_incremental_delta_does_not_overwrite_newer_rebuild(KubernetesBackend backend)
    {
        await using var clusterScope = await KubernetesTestWorkspaceScope.CreateAsync(TestApp.CurrentServices!, backend);
        var cluster = clusterScope.Workspace;
        await cluster.SeedResource<V1Pod>(true);
        var builder = new LateAdditionRelationshipBuilder();
        using VisualizationViewModel viewModel = new(builder);
        cluster.SelectedNamespaces.Clear();
        cluster.SelectedNamespaces.Add(new V1Namespace { Metadata = new() { Name = "default" } });
        viewModel.Initialize(cluster);
        Dispatcher.UIThread.RunJobs();
        await builder.WaitForInitialBuildAsync().WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Dispatcher.UIThread.RunJobs();
        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        V1Pod pod = CreatePod("late");
        await cluster.AddOrUpdateResource(pod);
        await builder.WaitForAdditionStartedAsync().WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);

        viewModel.HideNoise = false;
        await builder.WaitForSecondBuildAsync().WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Dispatcher.UIThread.RunJobs();
        viewModel.Graph!.Resources.ShouldBeEmpty();

        await builder.WaitForAdditionCompletedAsync().WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        Dispatcher.UIThread.RunJobs();

        viewModel.Graph!.Resources.ShouldBeEmpty();
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task modified_resource_rebuilds_changed_relationships_in_visualization_graph(KubernetesBackend backend)
    {
        await using var clusterScope = await KubernetesTestWorkspaceScope.CreateAsync(TestApp.CurrentServices!, backend);
        var cluster = clusterScope.Workspace;
        cluster.SelectedNamespaces.Add(new V1Namespace { Metadata = new() { Name = "default" } });
        using VisualizationViewModel viewModel = new(new OwnerRelationshipBuilder());
        await cluster.Runtime.SeedResource<V1Deployment>(true);
        await cluster.Runtime.SeedResource<V1Pod>(true);
        viewModel.Initialize(cluster);

        V1Deployment firstOwner = CreateDeployment("first-owner");
        V1Deployment secondOwner = CreateDeployment("second-owner");
        V1Pod pod = CreatePodWithOwner("owned-pod", firstOwner);

        await cluster.AddOrUpdateResource(firstOwner);
        await cluster.AddOrUpdateResource(secondOwner);
        await cluster.AddOrUpdateResource(pod);
        Dispatcher.UIThread.RunJobs();

        ResourceIdentity firstOwnerIdentity = GetIdentity(firstOwner);
        ResourceIdentity secondOwnerIdentity = GetIdentity(secondOwner);
        ResourceIdentity podIdentity = GetIdentity(pod);
        ResourceRelationship initialRelationship = new(firstOwnerIdentity, podIdentity, ResourceRelationshipKind.Owner);
        ResourceRelationship changedRelationship = new(secondOwnerIdentity, podIdentity, ResourceRelationshipKind.Owner);
        viewModel.ApplyGraph(new ResourceRelationshipGraph([firstOwner, secondOwner, pod], [initialRelationship]));

        pod.Metadata!.OwnerReferences =
        [
            new()
            {
                ApiVersion = secondOwner.ApiVersion,
                Kind = secondOwner.Kind,
                Name = secondOwner.Name(),
                Uid = secondOwner.Uid(),
            },
        ];
        await cluster.AddOrUpdateResource(pod);

        Stopwatch timeout = Stopwatch.StartNew();
        while (timeout.Elapsed < TimeSpan.FromSeconds(5)
            && !viewModel.Graph!.Relationships.Contains(changedRelationship))
        {
            Dispatcher.UIThread.RunJobs();
            await WaitForNextPollAsync();
        }

        viewModel.Graph!.Relationships.ShouldContain(changedRelationship);
        viewModel.Graph.Relationships.ShouldNotContain(initialRelationship);
    }

    private static ResourceIdentity GetIdentity(IKubernetesObject<V1ObjectMeta> resource)
        => new(resource.ApiVersion!, resource.Kind!, resource.Namespace(), resource.Name()!, resource.Uid());

    private static V1Deployment CreateDeployment(string name) => new()
    {
        ApiVersion = "apps/v1",
        Kind = V1Deployment.KubeKind,
        Metadata = new()
        {
            Name = name,
            NamespaceProperty = "default",
            Uid = name,
        },
    };

    private static V1Pod CreatePodWithOwner(string name, V1Deployment owner) => new()
    {
        ApiVersion = "v1",
        Kind = V1Pod.KubeKind,
        Metadata = new()
        {
            Name = name,
            NamespaceProperty = "default",
            Uid = name,
            OwnerReferences =
            [
                new()
                {
                    ApiVersion = owner.ApiVersion,
                    Kind = owner.Kind,
                    Name = owner.Name(),
                    Uid = owner.Uid(),
                },
            ],
        },
    };

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task processed_resource_config_starts_required_seed_without_waiting_for_ready(KubernetesBackend backend)
    {
        await using var scope = await KubernetesTestWorkspaceScope.CreateAsync(TestApp.CurrentServices!, backend);
        var cluster = scope.Workspace;
        var runtime = cluster.Runtime;
        using VisualizationViewModel viewModel = new(new ResourceRelationshipBuilder());

        viewModel.Initialize(cluster);

        await cluster.Connect();
        runtime.GetResourceSourceCache<Corev1Event>().ShouldNotBeNull();
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task applying_graph_starts_provider_prerequisite_seed(KubernetesBackend backend)
    {
        await using var scope = await KubernetesTestWorkspaceScope.CreateAsync(TestApp.CurrentServices!, backend);
        var cluster = scope.Workspace;
        var runtime = cluster.Runtime;
        await cluster.Connect();

        var seedRequested = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        void OnResourceSeeded(IClusterRuntime _, GroupApiVersionKind kind)
        {
            if (kind == GroupApiVersionKind.From<V1Node>())
            {
                seedRequested.TrySetResult(false);
            }
        }

        runtime.ResourceSeeded += OnResourceSeeded;
        try
        {
            using VisualizationViewModel viewModel = new(new ResourceRelationshipBuilder());
            viewModel.Initialize(cluster);
            viewModel.ApplyGraph(new ResourceRelationshipGraph(
                [],
                [],
                SeedPrerequisites: new HashSet<ResourceSeedPrerequisite>
                {
                    new(typeof(V1Node)),
                }));

            (await seedRequested.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken)).ShouldBeFalse();
        }
        finally
        {
            runtime.ResourceSeeded -= OnResourceSeeded;
        }
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task added_resource_starts_owner_reference_seed_without_waiting_for_ready(KubernetesBackend backend)
    {
        await using var scope = await KubernetesTestWorkspaceScope.CreateAsync(TestApp.CurrentServices!, backend);
        var cluster = scope.Workspace;
        var runtime = cluster.Runtime;
        var seedRequested = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        void OnResourceSeeded(IClusterRuntime _, GroupApiVersionKind kind)
        {
            if (kind == GroupApiVersionKind.From<V1CustomResourceDefinition>())
            {
                seedRequested.TrySetResult(true);
            }
        }

        runtime.ResourceSeeded += OnResourceSeeded;
        using VisualizationViewModel viewModel = new(new ResourceRelationshipBuilder());
        try
        {
            await cluster.Connect();
            viewModel.Initialize(cluster);
            await runtime.AddOrUpdateResource(new V1Pod
            {
                ApiVersion = "v1",
                Kind = V1Pod.KubeKind,
                Metadata = new()
                {
                    Name = "owned-pod",
                    NamespaceProperty = "default",
                    OwnerReferences =
                    [
                        new()
                        {
                            ApiVersion = "apiextensions.k8s.io/v1",
                            Kind = V1CustomResourceDefinition.KubeKind,
                            Name = "owner",
                            Uid = "owner",
                        },
                    ],
                },
                Spec = new()
                {
                    Containers =
                    [
                        new() { Name = "owned", Image = "busybox" },
                    ],
                },
            });

            (await seedRequested.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken)).ShouldBeTrue();
        }
        finally
        {
            runtime.ResourceSeeded -= OnResourceSeeded;
        }
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task added_unrelated_cluster_scoped_resource_does_not_bypass_namespace_filter(KubernetesBackend backend)
    {
        await using var clusterScope = await KubernetesTestWorkspaceScope.CreateAsync(TestApp.CurrentServices!, backend);
        var cluster = clusterScope.Workspace;
        cluster.SelectedNamespaces.Add(new V1Namespace { Metadata = new() { Name = "default" } });
        var builder = new AdditionCaptureRelationshipBuilder();
        using VisualizationViewModel viewModel = new(builder);
        await cluster.Runtime.SeedResource<V1Node>(true);
        viewModel.Initialize(cluster);
        Dispatcher.UIThread.RunJobs();
        await builder.WaitForInitialBuildAsync();
        DateTime initialGraphDeadline = DateTime.UtcNow.AddSeconds(5);
        while (!viewModel.Graph!.Resources.Any(resource => resource.Name() == "selected"))
        {
            Dispatcher.UIThread.RunJobs();
            if (DateTime.UtcNow >= initialGraphDeadline)
            {
                throw new TimeoutException("Timed out waiting for the initial visualization graph.");
            }

            await WaitForNextPollAsync();
        }

        await Dispatcher.UIThread.InvokeAsync(() => { }, DispatcherPriority.Background);

        await cluster.Runtime.AddOrUpdateResource(new V1Node
        {
            ApiVersion = "v1",
            Kind = V1Node.KubeKind,
            Metadata = new() { Name = "unrelated-node", Uid = "unrelated-node" },
        });

        ResourceRelationshipGraph delta = await builder.WaitForAdditionAsync();

        delta.Resources.ShouldBeEmpty();
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task changing_selected_namespaces_rebuilds_graph_with_new_namespace_filter(KubernetesBackend backend)
    {
        await using var clusterScope = await KubernetesTestWorkspaceScope.CreateAsync(TestApp.CurrentServices!, backend);
        var cluster = clusterScope.Workspace;
        cluster.SelectedNamespaces.Add(new V1Namespace { Metadata = new() { Name = "default" } });
        await cluster.Runtime.AddOrUpdateResource(new V1Namespace { Metadata = new() { Name = "other" } });
        var builder = new BuildCaptureRelationshipBuilder();
        using VisualizationViewModel viewModel = new(builder);

        viewModel.Initialize(cluster);
        (await builder.WaitForBuildAsync(1)).SelectedNamespaces.ShouldBe(["default"]);

        cluster.SelectedNamespaces.Clear();
        cluster.SelectedNamespaces.Add(cluster.Runtime.Namespaces.Single(item => item.Name() == "other"));
        Dispatcher.UIThread.RunJobs();

        (await builder.WaitForBuildAsync(input => input.SelectedNamespaces.SequenceEqual(["other"])))
            .SelectedNamespaces.ShouldBe(["other"]);
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task full_graph_rebuild_does_not_reintroduce_unselected_namespaced_resources(KubernetesBackend backend)
    {
        await using var clusterScope = await KubernetesTestWorkspaceScope.CreateAsync(TestApp.CurrentServices!, backend);
        var cluster = clusterScope.Workspace;
        cluster.SelectedNamespaces.Add(new V1Namespace { Metadata = new() { Name = "default" } });
        var builder = new NamespaceLeakingBuildRelationshipBuilder();
        using VisualizationViewModel viewModel = new(builder);

        viewModel.Initialize(cluster);
        await builder.WaitForBuildAsync();
        DateTime deadline = DateTime.UtcNow.AddSeconds(5);
        while (viewModel.Graph == null || viewModel.Graph.Resources.Count == 0)
        {
            Dispatcher.UIThread.RunJobs();
            if (DateTime.UtcNow >= deadline)
            {
                throw new TimeoutException("Timed out waiting for the namespace-filtered graph.");
            }

            await WaitForNextPollAsync();
        }

        viewModel.Graph.Resources.Select(resource => resource.Name()).ShouldNotContain("unselected");
        viewModel.Graph.Resources.Select(resource => resource.Name()).ShouldNotContain("unrelated-node");
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task changing_hide_noise_rebuilds_graph_with_new_noise_filter(KubernetesBackend backend)
    {
        await using var clusterScope = await KubernetesTestWorkspaceScope.CreateAsync(TestApp.CurrentServices!, backend);
        var cluster = clusterScope.Workspace;
        cluster.SelectedNamespaces.Add(new V1Namespace { Metadata = new() { Name = "default" } });
        var builder = new BuildCaptureRelationshipBuilder();
        using VisualizationViewModel viewModel = new(builder);

        viewModel.Initialize(cluster);
        (await builder.WaitForBuildAsync(1)).HideNoise.ShouldBeTrue();

        viewModel.HideNoise = false;
        Dispatcher.UIThread.RunJobs();

        (await builder.WaitForBuildAsync(2)).HideNoise.ShouldBeFalse();
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task added_unrelated_namespaced_resource_does_not_bypass_namespace_filter(KubernetesBackend backend)
    {
        await using var clusterScope = await KubernetesTestWorkspaceScope.CreateAsync(TestApp.CurrentServices!, backend);
        var cluster = clusterScope.Workspace;
        cluster.SelectedNamespaces.Add(new V1Namespace { Metadata = new() { Name = "default" } });
        var builder = new AdditionCaptureRelationshipBuilder();
        using VisualizationViewModel viewModel = new(builder);
        await cluster.Runtime.SeedResource<V1Pod>(true);
        viewModel.Initialize(cluster);
        await builder.WaitForInitialBuildAsync();

        await cluster.Runtime.AddOrUpdateResource(new V1Pod
        {
            ApiVersion = "v1",
            Kind = V1Pod.KubeKind,
            Metadata = new() { Name = "unrelated", NamespaceProperty = "other", Uid = "unrelated" },
        });

        Dispatcher.UIThread.RunJobs();
        viewModel.Graph!.Resources.ShouldNotContain(resource => resource.Name() == "unrelated");
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task repeated_incremental_deltas_do_not_accumulate_resources_from_unselected_namespaces(KubernetesBackend backend)
    {
        await using var clusterScope = await KubernetesTestWorkspaceScope.CreateAsync(TestApp.CurrentServices!, backend);
        var cluster = clusterScope.Workspace;
        V1Pod selected = CreatePod("selected");
        V1Pod unrelated = new()
        {
            ApiVersion = "v1",
            Kind = V1Pod.KubeKind,
            Metadata = new() { Name = "unrelated", NamespaceProperty = "other", Uid = "unrelated" },
        };
        await cluster.Runtime.AddOrUpdateResource(selected);
        await cluster.Runtime.AddOrUpdateResource(unrelated);
        await cluster.Runtime.AddOrUpdateResource(new V1Node
        {
            ApiVersion = "v1",
            Kind = V1Node.KubeKind,
            Metadata = new() { Name = "seed-node", Uid = "seed-node" },
        });
        await cluster.Runtime.SeedResource<V1Pod>(true);

        var builder = new LeakyAdditionRelationshipBuilder();
        using VisualizationViewModel viewModel = new(builder);
        viewModel.Initialize(cluster);
        cluster.SelectedNamespaces.Add(cluster.Runtime.Namespaces.Single(namespaceResource => namespaceResource.Name() == "default"));
        await builder.WaitForInitialBuildAsync();
        DateTime initialGraphDeadline = DateTime.UtcNow.AddSeconds(5);
        while (!viewModel.Graph!.Resources.Any(resource => resource.Name() == "selected"))
        {
            Dispatcher.UIThread.RunJobs();
            if (DateTime.UtcNow >= initialGraphDeadline)
            {
                throw new TimeoutException("Timed out waiting for the initial visualization graph.");
            }

            await TestWait.NextPollAsync(TimeSpan.FromMilliseconds(10), TestContext.Current.CancellationToken);
        }

        viewModel.Graph!.Resources.Select(resource => resource.Name()).ShouldNotContain("unrelated");

        for (int i = 0; i < 3; i++)
        {
            await cluster.Runtime.AddOrUpdateResource(CreatePod($"incremental-{i}"));
            Dispatcher.UIThread.RunJobs();
            await builder.WaitForAdditionAsync();
            Dispatcher.UIThread.RunJobs();
        }

        viewModel.Graph!.Resources.Select(resource => resource.Name()).ShouldNotContain("unrelated");
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task seeded_prerequisite_triggers_graph_rebuild(KubernetesBackend backend)
    {
        await using var clusterScope = await KubernetesTestWorkspaceScope.CreateAsync(TestApp.CurrentServices!, backend);
        var cluster = clusterScope.Workspace;
        var builder = new BuildCaptureRelationshipBuilder();
        using VisualizationViewModel viewModel = new(builder);
        cluster.SelectedNamespaces.Add(new V1Namespace { Metadata = new() { Name = "default" } });
        viewModel.Initialize(cluster);
        await builder.WaitForBuildAsync(1);

        ResourceSeedPrerequisite prerequisite = new(typeof(V1Deployment));
        viewModel.ApplyGraph(new ResourceRelationshipGraph([], [], SeedPrerequisites: new HashSet<ResourceSeedPrerequisite> { prerequisite }));
        await cluster.Runtime.SeedResource(typeof(V1Deployment));
        await builder.WaitForBuildAsync(2);
    }

    private sealed class LeakyAdditionRelationshipBuilder : IResourceRelationshipBuilder
    {
        private readonly TaskCompletionSource _initialBuild = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly ConcurrentQueue<TaskCompletionSource> _additions = [];

        public ResourceRelationshipGraph Build(
            IEnumerable<IKubernetesObject<V1ObjectMeta>> resources,
            IReadOnlySet<string> selectedNamespaces,
            bool hideNoise)
        {
            _initialBuild.TrySetResult();
            return new ResourceRelationshipGraph(
                [
                    CreatePod("selected"),
                    CreatePod("unselected", "other"),
                    new V1Node
                    {
                        ApiVersion = "v1",
                        Kind = V1Node.KubeKind,
                        Metadata = new() { Name = "unrelated-node", Uid = "unrelated-node" },
                    },
                ],
                []);
        }

        public ResourceRelationshipGraph BuildAdditionDelta(
            IEnumerable<IKubernetesObject<V1ObjectMeta>> resources,
            ResourceKey addedResource,
            IReadOnlySet<string> selectedNamespaces,
            bool hideNoise)
        {
            TaskCompletionSource addition = new(TaskCreationOptions.RunContinuationsAsynchronously);
            _additions.Enqueue(addition);
            addition.TrySetResult();
            return new ResourceRelationshipGraph(resources.ToArray(), []);
        }

        public async Task WaitForInitialBuildAsync() => await WaitForSignalAsync(_initialBuild.Task, "initial visualization build");

        public async Task WaitForAdditionAsync()
        {
            TaskCompletionSource? addition;
            DateTime deadline = DateTime.UtcNow.AddSeconds(5);
            while (!_additions.TryDequeue(out addition))
            {
                Dispatcher.UIThread.RunJobs();
                if (DateTime.UtcNow >= deadline)
                {
                    throw new TimeoutException("Timed out waiting for an incremental graph addition.");
                }

                await WaitForNextPollAsync();
            }

            await addition.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        }
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task disposed_view_model_unsubscribes_from_namespace_changes(KubernetesBackend backend)
    {
        await using var clusterScope = await KubernetesTestWorkspaceScope.CreateAsync(TestApp.CurrentServices!, backend);
        var cluster = clusterScope.Workspace;
        cluster.SelectedNamespaces.Add(new V1Namespace { Metadata = new() { Name = "default" } });
        var builder = new BuildCaptureRelationshipBuilder();
        using VisualizationViewModel viewModel = new(builder);
        viewModel.Initialize(cluster);
        await builder.WaitForBuildAsync(1);

        viewModel.Dispose();
        cluster.SelectedNamespaces.Add(new V1Namespace { Metadata = new() { Name = "other" } });
        Dispatcher.UIThread.RunJobs();

        builder.BuildCount.ShouldBe(1);
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task background_resource_changes_are_processed_on_the_ui_thread(KubernetesBackend backend)
    {
        await using var clusterScope = await KubernetesTestWorkspaceScope.CreateAsync(TestApp.CurrentServices!, backend);
        var cluster = clusterScope.Workspace;
        cluster.SelectedNamespaces.Add(new V1Namespace { Metadata = new() { Name = "default" } });
        var builder = new BuildCaptureRelationshipBuilder();
        using VisualizationViewModel viewModel = new(builder);
        await cluster.Runtime.SeedResource<V1Pod>(true);
        viewModel.Initialize(cluster);
        await builder.WaitForBuildAsync(1);

        V1Pod background = CreatePod("background");
        await cluster.Runtime.AddOrUpdateResource(background);
        await Task.Run(
            () => cluster.Runtime.AddOrUpdateResource(background),
            TestContext.Current.CancellationToken);

        await WaitForAsync(() => builder.BuildCount > 1);

        builder.BuildCount.ShouldBeGreaterThan(1);
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task disposed_view_model_ignores_runtime_resource_changes(KubernetesBackend backend)
    {
        await using var clusterScope = await KubernetesTestWorkspaceScope.CreateAsync(TestApp.CurrentServices!, backend);
        var cluster = clusterScope.Workspace;
        cluster.SelectedNamespaces.Add(new V1Namespace { Metadata = new() { Name = "default" } });
        var builder = new BuildCaptureRelationshipBuilder();
        using VisualizationViewModel viewModel = new(builder);
        viewModel.Initialize(cluster);
        await builder.WaitForBuildAsync(1);

        viewModel.Dispose();
        await cluster.Runtime.AddOrUpdateResource(CreatePod("after-dispose"));
        Dispatcher.UIThread.RunJobs();

        builder.BuildCount.ShouldBe(1);
    }

    [Fact]
    public void graph_skips_relationships_with_missing_vertices()
    {
        V1Pod source = CreatePod("source");
        ResourceIdentity sourceIdentity = GetIdentity(source);
        ResourceIdentity missingIdentity = new(V1Pod.KubeApiVersion, V1Pod.KubeKind, "default", "missing", "missing");

        using ResourceGraphControl control = new()
        {
            Graph = new ResourceRelationshipGraph(
                [source],
                [new ResourceRelationship(sourceIdentity, missingIdentity, ResourceRelationshipKind.Reference)]),
        };

        control.Area.LogicCore!.Graph.VertexCount.ShouldBe(1);
        control.Area.LogicCore.Graph.EdgeCount.ShouldBe(0);
    }

    [AvaloniaFact]
    public async Task layout_does_not_overlap_measured_resource_vertices()
    {
        V1Pod first = new() { ApiVersion = "v1", Kind = V1Pod.KubeKind, Metadata = new() { Name = "first", NamespaceProperty = "demo", Uid = "first" } };
        V1Pod second = new() { ApiVersion = "v1", Kind = V1Pod.KubeKind, Metadata = new() { Name = "second", NamespaceProperty = "demo", Uid = "second" } };
        using ResourceGraphControl control = new()
        {
            Graph = new ResourceRelationshipGraph([first], []),
        };
        Window window = new() { Width = 800, Height = 600, Content = control };
        try
        {
            window.Show();
            await WaitForAsync(() => control.Area.VertexList.Count == 1);

            control.Graph = new ResourceRelationshipGraph([first, second], []);
            await WaitForAsync(() => control.Area.VertexList.Count == 2);

            VertexControl[] vertices = control.Area.VertexList.Values.ToArray();
            Rect[] bounds = vertices
                .Select(vertex => new Rect(vertex.GetPosition(final: true), vertex.Bounds.Size))
                .ToArray();

            bounds[0].Intersects(bounds[1]).ShouldBeFalse();
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task graph_preserves_vertices_and_edges_when_control_is_detached_and_reattached()
    {
        V1Pod source = new() { ApiVersion = "v1", Kind = V1Pod.KubeKind, Metadata = new() { Name = "source", NamespaceProperty = "demo", Uid = "source" } };
        V1Pod target = new() { ApiVersion = "v1", Kind = V1Pod.KubeKind, Metadata = new() { Name = "target", NamespaceProperty = "demo", Uid = "target" } };
        ResourceIdentity sourceIdentity = new("v1", V1Pod.KubeKind, "demo", "source", "source");
        ResourceIdentity targetIdentity = new("v1", V1Pod.KubeKind, "demo", "target", "target");
        using ResourceGraphControl control = new()
        {
            Graph = new ResourceRelationshipGraph(
                [source, target],
                [new ResourceRelationship(sourceIdentity, targetIdentity, ResourceRelationshipKind.Reference)]),
        };
        Window window = new() { Width = 800, Height = 600, Content = control };
        try
        {
            window.Show();
            await WaitForAsync(() => control.Area.VertexList.Count == 2
                && control.Area.VertexList.Values.All(vertex => vertex.Bounds.Width > 0 && vertex.Bounds.Height > 0));
            control.Area.LogicCore!.Graph.VertexCount.ShouldBe(2);
            control.Area.LogicCore.Graph.EdgeCount.ShouldBe(1);

            window.Content = null;
            await Dispatcher.UIThread.InvokeAsync(window.UpdateLayout);
            window.Content = control;
            await WaitForAsync(() => control.Area.VertexList.Count == 2
                && control.Area.LogicCore?.Graph is { VertexCount: 2, EdgeCount: 1 });
        }
        finally
        {
            window.Close();
        }
    }

    private sealed class EmptyRelationshipBuilder : IResourceRelationshipBuilder
    {
        public ResourceRelationshipGraph Build(
            IEnumerable<IKubernetesObject<V1ObjectMeta>> resources,
            IReadOnlySet<string> selectedNamespaces,
            bool hideNoise)
            => ResourceRelationshipGraph.Empty;

        public ResourceRelationshipGraph BuildAdditionDelta(
            IEnumerable<IKubernetesObject<V1ObjectMeta>> resources,
            ResourceKey addedResource,
            IReadOnlySet<string> selectedNamespaces,
            bool hideNoise)
            => ResourceRelationshipGraph.Empty;
    }

    private sealed class OwnerRelationshipBuilder : IResourceRelationshipBuilder
    {
        public ResourceRelationshipGraph Build(
            IEnumerable<IKubernetesObject<V1ObjectMeta>> resources,
            IReadOnlySet<string> selectedNamespaces,
            bool hideNoise)
        {
            IKubernetesObject<V1ObjectMeta>[] resourceArray = resources.ToArray();
            Dictionary<string, IKubernetesObject<V1ObjectMeta>> resourcesByUid = resourceArray
                .Where(resource => resource.Uid() is not null)
                .ToDictionary(resource => resource.Uid()!, StringComparer.Ordinal);
            List<ResourceRelationship> relationships = [];
            foreach (IKubernetesObject<V1ObjectMeta> resource in resourceArray)
            {
                foreach (V1OwnerReference owner in resource.Metadata?.OwnerReferences ?? [])
                {
                    if (owner.Uid is not null && resourcesByUid.TryGetValue(owner.Uid, out IKubernetesObject<V1ObjectMeta>? ownerResource))
                    {
                        relationships.Add(new(
                            GetIdentity(ownerResource),
                            GetIdentity(resource),
                            ResourceRelationshipKind.Owner));
                    }
                }
            }

            return new ResourceRelationshipGraph(resourceArray, relationships);
        }

        public ResourceRelationshipGraph BuildAdditionDelta(
            IEnumerable<IKubernetesObject<V1ObjectMeta>> resources,
            ResourceKey addedResource,
            IReadOnlySet<string> selectedNamespaces,
            bool hideNoise)
            => ResourceRelationshipGraph.Empty;
    }

    private sealed class AdditionSnapshotRelationshipBuilder : IResourceRelationshipBuilder
    {
        private readonly TaskCompletionSource _initialBuild = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource<AdditionDeltaInput> _firstAddition = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public ResourceRelationshipGraph Build(
            IEnumerable<IKubernetesObject<V1ObjectMeta>> resources,
            IReadOnlySet<string> selectedNamespaces,
            bool hideNoise)
        {
            _initialBuild.TrySetResult();
            return ResourceRelationshipGraph.Empty;
        }

        public ResourceRelationshipGraph BuildAdditionDelta(
            IEnumerable<IKubernetesObject<V1ObjectMeta>> resources,
            ResourceKey addedResource,
            IReadOnlySet<string> selectedNamespaces,
            bool hideNoise)
        {
            if (addedResource.Name == "first")
            {
                _firstAddition.TrySetResult(new AdditionDeltaInput(resources.Select(resource => resource.Name()!).ToArray(), hideNoise));
            }

            return ResourceRelationshipGraph.Empty;
        }

        public async Task WaitForInitialBuildAsync()
        {
            DateTime deadline = DateTime.UtcNow.AddSeconds(5);
            while (!_initialBuild.Task.IsCompleted)
            {
                Dispatcher.UIThread.RunJobs();
                if (DateTime.UtcNow >= deadline)
                {
                    throw new TimeoutException("Timed out waiting for the initial visualization build.");
                }

                await WaitForNextPollAsync();
            }
        }

        public async Task<AdditionDeltaInput> WaitForFirstAdditionAsync()
        {
            await WaitForSignalAsync(_firstAddition.Task, "first incremental addition");
            return await _firstAddition.Task.WaitAsync(TestContext.Current.CancellationToken);
        }
    }

    private sealed class AdditionCaptureRelationshipBuilder : IResourceRelationshipBuilder
    {
        private readonly ResourceRelationshipBuilder _inner = new();
        private readonly TaskCompletionSource _initialBuild = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource<ResourceRelationshipGraph> _addition = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public ResourceRelationshipGraph Build(
            IEnumerable<IKubernetesObject<V1ObjectMeta>> resources,
            IReadOnlySet<string> selectedNamespaces,
            bool hideNoise)
        {
            _initialBuild.TrySetResult();
            return new ResourceRelationshipGraph(
                [
                    CreatePod("selected"),
                    CreatePod("unselected", "other"),
                    new V1Node
                    {
                        ApiVersion = "v1",
                        Kind = V1Node.KubeKind,
                        Metadata = new() { Name = "unrelated-node", Uid = "unrelated-node" },
                    },
                ],
                []);
        }

        public ResourceRelationshipGraph BuildAdditionDelta(
            IEnumerable<IKubernetesObject<V1ObjectMeta>> resources,
            ResourceKey addedResource,
            IReadOnlySet<string> selectedNamespaces,
            bool hideNoise)
        {
            ResourceRelationshipGraph graph = _inner.BuildAdditionDelta(resources, addedResource, selectedNamespaces, hideNoise);
            _addition.TrySetResult(graph);
            return graph;
        }

        public async Task WaitForInitialBuildAsync()
        {
            DateTime deadline = DateTime.UtcNow.AddSeconds(5);
            while (!_initialBuild.Task.IsCompleted)
            {
                Dispatcher.UIThread.RunJobs();
                if (DateTime.UtcNow >= deadline)
                {
                    throw new TimeoutException("Timed out waiting for the initial visualization build.");
                }

                await WaitForNextPollAsync();
            }
        }

        public async Task<ResourceRelationshipGraph> WaitForAdditionAsync()
        {
            await WaitForSignalAsync(_addition.Task, "incremental addition");
            return await _addition.Task.WaitAsync(TestContext.Current.CancellationToken);
        }
    }

    private sealed record AdditionDeltaInput(IReadOnlyList<string> Resources, bool HideNoise);

    private sealed class LateAdditionRelationshipBuilder : IResourceRelationshipBuilder
    {
        private readonly TaskCompletionSource _initialBuild = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource _secondBuild = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource _additionStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource _additionCompleted = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource _releaseAddition = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private int _buildCount;

        public int BuildCount => Volatile.Read(ref _buildCount);

        public ResourceRelationshipGraph Build(
            IEnumerable<IKubernetesObject<V1ObjectMeta>> resources,
            IReadOnlySet<string> selectedNamespaces,
            bool hideNoise)
        {
            if (Interlocked.Increment(ref _buildCount) == 1)
            {
                _initialBuild.TrySetResult();
            }
            else
            {
                _secondBuild.TrySetResult();
            }

            return ResourceRelationshipGraph.Empty;
        }

        public ResourceRelationshipGraph BuildAdditionDelta(
            IEnumerable<IKubernetesObject<V1ObjectMeta>> resources,
            ResourceKey addedResource,
            IReadOnlySet<string> selectedNamespaces,
            bool hideNoise)
        {
            _additionStarted.TrySetResult();
            _releaseAddition.Task.GetAwaiter().GetResult();
            _additionCompleted.TrySetResult();
            IKubernetesObject<V1ObjectMeta>? resource = resources.SingleOrDefault(item => item.Name() == addedResource.Name);
            return resource == null
                ? ResourceRelationshipGraph.Empty
                : new ResourceRelationshipGraph([resource], []);
        }

        public async Task WaitForInitialBuildAsync()
        {
            DateTime deadline = DateTime.UtcNow.AddSeconds(5);
            while (!_initialBuild.Task.IsCompleted)
            {
                Dispatcher.UIThread.RunJobs();
                if (DateTime.UtcNow >= deadline)
                {
                    throw new TimeoutException("Timed out waiting for the initial visualization build.");
                }

                await WaitForNextPollAsync();
            }
        }

        public async Task WaitForSecondBuildAsync()
        {
            await WaitForSignalAsync(_secondBuild.Task, "second visualization build");
            _releaseAddition.TrySetResult();
        }

        public async Task WaitForAdditionStartedAsync() => await WaitForSignalAsync(_additionStarted.Task, "incremental addition");

        public async Task WaitForAdditionCompletedAsync() => await WaitForSignalAsync(_additionCompleted.Task, "incremental addition completion");

    }

    private sealed class BuildCaptureRelationshipBuilder : IResourceRelationshipBuilder
    {
        private readonly ConcurrentDictionary<int, TaskCompletionSource<BuildInput>> _builds = [];
        private int _buildCount;

        public int BuildCount => Volatile.Read(ref _buildCount);

        public ResourceRelationshipGraph Build(
            IEnumerable<IKubernetesObject<V1ObjectMeta>> resources,
            IReadOnlySet<string> selectedNamespaces,
            bool hideNoise)
        {
            int buildNumber = Interlocked.Increment(ref _buildCount);
            _builds.GetOrAdd(buildNumber, _ => new(TaskCreationOptions.RunContinuationsAsynchronously))
                .TrySetResult(new(selectedNamespaces.Order(StringComparer.Ordinal).ToArray(), hideNoise));
            return ResourceRelationshipGraph.Empty;
        }

        public ResourceRelationshipGraph BuildAdditionDelta(
            IEnumerable<IKubernetesObject<V1ObjectMeta>> resources,
            ResourceKey addedResource,
            IReadOnlySet<string> selectedNamespaces,
            bool hideNoise)
            => ResourceRelationshipGraph.Empty;

        public async Task<BuildInput> WaitForBuildAsync(int buildNumber)
        {
            Task<BuildInput> build = _builds.GetOrAdd(buildNumber, _ => new(TaskCreationOptions.RunContinuationsAsynchronously)).Task;
            await WaitForAsync(() => build.IsCompleted);
            return await build.WaitAsync(TestContext.Current.CancellationToken);
        }

        public async Task<BuildInput> WaitForBuildAsync(Func<BuildInput, bool> predicate)
        {
            BuildInput? matchingBuild = null;
            await WaitForAsync(() =>
            {
                matchingBuild = _builds.Values
                    .Where(source => source.Task.IsCompletedSuccessfully)
                    .Select(source => source.Task.Result)
                    .FirstOrDefault(predicate);
                return matchingBuild is not null;
            });
            return matchingBuild!;
        }
    }

    private sealed class NamespaceLeakingBuildRelationshipBuilder : IResourceRelationshipBuilder
    {
        private readonly TaskCompletionSource _build = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public ResourceRelationshipGraph Build(
            IEnumerable<IKubernetesObject<V1ObjectMeta>> resources,
            IReadOnlySet<string> selectedNamespaces,
            bool hideNoise)
        {
            _build.TrySetResult();
            return new ResourceRelationshipGraph(
                [
                    CreatePod("selected"),
                    CreatePod("unselected", "other"),
                    new V1Node
                    {
                        ApiVersion = "v1",
                        Kind = V1Node.KubeKind,
                        Metadata = new() { Name = "unrelated-node", Uid = "unrelated-node" },
                    },
                ],
                []);
        }

        public ResourceRelationshipGraph BuildAdditionDelta(
            IEnumerable<IKubernetesObject<V1ObjectMeta>> resources,
            ResourceKey addedResource,
            IReadOnlySet<string> selectedNamespaces,
            bool hideNoise)
            => ResourceRelationshipGraph.Empty;

        public async Task WaitForBuildAsync() => await WaitForSignalAsync(_build.Task, "initial visualization build");
    }

    private sealed record BuildInput(IReadOnlyList<string> SelectedNamespaces, bool HideNoise);

    private static V1Pod CreatePod(string name, string namespaceName = "default") => new()
    {
        ApiVersion = "v1",
        Kind = V1Pod.KubeKind,
        Metadata = new()
        {
            Name = name,
            NamespaceProperty = namespaceName,
            Uid = name,
        },
    };

    private sealed class TestDynamicResource : IKubernetesObject<V1ObjectMeta>
    {
        public string? ApiVersion { get; set; }
        public string? Kind { get; set; }
        public V1ObjectMeta Metadata { get; set; } = new();
    }
}
