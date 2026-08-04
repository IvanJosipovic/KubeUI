using Avalonia.Controls.Templates;
using k8s;
using k8s.Models;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Infrastructure.DependencyInjection;
using KubeUI.Avalonia.Services.Icons;
using KubeUI.Kubernetes.Resources.Relationships;
using QuikGraph;
using Westermo.GraphX.Common.Enums;
using Westermo.GraphX.Controls.Controls;
using Westermo.GraphX.Controls.Controls.ZoomControl;
using Westermo.GraphX.Controls.Models.Interfaces;
using Westermo.GraphX.Logic.Algorithms.LayoutAlgorithms;
using Westermo.GraphX.Logic.Algorithms.OverlapRemoval;
using Westermo.GraphX.Logic.Models;

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
    private CancellationTokenSource? _graphPreparationCancellation;
    private int _graphPreparationVersion;
    private bool _layoutPending;
    private bool _hasGeneratedGraph;
    private bool _zoomAfterGeneration;
    private bool _isDetached;
    private bool _rebuildFromAttachment;

    public ResourceRelationshipGraph? Graph
    {
        get => _graph;
        set
        {
            if (ReferenceEquals(_graph, value))
            {
                return;
            }

            _graph = value;
            RaisePropertyChanged(GraphProperty, null, value);

            if (_isDetached)
            {
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
            EnableParallelEdges = true,
            ParallelEdgeDistance = 20,

            DefaultLayoutAlgorithmParams = new SimpleTreeLayoutParameters
            {
                Direction = LayoutDirection.TopToBottom,
                LayerGap = 240,
                VertexGap = 120,
                //ComponentGap = 120,
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
        _graphPreparationCancellation?.Cancel();
        CancellationTokenSource cancellation = new();
        _graphPreparationCancellation = cancellation;
        var version = Interlocked.Increment(ref _graphPreparationVersion);
        var graph = _graph;
        var cluster = _viewModel?.Cluster;
        _ = PrepareGraphAsync(graph, cluster, version, cancellation);
    }

    private async Task PrepareGraphAsync(
        ResourceRelationshipGraph? graph,
        ClusterWorkspace? cluster,
        int version,
        CancellationTokenSource cancellation)
    {
        var prepared = await Task.Run(
            () => CreatePreparedGraph(graph, cluster, cancellation.Token),
            cancellation.Token).ConfigureAwait(false);

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (_isDetached
                || version != _graphPreparationVersion
                || cancellation.IsCancellationRequested)
            {
                return;
            }

            _hasGeneratedGraph = false;
            var incremental = VisualRoot != null && _logicCore.Graph != null && !_rebuildFromAttachment;
            _rebuildFromAttachment = false;
            if (incremental)
            {
                ApplyGraphChanges(_graph ?? ResourceRelationshipGraph.Empty);
                return;
            }

            _vertices.Clear();
            foreach (var resource in prepared.Resources)
            {
                var vertex = CreateVertex(resource, prepared.Cluster);
                _vertices.Add(vertex.Identity, vertex);
            }

            BidirectionalGraph<ResourceGraphVertex, ResourceGraphEdge> graph = new();
            foreach (var vertex in _vertices.Values)
            {
                graph.AddVertex(vertex);
            }

            foreach (var relationship in prepared.Relationships)
            {
                if (TryCreateEdge(relationship, _vertices, out var edge))
                {
                    graph.AddEdge(edge);
                }
            }

            _logicCore.Graph = graph;
            if (VisualRoot != null)
            {
                QueueGraphGeneration();
            }
        }, DispatcherPriority.Background);
    }

    private PreparedGraph CreatePreparedGraph(
        ResourceRelationshipGraph? graph,
        ClusterWorkspace? cluster,
        CancellationToken cancellationToken)
    {
        List<IKubernetesObject<V1ObjectMeta>> resources = [];
        List<ResourceRelationship> relationships = [];
        if (graph != null)
        {
            foreach (var resource in graph.Resources)
            {
                cancellationToken.ThrowIfCancellationRequested();
                resources.Add(resource);
            }

            foreach (var relationship in RemoveTransitiveOwnerRelationships(graph.Relationships))
            {
                cancellationToken.ThrowIfCancellationRequested();
                relationships.Add(relationship);
            }
        }

        return new PreparedGraph(resources, relationships, cluster);
    }

    internal static IReadOnlyList<ResourceRelationship> RemoveTransitiveOwnerRelationships(
        IReadOnlyList<ResourceRelationship> relationships)
    {
        Dictionary<ResourceIdentity, List<ResourceIdentity>> childrenByOwner = [];
        foreach (var relationship in relationships)
        {
            if (relationship.Kind != ResourceRelationshipKind.Owner)
            {
                continue;
            }

            childrenByOwner.TryAdd(relationship.Source, []);
            childrenByOwner[relationship.Source].Add(relationship.Target);
        }

        var result = new List<ResourceRelationship>(relationships.Count);
        foreach (var relationship in relationships)
        {
            if (relationship.Kind != ResourceRelationshipKind.Owner
                || !childrenByOwner.TryGetValue(relationship.Source, out var children))
            {
                result.Add(relationship);
                continue;
            }

            var descendants = new Queue<ResourceIdentity>(children.Where(child => child != relationship.Target));
            var visited = new HashSet<ResourceIdentity>();
            var isTransitive = false;
            while (descendants.Count > 0)
            {
                var current = descendants.Dequeue();
                if (!visited.Add(current))
                {
                    continue;
                }

                if (current == relationship.Target)
                {
                    isTransitive = true;
                    break;
                }

                if (childrenByOwner.TryGetValue(current, out var currentChildren))
                {
                    foreach (var child in currentChildren)
                    {
                        descendants.Enqueue(child);
                    }
                }
            }

            if (!isTransitive)
            {
                result.Add(relationship);
            }
        }

        return result;
    }

    private sealed record PreparedGraph(
        IReadOnlyList<IKubernetesObject<V1ObjectMeta>> Resources,
        IReadOnlyList<ResourceRelationship> Relationships,
        ClusterWorkspace? Cluster);

    private void ApplyGraphChanges(ResourceRelationshipGraph current)
    {
        var graph = _logicCore.Graph;
        var vertices = _vertices;
        var relationships = RemoveTransitiveOwnerRelationships(current.Relationships);
        HashSet<ResourceIdentity> desiredIdentities = new(current.Resources.Count);
        foreach (var resource in current.Resources)
        {
            desiredIdentities.Add(GetIdentity(resource));
        }

        HashSet<ResourceRelationship> desiredRelationships = new(relationships.Count);
        desiredRelationships.UnionWith(relationships);

        HashSet<ResourceRelationship> existingRelationships = new(graph.EdgeCount);
        foreach (var edge in graph.Edges)
        {
            existingRelationships.Add(edge.Relationship);
        }

        var structureChanged = false;

        foreach (var vertex in vertices.Values.Where(vertex => !desiredIdentities.Contains(vertex.Identity)).ToArray())
        {
            structureChanged = true;
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
                structureChanged = true;
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
                if (!ReferenceEquals(existingVertex.Node.Resource, resource))
                {
                    existingVertex.Node.UpdateResource(resource);
                }

                continue;
            }

            var vertex = CreateVertex(resource, _viewModel?.Cluster);
            structureChanged = true;
            vertices.Add(identity, vertex);
            _area.AddVertexAndData(vertex, _area.ControlFactory.CreateVertexControl(vertex), generateLabel: false);
        }

        foreach (var relationship in relationships)
        {
            if (!existingRelationships.Add(relationship)
                || !vertices.TryGetValue(relationship.Source, out var source)
                || !vertices.TryGetValue(relationship.Target, out var target))
            {
                continue;
            }

            var edge = new ResourceGraphEdge(source, target, relationship);
            structureChanged = true;
            _area.InsertEdgeAndData(
                edge,
                _area.ControlFactory.CreateEdgeControl(
                    _area.VertexList[edge.Source],
                    _area.VertexList[edge.Target],
                    edge),
                generateLabel: true);
        }

        if (structureChanged)
        {
            QueueGraphGeneration();
        }
    }

    private static ResourceIdentity GetIdentity(IKubernetesObject<V1ObjectMeta> resource)
        => new(resource.ApiVersion ?? string.Empty, resource.Kind ?? string.Empty, resource.Namespace(), resource.Name() ?? string.Empty, resource.Uid());

    private ResourceGraphVertex CreateVertex(IKubernetesObject<V1ObjectMeta> resource, ClusterWorkspace? cluster)
    {
        var vertex = new ResourceGraphVertex
        {
            Identity = GetIdentity(resource),
            Node = new()
            {
                Cluster = cluster,
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

        if (_graphGenerationTask is { IsCompleted: false })
        {
            return;
        }

        _graphGenerationTask = ProcessPendingLayoutAsync();
    }

    private async Task ProcessPendingLayoutAsync()
    {
        while (_layoutPending && VisualRoot != null)
        {
            _layoutPending = false;
            var initialGeneration = !_hasGeneratedGraph;
            _zoomAfterGeneration |= initialGeneration;

            if (!initialGeneration)
            {
                await _area.RelayoutGraph(true);
            }
            else
            {
                await _area.GenerateGraph(true);
                _hasGeneratedGraph = true;
            }
        }
    }

    private void OnGraphLayoutFinished(object? sender, EventArgs e)
    {
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
            _rebuildFromAttachment = true;
            RebuildGraph();
        }
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        _isDetached = true;
        _layoutPending = false;
        _area.ClearLayout();
        base.OnDetachedFromVisualTree(e);
    }

    public void Dispose()
    {
        _graphPreparationCancellation?.Cancel();
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
        return new VertexControl(vertexData);
    }
}
