using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Security.Cryptography;
using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Kubernetes;

namespace KubeUI.Features.Clusters.Workspace;

public sealed partial class ClusterWorkspace : ObservableObject, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ClusterWorkspace> _logger;
    private readonly ConcurrentDictionary<GroupApiVersionKind, IClusterResourceConfig> _resourceConfigs = new();
    private readonly ConcurrentDictionary<string, long> _customResourceDefinitionRevisions = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, long> _customResourceDefinitionObservedGenerations = new(StringComparer.Ordinal);
    private readonly CancellationTokenSource _disposeCancellation = new();
    private static readonly GroupApiVersionKind[] PermissionWatchKinds =
    [
        GroupApiVersionKind.From<V1ClusterRole>(),
        GroupApiVersionKind.From<V1ClusterRoleBinding>(),
        GroupApiVersionKind.From<V1Role>(),
        GroupApiVersionKind.From<V1RoleBinding>(),
    ];

    private static readonly TimeSpan PermissionRefreshDebounce = TimeSpan.FromMilliseconds(500);

    private readonly Lock _connectLock = new();
    private readonly PermissionRefreshCoordinator _permissionRefreshCoordinator;
    private long _customResourceDefinitionRevision;
    private volatile bool _disposed;
    private volatile bool _workspaceStateInitialized;
    private Task? _connectTask;
    private INotifyCollectionChanged? _namespaces;

    private IClusterInstrumentation _instrumentation;

    public ClusterWorkspace(
        IClusterRuntime runtime,
        IServiceProvider serviceProvider,
        ILogger<ClusterWorkspace> logger, IClusterInstrumentation instrumentation)
    {
        Runtime = runtime;
        _serviceProvider = serviceProvider;
        _logger = logger;
        _instrumentation = instrumentation;
        _permissionRefreshCoordinator = new(
            RefreshPermissionsCoreAsync,
            CanRefreshPermissions,
            cancellationToken => Task.Delay(PermissionRefreshDebounce, cancellationToken),
            _disposeCancellation.Token,
            exception => _logger.LogDebug(exception, "Unable to refresh permissions for cluster {ClusterName}", Runtime.Name));

        SubscribeRuntime();
        SubscribeNamespaces();
        UpdateClusterColor();
    }

    [ObservableProperty]
    public partial IClusterRuntime Runtime { get; set; }

    public string Title => DisplayName;

    /// <summary>
    /// Gets or sets the KubeUI-only display name. The context name is used when empty.
    /// </summary>
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(DisplayName))]
    [NotifyPropertyChangedFor(nameof(Title))]
    public partial string? Alias { get; set; }

    /// <summary>
    /// Gets or sets whether the cluster is shown in navigation. Disabling a connected cluster disconnects it.
    /// </summary>
    [ObservableProperty]
    public partial bool IsEnabled { get; set; } = true;

    /// <summary>
    /// Gets the name shown to the user: the alias when set, otherwise the context name.
    /// </summary>
    public string DisplayName => string.IsNullOrWhiteSpace(Alias) ? Runtime.Name : Alias;

    partial void OnIsEnabledChanged(bool value)
    {
        if (!value)
        {
            _ = DisconnectDisabledAsync();
        }
    }

    private async Task DisconnectDisabledAsync()
    {
        try
        {
            Task? connectTask;
            lock (_connectLock)
            {
                connectTask = _connectTask;
            }
            if (connectTask is not null)
            {
                await connectTask.ConfigureAwait(false);
            }
            if (!IsEnabled && Runtime.Connected)
            {
                await Disconnect().ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disconnecting disabled cluster {ClusterName}", Runtime.Name);
        }
    }

    /// <summary>Gets the service provider used to create cluster-scoped services.</summary>
    public IServiceProvider ServiceProvider => _serviceProvider;

    [ObservableProperty]
    public partial IBrush ClusterColor { get; set; } = Brushes.Red;

    [ObservableProperty]
    public partial ObservableCollection<V1Namespace> SelectedNamespaces { get; set; } = [];

    /// <summary>
    /// Raised after a resource config has been permission-evaluated and registered by the workspace.
    /// </summary>
    public event Action<ClusterWorkspace, IClusterResourceConfig>? ResourceConfigProcessed;

    /// <summary>Raised when a custom resource definition is added, changed, or removed.</summary>
    public event Action<ClusterWorkspace>? CustomResourceDefinitionChanged;

    public event Action<ClusterWorkspace, GroupApiVersionKind>? CustomResourceDefinitionRemoved;

    private Activity? StartWorkspaceActivity(string activityName, ActivityKind activityKind = ActivityKind.Internal)
    {
        var activity = _instrumentation.Source.StartActivity(activityName, activityKind);
        activity?.SetTag("kubernetes.cluster.name", Runtime.Name);
        return activity;
    }

    public Task Connect()
    {
        lock (_connectLock)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (_connectTask is { IsCompleted: false })
            {
                return _connectTask;
            }

            _connectTask = Task.Run(ConnectCoreAsync);
            return _connectTask;
        }
    }

    private async Task ConnectCoreAsync()
    {
        using var activity = StartWorkspaceActivity(nameof(Connect), ActivityKind.Client);
        try
        {
            EnsureBuiltInResourceConfigs();
            await Runtime.Connect().ConfigureAwait(false);
            if (!Runtime.Connected)
            {
                return;
            }

            if (!IsEnabled || _disposed)
            {
                await Disconnect().ConfigureAwait(false);
                return;
            }

            if (_workspaceStateInitialized)
            {
                return;
            }

            await UpdateResourceConfigsPermissionsAndEvaluateAsync().ConfigureAwait(false);
            await SeedResourcesConfiguredForConnectAsync().ConfigureAwait(false);
            await SeedPermissionWatchersAsync().ConfigureAwait(false);
            _workspaceStateInitialized = true;
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            Runtime.LastError = ex.Message;
            Runtime.Status = ClusterStatus.Errored;

            _logger.LogError(ex, "Error connecting to cluster {ClusterName}", Runtime.Name);
        }
    }

    /// <summary>
    /// Discards cached authorization rules and re-evaluates every resource configuration.
    /// Call from explicit permission triggers only; this method does not schedule polling.
    /// </summary>
    public Task RefreshPermissionsAsync()
    {
        return _permissionRefreshCoordinator.RefreshAsync();
    }

    private bool CanRefreshPermissions()
    {
        return !_disposed && _workspaceStateInitialized && Runtime.Connected;
    }

    private void OnPermissionsInvalidated()
    {
        _permissionRefreshCoordinator.Invalidate();
    }

    private async Task RefreshPermissionsCoreAsync(
        PermissionRefreshScope scope,
        CancellationToken cancellationToken)
    {
        if (scope == PermissionRefreshScope.All)
        {
            Runtime.Permissions.InvalidatePermissions();
        }
        else
        {
            Runtime.Permissions.InvalidateNamespacePermissions();
        }

        await UpdateResourceConfigsPermissionsAndEvaluateAsync().ConfigureAwait(false);
        await SeedPermissionWatchersAsync().ConfigureAwait(false);
    }

    private async Task UpdateResourceConfigsPermissionsAndEvaluateAsync(
        IReadOnlyCollection<IClusterResourceConfig>? resourceConfigs = null)
    {
        using var activity = StartWorkspaceActivity(nameof(UpdateResourceConfigsPermissionsAndEvaluateAsync));

        var orderBatches = (resourceConfigs ?? GetResourceConfigs())
            .GroupBy(static config => config.Order)
            .OrderBy(static batch => batch.Key);

        foreach (var orderBatch in orderBatches)
        {
            await Parallel.ForEachAsync(
                orderBatch,
                new ParallelOptions { MaxDegreeOfParallelism = 4 },
                async (resourceConfig, _) =>
                {
                    await UpdateResourceConfigPermissionsAndEvaluateAsync(resourceConfig).ConfigureAwait(false);
                    NotifyResourceConfigProcessed(resourceConfig);
                }).ConfigureAwait(false);
        }
    }

    private async Task UpdateResourceConfigPermissionsAsync(IClusterResourceConfig resourceConfig)
    {
        foreach (var request in resourceConfig.AuthorizationRequests().Distinct())
        {
            await Runtime.Permissions.UpdatePermissionsAllNamespaceAsync(
                request.ResourceKind,
                resourceConfig.IsNamespaced,
                request.Verb,
                request.Subresource).ConfigureAwait(false);
        }
    }

    private async Task UpdateResourceConfigPermissionsAndEvaluateAsync(IClusterResourceConfig resourceConfig)
    {
        await UpdateResourceConfigPermissionsAsync(resourceConfig).ConfigureAwait(false);
        await EvaluateResourceConfigAccessCoreAsync(resourceConfig).ConfigureAwait(false);
    }

    public async Task Disconnect()
    {
        await Runtime.Disconnect().ConfigureAwait(false);
        _workspaceStateInitialized = false;
        ResetWorkspaceState();
    }

    public bool CanReadEvents(IKubernetesObject<V1ObjectMeta> resource)
    {
        ArgumentNullException.ThrowIfNull(resource);

        var @namespace = resource.Metadata?.NamespaceProperty;
        return Runtime.Connected
            && Runtime.Permissions.CanI<Corev1Event>(Verb.List, @namespace)
            && Runtime.Permissions.CanI<Corev1Event>(Verb.Watch, @namespace);
    }

    public IClusterResourceConfig GetResourceConfig(GroupApiVersionKind kind)
    {
        return _resourceConfigs[kind];
    }

    /// <summary>Attempts to get the resource configuration registered for a resource kind.</summary>
    /// <param name="kind">The resource kind to look up.</param>
    /// <param name="resourceConfig">The registered configuration, when found.</param>
    /// <returns><see langword="true"/> when a configuration is registered for <paramref name="kind"/>.</returns>
    public bool TryGetResourceConfig(GroupApiVersionKind kind, [NotNullWhen(true)] out IClusterResourceConfig? resourceConfig)
    {
        return _resourceConfigs.TryGetValue(kind, out resourceConfig);
    }

    public IClusterResourceConfig? GetResourceConfig(IKubernetesObject<V1ObjectMeta> resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        return Runtime.ModelCatalog.TryGetResourceKind(resource, out var kind)
            ? _resourceConfigs.GetValueOrDefault(kind)
            : null;
    }

    public IEnumerable<IClusterResourceConfig> GetResourceConfigs()
    {
        return _resourceConfigs.Values.ToList();
    }

    internal void AddResourceConfigForTest(IClusterResourceConfig resourceConfig)
    {
        resourceConfig.Initialize(this);
        if (resourceConfig.IsCustomResource)
        {
            RemoveOtherCustomResourceVersions(resourceConfig.Kind);
        }

        _resourceConfigs[resourceConfig.Kind] = resourceConfig;

        NotifyResourceConfigProcessed(resourceConfig);
    }

    public void Dispose()
    {
        lock (_connectLock)
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;
        }

        Runtime.OnChange -= OnRuntimeChange;
        if (Runtime.Permissions is { } permissions)
        {
            permissions.PermissionsInvalidated -= OnPermissionsInvalidated;
        }

        if (_namespaces is not null)
        {
            _namespaces.CollectionChanged -= OnNamespacesChanged;
            _namespaces = null;
        }

        if (Runtime is INotifyPropertyChanged runtime)
        {
            runtime.PropertyChanged -= OnRuntimePropertyChanged;
        }

        _permissionRefreshCoordinator.Dispose();
        _disposeCancellation.Cancel();
        _disposeCancellation.Dispose();
    }

    internal void DisconnectAfterRemoval()
    {
        Dispose();
        _ = DisconnectAfterRemovalAsync();
    }

    private async Task DisconnectAfterRemovalAsync()
    {
        try
        {
            Task? connectTask;
            lock (_connectLock)
            {
                connectTask = _connectTask;
            }

            if (connectTask is not null)
            {
                await connectTask.ConfigureAwait(false);
            }

            if (Runtime.Connected)
            {
                await Disconnect().ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error disconnecting removed cluster {ClusterName}", Runtime.Name);
        }
    }

    private void EnsureBuiltInResourceConfigs()
    {
        using var activity = StartWorkspaceActivity(nameof(EnsureBuiltInResourceConfigs));

        foreach (var resourceConfig in _serviceProvider.GetServices<IClusterResourceConfig>())
        {
            if (resourceConfig.IsCustomResource)
            {
                continue;
            }

            resourceConfig.Initialize(this);
            _resourceConfigs[resourceConfig.Kind] = resourceConfig;
        }
    }

    private async Task EvaluateResourceConfigAccessCoreAsync(IClusterResourceConfig resourceConfig)
    {
        ArgumentNullException.ThrowIfNull(resourceConfig);

        try
        {
            await resourceConfig.EvaluateListWatchAccessAsync().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Activity.Current?.SetStatus(ActivityStatusCode.Error, ex.Message);
            _logger.LogDebug(ex, "Unable to evaluate permissions for {Kind}", resourceConfig.Kind);
        }
    }

    private async Task SeedResourcesConfiguredForConnectAsync()
    {
        using var activity = StartWorkspaceActivity(nameof(SeedResourcesConfiguredForConnectAsync));

        var seedBatches = _resourceConfigs.Values
            .Where(static config => config.SeedOnConnect && config.PermissionsLoaded && config.CanListAndWatch)
            .GroupBy(static config => config.Order)
            .OrderBy(static batch => batch.Key);

        foreach (var seedBatch in seedBatches)
        {
            await Task.WhenAll(seedBatch.Select(EnsureResourceSeededAsync)).ConfigureAwait(false);
        }
    }

    private async Task SeedPermissionWatchersAsync()
    {
        foreach (var kind in PermissionWatchKinds)
        {
            if (_resourceConfigs.TryGetValue(kind, out var resourceConfig)
                && resourceConfig.PermissionsLoaded
                && resourceConfig.CanListAndWatch)
            {
                await EnsureResourceSeededAsync(resourceConfig).ConfigureAwait(false);
            }
        }
    }

    private async Task EnsureResourceSeededAsync(IClusterResourceConfig resourceConfig)
    {
        using var activity = StartWorkspaceActivity(nameof(EnsureResourceSeededAsync));

        if (Runtime.Objects.TryGetValue(resourceConfig.Kind, out var existing)
            && existing is IResourceContainer { IsSeeded: true })
        {
            return;
        }

        await resourceConfig.SeedResource().ConfigureAwait(false);
    }

    private void NotifyResourceConfigProcessed(IClusterResourceConfig resourceConfig)
    {
        using var activity = StartWorkspaceActivity(nameof(NotifyResourceConfigProcessed));
        activity?.SetTag("kubernetes.resource_config.kind", resourceConfig.Kind.ToString());

        ResourceConfigProcessed?.Invoke(this, resourceConfig);
    }

    private void SubscribeRuntime()
    {
        Runtime.OnChange += OnRuntimeChange;
        if (Runtime.Permissions is { } permissions)
        {
            permissions.PermissionsInvalidated += OnPermissionsInvalidated;
        }

        if (Runtime is INotifyPropertyChanged runtime)
        {
            runtime.PropertyChanged += OnRuntimePropertyChanged;
        }
    }

    private void SubscribeNamespaces()
    {
        if (_namespaces is not null)
        {
            _namespaces.CollectionChanged -= OnNamespacesChanged;
        }

        _namespaces = Runtime.Namespaces;
        if (_namespaces is not null)
        {
            _namespaces.CollectionChanged += OnNamespacesChanged;
        }
    }

    private void OnNamespacesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        _permissionRefreshCoordinator.Request(PermissionRefreshScope.Namespaces);
    }

    private void OnRuntimePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IClusterRuntime.Status))
        {
            Dispatcher.UIThread.Post(UpdateClusterColor);
            return;
        }

        if (e.PropertyName == nameof(IClusterRuntime.Namespaces))
        {
            SubscribeNamespaces();
        }
        else if (e.PropertyName == nameof(IClusterRuntime.Name))
        {
            OnPropertyChanged(nameof(DisplayName));
            OnPropertyChanged(nameof(Title));
        }
    }

    private void ResetWorkspaceState()
    {
        _customResourceDefinitionRevisions.Clear();
        _customResourceDefinitionObservedGenerations.Clear();

        var removedCustomResourceKinds = _resourceConfigs
            .Where(static pair => pair.Value.IsCustomResource)
            .Select(static pair => pair.Key)
            .ToList();

        if (removedCustomResourceKinds.Count == 0)
        {
            return;
        }

        foreach (var removedKind in removedCustomResourceKinds)
        {
            if (_resourceConfigs.TryRemove(removedKind, out _))
            {
                NotifyCustomResourceDefinitionRemoved(removedKind);
            }
        }
    }

    private void OnRuntimeChange(WatchEventType eventType, GroupApiVersionKind resourceKind, IKubernetesObject<V1ObjectMeta> item)
    {
        if (Array.IndexOf(PermissionWatchKinds, resourceKind) >= 0)
        {
            _permissionRefreshCoordinator.Request(PermissionRefreshScope.All);
            return;
        }

        if (item is not V1CustomResourceDefinition crd)
        {
            return;
        }

        var definitionName = crd.Name() ?? string.Empty;

        if (eventType == WatchEventType.Deleted)
        {
            RemoveCustomResourceDefinition(definitionName, crd);
            _customResourceDefinitionRevisions.TryRemove(definitionName, out _);
            _customResourceDefinitionObservedGenerations.TryRemove(definitionName, out _);
            CustomResourceDefinitionChanged?.Invoke(this);
            return;
        }

        if (!crd.TryGetResourceKind(out _))
        {
            return;
        }

        if (eventType == WatchEventType.Modified
            && crd.Metadata?.Generation is long observedGeneration
            && observedGeneration > 0
            && _customResourceDefinitionObservedGenerations.TryGetValue(definitionName, out var previousGeneration)
            && previousGeneration == observedGeneration)
        {
            return;
        }

        // Navigation availability depends on the CRD itself, even when access to
        // instances of the custom resource is denied.
        CustomResourceDefinitionChanged?.Invoke(this);

        // The generation is recorded here, before the definition is processed, so that a
        // repeated event for the same generation does not start a second processing pass.
        // A definition that cannot resolve a served storage version returns above and is
        // never recorded, so it is retried when the definition changes.
        if (crd.Metadata?.Generation is long generation && generation > 0)
        {
            _customResourceDefinitionObservedGenerations[definitionName] = generation;
        }

        var revision = Interlocked.Increment(ref _customResourceDefinitionRevision);
        _customResourceDefinitionRevisions[definitionName] = revision;
        _ = ProcessCustomResourceDefinitionAsync(crd, definitionName, revision);
    }

    private void RemoveOtherCustomResourceVersions(GroupApiVersionKind currentKind)
    {
        var previousKinds = _resourceConfigs
            .Where(pair => pair.Value.IsCustomResource
                && pair.Key != currentKind
                && string.Equals(pair.Key.Group, currentKind.Group, StringComparison.Ordinal)
                && string.Equals(pair.Key.Kind, currentKind.Kind, StringComparison.Ordinal))
            .Select(static pair => pair.Key)
            .ToArray();

        foreach (var previousKind in previousKinds)
        {
            RemoveCustomResourceDefinition(previousKind);
        }
    }

    private async Task ProcessCustomResourceDefinitionAsync(
        V1CustomResourceDefinition crd,
        string definitionName,
        long revision)
    {
        using var activity = StartWorkspaceActivity(nameof(ProcessCustomResourceDefinitionAsync));
        activity?.SetTag("kubernetes.crd.name", crd.Name());

        try
        {
            var resourceConfig = _serviceProvider.GetRequiredService<IClusterCustomResourceConfigFactory>().Create(crd);
            resourceConfig.Initialize(this);
            resourceConfig.Configure(crd);

            _logger.LogDebug(
                "Custom resource definition discovered for {ClusterName}: Definition={DefinitionName}; ResourceKind={ResourceKind}; Revision={Revision}",
                Runtime.Name,
                definitionName,
                resourceConfig.Kind,
                revision);

            await UpdateResourceConfigPermissionsAndEvaluateAsync(resourceConfig).ConfigureAwait(false);

            _logger.LogDebug(
                "Custom resource definition access evaluated for {ClusterName}: Definition={DefinitionName}; ResourceKind={ResourceKind}; PermissionsLoaded={PermissionsLoaded}; CanListAndWatch={CanListAndWatch}; Revision={Revision}",
                Runtime.Name,
                definitionName,
                resourceConfig.Kind,
                resourceConfig.PermissionsLoaded,
                resourceConfig.CanListAndWatch,
                revision);

            if (!resourceConfig.CanListAndWatch)
            {
                _logger.LogDebug(
                    "Custom resource definition skipped for {ClusterName}: list/watch access denied; Definition={DefinitionName}; ResourceKind={ResourceKind}",
                    Runtime.Name,
                    definitionName,
                    resourceConfig.Kind);
                return;
            }

            if (!_customResourceDefinitionRevisions.TryGetValue(definitionName, out var currentRevision)
                || currentRevision != revision)
            {
                _logger.LogDebug(
                    "Custom resource definition skipped for {ClusterName}: stale revision; Definition={DefinitionName}; ResourceKind={ResourceKind}; CurrentRevision={CurrentRevision}; Revision={Revision}",
                    Runtime.Name,
                    definitionName,
                    resourceConfig.Kind,
                    currentRevision,
                    revision);
                return;
            }

            RemoveOtherCustomResourceVersions(resourceConfig.Kind);
            _resourceConfigs[resourceConfig.Kind] = resourceConfig;
            _logger.LogDebug(
                "Custom resource definition registered for {ClusterName}: Definition={DefinitionName}; ResourceKind={ResourceKind}; ConfigCount={ConfigCount}",
                Runtime.Name,
                definitionName,
                resourceConfig.Kind,
                _resourceConfigs.Count);
            NotifyResourceConfigProcessed(resourceConfig);
        }
        catch (OperationCanceledException) when (_disposeCancellation.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            _logger.LogError(ex, "Error processing custom resource definition {Crd}", crd.Name());
        }
    }

    private void RemoveCustomResourceDefinition(string definitionName, V1CustomResourceDefinition crd)
    {
        var removedKinds = _resourceConfigs
            .Where(pair => pair.Value.IsCustomResource
                && MatchesCustomResourceDefinition(pair.Value, definitionName, crd))
            .Select(static pair => pair.Key)
            .ToArray();

        foreach (var kind in removedKinds)
        {
            RemoveCustomResourceDefinition(kind);
        }
    }

    private static bool MatchesCustomResourceDefinition(
        IClusterResourceConfig resourceConfig,
        string definitionName,
        V1CustomResourceDefinition crd)
    {
        if (!string.IsNullOrEmpty(definitionName)
            && string.Equals(
                resourceConfig.CustomResourceDefinition?.Name(),
                definitionName,
                StringComparison.Ordinal))
        {
            return true;
        }

        return !string.IsNullOrEmpty(crd.Spec?.Group)
            && !string.IsNullOrEmpty(crd.Spec.Names?.Kind)
            && string.Equals(resourceConfig.Kind.Group, crd.Spec.Group, StringComparison.Ordinal)
            && string.Equals(resourceConfig.Kind.Kind, crd.Spec.Names.Kind, StringComparison.Ordinal);
    }

    private void RemoveCustomResourceDefinition(GroupApiVersionKind kind)
    {
        if (_resourceConfigs.TryRemove(kind, out _))
        {
            NotifyCustomResourceDefinitionRemoved(kind);
        }
    }

    private void NotifyCustomResourceDefinitionRemoved(GroupApiVersionKind kind)
    {
        CustomResourceDefinitionRemoved?.Invoke(this, kind);
    }

    private void UpdateClusterColor()
    {
        ClusterColor = Runtime.Status switch
        {
            ClusterStatus.Connecting => Brushes.Orange,
            ClusterStatus.Errored => Brushes.Red,
            ClusterStatus.Connected => GetConnectedBrush(),
            _ => Brushes.Red,
        };
    }

    private IBrush GetConnectedBrush()
    {
        var properties = typeof(Brushes)
            .GetProperties(BindingFlags.Public | BindingFlags.Static)
            .Where(x => x.Name != nameof(Brushes.Red) && x.Name != nameof(Brushes.Orange))
            .ToArray();

        return (IBrush)properties[RandomNumberGenerator.GetInt32(properties.Length)].GetValue(null)!;
    }
}
