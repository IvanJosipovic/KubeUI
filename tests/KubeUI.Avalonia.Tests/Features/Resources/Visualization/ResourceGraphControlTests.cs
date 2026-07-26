using Avalonia.Headless.XUnit;
using Avalonia.Controls;
using Avalonia.Controls.Presenters;
using Avalonia.Threading;
using Avalonia.VisualTree;
using System.Diagnostics;
using k8s;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Avalonia.Features.Resources.Visualization;
using KubeUI.Avalonia.Tests.Infra;
using KubeUI.Kubernetes.Resources.Relationships;
using Shouldly;
using k8s.Models;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Tests.Features.Clusters.Workspace;
using Westermo.GraphX.Controls.Controls;
using Westermo.GraphX.Controls.Behaviours;
using Westermo.GraphX.Controls.Controls.EdgeLabels;
using Westermo.GraphX.Controls.Controls.VertexLabels;

namespace KubeUI.Avalonia.Tests.Features.Resources.Visualization;

public sealed class ResourceGraphControlTests : AvaloniaTestBase
{
    [AvaloniaFact]
    public async Task processed_resource_config_starts_required_seed_without_waiting_for_ready()
    {
        var innerRuntime = new TestCluster();
        await innerRuntime.AddOrUpdateResource(new V1Namespace { Metadata = new() { Name = "default" } });
        var runtime = new CountingClusterRuntime(innerRuntime);
        using var cluster = ActivatorUtilities.CreateInstance<ClusterWorkspace>(TestApp.CurrentServices!, runtime);
        cluster.SelectedNamespaces.Add(innerRuntime.Namespaces.Single());
        using VisualizationViewModel viewModel = new(new ResourceRelationshipBuilder());

        viewModel.Initialize(cluster);

        runtime.EventSeedCalls.ShouldBe(0);
        await cluster.Connect();

        runtime.EventSeedCalls.ShouldBeGreaterThan(0);
        runtime.EventSeedWaitForReady.ShouldBeFalse();
    }

    [AvaloniaFact]
    public async Task added_resource_starts_owner_reference_seed_without_waiting_for_ready()
    {
        var innerRuntime = new TestCluster();
        await innerRuntime.AddOrUpdateResource(new V1Namespace { Metadata = new() { Name = "default" } });
        var runtime = new CountingClusterRuntime(innerRuntime);
        using var cluster = ActivatorUtilities.CreateInstance<ClusterWorkspace>(TestApp.CurrentServices!, runtime);
        cluster.SelectedNamespaces.Add(innerRuntime.Namespaces.Single());
        using VisualizationViewModel viewModel = new(new ResourceRelationshipBuilder());
        var ownerSeed = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);

        await cluster.Connect();

        void OnResourceSeedRequested(Type resourceType, bool waitForReady)
        {
            if (resourceType == typeof(V1CustomResourceDefinition))
            {
                ownerSeed.TrySetResult(waitForReady);
            }
        }

        runtime.ResourceSeedRequested += OnResourceSeedRequested;
        try
        {
            viewModel.Initialize(cluster);
            await innerRuntime.AddOrUpdateResource(new V1Pod
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
            });

            (await ownerSeed.Task.WaitAsync(TimeSpan.FromSeconds(5))).ShouldBeFalse();
        }
        finally
        {
            runtime.ResourceSeedRequested -= OnResourceSeedRequested;
        }
    }

    [AvaloniaFact]
    public async Task added_unrelated_cluster_scoped_resource_does_not_bypass_namespace_filter()
    {
        using var cluster = await TestCluster.GetAsync();
        var builder = new AdditionCaptureRelationshipBuilder();
        using VisualizationViewModel viewModel = new(builder);
        viewModel.Initialize(cluster);
        await builder.WaitForInitialBuildAsync();

        await cluster.Runtime.AddOrUpdateResource(new V1Node
        {
            ApiVersion = "v1",
            Kind = V1Node.KubeKind,
            Metadata = new() { Name = "unrelated-node", Uid = "unrelated-node" },
        });

        ResourceRelationshipGraph delta = await builder.WaitForAdditionAsync();

        delta.Resources.ShouldBeEmpty();
        delta.Relationships.ShouldBeEmpty();
    }

    [AvaloniaFact]
    public async Task namespace_selection_does_not_run_graph_build_on_ui_thread()
    {
        using var cluster = await TestCluster.GetAsync();
        cluster.SelectedNamespaces.Add(new V1Namespace { Metadata = new() { Name = "default" } });

        var builder = new DelayingRelationshipBuilder();
        using VisualizationViewModel viewModel = new(builder);
        viewModel.Initialize(cluster);
        await builder.WaitForBuildAsync(1);

        Stopwatch stopwatch = Stopwatch.StartNew();
        cluster.SelectedNamespaces.Add(new V1Namespace { Metadata = new() { Name = "other" } });
        stopwatch.Stop();

        stopwatch.Elapsed.ShouldBeLessThan(TimeSpan.FromMilliseconds(100));
        (await builder.WaitForBuildAsync(2)).ShouldNotBe(Environment.CurrentManagedThreadId);
    }

    [AvaloniaFact]
    public async Task added_resource_delta_uses_the_resource_snapshot_from_its_event()
    {
        using var cluster = await TestCluster.GetAsync();
        var builder = new AdditionSnapshotRelationshipBuilder();
        using VisualizationViewModel viewModel = new(builder);
        viewModel.Initialize(cluster);
        await builder.WaitForInitialBuildAsync();

        ThreadPool.GetMinThreads(out int workerThreads, out int completionPortThreads);
        ThreadPool.SetMinThreads(Math.Max(workerThreads, 32), completionPortThreads);
        using var workersStarted = new CountdownEvent(32);
        using var releaseWorkers = new ManualResetEventSlim();
        Task[] workers = Enumerable.Range(0, 32)
            .Select(_ => Task.Run(() =>
            {
                workersStarted.Signal();
                releaseWorkers.Wait();
            }))
            .ToArray();

        try
        {
            workersStarted.Wait(TimeSpan.FromSeconds(10)).ShouldBeTrue();

            await cluster.AddOrUpdateResource(CreatePod("first"));
            await cluster.AddOrUpdateResource(CreatePod("second"));
            viewModel.HideNoise = false;
            releaseWorkers.Set();

            AdditionDeltaInput firstDelta = await builder.WaitForFirstAdditionAsync();
            firstDelta.Resources.ShouldContain("first");
            firstDelta.Resources.ShouldNotContain("second");
            firstDelta.HideNoise.ShouldBeTrue();
        }
        finally
        {
            releaseWorkers.Set();
            await Task.WhenAll(workers);
            ThreadPool.SetMinThreads(workerThreads, completionPortThreads);
        }
    }

    [AvaloniaFact]
    public void creates_graph_vertices_and_edges_from_graph_input()
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

        control.Area.LogicCore.ShouldNotBeNull();
        control.Area.LogicCore!.Graph.VertexCount.ShouldBe(2);
        control.Area.LogicCore.Graph.EdgeCount.ShouldBe(1);
        control.Area.SelectionMode.ShouldBe(SelectionMode.Multiple);
        control.Area.SelectedVertices.ShouldNotBeNull();
    }

    [AvaloniaFact]
    public async Task initial_graph_shows_target_arrow_without_an_additional_layout_pass()
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
            await Task.Delay(250);

            EdgeControl edge = control.Area.EdgesList.Values.Single();
            edge.GetEdgePointerForSource().ShouldBeNull();
            edge.GetEdgePointerForTarget()!.IsVisible.ShouldBeTrue();
            Control targetPointer = (Control)edge.GetEdgePointerForTarget()!;
            targetPointer.Bounds.Width.ShouldBeGreaterThan(0);
            targetPointer.Bounds.Height.ShouldBeGreaterThan(0);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task graph_edges_attach_to_vertex_bounds_after_layout()
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
            await Dispatcher.UIThread.InvokeAsync(control.Area.UpdateLayout);

            EdgeControl edge = control.Area.EdgesList.Values.Single();
            VertexControl sourceControl = control.Area.VertexList.Values.First();
            VertexControl targetControl = control.Area.VertexList.Values.Last();
            control.Area.Children.OfType<AttachableVertexLabelControl>().ShouldBeEmpty();
            Label[] sourceLabels = sourceControl.GetVisualDescendants().OfType<Label>().ToArray();
            sourceLabels.Select(label => label.Content).ShouldBe([V1Pod.KubeKind, "source"]);
            DragBehaviour.GetIsDragEnabled(sourceControl).ShouldBeTrue();
            DragBehaviour.GetUpdateEdgesOnMove(sourceControl).ShouldBeTrue();
            edge.SourceEndpoint.ShouldNotBeNull();
            edge.TargetEndpoint.ShouldNotBeNull();
            edge.GetEdgePointerForSource().ShouldBeNull();
            edge.GetEdgePointerForTarget().ShouldNotBeNull();
            edge.GetEdgePointerForTarget()!.IsVisible.ShouldBeTrue();
            ((Control)edge.GetEdgePointerForTarget()!).Bounds.Width.ShouldBeGreaterThan(0);
            ((Control)edge.GetEdgePointerForTarget()!).Bounds.Height.ShouldBeGreaterThan(0);
            ContentControl targetPointer = (ContentControl)edge.GetEdgePointerForTarget()!;
            targetPointer.Content.ShouldNotBeNull();
            global::Avalonia.Controls.Shapes.Path targetArrow = (global::Avalonia.Controls.Shapes.Path)targetPointer.Content!;
            targetArrow.Bounds.Width.ShouldBeGreaterThan(0);
            targetArrow.Bounds.Height.ShouldBeGreaterThan(0);
            targetArrow.Fill.ShouldNotBeNull();
            edge.SourceEndpoint.Value.ShouldNotBe(sourceControl.GetCenterPosition(final: true));
            edge.TargetEndpoint.Value.ShouldNotBe(targetControl.GetCenterPosition(final: true));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task labels_and_arrows_are_created_when_graph_is_added_after_initial_empty_graph()
    {
        V1Pod source = new() { ApiVersion = "v1", Kind = V1Pod.KubeKind, Metadata = new() { Name = "source", NamespaceProperty = "demo", Uid = "source" } };
        V1Pod target = new() { ApiVersion = "v1", Kind = V1Pod.KubeKind, Metadata = new() { Name = "target", NamespaceProperty = "demo", Uid = "target" } };
        ResourceIdentity sourceIdentity = new("v1", V1Pod.KubeKind, "demo", "source", "source");
        ResourceIdentity targetIdentity = new("v1", V1Pod.KubeKind, "demo", "target", "target");
        using ResourceGraphControl control = new() { Graph = ResourceRelationshipGraph.Empty };
        Window window = new() { Width = 800, Height = 600, Content = control };
        window.Show();
        await Dispatcher.UIThread.InvokeAsync(control.Area.UpdateLayout);

        control.Graph = new ResourceRelationshipGraph(
            [source, target],
            [new ResourceRelationship(sourceIdentity, targetIdentity, ResourceRelationshipKind.Reference)]);

        await Task.Delay(250);

        control.Area.Children.OfType<AttachableVertexLabelControl>().ShouldBeEmpty();
        AttachableEdgeLabelControl edgeLabel = control.Area.Children.OfType<AttachableEdgeLabelControl>().Single();
        edgeLabel.GetVisualDescendants().OfType<Border>().Single().Background.ShouldBe(global::Avalonia.Media.Brushes.Transparent);
        control.Area.EdgesList.Values.Single().GetEdgePointerForSource().ShouldBeNull();
        control.Area.EdgesList.Values.Single().GetEdgePointerForTarget()!.IsVisible.ShouldBeTrue();
    }

    [AvaloniaFact]
    public void graph_edge_uses_relationship_text_for_graphx_label()
    {
        ResourceIdentity sourceIdentity = new("v1", V1Pod.KubeKind, "demo", "source", "source");
        ResourceIdentity targetIdentity = new("v1", V1Pod.KubeKind, "demo", "target", "target");
        ResourceGraphEdge edge = new(
            new ResourceGraphVertex { Identity = sourceIdentity, Node = null! },
            new ResourceGraphVertex { Identity = targetIdentity, Node = null! },
            new ResourceRelationship(sourceIdentity, targetIdentity, ResourceRelationshipKind.Label, "app.kubernetes.io/component"));

        edge.ToString().ShouldBe("Label: app.kubernetes.io/component");
        edge.RelationshipName.ShouldBe("Label");
    }

    [AvaloniaFact]
    public void graph_edge_exposes_relationship_brush_for_graphx_binding()
    {
        ResourceIdentity sourceIdentity = new("v1", V1Pod.KubeKind, "demo", "source", "source");
        ResourceIdentity targetIdentity = new("v1", V1Pod.KubeKind, "demo", "target", "target");
        ResourceGraphEdge edge = new(
            new ResourceGraphVertex { Identity = sourceIdentity, Node = null! },
            new ResourceGraphVertex { Identity = targetIdentity, Node = null! },
            new ResourceRelationship(sourceIdentity, targetIdentity, ResourceRelationshipKind.Owner));

        edge.Brush.ShouldBe(global::Avalonia.Media.Brushes.DodgerBlue);
    }

    [AvaloniaFact]
    public void applies_deleted_resource_from_graph_without_rebuilding_remaining_vertex()
    {
        V1Pod source = new() { ApiVersion = "v1", Kind = V1Pod.KubeKind, Metadata = new() { Name = "source", NamespaceProperty = "demo", Uid = "source" } };
        V1Pod target = new() { ApiVersion = "v1", Kind = V1Pod.KubeKind, Metadata = new() { Name = "target", NamespaceProperty = "demo", Uid = "target" } };
        ResourceIdentity sourceIdentity = new("v1", V1Pod.KubeKind, "demo", "source", "source");
        ResourceIdentity targetIdentity = new("v1", V1Pod.KubeKind, "demo", "target", "target");
        ResourceRelationshipGraph graph = new(
            [source, target],
            [new ResourceRelationship(sourceIdentity, targetIdentity, ResourceRelationshipKind.Reference)]);
        using ResourceGraphControl control = new() { Graph = graph };
        ResourceGraphVertex remainingVertex = control.Area.LogicCore!.Graph.Vertices.Single(vertex => vertex.Identity == targetIdentity);
        control.Graph = new ResourceRelationshipGraph([target], []);

        control.Area.LogicCore!.Graph.VertexCount.ShouldBe(1);
        control.Area.LogicCore.Graph.EdgeCount.ShouldBe(0);
        control.Area.LogicCore.Graph.Vertices.Single().ShouldBeSameAs(remainingVertex);
    }

    [AvaloniaFact]
    public void applies_graph_changes_after_visual_layout_has_been_cleared()
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

        control.Area.ClearLayout();
        control.Graph = ResourceRelationshipGraph.Empty;

        control.Area.LogicCore!.Graph.VertexCount.ShouldBe(0);
        control.Area.LogicCore.Graph.EdgeCount.ShouldBe(0);
    }

    private sealed class DelayingRelationshipBuilder : IResourceRelationshipBuilder
    {
        private readonly TaskCompletionSource<int> _firstBuild = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource<int> _secondBuild = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public ResourceRelationshipGraph Build(
            IEnumerable<IKubernetesObject<V1ObjectMeta>> resources,
            IReadOnlySet<string> selectedNamespaces,
            bool hideNoise)
        {
            int buildNumber = _firstBuild.Task.IsCompleted ? 2 : 1;
            if (buildNumber == 2)
            {
                Thread.Sleep(250);
            }

            (_firstBuild.Task.IsCompleted ? _secondBuild : _firstBuild).TrySetResult(Environment.CurrentManagedThreadId);
            return ResourceRelationshipGraph.Empty;
        }

        public ResourceRelationshipGraph BuildAdditionDelta(
            IEnumerable<IKubernetesObject<V1ObjectMeta>> resources,
            ResourceKey addedResource,
            IReadOnlySet<string> selectedNamespaces,
            bool hideNoise)
            => ResourceRelationshipGraph.Empty;

        public async Task<int> WaitForBuildAsync(int buildNumber)
            => await (buildNumber == 1 ? _firstBuild.Task : _secondBuild.Task);
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

        public async Task WaitForInitialBuildAsync() => await _initialBuild.Task;

        public async Task<AdditionDeltaInput> WaitForFirstAdditionAsync() => await _firstAddition.Task;
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
            ResourceRelationshipGraph graph = _inner.Build(resources, selectedNamespaces, hideNoise);
            _initialBuild.TrySetResult();
            return graph;
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

        public async Task WaitForInitialBuildAsync() => await _initialBuild.Task;

        public async Task<ResourceRelationshipGraph> WaitForAdditionAsync() => await _addition.Task;
    }

    private sealed record AdditionDeltaInput(IReadOnlyList<string> Resources, bool HideNoise);

    private static V1Pod CreatePod(string name) => new()
    {
        ApiVersion = "v1",
        Kind = V1Pod.KubeKind,
        Metadata = new()
        {
            Name = name,
            NamespaceProperty = "default",
            Uid = name,
        },
    };
}
