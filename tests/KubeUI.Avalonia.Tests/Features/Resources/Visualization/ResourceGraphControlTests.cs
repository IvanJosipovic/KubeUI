using System.Collections.Concurrent;
using System.Diagnostics;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.Markup.Declarative;
using Avalonia.Styling;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dock.Avalonia.Controls;
using Dock.Model.Controls;
using Dock.Model.Core;
using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Features.Resources.Visualization;
using KubeUI.Avalonia.Services.Icons;
using KubeUI.Avalonia.Tests.Infra;
using KubeUI.Kubernetes.Resources.Relationships;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.Resources.Visualization;

public sealed class ResourceGraphControlTests
{
    [Fact]
    public void visualization_pipeline_error_is_available_to_the_view()
    {
        using VisualizationViewModel viewModel = new(new ResourceRelationshipBuilder());
        viewModel.Error = new KeyNotFoundException("missing visualization resource config");

        viewModel.ErrorMessage.ShouldBe("missing visualization resource config");
    }

    [AvaloniaFact]
    public async Task graph_control_exposes_area_and_ignores_reassigning_same_graph()
    {
        using ResourceGraphControl control = new(Application.Current.GetRequiredTestService<IResourceIconService>());
        control.FactoryRootArea.ShouldBe(control.Area);
        control.Graph = ResourceRelationshipGraph.Empty;
        control.Graph = ResourceRelationshipGraph.Empty;

        await TestApplicationExtensions.WaitForUiAsync();
        control.Graph.ShouldBe(ResourceRelationshipGraph.Empty);
    }

    [AvaloniaFact]
    public async Task graph_assignment_after_detaching_does_not_rebuild()
    {
        using ResourceGraphControl control = new(Application.Current.GetRequiredTestService<IResourceIconService>());
        using var window = Application.Current.CreateTestWindow(content: control);
        try
        {
            window.Show();
            await TestApplicationExtensions.WaitForUiAsync();
            window.Close();
            await TestApplicationExtensions.WaitForUiAsync();

            control.Graph = ResourceRelationshipGraph.Empty;
            control.Graph.ShouldBe(ResourceRelationshipGraph.Empty);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task fire_and_forget_graph_application_publishes_the_graph()
    {
        using VisualizationViewModel viewModel = new(new ResourceRelationshipBuilder());
        V1Pod pod = CreatePod("direct-apply");

        viewModel.ApplyGraph(new ResourceRelationshipGraph([pod], []));

        await WaitForAsync(() => viewModel.Graph!.Resources.Contains(pod));
    }

    [AvaloniaFact]
    public async Task matching_resource_clears_pending_reference()
    {
        using VisualizationViewModel viewModel = new(new ResourceRelationshipBuilder());
        UnresolvedResourceReference pending = new("apps", "v1", "Deployment", "default", "deployment");

        await viewModel.ApplyGraphAsync(new ResourceRelationshipGraph([], [], new HashSet<UnresolvedResourceReference> { pending }));
        viewModel.Graph!.PendingReferences.ShouldContain(pending);

        V1Deployment deployment = new()
        {
            ApiVersion = "apps/v1",
            Kind = V1Deployment.KubeKind,
            Metadata = new() { Name = "deployment", NamespaceProperty = "default" },
        };
        await viewModel.ApplyGraphAsync(new ResourceRelationshipGraph([deployment], []));

        viewModel.Graph!.PendingReferences.ShouldBeEmpty();
    }

    [AvaloniaFact]
    public async Task build_failure_is_reported_as_visualization_error()
    {
        var cluster = await Application.Current.CreateClusterAsync(config => config.Type = KubernetesBackend.Fake);
        cluster.SelectedNamespaces.Add(new V1Namespace { Metadata = new() { Name = "default" } });
        using VisualizationViewModel viewModel = new(new ThrowingRelationshipBuilder());

        viewModel.Initialize(cluster);

        await WaitForAsync(() => viewModel.Error is InvalidOperationException);
        viewModel.ErrorMessage.ShouldBe("relationship build failed");
    }

    [AvaloniaFact]
    public async Task successful_graph_application_clears_previous_visualization_error()
    {
        using VisualizationViewModel viewModel = new(new ResourceRelationshipBuilder());
        viewModel.Error = new InvalidOperationException("stale visualization failure");

        await viewModel.ApplyGraphAsync(new ResourceRelationshipGraph([], []));

        viewModel.Error.ShouldBeNull();
        viewModel.ErrorMessage.ShouldBeNull();
    }

    [AvaloniaFact]
    public async Task changing_root_resource_rebuilds_between_root_and_namespace_scopes()
    {
        var cluster = await Application.Current.CreateClusterAsync(config => config.Type = KubernetesBackend.Fake);
        cluster.SelectedNamespaces.Add(new V1Namespace { Metadata = new() { Name = "default" } });
        using VisualizationViewModel viewModel = new(new ResourceRelationshipBuilder());
        viewModel.Initialize(cluster);
        await WaitForAsync(() => !viewModel.IsRebuildPendingOrRunning);

        viewModel.RootResource = CreatePod("root");
        await WaitForAsync(() => !viewModel.IsNamespaceSelectorVisible);
        viewModel.RootResource = null;
        await WaitForAsync(() => viewModel.IsNamespaceSelectorVisible);
    }

    [AvaloniaFact]
    public async Task changing_resource_type_selection_updates_display_filter()
    {
        using VisualizationViewModel viewModel = new(new ResourceRelationshipBuilder());
        V1Pod pod = CreatePod("filter");

        await viewModel.ApplyGraphAsync(new ResourceRelationshipGraph([pod], []));
        viewModel.SelectedResourceTypes.Remove(V1Pod.KubeKind);
        await WaitForAsync(() => viewModel.Graph!.Resources.Count == 0);
        viewModel.SelectedResourceTypes.Add(V1Pod.KubeKind);
        await WaitForAsync(() => viewModel.Graph!.Resources.Contains(pod));
    }

    [AvaloniaFact]
    public void visualization_view_displays_pipeline_error()
    {
        using VisualizationViewModel viewModel = new(new ResourceRelationshipBuilder())
        {
            Error = new InvalidOperationException("visualization failed"),
        };
        var view = new VisualizationView { DataContext = viewModel };
        using var window = Application.Current.CreateTestWindow(content: view);

        window.Show();
        Dispatcher.UIThread.RunJobs();

        view.GetVisualDescendants().OfType<SelectableTextBlock>()
            .Select(control => control.Text)
            .ShouldContain("visualization failed");
    }

    [Fact]
    public void Served_seed_prerequisite_matches_same_group_and_kind_across_api_versions()
    {
        var prerequisite = new GroupApiVersionKind("gateway.networking.k8s.io", "v1", "HTTPRoute", "httproutes");
        var servedVersion = new GroupApiVersionKind("gateway.networking.k8s.io", "v1beta1", "HTTPRoute", "httproutes");
        var differentKind = new GroupApiVersionKind("gateway.networking.k8s.io", "v1beta1", "Gateway", "gateways");
        var differentGroup = new GroupApiVersionKind("example.io", "v1", "HTTPRoute", "httproutes");

        VisualizationSeedPlanner.MatchesSeedKind(prerequisite, servedVersion).ShouldBeTrue();
        VisualizationSeedPlanner.MatchesSeedKind(prerequisite, differentKind).ShouldBeFalse();
        VisualizationSeedPlanner.MatchesSeedKind(prerequisite, differentGroup).ShouldBeFalse();
    }

    [Fact]
    public void Resource_seed_prerequisite_disables_served_version_fallback_by_default()
    {
        var prerequisite = new ResourceSeedPrerequisite(
            new GroupApiVersionKind("example.io", "v1", "Widget", "widgets"));

        prerequisite.AllowServedVersionFallback.ShouldBeFalse();
    }

    [Fact]
    public void Resource_seed_prerequisite_can_enable_served_version_fallback()
    {
        var prerequisite = new ResourceSeedPrerequisite(
            new GroupApiVersionKind("example.io", "v1", "Widget", "widgets"),
            allowServedVersionFallback: true);

        prerequisite.AllowServedVersionFallback.ShouldBeTrue();
    }

    [Fact]
    public void Resource_seed_prerequisite_can_match_kind_across_api_groups()
    {
        var prerequisite = new ResourceSeedPrerequisite(
            new GroupApiVersionKind(string.Empty, string.Empty, "ProviderConfigUsage", "providerconfigusages"),
            matchAnyApiGroup: true);

        prerequisite.MatchAnyApiGroup.ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task updating_resource_node_raises_a_short_lived_update_signal()
    {
        ResourceNodeViewModel node = new()
        {
            Resource = new V1Pod { Metadata = new V1ObjectMeta { Name = "pod" } },
            Icon = null!,
        };

        node.UpdateResource(new V1Pod { Metadata = new V1ObjectMeta { Name = "pod" } });

        node.IsUpdated.ShouldBeTrue();
        await WaitForAsync(() => !node.IsUpdated, timeoutMs: 2000);
        node.IsUpdated.ShouldBeFalse();
    }

    [Fact]
    public void resource_version_change_is_detected_when_resource_instance_is_reused()
    {
        var resource = new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "pod",
                ResourceVersion = "1",
            },
        };
        var node = new ResourceNodeViewModel
        {
            Resource = resource,
            Icon = null!,
        };

        resource.Metadata.ResourceVersion = "2";

        node.HasResourceChanged(resource).ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task updated_resource_node_resolves_the_dynamic_theme_background()
    {
        ResourceNodeViewModel node = new()
        {
            Resource = new V1Pod { Metadata = new V1ObjectMeta { Name = "pod" } },
            Icon = null!,
        };
        Window window = new()
        {
            Content = VisualizationView.CreateResourceNode(node),
        };

        try
        {
            window.Show();
            node.UpdateResource(new V1Pod { Metadata = new V1ObjectMeta { Name = "pod" } });

            var nodeBorder = window.Content.ShouldBeOfType<Border>();
            var nodeGrid = nodeBorder.Child.ShouldBeOfType<Grid>();
            var flashBorder = nodeGrid.Children[0].ShouldBeOfType<Border>();

            flashBorder.Background.ShouldNotBeNull();
            await WaitForAsync(() => flashBorder.Opacity > 0d);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void visualization_theme_resources_resolve_in_light_variant()
        => AssertVisualizationThemeResourcesResolve(ThemeVariant.Light);

    [AvaloniaFact]
    public void visualization_theme_resources_resolve_in_dark_variant()
        => AssertVisualizationThemeResourcesResolve(ThemeVariant.Dark);

    private static void AssertVisualizationThemeResourcesResolve(ThemeVariant themeVariant)
    {
        var application = Application.Current!;
        var originalThemeVariant = application.RequestedThemeVariant;
        var keys = new[]
        {
            "VisualizationRelationshipOwnerBrush",
            "VisualizationRelationshipReferenceBrush",
            "VisualizationRelationshipSelectorBrush",
            "VisualizationRelationshipLabelBrush",
            "VisualizationRelationshipStorageBrush",
            "VisualizationRelationshipIdentityBrush",
            "VisualizationRelationshipRbacBrush",
            "VisualizationRelationshipEventBrush",
            "VisualizationRelationshipGitOpsBrush",
            "VisualizationRelationshipDefaultBrush",
        };
        var grid = new Grid();
        foreach (var key in keys)
        {
            grid.Children.Add(new Border().BindValue(Border.BackgroundProperty, new DynamicResourceExtension(key)));
        }

        var window = new Window { Content = grid };
        try
        {
            application.RequestedThemeVariant = themeVariant;
            window.Show();
            Dispatcher.UIThread.RunJobs();

            grid.Children.Cast<Border>().Select(border => border.Background).ShouldAllBe(value => value != null);
        }
        finally
        {
            window.Close();
            application.RequestedThemeVariant = originalThemeVariant;
        }
    }

    private static async Task WaitForAsync(Func<bool> predicate, int timeoutMs = 5000, CancellationToken cancellationToken = default)
    {
        await TestWait.UntilAsync(
            predicate,
            timeoutMs,
            cancellationToken == default ? TestContext.Current.CancellationToken : cancellationToken,
            () => Dispatcher.UIThread.RunJobs());
    }

    private static async Task WaitForNextPollAsync(CancellationToken cancellationToken = default)
    {
        await TestWait.NextPollAsync(
            TimeSpan.FromMilliseconds(10),
            cancellationToken == default ? TestContext.Current.CancellationToken : cancellationToken);
    }

    private static async Task WaitForSignalAsync(Task signal, string description)
    {
        await TestWait.UntilAsync(
            () => signal.IsCompleted,
            TimeSpan.FromSeconds(15),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());

        if (signal.IsFaulted)
        {
            throw new InvalidOperationException($"The {description} signal failed.", signal.Exception);
        }
    }

    private static async Task ConnectAndWaitForResourceConfigsAsync(
        ClusterWorkspace cluster,
        params GroupApiVersionKind[] resourceKinds)
    {
        await cluster.Connect();
        await WaitForAsync(() => resourceKinds.All(resourceKind =>
        {
            var resourceConfig = cluster.GetResourceConfig(resourceKind);
            return resourceConfig.PermissionsLoaded && resourceConfig.CanListAndWatch;
        }));
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task visualizing_namespace_links_namespace_selector_and_can_unlink(KubernetesBackend backend)
    {
        var cluster = await Application.Current.CreateClusterAsync(config => config.Type = backend);
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
        await TestApplicationExtensions.WaitForUiAsync();

        ReferenceEquals(viewModel.SelectedNamespaces, cluster.SelectedNamespaces).ShouldBeFalse();
        viewModel.SelectedNamespaces.Select(x => x.Name()).ShouldContain("team-a");

        cluster.SelectedNamespaces.Add(new V1Namespace { Metadata = new() { Name = "team-b" } });
        await TestApplicationExtensions.WaitForUiAsync();

        viewModel.SelectedNamespaces.Select(x => x.Name()).ShouldContain("team-a");
        viewModel.SelectedNamespaces.Select(x => x.Name()).ShouldNotContain("team-b");
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task visualizing_selected_namespace_includes_pvc_already_loaded_before_view_initialization(KubernetesBackend backend)
    {
        var cluster = await Application.Current.CreateClusterAsync(config => config.Type = backend);
        V1Namespace namespaceResource = new() { Metadata = new() { Name = "platform-dev-ijosipov" } };
        await cluster.Runtime.CreateAsync(namespaceResource, TestContext.Current.CancellationToken);
        await cluster.Runtime.SeedResource<V1PersistentVolumeClaim>(true);

        V1PersistentVolumeClaim claim = new()
        {
            ApiVersion = V1PersistentVolumeClaim.KubeApiVersion,
            Kind = V1PersistentVolumeClaim.KubeKind,
            Metadata = new()
            {
                Name = "unity-unitycatalog-server-db",
                NamespaceProperty = "platform-dev-ijosipov",
                Uid = "unity-unitycatalog-server-db-uid",
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
            await cluster.Runtime.CreateAsync(claim, TestContext.Current.CancellationToken);
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

        var cluster = await Application.Current.CreateClusterAsync(
            config => config.Type = backend,
            connect: false);

        await ConnectAndWaitForResourceConfigsAsync(cluster, GroupApiVersionKind.From<V1Secret>());
        V1Namespace namespaceResource = new() { Metadata = new() { Name = "platform-dev-ijosipov" } };
        await cluster.Runtime.CreateAsync(namespaceResource, TestContext.Current.CancellationToken);

        using (VisualizationViewModel firstView = new(new ResourceRelationshipBuilder()))
        {
            firstView.Initialize(cluster, namespaceResource);
            await cluster.Runtime.CreateAsync(secret, TestContext.Current.CancellationToken);
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

        var cluster = await Application.Current.CreateClusterAsync(
            config => config.Type = backend,
            connect: false);

        await ConnectAndWaitForResourceConfigsAsync(cluster, GroupApiVersionKind.From<V1Secret>());
        V1Namespace namespaceResource = new() { Metadata = new() { Name = "platform-dev-ijosipov" } };
        await cluster.Runtime.CreateAsync(namespaceResource, TestContext.Current.CancellationToken);

        using (VisualizationViewModel firstView = new(new ResourceRelationshipBuilder()))
        {
            firstView.Initialize(cluster);
            firstView.SelectedNamespaces.Add(namespaceResource);
            await cluster.Runtime.CreateAsync(secret, TestContext.Current.CancellationToken);
            await cluster.Runtime.SeedResource<V1Secret>(true);
            await WaitForAsync(() => firstView.Graph!.Resources.Any(resource => resource.Name() == secret.Name()));
        }

        using VisualizationViewModel reopenedView = new(new ResourceRelationshipBuilder());
        reopenedView.Initialize(cluster, namespaceResource);

        await WaitForAsync(() => reopenedView.Graph!.Resources.Any(resource => resource.Name() == secret.Name()));
    }

    [AvaloniaFact]
    public async Task closing_and_reopening_visualization_document_rebuilds_graph()
    {
        var cluster = await Application.Current.CreateClusterAsync(config => config.Type = KubernetesBackend.Fake);
        var namespaceResource = new V1Namespace
        {
            ApiVersion = V1Namespace.KubeApiVersion,
            Kind = V1Namespace.KubeKind,
            Metadata = new() { Name = "default", Uid = "default-namespace-uid" },
        };
        cluster.SelectedNamespaces.Add(namespaceResource);
        var pod = CreatePod("reopened");
        await cluster.Runtime.AddOrUpdateResource(pod);

        var factory = Application.Current.GetRequiredTestService<IFactory>();
        var layout = factory.CreateLayout();
        factory.InitLayout(layout);
        var documents = factory.GetDockable<IDocumentDock>("Documents");
        documents.ShouldNotBeNull();
        using var window = Application.Current.CreateTestWindow(content: new DockControl { Layout = layout });
        window.Show();

        var firstView = Application.Current.GetRequiredTestService<VisualizationViewModel>();
        firstView.Initialize(cluster);
        factory.AddToDocuments(firstView).ShouldBeTrue();
        await WaitForAsync(() => firstView.Graph!.Resources.Any(resource => resource.Name() == pod.Name()));

        factory.CloseDockable(firstView);
        documents.VisibleDockables.ShouldNotContain(firstView);

        var reopenedView = Application.Current.GetRequiredTestService<VisualizationViewModel>();
        reopenedView.Initialize(cluster);
        factory.AddToDocuments(reopenedView).ShouldBeTrue();
        documents.VisibleDockables.ShouldContain(reopenedView);
        await WaitForAsync(() => reopenedView.Graph!.Resources.Any(resource => resource.Name() == pod.Name()));

        reopenedView.Dispose();
    }

    [AvaloniaFact]
    public async Task reloading_visualization_before_initial_snapshot_completes_rebuilds_graph()
    {
        var cluster = await Application.Current.CreateClusterAsync(config => config.Type = KubernetesBackend.Fake);
        cluster.SelectedNamespaces.Add(new V1Namespace { Metadata = new() { Name = "default" } });
        await cluster.Runtime.SeedResource<V1Pod>(true);
        V1Pod pod = CreatePod("reload-before-snapshot");
        await cluster.Runtime.AddOrUpdateResource(pod);

        using (VisualizationViewModel firstView = new(new ResourceRelationshipBuilder()))
        {
            firstView.Initialize(cluster);
        }

        using VisualizationViewModel reloadedView = new(new ResourceRelationshipBuilder());
        reloadedView.Initialize(cluster);

        await WaitForAsync(() => reloadedView.Graph!.Resources.Any(resource => resource.Name() == pod.Name()));
    }

    [AvaloniaFact]
    public async Task modifying_unrelated_cluster_resource_does_not_restart_visualization_build()
    {
        var cluster = await Application.Current.CreateClusterAsync(config => config.Type = KubernetesBackend.Fake);
        cluster.SelectedNamespaces.Add(new V1Namespace { Metadata = new() { Name = "default" } });
        var builder = new BuildCaptureRelationshipBuilder();
        using VisualizationViewModel viewModel = new(builder);
        await cluster.Runtime.SeedResource<V1Node>(true);
        viewModel.Initialize(cluster);
        await builder.WaitForBuildAsync(1);

        V1Node unrelated = new()
        {
            ApiVersion = V1Node.KubeApiVersion,
            Kind = V1Node.KubeKind,
            Metadata = new() { Name = "unrelated-node" },
        };
        await cluster.Runtime.AddOrUpdateResource(unrelated);
        unrelated.Metadata!.Labels = new Dictionary<string, string> { ["updated"] = "true" };
        await cluster.Runtime.AddOrUpdateResource(unrelated);

        await TestApplicationExtensions.WaitForUiAsync();
        await WaitForNextPollAsync();
        builder.BuildCount.ShouldBe(1);
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

        var filtered = ResourceGraphProjection.ToRootResource(graph, root);

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

        var filtered = ResourceGraphProjection.ToRootResource(graph, root);

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

        var graph = new ResourceRelationshipBuilder().Build([service, pod, siblingPod, root, sibling], new HashSet<string>(), hideNoise: true);
        var delta = new ResourceRelationshipBuilder().BuildAdditionDelta(
            [service, pod, siblingPod, root, sibling],
            new ResourceKey("discovery.k8s.io/v1", V1EndpointSlice.KubeKind, "authentik", "database-root"),
            new HashSet<string>(),
            hideNoise: true);
        delta.Resources.Select(resource => resource.Name()).ShouldBe(["database", "database-0", "database-root"]);
        var filtered = ResourceGraphProjection.ToRootResource(graph, root);

        filtered.Resources.Select(resource => resource.Name()).ShouldBe(["database", "database-0", "database-root"]);
    }

    [AvaloniaFact]
    public async Task resource_type_selection_persists_across_graph_updates_and_selects_new_types()
    {
        var pod = CreatePod("pod");
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
        await viewModel.ApplyGraphAsync(new ResourceRelationshipGraph([pod, service], []));

        viewModel.ResourceTypes.ShouldBe([V1Pod.KubeKind, V1Service.KubeKind]);
        viewModel.SelectedResourceTypes.ShouldBe([V1Pod.KubeKind, V1Service.KubeKind]);

        viewModel.SelectedResourceTypes.Remove(V1Pod.KubeKind);
        await WaitForAsync(() => viewModel.Graph!.Resources.Select(resource => resource.Kind).SequenceEqual([V1Service.KubeKind]));
        viewModel.Graph!.Resources.Select(resource => resource.Kind).ShouldBe([V1Service.KubeKind]);

        await viewModel.ApplyGraphAsync(new ResourceRelationshipGraph([pod, service, deployment], []));

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
        var graph = new ResourceRelationshipBuilder().Build(
            [managed, application, unrelatedManaged],
            new HashSet<string>(),
            hideNoise: true);

        var filtered = ResourceGraphProjection.ToSelectedNamespaces(
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
        var graph = new ResourceRelationshipBuilder().Build(
            [managed, unrelatedClusterResource, application],
            new HashSet<string>(),
            hideNoise: true);

        var filtered = ResourceGraphProjection.ToSelectedNamespaces(
            graph,
            new HashSet<string> { "workload" });

        filtered.Resources.Select(resource => resource.Name()).ShouldBe(["managed", "demo-app"]);
        filtered.Resources.Select(resource => resource.Name()).ShouldNotContain("unrelated-cluster-resource");
    }

    [Fact]
    public void selected_namespace_does_not_expand_through_cluster_scoped_resources()
    {
        V1ConfigMap selected = new()
        {
            ApiVersion = "v1",
            Kind = V1ConfigMap.KubeKind,
            Metadata = new() { Name = "selected", NamespaceProperty = "crossplane-system", Uid = "selected-uid" },
        };
        V1ClusterRole direct = new()
        {
            ApiVersion = "rbac.authorization.k8s.io/v1",
            Kind = V1ClusterRole.KubeKind,
            Metadata = new() { Name = "direct", Uid = "direct-uid" },
        };
        V1ClusterRole unrelated = new()
        {
            ApiVersion = "rbac.authorization.k8s.io/v1",
            Kind = V1ClusterRole.KubeKind,
            Metadata = new() { Name = "unrelated", Uid = "unrelated-uid" },
        };

        ResourceRelationshipGraph graph = new(
            [selected, direct, unrelated],
            [
                new(GetIdentity(selected), GetIdentity(direct), ResourceRelationshipKind.Reference),
                new(GetIdentity(direct), GetIdentity(unrelated), ResourceRelationshipKind.Owner),
            ]);

        var filtered = ResourceGraphProjection.ToSelectedNamespaces(
            graph,
            new HashSet<string> { "crossplane-system" });

        filtered.Resources.Select(resource => resource.Name()).ShouldBe(["selected", "direct"]);
    }

    [AvaloniaFact]
    public void selected_crossplane_namespace_keeps_live_provider_and_function_owner_chains()
    {
        TestDynamicResource provider = new()
        {
            ApiVersion = "pkg.crossplane.io/v1",
            Kind = "Provider",
            Metadata = new() { Name = "provider-databricks", Uid = "provider-databricks-uid" },
        };
        TestDynamicResource providerRevision = new()
        {
            ApiVersion = "pkg.crossplane.io/v1",
            Kind = "ProviderRevision",
            Metadata = new() { Name = "provider-databricks-5bec9d044d7e", Uid = "provider-revision-uid" },
        };
        TestDynamicResource function = new()
        {
            ApiVersion = "pkg.crossplane.io/v1beta1",
            Kind = "Function",
            Metadata = new() { Name = "function-go-templating", Uid = "function-uid" },
        };
        TestDynamicResource functionRevision = new()
        {
            ApiVersion = "pkg.crossplane.io/v1beta1",
            Kind = "FunctionRevision",
            Metadata = new() { Name = "function-go-templating-117c9a95eb57", Uid = "function-revision-uid" },
        };
        var providerDeployment = CreateDeployment("provider-databricks-5bec9d044d7e");
        providerDeployment.Metadata.NamespaceProperty = "crossplane-system";
        var functionDeployment = CreateDeployment("function-go-templating-117c9a95eb57");
        functionDeployment.Metadata.NamespaceProperty = "crossplane-system";

        ResourceRelationshipGraph graph = new(
            [provider, providerRevision, function, functionRevision, providerDeployment, functionDeployment],
            [
                new(GetIdentity(provider), GetIdentity(providerRevision), ResourceRelationshipKind.Owner),
                new(GetIdentity(providerRevision), GetIdentity(providerDeployment), ResourceRelationshipKind.Owner),
                new(GetIdentity(function), GetIdentity(functionRevision), ResourceRelationshipKind.Owner),
                new(GetIdentity(functionRevision), GetIdentity(functionDeployment), ResourceRelationshipKind.Owner),
            ]);

        var filtered = ResourceGraphProjection.ToSelectedNamespaces(
            graph,
            new HashSet<string> { "crossplane-system" });

        filtered.Resources.Select(resource => $"{resource.Kind}/{resource.Name()}").ShouldBe(
        [
            "Provider/provider-databricks",
            "ProviderRevision/provider-databricks-5bec9d044d7e",
            "Function/function-go-templating",
            "FunctionRevision/function-go-templating-117c9a95eb57",
            "Deployment/provider-databricks-5bec9d044d7e",
            "Deployment/function-go-templating-117c9a95eb57",
        ]);
    }

    [Fact]
    public void selected_namespace_keeps_cross_namespace_gateway_parent()
    {
        TestDynamicResource route = new()
        {
            ApiVersion = "gateway.networking.k8s.io/v1",
            Kind = "HTTPRoute",
            Metadata = new() { Name = "frigate", NamespaceProperty = "frigate" },
        };
        TestDynamicResource gateway = new()
        {
            ApiVersion = "gateway.networking.k8s.io/v1",
            Kind = "Gateway",
            Metadata = new() { Name = "public", NamespaceProperty = "envoy-gateway-system" },
        };
        ResourceRelationshipGraph graph = new(
            [route, gateway],
            [new(GetIdentity(route), GetIdentity(gateway), ResourceRelationshipKind.Reference)]);

        var filtered = ResourceGraphProjection.ToSelectedNamespaces(
            graph,
            new HashSet<string> { "frigate" });

        filtered.Resources.Select(resource => resource.Name()).ShouldBe(["frigate", "public"]);
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
        var delta = new ResourceRelationshipBuilder().BuildAdditionDelta(
            [managed, application],
            new ResourceKey("argoproj.io/v1alpha1", "Application", "argocd", "crossplane-providers"),
            new HashSet<string> { "crossplane-system" },
            hideNoise: true);

        var filtered = ResourceGraphProjection.ToSelectedNamespacesIncremental(
            delta,
            new HashSet<string> { "crossplane-system" },
            new HashSet<ResourceIdentity> { new("pkg.crossplane.io/v1", "Provider", "crossplane-system", "provider-databricks", null) });

        filtered.Resources.Select(resource => resource.Name()).ShouldBe(["provider-databricks", "crossplane-providers"]);
        filtered.Relationships.ShouldContain(new ResourceRelationship(
            new("argoproj.io/v1alpha1", "Application", "argocd", "crossplane-providers", null),
            new("pkg.crossplane.io/v1", "Provider", "crossplane-system", "provider-databricks", null),
            ResourceRelationshipKind.GitOps));
    }

    [AvaloniaFact]
    public async Task not_ready_filter_keeps_only_resources_with_false_conditions()
    {
        var ready = CreatePod("ready");
        ready.Status = new() { Conditions = [new() { Type = "Ready", Status = "True" }] };
        var notReady = CreatePod("not-ready");
        notReady.Status = new() { Conditions = [new() { Type = "Ready", Status = "False" }] };
        var unknown = CreatePod("unknown");
        unknown.Status = new() { Conditions = [new() { Type = "Ready", Status = "Unknown" }] };
        var withoutConditions = CreatePod("without-conditions");

        using VisualizationViewModel viewModel = new(new ResourceRelationshipBuilder());
        await viewModel.ApplyGraphAsync(new ResourceRelationshipGraph([ready, notReady, unknown, withoutConditions], []));

        viewModel.ShowNotReadyOnly = true;
        await WaitForAsync(() => viewModel.Graph!.Resources.Select(resource => resource.Name()).SequenceEqual(["not-ready"]));

        viewModel.Graph!.Resources.Select(resource => resource.Name()).ShouldBe(["not-ready"]);
    }

    [AvaloniaFact]
    public async Task type_filter_preserves_pending_references_and_seed_prerequisites()
    {
        var pod = CreatePod("pod");
        UnresolvedResourceReference pending = new("apps", "v1", "Deployment", "default", "owner");
        ResourceSeedPrerequisite prerequisite = new(GroupApiVersionKind.From<V1Deployment>());

        using VisualizationViewModel viewModel = new(new ResourceRelationshipBuilder());
        await viewModel.ApplyGraphAsync(new ResourceRelationshipGraph(
            [pod],
            [],
            new HashSet<UnresolvedResourceReference> { pending },
            new HashSet<ResourceSeedPrerequisite> { prerequisite }));

        viewModel.ShowNotReadyOnly = true;
        await WaitForAsync(() => viewModel.Graph!.PendingReferences.Contains(pending));

        viewModel.Graph!.PendingReferences.ShouldContain(pending);
        viewModel.Graph.RequiredSeedPrerequisites.ShouldContain(prerequisite);
    }

    [AvaloniaFact]
    public async Task resource_type_is_selected_again_after_disappearing_and_reappearing()
    {
        var pod = CreatePod("pod");
        V1Service service = new()
        {
            ApiVersion = "v1",
            Kind = V1Service.KubeKind,
            Metadata = new() { Name = "service", NamespaceProperty = "default", Uid = "service" },
        };

        using VisualizationViewModel viewModel = new(new ResourceRelationshipBuilder());
        await viewModel.ApplyGraphAsync(new ResourceRelationshipGraph([pod, service], []));
        await viewModel.ApplyGraphAsync(new ResourceRelationshipGraph([service], []));
        await viewModel.ApplyGraphAsync(new ResourceRelationshipGraph([pod, service], []));

        viewModel.ResourceTypes.ShouldBe([V1Pod.KubeKind, V1Service.KubeKind]);
        viewModel.SelectedResourceTypes.Order(StringComparer.Ordinal).ShouldBe([V1Pod.KubeKind, V1Service.KubeKind]);
        viewModel.Graph!.Resources.ShouldBe([pod, service]);
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task late_incremental_delta_does_not_overwrite_newer_rebuild(KubernetesBackend backend)
    {
        var cluster = await Application.Current.CreateClusterAsync(config => config.Type = backend);
        await cluster.Runtime.SeedResource<V1Pod>(true);
        var builder = new LateAdditionRelationshipBuilder();
        using VisualizationViewModel viewModel = new(builder);
        cluster.SelectedNamespaces.Clear();
        cluster.SelectedNamespaces.Add(new V1Namespace { Metadata = new() { Name = "default" } });
        viewModel.Initialize(cluster);
        await builder.WaitForInitialBuildAsync(TimeSpan.FromSeconds(30), TestContext.Current.CancellationToken);
        await WaitForAsync(() => viewModel.Graph is not null);

        var pod = CreatePod("late");
        pod.Metadata.Uid = null;
        await cluster.Runtime.AddOrUpdateResource(pod);
        await builder.WaitForAdditionStartedAsync().WaitAsync(TimeSpan.FromSeconds(15), TestContext.Current.CancellationToken);

        viewModel.HideNoise = false;
        await builder.WaitForSecondBuildAsync().WaitAsync(TimeSpan.FromSeconds(15), TestContext.Current.CancellationToken);
        await WaitForAsync(() => viewModel.Graph is { Resources.Count: 0 });

        try
        {
            builder.ReleaseAddition();
            await builder.WaitForAdditionCompletedAsync().WaitAsync(TimeSpan.FromSeconds(15), TestContext.Current.CancellationToken);
            await WaitForAsync(() => viewModel.Graph is { Resources.Count: 0 });
        }
        finally
        {
            builder.ReleaseAddition();
        }

        viewModel.Graph!.Resources.ShouldBeEmpty();
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task modified_resource_rebuilds_changed_relationships_in_visualization_graph(KubernetesBackend backend)
    {
        var cluster = await Application.Current.CreateClusterAsync(
            config => config.Type = backend,
            connect: false);

        await ConnectAndWaitForResourceConfigsAsync(
            cluster,
            GroupApiVersionKind.From<V1Deployment>(),
            GroupApiVersionKind.From<V1Pod>());
        cluster.SelectedNamespaces.Add(new V1Namespace { Metadata = new() { Name = "default" } });
        var builder = new OwnerRelationshipBuilder();
        using VisualizationViewModel viewModel = new(builder);
        await cluster.Runtime.SeedResource<V1Deployment>(true);
        await cluster.Runtime.SeedResource<V1Pod>(true);
        viewModel.Initialize(cluster);

        var firstOwner = CreateDeployment("first-owner");
        var secondOwner = CreateDeployment("second-owner");
        firstOwner.Metadata.Uid = null;
        secondOwner.Metadata.Uid = null;

        await cluster.Runtime.AddOrUpdateResource(firstOwner);
        await cluster.Runtime.AddOrUpdateResource(secondOwner);
        var pod = CreatePodWithOwner("owned-pod", firstOwner);
        pod.Metadata.Uid = null;
        await cluster.Runtime.AddOrUpdateResource(pod);
        await TestApplicationExtensions.WaitForUiAsync();

        var firstOwnerIdentity = GetIdentity(firstOwner);
        var secondOwnerIdentity = GetIdentity(secondOwner);
        var podIdentity = GetIdentity(pod);
        ResourceRelationship initialRelationship = new(firstOwnerIdentity, podIdentity, ResourceRelationshipKind.Owner);
        ResourceRelationship changedRelationship = new(secondOwnerIdentity, podIdentity, ResourceRelationshipKind.Owner);
        await viewModel.ApplyGraphAsync(new ResourceRelationshipGraph([firstOwner, secondOwner, pod], [initialRelationship]));
        await WaitForAsync(() => !viewModel.IsRebuildPendingOrRunning);
        var fullBuildCount = builder.BuildCount;

        await TestWait.UntilAsync(
            () => cluster.Runtime.GetResource<V1Pod>("default", "owned-pod") is not null,
            TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken);
        pod = cluster.Runtime.GetResource<V1Pod>("default", "owned-pod").ShouldNotBeNull();
        await cluster.Runtime.Client!.GetGenericClient<V1Pod>().PatchNamespacedAsync<V1Pod>(
            new V1Patch(
                KubernetesJson.Serialize(new
                {
                    metadata = new
                    {
                        ownerReferences = new[]
                        {
                            new
                            {
                                apiVersion = secondOwner.ApiVersion,
                                kind = secondOwner.Kind,
                                name = secondOwner.Name(),
                                uid = secondOwner.Uid(),
                            },
                        },
                    },
                }),
                V1Patch.PatchType.MergePatch),
            "default",
            pod.Name());

        var timeout = Stopwatch.StartNew();
        while (timeout.Elapsed < TimeSpan.FromSeconds(5)
            && !viewModel.Graph!.Relationships.Contains(changedRelationship))
        {
            await TestApplicationExtensions.WaitForUiAsync();
            await WaitForNextPollAsync();
        }

        viewModel.Graph!.Relationships.ShouldContain(changedRelationship);
        viewModel.Graph.Relationships.ShouldNotContain(initialRelationship);
        builder.BuildCount.ShouldBe(fullBuildCount);
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
            Labels = new Dictionary<string, string> { ["app"] = name },
        },
        Spec = new()
        {
            Selector = new V1LabelSelector { MatchLabels = new Dictionary<string, string> { ["app"] = name } },
            Template = new V1PodTemplateSpec
            {
                Metadata = new V1ObjectMeta { Labels = new Dictionary<string, string> { ["app"] = name } },
                Spec = new V1PodSpec { Containers = [new V1Container { Name = "app", Image = "example/app:1" }] },
            },
        },
    };

    private sealed class ThrowingRelationshipBuilder : IResourceRelationshipBuilder
    {
        public ResourceRelationshipGraph Build(
            IEnumerable<IKubernetesObject<V1ObjectMeta>> resources,
            IReadOnlySet<string> selectedNamespaces,
            bool hideNoise)
            => throw new InvalidOperationException("relationship build failed");

        public ResourceRelationshipGraph BuildAdditionDelta(
            IEnumerable<IKubernetesObject<V1ObjectMeta>> resources,
            ResourceKey addedResource,
            IReadOnlySet<string> selectedNamespaces,
            bool hideNoise)
            => throw new InvalidOperationException("relationship build failed");
    }

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
        Spec = new V1PodSpec { Containers = [new V1Container { Name = "app", Image = "example/app:1" }] },
    };

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task processed_resource_config_starts_required_seed_without_waiting_for_ready(KubernetesBackend backend)
    {
        var cluster = await Application.Current.CreateClusterAsync(
            config => config.Type = backend,
            connect: false);
        using VisualizationViewModel viewModel = new(new ResourceRelationshipBuilder());

        viewModel.Initialize(cluster);

        await cluster.Connect();
        cluster.Runtime.GetResourceSourceCache<Corev1Event>().ShouldNotBeNull();
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task applying_graph_starts_provider_prerequisite_seed(KubernetesBackend backend)
    {
        var cluster = await Application.Current.CreateClusterAsync(
            config => config.Type = backend,
            connect: false);
        await cluster.Connect();
        cluster.SelectedNamespaces.Add(new V1Namespace { Metadata = new V1ObjectMeta { Name = "default" } });

        var seedRequested = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        void OnResourceSeeded(IClusterRuntime _, GroupApiVersionKind kind)
        {
            if (kind == GroupApiVersionKind.From<V1Node>())
            {
                seedRequested.TrySetResult(false);
            }
        }

        cluster.Runtime.ResourceSeeded += OnResourceSeeded;
        try
        {
            using VisualizationViewModel viewModel = new(new ResourceRelationshipBuilder());
            viewModel.Initialize(cluster);
            await viewModel.ApplyGraphAsync(new ResourceRelationshipGraph(
                [],
                [],
                SeedPrerequisites: new HashSet<ResourceSeedPrerequisite>
                {
                    new(GroupApiVersionKind.From<V1Node>()),
                }));

            (await seedRequested.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken)).ShouldBeFalse();
        }
        finally
        {
            cluster.Runtime.ResourceSeeded -= OnResourceSeeded;
        }
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task added_resource_starts_owner_reference_seed_without_waiting_for_ready(KubernetesBackend backend)
    {
        var cluster = await Application.Current.CreateClusterAsync(
            config => config.Type = backend,
            connect: false);
        var seedRequested = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        void OnResourceSeeded(IClusterRuntime _, GroupApiVersionKind kind)
        {
            if (kind == GroupApiVersionKind.From<V1CustomResourceDefinition>())
            {
                seedRequested.TrySetResult(true);
            }
        }

        cluster.Runtime.ResourceSeeded += OnResourceSeeded;
        using VisualizationViewModel viewModel = new(new ResourceRelationshipBuilder());
        try
        {
            await cluster.Connect();
            viewModel.Initialize(cluster);
            await cluster.Runtime.AddOrUpdateResource(new V1Pod
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
            cluster.Runtime.ResourceSeeded -= OnResourceSeeded;
        }
    }

    [AvaloniaFact]
    public async Task owner_reference_to_unconfigured_custom_kind_does_not_stop_visualization_updates()
    {
        var cluster = await Application.Current.CreateClusterAsync(config => config.Type = KubernetesBackend.Fake);
        cluster.SelectedNamespaces.Add(new V1Namespace { Metadata = new() { Name = "default" } });
        var ownerKind = new GroupApiVersionKind(
            "apiextensions.crossplane.io",
            "v1",
            "CompositeResourceDefinition",
            "compositeresourcedefinitions");
        cluster.Runtime.ModelCatalog.RegisterCustomResourceDefinition(ownerKind);

        using VisualizationViewModel viewModel = new(new ResourceRelationshipBuilder());
        viewModel.Initialize(cluster);
        var pod = new V1Pod
        {
            ApiVersion = "v1",
            Kind = V1Pod.KubeKind,
            Metadata = new()
            {
                Name = "owned-by-unconfigured-kind",
                NamespaceProperty = "default",
                OwnerReferences =
                [
                    new()
                    {
                        ApiVersion = "apiextensions.crossplane.io/v1",
                        Kind = "CompositeResourceDefinition",
                        Name = "owner",
                        Uid = "owner",
                    },
                ],
            },
            Spec = new() { Containers = [new() { Name = "owned", Image = "busybox" }] },
        };
        Should.NotThrow(() => viewModel.SeedOwnerReferenceResourceKinds(pod));
        await cluster.Runtime.AddOrUpdateResource(pod);

        await WaitForAsync(
            () => viewModel.Graph?.Resources.Any(resource => resource.Name() == "owned-by-unconfigured-kind") == true,
            timeoutMs: 5000,
            cancellationToken: TestContext.Current.CancellationToken);
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task added_unrelated_cluster_scoped_resource_does_not_bypass_namespace_filter(KubernetesBackend backend)
    {
        var cluster = await Application.Current.CreateClusterAsync(config => config.Type = backend);
        cluster.SelectedNamespaces.Add(new V1Namespace { Metadata = new() { Name = "default" } });
        var builder = new AdditionCaptureRelationshipBuilder();
        using VisualizationViewModel viewModel = new(builder);
        await cluster.Runtime.SeedResource<V1Node>(true);
        viewModel.Initialize(cluster);
        await TestApplicationExtensions.WaitForUiAsync();
        await builder.WaitForInitialBuildAsync();
        var initialGraphDeadline = DateTime.UtcNow.AddSeconds(5);
        while (!viewModel.Graph!.Resources.Any(resource => resource.Name() == "selected"))
        {
            await TestApplicationExtensions.WaitForUiAsync();
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
            Metadata = new() { Name = "unrelated-node" },
        });

        await TestApplicationExtensions.WaitForUiAsync();
        builder.AdditionStarted.ShouldBeFalse();
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task changing_selected_namespaces_rebuilds_graph_with_new_namespace_filter(KubernetesBackend backend)
    {
        var cluster = await Application.Current.CreateClusterAsync(config => config.Type = backend);
        cluster.SelectedNamespaces.Add(new V1Namespace { Metadata = new() { Name = "default" } });
        await cluster.Runtime.AddOrUpdateResource(new V1Namespace { Metadata = new() { Name = "other" } });
        var builder = new BuildCaptureRelationshipBuilder();
        using VisualizationViewModel viewModel = new(builder);

        viewModel.Initialize(cluster);
        (await builder.WaitForBuildAsync(1)).SelectedNamespaces.ShouldBe(["default"]);
        builder.BuildOnUiThread.ShouldBeFalse();

        cluster.SelectedNamespaces.Clear();
        cluster.SelectedNamespaces.Add(cluster.Runtime.Namespaces.Single(item => item.Name() == "other"));
        await TestApplicationExtensions.WaitForUiAsync();

        (await builder.WaitForBuildAsync(input => input.SelectedNamespaces.SequenceEqual(["other"])))
            .SelectedNamespaces.ShouldBe(["other"]);
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task full_graph_rebuild_does_not_reintroduce_unselected_namespaced_resources(KubernetesBackend backend)
    {
        var cluster = await Application.Current.CreateClusterAsync(config => config.Type = backend);
        cluster.SelectedNamespaces.Add(new V1Namespace { Metadata = new() { Name = "default" } });
        var builder = new NamespaceLeakingBuildRelationshipBuilder();
        using VisualizationViewModel viewModel = new(builder);

        viewModel.Initialize(cluster);
        await builder.WaitForBuildAsync();
        var deadline = DateTime.UtcNow.AddSeconds(5);
        while (viewModel.Graph == null || viewModel.Graph.Resources.Count == 0)
        {
            await TestApplicationExtensions.WaitForUiAsync();
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
        var cluster = await Application.Current.CreateClusterAsync(config => config.Type = backend);

        cluster.SelectedNamespaces.Add(new V1Namespace { Metadata = new() { Name = "default" } });
        var builder = new BuildCaptureRelationshipBuilder();
        using VisualizationViewModel viewModel = new(builder);

        viewModel.Initialize(cluster);
        (await builder.WaitForBuildAsync(1)).HideNoise.ShouldBeTrue();

        viewModel.HideNoise = false;
        await TestApplicationExtensions.WaitForUiAsync();

        (await builder.WaitForBuildAsync(2)).HideNoise.ShouldBeFalse();
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task added_unrelated_namespaced_resource_does_not_bypass_namespace_filter(KubernetesBackend backend)
    {
        var cluster = await Application.Current.CreateClusterAsync(config => config.Type = backend);
        cluster.SelectedNamespaces.Add(new V1Namespace { Metadata = new() { Name = "default" } });
        var builder = new AdditionCaptureRelationshipBuilder();
        using VisualizationViewModel viewModel = new(builder);
        await cluster.Runtime.SeedResource<V1Pod>(true);
        viewModel.Initialize(cluster);
        await builder.WaitForInitialBuildAsync();

        await cluster.Runtime.AddOrUpdateResource(new V1Namespace
        {
            Metadata = new() { Name = "other" },
        });

        await cluster.Runtime.AddOrUpdateResource(new V1Pod
        {
            ApiVersion = "v1",
            Kind = V1Pod.KubeKind,
            Metadata = new() { Name = "unrelated", NamespaceProperty = "other" },
            Spec = new()
            {
                Containers =
                [
                    new() { Name = "unrelated", Image = "busybox" },
                ],
            },
        });

        await TestApplicationExtensions.WaitForUiAsync();
        viewModel.Graph!.Resources.ShouldNotContain(resource => resource.Name() == "unrelated");
    }

    [AvaloniaFact]
    public async Task noise_resource_change_does_not_discard_pending_incremental_resource()
    {
        var cluster = await Application.Current.CreateClusterAsync(config => config.Type = KubernetesBackend.Fake);
        cluster.SelectedNamespaces.Add(new V1Namespace { Metadata = new() { Name = "default" } });
        await cluster.Runtime.SeedResource<V1Pod>(true);
        await cluster.Runtime.SeedResource<V1ReplicaSet>(true);

        var builder = new LateAdditionRelationshipBuilder();
        using VisualizationViewModel viewModel = new(builder);
        viewModel.Initialize(cluster);
        await builder.WaitForInitialBuildAsync();

        await cluster.Runtime.AddOrUpdateResource(CreatePod("pending"));
        await builder.WaitForAdditionStartedAsync();

        V1ReplicaSet noise = new()
        {
            ApiVersion = "apps/v1",
            Kind = V1ReplicaSet.KubeKind,
            Metadata = new() { Name = "noise", NamespaceProperty = "default" },
            Status = new() { Replicas = 0 },
        };
        await cluster.Runtime.AddOrUpdateResource(noise);
        await cluster.Runtime.DeleteAsync(noise, TestContext.Current.CancellationToken);
        await TestWait.UntilAsync(
            () => cluster.Runtime.GetResource<V1ReplicaSet>("default", "noise") is null,
            TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken);

        try
        {
            builder.ReleaseAddition();
            await builder.WaitForAdditionCompletedAsync();
            await WaitForAsync(
                () => viewModel.Graph!.Resources.Any(resource => resource.Name() == "pending"),
                cancellationToken: TestContext.Current.CancellationToken);
        }
        finally
        {
            builder.ReleaseAddition();
        }
    }

    [AvaloniaFact]
    public async Task repeated_incremental_deltas_do_not_accumulate_resources_from_unselected_namespaces()
    {
        var cluster = await Application.Current.CreateClusterAsync(
            config => config.Type = KubernetesBackend.Fake,
            connect: false);

        await ConnectAndWaitForResourceConfigsAsync(cluster, GroupApiVersionKind.From<V1Pod>());
        var selected = CreatePod("selected");
        selected.Metadata.Uid = null;
        await cluster.Runtime.AddOrUpdateResource(new V1Namespace { Metadata = new() { Name = "other" } });
        V1Pod unrelated = new()
        {
            ApiVersion = "v1",
            Kind = V1Pod.KubeKind,
            Metadata = new() { Name = "unrelated", NamespaceProperty = "other" },
            Spec = new() { Containers = [new V1Container { Name = "app", Image = "example/app:1" }] },
        };
        await cluster.Runtime.AddOrUpdateResource(selected);
        await cluster.Runtime.AddOrUpdateResource(unrelated);
        await cluster.Runtime.AddOrUpdateResource(new V1Node
        {
            ApiVersion = "v1",
            Kind = V1Node.KubeKind,
            Metadata = new() { Name = "seed-node" },
        });
        await cluster.Runtime.SeedResource<V1Pod>(true);

        var builder = new LeakyAdditionRelationshipBuilder();
        using VisualizationViewModel viewModel = new(builder);
        viewModel.Initialize(cluster);
        cluster.SelectedNamespaces.Add(cluster.Runtime.Namespaces.Single(namespaceResource => namespaceResource.Name() == "default"));
        await builder.WaitForInitialBuildAsync();
        await TestWait.UntilAsync(
            () => viewModel.Graph?.Resources.Any(resource => resource.Name() == "selected") == true,
            timeoutMs: 5_000,
            cancellationToken: TestContext.Current.CancellationToken);

        await TestWait.UntilAsync(
            () => !viewModel.IsRebuildPendingOrRunning,
            TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken);

        viewModel.Graph!.Resources.Select(resource => resource.Name()).ShouldNotContain("unrelated");

        for (var i = 0; i < 3; i++)
        {
            await TestWait.UntilAsync(
                () => !viewModel.IsRebuildPendingOrRunning,
                TimeSpan.FromSeconds(5),
                cancellationToken: TestContext.Current.CancellationToken);

            var incremental = CreatePod($"incremental-{i}");
            incremental.Metadata.Uid = null;
            await cluster.Runtime.AddOrUpdateResource(incremental);
            await TestWait.UntilAsync(
                () => cluster.Runtime.GetResource<V1Pod>("default", incremental.Name()) is not null,
                TimeSpan.FromSeconds(5),
                cancellationToken: TestContext.Current.CancellationToken);
            // The informer may coalesce this change with a rebuild or deliver it while
            // another graph application is pending. Wait for the observable graph state
            // instead of requiring one particular internal callback ordering.
            await TestWait.UntilAsync(
                () => viewModel.Graph?.Resources.Any(resource => resource.Name() == incremental.Name()) == true,
                timeoutMs: 30_000,
                cancellationToken: TestContext.Current.CancellationToken);
        }

        viewModel.Graph!.Resources.Select(resource => resource.Name()).ShouldNotContain("unrelated");
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task seeded_prerequisite_triggers_graph_rebuild(KubernetesBackend backend)
    {
        var cluster = await Application.Current.CreateClusterAsync(config => config.Type = backend);
        await WaitForAsync(() =>
        {
            var deploymentConfig = cluster.GetResourceConfig(GroupApiVersionKind.From<V1Deployment>());
            return deploymentConfig.PermissionsLoaded && deploymentConfig.CanListAndWatch;
        });

        var builder = new BuildCaptureRelationshipBuilder();
        using VisualizationViewModel viewModel = new(builder);
        cluster.SelectedNamespaces.Add(new V1Namespace { Metadata = new() { Name = "default" } });
        viewModel.Initialize(cluster);
        await builder.WaitForBuildAsync(1);

        ResourceSeedPrerequisite prerequisite = new(GroupApiVersionKind.From<V1Deployment>());
        await viewModel.ApplyGraphAsync(new ResourceRelationshipGraph([], [], SeedPrerequisites: new HashSet<ResourceSeedPrerequisite> { prerequisite }));
        await cluster.Runtime.SeedResource(GroupApiVersionKind.From<V1Deployment>());
        await builder.WaitForBuildAsync(2);
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task seeded_pending_reference_triggers_graph_rebuild(KubernetesBackend backend)
    {
        var cluster = await Application.Current.CreateClusterAsync(config => config.Type = backend);
        await WaitForAsync(() =>
        {
            var deploymentConfig = cluster.GetResourceConfig(GroupApiVersionKind.From<V1Deployment>());
            return deploymentConfig.PermissionsLoaded && deploymentConfig.CanListAndWatch;
        });

        var builder = new BuildCaptureRelationshipBuilder();
        using VisualizationViewModel viewModel = new(builder);
        cluster.SelectedNamespaces.Add(new V1Namespace { Metadata = new() { Name = "default" } });
        viewModel.Initialize(cluster);
        await builder.WaitForBuildAsync(1);

        var pending = new UnresolvedResourceReference("apps", "v1", V1Deployment.KubeKind, "default", "pending");
        await viewModel.ApplyGraphAsync(new ResourceRelationshipGraph([], [], new HashSet<UnresolvedResourceReference> { pending }));
        await cluster.Runtime.SeedResource(GroupApiVersionKind.From<V1Deployment>());
        await builder.WaitForBuildAsync(2);
    }

    private sealed class LeakyAdditionRelationshipBuilder : IResourceRelationshipBuilder
    {
        private readonly TaskCompletionSource _initialBuild = new(TaskCreationOptions.RunContinuationsAsynchronously);

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
            return new ResourceRelationshipGraph(resources.ToArray(), []);
        }

        public async Task WaitForInitialBuildAsync() => await WaitForSignalAsync(_initialBuild.Task, "initial visualization build");
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task disposed_view_model_unsubscribes_from_namespace_changes(KubernetesBackend backend)
    {
        var cluster = await Application.Current.CreateClusterAsync(config => config.Type = backend);

        cluster.SelectedNamespaces.Add(new V1Namespace { Metadata = new() { Name = "default" } });
        var builder = new BuildCaptureRelationshipBuilder();
        using VisualizationViewModel viewModel = new(builder);
        viewModel.Initialize(cluster);
        await builder.WaitForBuildAsync(1);
        await TestWait.UntilAsync(
            () => !viewModel.IsRebuildPendingOrRunning,
            TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken);

        viewModel.Dispose();
        cluster.SelectedNamespaces.Add(new V1Namespace { Metadata = new() { Name = "other" } });
        await TestApplicationExtensions.WaitForUiAsync();

        builder.BuildCount.ShouldBe(1);
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task background_resource_changes_are_processed_on_the_ui_thread(KubernetesBackend backend)
    {
        var cluster = await Application.Current.CreateClusterAsync(
            config => config.Type = backend,
            connect: false);

        await ConnectAndWaitForResourceConfigsAsync(cluster, GroupApiVersionKind.From<V1Pod>());
        cluster.SelectedNamespaces.Add(new V1Namespace { Metadata = new() { Name = "default" } });
        var builder = new BuildCaptureRelationshipBuilder();
        using VisualizationViewModel viewModel = new(builder);
        await cluster.Runtime.SeedResource<V1Pod>(true);
        viewModel.Initialize(cluster);
        await builder.WaitForBuildAsync(1);

        var background = CreatePod("background");
        background.Metadata.Uid = null;
        await cluster.Runtime.AddOrUpdateResource(background);
        await TestWait.UntilAsync(
            () => cluster.Runtime.GetResource<V1Pod>("default", "background") is not null,
            TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken);
        await Task.Run(
            () => cluster.Runtime.Client!.GetGenericClient<V1Pod>().PatchNamespacedAsync<V1Pod>(
                new V1Patch(
                    KubernetesJson.Serialize(new
                    {
                        metadata = new
                        {
                            labels = new Dictionary<string, string> { ["updated"] = "true" },
                        },
                    }),
                    V1Patch.PatchType.MergePatch),
                "default",
                "background",
                TestContext.Current.CancellationToken),
            TestContext.Current.CancellationToken);

        var fullBuildCount = builder.BuildCount;
        await builder.WaitForAdditionAsync();

        builder.BuildCount.ShouldBe(fullBuildCount);
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task disposed_view_model_ignores_runtime_resource_changes(KubernetesBackend backend)
    {
        var cluster = await Application.Current.CreateClusterAsync(
            config => config.Type = backend,
            connect: false);
        await cluster.Connect();
        cluster.SelectedNamespaces.Add(new V1Namespace { Metadata = new() { Name = "default" } });
        var builder = new BuildCaptureRelationshipBuilder();
        using VisualizationViewModel viewModel = new(builder);
        viewModel.Initialize(cluster);
        await builder.WaitForBuildAsync(1);
        await TestWait.UntilAsync(
            () => !viewModel.IsRebuildPendingOrRunning,
            TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken);

        var buildCountBeforeDispose = builder.BuildCount;
        viewModel.Dispose();
        var afterDispose = CreatePod("after-dispose");
        afterDispose.Metadata.Uid = null;
        await cluster.Runtime.AddOrUpdateResource(afterDispose);
        await TestApplicationExtensions.WaitForUiAsync();

        builder.BuildCount.ShouldBe(buildCountBeforeDispose);
    }

    [AvaloniaFact]
    public async Task graph_skips_relationships_with_missing_vertices()
    {
        var source = CreatePod("source");
        var sourceIdentity = GetIdentity(source);
        ResourceIdentity missingIdentity = new(V1Pod.KubeApiVersion, V1Pod.KubeKind, "default", "missing", "missing");

        using ResourceGraphControl control = new(Application.Current.GetRequiredTestService<IResourceIconService>())
        {
            Graph = new ResourceRelationshipGraph(
                [source],
                [new ResourceRelationship(sourceIdentity, missingIdentity, ResourceRelationshipKind.Reference)]),
        };

        try
        {
            await WaitForAsync(() => control.Area.LogicCore?.Graph?.VertexCount == 1, cancellationToken: TestContext.Current.CancellationToken);

            control.Area.LogicCore!.Graph.VertexCount.ShouldBe(1);
            control.Area.LogicCore.Graph.EdgeCount.ShouldBe(0);
        }
        finally
        {
            Dispatcher.UIThread.Post(control.Dispose);
            await TestApplicationExtensions.WaitForUiAsync();
        }
    }

    [Fact]
    public void graph_omits_transitive_owner_edge()
    {
        ResourceIdentity revision = new("pkg.crossplane.io/v1", "FunctionRevision", "", "revision", "revision");
        ResourceIdentity deployment = new("apps/v1", V1Deployment.KubeKind, "crossplane-system", "deployment", "deployment");
        ResourceIdentity replicaSet = new("apps/v1", V1ReplicaSet.KubeKind, "crossplane-system", "replicaset", "replicaset");
        ResourceIdentity pod = new("v1", V1Pod.KubeKind, "crossplane-system", "pod", "pod");

        var filtered = ResourceGraphControl.RemoveTransitiveOwnerRelationships(
        [
            new(revision, deployment, ResourceRelationshipKind.Owner),
            new(deployment, replicaSet, ResourceRelationshipKind.Owner),
            new(replicaSet, pod, ResourceRelationshipKind.Owner),
            new(revision, pod, ResourceRelationshipKind.Owner),
        ]);

        filtered.ShouldBe(
        [
            new(revision, deployment, ResourceRelationshipKind.Owner),
            new(deployment, replicaSet, ResourceRelationshipKind.Owner),
            new(replicaSet, pod, ResourceRelationshipKind.Owner),
        ]);
    }

    [Fact]
    public void graph_transitive_owner_filter_skips_duplicate_descendant_visits()
    {
        ResourceIdentity owner = new("v1", "Pod", "default", "owner", "owner");
        ResourceIdentity child = new("v1", "Pod", "default", "child", "child");
        ResourceIdentity leaf = new("v1", "Pod", "default", "leaf", "leaf");

        var filtered = ResourceGraphControl.RemoveTransitiveOwnerRelationships(
        [
            new(owner, child, ResourceRelationshipKind.Owner),
            new(owner, child, ResourceRelationshipKind.Owner),
            new(child, leaf, ResourceRelationshipKind.Owner),
        ]);

        filtered.ShouldContain(new ResourceRelationship(owner, child, ResourceRelationshipKind.Owner));
        filtered.ShouldContain(new ResourceRelationship(child, leaf, ResourceRelationshipKind.Owner));
    }

    [AvaloniaFact]
    public async Task incremental_graph_update_omits_transitive_owner_edge()
    {
        V1Pod revision = new()
        {
            ApiVersion = "pkg.crossplane.io/v1",
            Kind = "FunctionRevision",
            Metadata = new() { Name = "revision", NamespaceProperty = "crossplane-system", Uid = "revision" },
        };
        V1Pod deployment = new()
        {
            ApiVersion = "apps/v1",
            Kind = V1Deployment.KubeKind,
            Metadata = new() { Name = "deployment", NamespaceProperty = "crossplane-system", Uid = "deployment" },
        };
        V1Pod replicaSet = new()
        {
            ApiVersion = "apps/v1",
            Kind = V1ReplicaSet.KubeKind,
            Metadata = new() { Name = "replicaset", NamespaceProperty = "crossplane-system", Uid = "replicaset" },
        };
        V1Pod pod = new()
        {
            ApiVersion = "v1",
            Kind = V1Pod.KubeKind,
            Metadata = new() { Name = "pod", NamespaceProperty = "crossplane-system", Uid = "pod" },
        };
        V1Pod newlyAddedPod = new()
        {
            ApiVersion = "v1",
            Kind = V1Pod.KubeKind,
            Metadata = new() { Name = "newly-added-pod", NamespaceProperty = "crossplane-system", Uid = "newly-added-pod" },
        };

        var revisionIdentity = GetIdentity(revision);
        var deploymentIdentity = GetIdentity(deployment);
        var replicaSetIdentity = GetIdentity(replicaSet);
        var podIdentity = GetIdentity(pod);
        ResourceRelationship[] ownershipChain =
        [
            new(revisionIdentity, deploymentIdentity, ResourceRelationshipKind.Owner),
            new(deploymentIdentity, replicaSetIdentity, ResourceRelationshipKind.Owner),
            new(replicaSetIdentity, podIdentity, ResourceRelationshipKind.Owner),
        ];

        using ResourceGraphControl control = new(Application.Current.GetRequiredTestService<IResourceIconService>());
        Window window = new() { Width = 800, Height = 600, Content = control };
        try
        {
            control.Graph = new ResourceRelationshipGraph([revision, deployment, replicaSet, pod], ownershipChain);
            window.Show();
            await WaitForAsync(() => control.Area.LogicCore?.Graph?.EdgeCount == ownershipChain.Length, cancellationToken: TestContext.Current.CancellationToken);

            control.Graph = new ResourceRelationshipGraph(
                [revision, deployment, replicaSet, pod, newlyAddedPod],
                [.. ownershipChain, new(revisionIdentity, podIdentity, ResourceRelationshipKind.Owner)]);

            await WaitForAsync(() => control.Area.LogicCore?.Graph?.VertexCount == 5, cancellationToken: TestContext.Current.CancellationToken);

            control.Area.LogicCore!.Graph.Edges
                .Select(edge => edge.Relationship)
                .ShouldBe(ownershipChain);
        }
        finally
        {
            window.Close();
            await TestApplicationExtensions.WaitForUiAsync();
        }
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

            var vertices = control.Area.VertexList.Values.ToArray();
            var bounds = vertices
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
        private int _buildCount;

        public int BuildCount => Volatile.Read(ref _buildCount);

        public ResourceRelationshipGraph Build(
            IEnumerable<IKubernetesObject<V1ObjectMeta>> resources,
            IReadOnlySet<string> selectedNamespaces,
            bool hideNoise)
        {
            Interlocked.Increment(ref _buildCount);
            var resourceArray = resources.ToArray();
            var resourcesByUid = resourceArray
                .Where(resource => resource.Uid() is not null)
                .ToDictionary(resource => resource.Uid()!, StringComparer.Ordinal);
            List<ResourceRelationship> relationships = [];
            foreach (var resource in resourceArray)
            {
                foreach (var owner in resource.Metadata?.OwnerReferences ?? [])
                {
                    if (owner.Uid is not null && resourcesByUid.TryGetValue(owner.Uid, out var ownerResource))
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
        {
            var resourceArray = resources.ToArray();
            var resource = resourceArray.FirstOrDefault(item =>
                new ResourceKey(item.ApiVersion ?? string.Empty, item.Kind ?? string.Empty, item.Namespace(), item.Name() ?? string.Empty) == addedResource);
            if (resource is null)
            {
                return ResourceRelationshipGraph.Empty;
            }

            var identity = GetIdentity(resource);
            var resourcesByUid = resourceArray
                .Where(item => item.Uid() is not null)
                .ToDictionary(item => item.Uid()!, StringComparer.Ordinal);
            var owners = (resource.Metadata?.OwnerReferences ?? [])
                .Where(owner => owner.Uid is not null && resourcesByUid.ContainsKey(owner.Uid))
                .Select(owner => new ResourceRelationship(GetIdentity(resourcesByUid[owner.Uid!]), identity, ResourceRelationshipKind.Owner))
                .ToArray();
            var children = resourceArray
                .Where(item => item.Metadata?.OwnerReferences?.Any(owner => owner.Uid == resource.Uid()) == true)
                .Select(item => new ResourceRelationship(identity, GetIdentity(item), ResourceRelationshipKind.Owner));
            var related = owners.Concat(children).ToArray();
            var relatedIdentities = related
                .SelectMany(relationship => new[] { relationship.Source, relationship.Target })
                .ToHashSet();
            relatedIdentities.Add(identity);
            return new ResourceRelationshipGraph(
                resourceArray.Where(item => relatedIdentities.Contains(GetIdentity(item))).ToArray(),
                related);
        }
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
            var deadline = DateTime.UtcNow.AddSeconds(5);
            while (!_initialBuild.Task.IsCompleted)
            {
                await TestApplicationExtensions.WaitForUiAsync();
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

        public bool AdditionStarted => _addition.Task.IsCompleted;

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
            var graph = _inner.BuildAdditionDelta(resources, addedResource, selectedNamespaces, hideNoise);
            _addition.TrySetResult(graph);
            return graph;
        }

        public async Task WaitForInitialBuildAsync()
        {
            var deadline = DateTime.UtcNow.AddSeconds(5);
            while (!_initialBuild.Task.IsCompleted)
            {
                await TestApplicationExtensions.WaitForUiAsync();
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
            var resource = resources.SingleOrDefault(item => item.Name() == addedResource.Name);
            return resource == null
                ? ResourceRelationshipGraph.Empty
                : new ResourceRelationshipGraph([resource], []);
        }

        public async Task WaitForInitialBuildAsync(TimeSpan timeout = default, CancellationToken cancellationToken = default)
        {
            var deadline = DateTime.UtcNow + (timeout == default ? TimeSpan.FromSeconds(5) : timeout);
            while (!_initialBuild.Task.IsCompleted)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await TestApplicationExtensions.WaitForUiAsync();
                if (DateTime.UtcNow >= deadline)
                {
                    throw new TimeoutException("Timed out waiting for the initial visualization build.");
                }

                await WaitForNextPollAsync(cancellationToken);
            }
        }

        public async Task WaitForSecondBuildAsync()
        {
            await WaitForSignalAsync(_secondBuild.Task, "second visualization build");
        }

        public async Task WaitForAdditionStartedAsync() => await WaitForSignalAsync(_additionStarted.Task, "incremental addition");

        public async Task WaitForAdditionCompletedAsync() => await WaitForSignalAsync(_additionCompleted.Task, "incremental addition completion");

        public void ReleaseAddition() => _releaseAddition.TrySetResult();

    }

    private sealed class BuildCaptureRelationshipBuilder : IResourceRelationshipBuilder
    {
        private readonly ConcurrentDictionary<int, TaskCompletionSource<BuildInput>> _builds = [];
        private readonly TaskCompletionSource _addition = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private int _buildCount;
        private int _buildOnUiThread;

        public int BuildCount => Volatile.Read(ref _buildCount);
        public bool BuildOnUiThread => Volatile.Read(ref _buildOnUiThread) != 0;

        public ResourceRelationshipGraph Build(
            IEnumerable<IKubernetesObject<V1ObjectMeta>> resources,
            IReadOnlySet<string> selectedNamespaces,
            bool hideNoise)
        {
            if (Dispatcher.UIThread.CheckAccess())
            {
                Interlocked.Exchange(ref _buildOnUiThread, 1);
            }

            var buildNumber = Interlocked.Increment(ref _buildCount);
            _builds.GetOrAdd(buildNumber, _ => new(TaskCreationOptions.RunContinuationsAsynchronously))
                .TrySetResult(new(selectedNamespaces.Order(StringComparer.Ordinal).ToArray(), hideNoise));
            return ResourceRelationshipGraph.Empty;
        }

        public ResourceRelationshipGraph BuildAdditionDelta(
            IEnumerable<IKubernetesObject<V1ObjectMeta>> resources,
            ResourceKey addedResource,
            IReadOnlySet<string> selectedNamespaces,
            bool hideNoise)
        {
            _addition.TrySetResult();
            return ResourceRelationshipGraph.Empty;
        }

        public async Task WaitForAdditionAsync()
            => await _addition.Task.WaitAsync(TestContext.Current.CancellationToken);

        public async Task<BuildInput> WaitForBuildAsync(int buildNumber)
        {
            var build = _builds.GetOrAdd(buildNumber, _ => new(TaskCreationOptions.RunContinuationsAsynchronously)).Task;
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
        Spec = new()
        {
            Containers =
            [
                new V1Container
                {
                    Name = "app",
                    Image = "example.com/app:1",
                },
            ],
        },
    };

    private sealed class TestDynamicResource : IKubernetesObject<V1ObjectMeta>
    {
        public string? ApiVersion { get; set; }
        public string? Kind { get; set; }
        public V1ObjectMeta Metadata { get; set; } = new();
    }
}
