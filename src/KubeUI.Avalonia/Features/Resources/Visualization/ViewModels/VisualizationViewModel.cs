using System.Collections.Specialized;
using System.Reactive.Linq;
using System.Reactive.Subjects;
using Avalonia.Threading;
using AvaloniaGraphControl;
using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Features.Clusters.Workspace.ViewModels;
using KubeUI.Avalonia.Features.Resources.Common;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Avalonia.Infrastructure.DependencyInjection;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Avalonia.Infrastructure.Threading;
using KubeUI.Kubernetes;
using Microsoft.Extensions.DependencyInjection;
using static AvaloniaGraphControl.GraphPanel;

namespace KubeUI.Avalonia.Features.Resources.Visualization.ViewModels;

public sealed partial class VisualizationViewModel : ViewModelBase, IInitializeCluster, IDisposable
{
    private static readonly TimeSpan RefreshDelay = TimeSpan.FromMilliseconds(500);
    private readonly List<object> _groupNodes = [];
    private readonly Subject<int> _refreshRequests = new();
    private readonly IDisposable _refreshSubscription;
    private bool _disposed;

    [ObservableProperty]
    public partial ClusterWorkspaceViewModel? Cluster { get; set; }

    [ObservableProperty]
    public partial IKubernetesObject<V1ObjectMeta>? RootResource { get; set; }

    [ObservableProperty]
    public partial Graph Graph { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<ResourceNodeViewModel> Resources { get; set; } = [];

    [ObservableProperty]
    public partial bool HideNoise { get; set; } = true;

    [ObservableProperty]
    public partial LayoutMethods LayoutMethod { get; set; } = LayoutMethods.SugiyamaScheme;

    public VisualizationViewModel(IServiceProvider serviceProvider)
    {
        Title = Assets.Resources.VisualizationView_Title;
        _refreshSubscription = _refreshRequests
            .Throttle(RefreshDelay, AvaloniaScheduler.Instance)
            .ObserveOn(AvaloniaScheduler.Instance)
            .Subscribe(_ => Run());
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);

        if (e.PropertyName == nameof(HideNoise))
        {
            QueueRun();
        }
    }

    public void Initialize(ClusterWorkspaceViewModel cluster)
    {
        Initialize(cluster, null);
    }

    public void Initialize(ClusterWorkspaceViewModel cluster, IKubernetesObject<V1ObjectMeta>? rootResource)
    {
        // Detach from any previous cluster to avoid duplicate event subscriptions
        if (Cluster != null && Cluster.SelectedNamespaces != null)
        {
            Cluster.SelectedNamespaces.CollectionChanged -= SelectedNamespaces_CollectionChanged;
            Cluster.OnChange -= Cluster_OnChange;
            Cluster.ResourceSeeded -= Cluster_ResourceSeeded;
        }

        Cluster = cluster;
        RootResource = rootResource;

        if (Cluster?.SelectedNamespaces != null)
        {
            // Ensure single subscription
            Cluster.SelectedNamespaces.CollectionChanged -= SelectedNamespaces_CollectionChanged;
            Cluster.SelectedNamespaces.CollectionChanged += SelectedNamespaces_CollectionChanged;
            Cluster.OnChange -= Cluster_OnChange;
            Cluster.OnChange += Cluster_OnChange;
            Cluster.ResourceSeeded -= Cluster_ResourceSeeded;
            Cluster.ResourceSeeded += Cluster_ResourceSeeded;
        }

        Id = CreateId(cluster, rootResource);

        _ = cluster.SeedResource<V1Node>();

        _ = cluster.SeedResource<Corev1Event>();

        // Workloads
        _ = cluster.SeedResource<V1Pod>();
        _ = cluster.SeedResource<V1ReplicaSet>();
        _ = cluster.SeedResource<V1Deployment>();
        _ = cluster.SeedResource<V1StatefulSet>();
        _ = cluster.SeedResource<V1DaemonSet>();
        _ = cluster.SeedResource<V1CronJob>();
        _ = cluster.SeedResource<V1Job>();

        // Configuration
        _ = cluster.SeedResource<V1Secret>();
        _ = cluster.SeedResource<V1ConfigMap>();

        // Network
        _ = cluster.SeedResource<V1Service>();
        _ = cluster.SeedResource<V1EndpointSlice>();
        _ = cluster.SeedResource<V1Ingress>();
        _ = cluster.SeedResource<V1IngressClass>();

        // Storage
        _ = cluster.SeedResource<V1PersistentVolumeClaim>();
        _ = cluster.SeedResource<V1PersistentVolume>();

        // Access Control
        _ = cluster.SeedResource<V1ServiceAccount>();
        _ = cluster.SeedResource<V1RoleBinding>();
        _ = cluster.SeedResource<V1ClusterRoleBinding>();
        _ = cluster.SeedResource<V1Role>();
        _ = cluster.SeedResource<V1ClusterRole>();

        _ = SeedOwnerReferenceResourceTypesAsync();

        Run();
    }

    private void Run()
    {
        if (Cluster == null)
        {
            Graph = new Graph();
            Resources.Clear();
            return;
        }

        var graph = new Graph();
        Resources.Clear();
        PopulateAllResources(RootResource != null);
        UpdateRootHighlight();

        LinkOwners(Resources, graph);
        LinkEvent(Resources, graph);

        LinkIngress(Resources, graph);
        LinkEndpointSlice(Resources, graph);

        LinkConfigMap(Resources, graph);
        LinkSecret(Resources, graph);

        LinkArgoCDTracking(Resources, graph);
        LinkFluxCDKustomizationTracking(Resources, graph);
        LinkFluxCDHelmReleaseTracking(Resources, graph);

        LinkServiceAccount(Resources, graph);

        LinkPersistantVolumeClaim(Resources, graph);
        LinkPersistantVolume(Resources, graph);

        LinkRoleBinding(Resources, graph);
        LinkClusterRoleBinding(Resources, graph);

        RemoveDuplicateConnections(graph);

        if (RootResource != null)
        {
            FilterToRootResource(graph, RootResource);
            RemoveLessSpecificDependencyConnections(graph, includeAllNamespaces: true);
        }
        else
        {
            if (Cluster.SelectedNamespaces.Count == 0)
            {
                Graph = new Graph();
                Resources.Clear();
                return;
            }

            RemoveNonRelatedToNamespace(graph);
            RemoveLessSpecificDependencyConnections(graph, includeAllNamespaces: false);
        }

        ApplyGrouping(graph);
        Graph = graph;
    }

    private void SelectedNamespaces_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        QueueRun();
    }

    private void Cluster_OnChange(WatchEventType eventType, GroupApiVersionKind groupApiVersionKind, IKubernetesObject<V1ObjectMeta> resource)
    {
        if (Cluster == null || resource.Metadata == null)
        {
            return;
        }

        if (!IsVisibleResource(resource))
        {
            return;
        }

        QueueRun();
    }

    private void Cluster_ResourceSeeded(ClusterWorkspaceViewModel seededCluster, Type resourceType)
    {
        if (_disposed || !ReferenceEquals(seededCluster, Cluster))
        {
            return;
        }

        QueueRun();
    }

    private void QueueRun()
    {
        if (_disposed)
        {
            return;
        }

        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() =>
            {
                if (!_disposed)
                {
                    _refreshRequests.OnNext(0);
                }
            });
            return;
        }

        if (_disposed)
        {
            return;
        }

        _refreshRequests.OnNext(0);
    }

    private async Task SeedOwnerReferenceResourceTypesAsync()
    {
        var cluster = Cluster;
        if (cluster == null)
        {
            return;
        }

        var ownerReferenceTypes = new HashSet<Type>();

        foreach (var resource in EnumerateClusterResources(includeAllNamespaces: true))
        {
            if (_disposed)
            {
                return;
            }

            var ownerReferences = resource.Metadata?.OwnerReferences;
            if (ownerReferences == null)
            {
                continue;
            }

            foreach (var ownerReference in ownerReferences)
            {
                if (ownerReference.Kind == V1Namespace.KubeKind
                    || string.IsNullOrWhiteSpace(ownerReference.Kind)
                    || string.IsNullOrWhiteSpace(ownerReference.ApiVersion))
                {
                    continue;
                }

                if (!TryGetOwnerReferenceType(cluster, ownerReference.ApiVersion, ownerReference.Kind, out var ownerReferenceType))
                {
                    continue;
                }

                if (cluster.Objects.ContainsKey(GroupApiVersionKind.From(ownerReferenceType)))
                {
                    continue;
                }

                ownerReferenceTypes.Add(ownerReferenceType);
            }
        }

        foreach (var ownerReferenceType in ownerReferenceTypes)
        {
            if (_disposed)
            {
                return;
            }

            try
            {
                await cluster.SeedResource(ownerReferenceType).ConfigureAwait(false);
            }
            catch
            {
                // Ignore individual seed failures so the visible graph can still render.
            }
        }
    }

    private bool TryGetOwnerReferenceType(ClusterWorkspaceViewModel cluster, string apiVersion, string kind, out Type resourceType)
    {
        resourceType = null!;

        var slashIndex = apiVersion.IndexOf('/');
        var group = slashIndex < 0 ? string.Empty : apiVersion[..slashIndex];
        var version = slashIndex < 0 ? apiVersion : apiVersion[(slashIndex + 1)..];

        if (string.IsNullOrWhiteSpace(version))
        {
            return false;
        }

        resourceType = cluster.ModelCache.GetResourceType(group, version, kind);
        return resourceType != null;
    }

    private bool IsVisibleResource(IKubernetesObject<V1ObjectMeta> resource)
    {
        foreach (var node in Resources)
        {
            if (IsSameResource(node.Resource, resource))
            {
                return true;
            }
        }

        return false;
    }

    private static bool IsSameResource(IKubernetesObject<V1ObjectMeta> left, IKubernetesObject<V1ObjectMeta> right)
    {
        if (!string.Equals(left.ApiVersion, right.ApiVersion, StringComparison.Ordinal)
            || !string.Equals(left.Kind, right.Kind, StringComparison.Ordinal))
        {
            return false;
        }

        return string.Equals(left.Metadata?.Name, right.Metadata?.Name, StringComparison.Ordinal)
            && string.Equals(left.Metadata?.NamespaceProperty, right.Metadata?.NamespaceProperty, StringComparison.Ordinal);
    }

    private void PopulateAllResources(bool includeAllNamespaces)
    {
        if (Cluster?.Objects == null)
        {
            return;
        }

        foreach (var value in EnumerateClusterResources(includeAllNamespaces))
        {
            // HideNoise
            if (HideNoise
                && (
                       (value is V1ReplicaSet replicaSet && replicaSet.Status?.Replicas == 0)
                    || (value is Corev1Event)
                ))
            {
                continue;
            }

            var node = new ResourceNodeViewModel()
            {
                Cluster = Cluster,
                Resource = value,
                IconPath = Utilities.GetKubeAssetPath(value.GetType())
            };

            Resources.Add(node);
        }
    }

    private IEnumerable<IKubernetesObject<V1ObjectMeta>> EnumerateClusterResources(bool includeAllNamespaces)
    {
        if (Cluster?.Objects == null)
        {
            yield break;
        }

        foreach (var kvp in Cluster.Objects)
        {
            var container = kvp.Value;

            var itemsProperty = container.GetType().GetProperty("Items");
            var items = itemsProperty?.GetValue(container);
            var nestedItemsProperty = items?.GetType().GetProperty("Items");
            var nestedItems = nestedItemsProperty?.GetValue(items) as IList;

            if (nestedItems == null)
            {
                continue;
            }

            foreach (object item in nestedItems)
            {
                var value = (IKubernetesObject<V1ObjectMeta>)item;

                if (!includeAllNamespaces
                    && !string.IsNullOrEmpty(value.Namespace())
                    && !Cluster.SelectedNamespaces.Any(x => x.Name() == value.Namespace()))
                {
                    continue;
                }

                yield return value;
            }
        }
    }

    private void UpdateRootHighlight()
    {
        var rootNode = RootResource == null ? null : FindResourceNode(RootResource);

        foreach (var node in Resources)
        {
            node.IsRoot = ReferenceEquals(node, rootNode);
        }
    }

    private static string CreateId(ClusterWorkspaceViewModel cluster, IKubernetesObject<V1ObjectMeta>? rootResource)
    {
        if (rootResource == null)
        {
            return $"{nameof(VisualizationViewModel)}-{cluster.Name}";
        }

        return $"{nameof(VisualizationViewModel)}-{cluster.Name}-{rootResource.ApiVersion}/{rootResource.Kind}/{rootResource.Metadata?.NamespaceProperty}/{rootResource.Metadata?.Name}";
    }

    private static void RemoveDuplicateConnections(Graph graph)
    {
        if (graph?.Edges == null || graph.Edges.Count < 2)
        {
            return;
        }

        var seen = new HashSet<(object? Head, object? Tail)>();

        // Iterate backwards so we can remove in-place
        for (int i = graph.Edges.Count - 1; i >= 0; i--)
        {
            var edge = graph.Edges.ElementAt(i);
            var key = (edge.Head, edge.Tail);

            if (!seen.Add(key))
            {
                // Duplicate found, remove it
                graph.Edges.Remove(edge);
            }
        }
    }

    private static bool IsOwnerConnection(ResourceNodeViewModel owner, ResourceNodeViewModel child)
    {
        var ownerUid = owner.Resource.Uid();
        if (string.IsNullOrWhiteSpace(ownerUid))
        {
            return false;
        }

        var ownerReferences = child.Resource.Metadata?.OwnerReferences;
        if (ownerReferences == null)
        {
            return false;
        }

        foreach (var ownerReference in ownerReferences)
        {
            if (ownerReference.Uid == ownerUid)
            {
                return true;
            }
        }

        return false;
    }

    private void RemoveLessSpecificDependencyConnections(Graph graph, bool includeAllNamespaces)
    {
        if (Cluster == null || graph?.Edges == null || graph.Edges.Count < 2)
        {
            return;
        }

        var ownerChildrenByNode = new Dictionary<ResourceNodeViewModel, List<ResourceNodeViewModel>>();
        var ownerChildrenByUid = new Dictionary<string, List<string>>(StringComparer.Ordinal);
        var dependencyEdgesByHead = new Dictionary<ResourceNodeViewModel, List<Edge>>();

        foreach (var resource in EnumerateClusterResources(includeAllNamespaces))
        {
            var childUid = resource.Uid();
            if (string.IsNullOrWhiteSpace(childUid))
            {
                continue;
            }

            var ownerReferences = resource.Metadata?.OwnerReferences;
            if (ownerReferences == null)
            {
                continue;
            }

            foreach (var ownerReference in ownerReferences)
            {
                if (ownerReference.Kind == V1Namespace.KubeKind || string.IsNullOrWhiteSpace(ownerReference.Uid))
                {
                    continue;
                }

                if (!ownerChildrenByUid.TryGetValue(ownerReference.Uid, out var children))
                {
                    children = [];
                    ownerChildrenByUid[ownerReference.Uid] = children;
                }

                children.Add(childUid);
            }
        }

        foreach (var edge in graph.Edges)
        {
            if (edge.Head is not ResourceNodeViewModel head || edge.Tail is not ResourceNodeViewModel tail)
            {
                continue;
            }

            if (IsOwnerConnection(tail, head))
            {
                if (!ownerChildrenByNode.TryGetValue(tail, out var children))
                {
                    children = [];
                    ownerChildrenByNode[tail] = children;
                }

                children.Add(head);
                continue;
            }

            if (!dependencyEdgesByHead.TryGetValue(head, out var incoming))
            {
                incoming = [];
                dependencyEdgesByHead[head] = incoming;
            }

            incoming.Add(edge);
        }

        if (dependencyEdgesByHead.Count == 0)
        {
            return;
        }

        var edgesToRemove = new HashSet<Edge>();

        foreach (var incoming in dependencyEdgesByHead.Values)
        {
            for (var i = 0; i < incoming.Count; i++)
            {
                if (edgesToRemove.Contains(incoming[i]))
                {
                    continue;
                }

                if (incoming[i].Tail is not ResourceNodeViewModel source)
                {
                    continue;
                }

                for (var j = 0; j < incoming.Count; j++)
                {
                    if (i == j || edgesToRemove.Contains(incoming[j]))
                    {
                        continue;
                    }

                    if (incoming[j].Tail is not ResourceNodeViewModel otherSource)
                    {
                        continue;
                    }

                    if (IsOwnerDescendant(
                            source,
                            otherSource,
                            ownerChildrenByNode,
                            source.Resource.Uid(),
                            otherSource.Resource.Uid(),
                            ownerChildrenByUid))
                    {
                        edgesToRemove.Add(incoming[i]);
                        break;
                    }
                }
            }
        }

        foreach (var edge in edgesToRemove)
        {
            graph.Edges.Remove(edge);
        }

        static bool IsOwnerDescendant(
            ResourceNodeViewModel ancestor,
            ResourceNodeViewModel descendant,
            IReadOnlyDictionary<ResourceNodeViewModel, List<ResourceNodeViewModel>> ownerChildrenByNode,
            string ancestorUid,
            string descendantUid,
            IReadOnlyDictionary<string, List<string>> ownerChildrenByUid)
        {
            if (ReferenceEquals(ancestor, descendant))
            {
                return false;
            }

            if (IsOwnerDescendantByUid(ancestorUid, descendantUid, ownerChildrenByUid))
            {
                return true;
            }

            var visited = new HashSet<ResourceNodeViewModel>();
            var queue = new Queue<ResourceNodeViewModel>();
            queue.Enqueue(ancestor);
            visited.Add(ancestor);

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (!ownerChildrenByNode.TryGetValue(current, out var children))
                {
                    continue;
                }

                foreach (var child in children)
                {
                    if (!visited.Add(child))
                    {
                        continue;
                    }

                    if (ReferenceEquals(child, descendant))
                    {
                        return true;
                    }

                    queue.Enqueue(child);
                }
            }

            return false;
        }

        static bool IsOwnerDescendantByUid(
            string ancestorUid,
            string descendantUid,
            IReadOnlyDictionary<string, List<string>> ownerChildrenByUid)
        {
            if (!ownerChildrenByUid.TryGetValue(ancestorUid, out var children))
            {
                return false;
            }

            var visited = new HashSet<string>(StringComparer.Ordinal) { ancestorUid };
            var queue = new Queue<string>();

            foreach (var child in children)
            {
                if (visited.Add(child))
                {
                    if (string.Equals(child, descendantUid, StringComparison.Ordinal))
                    {
                        return true;
                    }

                    queue.Enqueue(child);
                }
            }

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (!ownerChildrenByUid.TryGetValue(current, out var currentChildren))
                {
                    continue;
                }

                foreach (var child in currentChildren)
                {
                    if (!visited.Add(child))
                    {
                        continue;
                    }

                    if (string.Equals(child, descendantUid, StringComparison.Ordinal))
                    {
                        return true;
                    }

                    queue.Enqueue(child);
                }
            }

            return false;
        }
    }

    private void FilterToRootResource(Graph graph, IKubernetesObject<V1ObjectMeta> rootResource)
    {
        var root = FindResourceNode(rootResource);
        if (root == null)
        {
            Resources.Clear();
            Graph = new Graph();
            return;
        }

        var parentsByNode = new Dictionary<ResourceNodeViewModel, List<ResourceNodeViewModel>>();
        var childrenByNode = new Dictionary<ResourceNodeViewModel, List<ResourceNodeViewModel>>();
        var outgoingDependenciesByNode = new Dictionary<ResourceNodeViewModel, List<ResourceNodeViewModel>>();
        var ownerEdges = new HashSet<(ResourceNodeViewModel Tail, ResourceNodeViewModel Head)>();

        foreach (var edge in graph.Edges)
        {
            if (edge.Head is not ResourceNodeViewModel head || edge.Tail is not ResourceNodeViewModel tail)
            {
                continue;
            }

            if (!parentsByNode.TryGetValue(head, out var headParents))
            {
                headParents = [];
                parentsByNode[head] = headParents;
            }

            headParents.Add(tail);

            if (IsOwnerConnection(tail, head))
            {
                ownerEdges.Add((tail, head));

                if (!childrenByNode.TryGetValue(tail, out var tailChildren))
                {
                    tailChildren = [];
                    childrenByNode[tail] = tailChildren;
                }

                tailChildren.Add(head);
                continue;
            }

            if (!outgoingDependenciesByNode.TryGetValue(tail, out var tailDependencies))
            {
                tailDependencies = [];
                outgoingDependenciesByNode[tail] = tailDependencies;
            }

            tailDependencies.Add(head);
        }

        var reachable = new HashSet<ResourceNodeViewModel>();
        var queue = new Queue<(ResourceNodeViewModel Node, bool AllowAnyIncoming)>();
        reachable.Add(root);
        queue.Enqueue((root, true));

        while (queue.Count > 0)
        {
            var current = queue.Dequeue();

            if (childrenByNode.TryGetValue(current.Node, out var children))
            {
                foreach (var child in children)
                {
                    if (reachable.Add(child))
                    {
                        queue.Enqueue((child, false));
                    }
                }
            }

            if (current.AllowAnyIncoming && outgoingDependenciesByNode.TryGetValue(current.Node, out var dependencies))
            {
                foreach (var dependency in dependencies)
                {
                    if (reachable.Add(dependency))
                    {
                        queue.Enqueue((dependency, false));
                    }
                }
            }

            if (!parentsByNode.TryGetValue(current.Node, out var parents))
            {
                continue;
            }

            foreach (var parent in parents)
            {
                if (!current.AllowAnyIncoming && !ownerEdges.Contains((parent, current.Node)))
                {
                    continue;
                }

                if (reachable.Add(parent))
                {
                    queue.Enqueue((parent, false));
                }
            }
        }

        foreach (var edge in graph.Edges.ToList())
        {
            if (edge.Head is not ResourceNodeViewModel head || edge.Tail is not ResourceNodeViewModel tail)
            {
                graph.Edges.Remove(edge);
                continue;
            }

            if (!reachable.Contains(head) || !reachable.Contains(tail))
            {
                graph.Edges.Remove(edge);
            }
        }

        for (var i = Resources.Count - 1; i >= 0; i--)
        {
            if (!reachable.Contains(Resources[i]))
            {
                Resources.RemoveAt(i);
            }
        }
    }

    private ResourceNodeViewModel? FindResourceNode(IKubernetesObject<V1ObjectMeta> resource)
    {
        var uid = resource.Uid();
        if (!string.IsNullOrWhiteSpace(uid))
        {
            var byUid = Resources.FirstOrDefault(node => node.Resource.Uid() == uid);
            if (byUid != null)
            {
                return byUid;
            }
        }

        var namespaceName = resource.Namespace();
        var name = resource.Name();
        return Resources.FirstOrDefault(node =>
            node.Resource.GetType() == resource.GetType()
            && node.Resource.Namespace() == namespaceName
            && node.Resource.Name() == name);
    }

    private void RemoveNonRelatedToNamespace(Graph graph)
    {
        if (graph?.Edges == null || graph.Edges.Count == 0 || Cluster == null)
        {
            return;
        }

        var selectedNamespaces = new HashSet<string>(
            Cluster.SelectedNamespaces
                .Where(n => !string.IsNullOrEmpty(n.Name()))
                .Select(n => n.Name()!));

        if (selectedNamespaces.Count == 0)
        {
            return;
        }

        // Collect all resource nodes participating in edges
        var nodes = new HashSet<ResourceNodeViewModel>();
        foreach (var edge in graph.Edges)
        {
            if (edge.Head is ResourceNodeViewModel head)
            {
                nodes.Add(head);
            }

            if (edge.Tail is ResourceNodeViewModel tail)
            {
                nodes.Add(tail);
            }
        }

        if (nodes.Count == 0)
        {
            return;
        }

        // Determine which nodes are in a selected namespace
        var nodeIsInSelectedNamespace = new Dictionary<ResourceNodeViewModel, bool>();
        foreach (var node in nodes)
        {
            var ns = node.Resource?.Namespace();
            nodeIsInSelectedNamespace[node] = ns != null && selectedNamespaces.Contains(ns);
        }

        // Build undirected adjacency list
        var adjacency = new Dictionary<ResourceNodeViewModel, List<ResourceNodeViewModel>>();
        foreach (var node in nodes)
        {
            adjacency[node] = new List<ResourceNodeViewModel>();
        }

        foreach (var edge in graph.Edges)
        {
            if (edge.Head is ResourceNodeViewModel head && edge.Tail is ResourceNodeViewModel tail)
            {
                adjacency[head].Add(tail);
                adjacency[tail].Add(head);
            }
        }

        var visited = new HashSet<ResourceNodeViewModel>();
        var edgesToRemove = new HashSet<Edge>();

        // Traverse connected components; keep only components that have at least one node in a selected namespace
        foreach (var start in nodes)
        {
            if (!visited.Add(start))
            {
                continue;
            }

            var componentNodes = new List<ResourceNodeViewModel>();
            var queue = new Queue<ResourceNodeViewModel>();
            queue.Enqueue(start);

            var componentHasSelectedNamespaceNode = false;

            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                componentNodes.Add(current);

                if (nodeIsInSelectedNamespace[current])
                {
                    componentHasSelectedNamespaceNode = true;
                }

                foreach (var neighbor in adjacency[current])
                {
                    if (visited.Add(neighbor))
                    {
                        queue.Enqueue(neighbor);
                    }
                }
            }

            if (!componentHasSelectedNamespaceNode)
            {
                var componentSet = new HashSet<ResourceNodeViewModel>(componentNodes);

                // Mark all edges entirely contained in this component for removal
                foreach (var edge in graph.Edges)
                {
                    if (edge.Head is ResourceNodeViewModel head && edge.Tail is ResourceNodeViewModel tail)
                    {
                        if (componentSet.Contains(head) && componentSet.Contains(tail))
                        {
                            edgesToRemove.Add(edge);
                        }
                    }
                }
            }
        }

        // Remove edges not related to any selected namespace chain
        if (edgesToRemove.Count > 0)
        {
            foreach (var edge in edgesToRemove)
            {
                graph.Edges.Remove(edge);
            }
        }
    }

    private void ApplyGrouping(Graph graph)
    {
        if (graph?.Edges == null || Resources.Count == 0)
        {
            return;
        }

        _groupNodes.Clear();

        var namespaceGroups = new Dictionary<string, GroupNodeViewModel>(StringComparer.Ordinal);
        var componentGroups = new Dictionary<(string NamespaceName, string ComponentName), GroupNodeViewModel>();
        var firstChildByGroup = new Dictionary<GroupNodeViewModel, object>();

        foreach (var resource in Resources)
        {
            var namespaceName = resource.Resource.Namespace();
            if (string.IsNullOrWhiteSpace(namespaceName))
            {
                continue;
            }

            var namespaceGroup = GetOrCreateNamespaceGroup(namespaceName, namespaceGroups);
            var componentName = GetComponentName(resource.Resource);

            if (!string.IsNullOrWhiteSpace(componentName))
            {
                var componentGroup = GetOrCreateComponentGroup(namespaceGroup, namespaceName, componentName, componentGroups);
                SetParent(resource, componentGroup);
                RememberGroupChild(firstChildByGroup, componentGroup, resource);
                RememberGroupChild(firstChildByGroup, namespaceGroup, componentGroup);
                continue;
            }

            SetParent(resource, namespaceGroup);
            RememberGroupChild(firstChildByGroup, namespaceGroup, resource);
        }

        foreach (var group in _groupNodes.OfType<GroupNodeViewModel>())
        {
            if (!firstChildByGroup.TryGetValue(group, out var child))
            {
                continue;
            }

            graph.Edges.Add(new GroupEdge(group, child));
        }

        void SetParent(ResourceNodeViewModel resource, GroupNodeViewModel parent)
            => graph.Parent[resource] = parent;

        static void RememberGroupChild(Dictionary<GroupNodeViewModel, object> firstChildByGroup, GroupNodeViewModel group, object child)
        {
            if (!firstChildByGroup.ContainsKey(group))
            {
                firstChildByGroup[group] = child;
            }
        }

        GroupNodeViewModel GetOrCreateNamespaceGroup(string namespaceName, Dictionary<string, GroupNodeViewModel> groups)
        {
            if (groups.TryGetValue(namespaceName, out var group))
            {
                return group;
            }

            group = new GroupNodeViewModel
            {
                GroupKind = GroupKind.Namespace,
                Name = namespaceName,
                DisplayText = $"Namespace {namespaceName}",
            };

            group.DNode = new Microsoft.Msagl.Drawing.Subgraph(group.Name);
            group.DNode.Attr.Padding = 18;

            groups[namespaceName] = group;
            _groupNodes.Add(group);
            return group;
        }

        GroupNodeViewModel GetOrCreateComponentGroup(
            GroupNodeViewModel namespaceGroup,
            string namespaceName,
            string componentName,
            Dictionary<(string NamespaceName, string ComponentName), GroupNodeViewModel> groups)
        {
            var key = (namespaceName, componentName);
            if (groups.TryGetValue(key, out var group))
            {
                return group;
            }

            group = new GroupNodeViewModel
            {
                GroupKind = GroupKind.Component,
                Name = componentName,
                DisplayText = $"Component {componentName}",
            };

            group.DNode = new Microsoft.Msagl.Drawing.Subgraph(group.Name);
            group.DNode.Attr.Padding = 12;

            groups[key] = group;
            _groupNodes.Add(group);
            graph.Parent[group] = namespaceGroup;
            return group;
        }

        static string? GetComponentName(IKubernetesObject<V1ObjectMeta> resource)
        {
            if (resource.Metadata?.Labels != null && resource.Metadata.Labels.TryGetValue("app.kubernetes.io/component", out var componentName))
            {
                return string.IsNullOrWhiteSpace(componentName) ? null : componentName;
            }

            return null;
        }
    }

    #region Links

    private static void LinkOwners(ObservableCollection<ResourceNodeViewModel> resources, Graph graph)
    {
        foreach (var end in resources)
        {
            if (end.Resource?.Metadata?.OwnerReferences != null)
            {
                foreach (var ownerReference in end.Resource.Metadata.OwnerReferences)
                {
                    if (ownerReference.Kind == V1Namespace.KubeKind)
                    {
                        continue;
                    }

                    var start = resources.FirstOrDefault(x => x.Resource.Uid() == ownerReference.Uid);

                    if (start != null)
                    {
                        graph.Edges.Add(new Edge(start, end));
                    }
                }
            }
        }
    }

    private static void LinkIngress(ObservableCollection<ResourceNodeViewModel> resources, Graph graph)
    {
        foreach (var start in resources)
        {
            if (start.Resource is V1Ingress ingress)
            {
                if (ingress?.Spec?.Rules != null)
                {
                    foreach (var serviceBackend in ingress.Spec.Rules
                                                    .SelectMany(x => x.Http.Paths.Where(y => y.Backend?.Service != null)
                                                    .Select(y => y.Backend.Service)))
                    {
                        foreach (var end in resources)
                        {
                            if (end.Resource is V1Service service)
                            {
                                if (service.Name() == serviceBackend.Name && service.Namespace() == ingress.Namespace())
                                {
                                    graph.Edges.Add(new Edge(start, end));
                                }
                            }
                        }
                    }
                }

                if (ingress?.Spec?.DefaultBackend != null)
                {
                    if (ingress.Spec.DefaultBackend.Service != null)
                    {
                        foreach (var end in resources)
                        {
                            if (end.Resource is V1Service service)
                            {
                                if (service.Name() == ingress.Spec.DefaultBackend.Service.Name && service.Namespace() == ingress.Namespace())
                                {
                                    graph.Edges.Add(new Edge(start, end));
                                }
                            }
                        }
                    }

                    //todo DefaultBackend.Resource
                }

                //todo Backend.Resource
            }
        }
    }

    private static void LinkEndpointSlice(ObservableCollection<ResourceNodeViewModel> resources, Graph graph)
    {
        foreach (var start in resources)
        {
            if (start.Resource is V1EndpointSlice endpointSlice)
            {
                if (endpointSlice.Endpoints == null)
                {
                    continue;
                }

                foreach (var endpoint in endpointSlice.Endpoints)
                {
                    if (endpoint.TargetRef == null)
                    {
                        continue;
                    }

                    foreach (var end in resources)
                    {
                        if (end.Resource is V1Pod pod)
                        {
                            if (pod.Uid() == endpoint.TargetRef.Uid)
                            {
                                graph.Edges.Add(new Edge(start, end));
                            }
                        }
                    }
                }
            }
        }
    }

    private static void LinkConfigMap(ObservableCollection<ResourceNodeViewModel> resources, Graph graph)
    {
        foreach (var start in resources)
        {
            if (start.Resource is V1Deployment deployment)
            {
                if (deployment?.Spec?.Template?.Spec?.Containers != null)
                {
                    foreach (var container in deployment.Spec.Template.Spec.Containers)
                    {
                        if (container.Env != null)
                        {
                            foreach (var env in container.Env)
                            {
                                if (env.ValueFrom?.ConfigMapKeyRef != null)
                                {
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Name() == env.ValueFrom.ConfigMapKeyRef.Name && configMap.Namespace() == deployment.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (container.EnvFrom != null)
                        {
                            foreach (var envFrom in container.EnvFrom)
                            {
                                if (envFrom.ConfigMapRef != null)
                                {
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Name() == envFrom.ConfigMapRef.Name && configMap.Namespace() == deployment.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (deployment?.Spec?.Template?.Spec?.InitContainers != null)
                {
                    foreach (var container in deployment.Spec.Template.Spec.InitContainers)
                    {
                        if (container.Env != null)
                        {
                            foreach (var env in container.Env)
                            {
                                if (env.ValueFrom?.ConfigMapKeyRef != null)
                                {
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Name() == env.ValueFrom.ConfigMapKeyRef.Name && configMap.Namespace() == deployment.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (container.EnvFrom != null)
                        {
                            foreach (var envFrom in container.EnvFrom)
                            {
                                if (envFrom.ConfigMapRef != null)
                                {
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Name() == envFrom.ConfigMapRef.Name && configMap.Namespace() == deployment.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (deployment?.Spec?.Template?.Spec?.Volumes != null)
                {
                    foreach (var volume in deployment.Spec.Template.Spec.Volumes)
                    {
                        if (volume.ConfigMap != null)
                        {
                            foreach (var end in resources)
                            {
                                if (end.Resource is V1ConfigMap configMap)
                                {
                                    if (configMap.Name() == volume.ConfigMap.Name && configMap.Namespace() == deployment.Namespace())
                                    {
                                        graph.Edges.Add(new Edge(start, end));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (start.Resource is V1ReplicaSet replicaSet)
            {
                if (replicaSet?.Spec?.Template?.Spec?.Containers != null)
                {
                    foreach (var container in replicaSet.Spec.Template.Spec.Containers)
                    {
                        if (container.Env != null)
                        {
                            foreach (var env in container.Env)
                            {
                                if (env.ValueFrom?.ConfigMapKeyRef != null)
                                {
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Name() == env.ValueFrom.ConfigMapKeyRef.Name && configMap.Namespace() == replicaSet.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (container.EnvFrom != null)
                        {
                            foreach (var envFrom in container.EnvFrom)
                            {
                                if (envFrom.ConfigMapRef != null)
                                {
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Name() == envFrom.ConfigMapRef.Name && configMap.Namespace() == replicaSet.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (replicaSet?.Spec?.Template?.Spec?.InitContainers != null)
                {
                    foreach (var container in replicaSet.Spec.Template.Spec.InitContainers)
                    {
                        if (container.Env != null)
                        {
                            foreach (var env in container.Env)
                            {
                                if (env.ValueFrom?.ConfigMapKeyRef != null)
                                {
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Name() == env.ValueFrom.ConfigMapKeyRef.Name && configMap.Namespace() == replicaSet.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (container.EnvFrom != null)
                        {
                            foreach (var envFrom in container.EnvFrom)
                            {
                                if (envFrom.ConfigMapRef != null)
                                {
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Name() == envFrom.ConfigMapRef.Name && configMap.Namespace() == replicaSet.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (replicaSet?.Spec?.Template?.Spec?.Volumes != null)
                {
                    foreach (var volume in replicaSet.Spec.Template.Spec.Volumes)
                    {
                        if (volume.ConfigMap != null)
                        {
                            foreach (var end in resources)
                            {
                                if (end.Resource is V1ConfigMap configMap)
                                {
                                    if (configMap.Name() == volume.ConfigMap.Name && configMap.Namespace() == replicaSet.Namespace())
                                    {
                                        graph.Edges.Add(new Edge(start, end));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (start.Resource is V1StatefulSet statefulSet)
            {
                if (statefulSet?.Spec?.Template?.Spec?.Containers != null)
                {
                    foreach (var container in statefulSet.Spec.Template.Spec.Containers)
                    {
                        if (container.Env != null)
                        {
                            foreach (var env in container.Env)
                            {
                                if (env.ValueFrom?.ConfigMapKeyRef != null)
                                {
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Name() == env.ValueFrom.ConfigMapKeyRef.Name && configMap.Namespace() == statefulSet.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (container.EnvFrom != null)
                        {
                            foreach (var envFrom in container.EnvFrom)
                            {
                                if (envFrom.ConfigMapRef != null)
                                {
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Name() == envFrom.ConfigMapRef.Name && configMap.Namespace() == statefulSet.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (statefulSet?.Spec?.Template?.Spec?.InitContainers != null)
                {
                    foreach (var container in statefulSet.Spec.Template.Spec.InitContainers)
                    {
                        if (container.Env != null)
                        {
                            foreach (var env in container.Env)
                            {
                                if (env.ValueFrom?.ConfigMapKeyRef != null)
                                {
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Name() == env.ValueFrom.ConfigMapKeyRef.Name && configMap.Namespace() == statefulSet.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (container.EnvFrom != null)
                        {
                            foreach (var envFrom in container.EnvFrom)
                            {
                                if (envFrom.ConfigMapRef != null)
                                {
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Name() == envFrom.ConfigMapRef.Name && configMap.Namespace() == statefulSet.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (statefulSet?.Spec?.Template?.Spec?.Volumes != null)
                {
                    foreach (var volume in statefulSet.Spec.Template.Spec.Volumes)
                    {
                        if (volume.ConfigMap != null)
                        {
                            foreach (var end in resources)
                            {
                                if (end.Resource is V1ConfigMap configMap)
                                {
                                    if (configMap.Name() == volume.ConfigMap.Name && configMap.Namespace() == statefulSet.Namespace())
                                    {
                                        graph.Edges.Add(new Edge(start, end));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (start.Resource is V1DaemonSet daemonSet)
            {
                if (daemonSet?.Spec?.Template?.Spec?.Containers != null)
                {
                    foreach (var container in daemonSet.Spec.Template.Spec.Containers)
                    {
                        if (container.Env != null)
                        {
                            foreach (var env in container.Env)
                            {
                                if (env.ValueFrom?.ConfigMapKeyRef != null)
                                {
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Name() == env.ValueFrom?.ConfigMapKeyRef.Name && configMap.Namespace() == daemonSet.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (container.EnvFrom != null)
                        {
                            foreach (var envFrom in container.EnvFrom)
                            {
                                if (envFrom.ConfigMapRef != null)
                                {
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Name() == envFrom.ConfigMapRef.Name && configMap.Namespace() == daemonSet.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (daemonSet?.Spec?.Template?.Spec?.InitContainers != null)
                {
                    foreach (var container in daemonSet.Spec.Template.Spec.InitContainers)
                    {
                        if (container.Env != null)
                        {
                            foreach (var env in container.Env)
                            {
                                if (env.ValueFrom?.ConfigMapKeyRef != null)
                                {
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Name() == env.ValueFrom.ConfigMapKeyRef.Name && configMap.Namespace() == daemonSet.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (container.EnvFrom != null)
                        {
                            foreach (var envFrom in container.EnvFrom)
                            {
                                if (envFrom.ConfigMapRef != null)
                                {
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Name() == envFrom.ConfigMapRef.Name && configMap.Namespace() == daemonSet.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (daemonSet?.Spec?.Template?.Spec?.Volumes != null)
                {
                    foreach (var volume in daemonSet.Spec.Template.Spec.Volumes)
                    {
                        if (volume.ConfigMap != null)
                        {
                            foreach (var end in resources)
                            {
                                if (end.Resource is V1ConfigMap configMap)
                                {
                                    if (configMap.Name() == volume.ConfigMap.Name && configMap.Namespace() == daemonSet.Namespace())
                                    {
                                        graph.Edges.Add(new Edge(start, end));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (start.Resource is V1Pod pod)
            {
                if (pod.Spec.Containers != null)
                {
                    foreach (var container in pod.Spec.Containers)
                    {
                        if (container.Env != null)
                        {
                            foreach (var env in container.Env)
                            {
                                if (env.ValueFrom?.ConfigMapKeyRef != null)
                                {
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Name() == env.ValueFrom.ConfigMapKeyRef.Name && configMap.Namespace() == pod.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (container.EnvFrom != null)
                        {
                            foreach (var envFrom in container.EnvFrom)
                            {
                                if (envFrom.ConfigMapRef != null)
                                {
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Name() == envFrom.ConfigMapRef.Name && configMap.Namespace() == pod.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (pod.Spec.InitContainers != null)
                {
                    foreach (var container in pod.Spec.InitContainers)
                    {
                        if (container.Env != null)
                        {
                            foreach (var env in container.Env)
                            {
                                if (env.ValueFrom?.ConfigMapKeyRef != null)
                                {
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Name() == env.ValueFrom.ConfigMapKeyRef.Name && configMap.Namespace() == pod.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (container.EnvFrom != null)
                        {
                            foreach (var envFrom in container.EnvFrom)
                            {
                                if (envFrom.ConfigMapRef != null)
                                {
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1ConfigMap configMap)
                                        {
                                            if (configMap.Name() == envFrom.ConfigMapRef.Name && configMap.Namespace() == pod.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (pod.Spec.Volumes != null)
                {
                    foreach (var volume in pod.Spec.Volumes)
                    {
                        if (volume.ConfigMap != null)
                        {
                            foreach (var end in resources)
                            {
                                if (end.Resource is V1ConfigMap configMap)
                                {
                                    if (configMap.Name() == volume.ConfigMap.Name && configMap.Namespace() == pod.Namespace())
                                    {
                                        graph.Edges.Add(new Edge(start, end));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private static void LinkSecret(ObservableCollection<ResourceNodeViewModel> resources, Graph graph)
    {
        foreach (var start in resources)
        {
            if (start.Resource is V1Deployment deployment)
            {
                if (deployment?.Spec?.Template?.Spec?.Containers != null)
                {
                    foreach (var container in deployment.Spec.Template.Spec.Containers)
                    {
                        if (container.Env != null)
                        {
                            foreach (var env in container.Env)
                            {
                                if (env.ValueFrom?.SecretKeyRef != null)
                                {
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Name() == env.ValueFrom.SecretKeyRef.Name && secret.Namespace() == deployment.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (container.EnvFrom != null)
                        {
                            foreach (var envFrom in container.EnvFrom)
                            {
                                if (envFrom.SecretRef != null)
                                {
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Name() == envFrom.SecretRef.Name && secret.Namespace() == deployment.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (deployment?.Spec?.Template?.Spec?.InitContainers != null)
                {
                    foreach (var container in deployment.Spec.Template.Spec.InitContainers)
                    {
                        if (container.Env != null)
                        {
                            foreach (var env in container.Env)
                            {
                                if (env.ValueFrom?.SecretKeyRef != null)
                                {
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Name() == env.ValueFrom.SecretKeyRef.Name && secret.Namespace() == deployment.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (container.EnvFrom != null)
                        {
                            foreach (var envFrom in container.EnvFrom)
                            {
                                if (envFrom.SecretRef != null)
                                {
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Name() == envFrom.SecretRef.Name && secret.Namespace() == deployment.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (deployment?.Spec?.Template?.Spec?.Volumes != null)
                {
                    foreach (var volume in deployment.Spec.Template.Spec.Volumes)
                    {
                        if (volume.Secret != null)
                        {
                            foreach (var end in resources)
                            {
                                if (end.Resource is V1Secret secret)
                                {
                                    if (secret.Name() == volume.Secret.SecretName && secret.Namespace() == deployment.Namespace())
                                    {
                                        graph.Edges.Add(new Edge(start, end));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (start.Resource is V1ReplicaSet replicaSet)
            {
                if (replicaSet?.Spec?.Template?.Spec?.Containers != null)
                {
                    foreach (var container in replicaSet.Spec.Template.Spec.Containers)
                    {
                        if (container.Env != null)
                        {
                            foreach (var env in container.Env)
                            {
                                if (env.ValueFrom?.SecretKeyRef != null)
                                {
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Name() == env.ValueFrom.SecretKeyRef.Name && secret.Namespace() == replicaSet.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (container.EnvFrom != null)
                        {
                            foreach (var envFrom in container.EnvFrom)
                            {
                                if (envFrom.SecretRef != null)
                                {
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Name() == envFrom.SecretRef.Name && secret.Namespace() == replicaSet.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (replicaSet?.Spec?.Template?.Spec?.InitContainers != null)
                {
                    foreach (var container in replicaSet.Spec.Template.Spec.InitContainers)
                    {
                        if (container.Env != null)
                        {
                            foreach (var env in container.Env)
                            {
                                if (env.ValueFrom?.SecretKeyRef != null)
                                {
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Name() == env.ValueFrom.SecretKeyRef.Name && secret.Namespace() == replicaSet.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (container.EnvFrom != null)
                        {
                            foreach (var envFrom in container.EnvFrom)
                            {
                                if (envFrom.SecretRef != null)
                                {
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Name() == envFrom.SecretRef.Name && secret.Namespace() == replicaSet.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (replicaSet?.Spec?.Template?.Spec?.Volumes != null)
                {
                    foreach (var volume in replicaSet.Spec.Template.Spec.Volumes)
                    {
                        if (volume.Secret != null)
                        {
                            foreach (var end in resources)
                            {
                                if (end.Resource is V1Secret secret)
                                {
                                    if (secret.Name() == volume.Secret.SecretName && secret.Namespace() == replicaSet.Namespace())
                                    {
                                        graph.Edges.Add(new Edge(start, end));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (start.Resource is V1StatefulSet statefulSet)
            {
                if (statefulSet?.Spec?.Template?.Spec?.Containers != null)
                {
                    foreach (var container in statefulSet.Spec.Template.Spec.Containers)
                    {
                        if (container.Env != null)
                        {
                            foreach (var env in container.Env)
                            {
                                if (env.ValueFrom?.SecretKeyRef != null)
                                {
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Name() == env.ValueFrom.SecretKeyRef.Name && secret.Namespace() == statefulSet.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (container.EnvFrom != null)
                        {
                            foreach (var envFrom in container.EnvFrom)
                            {
                                if (envFrom.SecretRef != null)
                                {
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Name() == envFrom.SecretRef.Name && secret.Namespace() == statefulSet.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (statefulSet?.Spec?.Template?.Spec?.InitContainers != null)
                {
                    foreach (var container in statefulSet.Spec.Template.Spec.InitContainers)
                    {
                        if (container.Env != null)
                        {
                            foreach (var env in container.Env)
                            {
                                if (env.ValueFrom?.SecretKeyRef != null)
                                {
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Name() == env.ValueFrom.SecretKeyRef.Name && secret.Namespace() == statefulSet.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (container.EnvFrom != null)
                        {
                            foreach (var envFrom in container.EnvFrom)
                            {
                                if (envFrom.SecretRef != null)
                                {
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Name() == envFrom.SecretRef.Name && secret.Namespace() == statefulSet.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (statefulSet?.Spec?.Template?.Spec?.Volumes != null)
                {
                    foreach (var volume in statefulSet.Spec.Template.Spec.Volumes)
                    {
                        if (volume.Secret != null)
                        {
                            foreach (var end in resources)
                            {
                                if (end.Resource is V1Secret secret)
                                {
                                    if (secret.Name() == volume.Secret.SecretName && secret.Namespace() == statefulSet.Namespace())
                                    {
                                        graph.Edges.Add(new Edge(start, end));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (start.Resource is V1DaemonSet daemonSet)
            {
                if (daemonSet?.Spec?.Template?.Spec?.Containers != null)
                {
                    foreach (var container in daemonSet.Spec.Template.Spec.Containers)
                    {
                        if (container.Env != null)
                        {
                            foreach (var env in container.Env)
                            {
                                if (env.ValueFrom?.SecretKeyRef != null)
                                {
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Name() == env.ValueFrom.SecretKeyRef.Name && secret.Namespace() == daemonSet.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (container.EnvFrom != null)
                        {
                            foreach (var envFrom in container.EnvFrom)
                            {
                                if (envFrom.SecretRef != null)
                                {
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Name() == envFrom.SecretRef.Name && secret.Namespace() == daemonSet.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (daemonSet?.Spec?.Template?.Spec?.InitContainers != null)
                {
                    foreach (var container in daemonSet.Spec.Template.Spec.InitContainers)
                    {
                        if (container.Env != null)
                        {
                            foreach (var env in container.Env)
                            {
                                if (env.ValueFrom?.SecretKeyRef != null)
                                {
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Name() == env.ValueFrom.SecretKeyRef.Name && secret.Namespace() == daemonSet.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (container.EnvFrom != null)
                        {
                            foreach (var envFrom in container.EnvFrom)
                            {
                                if (envFrom.SecretRef != null)
                                {
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Name() == envFrom.SecretRef.Name && secret.Namespace() == daemonSet.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (daemonSet?.Spec?.Template?.Spec?.Volumes != null)
                {
                    foreach (var volume in daemonSet.Spec.Template.Spec.Volumes)
                    {
                        if (volume.Secret != null)
                        {
                            foreach (var end in resources)
                            {
                                if (end.Resource is V1Secret secret)
                                {
                                    if (secret.Name() == volume.Secret.SecretName && secret.Namespace() == daemonSet.Namespace())
                                    {
                                        graph.Edges.Add(new Edge(start, end));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (start.Resource is V1Pod pod)
            {
                if (pod.Spec.Containers != null)
                {
                    foreach (var container in pod.Spec.Containers)
                    {
                        if (container.Env != null)
                        {
                            foreach (var env in container.Env)
                            {
                                if (env.ValueFrom?.SecretKeyRef != null)
                                {
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Name() == env.ValueFrom.SecretKeyRef.Name && secret.Namespace() == pod.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (container.EnvFrom != null)
                        {
                            foreach (var envFrom in container.EnvFrom)
                            {
                                if (envFrom.SecretRef != null)
                                {
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Name() == envFrom.SecretRef.Name && secret.Namespace() == pod.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (pod?.Spec?.InitContainers != null)
                {
                    foreach (var container in pod.Spec.InitContainers)
                    {
                        if (container.Env != null)
                        {
                            foreach (var env in container.Env)
                            {
                                if (env.ValueFrom?.SecretKeyRef != null)
                                {
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Name() == env.ValueFrom.SecretKeyRef.Name && secret.Namespace() == pod.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }

                        if (container.EnvFrom != null)
                        {
                            foreach (var envFrom in container.EnvFrom)
                            {
                                if (envFrom.SecretRef != null)
                                {
                                    foreach (var end in resources)
                                    {
                                        if (end.Resource is V1Secret secret)
                                        {
                                            if (secret.Name() == envFrom.SecretRef.Name && secret.Namespace() == pod.Namespace())
                                            {
                                                graph.Edges.Add(new Edge(start, end));
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    }
                }

                if (pod?.Spec?.Volumes != null)
                {
                    foreach (var volume in pod.Spec.Volumes)
                    {
                        if (volume.Secret != null)
                        {
                            foreach (var end in resources)
                            {
                                if (end.Resource is V1Secret secret)
                                {
                                    if (secret.Name() == volume.Secret.SecretName && secret.Namespace() == pod.Namespace())
                                    {
                                        graph.Edges.Add(new Edge(start, end));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (start.Resource is V1ServiceAccount serviceAccount)
            {
                if (serviceAccount.Secrets != null)
                {
                    foreach (var secretReference in serviceAccount.Secrets)
                    {
                        foreach (var end in resources)
                        {
                            if (end.Resource is V1Secret secret)
                            {
                                if (secret.Uid() == secretReference.Uid && end.Resource.Namespace() == serviceAccount.Namespace())
                                {
                                    graph.Edges.Add(new Edge(start, end));
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private static void LinkEvent(ObservableCollection<ResourceNodeViewModel> resources, Graph graph)
    {
        foreach (var start in resources)
        {
            if (start.Resource is Corev1Event @event)
            {
                foreach (var end in resources)
                {
                    if (end.Resource.Uid() == @event.InvolvedObject.Uid)
                    {
                        graph.Edges.Add(new Edge(start, end));
                    }
                }
            }
        }
    }

    private static void LinkServiceAccount(ObservableCollection<ResourceNodeViewModel> resources, Graph graph)
    {
        foreach (var start in resources)
        {
            if (start.Resource is V1Deployment deployment)
            {
                var templateSpec = deployment.Spec?.Template?.Spec;
                if (templateSpec == null)
                {
                    continue;
                }

                foreach (var end in resources)
                {
                    if (end.Resource is V1ServiceAccount serviceAccount &&
                        serviceAccount.Name() == templateSpec.ServiceAccountName &&
                        end.Resource.Namespace() == deployment.Namespace())
                    {
                        graph.Edges.Add(new Edge(start, end));
                    }
                }
            }

            if (start.Resource is V1ReplicaSet replicaSet)
            {
                var templateSpec = replicaSet.Spec?.Template?.Spec;
                if (templateSpec == null)
                {
                    continue;
                }

                foreach (var end in resources)
                {
                    if (end.Resource is V1ServiceAccount serviceAccount)
                    {
                        if (serviceAccount.Name() == templateSpec.ServiceAccountName && end.Resource.Namespace() == replicaSet.Namespace())
                        {
                            graph.Edges.Add(new Edge(start, end));
                        }
                    }
                }
            }

            if (start.Resource is V1DaemonSet daemonSet)
            {
                var templateSpec = daemonSet.Spec?.Template?.Spec;
                if (templateSpec == null)
                {
                    continue;
                }

                foreach (var end in resources)
                {
                    if (end.Resource is V1ServiceAccount serviceAccount)
                    {
                        if (serviceAccount.Name() == templateSpec.ServiceAccountName && end.Resource.Namespace() == daemonSet.Namespace())
                        {
                            graph.Edges.Add(new Edge(start, end));
                        }
                    }
                }
            }

            if (start.Resource is V1StatefulSet statefulSet)
            {
                var templateSpec = statefulSet.Spec?.Template?.Spec;
                if (templateSpec == null)
                {
                    continue;
                }

                foreach (var end in resources)
                {
                    if (end.Resource is V1ServiceAccount serviceAccount)
                    {
                        if (serviceAccount.Name() == templateSpec.ServiceAccountName && end.Resource.Namespace() == statefulSet.Namespace())
                        {
                            graph.Edges.Add(new Edge(start, end));
                        }
                    }
                }
            }

            if (start.Resource is V1Pod pod)
            {
                var podSpec = pod.Spec;
                if (podSpec == null)
                {
                    continue;
                }

                foreach (var end in resources)
                {
                    if (end.Resource is V1ServiceAccount serviceAccount)
                    {
                        if (serviceAccount.Name() == podSpec.ServiceAccountName && end.Resource.Namespace() == pod.Namespace())
                        {
                            graph.Edges.Add(new Edge(start, end));
                        }
                    }
                }
            }
        }
    }

    private static void LinkPersistantVolumeClaim(ObservableCollection<ResourceNodeViewModel> resources, Graph graph)
    {
        foreach (var start in resources)
        {
            if (start.Resource is V1Deployment deployment)
            {
                if (deployment?.Spec?.Template?.Spec?.Volumes != null)
                {
                    foreach (var volume in deployment.Spec.Template.Spec.Volumes)
                    {
                        if (volume?.PersistentVolumeClaim != null)
                        {
                            foreach (var end in resources)
                            {
                                if (end.Resource is V1PersistentVolumeClaim pvc)
                                {
                                    if (pvc.Name() == volume.PersistentVolumeClaim.ClaimName && pvc.Namespace() == deployment.Namespace())
                                    {
                                        graph.Edges.Add(new Edge(start, end));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (start.Resource is V1ReplicaSet replicaSet)
            {
                if (replicaSet?.Spec?.Template?.Spec?.Volumes != null)
                {
                    foreach (var volume in replicaSet.Spec.Template.Spec.Volumes)
                    {
                        if (volume?.PersistentVolumeClaim != null)
                        {
                            foreach (var end in resources)
                            {
                                if (end.Resource is V1PersistentVolumeClaim pvc)
                                {
                                    if (pvc.Name() == volume.PersistentVolumeClaim.ClaimName && pvc.Namespace() == replicaSet.Namespace())
                                    {
                                        graph.Edges.Add(new Edge(start, end));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (start.Resource is V1StatefulSet statefulSet)
            {
                if (statefulSet?.Spec?.Template?.Spec?.Volumes != null)
                {
                    foreach (var volume in statefulSet.Spec.Template.Spec.Volumes)
                    {
                        if (volume?.PersistentVolumeClaim != null)
                        {
                            foreach (var end in resources)
                            {
                                if (end.Resource is V1PersistentVolumeClaim pvc)
                                {
                                    if (pvc.Name() == volume.PersistentVolumeClaim.ClaimName && pvc.Namespace() == statefulSet.Namespace())
                                    {
                                        graph.Edges.Add(new Edge(start, end));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (start.Resource is V1DaemonSet daemonSet)
            {
                if (daemonSet?.Spec?.Template?.Spec?.Volumes != null)
                {
                    foreach (var volume in daemonSet.Spec.Template.Spec.Volumes)
                    {
                        if (volume?.PersistentVolumeClaim != null)
                        {
                            foreach (var end in resources)
                            {
                                if (end.Resource is V1PersistentVolumeClaim pvc)
                                {
                                    if (pvc.Name() == volume.PersistentVolumeClaim.ClaimName && pvc.Namespace() == daemonSet.Namespace())
                                    {
                                        graph.Edges.Add(new Edge(start, end));
                                    }
                                }
                            }
                        }
                    }
                }
            }

            if (start.Resource is V1Pod pod)
            {
                if (pod.Spec.Volumes != null)
                {
                    foreach (var volume in pod.Spec.Volumes)
                    {
                        if (volume?.PersistentVolumeClaim != null)
                        {
                            foreach (var end in resources)
                            {
                                if (end.Resource is V1PersistentVolumeClaim pvc)
                                {
                                    if (pvc.Name() == volume.PersistentVolumeClaim.ClaimName && pvc.Namespace() == pod.Namespace())
                                    {
                                        graph.Edges.Add(new Edge(start, end));
                                    }
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private static void LinkPersistantVolume(ObservableCollection<ResourceNodeViewModel> resources, Graph graph)
    {
        foreach (var start in resources)
        {
            if (start.Resource is V1PersistentVolumeClaim persistentVolumeClaim)
            {
                if (persistentVolumeClaim?.Spec?.VolumeName != null)
                {
                    foreach (var end in resources)
                    {
                        if (end.Resource is V1PersistentVolume persistentVolume)
                        {
                            if (persistentVolume.Name() == persistentVolumeClaim.Spec.VolumeName)
                            {
                                graph.Edges.Add(new Edge(start, end));
                            }
                        }
                    }
                }
            }
        }
    }

    private static void LinkRoleBinding(ObservableCollection<ResourceNodeViewModel> resources, Graph graph)
    {
        foreach (var start in resources)
        {
            if (start.Resource is V1RoleBinding roleBinding)
            {
                if (roleBinding.Subjects != null)
                {
                    foreach (var subject in roleBinding.Subjects)
                    {
                        foreach (var end in resources)
                        {
                            if (subject.Kind == V1ServiceAccount.KubeKind)
                            {
                                if (end.Resource is V1ServiceAccount serviceAccount)
                                {
                                    if (serviceAccount.Name() == subject.Name
                                        && serviceAccount.Namespace() == subject.NamespaceProperty)
                                    {
                                        graph.Edges.Add(new Edge(start, end));
                                    }
                                }
                            }

                            //todo User

                            //todo Group
                        }
                    }
                }

                if (roleBinding.RoleRef != null)
                {
                    foreach (var end in resources)
                    {
                        if (roleBinding.RoleRef.Kind == V1Role.KubeKind)
                        {
                            if (end.Resource is V1Role role)
                            {
                                if (role.Name() == roleBinding.RoleRef.Name
                                    && role.Namespace() == roleBinding.Namespace())
                                {
                                    graph.Edges.Add(new Edge(end, start));
                                }
                            }
                        }

                        if (roleBinding.RoleRef.Kind == V1ClusterRole.KubeKind)
                        {
                            if (end.Resource is V1ClusterRole clusterRole)
                            {
                                if (clusterRole.Name() == roleBinding.RoleRef.Name)
                                {
                                    graph.Edges.Add(new Edge(end, start));
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private static void LinkClusterRoleBinding(ObservableCollection<ResourceNodeViewModel> resources, Graph graph)
    {
        foreach (var start in resources)
        {
            if (start.Resource is V1ClusterRoleBinding roleBinding)
            {
                if (roleBinding.Subjects != null)
                {
                    foreach (var subject in roleBinding.Subjects)
                    {
                        foreach (var end in resources)
                        {
                            if (subject.Kind == V1ServiceAccount.KubeKind)
                            {
                                if (end.Resource is V1ServiceAccount serviceAccount)
                                {
                                    if (serviceAccount.Name() == subject.Name
                                        && serviceAccount.Namespace() == subject.NamespaceProperty)
                                    {
                                        graph.Edges.Add(new Edge(start, end));
                                    }
                                }
                            }

                            //todo User

                            //todo Group
                        }
                    }
                }

                if (roleBinding.RoleRef != null)
                {
                    foreach (var end in resources)
                    {
                        if (roleBinding.RoleRef.Kind == V1ClusterRole.KubeKind)
                        {
                            if (end.Resource is V1ClusterRole clusterRole)
                            {
                                if (clusterRole.Name() == roleBinding.RoleRef.Name)
                                {
                                    graph.Edges.Add(new Edge(end, start));
                                }
                            }
                        }
                    }
                }
            }
        }
    }

    private static void LinkArgoCDTracking(ObservableCollection<ResourceNodeViewModel> resources, Graph graph)
    {
        foreach (var resource in resources)
        {
            var annotations = resource.Resource?.Metadata?.Annotations;
            if (annotations != null && annotations.TryGetValue("argocd.argoproj.io/tracking-id", out var trackingId))
            {
                var firstColon = trackingId.IndexOf(':');
                if (firstColon > 0)
                {
                    var appName = trackingId.Substring(0, firstColon);
                    // Find Argo Application resource with matching appName, ApiVersion, and Kind
                    var argoApp = resources.FirstOrDefault(r =>
                        r.Resource?.Metadata?.Name == appName &&
                        r.Resource?.ApiVersion == "argoproj.io/v1alpha1" &&
                        r.Resource?.Kind == "Application");
                    if (argoApp != null)
                    {
                        graph.Edges.Add(new Edge(argoApp, resource));
                    }
                }
            }
        }
    }

    private static void LinkFluxCDKustomizationTracking(ObservableCollection<ResourceNodeViewModel> resources, Graph graph)
    {
        foreach (var resource in resources)
        {
            var labels = resource.Resource?.Metadata?.Labels;
            if (labels == null)
            {
                continue;
            }

            if (!labels.TryGetValue("kustomize.toolkit.fluxcd.io/name", out var name))
            {
                continue;
            }

            if (!labels.TryGetValue("kustomize.toolkit.fluxcd.io/namespace", out var namespaceProperty))
            {
                continue;
            }

            var kustomization = resources.FirstOrDefault(r =>
                r.Resource?.Metadata?.Name == name &&
                r.Resource?.Metadata?.NamespaceProperty == namespaceProperty &&
                r.Resource?.ApiVersion == "kustomize.toolkit.fluxcd.io/v1" &&
                r.Resource?.Kind == "Kustomization");

            if (kustomization != null && !ReferenceEquals(kustomization, resource))
            {
                graph.Edges.Add(new Edge(kustomization, resource));
            }
        }
    }

    private static void LinkFluxCDHelmReleaseTracking(ObservableCollection<ResourceNodeViewModel> resources, Graph graph)
    {
        foreach (var resource in resources)
        {
            var labels = resource.Resource?.Metadata?.Labels;
            if (labels == null)
            {
                continue;
            }

            if (!labels.TryGetValue("helm.toolkit.fluxcd.io/name", out var name))
            {
                continue;
            }

            if (!labels.TryGetValue("helm.toolkit.fluxcd.io/namespace", out var namespaceProperty))
            {
                continue;
            }

            var helmRelease = resources.FirstOrDefault(r =>
                r.Resource?.Metadata?.Name == name &&
                r.Resource?.Metadata?.NamespaceProperty == namespaceProperty &&
                r.Resource?.ApiVersion == "helm.toolkit.fluxcd.io/v2" &&
                r.Resource?.Kind == "HelmRelease");

            if (helmRelease != null && !ReferenceEquals(helmRelease, resource))
            {
                graph.Edges.Add(new Edge(helmRelease, resource));
            }
        }
    }

    #endregion

    public void Dispose()
    {
        _disposed = true;

        var cluster = Cluster;
        if (cluster != null)
        {
            if (cluster.SelectedNamespaces != null)
            {
                cluster.SelectedNamespaces.CollectionChanged -= SelectedNamespaces_CollectionChanged;
            }

            cluster.OnChange -= Cluster_OnChange;
            cluster.ResourceSeeded -= Cluster_ResourceSeeded;
        }
        _refreshSubscription.Dispose();
        _refreshRequests.Dispose();
        Cluster = null;
        // Clear heavy collections to avoid retaining large object graphs between runs
        try
        {
            Resources?.Clear();
        }
        catch { }

        try
        {
            Graph = new Graph();
        }
        catch { }
    }

    public sealed partial class ResourceNodeViewModel : ViewModelBase
    {
        [ObservableProperty]
        public partial ClusterWorkspaceViewModel Cluster { get; set; }

        [ObservableProperty]
        public partial IKubernetesObject<V1ObjectMeta> Resource { get; set; }

        [ObservableProperty]
        public partial string IconPath { get; set; }

        [ObservableProperty]
        public partial bool IsRoot { get; set; }

        public IEnumerable<MenuItemViewModel> ContextMenuItems
        {
            get
            {
                if (Cluster == null || Resource == null)
                {
                    return [];
                }

                var resourceConfig = Cluster.GetResourceConfigs().FirstOrDefault(config => config.Type == Resource.GetType());
                if (resourceConfig == null)
                {
                    return [];
                }

                var selectedItems = new[] { Resource };
                var items = new List<MenuItemViewModel>();
                items.AddRange(resourceConfig.GetDefaultMenuItems(selectedItems));

                var customItems = resourceConfig.GetCustomMenuItems(selectedItems).ToList();
                if (customItems.Count > 0)
                {
                    items.Add(new MenuItemViewModel
                    {
                        IsSeparator = true
                    });
                    items.AddRange(customItems);
                }

                return items;
            }
        }
    }

    public sealed partial class GroupNodeViewModel : ViewModelBase
    {
        [ObservableProperty]
        public partial GroupKind GroupKind { get; set; }

        [ObservableProperty]
        public partial string Name { get; set; } = string.Empty;

        [ObservableProperty]
        public partial string DisplayText { get; set; } = string.Empty;

        internal Microsoft.Msagl.Drawing.Subgraph? DNode { get; set; }
    }

    public enum GroupKind
    {
        Namespace,
        Component,
    }

    public sealed class GroupEdge : Edge
    {
        public GroupEdge(object tail, object head)
            : base(tail, head, null, Symbol.None, Symbol.None)
        {
        }
    }
}
