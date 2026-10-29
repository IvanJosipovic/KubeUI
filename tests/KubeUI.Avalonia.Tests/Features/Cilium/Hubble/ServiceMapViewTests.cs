using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using Dock.Model.Controls;
using Dock.Model.Core;
using FluentIcons.Avalonia;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Features.Cilium.Hubble;
using KubeUI.Avalonia.Features.Cilium.Hubble.Controls;
using KubeUI.Avalonia.Features.Resources.Properties.Controls;
using KubeUI.Avalonia.Services.Icons;
using KubeUI.Avalonia.Tests.Infra;
using KubeUI.Cilium.Hubble;
using KubeUI.Testing.Utilities;
using Microsoft.Extensions.DependencyInjection;
using QuikGraph;
using Shouldly;
using Westermo.GraphX.Common.Enums;
using Westermo.GraphX.Controls.Behaviours;
using Westermo.GraphX.Controls.Controls;
using Westermo.GraphX.Controls.Controls.EdgePointers;
using Westermo.GraphX.Logic.Algorithms.LayoutAlgorithms;
using Westermo.GraphX.Logic.Algorithms.OverlapRemoval;

namespace KubeUI.Avalonia.Tests.Features.Cilium.Hubble;

public sealed class ServiceMapViewTests
{
    [AvaloniaFact]
    public async Task Snapshot_updates_context_menu_and_denied_edge_style()
    {
        static ServiceMapGraph Snapshot(int denied)
        {
            var graph = new ServiceMapGraph();
            graph.Nodes.Add(new ServiceMapNode { Id = "a", Name = "a", Namespace = "team", DeniedCount = denied });
            graph.Nodes.Add(new ServiceMapNode { Id = "b", Name = "b", Namespace = "team" });
            graph.Edges.Add(new ServiceMapEdge { SourceId = "a", TargetId = "b", FlowCount = 1, DeniedCount = denied });
            return graph;
        }

        using var control = new ServiceMapGraphControl { Graph = Snapshot(0) };
        using var window = Application.Current.CreateTestWindow(1200, 800, control);
        window.Show();
        await TestWait.UntilAsync(() => control.HasGeneratedGraph && control.LayoutTask?.IsCompleted == true,
            timeoutMs: 10_000, cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
        var updated = Snapshot(1);
        control.Graph = updated;
        await TestWait.UntilAsync(() => control.Area.VertexList.Single(pair => pair.Key.Id == "a").Key.Node == updated.Nodes[0],
            timeoutMs: 10_000, cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
        var vertex = control.Area.VertexList.Single(pair => pair.Key.Id == "a").Value;
        vertex.ContextMenu!.Items.OfType<MenuItem>().Single().CommandParameter.ShouldBeSameAs(updated.Nodes[0]);
        control.Area.EdgesList.Single().Value.Classes.Contains("ServiceMapDeniedFlow").ShouldBeTrue();
        var cleared = Snapshot(0);
        control.Graph = cleared;
        await TestWait.UntilAsync(() => control.Area.VertexList.Single(pair => pair.Key.Id == "a").Key.Node == cleared.Nodes[0],
            timeoutMs: 10_000, cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
        control.Area.EdgesList.Single().Value.Classes.Contains("ServiceMapDeniedFlow").ShouldBeFalse();
    }

    [AvaloniaFact]
    public async Task Disposing_while_layout_is_running_defers_graphx_disposal()
    {
        var graph = new ServiceMapGraph();
        graph.Nodes.Add(new ServiceMapNode { Id = "team-a/client", Name = "client", Namespace = "team-a" });
        graph.Nodes.Add(new ServiceMapNode { Id = "team-a/api", Name = "api", Namespace = "team-a" });
        graph.Nodes.Add(new ServiceMapNode { Id = "team-b/db", Name = "db", Namespace = "team-b" });
        graph.Edges.Add(new ServiceMapEdge { SourceId = "team-a/client", TargetId = "team-a/api", FlowCount = 1 });
        graph.Edges.Add(new ServiceMapEdge { SourceId = "team-a/api", TargetId = "team-b/db", FlowCount = 1 });

        var graphControl = new ServiceMapGraphControl { Graph = graph };
        using var window = Application.Current.CreateTestWindow(1200, 800, graphControl);
        window.Show();
        await TestWait.UntilAsync(
            () => graphControl.HasGeneratedGraph && graphControl.LayoutTask?.IsCompleted == true,
            timeoutMs: 10_000,
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
        await graphControl.LayoutTask!.WaitAsync(TestContext.Current.CancellationToken);

        graphControl.LayoutAlgorithm = LayoutAlgorithmTypeEnum.Circular;
        graphControl.LayoutTask!.IsCompleted.ShouldBeFalse();
        graphControl.Dispose();
        graphControl.GraphXDisposed.ShouldBeFalse();

        await TestWait.UntilAsync(
            () => graphControl.GraphXDisposed,
            timeoutMs: 10_000,
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
        await graphControl.LayoutTask!.WaitAsync(TestContext.Current.CancellationToken);
        graphControl.GraphXDisposed.ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task Kubernetes_service_map_nodes_show_the_resource_kind_icon()
    {
        var iconService = new RecordingResourceIconService();
        var graph = new ServiceMapGraph();
        graph.Nodes.Add(new ServiceMapNode
        {
            Id = "team-a/frontend",
            Name = "frontend",
            Namespace = "team-a",
            WorkloadType = "Deployment",
            IngressFlowCount = 3,
            EgressFlowCount = 2,
            DeniedCount = 1,
        });
        graph.Nodes[0].AccessPoints.Add(new ServiceMapAccessPoint { Protocol = "TCP", Port = 443 });
        graph.Nodes[0].AccessPoints.Add(new ServiceMapAccessPoint { Protocol = "UDP", Port = 53 });
        graph.Nodes.Add(new ServiceMapNode
        {
            Id = "team-a/backend",
            Name = "backend",
            Namespace = "team-a",
            WorkloadType = "Deployment",
        });
        graph.Nodes.Add(new ServiceMapNode
        {
            Id = "world/dns",
            Name = "resolver.example",
            WorkloadType = "dns",
        });
        graph.Edges.Add(new ServiceMapEdge { SourceId = "team-a/frontend", TargetId = "team-a/backend", FlowCount = 1 });
        graph.Edges.Add(new ServiceMapEdge { SourceId = "team-a/backend", TargetId = "world/dns", FlowCount = 1 });

        var graphControl = new ServiceMapGraphControl(iconService) { Graph = graph };
        using var window = Application.Current.CreateTestWindow(1200, 800, graphControl);
        window.Show();
        await TestWait.UntilAsync(
            () => graphControl.HasGeneratedGraph && graphControl.LayoutTask?.IsCompleted == true,
            timeoutMs: 10_000,
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
        await graphControl.LayoutTask!.WaitAsync(TestContext.Current.CancellationToken);

        var frontend = graphControl.Area.VertexList.Single(pair => pair.Key.Id == "team-a/frontend").Value;
        var icon = frontend.GetVisualDescendants().OfType<Image>().ShouldHaveSingleItem();
        icon.Width.ShouldBe(64);
        icon.Height.ShouldBe(64);
        var nodeSurface = frontend.GetVisualDescendants().OfType<Border>()
            .Single(border => border.Classes.Contains("ServiceMapNode"));
        nodeSurface.Width.ShouldBe(200);
        nodeSurface.MinHeight.ShouldBe(84);
        var nodeContent = nodeSurface.Child.ShouldBeOfType<StackPanel>();
        nodeContent.Spacing.ShouldBe(16);
        var identityContent = nodeContent.Children[0].ShouldBeOfType<StackPanel>();
        identityContent.Spacing.ShouldBe(6);
        double.IsNaN(identityContent.Height).ShouldBeTrue();
        identityContent.Children.Count.ShouldBe(4);
        nodeContent.Children[1].ShouldBeOfType<StackPanel>().Spacing.ShouldBe(2);
        icon.Source.ShouldNotBeNull();
        var labels = frontend.GetVisualDescendants().OfType<TextBlock>()
            .Select(static textBlock => textBlock.Text)
            .ToArray();
        labels.ShouldContain("Deployment");
        labels.ShouldContain("frontend");
        labels.ShouldContain(static label => label.Contains("In 3", StringComparison.Ordinal));
        labels.ShouldContain(static label => label.Contains("Out 2", StringComparison.Ordinal));
        labels.ShouldContain("→ 443 • TCP");
        labels.ShouldContain("→ 53 • UDP");
        labels.Count(static label => label?.StartsWith("→ ", StringComparison.Ordinal) == true).ShouldBe(2);
        iconService.RequestedKinds.ShouldContain(new GroupApiVersionKind("apps", "v1", "Deployment", string.Empty));
        var externalNode = graphControl.Area.VertexList.Single(pair => pair.Key.Id == "world/dns").Value;
        externalNode.GetVisualDescendants().OfType<Image>().ShouldBeEmpty();
        var externalIcon = externalNode.GetVisualDescendants().OfType<FluentIcon>().ShouldHaveSingleItem();
        externalIcon.Icon.ShouldBe(FluentIcons.Common.Icon.Globe);
        externalIcon.FontSize.ShouldBe(64);
        externalIcon.Foreground.ShouldNotBeNull();
    }

    [AvaloniaFact]
    public async Task Service_map_nodes_resolve_icons_for_every_supported_workload_kind()
    {
        var iconService = new RecordingResourceIconService();
        var expectedKinds = new Dictionary<string, GroupApiVersionKind>
        {
            ["Service"] = new(string.Empty, "v1", "Service", string.Empty),
            ["Namespace"] = new(string.Empty, "v1", "Namespace", string.Empty),
            ["Pod"] = new(string.Empty, "v1", "Pod", string.Empty),
            ["Deployment"] = new("apps", "v1", "Deployment", string.Empty),
            ["DaemonSet"] = new("apps", "v1", "DaemonSet", string.Empty),
            ["StatefulSet"] = new("apps", "v1", "StatefulSet", string.Empty),
            ["ReplicaSet"] = new("apps", "v1", "ReplicaSet", string.Empty),
            ["Job"] = new("batch", "v1", "Job", string.Empty),
            ["CronJob"] = new("batch", "v1", "CronJob", string.Empty),
            ["Unicorn"] = new(string.Empty, "v1", "Unicorn", string.Empty),
        };
        var graph = new ServiceMapGraph();
        foreach (var workloadType in expectedKinds.Keys)
        {
            graph.Nodes.Add(new ServiceMapNode
            {
                Id = $"team-a/{workloadType}",
                Name = workloadType,
                Namespace = "team-a",
                WorkloadType = workloadType,
            });
        }

        graph.Nodes.Add(new ServiceMapNode { Id = "world/ext", Name = "example.org", WorkloadType = "world" });

        var graphControl = new ServiceMapGraphControl(iconService) { Graph = graph };
        using var window = Application.Current.CreateTestWindow(1200, 800, graphControl);
        window.Show();
        await TestWait.UntilAsync(
            () => graphControl.HasGeneratedGraph && graphControl.LayoutTask?.IsCompleted == true,
            timeoutMs: 10_000,
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
        await graphControl.LayoutTask!.WaitAsync(TestContext.Current.CancellationToken);

        foreach (var expected in expectedKinds.Values)
            iconService.RequestedKinds.ShouldContain(expected);
        iconService.RequestedKinds.Count.ShouldBe(expectedKinds.Count);
        graphControl.Area.VertexList.Single(pair => pair.Key.Id == "world/ext").Value
            .GetVisualDescendants().OfType<Image>().ShouldBeEmpty();
    }

    [AvaloniaFact]
    public async Task Service_map_node_context_menu_offers_view_action()
    {
        var services = Application.Current.GetTestServices();
        var factory = services.GetRequiredService<IFactory>();
        factory.InitLayout(factory.CreateLayout());
        var rightDock = factory.GetDockable<IToolDock>("RightDock")!;
        await using var viewModel = services.GetRequiredService<ServiceMapViewModel>();

        var graph = new ServiceMapGraph();
        var client = new ServiceMapNode
        {
            Id = "team-a/client",
            Name = "client",
            Namespace = "team-a",
            WorkloadType = "Deployment",
            IngressFlowCount = 3,
            EgressFlowCount = 5,
            AllowedCount = 7,
            DeniedCount = 1,
        };
        client.AccessPoints.Add(new ServiceMapAccessPoint
        {
            Protocol = "TCP",
            Port = 8080,
            FlowCount = 4,
            DeniedCount = 1,
        });
        graph.Nodes.Add(client);
        graph.Nodes.Add(new ServiceMapNode { Id = "team-a/api", Name = "api", Namespace = "team-a" });
        graph.Edges.Add(new ServiceMapEdge { SourceId = "team-a/client", TargetId = "team-a/api", FlowCount = 1 });

        viewModel.Graph = graph;
        var view = new ServiceMapView { DataContext = viewModel };
        using var window = Application.Current.CreateTestWindow(1200, 800, view);
        window.Show();
        var graphControl = view.GetVisualDescendants().OfType<ServiceMapGraphControl>().Single();
        await TestWait.UntilAsync(
            () => graphControl.HasGeneratedGraph && graphControl.LayoutTask?.IsCompleted == true,
            timeoutMs: 10_000,
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
        await graphControl.LayoutTask!.WaitAsync(TestContext.Current.CancellationToken);

        var vertexControl = graphControl.Area.VertexList
            .Single(pair => pair.Key.Id == client.Id)
            .Value;
        vertexControl.ContextMenu.ShouldNotBeNull();
        var viewItem = vertexControl.ContextMenu!.Items.OfType<MenuItem>().Single();
        viewItem.Header.ShouldBe(Assets.Resources.ServiceMapGraphControl_View);
        viewItem.Command.ShouldNotBeNull();
        viewItem.Command!.CanExecute(viewItem.CommandParameter).ShouldBeTrue();
        viewItem.Command.Execute(viewItem.CommandParameter);

        rightDock.VisibleDockables.ShouldHaveSingleItem();
        var properties = rightDock.VisibleDockables.Single().ShouldBeOfType<ServiceMapNodePropertiesViewModel>();
        properties.Node.ShouldBeSameAs(client);
        properties.AccessPoints.ShouldContain("TCP:8080");
        properties.AccessPoints.ShouldContain("4 flows");

        viewItem.Command.Execute(null);
        rightDock.VisibleDockables.ShouldHaveSingleItem();
        rightDock.VisibleDockables.Single().ShouldBeOfType<ServiceMapNodePropertiesViewModel>()
            .Node.ShouldBeSameAs(client);
    }

    [AvaloniaFact]
    public async Task Service_map_node_properties_view_shows_aggregated_hubble_details()
    {
        var node = new ServiceMapNode
        {
            Id = "team-a/client",
            Name = "client",
            Namespace = "team-a",
            WorkloadType = "Deployment",
            IngressFlowCount = 3,
            EgressFlowCount = 5,
            AllowedCount = 7,
            DeniedCount = 1,
        };
        node.AccessPoints.Add(new ServiceMapAccessPoint
        {
            Protocol = "TCP",
            Port = 8080,
            FlowCount = 4,
            DeniedCount = 1,
        });
        node.AccessPoints[0].ApplicationProtocols.Add("HTTP");
        var viewModel = new ServiceMapNodePropertiesViewModel();
        viewModel.Initialize(node);
        var view = new ServiceMapNodePropertiesView { DataContext = viewModel };
        using var window = Application.Current.CreateTestWindow(900, 600, view);
        window.Show();
        await TestApplicationExtensions.WaitForUiAsync(TestContext.Current.CancellationToken);

        var properties = view.GetVisualDescendants()
            .OfType<PropertyItem>()
            .ToDictionary(static item => item.Key, static item => item.Value?.ToString());
        properties[Assets.Resources.ServiceMapNodePropertiesView_Id!].ShouldBe(node.Id);
        properties[Assets.Resources.ServiceMapNodePropertiesView_Name!].ShouldBe(node.Name);
        properties[Assets.Resources.ServiceMapNodePropertiesView_Namespace!].ShouldBe(node.Namespace);
        properties[Assets.Resources.ServiceMapNodePropertiesView_WorkloadType!].ShouldBe(node.WorkloadType);
        properties[Assets.Resources.ServiceMapNodePropertiesView_IngressFlows!].ShouldBe("3");
        properties[Assets.Resources.ServiceMapNodePropertiesView_EgressFlows!].ShouldBe("5");
        properties[Assets.Resources.ServiceMapNodePropertiesView_AllowedFlows!].ShouldBe("7");
        properties[Assets.Resources.ServiceMapNodePropertiesView_DeniedFlows!].ShouldBe("1");
        var accessPoints = properties[Assets.Resources.ServiceMapNodePropertiesView_AccessPoints!];
        accessPoints.ShouldContain("TCP:8080");
        accessPoints.ShouldContain("4 flows");
        accessPoints.ShouldContain("HTTP");
    }

    [AvaloniaFact]
    public async Task Layout_and_routing_selection_refit_service_map_viewport()
    {
        var graph = new ServiceMapGraph();
        graph.Nodes.Add(new ServiceMapNode { Id = "team-a/client", Name = "client", Namespace = "team-a" });
        graph.Nodes.Add(new ServiceMapNode { Id = "team-b/server", Name = "server", Namespace = "team-b" });
        graph.Edges.Add(new ServiceMapEdge { SourceId = "team-a/client", TargetId = "team-b/server", FlowCount = 1 });

        var graphControl = new ServiceMapGraphControl { Graph = graph };
        using var window = Application.Current.CreateTestWindow(1200, 800, graphControl);
        window.Show();
        await TestWait.UntilAsync(
            () => graphControl.HasGeneratedGraph && graphControl.LayoutTask?.IsCompleted == true,
            timeoutMs: 10_000,
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
        await graphControl.LayoutTask!.WaitAsync(TestContext.Current.CancellationToken);

        graphControl.ZoomControl.Zoom = 0.5;
        graphControl.ZoomControl.TranslateX = 400;
        graphControl.ZoomControl.TranslateY = 300;
        graphControl.LayoutAlgorithm = LayoutAlgorithmTypeEnum.Circular;
        await graphControl.LayoutTask!.WaitAsync(TestContext.Current.CancellationToken);
        graphControl.ZoomControl.Zoom.ShouldNotBe(0.5);

        graphControl.ZoomControl.Zoom = 0.5;
        graphControl.ZoomControl.TranslateX = 400;
        graphControl.ZoomControl.TranslateY = 300;
        graphControl.EdgeRoutingAlgorithm = EdgeRoutingAlgorithmTypeEnum.Bundling;
        await graphControl.LayoutTask!.WaitAsync(TestContext.Current.CancellationToken);
        graphControl.ZoomControl.Zoom.ShouldNotBe(0.5);
    }

    [AvaloniaFact]
    public async Task Layout_selection_changes_positions_for_cross_namespace_traffic()
    {
        var graph = new ServiceMapGraph();
        graph.Nodes.Add(new ServiceMapNode { Id = "a/client", Name = "client", Namespace = "a" });
        graph.Nodes.Add(new ServiceMapNode { Id = "b/api", Name = "api", Namespace = "b" });
        graph.Nodes.Add(new ServiceMapNode { Id = "c/server", Name = "server", Namespace = "c" });
        graph.Edges.Add(new ServiceMapEdge { SourceId = "a/client", TargetId = "b/api", FlowCount = 1 });
        graph.Edges.Add(new ServiceMapEdge { SourceId = "b/api", TargetId = "c/server", FlowCount = 1 });

        var graphControl = new ServiceMapGraphControl { Graph = graph };
        using var window = Application.Current.CreateTestWindow(1200, 800, graphControl);
        window.Show();
        await TestWait.UntilAsync(
            () => graphControl.HasGeneratedGraph && graphControl.LayoutTask?.IsCompleted == true,
            timeoutMs: 10_000,
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());

        graphControl.LayoutAlgorithm = LayoutAlgorithmTypeEnum.Circular;
        await graphControl.LayoutTask!.WaitAsync(TestContext.Current.CancellationToken);
        var circularPositions = graphControl.Area.GetVertexPositions();
        graphControl.LayoutAlgorithm = LayoutAlgorithmTypeEnum.SimpleRandom;
        await graphControl.LayoutTask!.WaitAsync(TestContext.Current.CancellationToken);
        graphControl.Area.GetVertexPositions().Any(pair => pair.Value != circularPositions[pair.Key]).ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task GraphX_layout_and_edge_routing_options_can_be_selected()
    {
        var graph = new ServiceMapGraph();
        graph.Nodes.Add(new ServiceMapNode { Id = "team-a/client", Name = "client", Namespace = "team-a" });
        graph.Nodes.Add(new ServiceMapNode { Id = "team-a/api", Name = "api", Namespace = "team-a" });
        graph.Nodes.Add(new ServiceMapNode { Id = "team-b/worker", Name = "worker", Namespace = "team-b" });
        graph.Nodes.Add(new ServiceMapNode { Id = "world/dns", Name = "dns" });
        graph.Edges.Add(new ServiceMapEdge { SourceId = "team-a/client", TargetId = "team-a/api", FlowCount = 1 });
        graph.Edges.Add(new ServiceMapEdge { SourceId = "team-a/api", TargetId = "team-b/worker", FlowCount = 1 });
        graph.Edges.Add(new ServiceMapEdge { SourceId = "team-b/worker", TargetId = "world/dns", FlowCount = 1 });

        var graphControl = new ServiceMapGraphControl { Graph = graph };
        using var window = Application.Current.CreateTestWindow(1200, 800, graphControl);
        window.Show();
        await TestWait.UntilAsync(
            () => graphControl.HasGeneratedGraph && graphControl.LayoutTask?.IsCompleted == true,
            timeoutMs: 10_000,
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
        await graphControl.LayoutTask!.WaitAsync(TestContext.Current.CancellationToken);

        var overlapParameters = graphControl.Area.LogicCore!.DefaultOverlapRemovalAlgorithmParams
            .ShouldBeOfType<OverlapRemovalParameters>();
        overlapParameters.HorizontalGap.ShouldBeGreaterThanOrEqualTo(100);
        overlapParameters.VerticalGap.ShouldBeGreaterThanOrEqualTo(100);

        var beforeLayout = graphControl.Area.GetVertexPositions();
        var beforeEdgeBounds = graphControl.Area.GetVisualDescendants()
            .OfType<EdgeControl>()
            .First()
            .GeometryBounds;
        graphControl.LayoutAlgorithm = LayoutAlgorithmTypeEnum.SimpleRandom;
        await graphControl.LayoutTask!.WaitAsync(TestContext.Current.CancellationToken);
        var afterLayout = graphControl.Area.GetVertexPositions();
        afterLayout.Any(pair => pair.Value != beforeLayout[pair.Key]).ShouldBeTrue();
        graphControl.Area.GetVisualDescendants()
            .OfType<EdgeControl>()
            .First()
            .GeometryBounds.ShouldNotBe(beforeEdgeBounds);

        foreach (var algorithm in Enum.GetValues<LayoutAlgorithmTypeEnum>())
        {
            if (algorithm == LayoutAlgorithmTypeEnum.Custom)
                continue;

            graphControl.LayoutAlgorithm = algorithm;
            await graphControl.LayoutTask!.WaitAsync(TestContext.Current.CancellationToken);
            graphControl.ActiveExternalLayoutAlgorithm.ShouldBe(algorithm);
            graphControl.Area.GetVertexPositions().Values
                .ShouldAllBe(static position => double.IsFinite(position.X) && double.IsFinite(position.Y));
        }

        graphControl.LayoutAlgorithm = LayoutAlgorithmTypeEnum.CompoundFDP;
        await graphControl.LayoutTask!.WaitAsync(TestContext.Current.CancellationToken);
        foreach (var algorithm in Enum.GetValues<EdgeRoutingAlgorithmTypeEnum>())
        {
            graphControl.EdgeRoutingAlgorithm = algorithm;
            await graphControl.LayoutTask!.WaitAsync(TestContext.Current.CancellationToken);
            graphControl.GraphXEdgeCount.ShouldBe(graph.Edges.Count);
            graphControl.Area.LogicCore!.DefaultEdgeRoutingAlgorithm.ShouldBe(algorithm);
        }

        var compoundGraph = graphControl.Area.LogicCore!.Graph!;
        foreach (var node in graph.Nodes)
        {
            var vertex = compoundGraph.Vertices.Single(candidate => candidate.Id == node.Id);
            compoundGraph.GetParent(vertex).ShouldNotBeNull();
        }
    }

    [AvaloniaFact]
    public async Task Service_map_groups_namespace_nodes_and_external_nodes_in_compound_layout()
    {
        var graph = new ServiceMapGraph();
        graph.Nodes.Add(new ServiceMapNode { Id = "team-a/client", Name = "client", Namespace = "team-a" });
        graph.Nodes.Add(new ServiceMapNode { Id = "team-a/api", Name = "api", Namespace = "team-a" });
        graph.Nodes.Add(new ServiceMapNode { Id = "team-b/worker", Name = "worker", Namespace = "team-b" });
        graph.Nodes.Add(new ServiceMapNode { Id = "world/dns", Name = "dns" });
        graph.Nodes.Add(new ServiceMapNode { Id = "world/external", Name = "external" });
        graph.Edges.Add(new ServiceMapEdge { SourceId = "team-a/client", TargetId = "team-a/api", FlowCount = 1 });
        graph.Edges.Add(new ServiceMapEdge { SourceId = "team-a/api", TargetId = "team-b/worker", FlowCount = 1 });
        graph.Edges.Add(new ServiceMapEdge { SourceId = "team-a/client", TargetId = "world/dns", FlowCount = 1 });
        graph.Edges.Add(new ServiceMapEdge { SourceId = "world/dns", TargetId = "world/external", FlowCount = 1 });

        var graphControl = new ServiceMapGraphControl { Graph = graph };
        using var window = Application.Current.CreateTestWindow(1000, 700, graphControl);
        window.Show();
        await TestWait.UntilAsync(
            () => graphControl.HasGeneratedGraph,
            timeoutMs: 5_000,
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());

        graphControl.GraphXVertexCount.ShouldBe(graph.Nodes.Count);
        graphControl.Area.LogicCore!.DefaultEdgeRoutingAlgorithm.ShouldBe(EdgeRoutingAlgorithmTypeEnum.None);
        var compoundGraph = graphControl.Area.LogicCore!.Graph!
            .ShouldBeOfType<CompoundGraph<ServiceMapGraphControl.ServiceMapVertex, ServiceMapGraphControl.ServiceMapEdge>>();
        var vertices = compoundGraph.Vertices
            .Where(vertex => compoundGraph.GetParent(vertex) is not null)
            .ToDictionary(vertex => vertex.Id);
        var teamAParent = compoundGraph.GetParent(vertices["team-a/client"]);
        teamAParent.ShouldNotBeNull();
        compoundGraph.GetParent(vertices["team-a/api"]).ShouldBe(teamAParent);
        compoundGraph.GetParent(vertices["team-b/worker"]).ShouldNotBe(teamAParent);
        var worldParent = compoundGraph.GetParent(vertices["world/dns"]);
        worldParent.ShouldNotBe(teamAParent);
        compoundGraph.GetParent(vertices["world/external"]).ShouldBe(worldParent);
        compoundGraph.VertexCount.ShouldBe(graph.Nodes.Count + 3);
        graphControl.Area.VertexList.Count.ShouldBe(graph.Nodes.Count);

        var movedApi = new ServiceMapGraph();
        movedApi.Nodes.Add(new ServiceMapNode { Id = "team-a/client", Name = "client", Namespace = "team-a" });
        movedApi.Nodes.Add(new ServiceMapNode { Id = "team-a/api", Name = "api", Namespace = "team-b" });
        movedApi.Nodes.Add(new ServiceMapNode { Id = "team-b/worker", Name = "worker", Namespace = "team-b" });
        movedApi.Nodes.Add(new ServiceMapNode { Id = "world/dns", Name = "dns" });
        movedApi.Nodes.Add(new ServiceMapNode { Id = "world/external", Name = "external" });
        movedApi.Edges.Add(new ServiceMapEdge { SourceId = "team-a/client", TargetId = "team-a/api", FlowCount = 1 });
        movedApi.Edges.Add(new ServiceMapEdge { SourceId = "team-a/api", TargetId = "team-b/worker", FlowCount = 1 });
        movedApi.Edges.Add(new ServiceMapEdge { SourceId = "team-a/client", TargetId = "world/dns", FlowCount = 1 });
        movedApi.Edges.Add(new ServiceMapEdge { SourceId = "world/dns", TargetId = "world/external", FlowCount = 1 });
        graphControl.Graph = movedApi;
        await TestWait.UntilAsync(
            () =>
            {
                var updatedCompoundGraph = graphControl.Area.LogicCore!.Graph!;
                var updatedVertices = updatedCompoundGraph.Vertices
                    .Where(vertex => updatedCompoundGraph.GetParent(vertex) is not null)
                    .ToDictionary(vertex => vertex.Id);
                return ReferenceEquals(
                    updatedCompoundGraph.GetParent(updatedVertices["team-a/api"]),
                    updatedCompoundGraph.GetParent(updatedVertices["team-b/worker"]));
            },
            timeoutMs: 5_000,
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());

        var refreshedGraph = graphControl.Area.LogicCore!.Graph!;
        refreshedGraph.VertexCount.ShouldBe(graph.Nodes.Count + 3);
        graphControl.Area.VertexList.Count.ShouldBe(graph.Nodes.Count);
    }

    [AvaloniaFact]
    public async Task Grouped_layouts_keep_namespace_and_external_groups_separate()
    {
        var graph = CreateGroupedGraph();
        var graphControl = new ServiceMapGraphControl { Graph = graph };
        using var window = Application.Current.CreateTestWindow(1200, 800, graphControl);
        window.Show();
        await TestWait.UntilAsync(
            () => graphControl.HasGeneratedGraph && graphControl.LayoutTask?.IsCompleted == true,
            timeoutMs: 10_000,
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
        await graphControl.LayoutTask!.WaitAsync(TestContext.Current.CancellationToken);

        foreach (var algorithm in Enum.GetValues<LayoutAlgorithmTypeEnum>())
        {
            if (algorithm == LayoutAlgorithmTypeEnum.Custom)
            {
                continue;
            }

            graphControl.LayoutAlgorithm = algorithm;
            await graphControl.LayoutTask!.WaitAsync(TestContext.Current.CancellationToken);
            await TestApplicationExtensions.WaitForUiAsync(TestContext.Current.CancellationToken);

            var bounds = GetGroupBounds(graphControl);
            bounds.Keys.ShouldContain("team-a");
            bounds.Keys.ShouldContain("team-b");
            bounds.Keys.ShouldContain(string.Empty);
            AssertGroupsDoNotOverlap(bounds, algorithm);
        }
    }

    [AvaloniaFact]
    public async Task Grouped_layout_moves_nodes_to_their_updated_namespace()
    {
        var graph = CreateGroupedGraph();
        var graphControl = new ServiceMapGraphControl { Graph = graph };
        using var window = Application.Current.CreateTestWindow(1200, 800, graphControl);
        window.Show();
        await TestWait.UntilAsync(
            () => graphControl.HasGeneratedGraph && graphControl.LayoutTask?.IsCompleted == true,
            timeoutMs: 10_000,
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
        await graphControl.LayoutTask!.WaitAsync(TestContext.Current.CancellationToken);

        graphControl.LayoutAlgorithm = LayoutAlgorithmTypeEnum.Tree;
        await graphControl.LayoutTask!.WaitAsync(TestContext.Current.CancellationToken);
        await TestApplicationExtensions.WaitForUiAsync(TestContext.Current.CancellationToken);

        graphControl.Area.VertexList.Keys
            .Single(vertex => vertex.Id == "team-a/api")
            .GroupKey.ShouldBe("team-a");

        var movedGraph = new ServiceMapGraph();
        movedGraph.Nodes.Add(new ServiceMapNode { Id = "team-a/client", Name = "client", Namespace = "team-a" });
        movedGraph.Nodes.Add(new ServiceMapNode { Id = "team-a/api", Name = "api", Namespace = "team-b" });
        movedGraph.Nodes.Add(new ServiceMapNode { Id = "team-b/worker", Name = "worker", Namespace = "team-b" });
        movedGraph.Edges.Add(new ServiceMapEdge { SourceId = "team-a/client", TargetId = "team-a/api", FlowCount = 1 });
        movedGraph.Edges.Add(new ServiceMapEdge { SourceId = "team-a/api", TargetId = "team-b/worker", FlowCount = 1 });
        graphControl.Graph = movedGraph;

        await TestWait.UntilAsync(
            () =>
            {
                var vertex = graphControl.Area.VertexList.Keys
                    .SingleOrDefault(candidate => candidate.Id == "team-a/api");
                return vertex?.GroupKey == "team-b" && graphControl.LayoutTask?.IsCompleted == true;
            },
            timeoutMs: 5_000,
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());
        await graphControl.LayoutTask!.WaitAsync(TestContext.Current.CancellationToken);
        await TestApplicationExtensions.WaitForUiAsync(TestContext.Current.CancellationToken);

        graphControl.Area.VertexList.Keys
            .Single(vertex => vertex.Id == "team-a/api")
            .GroupKey.ShouldBe("team-b");
        AssertGroupsDoNotOverlap(GetGroupBounds(graphControl), LayoutAlgorithmTypeEnum.Tree);
    }

    [AvaloniaFact]
    public async Task Reverse_traffic_keeps_directed_edges_with_one_arrow_each()
    {
        var graph = new ServiceMapGraph();
        graph.Nodes.Add(new ServiceMapNode { Id = "team-a/client", Name = "client", Namespace = "team-a" });
        graph.Nodes.Add(new ServiceMapNode { Id = "team-b/server", Name = "server", Namespace = "team-b" });
        graph.Nodes[1].AccessPoints.Add(new ServiceMapAccessPoint
        {
            Protocol = "TCP",
            Port = 443,
            FlowCount = 4
        });
        graph.Edges.Add(new ServiceMapEdge
        {
            SourceId = "team-a/client",
            TargetId = "team-b/server",
            FlowCount = 2,
            DominantVerdict = FlowVerdict.Forwarded
        });
        graph.Edges.Add(new ServiceMapEdge
        {
            SourceId = "team-b/server",
            TargetId = "team-a/client",
            FlowCount = 3,
            DominantVerdict = FlowVerdict.Forwarded
        });

        var graphControl = new ServiceMapGraphControl { Graph = graph };
        using var window = Application.Current.CreateTestWindow(800, 600, graphControl);
        window.Show();
        await TestWait.UntilAsync(
            () => graphControl.HasGeneratedGraph,
            timeoutMs: 5_000,
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());

        graphControl.GraphXEdgeCount.ShouldBe(2);
        var edgeControls = graphControl.Area.GetVisualDescendants().OfType<EdgeControl>().ToArray();
        edgeControls.Length.ShouldBe(2);
        foreach (var edgeControl in edgeControls)
        {
            var arrows = edgeControl.GetVisualDescendants().OfType<DefaultEdgePointer>().ToArray();
            arrows.ShouldHaveSingleItem().IsVisible.ShouldBeTrue();
        }
    }

    [AvaloniaFact]
    [Obsolete]
    public async Task Dense_mock_graph_layout_preview()
    {
        var graph = CreateDenseMockGraph();
        var graphControl = new ServiceMapGraphControl { Graph = graph };
        using var window = Application.Current.CreateTestWindow(1400, 900, graphControl);
        window.Show();
        await TestWait.UntilAsync(
            () => graphControl.HasGeneratedGraph,
            timeoutMs: 10_000,
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());

        var positions = graphControl.Area.GetVertexPositions();
        positions.Count.ShouldBe(84);
        positions.Values.Distinct().Count().ShouldBe(positions.Count);
        positions.Values.ShouldAllBe(static point => double.IsFinite(point.X) && double.IsFinite(point.Y));
        graphControl.Area.GetVisualDescendants().OfType<EdgeControl>().ShouldAllBe(static edge => edge.ShowArrows);
        graphControl.Area.VertexList.Values.ShouldAllBe(static vertex =>
            vertex.GetValue(DragBehaviour.IsDragEnabledProperty));
        var nodeLabels = graphControl.Area.VertexList.Values.First()
            .GetVisualDescendants()
            .OfType<TextBlock>()
            .Take(2)
            .ToArray();
        nodeLabels.Length.ShouldBe(2);
        nodeLabels.ShouldAllBe(static label => label.TextAlignment == TextAlignment.Center);
        nodeLabels.ShouldAllBe(static label => label.TextWrapping == TextWrapping.Wrap);

        if (Environment.GetEnvironmentVariable("KUBEUI_CAPTURE_SERVICE_MAP_SCREENSHOT") == "1")
        {
            using var bitmap = new RenderTargetBitmap(new PixelSize(1400, 900), new Vector(96, 96));
            bitmap.Render(window);
            var screenshotPath = Environment.GetEnvironmentVariable("KUBEUI_SERVICE_MAP_SCREENSHOT_PATH")
                ?? Path.Combine(Path.GetTempPath(), "kubeui-service-map-mock.png");
            bitmap.Save(screenshotPath);
        }
    }

    private static ServiceMapGraph CreateGroupedGraph()
    {
        var graph = new ServiceMapGraph();
        graph.Nodes.Add(new ServiceMapNode { Id = "team-a/client", Name = "client", Namespace = "team-a" });
        graph.Nodes.Add(new ServiceMapNode { Id = "team-a/api", Name = "api", Namespace = "team-a" });
        graph.Nodes.Add(new ServiceMapNode { Id = "team-b/worker", Name = "worker", Namespace = "team-b" });
        graph.Nodes.Add(new ServiceMapNode { Id = "team-b/queue", Name = "queue", Namespace = "team-b" });
        graph.Nodes.Add(new ServiceMapNode { Id = "world/dns", Name = "dns.example" });
        graph.Nodes.Add(new ServiceMapNode { Id = "world/external", Name = "203.0.113.10" });
        graph.Edges.Add(new ServiceMapEdge { SourceId = "team-a/client", TargetId = "team-a/api", FlowCount = 1 });
        graph.Edges.Add(new ServiceMapEdge { SourceId = "team-b/worker", TargetId = "team-b/queue", FlowCount = 1 });
        graph.Edges.Add(new ServiceMapEdge { SourceId = "world/dns", TargetId = "world/external", FlowCount = 1 });
        graph.Edges.Add(new ServiceMapEdge { SourceId = "team-a/api", TargetId = "team-b/worker", FlowCount = 1 });
        graph.Edges.Add(new ServiceMapEdge { SourceId = "team-b/queue", TargetId = "world/dns", FlowCount = 1 });
        return graph;
    }

    private static Dictionary<string, Rect> GetGroupBounds(ServiceMapGraphControl graphControl)
    {
        var sizes = graphControl.Area.GetVertexSizesAndPositions(out var positions);
        var bounds = new Dictionary<string, Rect>(StringComparer.Ordinal);
        foreach (var pair in graphControl.Area.VertexList)
        {
            var position = positions[pair.Key];
            var size = sizes[pair.Key];
            var vertexBounds = new Rect(position.X, position.Y, size.Width, size.Height);
            bounds[pair.Key.GroupKey] = bounds.TryGetValue(pair.Key.GroupKey, out var existing)
                ? existing.Union(vertexBounds)
                : vertexBounds;
        }

        return bounds;
    }

    private static void AssertGroupsDoNotOverlap(
        IReadOnlyDictionary<string, Rect> bounds,
        LayoutAlgorithmTypeEnum algorithm)
    {
        var groupKeys = bounds.Keys.ToArray();
        for (var leftIndex = 0; leftIndex < groupKeys.Length; leftIndex++)
        {
            for (var rightIndex = leftIndex + 1; rightIndex < groupKeys.Length; rightIndex++)
            {
                bounds[groupKeys[leftIndex]].Intersects(bounds[groupKeys[rightIndex]]).ShouldBeFalse(
                    $"{algorithm}: groups '{groupKeys[leftIndex]}' {bounds[groupKeys[leftIndex]]} and '{groupKeys[rightIndex]}' {bounds[groupKeys[rightIndex]]} overlap.");
            }
        }
    }

    private static ServiceMapGraph CreateDenseMockGraph()
    {
        var graph = new ServiceMapGraph();
        var namespaces = Enumerable.Range(0, 20).Select(static index => $"team-{index:D2}").ToArray();
        foreach (var ns in namespaces)
        {
            for (var i = 0; i < 4; i++)
                graph.Nodes.Add(new ServiceMapNode { Id = $"{ns}/pod-{i:D2}", Name = $"pod-{i:D2}", Namespace = ns });
        }
        graph.Nodes.Add(new ServiceMapNode { Id = "external/client-a", Name = "client-a" });
        graph.Nodes.Add(new ServiceMapNode { Id = "external/client-b", Name = "client-b" });
        graph.Nodes.Add(new ServiceMapNode { Id = "external/payment-api", Name = "payment-api" });
        graph.Nodes.Add(new ServiceMapNode { Id = "external/public-api", Name = "public-api" });

        for (var i = 0; i < 4; i++)
        {
            for (var ns = 0; ns < namespaces.Length; ns++)
            {
                AddEdge(graph, namespaces[ns], i, namespaces[(ns + 1) % namespaces.Length], (i + ns) % 4);
                AddEdge(graph, namespaces[ns], i, namespaces[(ns + 3) % namespaces.Length], (i + 1) % 4);
            }
        }
        for (var i = 0; i < 10; i++)
        {
            var pod = i % 4;
            var ns = namespaces[i % namespaces.Length];
            AddEdge(graph, "external/client-a", $"{ns}/pod-{pod:D2}");
            AddEdge(graph, "external/client-b", $"{ns}/pod-{(pod + 1) % 4:D2}");
            AddEdge(graph, $"{ns}/pod-{pod:D2}", "external/payment-api");
            AddEdge(graph, $"{ns}/pod-{pod:D2}", "external/public-api");
        }
        return graph;
    }

    private static void AddEdge(ServiceMapGraph graph, string sourceNamespace, int source, string targetNamespace, int target)
        => AddEdge(graph, $"{sourceNamespace}/pod-{source:D2}", $"{targetNamespace}/pod-{target:D2}");

    private static void AddEdge(ServiceMapGraph graph, string sourceId, string targetId)
    {
        var source = graph.Nodes.Single(node => node.Id == sourceId);
        var target = graph.Nodes.Single(node => node.Id == targetId);
        source.EgressFlowCount++;
        target.IngressFlowCount++;
        graph.Edges.Add(new ServiceMapEdge
        {
            SourceId = sourceId,
            TargetId = targetId,
            FlowCount = 1,
            DominantVerdict = FlowVerdict.Forwarded,
        });
    }

    [AvaloniaFact]
    [Obsolete]
    public async Task View_loads_empty_map_and_updates_when_graph_changes()
    {
        var services = Application.Current.GetTestServices();
        await using var viewModel = services.GetRequiredService<ServiceMapViewModel>();
        var view = new ServiceMapView { DataContext = viewModel };

        using var window = Application.Current.CreateTestWindow(content: view);
        window.Show();
        await TestApplicationExtensions.WaitForUiAsync(TestContext.Current.CancellationToken);

        var graphControl = view.GetVisualDescendants().OfType<ServiceMapGraphControl>().Single();
        graphControl.GetVisualAncestors().OfType<ScrollViewer>().ShouldBeEmpty();
        ReferenceEquals(graphControl.Graph, viewModel.Graph).ShouldBeTrue();
        var emptyState = view.GetVisualDescendants()
            .OfType<TextBlock>()
            .Single(textBlock => textBlock.Text == Assets.Resources.ServiceMapView_SelectNamespacePrompt);
        emptyState.IsVisible.ShouldBeTrue();
        var graphSummary = view.GetVisualDescendants()
            .OfType<TextBlock>()
            .Single(textBlock => textBlock.Text == viewModel.GraphSummary);
        view.GetVisualDescendants().OfType<ToggleButton>().Count().ShouldBe(3);
        var comboBoxes = view.GetVisualDescendants().OfType<ComboBox>().ToArray();
        comboBoxes.Length.ShouldBe(2);
        var timeWindowSelector = comboBoxes.Single(comboBox => comboBox.SelectedItem is TimeSpan);
        timeWindowSelector.Bounds.Height.ShouldBe(32);
        var layoutSelector = comboBoxes.Single(comboBox => comboBox.SelectedItem is LayoutAlgorithmTypeEnum);
        layoutSelector.SelectedItem.ShouldBe(LayoutAlgorithmTypeEnum.CompoundFDP);
        viewModel.EdgeRoutingAlgorithm.ShouldBe(EdgeRoutingAlgorithmTypeEnum.None);
        graphControl.EdgeRoutingAlgorithm.ShouldBe(EdgeRoutingAlgorithmTypeEnum.None);
        graphControl.Area.LogicCore!.DefaultEdgeRoutingAlgorithm.ShouldBe(EdgeRoutingAlgorithmTypeEnum.None);
        layoutSelector.ItemsSource.ShouldBeAssignableTo<IEnumerable<LayoutAlgorithmTypeEnum>>().Count()
            .ShouldBe(Enum.GetValues<LayoutAlgorithmTypeEnum>().Length - 1);
        layoutSelector.SelectedItem = LayoutAlgorithmTypeEnum.Circular;
        viewModel.LayoutAlgorithm.ShouldBe(LayoutAlgorithmTypeEnum.Circular);

        var graph = new ServiceMapGraph();
        graph.Nodes.Add(new ServiceMapNode { Id = "team-a/client", Name = "client", Namespace = "team-a" });
        graph.Nodes.Add(new ServiceMapNode { Id = "team-b/server", Name = "server", Namespace = "team-b" });
        graph.Edges.Add(new ServiceMapEdge
        {
            SourceId = "team-a/client",
            TargetId = "team-b/server",
            FlowCount = 2,
            DominantVerdict = FlowVerdict.Forwarded
        });
        graph.Edges[0].AccessPoints.Add(new ServiceMapAccessPoint
        {
            Protocol = "TCP",
            Port = 8080,
            FlowCount = 2
        });
        graph.Nodes.Single(node => node.Id == "team-b/server").AccessPoints.Add(new ServiceMapAccessPoint
        {
            Protocol = "TCP",
            Port = 8080,
            FlowCount = 2
        });

        viewModel.Graph = graph;
        await TestApplicationExtensions.WaitForUiAsync(TestContext.Current.CancellationToken);
        await TestWait.UntilAsync(
            () => graphControl.HasGeneratedGraph
                && graphControl.GraphXVertexCount == 2
                && graphControl.GraphXEdgeCount == 1
                && graphControl.Area.GetVisualDescendants().OfType<EdgeControl>().Count() == 1,
            timeoutMs: 5_000,
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());

        ReferenceEquals(graphControl.Graph, graph).ShouldBeTrue();
        graphControl.LayoutAlgorithm.ShouldBe(LayoutAlgorithmTypeEnum.Circular);
        graphControl.EdgeRoutingAlgorithm.ShouldBe(EdgeRoutingAlgorithmTypeEnum.None);
        graphControl.GraphXVertexCount.ShouldBe(2);
        graphControl.GraphXEdgeCount.ShouldBe(1);
        var serverVertex = graphControl.Area.VertexList.Values
            .Single(vertex => ((ServiceMapGraphControl.ServiceMapVertex)vertex.DataContext!).Node.Id == "team-b/server");
        var serverNodeBorder = serverVertex.GetVisualDescendants()
            .OfType<Border>()
            .Single(border => border.Classes.Contains("ServiceMapNode"));
        ToolTip.GetTip(serverNodeBorder)!.ToString().ShouldContain("TCP:8080");
        ToolTip.GetTip(graphControl.Area.GetVisualDescendants().OfType<EdgeControl>().Single())
            .ShouldBe(Assets.Resources.ServiceMapGraphControl_EdgeTooltipFormat
                .Replace("{0}", "2", StringComparison.Ordinal)
                .Replace("{1}", "TCP 8080 (2)", StringComparison.Ordinal));
        emptyState.IsVisible.ShouldBeFalse();
        graphSummary.Text.ShouldBe(viewModel.GraphSummary);
        viewModel.HasFlows.ShouldBeTrue();
        viewModel.Nodes.Count.ShouldBe(2);

        var updatedGraph = new ServiceMapGraph();
        updatedGraph.Nodes.Add(new ServiceMapNode
        {
            Id = "team-a/client",
            Name = "client",
            Namespace = "team-a",
            EgressFlowCount = 3
        });
        var updatedServer = new ServiceMapNode
        {
            Id = "team-b/server",
            Name = "server",
            Namespace = "team-b",
            IngressFlowCount = 3
        };
        updatedServer.AccessPoints.Add(new ServiceMapAccessPoint
        {
            Protocol = "TCP",
            Port = 8443,
            FlowCount = 3
        });
        updatedGraph.Nodes.Add(updatedServer);
        updatedGraph.Edges.Add(new ServiceMapEdge
        {
            SourceId = "team-a/client",
            TargetId = "team-b/server",
            FlowCount = 3,
            DominantVerdict = FlowVerdict.Forwarded
        });

        viewModel.Graph = updatedGraph;
        await TestWait.UntilAsync(
            () => graphControl.Area.GetVisualDescendants()
                .OfType<Border>()
                .Any(border => border.Classes.Contains("ServiceMapNode")
                    && ToolTip.GetTip(border)?.ToString().Contains("TCP:8443", StringComparison.Ordinal) == true),
            timeoutMs: 5_000,
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());

        var updatedServerVertex = graphControl.Area.VertexList.Values
            .Single(vertex => ((ServiceMapGraphControl.ServiceMapVertex)vertex.DataContext!).Node.Id == "team-b/server");
        var updatedNodeBorder = updatedServerVertex.GetVisualDescendants()
            .OfType<Border>()
            .Single(border => border.Classes.Contains("ServiceMapNode"));
        updatedNodeBorder.Width.ShouldBe(200);
        updatedNodeBorder.MinHeight.ShouldBe(84);
        updatedNodeBorder.BorderThickness.ShouldBe(new Thickness(1.5));

        if (Environment.GetEnvironmentVariable("KUBEUI_CAPTURE_SERVICE_MAP_SCREENSHOT") == "1")
        {
            using var bitmap = new RenderTargetBitmap(new PixelSize(1400, 900), new Vector(96, 96));
            bitmap.Render(window);
            var screenshotPath = Environment.GetEnvironmentVariable("KUBEUI_SERVICE_MAP_SCREENSHOT_PATH")
                ?? Path.Combine(Path.GetTempPath(), "kubeui-service-map-toolbar.png");
            bitmap.Save(screenshotPath);
        }

    }

    private sealed class RecordingResourceIconService : IResourceIconService
    {
        private readonly ResourceIconService _inner = new();

        public List<GroupApiVersionKind> RequestedKinds { get; } = [];

        public IImage GetIcon(GroupApiVersionKind resourceKind)
        {
            RequestedKinds.Add(resourceKind);
            return _inner.GetIcon(resourceKind);
        }
    }
}
