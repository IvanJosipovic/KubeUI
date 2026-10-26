using System.ComponentModel;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Xaml.MarkupExtensions;
using Avalonia.VisualTree;
using KubeUI.Avalonia.Features.Resources.Visualization;
using KubeUI.Cilium.Hubble;
using Westermo.GraphX.Common.Enums;
using Westermo.GraphX.Common.Interfaces;
using Westermo.GraphX.Common.Models;
using Westermo.GraphX.Controls.Behaviours;
using Westermo.GraphX.Controls.Controls;
using Westermo.GraphX.Controls.Controls.ZoomControl;
using Westermo.GraphX.Controls.Controls.ZoomControl.SupportClasses;
using Westermo.GraphX.Controls.Models.Interfaces;
using Westermo.GraphX.Logic.Algorithms.EdgeRouting;
using Westermo.GraphX.Logic.Algorithms.LayoutAlgorithms;
using Westermo.GraphX.Logic.Algorithms.OverlapRemoval;
using Westermo.GraphX.Logic.Models;

namespace KubeUI.Avalonia.Features.Cilium.Hubble.Controls;

/// <summary>Renders Hubble service-map snapshots through GraphX.</summary>
public sealed class ServiceMapGraphControl : UserControl, IDisposable, IGraphControlFactory
{
    private static readonly TimeSpan UpdateBuffer = TimeSpan.FromMilliseconds(400);

    public static readonly DirectProperty<ServiceMapGraphControl, ServiceMapGraph?> GraphProperty =
        AvaloniaProperty.RegisterDirect<ServiceMapGraphControl, ServiceMapGraph?>(nameof(Graph), control => control.Graph, (control, value) => control.Graph = value);
    public static readonly DirectProperty<ServiceMapGraphControl, LayoutAlgorithmTypeEnum> LayoutAlgorithmProperty =
        AvaloniaProperty.RegisterDirect<ServiceMapGraphControl, LayoutAlgorithmTypeEnum>(
            nameof(LayoutAlgorithm), control => control.LayoutAlgorithm, (control, value) => control.LayoutAlgorithm = value);
    public static readonly DirectProperty<ServiceMapGraphControl, EdgeRoutingAlgorithmTypeEnum> EdgeRoutingAlgorithmProperty =
        AvaloniaProperty.RegisterDirect<ServiceMapGraphControl, EdgeRoutingAlgorithmTypeEnum>(
            nameof(EdgeRoutingAlgorithm), control => control.EdgeRoutingAlgorithm, (control, value) => control.EdgeRoutingAlgorithm = value);
    private readonly GraphArea<ServiceMapVertex, ServiceMapEdge, CompoundGraph<ServiceMapVertex, ServiceMapEdge>> _area;
    private readonly GXLogicCore<ServiceMapVertex, ServiceMapEdge, CompoundGraph<ServiceMapVertex, ServiceMapEdge>> _logicCore;
    private readonly ZoomControl _zoomControl;
    private readonly Dictionary<string, ServiceMapVertex> _vertices = new(StringComparer.Ordinal);
    private readonly Dictionary<string, ServiceMapVertex> _groups = new(StringComparer.Ordinal);
    private readonly ServiceMapExternalLayout _externalLayout;
    private ServiceMapGraph? _graph;
    private LayoutAlgorithmTypeEnum _layoutAlgorithm = LayoutAlgorithmTypeEnum.CompoundFDP;
    private EdgeRoutingAlgorithmTypeEnum _edgeRoutingAlgorithm = EdgeRoutingAlgorithmTypeEnum.SimpleER;
    private ServiceMapGraph? _pendingGraph;
    private CancellationTokenSource? _bufferCancellation;
    private CancellationTokenSource? _layoutCancellation;
    private Task? _layoutTask;
    private bool _layoutPending;
    private bool _relayoutPending;
    private bool _attached;
    private bool _hasGeneratedGraph;
    private bool _disposed;
    private bool _graphXDisposed;

    /// <summary>Gets or sets graph snapshot to render.</summary>
    public ServiceMapGraph? Graph
    {
        get => _graph;
        set
        {
            if (ReferenceEquals(_graph, value))
                return;
            var old = _graph;
            _graph = value;
            RaisePropertyChanged(GraphProperty, old, value);
            _pendingGraph = value;
            if (!_hasGeneratedGraph && value is not null)
            {
                _bufferCancellation?.Cancel();
                ApplySnapshot(value);
                return;
            }

            QueueBufferedUpdate();
        }
    }

    /// <summary>Gets or sets GraphX layout algorithm used to position service nodes.</summary>
    public LayoutAlgorithmTypeEnum LayoutAlgorithm
    {
        get => _layoutAlgorithm;
        set
        {
            if (_layoutAlgorithm == value)
                return;
            var old = _layoutAlgorithm;
            _layoutAlgorithm = value;
            _externalLayout.Algorithm = value;
            RaisePropertyChanged(LayoutAlgorithmProperty, old, value);
            RequestRelayout();
        }
    }

    /// <summary>Gets or sets GraphX edge-routing algorithm used to draw connections.</summary>
    public EdgeRoutingAlgorithmTypeEnum EdgeRoutingAlgorithm
    {
        get => _edgeRoutingAlgorithm;
        set
        {
            if (_edgeRoutingAlgorithm == value)
                return;
            var old = _edgeRoutingAlgorithm;
            _edgeRoutingAlgorithm = value;
            _logicCore.DefaultEdgeRoutingAlgorithm = value;
            _logicCore.DefaultEdgeRoutingAlgorithmParams = CreateEdgeRoutingParameters(value);
            RaisePropertyChanged(EdgeRoutingAlgorithmProperty, old, value);
            RequestRelayout();
        }
    }

    public GraphAreaBase FactoryRootArea => _area;
    internal GraphArea<ServiceMapVertex, ServiceMapEdge, CompoundGraph<ServiceMapVertex, ServiceMapEdge>> Area => _area;
    internal int GraphXVertexCount => _vertices.Count;
    internal int GraphXEdgeCount => _logicCore.Graph?.EdgeCount ?? 0;
    internal bool HasGeneratedGraph => _hasGeneratedGraph;
    internal LayoutAlgorithmTypeEnum ActiveExternalLayoutAlgorithm => _externalLayout.Algorithm;
    internal Task? LayoutTask => _layoutTask;

    public ServiceMapGraphControl()
    {
        CompoundGraph<ServiceMapVertex, ServiceMapEdge> graph = new();
        _externalLayout = new ServiceMapExternalLayout(graph);
        _logicCore = new GXLogicCore<ServiceMapVertex, ServiceMapEdge, CompoundGraph<ServiceMapVertex, ServiceMapEdge>>
        {
            Graph = graph,
            DefaultLayoutAlgorithm = LayoutAlgorithmTypeEnum.Custom,
            ExternalLayoutAlgorithm = _externalLayout,
            DefaultOverlapRemovalAlgorithm = OverlapRemovalAlgorithmTypeEnum.FSA,
            DefaultEdgeRoutingAlgorithm = EdgeRoutingAlgorithmTypeEnum.SimpleER,
            EnableParallelEdges = true,
            ParallelEdgeDistance = 20,
            DefaultLayoutAlgorithmParams = new SimpleTreeLayoutParameters
            {
                Direction = LayoutDirection.TopToBottom,
                LayerGap = 240,
                VertexGap = 120,
                SpanningTreeGeneration = SpanningTreeGeneration.DFS,
            },
            DefaultOverlapRemovalAlgorithmParams = new OverlapRemovalParameters { HorizontalGap = 120, VerticalGap = 120 },
            AsyncAlgorithmCompute = true,
        };
        _area = new GraphArea<ServiceMapVertex, ServiceMapEdge, CompoundGraph<ServiceMapVertex, ServiceMapEdge>>
        {
            LogicCore = _logicCore,
            ControlFactory = this,
            EdgeLabelFactory = null,
            VertexLabelFactory = null,
        };
        _area.Styles.Add(new Style<EdgeControl>(style => style.OfType<EdgeControl>())
            .Setter(EdgeControl.StrokeThicknessProperty, 1d)
            .Setter(ToolTip.TipProperty, CompiledBinding.Create<ServiceMapEdge, string>(edge => edge.Tooltip))
            .Foreground(new DynamicResourceExtension("VisualizationRelationshipDefaultBrush"))
            .Opacity(0.35d)
            .Template(ResourceGraphStyles.CreateDirectedEdgeTemplate()));
        _area.Styles.Add(new Style<EdgeControl>(style => style.OfType<EdgeControl>().Class("ServiceMapDeniedFlow"))
            .Foreground(new DynamicResourceExtension("ContainerStatusErrorBrush")).Opacity(0.9d));
        _area.Styles.Add(new Style<Border>(style => style.OfType<Border>().Class("ServiceMapNode"))
            .Background(new DynamicResourceExtension("SystemBaseLowColor"))
            .BorderBrush(new DynamicResourceExtension("VisualizationRelationshipOwnerBrush"))
            .BorderThickness(new Thickness(1.5)));
        _area.Styles.Add(new Style<Border>(style => style.OfType<Border>().Class("ServiceMapDeniedNode"))
            .BorderBrush(new DynamicResourceExtension("ContainerStatusErrorBrush")));
        _area.Styles.Add(new Style<VertexControl>(style => style.OfType<VertexControl>())
            .Background(Brushes.Transparent)
            .Padding(new Thickness(0))
            .BorderThickness(new Thickness(0))
            .Setter(VertexControlBase.VertexShapeProperty, VertexShape.Rectangle)
            .Setter(DragBehaviour.IsDragEnabledProperty, true)
            .Setter(DragBehaviour.UpdateEdgesOnMoveProperty, true));
        _area.DataTemplates.Add(new FuncDataTemplate<ServiceMapVertex>((vertex, _) => CreateNode(vertex!)));
        _zoomControl = new ZoomControl { Background = Brushes.Transparent, AllowZoomingWithoutCtrl = true, Content = _area };
        Content = _zoomControl;
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _attached = true;
        QueueBufferedUpdate();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _attached = false;
        _bufferCancellation?.Cancel();
        _layoutCancellation?.Cancel();
        _hasGeneratedGraph = false;
        _vertices.Clear();
        _groups.Clear();
        _area.ClearLayout(clearStates: true, clearLogicCore: true);
        base.OnDetachedFromVisualTree(e);
    }

    private void QueueBufferedUpdate()
    {
        if (!_attached || _disposed)
            return;
        _bufferCancellation?.Cancel();
        _bufferCancellation?.Dispose();
        var cancellation = new CancellationTokenSource();
        _bufferCancellation = cancellation;
        _ = ApplyAfterBufferAsync(cancellation);
    }

    private async Task ApplyAfterBufferAsync(CancellationTokenSource cancellation)
    {
        try
        {
            await Task.Delay(UpdateBuffer, cancellation.Token).ConfigureAwait(false);
            Dispatcher.UIThread.Post(() =>
            {
                if (!_disposed && _attached && !cancellation.IsCancellationRequested)
                {
                    ApplySnapshot(_pendingGraph);
                }
            }, DispatcherPriority.Background);
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested) { }
    }

    [SuppressMessage("Reliability", "CA2000:Dispose objects before losing scope", Justification = "GraphX owns controls passed to InsertEdgeAndData.")]
    private void ApplySnapshot(ServiceMapGraph? snapshot)
    {
        if (_layoutTask is { IsCompleted: false })
        {
            _layoutPending = true;
            return;
        }

        var graph = _logicCore.Graph
            ?? new CompoundGraph<ServiceMapVertex, ServiceMapEdge>();
        _externalLayout.Graph = graph;
        _logicCore.Graph = graph;
        var topologyChanged = false;
        var desiredNodeIds = new HashSet<string>(StringComparer.Ordinal);
        if (snapshot is not null)
            foreach (var node in snapshot.Nodes)
                desiredNodeIds.Add(node.Id);

        foreach (var vertex in _vertices.Values.Where(vertex => !desiredNodeIds.Contains(vertex.Id)).ToArray())
        {
            RemoveChildMembership(graph, vertex);
            _area.RemoveVertexAndEdges(vertex);
            if (graph.ContainsVertex(vertex))
                graph.RemoveVertex(vertex);
            _vertices.Remove(vertex.Id);
            topologyChanged = true;
        }

        if (snapshot is not null)
        {
            foreach (var node in snapshot.Nodes)
            {
                if (_vertices.TryGetValue(node.Id, out var vertex))
                {
                    var groupKey = GetGroupKey(node);
                    if (!ReferenceEquals(graph.GetParent(vertex), GetOrCreateGroup(graph, groupKey)))
                    {
                        MoveToGroup(graph, vertex, groupKey);
                        topologyChanged = true;
                    }
                    vertex.Node = node;
                    UpdateNodeControl(vertex);
                }
                else
                {
                    vertex = new ServiceMapVertex(node);
                    _vertices.Add(node.Id, vertex);
                    graph.AddChildVertex(GetOrCreateGroup(graph, GetGroupKey(node)), vertex);
                    _area.AddVertexAndData(vertex, CreateVertexControl(vertex), generateLabel: false);
                    topologyChanged = true;
                }
            }
        }

        Dictionary<(string Source, string Target), KubeUI.Cilium.Hubble.ServiceMapEdge> desiredEdges = [];
        if (snapshot is not null)
        {
            foreach (var edge in snapshot.Edges)
                if (_vertices.TryGetValue(edge.SourceId, out var source)
                    && _vertices.TryGetValue(edge.TargetId, out var target))
                    desiredEdges[(edge.SourceId, edge.TargetId)] = edge;
        }

        foreach (var edge in graph.Edges.ToArray())
        {
            if (!desiredEdges.Remove((edge.Source.Id, edge.Target.Id), out var updated))
            {
                _area.RemoveEdge(edge, removeEdgeFromDataGraph: true);
                if (graph.ContainsEdge(edge))
                    graph.RemoveEdge(edge);
                topologyChanged = true;
            }
            else
            {
                edge.UpdateFrom(updated);
            }
        }

        foreach (var edge in desiredEdges)
        {
            var graphEdge = new ServiceMapEdge(_vertices[edge.Key.Source], _vertices[edge.Key.Target], edge.Value);
            _area.InsertEdgeAndData(graphEdge, CreateEdgeControl(_area.VertexList[graphEdge.Source], _area.VertexList[graphEdge.Target], graphEdge), generateLabel: false);
            topologyChanged = true;
        }

        RemoveEmptyGroups(graph);
        _logicCore.Graph = graph;
        if (topologyChanged || _relayoutPending || (!_hasGeneratedGraph && graph.VertexCount > 0))
        {
            _relayoutPending = false;
            QueueLayout();
        }
    }

    private void QueueLayout()
    {
        _layoutPending = false;
        _relayoutPending = false;
        _layoutCancellation?.Cancel();
        _layoutCancellation?.Dispose();
        var cancellation = new CancellationTokenSource();
        _layoutCancellation = cancellation;
        _layoutTask = LayoutAsync(cancellation.Token);
    }

    private void RequestRelayout()
    {
        if (!_hasGeneratedGraph || _disposed || _logicCore.Graph is not { VertexCount: > 0 })
            return;

        if (_layoutTask is { IsCompleted: false })
        {
            _layoutPending = true;
            _relayoutPending = true;
            return;
        }

        QueueLayout();
    }

    private static IEdgeRoutingParameters? CreateEdgeRoutingParameters(EdgeRoutingAlgorithmTypeEnum algorithm)
    {
        return algorithm switch
        {
            EdgeRoutingAlgorithmTypeEnum.PathFinder => new PathFinderEdgeRoutingParameters(),
            EdgeRoutingAlgorithmTypeEnum.SimpleER => new SimpleERParameters(),
            EdgeRoutingAlgorithmTypeEnum.Bundling => new BundleEdgeRoutingParameters
            {
                Iterations = 50,
                RepulseOpposite = true
            },
            _ => null
        };
    }

    private async Task LayoutAsync(CancellationToken cancellationToken)
    {
        try
        {
            var initial = !_hasGeneratedGraph;
            cancellationToken.ThrowIfCancellationRequested();
            if (initial)
            {
                _zoomControl.Mode = ZoomControlModes.Fill;
                await _area.GenerateGraph(true, cancellation: cancellationToken);
                _hasGeneratedGraph = true;
                _zoomControl.ZoomToFill();
                _zoomControl.Mode = ZoomControlModes.Custom;
            }
            else
            {
                await _area.RelayoutGraph(true, cancellationToken);
            }

            _area.ShowAllEdgesArrows(true);
            if (_layoutPending && _attached && !_disposed)
                await Dispatcher.UIThread.InvokeAsync(() => ApplySnapshot(_pendingGraph), DispatcherPriority.Background);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) { }
        finally
        {
            if (_layoutCancellation?.Token == cancellationToken)
            {
                _layoutCancellation.Dispose();
                _layoutCancellation = null;
                if (_layoutPending && _attached && !_disposed)
                {
                    _layoutPending = false;
                    Dispatcher.UIThread.Post(() => ApplySnapshot(_pendingGraph), DispatcherPriority.Background);
                }
            }
        }
    }

    private static Border CreateNode(ServiceMapVertex vertex)
    {
        var node = vertex.Node ?? throw new InvalidOperationException("Group vertices are excluded from rendering.");
        var border = new Border().Width(200).MinHeight(84).Padding(12, 9).CornerRadius(8).Classes("ServiceMapNode");
        if (node.DeniedCount > 0)
        {
            border.Classes.Add("ServiceMapDeniedNode");
        }

        return border.Child(CreateNodeContent(node));
    }

    private static StackPanel CreateNodeContent(ServiceMapNode node)
    {
        var namespaceLabel = string.IsNullOrEmpty(node.Namespace)
            ? Assets.Resources.ServiceMapGraphControl_External
            : node.Namespace;
        var counts = string.Format(
            CultureInfo.CurrentCulture,
            Assets.Resources.ServiceMapGraphControl_FlowCountsFormat,
            node.IngressFlowCount,
            node.EgressFlowCount,
            node.DeniedCount);
        var content = new StackPanel().Spacing(3);
        content.Children.Add(new TextBlock().Text(Shorten(node.Name)).FontSize(14).FontWeight(FontWeight.SemiBold)
            .Foreground(new DynamicResourceExtension("SystemBaseHighColor")));
        content.Children.Add(new TextBlock().Text(Shorten(namespaceLabel)).FontSize(12).Opacity(0.88)
            .Foreground(new DynamicResourceExtension("SystemBaseHighColor")));
        content.Children.Add(new TextBlock().Text(counts).FontSize(11).FontWeight(FontWeight.Medium)
            .Foreground(new DynamicResourceExtension("SystemBaseHighColor")));

        foreach (var accessPoint in node.AccessPoints)
        {
            var applicationProtocols = accessPoint.ApplicationProtocols.Count == 0
                ? string.Empty
                : $" • {string.Join(" • ", accessPoint.ApplicationProtocols)}";
            content.Children.Add(new TextBlock()
                .Text($"→ {accessPoint.Port} • {accessPoint.Protocol}{applicationProtocols}")
                .FontSize(11)
                .Foreground(new DynamicResourceExtension("SystemBaseHighColor")));
        }

        return content;
    }

    private void UpdateNodeControl(ServiceMapVertex vertex)
    {
        if (vertex.Node is not { } node)
            return;
        if (!_area.VertexList.TryGetValue(vertex, out var control))
            return;
        var border = control.GetVisualDescendants()
            .OfType<Border>()
            .FirstOrDefault(static candidate => candidate.Classes.Contains("ServiceMapNode"));
        if (border is not null)
        {
            if (node.DeniedCount > 0)
            {
                if (!border.Classes.Contains("ServiceMapDeniedNode"))
                {
                    border.Classes.Add("ServiceMapDeniedNode");
                }
            }
            else
            {
                border.Classes.Remove("ServiceMapDeniedNode");
            }
        }
        if (border is not null)
        {
            border.Child = CreateNodeContent(node);
        }
    }

    private static string Shorten(string text) => text.Length <= 24 ? text : string.Concat(text.AsSpan(0, 21), "…");

    public EdgeControl CreateEdgeControl(VertexControl source, VertexControl target, object edge, bool showArrows = true, bool isVisible = true)
    {
        var control = new EdgeControl(source, target, edge, showArrows) { RootArea = _area, IsVisible = isVisible };
        if (edge is ServiceMapEdge { DeniedCount: > 0 })
            control.Classes.Add("ServiceMapDeniedFlow");
        return control;
    }

    public VertexControl CreateVertexControl(object vertexData) => new(vertexData);

    public void Dispose()
    {
        if (_disposed)
            return;
        _disposed = true;
        _bufferCancellation?.Cancel();
        _layoutCancellation?.Cancel();
        if (_layoutTask is { IsCompleted: false } task)
            _ = DisposeAfterLayoutAsync(task);
        else
            DisposeGraphX();
    }

    private async Task DisposeAfterLayoutAsync(Task task)
    {
        try
        { await task.ConfigureAwait(false); }
        catch (Exception) { }
        await Dispatcher.UIThread.InvokeAsync(DisposeGraphX);
    }

    private void DisposeGraphX()
    {
        if (_graphXDisposed)
            return;
        _graphXDisposed = true;
        _area.Dispose();
        _logicCore.Dispose();
    }

    private static string GetGroupKey(ServiceMapNode node) => string.IsNullOrEmpty(node.Namespace)
        ? string.Empty
        : node.Namespace;

    private ServiceMapVertex GetOrCreateGroup(CompoundGraph<ServiceMapVertex, ServiceMapEdge> graph, string groupKey)
    {
        if (_groups.TryGetValue(groupKey, out var group))
            return group;

        group = graph.Vertices.FirstOrDefault(candidate => candidate.IsGroup && candidate.GroupKey == groupKey);
        if (group is not null)
        {
            _groups.Add(groupKey, group);
            return group;
        }

        group = ServiceMapVertex.CreateGroup(groupKey);
        graph.AddVertex(group);
        _groups.Add(groupKey, group);
        return group;
    }

    private void MoveToGroup(
        CompoundGraph<ServiceMapVertex, ServiceMapEdge> graph,
        ServiceMapVertex vertex,
        string groupKey)
    {
        RemoveChildMembership(graph, vertex);
        graph.AddChildVertex(GetOrCreateGroup(graph, groupKey), vertex);
    }

    private static void RemoveChildMembership(
        CompoundGraph<ServiceMapVertex, ServiceMapEdge> graph,
        ServiceMapVertex vertex)
    {
        var parent = graph.GetParent(vertex);
        if (parent is not null && graph.GetChildrenVertices(parent) is ICollection<ServiceMapVertex> children)
            children.Remove(vertex);
    }

    private void RemoveEmptyGroups(CompoundGraph<ServiceMapVertex, ServiceMapEdge> graph)
    {
        foreach (var group in _groups.Values.Where(group => graph.GetChildrenCount(group) == 0).ToArray())
        {
            graph.RemoveVertex(group);
            _groups.Remove(group.GroupKey);
        }
    }

    internal sealed class ServiceMapVertex : VertexBase
    {
        public string Id { get; }
        public ServiceMapNode? Node { get; set; }
        public bool IsGroup { get; }
        public string GroupKey { get; }

        public ServiceMapVertex(ServiceMapNode node)
        {
            Id = node.Id;
            Node = node;
            GroupKey = GetGroupKey(node);
        }

        private ServiceMapVertex(string groupKey)
        {
            Id = $"\0group/{groupKey}";
            GroupKey = groupKey;
            IsGroup = true;
            SkipProcessing = ProcessingOptionEnum.Exclude;
        }

        public static ServiceMapVertex CreateGroup(string groupKey) => new(groupKey);
    }

    private sealed class ServiceMapExternalLayout(
        CompoundGraph<ServiceMapVertex, ServiceMapEdge> graph) :
        IExternalLayout<ServiceMapVertex, ServiceMapEdge>
    {
        private readonly Dictionary<ServiceMapVertex, Westermo.GraphX.Measure.Point> _positions = [];

        public CompoundGraph<ServiceMapVertex, ServiceMapEdge> Graph { get; set; } = graph;
        public LayoutAlgorithmTypeEnum Algorithm { get; set; } = LayoutAlgorithmTypeEnum.CompoundFDP;
        public IDictionary<ServiceMapVertex, Westermo.GraphX.Measure.Point> VertexPositions => _positions;
        public IDictionary<ServiceMapVertex, Westermo.GraphX.Measure.Size> VertexSizes { get; set; } = new Dictionary<ServiceMapVertex, Westermo.GraphX.Measure.Size>();
        public bool NeedVertexSizes => true;
        public bool SupportsObjectFreeze => false;

        public void Compute(CancellationToken cancellationToken)
        {
            var vertices = Graph.Vertices.Where(vertex => !vertex.IsGroup).ToArray();
            if (vertices.Length == 0)
            {
                _positions.Clear();
                return;
            }

            var sizes = Graph.Vertices.ToDictionary(
                static vertex => vertex,
                vertex => vertex.IsGroup
                    ? new Westermo.GraphX.Measure.Size(180, 120)
                    : VertexSizes[vertex]);
            var borders = Graph.Vertices.ToDictionary(
                static vertex => vertex,
                static vertex => vertex.IsGroup ? new Westermo.GraphX.Measure.Thickness(12, 12, 12, 12) : default);
            var layoutTypes = Graph.Vertices.ToDictionary(
                static vertex => vertex,
                static _ => CompoundVertexInnerLayoutType.Automatic);
            var initialPositions = _positions
                .Where(pair => !pair.Key.IsGroup && vertices.Contains(pair.Key))
                .ToDictionary(static pair => pair.Key, static pair => pair.Value);
            IDictionary<ServiceMapVertex, Westermo.GraphX.Measure.Point> computedPositions;
            if (Algorithm == LayoutAlgorithmTypeEnum.CompoundFDP)
            {
                var compoundLayout = new CompoundFDPLayoutAlgorithm<ServiceMapVertex, ServiceMapEdge, CompoundGraph<ServiceMapVertex, ServiceMapEdge>>(
                    Graph,
                    sizes,
                    borders,
                    layoutTypes,
                    initialPositions,
                    new CompoundFDPLayoutParameters
                    {
                        Seed = 42,
                        IdealEdgeLength = 280,
                        GravitationFactor = 10,
                        NestingFactor = 0.35
                    });
                compoundLayout.Compute(cancellationToken);
                computedPositions = compoundLayout.VertexPositions;
            }
            else
            {
                ILayoutAlgorithm<ServiceMapVertex, ServiceMapEdge, CompoundGraph<ServiceMapVertex, ServiceMapEdge>> layoutAlgorithm;
                if (Algorithm == LayoutAlgorithmTypeEnum.SimpleRandom)
                {
                    layoutAlgorithm = new RandomLayoutAlgorithm<
                        ServiceMapVertex,
                        ServiceMapEdge,
                        CompoundGraph<ServiceMapVertex, ServiceMapEdge>>(
                            Graph,
                            initialPositions,
                            new RandomLayoutAlgorithmParams { Seed = 42 })
                    {
                        VertexSizes = sizes
                    };
                }
                else
                {
                    var algorithmFactory = new AlgorithmFactory<
                        ServiceMapVertex,
                        ServiceMapEdge,
                        CompoundGraph<ServiceMapVertex, ServiceMapEdge>>();
                    layoutAlgorithm = algorithmFactory.CreateLayoutAlgorithm(
                        Algorithm,
                        Graph,
                        initialPositions,
                        sizes,
                        algorithmFactory.CreateLayoutParameters(Algorithm))
                        ?? throw new InvalidOperationException($"GraphX does not support layout algorithm {Algorithm}.");
                }
                layoutAlgorithm.Compute(cancellationToken);
                computedPositions = layoutAlgorithm.VertexPositions;
            }

            _positions.Clear();
            foreach (var vertex in vertices)
                _positions.Add(vertex, computedPositions[vertex]);
        }

        public void ResetGraph(IEnumerable<ServiceMapVertex> vertices, IEnumerable<ServiceMapEdge> edges)
        {
            // Rebuilding from flat collections would discard compound parent-child membership.
        }
    }

    internal sealed class ServiceMapEdge(ServiceMapVertex source, ServiceMapVertex target, KubeUI.Cilium.Hubble.ServiceMapEdge edge)
        : EdgeBase<ServiceMapVertex>(source, target), INotifyPropertyChanged
    {
        public int FlowCount { get; private set; } = edge.FlowCount;
        public int DeniedCount { get; private set; } = edge.DeniedCount;
        public event PropertyChangedEventHandler? PropertyChanged;
        public string Tooltip { get; private set; } = FormatTooltip(edge);

        public void UpdateFrom(KubeUI.Cilium.Hubble.ServiceMapEdge edge)
        {
            FlowCount = edge.FlowCount;
            DeniedCount = edge.DeniedCount;
            var tooltip = FormatTooltip(edge);
            if (Tooltip == tooltip)
                return;
            Tooltip = tooltip;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Tooltip)));
        }

        private static string FormatTooltip(KubeUI.Cilium.Hubble.ServiceMapEdge edge)
        {
            var accessPoints = string.Join(", ", edge.AccessPoints.Select(accessPoint =>
                $"{accessPoint.Protocol} {accessPoint.Port} ({accessPoint.FlowCount})"));
            return string.Format(
                CultureInfo.CurrentCulture,
                Assets.Resources.ServiceMapGraphControl_EdgeTooltipFormat,
                edge.FlowCount,
                accessPoints);
        }

        public override string ToString() => FlowCount.ToString(CultureInfo.CurrentCulture);
    }
}
