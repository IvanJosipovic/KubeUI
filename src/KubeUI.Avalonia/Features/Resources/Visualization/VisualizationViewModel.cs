using System.Collections.Specialized;
using System.Reactive.Linq;
using k8s;
using k8s.Models;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Kubernetes;
using KubeUI.Kubernetes.Resources.Relationships;

namespace KubeUI.Avalonia.Features.Resources.Visualization;

public sealed partial class VisualizationViewModel : ViewModelBase, IInitializeCluster, IDisposable
{
    private readonly IResourceRelationshipBuilder _resourceRelationshipBuilder;
    private readonly Dictionary<ResourceKey, IKubernetesObject<V1ObjectMeta>> _resourcesByKey = [];
    private bool _disposed;
    private bool _suppressResourceChanges;
    private IDisposable? _resourceChangesSubscription;
    private int _rebuildVersion;

    [ObservableProperty]
    public partial IKubernetesObject<V1ObjectMeta>? RootResource { get; set; }

    public string RootResourceDisplay => RootResource == null
        ? string.Empty
        : $"Resource: {RootResource.ApiVersion}/{RootResource.Kind} {RootResource.Namespace()}/{RootResource.Name()}";

    partial void OnRootResourceChanged(IKubernetesObject<V1ObjectMeta>? value)
    {
        OnPropertyChanged(nameof(RootResourceDisplay));
        Run();
    }

    [ObservableProperty]
    public partial ClusterWorkspace? Cluster { get; set; }

    [ObservableProperty]
    public partial ResourceRelationshipGraph? Graph { get; set; } = ResourceRelationshipGraph.Empty;

    [ObservableProperty]
    public partial bool HideNoise { get; set; } = true;

    public VisualizationViewModel()
    {
        _resourceRelationshipBuilder = new ResourceRelationshipBuilder();
        Title = Assets.Resources.VisualizationView_Title;
    }

    internal VisualizationViewModel(IResourceRelationshipBuilder resourceRelationshipBuilder)
    {
        _resourceRelationshipBuilder = resourceRelationshipBuilder;
        Title = Assets.Resources.VisualizationView_Title;
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.PropertyName == nameof(HideNoise))
        {
            Run();
        }
    }

    public void Initialize(ClusterWorkspace cluster) => Initialize(cluster, null);

    public void Initialize(ClusterWorkspace cluster, IKubernetesObject<V1ObjectMeta>? rootResource)
    {
        ArgumentNullException.ThrowIfNull(cluster);
        UnsubscribeCluster();
        Cluster = cluster;
        RootResource = rootResource;
        Id = nameof(VisualizationViewModel) + "-" + cluster;

        _resourcesByKey.Clear();
        _suppressResourceChanges = true;
        _resourceChangesSubscription = cluster.Runtime.ConnectResources().Subscribe(OnResourceChange);

        cluster.SelectedNamespaces.CollectionChanged += SelectedNamespaces_CollectionChanged;
        _ = InitializeResourcesAsync(cluster);
    }

    private static readonly Type[] SeedTypes =
    [
        typeof(V1Node), typeof(Corev1Event), typeof(V1Pod), typeof(V1ReplicaSet), typeof(V1Deployment),
        typeof(V1StatefulSet), typeof(V1DaemonSet), typeof(V1CronJob), typeof(V1Job), typeof(V1Secret),
        typeof(V1ConfigMap), typeof(V1Service), typeof(V1EndpointSlice), typeof(V1Ingress), typeof(V1IngressClass),
        typeof(V1PersistentVolumeClaim), typeof(V1PersistentVolume), typeof(V1ServiceAccount), typeof(V1RoleBinding),
        typeof(V1ClusterRoleBinding), typeof(V1Role), typeof(V1ClusterRole),
    ];

    private void SelectedNamespaces_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => Run();

    private void OnResourceChange(ResourceChange change)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            ApplyResourceChange(change);
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            if (!_disposed)
            {
                ApplyResourceChange(change);
            }
        });
    }

    private void ApplyResourceChange(ResourceChange change)
    {
        if (_disposed)
        {
            return;
        }

        ResourceKey key = GetResourceKey(change.Resource);
        if (change.EventType == WatchEventType.Deleted)
        {
            _resourcesByKey.Remove(key);
            RemoveResourceFromGraph(change.Resource);
            return;
        }

        _resourcesByKey[key] = change.Resource;
        if (change.EventType == WatchEventType.Modified)
        {
            ReplaceResourceInGraph(change.Resource);
            return;
        }

        if (change.EventType == WatchEventType.Added)
        {
            if (!_suppressResourceChanges && ResourceCanAffectGraph(change.Resource))
            {
                _ = AddResourceIncrementallyAsync(change.Resource, key);
            }

            return;
        }

        if (!_suppressResourceChanges && ResourceCanAffectGraph(change.Resource))
        {
            Run();
        }
    }

    private async Task AddResourceIncrementallyAsync(
        IKubernetesObject<V1ObjectMeta> resource,
        ResourceKey key)
    {
        ClusterWorkspace? cluster = Cluster;
        if (cluster == null)
        {
            return;
        }

        HashSet<string> namespaces = cluster.SelectedNamespaces.Select(x => x.Name()).OfType<string>().ToHashSet(StringComparer.Ordinal);
        ResourceRelationshipGraph delta = await Task.Run(() => _resourceRelationshipBuilder.BuildAdditionDelta(
            _resourcesByKey.Values.ToArray(),
            key,
            namespaces,
            HideNoise)).ConfigureAwait(false);

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (_disposed || !ReferenceEquals(Cluster, cluster) || !_resourcesByKey.ContainsKey(key) || delta.Resources.Count == 0)
            {
                return;
            }

            ResourceIdentity identity = new(resource.ApiVersion ?? string.Empty, resource.Kind ?? string.Empty, resource.Namespace(), resource.Name() ?? string.Empty, resource.Uid());
            ResourceRelationshipGraph current = Graph ?? ResourceRelationshipGraph.Empty;
            HashSet<ResourceIdentity> currentIdentities = current.Resources.Select(GetIdentity).ToHashSet();
            if (RootResource != null
                && identity != GetIdentity(RootResource)
                && !delta.Relationships.Any(relationship => currentIdentities.Contains(relationship.Source) || currentIdentities.Contains(relationship.Target)))
            {
                return;
            }

            IReadOnlyList<IKubernetesObject<V1ObjectMeta>> resources = current.Resources
                .Concat(delta.Resources)
                .GroupBy(GetIdentity)
                .Select(group => group.First())
                .ToArray();
            IReadOnlyList<ResourceRelationship> relationships = ResourceRelationshipBuilder.SimplifyRelationships(
                current.Relationships.Concat(delta.Relationships));
            Graph = new ResourceRelationshipGraph(resources, relationships);
        });
    }

    private async Task InitializeResourcesAsync(ClusterWorkspace cluster)
    {
        try
        {
            await Task.WhenAll(SeedTypes.Select(type => cluster.Runtime.SeedResource(type))).ConfigureAwait(false);
            await SeedOwnerReferenceResourceTypesAsync().ConfigureAwait(false);
        }
        finally
        {
            if (!_disposed && ReferenceEquals(Cluster, cluster))
            {
                _suppressResourceChanges = false;
                Run();
            }
        }
    }

    private bool ResourceCanAffectGraph(IKubernetesObject<V1ObjectMeta> resource)
    {
        if (HideNoise && (resource is Corev1Event || resource is V1ReplicaSet replicaSet && replicaSet.Status?.Replicas == 0))
        {
            return false;
        }

        ClusterWorkspace? cluster = Cluster;
        if (cluster == null)
        {
            return false;
        }

        if (RootResource == null && cluster.SelectedNamespaces.Count == 0)
        {
            return false;
        }

        if (cluster.SelectedNamespaces.Count == 0)
        {
            return true;
        }

        string? namespaceName = resource.Namespace();
        return string.IsNullOrEmpty(namespaceName)
            || cluster.SelectedNamespaces.Any(selected => selected.Name() == namespaceName);
    }

    private void Run()
    {
        if (_disposed)
        {
            return;
        }

        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(Run);
            return;
        }

        int version = Interlocked.Increment(ref _rebuildVersion);

        ClusterWorkspace? cluster = Cluster;
        if (cluster == null || (RootResource == null && cluster.SelectedNamespaces.Count == 0))
        {
            Graph = ResourceRelationshipGraph.Empty;
            return;
        }

        HashSet<string> namespaces = cluster.SelectedNamespaces.Select(x => x.Name()).OfType<string>().ToHashSet(StringComparer.Ordinal);
        IKubernetesObject<V1ObjectMeta>? root = RootResource;
        bool hideNoise = HideNoise;
        IReadOnlyList<IKubernetesObject<V1ObjectMeta>> source = _resourcesByKey.Values.ToArray();

        _ = Task.Run(() => BuildGraph(source, root, namespaces, hideNoise))
            .ContinueWith(task =>
            {
                if (task.IsFaulted || task.IsCanceled)
                {
                    return;
                }

                Dispatcher.UIThread.Post(() =>
                {
                    if (!_disposed && version == _rebuildVersion)
                    {
                        Graph = task.Result;
                    }
                });
            }, TaskScheduler.Default);
    }

    private ResourceRelationshipGraph BuildGraph(
        IReadOnlyList<IKubernetesObject<V1ObjectMeta>> source,
        IKubernetesObject<V1ObjectMeta>? root,
        IReadOnlySet<string> namespaces,
        bool hideNoise)
    {
        ResourceRelationshipGraph graph = _resourceRelationshipBuilder.Build(source, namespaces, hideNoise);
        if (root != null)
        {
            graph = FilterToRootResource(graph, root);
        }

        return graph;
    }

    private void ReplaceResourceInGraph(IKubernetesObject<V1ObjectMeta> resource)
    {
        ResourceRelationshipGraph current = Graph ?? ResourceRelationshipGraph.Empty;
        ResourceIdentity identity = GetIdentity(resource);
        if (!current.Resources.Any(item => GetIdentity(item) == identity))
        {
            return;
        }

        Graph = new ResourceRelationshipGraph(
            current.Resources.Select(item => GetIdentity(item) == identity ? resource : item).ToArray(),
            current.Relationships);
    }

    private void RemoveResourceFromGraph(IKubernetesObject<V1ObjectMeta> resource)
    {
        ResourceRelationshipGraph current = Graph ?? ResourceRelationshipGraph.Empty;
        ResourceIdentity identity = GetIdentity(resource);
        if (!current.Resources.Any(item => GetIdentity(item) == identity))
        {
            return;
        }

        Graph = new ResourceRelationshipGraph(
            current.Resources.Where(item => GetIdentity(item) != identity).ToArray(),
            current.Relationships.Where(relationship => relationship.Source != identity && relationship.Target != identity).ToArray());
    }

    private static ResourceRelationshipGraph FilterToRootResource(ResourceRelationshipGraph graph, IKubernetesObject<V1ObjectMeta> root)
    {
        ResourceIdentity rootIdentity = new(root.ApiVersion ?? string.Empty, root.Kind ?? string.Empty, root.Namespace(), root.Name() ?? string.Empty, root.Uid());
        HashSet<ResourceIdentity> reachable = [rootIdentity];
        bool changed;
        do
        {
            changed = false;
            foreach (ResourceRelationship relationship in graph.Relationships)
            {
                if (reachable.Contains(relationship.Source) && reachable.Add(relationship.Target))
                {
                    changed = true;
                }
                else if (reachable.Contains(relationship.Target) && reachable.Add(relationship.Source))
                {
                    changed = true;
                }
            }
        } while (changed);

        IReadOnlyList<IKubernetesObject<V1ObjectMeta>> resources = graph.Resources.Where(resource => reachable.Contains(new ResourceIdentity(resource.ApiVersion ?? string.Empty, resource.Kind ?? string.Empty, resource.Namespace(), resource.Name() ?? string.Empty, resource.Uid()))).ToArray();
        IReadOnlyList<ResourceRelationship> relationships = graph.Relationships.Where(x => reachable.Contains(x.Source) && reachable.Contains(x.Target)).ToArray();
        return new ResourceRelationshipGraph(resources, relationships);
    }

    private async Task SeedOwnerReferenceResourceTypesAsync()
    {
        ClusterWorkspace? cluster = Cluster;
        if (cluster == null)
        {
            return;
        }

        foreach (IKubernetesObject<V1ObjectMeta> resource in _resourcesByKey.Values.ToArray())
        {
            foreach (V1OwnerReference owner in resource.Metadata?.OwnerReferences ?? [])
            {
                if (owner.Kind == V1Namespace.KubeKind || string.IsNullOrWhiteSpace(owner.ApiVersion) || string.IsNullOrWhiteSpace(owner.Kind))
                {
                    continue;
                }

                int slash = owner.ApiVersion.IndexOf('/');
                string group = slash < 0 ? string.Empty : owner.ApiVersion[..slash];
                string version = slash < 0 ? owner.ApiVersion : owner.ApiVersion[(slash + 1)..];
                Type? type = cluster.Runtime.ModelCache.GetResourceType(group, version, owner.Kind);
                if (type != null)
                {
                    await cluster.Runtime.SeedResource(type).ConfigureAwait(false);
                }
            }
        }
    }

    private static bool IsSameResource(IKubernetesObject<V1ObjectMeta> left, IKubernetesObject<V1ObjectMeta> right)
        => left.GetType() == right.GetType() && left.Namespace() == right.Namespace() && left.Name() == right.Name();

    private static ResourceIdentity GetIdentity(IKubernetesObject<V1ObjectMeta> resource)
        => new(resource.ApiVersion ?? string.Empty, resource.Kind ?? string.Empty, resource.Namespace(), resource.Name() ?? string.Empty, resource.Uid());

    private static ResourceKey GetResourceKey(IKubernetesObject<V1ObjectMeta> resource)
        => new(resource.ApiVersion ?? string.Empty, resource.Kind ?? string.Empty, resource.Namespace(), resource.Name() ?? string.Empty);

    private void UnsubscribeCluster()
    {
        if (Cluster != null)
        {
            Cluster.SelectedNamespaces.CollectionChanged -= SelectedNamespaces_CollectionChanged;
        }

        _resourceChangesSubscription?.Dispose();
        _resourceChangesSubscription = null;
    }

    public void Dispose()
    {
        _disposed = true;
        Interlocked.Increment(ref _rebuildVersion);
        UnsubscribeCluster();
        _resourcesByKey.Clear();
        Graph = ResourceRelationshipGraph.Empty;
        Cluster = null;
    }
}
