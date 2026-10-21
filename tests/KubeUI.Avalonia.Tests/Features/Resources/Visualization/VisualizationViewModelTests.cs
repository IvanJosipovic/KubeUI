using Avalonia.Headless.XUnit;
using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Features.Resources.Visualization;
using KubeUI.Kubernetes.Resources.Relationships;
using Moq;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.Resources.Visualization;

public sealed class VisualizationViewModelTests
{
    private static VisualizationScope ReleaseScope(string name, string namespaceName)
        => new(namespaceName, $"Helm release: {name} ({namespaceName})",
            new ResourceAnnotationSelector(new Dictionary<string, string>
            {
                ["meta.helm.sh/release-name"] = name,
                ["meta.helm.sh/release-namespace"] = namespaceName
            }), IsNamespaceReadOnly: true);

    [AvaloniaFact]
    public async Task annotation_scope_ordinary_additions_and_updates_use_shared_incremental_pipeline()
    {
        using var cluster = await Application.Current.CreateClusterAsync(config => config.Type = KubernetesBackend.Fake);
        await cluster.Runtime.SeedResource<V1ConfigMap>(true, TestContext.Current.CancellationToken);
        var realBuilder = new ResourceRelationshipBuilder();
        var builder = new Mock<IResourceRelationshipBuilder>();
        builder.Setup(item => item.Build(It.IsAny<IEnumerable<IKubernetesObject<V1ObjectMeta>>>(),
                It.IsAny<IReadOnlySet<string>>(), It.IsAny<bool>()))
            .Returns((IEnumerable<IKubernetesObject<V1ObjectMeta>> source, IReadOnlySet<string> namespaces, bool hideNoise) =>
                realBuilder.Build(source, namespaces, hideNoise));
        builder.Setup(item => item.BuildAdditionDelta(It.IsAny<IEnumerable<IKubernetesObject<V1ObjectMeta>>>(),
                It.IsAny<ResourceKey>(), It.IsAny<IReadOnlySet<string>>(), It.IsAny<bool>()))
            .Returns((IEnumerable<IKubernetesObject<V1ObjectMeta>> source, ResourceKey key, IReadOnlySet<string> namespaces, bool hideNoise) =>
                realBuilder.BuildAdditionDelta(source, key, namespaces, hideNoise));
        using var viewModel = new VisualizationViewModel(builder.Object);
        viewModel.InitializeScope(cluster, new VisualizationScope("default", "Application authentik",
            new ResourceAnnotationSelector(new Dictionary<string, string>
            {
                ["example.test/application"] = "authentik",
                ["example.test/environment"] = "production"
            }), IsNamespaceReadOnly: true));
        await TestWait.UntilAsync(() => viewModel.Graph!.RequiredSeedPrerequisites.Count > 0,
            TimeSpan.FromSeconds(5), cancellationToken: TestContext.Current.CancellationToken);
        var resource = new V1ConfigMap
        {
            ApiVersion = "v1",
            Kind = "ConfigMap",
            Metadata = new V1ObjectMeta
            {
                Name = "incremental",
                NamespaceProperty = "default",
                Annotations = new Dictionary<string, string>
                {
                    ["example.test/application"] = "authentik",
                    ["example.test/environment"] = "production"
                }
            }
        };
        await cluster.Runtime.AddOrUpdateResource(resource);
        await TestWait.UntilAsync(() => viewModel.Graph!.Resources.Any(item => item.Name() == "incremental"),
            TimeSpan.FromSeconds(5), cancellationToken: TestContext.Current.CancellationToken);
        resource.Data = new Dictionary<string, string> { ["update"] = "ready" };
        await cluster.Runtime.AddOrUpdateResource(resource);
        await TestWait.UntilAsync(() => viewModel.Graph!.Resources.OfType<V1ConfigMap>().Any(item => item.Data?.ContainsKey("update") == true),
            TimeSpan.FromSeconds(5), cancellationToken: TestContext.Current.CancellationToken);

        builder.Verify(item => item.BuildAdditionDelta(It.IsAny<IEnumerable<IKubernetesObject<V1ObjectMeta>>>(),
            It.Is<ResourceKey>(key => key.Name == "incremental"), It.Is<IReadOnlySet<string>>(namespaces => namespaces.Count == 0),
            It.IsAny<bool>()), Times.AtLeast(2));
        viewModel.Error.ShouldBeNull();
    }

    [AvaloniaTheory]
    [InlineData(true)]
    [InlineData(false)]
    public async Task named_scope_displays_resource_and_applies_explicit_namespace_restrictions(bool readOnly)
    {
        using var cluster = await Application.Current.CreateClusterAsync(config => config.Type = KubernetesBackend.Fake);
        using var viewModel = new VisualizationViewModel();
        var scope = new VisualizationScope("authentik", "Application authentik",
            new ResourceAnnotationSelector(new Dictionary<string, string> { ["example.test/application"] = "authentik" }),
            IsNamespaceReadOnly: readOnly);
        viewModel.InitializeScope(cluster, scope);

        viewModel.Title.ShouldBe(scope.ResourceDisplay);
        viewModel.RootResourceDisplay.ShouldBe(scope.ResourceDisplay);
        viewModel.IsRootResourceDisplayVisible.ShouldBeTrue();
        viewModel.IsNamespaceSelectionEnabled.ShouldBe(!readOnly);
        viewModel.IsNamespaceSelectionLinked.ShouldBeFalse();
        viewModel.SelectedNamespaces.Select(item => item.Name()).ShouldBe(["authentik"]);

        using var window = Application.Current.CreateTestWindow(content: new VisualizationView { DataContext = viewModel });
        try
        {
            window.Show();
            await TestApplicationExtensions.WaitForUiAsync();
            var controls = global::Avalonia.VisualTree.VisualExtensions.GetVisualDescendants(window).ToArray();
            controls.OfType<Ursa.Controls.MultiComboBox>()
                .Single(control => ReferenceEquals(control.SelectedItems, viewModel.SelectedNamespaces))
                .IsEnabled.ShouldBe(!readOnly);
            controls.OfType<global::Avalonia.Controls.Primitives.ToggleButton>()
                .Single(control => Equals(global::Avalonia.Controls.ToolTip.GetTip(control),
                    Assets.Resources.ResourceListView_NamespaceLink)).IsEnabled.ShouldBe(!readOnly);

            viewModel.InitializePreview(cluster, "preview", [Pod("authentik", "preview")]);
            viewModel.IsNamespaceSelectionEnabled.ShouldBeTrue();
            viewModel.RootResourceDisplay.ShouldNotBe(scope.ResourceDisplay);
            viewModel.Initialize(cluster);
            viewModel.IsNamespaceSelectionEnabled.ShouldBeTrue();
            viewModel.IsRootResourceDisplayVisible.ShouldBeFalse();
            viewModel.IsNamespaceSelectionLinked.ShouldBeTrue();
        }
        finally
        {
            window.Close();
        }
    }

    [Fact]
    public void annotation_selector_snapshots_requirements_and_matches_all_values_exactly()
    {
        var requirements = new Dictionary<string, string> { ["application"] = "authentik", ["environment"] = "production" };
        var selector = new ResourceAnnotationSelector(requirements);
        requirements["application"] = "changed";
        var resource = Pod("authentik", "server");
        selector.Matches(resource).ShouldBeFalse();
        resource.Metadata.Annotations = new Dictionary<string, string> { ["application"] = "authentik" };
        selector.Matches(resource).ShouldBeFalse();
        resource.Metadata.Annotations["environment"] = "Production";
        selector.Matches(resource).ShouldBeFalse();
        resource.Metadata.Annotations["environment"] = "production";
        selector.Matches(resource).ShouldBeTrue();
        ResourceGraphProjection.ToSelector(Graph(resource), selector).Resources.ShouldBe([resource]);
    }

    [Fact]
    public void annotation_selector_rejects_invalid_requirements_and_resources()
    {
        Should.Throw<ArgumentNullException>(() => new ResourceAnnotationSelector(null!));
        Should.Throw<ArgumentException>(() => new ResourceAnnotationSelector(new Dictionary<string, string>()));
        Should.Throw<ArgumentException>(() => new ResourceAnnotationSelector(new Dictionary<string, string> { [" "] = "value" }));
        Should.Throw<ArgumentNullException>(() => new ResourceAnnotationSelector(new Dictionary<string, string> { ["key"] = null! }));
        var selector = new ResourceAnnotationSelector(new Dictionary<string, string> { ["key"] = "value" });
        Should.Throw<ArgumentNullException>(() => selector.Matches(null!));
        Should.Throw<ArgumentNullException>(() => ResourceGraphProjection.ToSelector(null!, selector));
        Should.Throw<ArgumentNullException>(() => ResourceGraphProjection.ToSelector(Graph(), null!));
    }

    [AvaloniaTheory]
    [InlineData("helm")]
    [InlineData("namespace")]
    [InlineData("root")]
    public async Task build_publishes_before_processing_changes_received_during_build(string scope)
    {
        using var cluster = await Application.Current.CreateClusterAsync(config => config.Type = KubernetesBackend.Fake);
        await cluster.Runtime.SeedResource<V1ConfigMap>(true, TestContext.Current.CancellationToken);
        var resource = new V1ConfigMap
        {
            ApiVersion = "v1",
            Kind = "ConfigMap",
            Metadata = new V1ObjectMeta
            {
                Name = "authentik",
                NamespaceProperty = "default",
                Annotations = new Dictionary<string, string>
                {
                    ["meta.helm.sh/release-name"] = "authentik",
                    ["meta.helm.sh/release-namespace"] = "default"
                }
            }
        };
        await cluster.Runtime.AddOrUpdateResource(resource);
        await TestWait.UntilAsync(() => cluster.Runtime.GetResource<V1ConfigMap>("default", "authentik") is not null,
            TimeSpan.FromSeconds(5), cancellationToken: TestContext.Current.CancellationToken);
        var firstStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseFirst = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var secondStarted = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseSecond = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
        var builder = new Mock<IResourceRelationshipBuilder>();
        var realBuilder = new ResourceRelationshipBuilder();
        var count = 0;
        builder.Setup(item => item.Build(It.IsAny<IEnumerable<IKubernetesObject<V1ObjectMeta>>>(),
                It.IsAny<IReadOnlySet<string>>(), It.IsAny<bool>()))
            .Returns((IEnumerable<IKubernetesObject<V1ObjectMeta>> source, IReadOnlySet<string> namespaces, bool hideNoise) =>
            {
                var snapshot = source.ToArray();
                if (snapshot.Any(item => item.Name() == "authentik"))
                {
                    var build = Interlocked.Increment(ref count);
                    if (build == 1)
                    {
                        firstStarted.TrySetResult();
                        releaseFirst.Task.Wait(TestContext.Current.CancellationToken);
                    }
                    else if (build == 2)
                    {
                        secondStarted.TrySetResult();
                        releaseSecond.Task.Wait(TestContext.Current.CancellationToken);
                    }
                }

                return realBuilder.Build(snapshot, namespaces, hideNoise);
            });
        using var viewModel = new VisualizationViewModel(builder.Object);
        try
        {
            if (scope == "helm")
            {
                viewModel.InitializeScope(cluster, ReleaseScope("authentik", "default"));
            }
            else
            {
                viewModel.Initialize(cluster, scope == "root" ? resource
                    : new V1Namespace { Metadata = new V1ObjectMeta { Name = "default" } });
            }
            await firstStarted.Task.WaitAsync(TestContext.Current.CancellationToken);
            var updated = new TaskCompletionSource(TaskCreationOptions.RunContinuationsAsynchronously);
            using var subscription = cluster.Runtime.ConnectResources().Subscribe(change =>
            {
                if (change.Resource.Name() == "authentik" && change.EventType == WatchEventType.Modified)
                {
                    updated.TrySetResult();
                }
            });
            resource.Data = new Dictionary<string, string> { ["changed"] = "during-build" };
            await cluster.Runtime.AddOrUpdateResource(resource);
            await updated.Task.WaitAsync(TestContext.Current.CancellationToken);
            await TestApplicationExtensions.WaitForUiAsync(TestContext.Current.CancellationToken);
            releaseFirst.TrySetResult();
            await secondStarted.Task.WaitAsync(TestContext.Current.CancellationToken);

            viewModel.Graph!.Resources.ShouldContain(item => item.Name() == "authentik");
            releaseSecond.TrySetResult();
            await TestWait.UntilAsync(
                () => viewModel.Graph!.Resources.OfType<V1ConfigMap>().Any(item => item.Data?.ContainsKey("changed") == true),
                TimeSpan.FromSeconds(5), cancellationToken: TestContext.Current.CancellationToken);
        }
        finally
        {
            releaseFirst.TrySetResult();
            releaseSecond.TrySetResult();
        }
    }

    [AvaloniaFact]
    public async Task helm_visualization_loads_unseeded_release_resources_and_locks_namespace_controls()
    {
        using var cluster = await Application.Current.CreateClusterAsync(config => config.Type = KubernetesBackend.Fake);
        await cluster.Runtime.AddOrUpdateResource(new V1Namespace { Metadata = new V1ObjectMeta { Name = "authentik" } });
        await cluster.Runtime.AddOrUpdateResource(new V1Deployment
        {
            ApiVersion = "apps/v1",
            Kind = "Deployment",
            Metadata = new V1ObjectMeta
            {
                Name = "authentik-server",
                NamespaceProperty = "authentik",
                Annotations = new Dictionary<string, string>
                {
                    ["meta.helm.sh/release-name"] = "authentik",
                    ["meta.helm.sh/release-namespace"] = "authentik"
                }
            },
            Spec = new V1DeploymentSpec
            {
                Selector = new V1LabelSelector { MatchLabels = new Dictionary<string, string> { ["app"] = "authentik" } },
                Template = new V1PodTemplateSpec
                {
                    Metadata = new V1ObjectMeta { Labels = new Dictionary<string, string> { ["app"] = "authentik" } },
                    Spec = new V1PodSpec { Containers = [new V1Container { Name = "server", Image = "authentik:test" }] }
                }
            }
        });
        using var viewModel = new VisualizationViewModel();
        viewModel.InitializeScope(cluster, ReleaseScope("authentik", "authentik"));
        await TestWait.UntilAsync(() => viewModel.Graph!.Resources.Any(resource => resource.Name() == "authentik-server"),
            TimeSpan.FromSeconds(10), cancellationToken: TestContext.Current.CancellationToken);
        using var window = Application.Current.CreateTestWindow(content: new VisualizationView { DataContext = viewModel });
        try
        {
            window.Show();
            await TestApplicationExtensions.WaitForUiAsync();
            viewModel.SelectedNamespaces.Select(item => item.Name()).ShouldBe(["authentik"]);
            await TestWait.UntilAsync(() => viewModel.Graph!.Resources.Any(resource => resource.Name() == "authentik-server"),
                TimeSpan.FromSeconds(5), cancellationToken: TestContext.Current.CancellationToken);
            var controls = global::Avalonia.VisualTree.VisualExtensions.GetVisualDescendants(window).ToArray();
            var namespaceSelector = controls.OfType<Ursa.Controls.MultiComboBox>()
                .Single(control => ReferenceEquals(control.SelectedItems, viewModel.SelectedNamespaces));
            namespaceSelector.IsEnabled.ShouldBeFalse();
            controls.OfType<global::Avalonia.Controls.Primitives.ToggleButton>()
                .Single(control => Equals(global::Avalonia.Controls.ToolTip.GetTip(control),
                    Assets.Resources.ResourceListView_NamespaceLink)).IsEnabled.ShouldBeFalse();
        }
        finally
        {
            window.Close();
        }
    }

    [Theory]
    [InlineData(false, false)]
    [InlineData(true, false)]
    [InlineData(false, true)]
    public void helm_projection_requires_complete_ownership_annotations(bool hasName, bool hasNamespace)
    {
        var resource = Pod("kube-system", "incomplete");
        resource.Metadata.Annotations = new Dictionary<string, string>();
        if (hasName)
        {
            resource.Metadata.Annotations["meta.helm.sh/release-name"] = "cilium";
        }

        if (hasNamespace)
        {
            resource.Metadata.Annotations["meta.helm.sh/release-namespace"] = "kube-system";
        }

        ResourceGraphProjection.ToSelector(Graph(resource), ReleaseScope("cilium", "kube-system").Selector).Resources.ShouldBeEmpty();
    }

    [Fact]
    public void helm_projection_matches_both_annotations_and_reuses_relationship_traversal()
    {
        var root = Pod("kube-system", "cilium");
        root.Metadata.Annotations = new Dictionary<string, string>
        {
            ["meta.helm.sh/release-name"] = "cilium",
            ["meta.helm.sh/release-namespace"] = "kube-system"
        };
        var child = Pod("kube-system", "child");
        var gitOpsChild = Pod("tempo-operator-system", "tempo-operator-controller");
        var otherRelease = Pod("kube-system", "other");
        otherRelease.Metadata.Annotations = new Dictionary<string, string>(root.Metadata.Annotations)
        {
            ["meta.helm.sh/release-name"] = "another-release"
        };
        var otherNamespace = Pod("other", "cilium");
        otherNamespace.Metadata.Annotations = new Dictionary<string, string>(root.Metadata.Annotations)
        {
            ["meta.helm.sh/release-namespace"] = "other"
        };
        var clusterResource = ClusterResource("rbac.authorization.k8s.io/v1", "ClusterRole", "cilium", "role-uid");
        clusterResource.Metadata.Annotations = root.Metadata.Annotations;
        var sharedAncestor = ClusterResource("v1", "Node", "shared", "node-uid");
        var pending = new HashSet<UnresolvedResourceReference> { new("", "v1", "Secret", "kube-system", "missing") };
        var prerequisites = new HashSet<ResourceSeedPrerequisite> { new(GroupApiVersionKind.From<V1Pod>()) };
        var graph = new ResourceRelationshipGraph(
            [root, child, gitOpsChild, otherRelease, otherNamespace, clusterResource, sharedAncestor, Pod("kube-system", "unannotated")],
            [
                new(Identity(root), Identity(child), ResourceRelationshipKind.Owner),
                new(Identity(root), Identity(gitOpsChild), ResourceRelationshipKind.GitOps),
                new(Identity(sharedAncestor), Identity(root), ResourceRelationshipKind.Owner),
                new(Identity(sharedAncestor), Identity(otherRelease), ResourceRelationshipKind.Owner)
            ], pending, prerequisites);

        var scoped = ResourceGraphProjection.ToSelector(graph, ReleaseScope("cilium", "kube-system").Selector);

        scoped.Resources.ShouldBe([root, child, clusterResource, sharedAncestor]);
        scoped.Relationships.Count.ShouldBe(2);
        scoped.PendingReferences.ShouldBeSameAs(pending);
        scoped.RequiredSeedPrerequisites.ShouldBeSameAs(prerequisites);
        ResourceGraphProjection.ToSelector(graph, ReleaseScope("missing", "kube-system").Selector).Resources.ShouldBeEmpty();
    }

    [AvaloniaFact]
    public async Task helm_visualization_uses_local_namespace_and_tracks_live_ownership_changes()
    {
        using var cluster = await Application.Current.CreateClusterAsync(config => config.Type = KubernetesBackend.Fake);
        cluster.SelectedNamespaces.Clear();
        cluster.SelectedNamespaces.Add(NamespaceResource("unrelated"));
        await cluster.Runtime.SeedResource<V1ConfigMap>(true, TestContext.Current.CancellationToken);
        using var viewModel = new VisualizationViewModel();
        viewModel.InitializeScope(cluster, ReleaseScope("cilium", "default"));

        viewModel.IsPreview.ShouldBeFalse();
        viewModel.IsNamespaceSelectionLinked.ShouldBeFalse();
        viewModel.SelectedNamespaces.Select(item => item.Name()).ShouldBe(["default"]);
        cluster.SelectedNamespaces.Select(item => item.Name()).ShouldBe(["unrelated"]);
        viewModel.RootResourceDisplay.ShouldBe("Helm release: cilium (default)");
        viewModel.IsRootResourceDisplayVisible.ShouldBeTrue();
        var resource = new V1ConfigMap
        {
            ApiVersion = "v1",
            Kind = "ConfigMap",
            Metadata = new V1ObjectMeta
            {
                Name = "cilium",
                NamespaceProperty = "default",
                Annotations = new Dictionary<string, string>
                {
                    ["meta.helm.sh/release-name"] = "cilium",
                    ["meta.helm.sh/release-namespace"] = "default"
                }
            }
        };
        await cluster.Runtime.AddOrUpdateResource(resource);
        await TestWait.UntilAsync(() => viewModel.Graph!.Resources.Any(item => item.Name() == "cilium"),
            TimeSpan.FromSeconds(10), cancellationToken: TestContext.Current.CancellationToken);
        resource.Metadata.Annotations.Clear();
        await cluster.Runtime.AddOrUpdateResource(resource);
        await TestWait.UntilAsync(() => viewModel.Graph!.Resources.Count == 0,
            TimeSpan.FromSeconds(10), cancellationToken: TestContext.Current.CancellationToken);
        resource.Metadata.Annotations["meta.helm.sh/release-name"] = "cilium";
        resource.Metadata.Annotations["meta.helm.sh/release-namespace"] = "default";
        await cluster.Runtime.AddOrUpdateResource(resource);
        await TestWait.UntilAsync(() => viewModel.Graph!.Resources.Count == 1,
            TimeSpan.FromSeconds(10), cancellationToken: TestContext.Current.CancellationToken);
        await cluster.Runtime.DeleteResource(resource);
        await TestWait.UntilAsync(() => viewModel.Graph!.Resources.Count == 0,
            TimeSpan.FromSeconds(10), cancellationToken: TestContext.Current.CancellationToken);
        viewModel.Error.ShouldBeNull();
    }

    [AvaloniaFact]
    public async Task preview_initializes_relationship_graph_without_adding_resources_to_cluster()
    {
        using var cluster = await Application.Current.CreateClusterAsync(config => config.Type = KubernetesBackend.Fake);
        var runtimeObjectCount = cluster.Runtime.Objects.Count;
        var deployment = new V1Deployment
        {
            ApiVersion = "apps/v1",
            Kind = "Deployment",
            Metadata = new V1ObjectMeta { Name = "web", NamespaceProperty = "apps", Uid = "deployment-uid" }
        };
        var replicaSet = new V1ReplicaSet
        {
            ApiVersion = "apps/v1",
            Kind = "ReplicaSet",
            Metadata = new V1ObjectMeta
            {
                Name = "web-abc",
                NamespaceProperty = "apps",
                Uid = "replicaset-uid",
                OwnerReferences =
                [
                    new V1OwnerReference
                    {
                        ApiVersion = "apps/v1",
                        Kind = "Deployment",
                        Name = "web",
                        Uid = "deployment-uid"
                    }
                ]
            }
        };
        using var viewModel = new VisualizationViewModel();

        viewModel.InitializePreview(cluster, "web", [deployment, replicaSet]);

        await TestWait.UntilAsync(
            () => viewModel.Graph?.Resources.Count == 2,
            TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken);

        viewModel.IsPreview.ShouldBeTrue();
        viewModel.PreviewTitle.ShouldBe("web");
        viewModel.Graph!.Relationships.ShouldContain(relationship => relationship.Kind == ResourceRelationshipKind.Owner);
        cluster.Runtime.Objects.Count.ShouldBe(runtimeObjectCount);
    }

    [AvaloniaFact]
    public async Task namespace_selection_follows_cluster_until_unlinked_then_relinks()
    {
        using var cluster = await Application.Current.CreateClusterAsync(config => config.Type = KubernetesBackend.Fake);
        var teamA = NamespaceResource("team-a");
        var teamB = NamespaceResource("team-b");
        var teamC = NamespaceResource("team-c");
        cluster.SelectedNamespaces.Add(teamA);

        using var viewModel = new VisualizationViewModel(new ResourceRelationshipBuilder());
        viewModel.Initialize(cluster);

        viewModel.IsNamespaceSelectionLinked.ShouldBeTrue();
        ReferenceEquals(viewModel.SelectedNamespaces, cluster.SelectedNamespaces).ShouldBeTrue();

        cluster.SelectedNamespaces.Add(teamB);
        viewModel.SelectedNamespaces.Select(item => item.Name()).ShouldBe(["team-a", "team-b"]);

        viewModel.IsNamespaceSelectionLinked = false;
        viewModel.SelectedNamespaces.Select(item => item.Name()).ShouldBe(["team-a", "team-b"]);
        viewModel.SelectedNamespaces.Add(teamC);

        cluster.SelectedNamespaces.Select(item => item.Name()).ShouldBe(["team-a", "team-b"]);
        viewModel.SelectedNamespaces.Select(item => item.Name()).ShouldBe(["team-a", "team-b", "team-c"]);

        viewModel.IsNamespaceSelectionLinked = true;
        ReferenceEquals(viewModel.SelectedNamespaces, cluster.SelectedNamespaces).ShouldBeTrue();
        cluster.SelectedNamespaces.Add(NamespaceResource("team-d"));
        viewModel.SelectedNamespaces.Select(item => item.Name()).ShouldBe(["team-a", "team-b", "team-d"]);
    }

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
        var rootIdentity = Identity(root);
        var parentIdentity = Identity(parent);
        var childIdentity = Identity(child);

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
                using var registration = cancellationToken.Register(canceled.SetResult);
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
                using var registration = cancellationToken.Register(canceled.SetResult);
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

    private static V1Namespace NamespaceResource(string name)
        => new() { Metadata = new V1ObjectMeta { Name = name } };

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
