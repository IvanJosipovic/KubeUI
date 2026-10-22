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
    private readonly record struct PendingIncrementalChange(
        IKubernetesObject<V1ObjectMeta> Resource,
        bool ReplaceExisting);

    private readonly IResourceRelationshipBuilder _resourceRelationshipBuilder;
    private readonly ILogger<VisualizationViewModel>? _logger;
    private readonly VisualizationResourceStore _resourceStore = new();
    private readonly HashSet<GroupApiVersionKind> _requiredSeedKinds = [];
    private readonly HashSet<UnresolvedResourceReference> _pendingReferences = [];
    private readonly HashSet<string> _knownResourceTypes = new(StringComparer.Ordinal);
    private readonly HashSet<string> _excludedResourceTypes = new(StringComparer.Ordinal);
    private bool _disposed;
    private bool _suppressResourceChanges;
    private bool _suppressResourceTypeChanges;
    private IDisposable? _resourceChangesSubscription;
    private CancellationTokenSource? _initializationCancellation;
    private readonly VisualizationBuildCoordinator<RebuildRequest> _buildCoordinator;
    private int _filterVersion;
    private int _graphApplicationVersion;
    private int _initializationVersion;
    private int _incrementalBuildRunning;
    private readonly List<ResourceChange> _deferredResourceChanges = [];
    private readonly Dictionary<ResourceKey, PendingIncrementalChange> _pendingIncrementalChanges = [];
    private readonly ObservableCollection<V1Namespace> _localSelectedNamespaces = [];
    private ResourceRelationshipGraph _completeGraph = ResourceRelationshipGraph.Empty;
    private IReadOnlyList<IKubernetesObject<V1ObjectMeta>> _previewResources = [];
    private VisualizationScope? _scope;
    private bool _fullBuildRunning;
    private bool _resourceRebuildPending;

    [ObservableProperty]
    public partial IKubernetesObject<V1ObjectMeta>? RootResource { get; set; }

    public string RootResourceDisplay => _scope?.ResourceDisplay ?? (RootResource == null
        ? IsPreview ? string.Format(Assets.Resources.VisualizationView_HelmPreviewTitle!, PreviewTitle) : string.Empty
        : $"Resource: {RootResource.ApiVersion}/{RootResource.Kind} {RootResource.Namespace()}/{RootResource.Name()}");

    public bool IsRootResourceDisplayVisible => _scope is not null || IsPreview || RootResource != null;

    [ObservableProperty]
    public partial bool IsPreview { get; private set; }

    [ObservableProperty]
    public partial string PreviewTitle { get; private set; } = string.Empty;

    partial void OnRootResourceChanged(IKubernetesObject<V1ObjectMeta>? value)
    {
        OnPropertyChanged(nameof(RootResourceDisplay));
        OnPropertyChanged(nameof(IsRootResourceDisplayVisible));
        OnPropertyChanged(nameof(IsNamespaceSelectorVisible));
        Run();
    }

    partial void OnIsPreviewChanged(bool value)
    {
        OnPropertyChanged(nameof(RootResourceDisplay));
        OnPropertyChanged(nameof(IsRootResourceDisplayVisible));
        OnPropertyChanged(nameof(IsNamespaceSelectorVisible));
    }

    partial void OnPreviewTitleChanged(string value)
        => OnPropertyChanged(nameof(RootResourceDisplay));

    [ObservableProperty]
    public partial ClusterWorkspace? Cluster { get; set; }

    [ObservableProperty]
    public partial ResourceRelationshipGraph? Graph { get; set; } = ResourceRelationshipGraph.Empty;

    /// <summary>
    /// Gets or sets the most recent visualization pipeline error.
    /// </summary>
    [ObservableProperty]
    public partial Exception? Error { get; set; }

    /// <summary>
    /// Gets the message from the most recent visualization pipeline error.
    /// </summary>
    public string? ErrorMessage => Error?.Message;

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

    public bool IsNamespaceSelectorVisible => !IsPreview && (RootResource == null || RootResource is V1Namespace);

    /// <summary>Whether the namespace scope can be changed by the visualization toolbar.</summary>
    public bool IsNamespaceSelectionEnabled => _scope?.IsNamespaceReadOnly != true;

    public VisualizationViewModel(ILogger<VisualizationViewModel>? logger = null)
    {
        _resourceRelationshipBuilder = new ResourceRelationshipBuilder();
        _buildCoordinator = new(ProcessBuildAsync);
        _logger = logger;
        SelectedResourceTypes.CollectionChanged += SelectedResourceTypes_CollectionChanged;
        Title = Assets.Resources.VisualizationView_Title!;
    }

    internal VisualizationViewModel(IResourceRelationshipBuilder resourceRelationshipBuilder)
    {
        _resourceRelationshipBuilder = resourceRelationshipBuilder;
        _buildCoordinator = new(ProcessBuildAsync);
        SelectedResourceTypes.CollectionChanged += SelectedResourceTypes_CollectionChanged;
        Title = Assets.Resources.VisualizationView_Title!;
    }

    protected override void OnPropertyChanged(PropertyChangedEventArgs e)
    {
        base.OnPropertyChanged(e);
        if (e.PropertyName == nameof(Error))
        {
            OnPropertyChanged(nameof(ErrorMessage));
        }
        else if (e.PropertyName == nameof(HideNoise))
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
        _scope = null;
        _resourceRebuildPending = false;
        OnPropertyChanged(nameof(IsNamespaceSelectionEnabled));
        OnPropertyChanged(nameof(RootResourceDisplay));
        OnPropertyChanged(nameof(IsRootResourceDisplayVisible));
        UnsubscribeCluster();
        IsPreview = false;
        PreviewTitle = string.Empty;
        _previewResources = [];
        Title = Assets.Resources.VisualizationView_Title!;
        Cluster = cluster;
        var isNamespaceRoot = rootResource is V1Namespace;
        RootResource = isNamespaceRoot ? null : rootResource;
        IsNamespaceSelectionLinked = !isNamespaceRoot;
        SelectRootNamespace(rootResource);
        Id = nameof(VisualizationViewModel) + "-" + cluster.Title + "-" + (rootResource?.Uid() ?? "null");
        _logger?.LogDebug(
            "Visualization initialized for cluster {Cluster}, root {Root}, linked namespaces {Linked}, selected namespaces {Namespaces}",
            cluster.Title,
            rootResource?.Name(),
            IsNamespaceSelectionLinked,
            SelectedNamespaces.Select(namespaceResource => namespaceResource.Name()));

        _initializationCancellation?.Cancel();
        var initializationVersion = Interlocked.Increment(ref _initializationVersion);
        _resourceStore.Clear();
        _requiredSeedKinds.Clear();
        _pendingReferences.Clear();
        _completeGraph = ResourceRelationshipGraph.Empty;
        Graph = ResourceRelationshipGraph.Empty;
        Error = null;
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
                if (resourceConfig.IsCustomResource() && resourceConfig.IsNamespaced)
                {
                    RequireSeed(cluster, resourceConfig.Kind);
                }
            }

        }

        _initializationCancellation = new CancellationTokenSource();
        _ = LoadCurrentResourcesAsync(cluster.Runtime, initializationVersion, _initializationCancellation.Token);
    }

    /// <summary>Initializes a named live graph using annotation-selected roots and local namespace controls.</summary>
    /// <param name="cluster">Cluster containing the resources.</param>
    /// <param name="scope">Resource description, selector, namespace, and toolbar restrictions.</param>
    public void InitializeScope(ClusterWorkspace cluster, VisualizationScope scope)
    {
        ArgumentNullException.ThrowIfNull(cluster);
        ArgumentNullException.ThrowIfNull(scope);
        ArgumentException.ThrowIfNullOrWhiteSpace(scope.Namespace);
        ArgumentException.ThrowIfNullOrWhiteSpace(scope.ResourceDisplay);
        ArgumentNullException.ThrowIfNull(scope.Selector);
        var namespaceResource = cluster.Runtime.Namespaces.FirstOrDefault(item => item.Name() == scope.Namespace)
            ?? new V1Namespace { Metadata = new V1ObjectMeta { Name = scope.Namespace } };
        Initialize(cluster, namespaceResource);
        _scope = scope;
        OnPropertyChanged(nameof(IsNamespaceSelectionEnabled));
        OnPropertyChanged(nameof(RootResourceDisplay));
        OnPropertyChanged(nameof(IsRootResourceDisplayVisible));
        foreach (var config in cluster.GetResourceConfigs())
        {
            RequireSeed(cluster, config.Kind);
        }

        Title = scope.ResourceDisplay;
        Id = $"{nameof(VisualizationViewModel)}-{cluster.Title}-{scope.Namespace}-{scope.ResourceDisplay}";
        Run();
    }

    /// <summary>
    /// Initializes a read-only graph preview from rendered Helm resources without seeding or mutating the cluster.
    /// </summary>
    /// <param name="cluster">Cluster whose theme and navigation context the preview uses.</param>
    /// <param name="releaseName">Name of the Helm release being previewed.</param>
    /// <param name="resources">Kubernetes resources rendered by Helm.</param>
    public void InitializePreview(
        ClusterWorkspace cluster,
        string releaseName,
        IEnumerable<IKubernetesObject<V1ObjectMeta>> resources)
    {
        ArgumentNullException.ThrowIfNull(cluster);
        ArgumentException.ThrowIfNullOrWhiteSpace(releaseName);
        ArgumentNullException.ThrowIfNull(resources);

        _scope = null;
        _resourceRebuildPending = false;
        OnPropertyChanged(nameof(IsNamespaceSelectionEnabled));
        OnPropertyChanged(nameof(RootResourceDisplay));
        OnPropertyChanged(nameof(IsRootResourceDisplayVisible));
        UnsubscribeCluster();
        Interlocked.Increment(ref _initializationVersion);
        Interlocked.Increment(ref _graphApplicationVersion);
        Interlocked.Increment(ref _filterVersion);
        _resourceStore.Clear();
        _requiredSeedKinds.Clear();
        _pendingReferences.Clear();
        _completeGraph = ResourceRelationshipGraph.Empty;
        Graph = ResourceRelationshipGraph.Empty;
        Error = null;
        _knownResourceTypes.Clear();
        _excludedResourceTypes.Clear();
        _deferredResourceChanges.Clear();
        _pendingIncrementalChanges.Clear();
        _suppressResourceChanges = false;
        _previewResources = resources.ToArray();

        _suppressResourceTypeChanges = true;
        SelectedResourceTypes.Clear();
        ResourceTypes.Clear();
        _suppressResourceTypeChanges = false;
        OnPropertyChanged(nameof(HasResourceTypes));

        IsPreview = true;
        PreviewTitle = releaseName;
        RootResource = null;
        IsNamespaceSelectionLinked = false;
        Cluster = cluster;
        Title = string.Format(Assets.Resources.VisualizationView_HelmPreviewTitle!, releaseName);
        Id = $"{nameof(VisualizationViewModel)}-{cluster.Title}-helm-preview-{Guid.NewGuid():N}";
        OnPropertyChanged(nameof(SelectedNamespaces));
        Run();
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
        catch (Exception exception)
        {
            ReportError(exception);
            return;
        }

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            try
            {
                if (_disposed
                    || initializationVersion != _initializationVersion
                    || !ReferenceEquals(Cluster?.Runtime, runtime))
                {
                    return;
                }

                _resourceStore.Replace(state.ResourcesByKey, state.ResourcesByOwnerUid);
                _logger?.LogDebug(
                    "Visualization snapshot loaded with {ResourceCount} resources across types {ResourceTypes}; selected namespaces {Namespaces}",
                    _resourceStore.Count,
                    state.ResourcesByKey.Keys.Select(key => key.Kind).Distinct(StringComparer.Ordinal),
                    SelectedNamespaces.Select(namespaceResource => namespaceResource.Name()));
                foreach (var resourceKind in state.RequiredSeedKinds)
                {
                    RequireSeed(Cluster!, resourceKind);
                }

                // Keep the feed suppressed while replaying the changes that arrived during
                // snapshot creation. Otherwise every replayed Added/Modified change schedules
                // another rebuild, causing a large cluster to repeatedly rebuild its graph.
                foreach (var change in _deferredResourceChanges
                    .GroupBy(change => GetResourceKey(change.Resource))
                    .Select(changes => changes.Last()))
                {
                    ApplyResourceChange(change);
                }

                _deferredResourceChanges.Clear();
                _suppressResourceChanges = false;
                Run();
            }
            catch (Exception exception)
            {
                ReportError(exception);
            }
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

    internal bool IsRebuildPendingOrRunning => _buildCoordinator.IsPendingOrRunning;

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
                if (!IsPreview)
                {
                    RequireSeed(cluster!, resourceKind);
                }
            }

            Graph = application.FilteredGraph;
            Error = null;
            _logger?.LogDebug(
                "Visualization graph applied: complete {CompleteCount}, filtered {FilteredCount}, selected types {SelectedTypes}, namespaces {Namespaces}",
                application.CompleteGraph.Resources.Count,
                application.FilteredGraph.Resources.Count,
                SelectedResourceTypes,
                SelectedNamespaces.Select(namespaceResource => namespaceResource.Name()));
        });
    }

    private VisualizationPipelineState PrepareGraphApplication(
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
        var requiredSeedKinds = IsPreview
            ? new HashSet<GroupApiVersionKind>()
            : VisualizationSeedPlanner.FindRequiredSeedKinds(graph, pendingReferences, cluster);
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
        return new VisualizationPipelineState(
            graph,
            pendingReferences.ToHashSet(),
            requiredSeedKinds.ToHashSet(),
            availableTypes,
            filteredGraph);
    }

    private static ResourceRelationshipGraph FilterGraphByTypes(
        ResourceRelationshipGraph graph,
        HashSet<string> selectedTypes,
        bool showNotReadyOnly)
        => ResourceGraphDisplayFilter.Apply(graph, selectedTypes, showNotReadyOnly);

    private void UpdateResourceTypes(IReadOnlySet<string> availableTypes)
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
        if (IsPreview)
        {
            OnPropertyChanged(nameof(SelectedNamespaces));
            return;
        }

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
        try
        {
            if (_suppressResourceChanges)
            {
                _deferredResourceChanges.Add(change);
                return;
            }

            ApplyResourceChange(change);
        }
        catch (Exception exception)
        {
            ReportError(exception);
        }
    }

    private void ApplyResourceChange(ResourceChange change)
    {
        if (_disposed)
        {
            return;
        }

        var key = GetResourceKey(change.Resource);
        if (change.EventType == WatchEventType.Deleted)
        {
            var affectsGraph = ResourceCanAffectGraph(change.Resource);
            _resourceStore.Remove(key, change.Resource);
            if (affectsGraph)
            {
                if (!_fullBuildRunning)
                {
                    RemoveResourceFromGraph(change.Resource);
                }

                RequestResourceRebuild();
            }

            return;
        }

        _resourceStore.TryGet(key, out var previousResource);
        _resourceStore.Upsert(key, change.Resource);
        SeedOwnerReferenceResourceKinds(change.Resource);

        if (_suppressResourceChanges)
        {
            return;
        }

        if (_scope is not null && previousResource is not null
            && _scope.Selector.Matches(previousResource) != _scope.Selector.Matches(change.Resource))
        {
            RequestResourceRebuild();
            return;
        }

        if (!ResourceCanAffectGraph(change.Resource))
        {
            return;
        }

        if (_fullBuildRunning)
        {
            _resourceRebuildPending = true;
            return;
        }

        if (change.EventType == WatchEventType.Modified)
        {
            var identity = GetIdentity(change.Resource);
            var conflictingIdentity = _completeGraph.Resources.Any(item =>
                GetResourceKey(item) == key && GetIdentity(item) != identity);
            if (_buildCoordinator.IsPendingOrRunning)
            {
                _pendingIncrementalChanges.Clear();
                _buildCoordinator.Invalidate();
                Run();
            }
            else if (Volatile.Read(ref _incrementalBuildRunning) != 0)
            {
                _pendingIncrementalChanges[key] = new(change.Resource, true);
            }
            else if ((previousResource is null || GetIdentity(previousResource) == identity)
                && !conflictingIdentity)
            {
                Interlocked.Exchange(ref _incrementalBuildRunning, 1);
                var changeVersion = _buildCoordinator.Invalidate();
                _ = AddResourceIncrementallyAsync(change.Resource, key, changeVersion, replaceExisting: true);
            }
            else
            {
                _buildCoordinator.Invalidate();
                Run();
            }

            return;
        }

        if (change.EventType == WatchEventType.Added)
        {
            if (_resourceStore.HasOwnerReferencesTo(change.Resource)
                || _buildCoordinator.IsPendingOrRunning)
            {
                _pendingIncrementalChanges.Clear();
                _buildCoordinator.Invalidate();
                Run();
                return;
            }

            if (Volatile.Read(ref _incrementalBuildRunning) != 0)
            {
                _pendingIncrementalChanges[key] = new(change.Resource, false);
                return;
            }

            if (ResourceCanAffectGraph(change.Resource))
            {
                Interlocked.Exchange(ref _incrementalBuildRunning, 1);
                var changeVersion = _buildCoordinator.Invalidate();
                _ = AddResourceIncrementallyAsync(change.Resource, key, changeVersion);
            }

            return;
        }

        if (ResourceCanAffectGraph(change.Resource))
        {
            _buildCoordinator.Invalidate();
            Run();
        }
    }

    private async Task AddResourceIncrementallyAsync(
        IKubernetesObject<V1ObjectMeta> resource,
        ResourceKey key,
        int changeVersion,
        bool replaceExisting = false)
    {
        try
        {
            var cluster = Cluster;
            if (cluster == null)
            {
                return;
            }

            var namespaces = SelectedNamespaces.Select(x => x.Name()).OfType<string>().ToHashSet(StringComparer.Ordinal);
            var hideNoise = HideNoise;
            var source = _resourceStore.Snapshot();
            IReadOnlySet<string> buildNamespaces = RootResource == null && _scope is null
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
                    || !ReferenceEquals(Cluster, cluster)
                    || !_resourceStore.TryGet(key, out var currentResource)
                    || !ReferenceEquals(currentResource, resource))
                {
                    return;
                }

                if (changeVersion != _buildCoordinator.CurrentVersion)
                {
                    Run();
                    return;
                }

                if (_buildCoordinator.IsPendingOrRunning && !replaceExisting)
                {
                    Run();
                    return;
                }

                var identity = GetIdentity(resource);
                var current = _completeGraph;
                var currentResources = replaceExisting
                    ? current.Resources.Where(item => GetIdentity(item) != identity).ToArray()
                    : current.Resources;
                var currentRelationships = replaceExisting
                    ? current.Relationships.Where(relationship => relationship.Source != identity && relationship.Target != identity).ToArray()
                    : current.Relationships;
                var currentIdentities = currentResources.Select(GetIdentity).ToHashSet();
                if (RootResource != null
                    && identity != GetIdentity(RootResource)
                    && !delta.Relationships.Any(relationship => currentIdentities.Contains(relationship.Source) || currentIdentities.Contains(relationship.Target)))
                {
                    Run();
                    return;
                }

                if (RootResource == null && _scope is null)
                {
                    delta = ResourceGraphProjection.ToSelectedNamespacesIncremental(delta, namespaces, currentIdentities);
                    if (!replaceExisting && delta.Resources.Count == 0)
                    {
                        return;
                    }
                }

                IReadOnlyList<IKubernetesObject<V1ObjectMeta>> resources = currentResources
                    .Concat(delta.Resources)
                    .GroupBy(GetIdentity)
                    .Select(group => group.First())
                    .ToArray();
                var relationships = ResourceRelationshipBuilder.SimplifyRelationships(
                    currentRelationships.Concat(delta.Relationships));
                ResourceRelationshipGraph merged = new(
                    resources,
                    relationships,
                    current.PendingReferences.Union(delta.PendingReferences).ToHashSet(),
                    current.RequiredSeedPrerequisites.Union(delta.RequiredSeedPrerequisites).ToHashSet());
                var completeGraph = _scope is { } scope
                    ? ResourceGraphProjection.ToSelector(merged, scope.Selector)
                    : RootResource is { } root
                    ? ResourceGraphProjection.ToRootResource(merged, root)
                    : ResourceGraphProjection.ToSelectedNamespaces(merged, namespaces);
                await ApplyGraphAsync(completeGraph);
            });
        }
        finally
        {
            Interlocked.Exchange(ref _incrementalBuildRunning, 0);
            Dispatcher.UIThread.Post(StartPendingIncrementalChange);
        }
    }

    private void StartPendingIncrementalChange()
    {
        if (_disposed || _pendingIncrementalChanges.Count == 0)
        {
            return;
        }

        if (_buildCoordinator.IsPendingOrRunning)
        {
            _pendingIncrementalChanges.Clear();
            return;
        }

        var pending = _pendingIncrementalChanges.First();
        _pendingIncrementalChanges.Remove(pending.Key);
        if (!_resourceStore.TryGet(pending.Key, out var resource))
        {
            StartPendingIncrementalChange();
            return;
        }

        Interlocked.Exchange(ref _incrementalBuildRunning, 1);
        var changeVersion = _buildCoordinator.Invalidate();
        _ = AddResourceIncrementallyAsync(
            resource,
            pending.Key,
            changeVersion,
            pending.Value.ReplaceExisting);
    }

    private void OnResourceSeeded(IClusterRuntime runtime, GroupApiVersionKind kind)
    {
        if (_disposed || Cluster?.Runtime != runtime || !(_requiredSeedKinds.Contains(kind)
            || _completeGraph.RequiredSeedPrerequisites.Any(prerequisite =>
                prerequisite.Kind == kind
                || prerequisite.MatchAnyApiGroup && string.Equals(prerequisite.Kind.Kind, kind.Kind, StringComparison.Ordinal)
                || prerequisite.AllowServedVersionFallback && VisualizationSeedPlanner.MatchesSeedKind(prerequisite.Kind, kind))))
        {
            return;
        }

        RequestResourceRebuild();
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

        if (_scope is not null)
        {
            RequireSeed(cluster, resourceConfig.Kind);
        }

        var seedRequested = _requiredSeedKinds.Contains(resourceConfig.Kind)
            && resourceConfig.PermissionsLoaded
            && resourceConfig.CanListAndWatch;
        if (seedRequested)
        {
            _ = SeedResourceOffUiThreadAsync(resourceConfig);
        }

        var ownerReferenceFound = false;
        var resources = _resourceStore.Snapshot();
        foreach (var resource in resources)
        {
            ownerReferenceFound |= SeedOwnerReferenceResourceKind(
                resource,
                resourceConfig.Kind,
                resourceConfig.PermissionsLoaded && resourceConfig.CanListAndWatch);
        }

        if (seedRequested || ownerReferenceFound)
        {
            RequestResourceRebuild();
        }
    }

    private void RequireSeed(ClusterWorkspace cluster, GroupApiVersionKind resourceKind)
    {
        if (!_requiredSeedKinds.Add(resourceKind))
        {
            return;
        }

        var resourceConfig = cluster.GetResourceConfigs().FirstOrDefault(config => config.Kind == resourceKind);
        if (resourceConfig is { PermissionsLoaded: true, CanListAndWatch: true })
        {
            _ = SeedResourceOffUiThreadAsync(resourceConfig);
        }
    }

    private void ReportError(Exception exception)
    {
        if (_disposed)
        {
            return;
        }

        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(() => ReportError(exception));
            return;
        }

        Error = exception;
    }

    internal void ReportGraphError(Exception exception) => ReportError(exception);

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

        if (_scope is { } scope)
        {
            // A named scope projects the selector matches plus their relationship ancestry and descendants.
            // Resources from the scope namespace can still join through ownership; anything else only counts
            // when it is already connected to the graph.
            return scope.Selector.Matches(resource)
                || string.Equals(resource.Namespace(), scope.Namespace, StringComparison.Ordinal)
                || IsConnectedToGraph(resource);
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
        if (!string.IsNullOrEmpty(namespaceName))
        {
            return SelectedNamespaces.Any(selected => selected.Name() == namespaceName);
        }

        return IsConnectedToGraph(resource);
    }

    private bool IsConnectedToGraph(IKubernetesObject<V1ObjectMeta> resource)
    {
        var identity = GetIdentity(resource);
        return _completeGraph.Resources.Any(current => GetIdentity(current) == identity)
            || _resourceStore.HasOwnerReferencesTo(resource)
            || _completeGraph.PendingReferences.Any(reference => Matches(reference, resource));
    }

    private void RequestResourceRebuild()
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            Dispatcher.UIThread.Post(RequestResourceRebuild);
            return;
        }

        if (_fullBuildRunning)
        {
            _resourceRebuildPending = true;
            return;
        }

        Run();
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

        var cluster = Cluster;
        if (cluster == null || (!IsPreview && RootResource == null && SelectedNamespaces.Count == 0))
        {
            _buildCoordinator.Clear();
            Interlocked.Increment(ref _graphApplicationVersion);
            Interlocked.Increment(ref _filterVersion);
            _pendingReferences.Clear();
            _completeGraph = ResourceRelationshipGraph.Empty;
            Graph = ResourceRelationshipGraph.Empty;
            return;
        }

        var preview = IsPreview;
        var namespaces = preview
            ? new HashSet<string>(StringComparer.Ordinal)
            : SelectedNamespaces.Select(x => x.Name()).OfType<string>().ToHashSet(StringComparer.Ordinal);
        var root = RootResource;
        var hideNoise = HideNoise;
        var source = preview ? _previewResources : _resourceStore.Snapshot();

        _buildCoordinator.Enqueue(new RebuildRequest(source, root, namespaces, hideNoise, preview, _scope?.Selector));
    }

    private async Task ProcessBuildAsync(RebuildRequest request, int version, CancellationToken cancellationToken)
    {
        Dispatcher.UIThread.VerifyAccess();
        _fullBuildRunning = true;

        try
        {
            var graph = await Task.Run(
                () => BuildGraph(request.Source, request.Root, request.Namespaces, request.HideNoise, request.IsPreview, request.Selector),
                cancellationToken).ConfigureAwait(true);

            if (_buildCoordinator.IsCurrent(version) && !cancellationToken.IsCancellationRequested)
            {
                await ApplyGraphAsync(graph);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception exception)
        {
            ReportError(exception);
        }
        finally
        {
            _fullBuildRunning = false;
            if (_resourceRebuildPending && !_disposed)
            {
                _resourceRebuildPending = false;
                Run();
            }
        }
    }

    private sealed record RebuildRequest(
        IReadOnlyList<IKubernetesObject<V1ObjectMeta>> Source,
        IKubernetesObject<V1ObjectMeta>? Root,
        IReadOnlySet<string> Namespaces,
        bool HideNoise,
        bool IsPreview,
        ResourceAnnotationSelector? Selector);

    private ResourceRelationshipGraph BuildGraph(
        IReadOnlyList<IKubernetesObject<V1ObjectMeta>> source,
        IKubernetesObject<V1ObjectMeta>? root,
        IReadOnlySet<string> namespaces,
        bool hideNoise,
        bool isPreview,
        ResourceAnnotationSelector? selector)
    {
        if (isPreview)
        {
            return _resourceRelationshipBuilder.Build(source, new HashSet<string>(StringComparer.Ordinal), hideNoise);
        }

        var buildNamespaces = root == null && selector is null
            ? namespaces
            : new HashSet<string>(StringComparer.Ordinal);
        var graph = _resourceRelationshipBuilder.Build(source, buildNamespaces, hideNoise);
        if (selector is not null)
        {
            graph = ResourceGraphProjection.ToSelector(graph, selector);
        }
        else if (root != null)
        {
            graph = ResourceGraphProjection.ToRootResource(graph, root);
        }
        else
        {
            graph = ResourceGraphProjection.ToSelectedNamespaces(graph, namespaces);
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

    internal void SeedOwnerReferenceResourceKinds(IKubernetesObject<V1ObjectMeta> resource)
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
        _buildCoordinator.Clear();
        _deferredResourceChanges.Clear();
        _requiredSeedKinds.Clear();
    }

    public void Dispose()
    {
        _disposed = true;
        UnsubscribeCluster();
        _buildCoordinator.Dispose();
        _resourceStore.Clear();
        _completeGraph = ResourceRelationshipGraph.Empty;
        Graph = ResourceRelationshipGraph.Empty;
        Cluster = null;
    }
}
