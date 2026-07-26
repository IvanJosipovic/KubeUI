using Avalonia.Controls.Templates;
using k8s;
using k8s.Models;
using KubeUI.Kubernetes.Resources.Relationships;
using QuikGraph;
using Westermo.GraphX.Common.Enums;
using Westermo.GraphX.Controls.Controls;
using Westermo.GraphX.Logic.Models;
using Westermo.GraphX.Controls.Controls.ZoomControl;
using Westermo.GraphX.Controls.Controls.ZoomControl.SupportClasses;
using Westermo.GraphX.Logic.Algorithms.LayoutAlgorithms;
using Westermo.GraphX.Controls.Controls.Misc;

namespace KubeUI.Avalonia.Features.Resources.Visualization;

public sealed class ResourceGraphControl : UserControl, IDisposable
{
    public static readonly DirectProperty<ResourceGraphControl, ResourceRelationshipGraph?> GraphProperty =
        AvaloniaProperty.RegisterDirect<ResourceGraphControl, ResourceRelationshipGraph?>(nameof(Graph), control => control.Graph, (control, value) => control.Graph = value);

    private ResourceRelationshipGraph? _graph;
    private readonly GraphArea<ResourceGraphVertex, ResourceGraphEdge, BidirectionalGraph<ResourceGraphVertex, ResourceGraphEdge>> _area;
    private readonly GXLogicCore<ResourceGraphVertex, ResourceGraphEdge, BidirectionalGraph<ResourceGraphVertex, ResourceGraphEdge>> _logicCore;
    private readonly ZoomControl _zoomControl;
    private readonly Dictionary<ResourceIdentity, ResourceGraphVertex> _vertices = [];
    private VisualizationViewModel? _viewModel;
    private Task? _graphGenerationTask;
    private bool _layoutPending;
    private bool _hasGeneratedGraph;
    private bool _zoomAfterLayout;
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

            ResourceRelationshipGraph? previous = _graph;
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

    public ResourceGraphControl()
    {
        _area = new GraphArea<ResourceGraphVertex, ResourceGraphEdge, BidirectionalGraph<ResourceGraphVertex, ResourceGraphEdge>>
        {
            EdgeLabelFactory = null,
            LogicCoreChangeAction = LogicCoreChangedAction.None,
            ControlsDrawOrder = ControlDrawOrder.EdgesOnTop,
            SelectedVertices = new HashSet<ResourceGraphVertex>(),
            SelectionMode = SelectionMode.Multiple,
        };
        ResourceGraphStyles.Apply(_area);
        _area.DataTemplates.Add(new FuncDataTemplate<ResourceGraphVertex>((vertex, _) => VisualizationView.CreateResourceNode(vertex!.Node)));

        _logicCore = new GXLogicCore<ResourceGraphVertex, ResourceGraphEdge, BidirectionalGraph<ResourceGraphVertex, ResourceGraphEdge>>
        {
            DefaultLayoutAlgorithm = LayoutAlgorithmTypeEnum.Tree,
            DefaultOverlapRemovalAlgorithm = OverlapRemovalAlgorithmTypeEnum.None,
            DefaultEdgeRoutingAlgorithm = EdgeRoutingAlgorithmTypeEnum.None,

            DefaultLayoutAlgorithmParams = new SimpleTreeLayoutParameters
            {
                Direction = LayoutDirection.TopToBottom,
                LayerGap = 80,
                VertexGap = 80,
                ComponentGap = 80,
                OptimizeWidthAndHeight = true,
                SpanningTreeGeneration = SpanningTreeGeneration.DFS,
            },
            AsyncAlgorithmCompute = false,
        };
        _area.LogicCore = _logicCore;
        _zoomControl = new ZoomControl
        {
            Background = Brushes.Transparent,
            AllowZoomingWithoutCtrl = true,
            IsAnimationEnabled = false,
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
            Dictionary<IKubernetesObject<V1ObjectMeta>, ResourceNodeViewModel> nodesByResource = CreateNodesByResource();

            foreach (var resource in _graph.Resources)
            {
                ResourceGraphVertex vertex = CreateVertex(resource, nodesByResource);
                vertices.Add(vertex.Identity, vertex);
                _vertices.Add(vertex.Identity, vertex);
                graph.AddVertex(vertex);
            }

            foreach (ResourceRelationship relationship in _graph.Relationships)
            {
                if (TryCreateEdge(relationship, vertices, out ResourceGraphEdge edge))
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
        BidirectionalGraph<ResourceGraphVertex, ResourceGraphEdge> graph = _logicCore.Graph;
        Dictionary<ResourceIdentity, ResourceGraphVertex> vertices = _vertices;
        HashSet<ResourceIdentity> desiredIdentities = current.Resources.Select(GetIdentity).ToHashSet();
        foreach (ResourceGraphVertex vertex in vertices.Values.Where(vertex => !desiredIdentities.Contains(vertex.Identity)).ToArray())
        {
            _area.RemoveVertexAndEdges(vertex);
            if (graph.ContainsVertex(vertex))
            {
                graph.RemoveVertex(vertex);
            }

            vertices.Remove(vertex.Identity);
        }

        HashSet<ResourceRelationship> desiredRelationships = current.Relationships.ToHashSet();
        foreach (ResourceGraphEdge edge in graph.Edges.ToArray())
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

        Dictionary<IKubernetesObject<V1ObjectMeta>, ResourceNodeViewModel> nodesByResource = CreateNodesByResource();

        foreach (IKubernetesObject<V1ObjectMeta> resource in current.Resources)
        {
            ResourceIdentity identity = GetIdentity(resource);
            if (vertices.TryGetValue(identity, out ResourceGraphVertex? existingVertex))
            {
                existingVertex.Node.UpdateResource(resource);
                continue;
            }

            ResourceGraphVertex vertex = CreateVertex(resource, nodesByResource);
            vertices.Add(identity, vertex);
            _area.AddVertexAndData(vertex, _area.ControlFactory.CreateVertexControl(vertex));
        }

        HashSet<ResourceRelationship> existingRelationships = graph.Edges.Select(edge => edge.Relationship).ToHashSet();
        foreach (ResourceRelationship relationship in current.Relationships)
        {
            if (existingRelationships.Contains(relationship)
                || !vertices.TryGetValue(relationship.Source, out ResourceGraphVertex? source)
                || !vertices.TryGetValue(relationship.Target, out ResourceGraphVertex? target))
            {
                continue;
            }

            if (TryCreateEdge(relationship, vertices, out ResourceGraphEdge edge))
            {
                _area.InsertEdgeAndData(
                    edge,
                    _area.ControlFactory.CreateEdgeControl(
                        _area.VertexList[edge.Source],
                        _area.VertexList[edge.Target],
                        edge));
            }
        }

        QueueGraphGeneration();
    }

    private static ResourceIdentity GetIdentity(IKubernetesObject<V1ObjectMeta> resource)
        => new(resource.ApiVersion ?? string.Empty, resource.Kind ?? string.Empty, resource.Namespace(), resource.Name() ?? string.Empty, resource.Uid());

    private Dictionary<IKubernetesObject<V1ObjectMeta>, ResourceNodeViewModel> CreateNodesByResource()
    {
        Dictionary<IKubernetesObject<V1ObjectMeta>, ResourceNodeViewModel> nodesByResource =
            new(ReferenceEqualityComparer.Instance);
        if (DataContext is VisualizationViewModel viewModel)
        {
            foreach (ResourceNodeViewModel node in viewModel.Resources)
            {
                nodesByResource.TryAdd(node.Resource, node);
            }
        }

        return nodesByResource;
    }

    private static ResourceGraphVertex CreateVertex(
        IKubernetesObject<V1ObjectMeta> resource,
        Dictionary<IKubernetesObject<V1ObjectMeta>, ResourceNodeViewModel> nodesByResource)
    {
        if (!nodesByResource.TryGetValue(resource, out ResourceNodeViewModel? node))
        {
            node = new ResourceNodeViewModel
            {
                Resource = resource,
            };
        }

        return new ResourceGraphVertex
        {
            Identity = GetIdentity(resource),
            Node = node,
        };
    }

    private static bool TryCreateEdge(
        ResourceRelationship relationship,
        Dictionary<ResourceIdentity, ResourceGraphVertex> vertices,
        out ResourceGraphEdge edge)
    {
        if (!vertices.TryGetValue(relationship.Source, out ResourceGraphVertex? source)
            || !vertices.TryGetValue(relationship.Target, out ResourceGraphVertex? target))
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
            _viewModel.ResourceDeleted -= ApplyResourceDeletion;
            _viewModel.PropertyChanged -= ViewModelPropertyChanged;
        }

        _viewModel = DataContext as VisualizationViewModel;
        if (_viewModel != null)
        {
            _viewModel.ResourceDeleted += ApplyResourceDeletion;
            _viewModel.PropertyChanged += ViewModelPropertyChanged;
        }

        if (_viewModel != null && _graph != null && !_isDetached)
        {
            RebuildGraph();
        }
    }

    internal void ApplyResourceDeletion(ResourceIdentity identity)
    {
        Dispatcher.UIThread.VerifyAccess();
        if (!_vertices.Remove(identity, out ResourceGraphVertex? vertex))
        {
            return;
        }

        _area.RemoveVertexAndEdges(vertex);
        if (_logicCore.Graph.ContainsVertex(vertex))
        {
            _logicCore.Graph.RemoveVertex(vertex);
        }

        QueueGraphGeneration();
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

    private void ViewModelPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(VisualizationViewModel.Resources))
        {
            _zoomAfterLayout = true;
        }
    }

    private async Task ProcessPendingLayoutAsync()
    {
        Dispatcher.UIThread.VerifyAccess();
        while (_layoutPending && VisualRoot != null)
        {
            _layoutPending = false;
            bool initialGeneration = !_hasGeneratedGraph;
            if (!initialGeneration)
            {
                await _area.RelayoutGraph(generateAllEdges: true).ConfigureAwait(true);
            }
            else
            {
                await _area.GenerateGraph().ConfigureAwait(true);
                _hasGeneratedGraph = true;
            }
            _area.ShowAllEdgesArrows(true);
            foreach (EdgeControl control in _area.EdgesList.Values)
            {
                control.GetEdgePointerForSource()?.Hide();
                ShowTargetArrow(control);
            }
            if (initialGeneration || _zoomAfterLayout)
            {
                _zoomAfterLayout = false;
                await Dispatcher.UIThread.InvokeAsync(_zoomControl.ZoomToFill, DispatcherPriority.Render);
            }
        }
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
        _area.ClearLayout();
        _vertices.Clear();
        base.OnDetachedFromVisualTree(e);
    }

    private static void ShowTargetArrow(EdgeControl control)
    {
        control.GetEdgePointerForSource()?.Hide();
        control.GetEdgePointerForTarget()?.Show();

        if (control.GetEdgePointerForTarget() is ContentControl { Content: global::Avalonia.Controls.Shapes.Path path })
        {
            path.Fill = control.Foreground;
        }
    }

    public void Dispose()
    {
        _area.Dispose();
        _logicCore.Dispose();
    }
}
