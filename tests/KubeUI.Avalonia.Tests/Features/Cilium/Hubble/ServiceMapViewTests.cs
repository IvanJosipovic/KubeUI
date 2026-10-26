using System.IO;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless.XUnit;
using Avalonia.Media.Imaging;
using Avalonia.Threading;
using Avalonia.VisualTree;
using KubeUI.Avalonia.Features.Cilium.Hubble;
using KubeUI.Avalonia.Features.Cilium.Hubble.Controls;
using KubeUI.Avalonia.Tests.Infra;
using KubeUI.Cilium.Hubble;
using KubeUI.Testing.Utilities;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;
using Westermo.GraphX.Controls.Behaviours;
using Westermo.GraphX.Controls.Controls;
using Westermo.GraphX.Controls.Controls.EdgePointers;

namespace KubeUI.Avalonia.Tests.Features.Cilium.Hubble;

public sealed class ServiceMapViewTests
{
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
            .Take(3)
            .ToArray();
        nodeLabels.Length.ShouldBe(3);
        nodeLabels[0].FontSize.ShouldBe(14);
        nodeLabels[1].FontSize.ShouldBe(12);
        nodeLabels[1].Opacity.ShouldBe(0.88);
        nodeLabels[2].FontSize.ShouldBe(11);

        if (Environment.GetEnvironmentVariable("KUBEUI_CAPTURE_SERVICE_MAP_SCREENSHOT") == "1")
        {
            using var bitmap = new RenderTargetBitmap(new PixelSize(1400, 900), new Vector(96, 96));
            bitmap.Render(window);
            var screenshotPath = Environment.GetEnvironmentVariable("KUBEUI_SERVICE_MAP_SCREENSHOT_PATH")
                ?? Path.Combine(Path.GetTempPath(), "kubeui-service-map-mock.png");
            bitmap.Save(screenshotPath);
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
        var timeWindowSelector = view.GetVisualDescendants().OfType<ComboBox>().ShouldHaveSingleItem();
        timeWindowSelector.Bounds.Height.ShouldBe(32);

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
        graphControl.GraphXVertexCount.ShouldBe(2);
        graphControl.GraphXEdgeCount.ShouldBe(1);
        graphControl.Area.VertexList.Values
            .Single(vertex => ((ServiceMapGraphControl.ServiceMapVertex)vertex.DataContext!).Node.Id == "team-b/server")
            .GetVisualDescendants()
            .OfType<TextBlock>()
            .Select(textBlock => textBlock.Text)
            .ShouldContain("→ 8080 • TCP");
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
                    && border.GetVisualDescendants().OfType<TextBlock>()
                        .Any(textBlock => textBlock.Text == "→ 8443 • TCP")),
            timeoutMs: 5_000,
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs());

        var updatedServerVertex = graphControl.Area.VertexList.Values
            .Single(vertex => ((ServiceMapGraphControl.ServiceMapVertex)vertex.DataContext!).Node.Id == "team-b/server");
        var updatedNodeBorder = updatedServerVertex.GetVisualDescendants()
            .OfType<Border>()
            .Single(border => border.Classes.Contains("ServiceMapNode"));
        updatedNodeBorder.BorderThickness.ShouldBe(new Thickness(1.5));
        updatedNodeBorder.CornerRadius.ShouldBe(new CornerRadius(8));

        if (Environment.GetEnvironmentVariable("KUBEUI_CAPTURE_SERVICE_MAP_SCREENSHOT") == "1")
        {
            using var bitmap = new RenderTargetBitmap(new PixelSize(1400, 900), new Vector(96, 96));
            bitmap.Render(window);
            var screenshotPath = Environment.GetEnvironmentVariable("KUBEUI_SERVICE_MAP_SCREENSHOT_PATH")
                ?? Path.Combine(Path.GetTempPath(), "kubeui-service-map-toolbar.png");
            bitmap.Save(screenshotPath);
        }

    }
}
