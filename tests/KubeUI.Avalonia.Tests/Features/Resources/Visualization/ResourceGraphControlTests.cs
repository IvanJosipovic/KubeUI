using Avalonia.Headless.XUnit;
using Avalonia.Controls;
using Avalonia.Threading;
using System.Diagnostics;
using k8s;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Avalonia.Features.Resources.Visualization;
using KubeUI.Avalonia.Tests.Infra;
using KubeUI.Kubernetes.Resources.Relationships;
using Shouldly;
using k8s.Models;
using Westermo.GraphX.Controls.Controls;
using Westermo.GraphX.Controls.Behaviours;

namespace KubeUI.Avalonia.Tests.Features.Resources.Visualization;

public sealed class ResourceGraphControlTests : AvaloniaTestBase
{
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
            DragBehaviour.GetIsDragEnabled(sourceControl).ShouldBeTrue();
            DragBehaviour.GetUpdateEdgesOnMove(sourceControl).ShouldBeTrue();
            edge.SourceEndpoint.ShouldNotBeNull();
            edge.TargetEndpoint.ShouldNotBeNull();
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
    public void removes_deleted_resource_from_live_graph_without_replacing_graph()
    {
        V1Pod source = new() { ApiVersion = "v1", Kind = V1Pod.KubeKind, Metadata = new() { Name = "source", NamespaceProperty = "demo", Uid = "source" } };
        V1Pod target = new() { ApiVersion = "v1", Kind = V1Pod.KubeKind, Metadata = new() { Name = "target", NamespaceProperty = "demo", Uid = "target" } };
        ResourceIdentity sourceIdentity = new("v1", V1Pod.KubeKind, "demo", "source", "source");
        ResourceIdentity targetIdentity = new("v1", V1Pod.KubeKind, "demo", "target", "target");
        ResourceRelationshipGraph graph = new(
            [source, target],
            [new ResourceRelationship(sourceIdentity, targetIdentity, ResourceRelationshipKind.Reference)]);
        using ResourceGraphControl control = new() { Graph = graph };
        ResourceRelationshipGraph graphBeforeDelete = control.Graph!;

        control.ApplyResourceDeletion(sourceIdentity);

        ReferenceEquals(control.Graph, graphBeforeDelete).ShouldBeTrue();
        control.Area.LogicCore!.Graph.VertexCount.ShouldBe(1);
        control.Area.LogicCore.Graph.EdgeCount.ShouldBe(0);
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
}
