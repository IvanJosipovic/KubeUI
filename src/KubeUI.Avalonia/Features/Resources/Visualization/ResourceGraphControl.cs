using Avalonia.Controls.Templates;
using Avalonia.Svg.Skia;
using k8s;
using k8s.Models;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Avalonia.Infrastructure.DependencyInjection;
using KubeUI.Avalonia.Services.Icons;
using KubeUI.Kubernetes.Resources.Relationships;
using QuikGraph;
using Westermo.GraphX.Common.Enums;
using Westermo.GraphX.Controls.Controls;
using Westermo.GraphX.Logic.Models;
using Westermo.GraphX.Controls.Controls.ZoomControl;
using Westermo.GraphX.Logic.Algorithms.LayoutAlgorithms;
using Westermo.GraphX.Logic.Algorithms.OverlapRemoval;
using Westermo.GraphX.Controls.Models.Interfaces;

namespace KubeUI.Avalonia.Features.Resources.Visualization;

public sealed class ResourceGraphControl : UserControl, IDisposable, IGraphControlFactory
{
    public static readonly DirectProperty<ResourceGraphControl, ResourceRelationshipGraph?> GraphProperty =
        AvaloniaProperty.RegisterDirect<ResourceGraphControl, ResourceRelationshipGraph?>(nameof(Graph), control => control.Graph, (control, value) => control.Graph = value);

    private ResourceRelationshipGraph? _graph;
    private readonly GraphArea<ResourceGraphVertex, ResourceGraphEdge, BidirectionalGraph<ResourceGraphVertex, ResourceGraphEdge>> _area;
    private readonly GXLogicCore<ResourceGraphVertex, ResourceGraphEdge, BidirectionalGraph<ResourceGraphVertex, ResourceGraphEdge>> _logicCore;
    private readonly ZoomControl _zoomControl;
    private readonly IResourceIconService _iconService;
    private readonly Dictionary<ResourceIdentity, ResourceGraphVertex> _vertices = [];
    private VisualizationViewModel? _viewModel;
    private Task? _graphGenerationTask;
    private bool _layoutPending;
    private bool _hasGeneratedGraph;
    private bool _zoomAfterGeneration;
    private bool _vertexMeasurementPending;
    private bool _isDetached;

    public ResourceRelationshipGraph? Graph
    {
        get => _graph;
        set
        {
            if (ReferenceEquals(_graph, value))
            {
                return;
            }

            var previous = _graph;
            _graph = value;
            RaisePropertyChanged(GraphProperty, null, value);

            if (_isDetached)
            {
                return;
            }

            if (previous != null && value != null && _logicCore.Graph != null)
            {
                ApplyGraphChanges(value);
                return;
            }

            RebuildGraph();
        }
    }

    internal GraphArea<ResourceGraphVertex, ResourceGraphEdge, BidirectionalGraph<ResourceGraphVertex, ResourceGraphEdge>> Area => _area;

    public GraphAreaBase FactoryRootArea => _area;

    public ResourceGraphControl(IResourceIconService? iconService = null)
    {
        _iconService = iconService
            ?? (Application.Current as IServiceProviderHost)?.Services.GetService<IResourceIconService>()
            ?? new ResourceIconService();
        _logicCore = new GXLogicCore<ResourceGraphVertex, ResourceGraphEdge, BidirectionalGraph<ResourceGraphVertex, ResourceGraphEdge>>
        {
            DefaultLayoutAlgorithm = LayoutAlgorithmTypeEnum.Tree,
            DefaultOverlapRemovalAlgorithm = OverlapRemovalAlgorithmTypeEnum.FSA,
            DefaultEdgeRoutingAlgorithm = EdgeRoutingAlgorithmTypeEnum.None,

            DefaultLayoutAlgorithmParams = new SimpleTreeLayoutParameters
            {
                Direction = LayoutDirection.TopToBottom,
                LayerGap = 240,
                VertexGap = 120,
                //ComponentGap = 120,
                OptimizeWidthAndHeight = false,
                SpanningTreeGeneration = SpanningTreeGeneration.DFS,
            },
            DefaultOverlapRemovalAlgorithmParams = new OverlapRemovalParameters
            {
                HorizontalGap = 120,
                VerticalGap = 120,
            },
            AsyncAlgorithmCompute = true,
        };

        _area = new GraphArea<ResourceGraphVertex, ResourceGraphEdge, BidirectionalGraph<ResourceGraphVertex, ResourceGraphEdge>>
        {
            LogicCore = _logicCore,
            SelectedVertices = new HashSet<ResourceGraphVertex>(),
            SelectionMode = SelectionMode.Multiple,
            ControlFactory = this,
            VertexLabelFactory = null
        };

        ResourceGraphStyles.Apply(_area);
        _area.GenerateGraphFinished += OnGraphLayoutFinished;
        _area.RelayoutFinished += OnGraphLayoutFinished;
        _area.DataTemplates.Add(new FuncDataTemplate<ResourceGraphVertex>((vertex, _) => VisualizationView.CreateResourceNode(vertex!.Node)));

        _zoomControl = new ZoomControl
        {
            Background = Brushes.Transparent,
            AllowZoomingWithoutCtrl = true,
            Content = _area,
        };

        Content = _zoomControl;
    }

    private void RebuildGraph()
    {
        BidirectionalGraph<ResourceGraphVertex, ResourceGraphEdge> graph = new();
        _hasGeneratedGraph = false;
        _vertices.Clear();
        if (_graph != null)
        {
            Dictionary<ResourceIdentity, ResourceGraphVertex> vertices = [];

            foreach (var resource in _graph.Resources)
            {
                var vertex = CreateVertex(resource);
                vertices.Add(vertex.Identity, vertex);
                _vertices.Add(vertex.Identity, vertex);
                graph.AddVertex(vertex);
            }

            foreach (var relationship in _graph.Relationships)
            {
                if (TryCreateEdge(relationship, vertices, out var edge))
                {
                    graph.AddEdge(edge);
                }
            }
        }

        _logicCore.Graph = graph;
        if (VisualRoot != null)
        {
            QueueGraphGeneration();
        }
    }

    private void ApplyGraphChanges(ResourceRelationshipGraph current)
    {
        var graph = _logicCore.Graph;
        var vertices = _vertices;
        HashSet<ResourceIdentity> desiredIdentities = current.Resources.Select(GetIdentity).ToHashSet();
        HashSet<ResourceRelationship> desiredRelationships = current.Relationships.ToHashSet();
        HashSet<ResourceRelationship> existingRelationships = graph.Edges.Select(edge => edge.Relationship).ToHashSet();
        bool structureChanged = vertices.Keys.Any(identity => !desiredIdentities.Contains(identity))
            || desiredIdentities.Any(identity => !vertices.ContainsKey(identity))
            || graph.Edges.Any(edge => !desiredRelationships.Contains(edge.Relationship))
            || desiredRelationships.Any(relationship => !existingRelationships.Contains(relationship));

        foreach (var vertex in vertices.Values.Where(vertex => !desiredIdentities.Contains(vertex.Identity)).ToArray())
        {
            _area.RemoveVertexAndEdges(vertex);
            if (graph.ContainsVertex(vertex))
            {
                graph.RemoveVertex(vertex);
            }

            vertices.Remove(vertex.Identity);
        }

        foreach (var edge in graph.Edges.ToArray())
        {
            if (!desiredRelationships.Contains(edge.Relationship)
                || !desiredIdentities.Contains(edge.Source.Identity)
                || !desiredIdentities.Contains(edge.Target.Identity))
            {
                _area.RemoveEdge(edge, removeEdgeFromDataGraph: true);
                if (graph.ContainsEdge(edge))
                {
                    graph.RemoveEdge(edge);
                }
            }
        }

        foreach (var resource in current.Resources)
        {
            var identity = GetIdentity(resource);
            if (vertices.TryGetValue(identity, out var existingVertex))
            {
                existingVertex.Node.UpdateResource(resource);
                continue;
            }

            var vertex = CreateVertex(resource);
            vertices.Add(identity, vertex);
            _area.AddVertexAndData(vertex, _area.ControlFactory.CreateVertexControl(vertex), generateLabel: false);
            _vertexMeasurementPending = true;
        }

        foreach (var relationship in current.Relationships)
        {
            if (existingRelationships.Contains(relationship)
                || !vertices.TryGetValue(relationship.Source, out var source)
                || !vertices.TryGetValue(relationship.Target, out var target))
            {
                continue;
            }

            if (TryCreateEdge(relationship, vertices, out var edge))
            {
                _area.InsertEdgeAndData(
                    edge,
                    _area.ControlFactory.CreateEdgeControl(
                        _area.VertexList[edge.Source],
                        _area.VertexList[edge.Target],
                        edge),
                    generateLabel: true);
            }
        }

        if (structureChanged)
        {
            QueueGraphGeneration();
        }
    }

    private static ResourceIdentity GetIdentity(IKubernetesObject<V1ObjectMeta> resource)
        => new(resource.ApiVersion ?? string.Empty, resource.Kind ?? string.Empty, resource.Namespace(), resource.Name() ?? string.Empty, resource.Uid());

    private ResourceGraphVertex CreateVertex(IKubernetesObject<V1ObjectMeta> resource)
    {
        var vertex = new ResourceGraphVertex
        {
            Identity = GetIdentity(resource),
            Node = new()
            {
                Cluster = _viewModel?.Cluster,
                Resource = resource,
                Icon = _iconService.GetIcon(resource.GetType()),
            },
        };

        return vertex;
    }

    private static bool TryCreateEdge(
        ResourceRelationship relationship,
        Dictionary<ResourceIdentity, ResourceGraphVertex> vertices,
        out ResourceGraphEdge edge)
    {
        if (!vertices.TryGetValue(relationship.Source, out var source)
            || !vertices.TryGetValue(relationship.Target, out var target))
        {
            edge = null!;
            return false;
        }

        edge = new ResourceGraphEdge(source, target, relationship);
        return true;
    }

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        if (_viewModel != null)
        {
            _viewModel = null;
        }

        _viewModel = DataContext as VisualizationViewModel;

        if (_viewModel != null && _graph != null && !_isDetached)
        {
            RebuildGraph();
        }
    }

    private void QueueGraphGeneration()
    {
        Dispatcher.UIThread.VerifyAccess();
        _layoutPending = true;
        if (_vertexMeasurementPending && VisualRoot != null)
        {
            _area.UpdateLayout();
            _vertexMeasurementPending = false;
        }

        if (_graphGenerationTask is { IsCompleted: false })
        {
            return;
        }

        _graphGenerationTask = ProcessPendingLayoutAsync();
    }

    private async Task ProcessPendingLayoutAsync()
    {
        Dispatcher.UIThread.VerifyAccess();
        while (_layoutPending && VisualRoot != null)
        {
            _layoutPending = false;
            bool initialGeneration = !_hasGeneratedGraph;
            _zoomAfterGeneration |= initialGeneration;

            if (!initialGeneration)
            {
                await _area.RelayoutGraph(true).ConfigureAwait(true);
            }
            else
            {
                await _area.GenerateGraph(true).ConfigureAwait(true);
                _hasGeneratedGraph = true;
            }
        }
    }

    private void OnGraphLayoutFinished(object? sender, EventArgs e)
    {
        _area.UpdateAllEdges(performFullUpdate: true, skipHiddenEdges: false);
        if (!_zoomAfterGeneration)
        {
            return;
        }

        _zoomAfterGeneration = false;
        _zoomControl.ZoomToFill();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        _isDetached = false;
        if (_graph != null)
        {
            RebuildGraph();
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _isDetached = true;
        _layoutPending = false;
        _vertexMeasurementPending = false;
        _area.ClearLayout();
        _vertices.Clear();
        base.OnDetachedFromVisualTree(e);
    }

    public void Dispose()
    {
        _area.GenerateGraphFinished -= OnGraphLayoutFinished;
        _area.RelayoutFinished -= OnGraphLayoutFinished;
        _area.Dispose();
        _logicCore.Dispose();
    }

    public EdgeControl CreateEdgeControl(VertexControl source, VertexControl target, object edge, bool showArrows = true, bool isVisible = true)
    {
        return new EdgeControl(source, target, edge, showArrows)
        {
            RootArea = _area,
            IsVisible = isVisible,
        };
    }

    public VertexControl CreateVertexControl(object vertexData)
    {
        var vertexControl = new VertexControl(vertexData);
        return vertexControl;
    }
}
