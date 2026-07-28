using System.Collections.Specialized;
using System.Reactive.Linq;
using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Avalonia.Resources;
using KubeUI.Kubernetes;
using KubeUI.Kubernetes.Resources.Relationships;

namespace KubeUI.Avalonia.Features.Resources.Visualization;

public sealed partial class VisualizationViewModel : ViewModelBase, IInitializeCluster, IDisposable
{
    private readonly IResourceRelationshipBuilder _resourceRelationshipBuilder;
    private readonly Dictionary<ResourceKey, IKubernetesObject<V1ObjectMeta>> _resourcesByKey = [];
    private readonly Dictionary<string, HashSet<ResourceKey>> _resourcesByOwnerUid = new(StringComparer.Ordinal);
    private readonly HashSet<Type> _requiredSeedTypes = [];
    private readonly HashSet<string> _knownResourceTypes = new(StringComparer.Ordinal);
    private readonly HashSet<string> _excludedResourceTypes = new(StringComparer.Ordinal);
    private bool _disposed;
    private bool _suppressResourceChanges;
    private bool _suppressResourceTypeChanges;
    private IDisposable? _resourceChangesSubscription;
    private CancellationTokenSource? _rebuildCancellation;
    private RebuildRequest? _pendingRebuild;
    private bool _rebuildRunning;
    private int _rebuildVersion;
    private readonly ObservableCollection<V1Namespace> _localSelectedNamespaces = [];
    private ResourceRelationshipGraph _completeGraph = ResourceRelationshipGraph.Empty;

    [ObservableProperty]
    public partial IKubernetesObject<V1ObjectMeta>? RootResource { get; set; }

    public string RootResourceDisplay => RootResource == null
        ? string.Empty
        : $"Resource: {RootResource.ApiVersion}/{RootResource.Kind} {RootResource.Namespace()}/{RootResource.Name()}";

    partial void OnRootResourceChanged(IKubernetesObject<V1ObjectMeta>? value)
    {
        OnPropertyChanged(nameof(RootResourceDisplay));
        OnPropertyChanged(nameof(IsNamespaceSelectorVisible));
        Run();
    }

    [ObservableProperty]
    public partial ClusterWorkspace? Cluster { get; set; }

    [ObservableProperty]
    public partial ResourceRelationshipGraph? Graph { get; set; } = ResourceRelationshipGraph.Empty;

    public ObservableCollection<string> ResourceTypes { get; } = [];

    public ObservableCollection<string> SelectedResourceTypes { get; } = [];

    public bool HasResourceTypes => ResourceTypes.Count > 0;

    [ObservableProperty]
    public partial bool HideNoise { get; set; } = true;

    [ObservableProperty]
    public partial bool ShowNotReadyOnly { get; set; }

    [ObservableProperty]
    public partial bool IsNamespaceSelectionLinked { get; set; } = true;

    public ObservableCollection<V1Namespace> SelectedNamespaces
        => IsNamespaceSelectionLinked && Cluster != null ? Cluster.SelectedNamespaces : _localSelectedNamespaces;

    public bool IsNamespaceSelectorVisible => RootResource == null || RootResource is V1Namespace;

    public VisualizationViewModel()
    {
        _resourceRelationshipBuilder = new ResourceRelationshipBuilder();
        SelectedResourceTypes.CollectionChanged += SelectedResourceTypes_CollectionChanged;
        Title = Assets.Resources.VisualizationView_Title;
    }

    internal VisualizationViewModel(IResourceRelationshipBuilder resourceRelationshipBuilder)
    {
        _resourceRelationshipBuilder = resourceRelationshipBuilder;
        SelectedResourceTypes.CollectionChanged += SelectedResourceTypes_CollectionChanged;
        Title = Assets.Resources.VisualizationView_Title;
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.PropertyName == nameof(HideNoise))
        {
            Run();
        }
        else if (e.PropertyName == nameof(ShowNotReadyOnly))
        {
            ApplyTypeFilter();
        }
    }

    public void Initialize(ClusterWorkspace cluster) => Initialize(cluster, null);

    public void Initialize(ClusterWorkspace cluster, IKubernetesObject<V1ObjectMeta>? rootResource)
    {
        ArgumentNullException.ThrowIfNull(cluster);
        UnsubscribeCluster();
        Cluster = cluster;
        bool isNamespaceRoot = rootResource is V1Namespace;
        RootResource = isNamespaceRoot ? null : rootResource;
        IsNamespaceSelectionLinked = !isNamespaceRoot;
        SelectRootNamespace(rootResource);
        Id = nameof(VisualizationViewModel) + "-" + cluster + "-" + (rootResource?.Uid() ?? "null");

        _resourcesByKey.Clear();
        _resourcesByOwnerUid.Clear();
        _requiredSeedTypes.Clear();
        _completeGraph = ResourceRelationshipGraph.Empty;
        _knownResourceTypes.Clear();
        _excludedResourceTypes.Clear();
        ResourceTypes.Clear();
        OnPropertyChanged(nameof(HasResourceTypes));
        _suppressResourceTypeChanges = true;
        SelectedResourceTypes.Clear();
        _suppressResourceTypeChanges = false;
        _suppressResourceChanges = true;
        _resourceChangesSubscription = cluster.Runtime.ConnectResources().Subscribe(OnResourceChange);

        SubscribeToSelectedNamespaces();
        cluster.ResourceConfigProcessed += ResourceConfigProcessed;
        foreach (Type type in SeedTypes)
        {
            RequireSeed(cluster, type);
        }

        if (RootResource == null)
        {
            foreach (IResourceConfig resourceConfig in cluster.GetResourceConfigs())
            {
                if (resourceConfig.IsCustomResource && resourceConfig.IsNamespaced)
                {
                    RequireSeed(cluster, resourceConfig.Type);
                }
            }
        }

        _suppressResourceChanges = false;
        Run();
    }

    private static readonly Type[] SeedTypes =
    [
        typeof(V1Node),
        typeof(Corev1Event),

        typeof(V1Pod),
        typeof(V1ReplicaSet),
        typeof(V1Deployment),
        typeof(V1StatefulSet),
        typeof(V1DaemonSet),
        typeof(V1CronJob),
        typeof(V1Job),

        typeof(V1Secret),
        typeof(V1ConfigMap),
        typeof(V1Service),
        typeof(V1EndpointSlice),

        typeof(V1Ingress),
        typeof(V1IngressClass),

        typeof(V1PersistentVolumeClaim),
        typeof(V1PersistentVolume),
        typeof(V1StorageClass),
        typeof(V1ServiceAccount),

        typeof(V1RoleBinding),
        typeof(V1ClusterRoleBinding),
        typeof(V1Role),
        typeof(V1ClusterRole),
    ];

    private void SelectedNamespaces_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => Run();

    private void SelectedResourceTypes_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_suppressResourceTypeChanges)
        {
            return;
        }

        if (e.OldItems != null)
        {
            foreach (string type in e.OldItems.OfType<string>())
            {
                _excludedResourceTypes.Add(type);
            }
        }

        if (e.NewItems != null)
        {
            foreach (string type in e.NewItems.OfType<string>())
            {
                _excludedResourceTypes.Remove(type);
            }
        }

        ApplyTypeFilter();
    }

    internal void ApplyGraph(ResourceRelationshipGraph graph)
    {
        _completeGraph = graph;
        UpdateResourceTypes(graph);
        ApplyTypeFilter();
    }

    private void UpdateResourceTypes(ResourceRelationshipGraph graph)
    {
        HashSet<string> availableTypes = graph.Resources
            .Select(resource => resource.Kind)
            .OfType<string>()
            .Where(type => !string.IsNullOrWhiteSpace(type))
            .ToHashSet(StringComparer.Ordinal);

        _suppressResourceTypeChanges = true;
        try
        {
            foreach (string type in availableTypes.Order(StringComparer.Ordinal))
            {
                if (_knownResourceTypes.Add(type) && !_excludedResourceTypes.Contains(type))
                {
                    SelectedResourceTypes.Add(type);
                }
            }

            foreach (string type in ResourceTypes.Where(type => !availableTypes.Contains(type)).ToArray())
            {
                _knownResourceTypes.Remove(type);
                ResourceTypes.Remove(type);
                SelectedResourceTypes.Remove(type);
            }

            ResourceTypes.Clear();
            foreach (string type in availableTypes.Order(StringComparer.Ordinal))
            {
                ResourceTypes.Add(type);
            }

            OnPropertyChanged(nameof(HasResourceTypes));
        }
        finally
        {
            _suppressResourceTypeChanges = false;
        }
    }

    private void ApplyTypeFilter()
    {
        HashSet<string> selectedTypes = SelectedResourceTypes.ToHashSet(StringComparer.Ordinal);
        IReadOnlyList<IKubernetesObject<V1ObjectMeta>> resources = _completeGraph.Resources
            .Where(resource => resource.Kind is string kind && selectedTypes.Contains(kind))
            .Where(resource => !ShowNotReadyOnly || ResourceReadiness.IsNotReady(resource))
            .ToArray();
        HashSet<ResourceIdentity> identities = resources.Select(GetIdentity).ToHashSet();
        Graph = new ResourceRelationshipGraph(
            resources,
            _completeGraph.Relationships.Where(relationship => identities.Contains(relationship.Source) && identities.Contains(relationship.Target)).ToArray());
    }

    partial void OnIsNamespaceSelectionLinkedChanged(bool value)
    {
        if (Cluster == null)
        {
            OnPropertyChanged(nameof(SelectedNamespaces));
            return;
        }

        if (!value)
        {
            CopyNamespaces(Cluster.SelectedNamespaces, _localSelectedNamespaces);
        }

        SubscribeToSelectedNamespaces();
        OnPropertyChanged(nameof(SelectedNamespaces));
        Run();
    }

    private void SelectRootNamespace(IKubernetesObject<V1ObjectMeta>? rootResource)
    {
        if (rootResource is not V1Namespace namespaceResource || namespaceResource.Name() is not string namespaceName)
        {
            return;
        }

        V1Namespace selectedNamespace = Cluster!.Runtime.Namespaces.FirstOrDefault(x => x.Name() == namespaceName) ?? namespaceResource;
        _localSelectedNamespaces.Clear();
        _localSelectedNamespaces.Add(selectedNamespace);
    }

    private static void CopyNamespaces(IEnumerable<V1Namespace> source, ObservableCollection<V1Namespace> target)
    {
        target.Clear();
        foreach (V1Namespace selectedNamespace in source)
        {
            target.Add(selectedNamespace);
        }
    }

    private void SubscribeToSelectedNamespaces()
    {
        UnsubscribeFromSelectedNamespaces();
        SelectedNamespaces.CollectionChanged += SelectedNamespaces_CollectionChanged;
    }

    private void UnsubscribeFromSelectedNamespaces()
    {
        Cluster?.SelectedNamespaces.CollectionChanged -= SelectedNamespaces_CollectionChanged;
        _localSelectedNamespaces.CollectionChanged -= SelectedNamespaces_CollectionChanged;
    }

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

        int changeVersion = Interlocked.Increment(ref _rebuildVersion);

        ResourceKey key = GetResourceKey(change.Resource);
        if (change.EventType == WatchEventType.Deleted)
        {
            RemoveOwnerReferenceIndex(change.Resource, key);
            _resourcesByKey.Remove(key);
            RemoveResourceFromGraph(change.Resource);
            return;
        }

        if (_resourcesByKey.TryGetValue(key, out IKubernetesObject<V1ObjectMeta>? previousResource))
        {
            RemoveOwnerReferenceIndex(previousResource, key);
        }

        _resourcesByKey[key] = change.Resource;
        AddOwnerReferenceIndex(change.Resource, key);
        SeedOwnerReferenceResourceTypes(change.Resource);

        if (_suppressResourceChanges)
        {
            return;
        }

        if (change.EventType == WatchEventType.Modified)
        {
            Run();
            return;
        }

        if (change.EventType == WatchEventType.Added)
        {
            if (HasOwnerReferencesTo(change.Resource)
                || _rebuildRunning
                || _pendingRebuild != null)
            {
                Run();
                return;
            }

            if (ResourceCanAffectGraph(change.Resource))
            {
                _ = AddResourceIncrementallyAsync(change.Resource, key, changeVersion);
            }

            return;
        }

        if (ResourceCanAffectGraph(change.Resource))
        {
            Run();
        }
    }

    private async Task AddResourceIncrementallyAsync(
        IKubernetesObject<V1ObjectMeta> resource,
        ResourceKey key,
        int changeVersion)
    {
        ClusterWorkspace? cluster = Cluster;
        if (cluster == null)
        {
            return;
        }

        HashSet<string> namespaces = SelectedNamespaces.Select(x => x.Name()).OfType<string>().ToHashSet(StringComparer.Ordinal);
        bool hideNoise = HideNoise;
        IReadOnlyList<IKubernetesObject<V1ObjectMeta>> source = _resourcesByKey.Values.ToArray();
        IReadOnlySet<string> buildNamespaces = RootResource == null
            ? namespaces
            : new HashSet<string>(StringComparer.Ordinal);
        ResourceRelationshipGraph delta = await Task.Run(() => _resourceRelationshipBuilder.BuildAdditionDelta(
            source,
            key,
            buildNamespaces,
            hideNoise)).ConfigureAwait(false);

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (_disposed
                || changeVersion != _rebuildVersion
                || !ReferenceEquals(Cluster, cluster)
                || !_resourcesByKey.TryGetValue(key, out IKubernetesObject<V1ObjectMeta>? currentResource)
                || !ReferenceEquals(currentResource, resource)
                || delta.Resources.Count == 0)
            {
                return;
            }

                if (_rebuildRunning || _pendingRebuild != null)
                {
                    Run();
                    return;
                }

            ResourceIdentity identity = new(resource.ApiVersion ?? string.Empty, resource.Kind ?? string.Empty, resource.Namespace(), resource.Name() ?? string.Empty, resource.Uid());
            ResourceRelationshipGraph current = _completeGraph;
            HashSet<ResourceIdentity> currentIdentities = current.Resources.Select(GetIdentity).ToHashSet();
            if (RootResource != null
                && identity != GetIdentity(RootResource)
                && !delta.Relationships.Any(relationship => currentIdentities.Contains(relationship.Source) || currentIdentities.Contains(relationship.Target)))
            {
                Run();
                return;
            }

            IReadOnlyList<IKubernetesObject<V1ObjectMeta>> resources = current.Resources
                .Concat(delta.Resources)
                .GroupBy(GetIdentity)
                .Select(group => group.First())
                .ToArray();
            IReadOnlyList<ResourceRelationship> relationships = ResourceRelationshipBuilder.SimplifyRelationships(
                current.Relationships.Concat(delta.Relationships));
            ResourceRelationshipGraph merged = new(resources, relationships);
            ResourceRelationshipGraph completeGraph = RootResource is { } root
                ? FilterToRootResource(merged, root)
                : merged;
            ApplyGraph(completeGraph);
        });
    }

    private void ResourceConfigProcessed(ClusterWorkspace cluster, IResourceConfig resourceConfig)
    {
        if (_disposed || !ReferenceEquals(Cluster, cluster))
        {
            return;
        }

        if (_requiredSeedTypes.Contains(resourceConfig.Type)
            && resourceConfig.PermissionsLoaded
            && resourceConfig.CanListAndWatch)
        {
            _ = cluster.Runtime.SeedResource(resourceConfig.Type);
        }

        bool ownerReferenceFound = false;
        IReadOnlyList<IKubernetesObject<V1ObjectMeta>> resources = _resourcesByKey.Values.ToArray();
        foreach (IKubernetesObject<V1ObjectMeta> resource in resources)
        {
            ownerReferenceFound |= SeedOwnerReferenceResourceType(
                resource,
                resourceConfig.Type,
                resourceConfig.PermissionsLoaded && resourceConfig.CanListAndWatch);
        }

        if (ownerReferenceFound)
        {
            Run();
        }
    }

    private void RequireSeed(ClusterWorkspace cluster, Type resourceType)
    {
        _requiredSeedTypes.Add(resourceType);

        IResourceConfig? resourceConfig = cluster.GetResourceConfig(resourceType);
        if (resourceConfig is { PermissionsLoaded: true, CanListAndWatch: true })
        {
            _ = cluster.Runtime.SeedResource(resourceType);
        }
    }

    private bool ResourceCanAffectGraph(IKubernetesObject<V1ObjectMeta> resource)
    {
        if (HideNoise && (resource is Corev1Event
            || resource is V1ReplicaSet replicaSet && replicaSet.Status?.Replicas == 0
            || resource is V1Pod pod && pod.Status?.Phase == "Succeeded"))
        {
            return false;
        }

        ClusterWorkspace? cluster = Cluster;
        if (cluster == null)
        {
            return false;
        }

        if (RootResource == null && SelectedNamespaces.Count == 0)
        {
            return false;
        }

        if (SelectedNamespaces.Count == 0)
        {
            return true;
        }

        string? namespaceName = resource.Namespace();
        return string.IsNullOrEmpty(namespaceName)
            || SelectedNamespaces.Any(selected => selected.Name() == namespaceName);
    }

    private void AddOwnerReferenceIndex(IKubernetesObject<V1ObjectMeta> resource, ResourceKey key)
    {
        foreach (V1OwnerReference owner in resource.Metadata?.OwnerReferences ?? [])
        {
            if (string.IsNullOrWhiteSpace(owner.Uid))
            {
                continue;
            }

            if (!_resourcesByOwnerUid.TryGetValue(owner.Uid, out HashSet<ResourceKey>? resources))
            {
                resources = [];
                _resourcesByOwnerUid.Add(owner.Uid, resources);
            }

            resources.Add(key);
        }
    }

    private void RemoveOwnerReferenceIndex(IKubernetesObject<V1ObjectMeta> resource, ResourceKey key)
    {
        foreach (V1OwnerReference owner in resource.Metadata?.OwnerReferences ?? [])
        {
            if (string.IsNullOrWhiteSpace(owner.Uid)
                || !_resourcesByOwnerUid.TryGetValue(owner.Uid, out HashSet<ResourceKey>? resources))
            {
                continue;
            }

            resources.Remove(key);
            if (resources.Count == 0)
            {
                _resourcesByOwnerUid.Remove(owner.Uid);
            }
        }
    }

    private bool HasOwnerReferencesTo(IKubernetesObject<V1ObjectMeta> resource)
    {
        string? uid = resource.Uid();
        return !string.IsNullOrWhiteSpace(uid)
            && _resourcesByOwnerUid.ContainsKey(uid);
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
        if (cluster == null || (RootResource == null && SelectedNamespaces.Count == 0))
        {
            Graph = ResourceRelationshipGraph.Empty;
            return;
        }

        HashSet<string> namespaces = SelectedNamespaces.Select(x => x.Name()).OfType<string>().ToHashSet(StringComparer.Ordinal);
        IKubernetesObject<V1ObjectMeta>? root = RootResource;
        bool hideNoise = HideNoise;
        IReadOnlyList<IKubernetesObject<V1ObjectMeta>> source = _resourcesByKey.Values.ToArray();

        _pendingRebuild = new RebuildRequest(source, root, namespaces, hideNoise, version);
        _rebuildCancellation?.Cancel();
        if (!_rebuildRunning)
        {
            _rebuildRunning = true;
            _ = ProcessRebuildsAsync();
        }
    }

    private async Task ProcessRebuildsAsync()
    {
        Dispatcher.UIThread.VerifyAccess();
        while (!_disposed && _pendingRebuild is { } request)
        {
            _pendingRebuild = null;
            using CancellationTokenSource cancellation = new();
            _rebuildCancellation = cancellation;

            try
            {
                ResourceRelationshipGraph graph = await Task.Run(
                    () => BuildGraph(request.Source, request.Root, request.Namespaces, request.HideNoise),
                    cancellation.Token).ConfigureAwait(true);

                if (!_disposed && !cancellation.IsCancellationRequested && request.Version == _rebuildVersion)
                {
                    ApplyGraph(graph);
                }
            }
            catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
            {
            }
            finally
            {
                if (ReferenceEquals(_rebuildCancellation, cancellation))
                {
                    _rebuildCancellation = null;
                }
            }
        }

        _rebuildRunning = false;
    }

    private sealed record RebuildRequest(
        IReadOnlyList<IKubernetesObject<V1ObjectMeta>> Source,
        IKubernetesObject<V1ObjectMeta>? Root,
        IReadOnlySet<string> Namespaces,
        bool HideNoise,
        int Version);

    private ResourceRelationshipGraph BuildGraph(
        IReadOnlyList<IKubernetesObject<V1ObjectMeta>> source,
        IKubernetesObject<V1ObjectMeta>? root,
        IReadOnlySet<string> namespaces,
        bool hideNoise)
    {
        IReadOnlySet<string> buildNamespaces = root == null
            ? namespaces
            : new HashSet<string>(StringComparer.Ordinal);
        ResourceRelationshipGraph graph = _resourceRelationshipBuilder.Build(source, buildNamespaces, hideNoise);
        if (root != null)
        {
            graph = FilterToRootResource(graph, root);
        }

        return graph;
    }

    private void RemoveResourceFromGraph(IKubernetesObject<V1ObjectMeta> resource)
    {
        ResourceRelationshipGraph current = _completeGraph;
        ResourceIdentity identity = GetIdentity(resource);
        if (!current.Resources.Any(item => GetIdentity(item) == identity))
        {
            return;
        }

        ApplyGraph(new ResourceRelationshipGraph(
            current.Resources.Where(item => GetIdentity(item) != identity).ToArray(),
            current.Relationships.Where(relationship => relationship.Source != identity && relationship.Target != identity).ToArray()));
    }

    internal static ResourceRelationshipGraph FilterToRootResource(ResourceRelationshipGraph graph, IKubernetesObject<V1ObjectMeta> root)
    {
        ResourceIdentity rootIdentity = new(root.ApiVersion ?? string.Empty, root.Kind ?? string.Empty, root.Namespace(), root.Name() ?? string.Empty, root.Uid());
        Dictionary<ResourceIdentity, List<ResourceIdentity>> parentsByChild = [];
        Dictionary<ResourceIdentity, List<ResourceIdentity>> childrenByParent = [];
        foreach (ResourceRelationship relationship in graph.Relationships)
        {
            parentsByChild.TryAdd(relationship.Target, []);
            parentsByChild[relationship.Target].Add(relationship.Source);
            childrenByParent.TryAdd(relationship.Source, []);
            childrenByParent[relationship.Source].Add(relationship.Target);
        }

        HashSet<ResourceIdentity> ancestors = [rootIdentity];
        Queue<ResourceIdentity> parents = new([rootIdentity]);
        while (parents.Count > 0)
        {
            ResourceIdentity current = parents.Dequeue();
            if (!parentsByChild.TryGetValue(current, out List<ResourceIdentity>? parentIdentities))
            {
                continue;
            }

            foreach (ResourceIdentity parent in parentIdentities)
            {
                if (ancestors.Add(parent))
                {
                    parents.Enqueue(parent);
                }
            }
        }

        HashSet<ResourceIdentity> reachable = [.. ancestors];
        Queue<ResourceIdentity> descendants = new([rootIdentity]);
        HashSet<ResourceIdentity> visitedDescendants = [];
        while (descendants.Count > 0)
        {
            ResourceIdentity current = descendants.Dequeue();
            if (!visitedDescendants.Add(current) || current != rootIdentity && ancestors.Contains(current))
            {
                continue;
            }

            if (!childrenByParent.TryGetValue(current, out List<ResourceIdentity>? childIdentities))
            {
                continue;
            }

            foreach (ResourceIdentity child in childIdentities)
            {
                if (reachable.Add(child))
                {
                    descendants.Enqueue(child);
                }
            }
        }

        IReadOnlyList<IKubernetesObject<V1ObjectMeta>> resources = graph.Resources.Where(resource => reachable.Contains(new ResourceIdentity(resource.ApiVersion ?? string.Empty, resource.Kind ?? string.Empty, resource.Namespace(), resource.Name() ?? string.Empty, resource.Uid()))).ToArray();
        IReadOnlyList<ResourceRelationship> relationships = graph.Relationships.Where(x => reachable.Contains(x.Source) && reachable.Contains(x.Target)).ToArray();
        return new ResourceRelationshipGraph(resources, relationships);
    }

    private void SeedOwnerReferenceResourceTypes(IKubernetesObject<V1ObjectMeta> resource)
    {
        ClusterWorkspace? cluster = Cluster;
        if (cluster == null)
        {
            return;
        }

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
                RequireSeed(cluster, type);
            }
        }
    }

    private bool SeedOwnerReferenceResourceType(
        IKubernetesObject<V1ObjectMeta> resource,
        Type ownerType,
        bool canSeed)
    {
        ClusterWorkspace? cluster = Cluster;
        if (cluster == null)
        {
            return false;
        }

        GroupApiVersionKind target = GroupApiVersionKind.From(ownerType);
        foreach (V1OwnerReference owner in resource.Metadata?.OwnerReferences ?? [])
        {
            if (owner.Kind == V1Namespace.KubeKind || string.IsNullOrWhiteSpace(owner.ApiVersion) || string.IsNullOrWhiteSpace(owner.Kind))
            {
                continue;
            }

            int slash = owner.ApiVersion.IndexOf('/');
            string group = slash < 0 ? string.Empty : owner.ApiVersion[..slash];
            string version = slash < 0 ? owner.ApiVersion : owner.ApiVersion[(slash + 1)..];
            if (string.Equals(group, target.Group, StringComparison.Ordinal)
                && string.Equals(version, target.ApiVersion, StringComparison.Ordinal)
                && string.Equals(owner.Kind, target.Kind, StringComparison.Ordinal))
            {
                if (canSeed)
                {
                    _ = cluster.Runtime.SeedResource(ownerType);
                }
                else
                {
                    RequireSeed(cluster, ownerType);
                }

                return true;
            }
        }

        return false;
    }

    private static ResourceIdentity GetIdentity(IKubernetesObject<V1ObjectMeta> resource)
        => new(resource.ApiVersion ?? string.Empty, resource.Kind ?? string.Empty, resource.Namespace(), resource.Name() ?? string.Empty, resource.Uid());

    private static ResourceKey GetResourceKey(IKubernetesObject<V1ObjectMeta> resource)
        => new(resource.ApiVersion ?? string.Empty, resource.Kind ?? string.Empty, resource.Namespace(), resource.Name() ?? string.Empty);

    private void UnsubscribeCluster()
    {
        if (Cluster != null)
        {
            Cluster.SelectedNamespaces.CollectionChanged -= SelectedNamespaces_CollectionChanged;
            Cluster.ResourceConfigProcessed -= ResourceConfigProcessed;
        }

        _localSelectedNamespaces.CollectionChanged -= SelectedNamespaces_CollectionChanged;

        _resourceChangesSubscription?.Dispose();
        _resourceChangesSubscription = null;
        _rebuildCancellation?.Cancel();
        _rebuildCancellation = null;
        _requiredSeedTypes.Clear();
    }

    public void Dispose()
    {
        _disposed = true;
        Interlocked.Increment(ref _rebuildVersion);
        UnsubscribeCluster();
        _resourcesByKey.Clear();
        _resourcesByOwnerUid.Clear();
        _completeGraph = ResourceRelationshipGraph.Empty;
        Graph = ResourceRelationshipGraph.Empty;
        Cluster = null;
    }
}
