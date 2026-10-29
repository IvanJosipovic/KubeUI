using System.Collections.Generic;
using System.Threading;
using Avalonia;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.VisualTree;
using KubeUI.Avalonia.Tests.Infra;
using KubeUI.Cilium.Hubble;
using KubeUI.Testing.Utilities;
using QuikGraph;
using Shouldly;
using Westermo.GraphX.Common.Models;
using Westermo.GraphX.Controls.Controls;
using Westermo.GraphX.Controls.Models.Interfaces;
using Westermo.GraphX.Logic.Algorithms.LayoutAlgorithms;
using Westermo.GraphX.Logic.Algorithms.LayoutAlgorithms.Grouped;
using Westermo.GraphX.Logic.Models;
using GraphPoint = Westermo.GraphX.Measure.Point;
using GraphSize = Westermo.GraphX.Measure.Size;
using GraphThickness = Westermo.GraphX.Measure.Thickness;

namespace KubeUI.Avalonia.Tests.Features.Cilium.Hubble;

public sealed class ServiceMapGroupingLayoutPrototypeTests
{
    [AvaloniaFact]
    public async Task Compound_external_layout_keeps_compound_membership_and_hides_group_vertices()
    {
        var (graph, groups) = CreateCompoundGraph();
        foreach (var group in groups)
            group.SkipProcessing = Westermo.GraphX.Common.Enums.ProcessingOptionEnum.Exclude;

        var layout = new CompoundFdpExternalLayout(graph);
        var core = new GXLogicCore<LayoutVertex, LayoutEdge, CompoundGraph<LayoutVertex, LayoutEdge>>
        {
            Graph = graph,
            DefaultLayoutAlgorithm = Westermo.GraphX.Common.Enums.LayoutAlgorithmTypeEnum.Custom,
            DefaultOverlapRemovalAlgorithm = Westermo.GraphX.Common.Enums.OverlapRemovalAlgorithmTypeEnum.None,
            ExternalLayoutAlgorithm = layout,
            AsyncAlgorithmCompute = true
        };
        var area = new GraphArea<LayoutVertex, LayoutEdge, CompoundGraph<LayoutVertex, LayoutEdge>>
        {
            LogicCore = core
        };
        area.ControlFactory = new TestGraphControlFactory(area);
        using var window = Application.Current.CreateTestWindow(900, 700, area);
        window.Show();

        await area.GenerateGraph(
            generateAllEdges: true,
            cancellation: TestContext.Current.CancellationToken);

        area.VertexList.Count.ShouldBe(6);
        area.VertexList.Keys.ShouldAllBe(vertex => !groups.Contains(vertex));
        graph.IsCompoundVertex(groups[0]).ShouldBeTrue();
        graph.GetChildrenVertices(groups[0]).Count().ShouldBe(2);
        layout.VertexPositions.Keys.ShouldBeSubsetOf(area.VertexList.Keys);
        area.GetVisualDescendants().OfType<EdgeControl>().Count().ShouldBe(graph.EdgeCount);

        var addedNode = new LayoutVertex("team-a/added");
        graph.AddChildVertex(groups[0], addedNode);
        graph.AddEdge(new LayoutEdge(addedNode, graph.GetChildrenVertices(groups[0]).First()));
        await area.GenerateGraph(
            graph,
            generateAllEdges: true,
            cancellation: TestContext.Current.CancellationToken);

        area.VertexList.Count.ShouldBe(7);
        graph.GetChildrenVertices(groups[0]).Count().ShouldBe(3);
        area.VertexList.Keys.ShouldAllBe(vertex => !groups.Contains(vertex));
        area.GetVisualDescendants().OfType<EdgeControl>().Count().ShouldBe(graph.EdgeCount);
    }

    [Fact]
    public void Grouping_layout_places_namespace_and_world_groups()
    {
        var graph = CreateFlatGraph();
        var vertexSizes = graph.Vertices.ToDictionary(static vertex => vertex, static _ => new GraphSize(100, 56));
        var positions = new Dictionary<LayoutVertex, GraphPoint>();
        var groupParameters = new List<AlgorithmGroupParameters<LayoutVertex, LayoutEdge>>
        {
            CreateGroupParameters(graph, vertexSizes, 1),
            CreateGroupParameters(graph, vertexSizes, 2),
            CreateGroupParameters(graph, vertexSizes, 3)
        };
        var layout = new GroupingLayoutAlgorithm<LayoutVertex, LayoutEdge, BidirectionalGraph<LayoutVertex, LayoutEdge>>(
            graph,
            positions,
            new GroupingLayoutAlgorithmParameters<LayoutVertex, LayoutEdge>(groupParameters, arrangeGroups: true))
        {
            VertexSizes = vertexSizes
        };

        layout.Compute(CancellationToken.None);

        layout.VertexPositions.Count.ShouldBe(graph.VertexCount);
        foreach (var group in groupParameters)
        {
            var groupVertices = graph.Vertices.Where(vertex => vertex.GroupId == group.GroupId).ToArray();
            groupVertices.ShouldNotBeEmpty();
            groupVertices
                .ShouldAllBe(vertex => layout.VertexPositions.ContainsKey(vertex));
        }

    }

    [Fact]
    public void Compound_layout_accepts_namespace_and_world_parent_membership_with_cross_group_edges()
    {
        var (graph, groups) = CreateCompoundGraph();
        var vertexSizes = graph.Vertices.ToDictionary(
            static vertex => vertex,
            static vertex => vertex.IsGroup ? new GraphSize(180, 120) : new GraphSize(100, 56));
        var borders = graph.Vertices.ToDictionary(
            static vertex => vertex,
            static vertex => vertex.IsGroup ? new GraphThickness(12, 12, 12, 12) : default);
        var layoutTypes = graph.Vertices.ToDictionary(
            static vertex => vertex,
            static _ => CompoundVertexInnerLayoutType.Automatic);
        var layout = new CompoundFDPLayoutAlgorithm<LayoutVertex, LayoutEdge, CompoundGraph<LayoutVertex, LayoutEdge>>(
            graph,
            vertexSizes,
            borders,
            layoutTypes,
            new Dictionary<LayoutVertex, GraphPoint>(),
            new CompoundFDPLayoutParameters
            {
                Phase1Iterations = 5,
                Phase2Iterations = 5,
                Phase3Iterations = 5
            });

        layout.Compute(CancellationToken.None);

        graph.EdgeCount.ShouldBeGreaterThan(0);
        foreach (var group in groups)
        {
            graph.IsCompoundVertex(group).ShouldBeTrue();
            layout.InnerCanvasSizes.ContainsKey(group).ShouldBeTrue();
            graph.GetChildrenVertices(group).ShouldNotBeEmpty();
        }
        graph.Vertices.ShouldAllBe(vertex => layout.VertexPositions.ContainsKey(vertex));
    }

    [Fact]
    public void Compound_layout_keeps_group_members_closer_than_flat_tree_baseline()
    {
        var flatGraph = CreateFlatGraph();
        var flatSizes = flatGraph.Vertices.ToDictionary(static vertex => vertex, static _ => new GraphSize(100, 56));
        SimpleTreeLayoutParameters treeParameters = new()
        {
            Direction = LayoutDirection.TopToBottom,
            LayerGap = 60,
            VertexGap = 40
        };
        var treeLayout = new SimpleTreeLayoutAlgorithm<LayoutVertex, LayoutEdge, BidirectionalGraph<LayoutVertex, LayoutEdge>>(
            flatGraph,
            new Dictionary<LayoutVertex, GraphPoint>(),
            flatSizes,
            treeParameters);
        treeLayout.Compute(CancellationToken.None);

        var (compoundGraph, groups) = CreateCompoundGraph();
        var sizes = compoundGraph.Vertices.ToDictionary(
            static vertex => vertex,
            static vertex => vertex.IsGroup ? new GraphSize(180, 120) : new GraphSize(100, 56));
        var borders = compoundGraph.Vertices.ToDictionary(
            static vertex => vertex,
            static vertex => vertex.IsGroup ? new GraphThickness(12, 12, 12, 12) : default);
        var layoutTypes = compoundGraph.Vertices.ToDictionary(
            static vertex => vertex,
            static _ => CompoundVertexInnerLayoutType.Automatic);
        var compoundLayout = new CompoundFDPLayoutAlgorithm<LayoutVertex, LayoutEdge, CompoundGraph<LayoutVertex, LayoutEdge>>(
            compoundGraph,
            sizes,
            borders,
            layoutTypes,
            new Dictionary<LayoutVertex, GraphPoint>(),
            new CompoundFDPLayoutParameters { Seed = 42 });
        compoundLayout.Compute(CancellationToken.None);

        var treeSpread = AverageWithinGroupDistance(
            groups.Select(group => flatGraph.Vertices.Where(vertex => vertex.GroupId == GetGroupId(group)).ToArray()).ToArray(),
            flatGraph.Vertices.ToDictionary(vertex => vertex, vertex => treeLayout.VertexPositions[vertex]));
        var compoundSpread = AverageWithinGroupDistance(
            groups.Select(compoundGraph.GetChildrenVertices).Select(children => children.ToArray()).ToArray(),
            compoundLayout.VertexPositions);

        compoundSpread.ShouldBeLessThan(treeSpread,
            $"Expected CompoundFDP group spread below Tree baseline. Compound={compoundSpread:F2}, Tree={treeSpread:F2}.");
    }

    private static AlgorithmGroupParameters<LayoutVertex, LayoutEdge> CreateGroupParameters(
        BidirectionalGraph<LayoutVertex, LayoutEdge> graph,
        IDictionary<LayoutVertex, GraphSize> vertexSizes,
        int groupId)
    {
        var groupVertices = graph.Vertices.Where(vertex => vertex.GroupId == groupId).ToArray();
        var groupEdges = graph.Edges
            .Where(edge => edge.Source.GroupId == groupId && edge.Target.GroupId == groupId)
            .ToArray();
        var groupGraph = new BidirectionalGraph<LayoutVertex, LayoutEdge>();
        groupGraph.AddVertexRange(groupVertices);
        groupGraph.AddEdgeRange(groupEdges);

        SimpleTreeLayoutParameters treeParameters = new()
        {
            Direction = LayoutDirection.TopToBottom,
            LayerGap = 60,
            VertexGap = 40
        };

        return new AlgorithmGroupParameters<LayoutVertex, LayoutEdge>
        {
            GroupId = groupId,
            LayoutAlgorithm = new SimpleTreeLayoutAlgorithm<LayoutVertex, LayoutEdge, BidirectionalGraph<LayoutVertex, LayoutEdge>>(
                groupGraph,
                new Dictionary<LayoutVertex, GraphPoint>(),
                vertexSizes,
                treeParameters)
        };
    }

    private static BidirectionalGraph<LayoutVertex, LayoutEdge> CreateFlatGraph()
    {
        var graph = new BidirectionalGraph<LayoutVertex, LayoutEdge>();
        var teamAClient = new LayoutVertex("team-a/client", groupId: 1);
        var teamAApi = new LayoutVertex("team-a/api", groupId: 1);
        var teamBWorker = new LayoutVertex("team-b/worker", groupId: 2);
        var teamBQueue = new LayoutVertex("team-b/queue", groupId: 2);
        var worldDns = new LayoutVertex("world/dns", groupId: 3);
        var worldExternal = new LayoutVertex("world/external", groupId: 3);

        graph.AddVertexRange([teamAClient, teamAApi, teamBWorker, teamBQueue, worldDns, worldExternal]);
        graph.AddEdgeRange(
        [
            new LayoutEdge(teamAClient, teamAApi),
            new LayoutEdge(teamAApi, teamBWorker),
            new LayoutEdge(teamBWorker, teamBQueue),
            new LayoutEdge(teamAClient, worldDns),
            new LayoutEdge(worldDns, worldExternal)
        ]);
        return graph;
    }

    private static int GetGroupId(LayoutVertex group) => group.Id switch
    {
        "namespace/team-a" => 1,
        "namespace/team-b" => 2,
        "namespace/world" => 3,
        _ => throw new InvalidOperationException($"Unknown group {group.Id}.")
    };

    private static double AverageWithinGroupDistance(
        IReadOnlyList<LayoutVertex[]> groups,
        IDictionary<LayoutVertex, GraphPoint> positions)
    {
        var distanceTotal = 0d;
        var pairCount = 0;
        foreach (var group in groups)
        {
            for (var leftIndex = 0; leftIndex < group.Length; leftIndex++)
            {
                for (var rightIndex = leftIndex + 1; rightIndex < group.Length; rightIndex++)
                {
                    var dx = positions[group[leftIndex]].X - positions[group[rightIndex]].X;
                    var dy = positions[group[leftIndex]].Y - positions[group[rightIndex]].Y;
                    distanceTotal += Math.Sqrt(dx * dx + dy * dy);
                    pairCount++;
                }
            }
        }

        return distanceTotal / pairCount;
    }

    private sealed class CompoundFdpExternalLayout(
        CompoundGraph<LayoutVertex, LayoutEdge> graph) :
        Westermo.GraphX.Common.Interfaces.IExternalLayout<LayoutVertex, LayoutEdge>
    {
        private readonly Dictionary<LayoutVertex, GraphPoint> _positions = [];

        public IDictionary<LayoutVertex, GraphPoint> VertexPositions => _positions;
        public IDictionary<LayoutVertex, GraphSize> VertexSizes { get; set; } = new Dictionary<LayoutVertex, GraphSize>();
        public bool NeedVertexSizes => true;
        public bool SupportsObjectFreeze => false;

        public void Compute(CancellationToken cancellationToken)
        {
            var sizes = graph.Vertices.ToDictionary(
                static vertex => vertex,
                vertex => vertex.IsGroup ? new GraphSize(180, 120) : VertexSizes[vertex]);
            var borders = graph.Vertices.ToDictionary(
                static vertex => vertex,
                static vertex => vertex.IsGroup ? new GraphThickness(12, 12, 12, 12) : default);
            var layoutTypes = graph.Vertices.ToDictionary(
                static vertex => vertex,
                static _ => CompoundVertexInnerLayoutType.Automatic);
            var compoundLayout = new CompoundFDPLayoutAlgorithm<LayoutVertex, LayoutEdge, CompoundGraph<LayoutVertex, LayoutEdge>>(
                graph,
                sizes,
                borders,
                layoutTypes,
                new Dictionary<LayoutVertex, GraphPoint>(_positions),
                new CompoundFDPLayoutParameters { Seed = 42 });

            compoundLayout.Compute(cancellationToken);
            _positions.Clear();
            foreach (var vertex in graph.Vertices)
            {
                if (!vertex.IsGroup)
                    _positions.Add(vertex, compoundLayout.VertexPositions[vertex]);
            }
        }

        public void ResetGraph(IEnumerable<LayoutVertex> vertices, IEnumerable<LayoutEdge> edges)
        {
            // CompoundGraph already holds parent-child relationships; rebuilding from flat collections would erase them.
        }
    }

    private sealed class TestGraphControlFactory(GraphArea<LayoutVertex, LayoutEdge, CompoundGraph<LayoutVertex, LayoutEdge>> area)
        : IGraphControlFactory
    {
        public GraphAreaBase FactoryRootArea => area;

        public VertexControl CreateVertexControl(object vertexData) => new(vertexData);

        public EdgeControl CreateEdgeControl(
            VertexControl source,
            VertexControl target,
            object edge,
            bool showArrows = true,
            bool isVisible = true)
            => new(source, target, edge, showArrows) { RootArea = area, IsVisible = isVisible };
    }

    private static (CompoundGraph<LayoutVertex, LayoutEdge> Graph, LayoutVertex[] Groups) CreateCompoundGraph()
    {
        var graph = new CompoundGraph<LayoutVertex, LayoutEdge>();
        var teamA = new LayoutVertex("namespace/team-a", isGroup: true);
        var teamB = new LayoutVertex("namespace/team-b", isGroup: true);
        var world = new LayoutVertex("namespace/world", isGroup: true);
        var teamAClient = new LayoutVertex("team-a/client");
        var teamAApi = new LayoutVertex("team-a/api");
        var teamBWorker = new LayoutVertex("team-b/worker");
        var teamBQueue = new LayoutVertex("team-b/queue");
        var worldDns = new LayoutVertex("world/dns");
        var worldExternal = new LayoutVertex("world/external");

        graph.AddVertexRange([teamA, teamB, world]);
        graph.AddChildVertexRange(teamA, [teamAClient, teamAApi]);
        graph.AddChildVertexRange(teamB, [teamBWorker, teamBQueue]);
        graph.AddChildVertexRange(world, [worldDns, worldExternal]);
        graph.AddEdgeRange(
        [
            new LayoutEdge(teamAClient, teamAApi),
            new LayoutEdge(teamAApi, teamBWorker),
            new LayoutEdge(teamBWorker, teamBQueue),
            new LayoutEdge(teamAClient, worldDns),
            new LayoutEdge(worldDns, worldExternal)
        ]);
        return (graph, [teamA, teamB, world]);
    }

    private sealed class LayoutVertex : VertexBase
    {
        public string Id { get; }
        public bool IsGroup { get; }

        public LayoutVertex(string id, int groupId = 0, bool isGroup = false)
        {
            Id = id;
            GroupId = groupId;
            IsGroup = isGroup;
        }
    }

    private sealed class LayoutEdge(LayoutVertex source, LayoutVertex target) : EdgeBase<LayoutVertex>(source, target);
}
