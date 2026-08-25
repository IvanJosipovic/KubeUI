using System.Collections.Specialized;
using System.Reactive.Linq;
using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Avalonia.Resources;
using KubeUI.Kubernetes;
using KubeUI.Kubernetes.Resources.Relationships;

namespace KubeUI.Avalonia.Features.Resources.Visualization;

public sealed partial class VisualizationViewModel : ViewModelBase, IInitializeCluster, IDisposable
{
    private readonly IResourceRelationshipBuilder _resourceRelationshipBuilder;
    private Dictionary<ResourceKey, IKubernetesObject<V1ObjectMeta>> _resourcesByKey = [];
    private Dictionary<string, HashSet<ResourceKey>> _resourcesByOwnerUid = new(StringComparer.Ordinal);
    private readonly HashSet<GroupApiVersionKind> _requiredSeedKinds = [];
    private readonly HashSet<UnresolvedResourceReference> _pendingReferences = [];
    private readonly HashSet<string> _knownResourceTypes = new(StringComparer.Ordinal);
    private readonly HashSet<string> _excludedResourceTypes = new(StringComparer.Ordinal);
    private bool _disposed;
    private bool _suppressResourceChanges;
    private bool _suppressResourceTypeChanges;
    private IDisposable? _resourceChangesSubscription;
    private CancellationTokenSource? _rebuildCancellation;
    private CancellationTokenSource? _initializationCancellation;
    private RebuildRequest? _pendingRebuild;
    private bool _rebuildRunning;
    private int _rebuildVersion;
    private int _filterVersion;
    private int _graphApplicationVersion;
    private int _initializationVersion;
    private readonly List<ResourceChange> _deferredResourceChanges = [];
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
        Title = Assets.Resources.VisualizationView_Title!;
    }

    internal VisualizationViewModel(IResourceRelationshipBuilder resourceRelationshipBuilder)
    {
        _resourceRelationshipBuilder = resourceRelationshipBuilder;
        SelectedResourceTypes.CollectionChanged += SelectedResourceTypes_CollectionChanged;
        Title = Assets.Resources.VisualizationView_Title!;
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
        var isNamespaceRoot = rootResource is V1Namespace;
        RootResource = isNamespaceRoot ? null : rootResource;
        IsNamespaceSelectionLinked = !isNamespaceRoot;
        SelectRootNamespace(rootResource);
        Id = nameof(VisualizationViewModel) + "-" + cluster.Title + "-" + (rootResource?.Uid() ?? "null");

        _initializationCancellation?.Cancel();
        var initializationVersion = Interlocked.Increment(ref _initializationVersion);
        _resourcesByKey = [];
        _resourcesByOwnerUid = new(StringComparer.Ordinal);
        _requiredSeedKinds.Clear();
        _pendingReferences.Clear();
        _completeGraph = ResourceRelationshipGraph.Empty;
        _knownResourceTypes.Clear();
        _excludedResourceTypes.Clear();
        ResourceTypes.Clear();
        OnPropertyChanged(nameof(HasResourceTypes));
        _suppressResourceTypeChanges = true;
        SelectedResourceTypes.Clear();
        _suppressResourceTypeChanges = false;
        _suppressResourceChanges = true;
        _deferredResourceChanges.Clear();
        _resourceChangesSubscription = cluster.Runtime.ConnectResources().Subscribe(OnResourceChange);

        SubscribeToSelectedNamespaces();
        cluster.ResourceConfigProcessed += ResourceConfigProcessed;
        cluster.Runtime.ResourceSeeded += OnResourceSeeded;

        if (RootResource == null)
        {
            foreach (var resourceConfig in cluster.GetResourceConfigs())
            {
                if (resourceConfig.IsCustomResource && resourceConfig.IsNamespaced)
                {
                    RequireSeed(cluster, resourceConfig.Kind);
                }
            }

        }

        _initializationCancellation = new CancellationTokenSource();
        _ = LoadCurrentResourcesAsync(cluster.Runtime, initializationVersion, _initializationCancellation.Token);
    }

    private async Task LoadCurrentResourcesAsync(
        IClusterRuntime runtime,
        int initializationVersion,
        CancellationToken cancellationToken)
    {
        InitialResourceState state;
        try
        {
            state = await Task.Run(() => SnapshotCurrentResources(runtime, cancellationToken), cancellationToken)
                .ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
            return;
        }

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (_disposed
                || initializationVersion != _initializationVersion
                || !ReferenceEquals(Cluster?.Runtime, runtime))
            {
                return;
            }

            _resourcesByKey = state.ResourcesByKey;
            _resourcesByOwnerUid = state.ResourcesByOwnerUid;
            foreach (var resourceKind in state.RequiredSeedKinds)
            {
                RequireSeed(Cluster!, resourceKind);
            }

            _suppressResourceChanges = false;
            foreach (var change in _deferredResourceChanges)
            {
                ApplyResourceChange(change);
            }

            _deferredResourceChanges.Clear();
            Run();
        });
    }

    private static InitialResourceState SnapshotCurrentResources(
        IClusterRuntime runtime,
        CancellationToken cancellationToken)
    {
        Dictionary<ResourceKey, IKubernetesObject<V1ObjectMeta>> resourcesByKey = [];
        Dictionary<string, HashSet<ResourceKey>> resourcesByOwnerUid = new(StringComparer.Ordinal);
        HashSet<GroupApiVersionKind> requiredSeedKinds = [];

        foreach (var entry in runtime.Objects)
        {
            cancellationToken.ThrowIfCancellationRequested();
            if (entry.Value is not IResourceContainer container)
            {
                continue;
            }

            foreach (var resource in container.Snapshot())
            {
                var key = GetResourceKey(resource);
                resourcesByKey[key] = resource;
                foreach (var owner in resource.Metadata?.OwnerReferences ?? [])
                {
                    if (string.IsNullOrWhiteSpace(owner.Uid))
                    {
                        continue;
                    }

                    if (!resourcesByOwnerUid.TryGetValue(owner.Uid, out var ownedResources))
                    {
                        ownedResources = [];
                        resourcesByOwnerUid.Add(owner.Uid, ownedResources);
                    }

                    ownedResources.Add(key);
                    if (owner.Kind == V1Namespace.KubeKind || string.IsNullOrWhiteSpace(owner.ApiVersion) || string.IsNullOrWhiteSpace(owner.Kind))
                    {
                        continue;
                    }

                    if (TryResolveResourceKind(runtime.ModelCatalog, owner.ApiVersion, owner.Kind, out var ownerKind))
                    {
                        requiredSeedKinds.Add(ownerKind);
                    }
                }
            }
        }

        return new InitialResourceState(resourcesByKey, resourcesByOwnerUid, requiredSeedKinds);
    }

    private sealed record InitialResourceState(
        Dictionary<ResourceKey, IKubernetesObject<V1ObjectMeta>> ResourcesByKey,
        Dictionary<string, HashSet<ResourceKey>> ResourcesByOwnerUid,
        HashSet<GroupApiVersionKind> RequiredSeedKinds);

    private void SelectedNamespaces_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => Run();

    private void SelectedResourceTypes_CollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_suppressResourceTypeChanges)
        {
            return;
        }

        if (e.OldItems != null)
        {
            foreach (var type in e.OldItems.OfType<string>())
            {
                _excludedResourceTypes.Add(type);
            }
        }

        if (e.NewItems != null)
        {
            foreach (var type in e.NewItems.OfType<string>())
            {
                _excludedResourceTypes.Remove(type);
            }
        }

        ApplyTypeFilter();
    }

    internal void ApplyGraph(ResourceRelationshipGraph graph)
    {
        _ = ApplyGraphAsync(graph);
    }

    internal bool IsRebuildPendingOrRunning => _rebuildRunning || _pendingRebuild is not null;

    internal async Task ApplyGraphAsync(ResourceRelationshipGraph graph)
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            await Dispatcher.UIThread.InvokeAsync(() => ApplyGraphAsync(graph));
            return;
        }
        var pendingReferences = _pendingReferences.ToHashSet();
        var selectedTypes = SelectedResourceTypes.ToHashSet(StringComparer.Ordinal);
        var excludedTypes = _excludedResourceTypes.ToHashSet(StringComparer.Ordinal);
        var knownTypes = _knownResourceTypes.ToHashSet(StringComparer.Ordinal);
        var cluster = Cluster;
        var showNotReadyOnly = ShowNotReadyOnly;
        var applicationVersion = Interlocked.Increment(ref _graphApplicationVersion);
        Interlocked.Increment(ref _filterVersion);
        await PrepareGraphApplicationAsync(
            graph,
            pendingReferences,
            selectedTypes,
            excludedTypes,
            knownTypes,
            cluster,
            showNotReadyOnly,
            applicationVersion).ConfigureAwait(true);
    }

    private async Task PrepareGraphApplicationAsync(
        ResourceRelationshipGraph graph,
        HashSet<UnresolvedResourceReference> pendingReferences,
        HashSet<string> selectedTypes,
        HashSet<string> excludedTypes,
        HashSet<string> knownTypes,
        ClusterWorkspace? cluster,
        bool showNotReadyOnly,
        int applicationVersion)
    {
        var application = await Task.Run(() => PrepareGraphApplication(
            graph,
            pendingReferences,
            selectedTypes,
            excludedTypes,
            knownTypes,
            cluster,
            showNotReadyOnly)).ConfigureAwait(false);

        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            if (_disposed
                || applicationVersion != _graphApplicationVersion
                || !ReferenceEquals(Cluster, cluster))
            {
                return;
            }

            _pendingReferences.Clear();
            _pendingReferences.UnionWith(application.PendingReferences);
            _completeGraph = application.CompleteGraph;
            UpdateResourceTypes(application.AvailableTypes);
            foreach (var resourceKind in application.RequiredSeedKinds)
            {
                RequireSeed(cluster!, resourceKind);
            }

            Graph = application.FilteredGraph;
        });
    }

    private GraphApplication PrepareGraphApplication(
        ResourceRelationshipGraph graph,
        HashSet<UnresolvedResourceReference> pendingReferences,
        HashSet<string> selectedTypes,
        HashSet<string> excludedTypes,
        HashSet<string> knownTypes,
        ClusterWorkspace? cluster,
        bool showNotReadyOnly)
    {
        foreach (var reference in pendingReferences.ToArray())
        {
            if (graph.Resources.Any(resource => Matches(reference, resource)))
            {
                pendingReferences.Remove(reference);
            }
        }

        pendingReferences.UnionWith(graph.PendingReferences);
        var requiredSeedKinds = FindRequiredSeedKinds(graph, pendingReferences, cluster);
        var availableTypes = graph.Resources
            .Select(resource => resource.Kind)
            .OfType<string>()
            .Where(type => !string.IsNullOrWhiteSpace(type))
            .ToHashSet(StringComparer.Ordinal);
        var effectiveSelectedTypes = selectedTypes.ToHashSet(StringComparer.Ordinal);
        foreach (var type in availableTypes)
        {
            if (!knownTypes.Contains(type) && !excludedTypes.Contains(type))
            {
                effectiveSelectedTypes.Add(type);
            }
        }
        var filteredGraph = FilterGraphByTypes(graph, effectiveSelectedTypes, showNotReadyOnly);
        return new GraphApplication(graph, pendingReferences, requiredSeedKinds, availableTypes, filteredGraph);
    }

    private static HashSet<GroupApiVersionKind> FindRequiredSeedKinds(
        ResourceRelationshipGraph graph,
        HashSet<UnresolvedResourceReference> pendingReferences,
        ClusterWorkspace? cluster)
    {
        HashSet<GroupApiVersionKind> requiredSeedKinds = [];
        if (cluster == null)
        {
            return requiredSeedKinds;
        }

        foreach (var prerequisite in graph.RequiredSeedPrerequisites)
        {
            var kind = prerequisite.Kind;
            var matchingConfigs = cluster.GetResourceConfigs()
                .Where(resourceConfig => resourceConfig.Kind == kind
                    || prerequisite.MatchAnyApiGroup && string.Equals(resourceConfig.Kind.Kind, kind.Kind, StringComparison.Ordinal)
                    || prerequisite.AllowServedVersionFallback && MatchesSeedKind(kind, resourceConfig.Kind))
                .ToArray();

            if (matchingConfigs.Length == 0 || !prerequisite.AllowServedVersionFallback)
            {
                foreach (var resourceConfig in matchingConfigs)
                {
                    requiredSeedKinds.Add(resourceConfig.Kind);
                }
            }
            else
            {
                var selectedConfig = matchingConfigs
                    .OrderByDescending(resourceConfig => resourceConfig.Kind.ApiVersion, ApiVersionComparer.Instance)
                    .First();
                requiredSeedKinds.Add(selectedConfig.Kind);
            }
        }

        foreach (var reference in pendingReferences)
        {
            foreach (var resourceConfig in cluster.GetResourceConfigs())
            {
                if (string.Equals(resourceConfig.Kind.Group, reference.ApiGroup, StringComparison.Ordinal)
                    && string.Equals(resourceConfig.Kind.Kind, reference.Kind, StringComparison.Ordinal)
                    && (reference.ApiVersion == null || string.Equals(resourceConfig.Kind.ApiVersion, reference.ApiVersion, StringComparison.Ordinal)))
                {
                    requiredSeedKinds.Add(resourceConfig.Kind);
                }
            }
        }

        return requiredSeedKinds;
    }

    internal static bool MatchesSeedKind(GroupApiVersionKind prerequisite, GroupApiVersionKind resourceKind)
        => prerequisite == resourceKind
            || string.Equals(prerequisite.Group, resourceKind.Group, StringComparison.Ordinal)
                && string.Equals(prerequisite.Kind, resourceKind.Kind, StringComparison.Ordinal);

    private sealed class ApiVersionComparer : IComparer<string>
    {
        public static ApiVersionComparer Instance { get; } = new();

        public int Compare(string? x, string? y)
        {
            var xVersion = Parse(x);
            var yVersion = Parse(y);
            var comparison = xVersion.Major.CompareTo(yVersion.Major);
            if (comparison != 0)
            {
                return comparison;
            }

            comparison = xVersion.Stage.CompareTo(yVersion.Stage);
            return comparison != 0 ? comparison : xVersion.Minor.CompareTo(yVersion.Minor);
        }

        private static (int Major, int Stage, int Minor) Parse(string? apiVersion)
        {
            if (string.IsNullOrWhiteSpace(apiVersion))
            {
                return (0, 0, 0);
            }

            var version = apiVersion.AsSpan();
            var majorEnd = 1;
            while (majorEnd < version.Length && char.IsDigit(version[majorEnd]))
            {
                majorEnd++;
            }

            _ = int.TryParse(version[1..majorEnd], out var major);
            var stage = version[majorEnd..].StartsWith("beta", StringComparison.Ordinal) ? 1
                : version[majorEnd..].StartsWith("alpha", StringComparison.Ordinal) ? 0
                : 2;
            var minorStart = stage == 2 ? majorEnd : majorEnd + (stage == 1 ? 4 : 5);
            _ = int.TryParse(version[minorStart..], out var minor);
            return (major, stage, minor);
        }
    }

    private static ResourceRelationshipGraph FilterGraphByTypes(
        ResourceRelationshipGraph graph,
        HashSet<string> selectedTypes,
        bool showNotReadyOnly)
    {
        IReadOnlyList<IKubernetesObject<V1ObjectMeta>> resources = graph.Resources
            .Where(resource => resource.Kind is string kind && selectedTypes.Contains(kind))
            .Where(resource => !showNotReadyOnly || ResourceReadiness.IsNotReady(resource))
            .ToArray();
        var identities = resources.Select(GetIdentity).ToHashSet();
        return new ResourceRelationshipGraph(
            resources,
            graph.Relationships.Where(relationship => identities.Contains(relationship.Source) && identities.Contains(relationship.Target)).ToArray(),
            graph.UnresolvedReferences,
            graph.RequiredSeedPrerequisites);
    }

    private sealed record GraphApplication(
        ResourceRelationshipGraph CompleteGraph,
        HashSet<UnresolvedResourceReference> PendingReferences,
        HashSet<GroupApiVersionKind> RequiredSeedKinds,
        HashSet<string> AvailableTypes,
        ResourceRelationshipGraph FilteredGraph);

    private void UpdateResourceTypes(HashSet<string> availableTypes)
    {
        _suppressResourceTypeChanges = true;
        try
        {
            foreach (var type in availableTypes.Order(StringComparer.Ordinal))
            {
                if (_knownResourceTypes.Add(type) && !_excludedResourceTypes.Contains(type))
                {
                    SelectedResourceTypes.Add(type);
                }
            }

            foreach (var type in ResourceTypes.Where(type => !availableTypes.Contains(type)).ToArray())
            {
                _knownResourceTypes.Remove(type);
                ResourceTypes.Remove(type);
                SelectedResourceTypes.Remove(type);
            }

            ResourceTypes.Clear();
            foreach (var type in availableTypes.Order(StringComparer.Ordinal))
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
        var completeGraph = _completeGraph;
        var selectedTypes = SelectedResourceTypes.ToHashSet(StringComparer.Ordinal);
        var showNotReadyOnly = ShowNotReadyOnly;
        var version = Interlocked.Increment(ref _filterVersion);
        _ = ApplyTypeFilterAsync(completeGraph, selectedTypes, showNotReadyOnly, version);
    }

    private async Task ApplyTypeFilterAsync(
        ResourceRelationshipGraph completeGraph,
        HashSet<string> selectedTypes,
        bool showNotReadyOnly,
        int version)
    {
        var filteredGraph = await Task.Run(
            () => FilterGraphByTypes(completeGraph, selectedTypes, showNotReadyOnly)).ConfigureAwait(false);
        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            if (!_disposed
                && version == _filterVersion
                && ReferenceEquals(_completeGraph, completeGraph))
            {
                Graph = filteredGraph;
            }
        }, DispatcherPriority.Background);
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
        if (rootResource is not V1Namespace namespaceResource)
        {
            return;
        }

        _localSelectedNamespaces.Clear();
        _localSelectedNamespaces.Add(namespaceResource);
    }

    private static void CopyNamespaces(IEnumerable<V1Namespace> source, ObservableCollection<V1Namespace> target)
    {
        target.Clear();
        foreach (var selectedNamespace in source)
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
            ProcessResourceChangeOnUiThread(change);
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            if (!_disposed)
            {
                ProcessResourceChangeOnUiThread(change);
            }
        });
    }

    private void ProcessResourceChangeOnUiThread(ResourceChange change)
    {
        if (_suppressResourceChanges)
        {
            _deferredResourceChanges.Add(change);
            return;
        }

        ApplyResourceChange(change);
    }

    private void ApplyResourceChange(ResourceChange change)
    {
        if (_disposed)
        {
            return;
        }

        var changeVersion = Interlocked.Increment(ref _rebuildVersion);

        var key = GetResourceKey(change.Resource);
        if (change.EventType == WatchEventType.Deleted)
        {
            RemoveOwnerReferenceIndex(change.Resource, key);
            _resourcesByKey.Remove(key);
            RemoveResourceFromGraph(change.Resource);
            return;
        }

        if (_resourcesByKey.TryGetValue(key, out var previousResource))
        {
            RemoveOwnerReferenceIndex(previousResource, key);
        }

        _resourcesByKey[key] = change.Resource;
        AddOwnerReferenceIndex(change.Resource, key);
        SeedOwnerReferenceResourceKinds(change.Resource);

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
        var cluster = Cluster;
        if (cluster == null)
        {
            return;
        }

        var namespaces = SelectedNamespaces.Select(x => x.Name()).OfType<string>().ToHashSet(StringComparer.Ordinal);
        var hideNoise = HideNoise;
        IReadOnlyList<IKubernetesObject<V1ObjectMeta>> source = _resourcesByKey.Values.ToArray();
        IReadOnlySet<string> buildNamespaces = RootResource == null
            ? namespaces
            : new HashSet<string>(StringComparer.Ordinal);
        var delta = await Task.Run(() => _resourceRelationshipBuilder.BuildAdditionDelta(
            source,
            key,
            buildNamespaces,
            hideNoise)).ConfigureAwait(false);

        await Dispatcher.UIThread.InvokeAsync(async () =>
        {
            if (_disposed
                || changeVersion != _rebuildVersion
                || !ReferenceEquals(Cluster, cluster)
                || !_resourcesByKey.TryGetValue(key, out var currentResource)
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

            var current = _completeGraph;
            var currentIdentities = current.Resources.Select(GetIdentity).ToHashSet();
            var identity = GetIdentity(resource);
            if (RootResource != null
                && identity != GetIdentity(RootResource)
                && !delta.Relationships.Any(relationship => currentIdentities.Contains(relationship.Source) || currentIdentities.Contains(relationship.Target)))
            {
                Run();
                return;
            }

            if (RootResource == null)
            {
                delta = FilterIncrementalDelta(delta, namespaces, currentIdentities);
                if (delta.Resources.Count == 0)
                {
                    return;
                }
            }

            IReadOnlyList<IKubernetesObject<V1ObjectMeta>> resources = current.Resources
                .Concat(delta.Resources)
                .GroupBy(GetIdentity)
                .Select(group => group.First())
                .ToArray();
            var relationships = ResourceRelationshipBuilder.SimplifyRelationships(
                current.Relationships.Concat(delta.Relationships));
            ResourceRelationshipGraph merged = new(
                resources,
                relationships,
                current.PendingReferences.Union(delta.PendingReferences).ToHashSet(),
                current.RequiredSeedPrerequisites.Union(delta.RequiredSeedPrerequisites).ToHashSet());
            var completeGraph = RootResource is { } root
                ? FilterToRootResource(merged, root)
                : FilterToSelectedNamespaces(merged, namespaces);
            await ApplyGraphAsync(completeGraph);
        });
    }

    internal static ResourceRelationshipGraph FilterToSelectedNamespaces(
        ResourceRelationshipGraph graph,
        IReadOnlySet<string> selectedNamespaces)
    {
        var selected = graph.Resources
            .Select(GetIdentity)
            .Where(identity => selectedNamespaces.Contains(identity.Namespace ?? string.Empty))
            .ToHashSet();
        HashSet<ResourceIdentity> included = [.. selected];

        foreach (var relationship in graph.Relationships)
        {
            if (relationship.Kind == ResourceRelationshipKind.GitOps
                && selected.Contains(relationship.Target))
            {
                included.Add(relationship.Source);
                continue;
            }

            if (relationship.Kind == ResourceRelationshipKind.GitOps)
            {
                continue;
            }

            if (selected.Contains(relationship.Source)
                && relationship.Kind == ResourceRelationshipKind.Reference)
            {
                included.Add(relationship.Target);
            }

            if (selected.Contains(relationship.Target)
                && relationship.Kind == ResourceRelationshipKind.Reference)
            {
                included.Add(relationship.Source);
            }
        }

        bool changed;
        do
        {
            changed = false;
            foreach (var relationship in graph.Relationships)
            {
                if (relationship.Kind != ResourceRelationshipKind.Owner
                    || !included.Contains(relationship.Target)
                    || (!string.IsNullOrEmpty(relationship.Source.Namespace)
                        && !selectedNamespaces.Contains(relationship.Source.Namespace)))
                {
                    continue;
                }

                changed |= included.Add(relationship.Source);
            }
        }
        while (changed);

        return new ResourceRelationshipGraph(
            graph.Resources.Where(resource => included.Contains(GetIdentity(resource))).ToArray(),
            graph.Relationships.Where(relationship => included.Contains(relationship.Source) && included.Contains(relationship.Target)).ToArray(),
            graph.PendingReferences,
            graph.RequiredSeedPrerequisites);
    }

    internal static ResourceRelationshipGraph FilterIncrementalDelta(
        ResourceRelationshipGraph delta,
        IReadOnlySet<string> selectedNamespaces,
        IReadOnlySet<ResourceIdentity> currentIdentities)
    {
        var included = delta.Resources
            .Select(GetIdentity)
            .Where(identity => selectedNamespaces.Contains(identity.Namespace ?? string.Empty)
                || string.IsNullOrEmpty(identity.Namespace)
                    && delta.Relationships.Any(relationship =>
                        (relationship.Source == identity || relationship.Target == identity)
                        && (currentIdentities.Contains(relationship.Source)
                            || currentIdentities.Contains(relationship.Target))))
            .ToHashSet();

        foreach (var relationship in delta.Relationships)
        {
            var connectsToCurrent = currentIdentities.Contains(relationship.Source)
                || currentIdentities.Contains(relationship.Target);
            if (!connectsToCurrent || relationship.Kind is not (ResourceRelationshipKind.Reference or ResourceRelationshipKind.GitOps))
            {
                continue;
            }

            if (relationship.Kind != ResourceRelationshipKind.GitOps
                && (selectedNamespaces.Contains(relationship.Source.Namespace ?? string.Empty)
                    || string.IsNullOrEmpty(relationship.Source.Namespace)))
            {
                included.Add(relationship.Source);
            }

            if (relationship.Kind == ResourceRelationshipKind.GitOps
                && currentIdentities.Contains(relationship.Target))
            {
                included.Add(relationship.Source);
            }

            if (relationship.Kind != ResourceRelationshipKind.GitOps
                && (selectedNamespaces.Contains(relationship.Target.Namespace ?? string.Empty)
                    || string.IsNullOrEmpty(relationship.Target.Namespace)))
            {
                included.Add(relationship.Target);
            }
        }

        bool changed;
        do
        {
            changed = false;
            foreach (var relationship in delta.Relationships)
            {
                if (relationship.Kind != ResourceRelationshipKind.Owner
                    || !included.Contains(relationship.Target)
                    || (!string.IsNullOrEmpty(relationship.Source.Namespace)
                        && !selectedNamespaces.Contains(relationship.Source.Namespace)))
                {
                    continue;
                }

                changed |= included.Add(relationship.Source);
            }
        }
        while (changed);

        return new ResourceRelationshipGraph(
            delta.Resources.Where(resource => included.Contains(GetIdentity(resource))).ToArray(),
            delta.Relationships.Where(relationship => included.Contains(relationship.Source) && included.Contains(relationship.Target)).ToArray(),
            delta.PendingReferences,
            delta.RequiredSeedPrerequisites);
    }

    private void OnResourceSeeded(IClusterRuntime runtime, GroupApiVersionKind kind)
    {
        if (_disposed || Cluster?.Runtime != runtime || !_completeGraph.RequiredSeedPrerequisites.Any(prerequisite =>
                prerequisite.Kind == kind
                || prerequisite.MatchAnyApiGroup && string.Equals(prerequisite.Kind.Kind, kind.Kind, StringComparison.Ordinal)
                || prerequisite.AllowServedVersionFallback && MatchesSeedKind(prerequisite.Kind, kind)))
        {
            return;
        }

        Run();
    }

    private void ResourceConfigProcessed(ClusterWorkspace cluster, IResourceConfig resourceConfig)
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => ResourceConfigProcessed(cluster, resourceConfig));
            return;
        }

        if (_disposed || !ReferenceEquals(Cluster, cluster))
        {
            return;
        }

        if (_requiredSeedKinds.Contains(resourceConfig.Kind)
            && resourceConfig.PermissionsLoaded
            && resourceConfig.CanListAndWatch)
        {
            _ = SeedResourceOffUiThreadAsync(resourceConfig);
        }

        Run();

        var ownerReferenceFound = false;
        IReadOnlyList<IKubernetesObject<V1ObjectMeta>> resources = _resourcesByKey.Values.ToArray();
        foreach (var resource in resources)
        {
            ownerReferenceFound |= SeedOwnerReferenceResourceKind(
                resource,
                resourceConfig.Kind,
                resourceConfig.PermissionsLoaded && resourceConfig.CanListAndWatch);
        }

        if (ownerReferenceFound)
        {
            Run();
        }
    }

    private void RequireSeed(ClusterWorkspace cluster, GroupApiVersionKind resourceKind)
    {
        if (!_requiredSeedKinds.Add(resourceKind))
        {
            return;
        }

        var resourceConfig = cluster.GetResourceConfig(resourceKind);
        if (resourceConfig is { PermissionsLoaded: true, CanListAndWatch: true })
        {
            _ = SeedResourceOffUiThreadAsync(cluster.GetResourceConfig(resourceKind));
        }
    }

    private static async Task SeedResourceOffUiThreadAsync(IResourceConfig resourceConfig)
    {
        try
        {
            await Task.Run(() => resourceConfig.SeedResource()).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
        }
    }

    private static bool Matches(UnresolvedResourceReference reference, IKubernetesObject<V1ObjectMeta> resource)
    {
        var apiVersion = resource.ApiVersion;
        var slash = apiVersion?.IndexOf('/') ?? -1;
        var group = slash < 0 ? string.Empty : apiVersion![..slash];
        var version = slash < 0 ? apiVersion ?? string.Empty : apiVersion![(slash + 1)..];
        return string.Equals(group, reference.ApiGroup, StringComparison.Ordinal)
            && (reference.ApiVersion == null || string.Equals(version, reference.ApiVersion, StringComparison.Ordinal))
            && string.Equals(resource.Kind, reference.Kind, StringComparison.Ordinal)
            && string.Equals(resource.Namespace(), reference.Namespace, StringComparison.Ordinal)
            && string.Equals(resource.Name(), reference.Name, StringComparison.Ordinal);
    }

    private bool ResourceCanAffectGraph(IKubernetesObject<V1ObjectMeta> resource)
    {
        if (HideNoise && (resource is Corev1Event
            || resource is V1ReplicaSet replicaSet && replicaSet.Status?.Replicas == 0
            || resource is V1Pod pod && pod.Status?.Phase == "Succeeded"))
        {
            return false;
        }

        var cluster = Cluster;
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

        if (string.Equals(resource.ApiVersion, "argoproj.io/v1alpha1", StringComparison.Ordinal)
            && string.Equals(resource.Kind, "Application", StringComparison.Ordinal))
        {
            return true;
        }

        var namespaceName = resource.Namespace();
        return string.IsNullOrEmpty(namespaceName)
            || SelectedNamespaces.Any(selected => selected.Name() == namespaceName);
    }

    private void AddOwnerReferenceIndex(IKubernetesObject<V1ObjectMeta> resource, ResourceKey key)
    {
        foreach (var owner in resource.Metadata?.OwnerReferences ?? [])
        {
            if (string.IsNullOrWhiteSpace(owner.Uid))
            {
                continue;
            }

            if (!_resourcesByOwnerUid.TryGetValue(owner.Uid, out var resources))
            {
                resources = [];
                _resourcesByOwnerUid.Add(owner.Uid, resources);
            }

            resources.Add(key);
        }
    }

    private void RemoveOwnerReferenceIndex(IKubernetesObject<V1ObjectMeta> resource, ResourceKey key)
    {
        foreach (var owner in resource.Metadata?.OwnerReferences ?? [])
        {
            if (string.IsNullOrWhiteSpace(owner.Uid)
                || !_resourcesByOwnerUid.TryGetValue(owner.Uid, out var resources))
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
        var uid = resource.Uid();
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

        var version = Interlocked.Increment(ref _rebuildVersion);

        var cluster = Cluster;
        if (cluster == null || (RootResource == null && SelectedNamespaces.Count == 0))
        {
            _pendingRebuild = null;
            _rebuildCancellation?.Cancel();
            Interlocked.Increment(ref _graphApplicationVersion);
            Interlocked.Increment(ref _filterVersion);
            _pendingReferences.Clear();
            _completeGraph = ResourceRelationshipGraph.Empty;
            Graph = ResourceRelationshipGraph.Empty;
            return;
        }

        var namespaces = SelectedNamespaces.Select(x => x.Name()).OfType<string>().ToHashSet(StringComparer.Ordinal);
        var root = RootResource;
        var hideNoise = HideNoise;
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
                var graph = await Task.Run(
                    () => BuildGraph(request.Source, request.Root, request.Namespaces, request.HideNoise),
                    cancellation.Token).ConfigureAwait(true);

                if (!_disposed && !cancellation.IsCancellationRequested && request.Version == _rebuildVersion)
                {
                    await ApplyGraphAsync(graph);
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
        var buildNamespaces = root == null
            ? namespaces
            : new HashSet<string>(StringComparer.Ordinal);
        var graph = _resourceRelationshipBuilder.Build(source, buildNamespaces, hideNoise);
        if (root != null)
        {
            graph = FilterToRootResource(graph, root);
        }
        else
        {
            graph = FilterToSelectedNamespaces(graph, namespaces);
        }

        return graph;
    }

    private void RemoveResourceFromGraph(IKubernetesObject<V1ObjectMeta> resource)
    {
        var current = _completeGraph;
        var identity = GetIdentity(resource);
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
        foreach (var relationship in graph.Relationships)
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
            var current = parents.Dequeue();
            if (!parentsByChild.TryGetValue(current, out var parentIdentities))
            {
                continue;
            }

            foreach (var parent in parentIdentities)
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
            var current = descendants.Dequeue();
            if (!visitedDescendants.Add(current) || current != rootIdentity && ancestors.Contains(current))
            {
                continue;
            }

            if (!childrenByParent.TryGetValue(current, out var childIdentities))
            {
                continue;
            }

            foreach (var child in childIdentities)
            {
                if (reachable.Add(child))
                {
                    descendants.Enqueue(child);
                }
            }
        }

        IReadOnlyList<IKubernetesObject<V1ObjectMeta>> resources = graph.Resources.Where(resource => reachable.Contains(new ResourceIdentity(resource.ApiVersion ?? string.Empty, resource.Kind ?? string.Empty, resource.Namespace(), resource.Name() ?? string.Empty, resource.Uid()))).ToArray();
        IReadOnlyList<ResourceRelationship> relationships = graph.Relationships.Where(x => reachable.Contains(x.Source) && reachable.Contains(x.Target)).ToArray();
        return new ResourceRelationshipGraph(resources, relationships, graph.UnresolvedReferences, graph.RequiredSeedPrerequisites);
    }

    private void SeedOwnerReferenceResourceKinds(IKubernetesObject<V1ObjectMeta> resource)
    {
        var cluster = Cluster;
        if (cluster == null)
        {
            return;
        }

        foreach (var owner in resource.Metadata?.OwnerReferences ?? [])
        {
            if (owner.Kind == V1Namespace.KubeKind || string.IsNullOrWhiteSpace(owner.ApiVersion) || string.IsNullOrWhiteSpace(owner.Kind))
            {
                continue;
            }

            if (TryResolveResourceKind(cluster.Runtime.ModelCatalog, owner.ApiVersion, owner.Kind, out var ownerKind))
            {
                RequireSeed(cluster, ownerKind);
            }
        }
    }

    private bool SeedOwnerReferenceResourceKind(
        IKubernetesObject<V1ObjectMeta> resource,
        GroupApiVersionKind ownerKind,
        bool canSeed)
    {
        var cluster = Cluster;
        if (cluster == null)
        {
            return false;
        }

        foreach (var owner in resource.Metadata?.OwnerReferences ?? [])
        {
            if (owner.Kind == V1Namespace.KubeKind || string.IsNullOrWhiteSpace(owner.ApiVersion) || string.IsNullOrWhiteSpace(owner.Kind))
            {
                continue;
            }

            var ownerApiVersion = owner.ApiVersion;
            var slash = ownerApiVersion.IndexOf('/');
            var group = slash < 0 ? string.Empty : ownerApiVersion[..slash];
            var version = slash < 0 ? ownerApiVersion : ownerApiVersion[(slash + 1)..];
            if (string.Equals(group, ownerKind.Group, StringComparison.Ordinal)
                && string.Equals(version, ownerKind.ApiVersion, StringComparison.Ordinal)
                && string.Equals(owner.Kind, ownerKind.Kind, StringComparison.Ordinal))
            {
                if (canSeed)
                {
                    _ = SeedResourceOffUiThreadAsync(cluster.GetResourceConfig(ownerKind));
                }
                else
                {
                    RequireSeed(cluster, ownerKind);
                }

                return true;
            }
        }

        return false;
    }

    private static bool TryResolveResourceKind(
        ClusterModelCatalog catalog,
        string apiVersion,
        string kind,
        out GroupApiVersionKind resourceKind)
    {
        if (!catalog.TryGetResourceKind(apiVersion, kind, out resourceKind))
        {
            resourceKind = default;
            return false;
        }
        return true;
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
            Cluster.Runtime.ResourceSeeded -= OnResourceSeeded;
        }

        _localSelectedNamespaces.CollectionChanged -= SelectedNamespaces_CollectionChanged;

        _resourceChangesSubscription?.Dispose();
        _resourceChangesSubscription = null;
        _initializationCancellation?.Cancel();
        _initializationCancellation = null;
        _rebuildCancellation?.Cancel();
        _rebuildCancellation = null;
        _deferredResourceChanges.Clear();
        _requiredSeedKinds.Clear();
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
