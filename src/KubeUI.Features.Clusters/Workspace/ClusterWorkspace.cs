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
    private readonly CancellationTokenSource _disposeCancellation = new();
    private readonly SemaphoreSlim _discoveryReconcileGate = new(1, 1);
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
    private volatile bool _disposed;
    private volatile bool _workspaceStateInitialized;
    private long _connectGeneration;
    private long _connectTaskGeneration;
    private Task? _connectTask;
    private Task? _disconnectTask;
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
        Task? connectTask;
        lock (_connectLock)
        {
            connectTask = _connectTask;
        }

        try
        {
            if (connectTask is not null)
            {
                try
                {
                    await connectTask.ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    _logger.LogDebug(ex, "Connection initialization ended while disabling cluster {ClusterName}", Runtime.Name);
                }
            }

            if (!IsEnabled)
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

    /// <summary>Raised after a discovered custom-resource kind is removed from the workspace.</summary>
    public event Action<ClusterWorkspace, GroupApiVersionKind>? CustomResourceRemoved;

    /// <summary>Raised when a custom resource definition changes in the runtime.</summary>
    public event Action<ClusterWorkspace>? CustomResourceDefinitionChanged;

    private Activity? StartWorkspaceActivity(string activityName, ActivityKind activityKind = ActivityKind.Internal)
    {
        var activity = _instrumentation.Source.StartActivity(activityName, activityKind);
        activity?.SetTag("kubernetes.cluster.name", Runtime.Name);
        return activity;
    }

    /// <summary>Connects the runtime and initializes workspace resources for the current connection generation.</summary>
    public Task Connect()
    {
        lock (_connectLock)
        {
            ObjectDisposedException.ThrowIf(_disposed, this);
            if (!IsEnabled)
            {
                return Task.CompletedTask;
            }

            if (_connectTask is { IsCompleted: false } && _connectTaskGeneration == _connectGeneration)
            {
                return _connectTask;
            }

            var previousConnect = _connectTask;
            var previousDisconnect = _disconnectTask;
            var connectGeneration = ++_connectGeneration;
            _connectTaskGeneration = connectGeneration;
            _connectTask = previousConnect is { IsCompleted: false }
                || previousDisconnect is { IsCompleted: false }
                ? Task.Run(() => ConnectAfterPreviousAsync(previousConnect, previousDisconnect, connectGeneration))
                : Task.Run(() => ConnectCoreAsync(connectGeneration));
            return _connectTask;
        }
    }

    /// <summary>Waits for prior connection and disconnection work before initializing a replacement.</summary>
    private async Task ConnectAfterPreviousAsync(Task? previousConnect, Task? previousDisconnect, long connectGeneration)
    {
        if (previousConnect is not null)
        {
            try
            {
                await previousConnect.ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Prior cluster connection ended before a replacement connection started for {ClusterName}", Runtime.Name);
            }
        }

        if (previousDisconnect is not null)
        {
            await previousDisconnect.ConfigureAwait(false);
        }

        await ConnectCoreAsync(connectGeneration).ConfigureAwait(false);
    }

    /// <summary>Runs connection initialization while the supplied generation remains current.</summary>
    private async Task ConnectCoreAsync(long connectGeneration)
    {
        using var activity = StartWorkspaceActivity(nameof(Connect), ActivityKind.Client);
        try
        {
            if (!IsConnectGenerationCurrent(connectGeneration))
            {
                await WaitForDisabledDisconnectAsync().ConfigureAwait(false);
                return;
            }

            if (!IsEnabled)
            {
                return;
            }

            EnsureBuiltInResourceConfigs();
            await Runtime.Connect().ConfigureAwait(false);
            if (!IsConnectGenerationCurrent(connectGeneration))
            {
                await WaitForDisabledDisconnectAsync().ConfigureAwait(false);
                return;
            }

            if (!IsEnabled)
            {
                return;
            }

            if (!Runtime.Connected)
            {
                return;
            }

            await Runtime.RefreshApiDiscoveryAsync().ConfigureAwait(false);
            if (!IsConnectGenerationCurrent(connectGeneration) || !Runtime.Connected)
            {
                await WaitForDisabledDisconnectAsync().ConfigureAwait(false);
                return;
            }

            if (!IsEnabled || _disposed)
            {
                return;
            }

            if (Runtime.ApiDiscoverySnapshot is { } discoverySnapshot)
            {
                await ReconcileApiDiscoverySnapshotAsync(discoverySnapshot, evaluatePermissions: false).ConfigureAwait(false);
                if (!IsConnectGenerationCurrent(connectGeneration) || !Runtime.Connected)
                {
                    await WaitForDisabledDisconnectAsync().ConfigureAwait(false);
                    return;
                }

                if (!IsEnabled)
                {
                    return;
                }
            }

            if (_workspaceStateInitialized)
            {
                return;
            }

            await UpdateResourceConfigsPermissionsAndEvaluateAsync(connectGeneration: connectGeneration).ConfigureAwait(false);
            if (!IsConnectGenerationCurrent(connectGeneration))
            {
                await WaitForDisabledDisconnectAsync().ConfigureAwait(false);
                return;
            }

            if (!Runtime.Connected || !IsEnabled || _disposed)
            {
                return;
            }

            await SeedResourcesConfiguredForConnectAsync(connectGeneration).ConfigureAwait(false);
            if (!IsConnectGenerationCurrent(connectGeneration))
            {
                await WaitForDisabledDisconnectAsync().ConfigureAwait(false);
                return;
            }

            if (!Runtime.Connected || !IsEnabled || _disposed)
            {
                return;
            }

            await SeedPermissionWatchersAsync(connectGeneration).ConfigureAwait(false);
            if (!IsConnectGenerationCurrent(connectGeneration))
            {
                await WaitForDisabledDisconnectAsync().ConfigureAwait(false);
                return;
            }

            if (!Runtime.Connected || !IsEnabled || _disposed)
            {
                return;
            }

            if (!TryMarkWorkspaceInitialized(connectGeneration))
            {
                return;
            }

            if (Runtime.ApiDiscoverySnapshot is { } completedSnapshot)
            {
                await ReconcileApiDiscoverySnapshotAsync(completedSnapshot, evaluatePermissions: true).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) when (!IsConnectGenerationCurrent(connectGeneration)
            || _disposed
            || !IsEnabled
            || !Runtime.Connected)
        {
        }
        catch (Exception ex)
        {
            if (!IsConnectGenerationCurrent(connectGeneration))
            {
                await WaitForDisabledDisconnectAsync().ConfigureAwait(false);
                _logger.LogDebug(ex, "Ignoring error from stale connection attempt for cluster {ClusterName}", Runtime.Name);
                return;
            }

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

    /// <summary>Refreshes the complete authenticated Kubernetes API discovery snapshot.</summary>
    public Task RefreshApiDiscoveryAsync()
        => Runtime.RefreshApiDiscoveryAsync();

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
        IReadOnlyCollection<IClusterResourceConfig>? resourceConfigs = null,
        long? connectGeneration = null)
    {
        using var activity = StartWorkspaceActivity(nameof(UpdateResourceConfigsPermissionsAndEvaluateAsync));

        var orderBatches = (resourceConfigs ?? GetResourceConfigs())
            .GroupBy(static config => config.Order)
            .OrderBy(static batch => batch.Key);

        foreach (var orderBatch in orderBatches)
        {
            if (connectGeneration.HasValue && !IsConnectInitializationCurrent(connectGeneration.Value))
            {
                return;
            }

            await Parallel.ForEachAsync(
                orderBatch,
                new ParallelOptions { MaxDegreeOfParallelism = 4 },
                async (resourceConfig, _) =>
                {
                    if (connectGeneration.HasValue && !IsConnectInitializationCurrent(connectGeneration.Value))
                    {
                        return;
                    }

                    await UpdateResourceConfigPermissionsAndEvaluateAsync(resourceConfig).ConfigureAwait(false);
                    if (connectGeneration.HasValue && !IsConnectInitializationCurrent(connectGeneration.Value))
                    {
                        return;
                    }

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

    private async Task WaitForDisabledDisconnectAsync()
    {
        if (!IsEnabled || _disposed)
        {
            await Disconnect().ConfigureAwait(false);
        }
    }

    public Task Disconnect()
    {
        lock (_connectLock)
        {
            _connectGeneration++;
            _workspaceStateInitialized = false;
            _connectTask = null;
            _connectTaskGeneration = -1;
            if (_disconnectTask is { IsCompleted: false })
            {
                return _disconnectTask;
            }

            _disconnectTask = DisconnectCoreAsync();
            return _disconnectTask;
        }
    }

    private async Task DisconnectCoreAsync()
    {
        await Runtime.Disconnect().ConfigureAwait(false);
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

    /// <summary>Unsubscribes workspace handlers and cancels pending workspace operations.</summary>
    public void Dispose()
    {
        lock (_connectLock)
        {
            if (_disposed)
            {
                return;
            }
            _disposed = true;
            _connectGeneration++;
        }

        Runtime.OnChange -= OnRuntimeChange;
        Runtime.ApiDiscoverySnapshotRefreshed -= OnApiDiscoverySnapshotRefreshed;
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
            await Disconnect().ConfigureAwait(false);
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

    private bool IsConnectGenerationCurrent(long generation)
    {
        lock (_connectLock)
        {
            return !_disposed && _connectGeneration == generation;
        }
    }

    private bool IsConnectInitializationCurrent(long generation)
    {
        lock (_connectLock)
        {
            return !_disposed && IsEnabled && _connectGeneration == generation;
        }
    }

    /// <summary>Marks initialization complete only when the supplied connection generation is current.</summary>
    private bool TryMarkWorkspaceInitialized(long generation)
    {
        lock (_connectLock)
        {
            if (_disposed || !IsEnabled || _connectGeneration != generation)
            {
                return false;
            }

            _workspaceStateInitialized = true;
            return true;
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

    private async Task SeedResourcesConfiguredForConnectAsync(long generation)
    {
        using var activity = StartWorkspaceActivity(nameof(SeedResourcesConfiguredForConnectAsync));

        var seedBatches = _resourceConfigs.Values
            .Where(static config => config.SeedOnConnect && config.PermissionsLoaded && config.CanListAndWatch)
            .GroupBy(static config => config.Order)
            .OrderBy(static batch => batch.Key);

        foreach (var seedBatch in seedBatches)
        {
            if (!IsConnectInitializationCurrent(generation))
            {
                return;
            }

            await Task.WhenAll(seedBatch.Select(resourceConfig => EnsureResourceSeededAsync(resourceConfig, generation)))
                .ConfigureAwait(false);
        }
    }

    private async Task SeedPermissionWatchersAsync(long? generation = null)
    {
        foreach (var kind in PermissionWatchKinds)
        {
            if (generation is long currentGeneration && !IsConnectInitializationCurrent(currentGeneration))
            {
                return;
            }

            if (_resourceConfigs.TryGetValue(kind, out var resourceConfig)
                && resourceConfig.PermissionsLoaded
                && resourceConfig.CanListAndWatch)
            {
                await EnsureResourceSeededAsync(resourceConfig, generation).ConfigureAwait(false);
            }
        }
    }

    private async Task EnsureResourceSeededAsync(IClusterResourceConfig resourceConfig, long? generation = null)
    {
        using var activity = StartWorkspaceActivity(nameof(EnsureResourceSeededAsync));

        if (generation is long currentGeneration && !IsConnectInitializationCurrent(currentGeneration))
        {
            return;
        }

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

    /// <summary>Subscribes workspace state handlers to the current runtime and permission cache.</summary>
    private void SubscribeRuntime()
    {
        Runtime.OnChange += OnRuntimeChange;
        Runtime.ApiDiscoverySnapshotRefreshed += OnApiDiscoverySnapshotRefreshed;
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

    /// <summary>Removes discovery-backed resource configs when the runtime disconnects.</summary>
    private void ResetWorkspaceState()
    {
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
                NotifyCustomResourceRemoved(removedKind);
            }
        }
    }

    private void OnRuntimeChange(WatchEventType eventType, GroupApiVersionKind resourceKind, IKubernetesObject<V1ObjectMeta> item)
    {
        if (Array.IndexOf(PermissionWatchKinds, resourceKind) >= 0)
        {
            _permissionRefreshCoordinator.Request(PermissionRefreshScope.All);
        }

        if (item is V1CustomResourceDefinition)
        {
            CustomResourceDefinitionChanged?.Invoke(this);
        }
    }

    /// <summary>Reconciles refreshed discovery snapshots for the active connected runtime.</summary>
    private void OnApiDiscoverySnapshotRefreshed(IClusterRuntime runtime, ApiDiscoverySnapshot snapshot)
    {
        if (ReferenceEquals(runtime, Runtime) && _workspaceStateInitialized && !_disposed)
        {
            _ = ReconcileApiDiscoverySnapshotFromRefreshAsync(snapshot);
        }
    }

    /// <summary>Applies matching cached CRD printer columns to discovery-backed resource configs.</summary>
    private void ApplyCachedCustomResourcePrinterColumns()
    {
        if (!Runtime.Objects.ContainsKey(GroupApiVersionKind.From<V1CustomResourceDefinition>()))
        {
            return;
        }

        var definitions = Runtime.GetResourceList<V1CustomResourceDefinition>();
        foreach (var resourceConfig in _resourceConfigs.Values)
        {
            if (!resourceConfig.IsCustomResource
                || resourceConfig.CustomResourceDefinition is not null
                || resourceConfig is not IClusterCustomResourcePrinterColumnConsumer consumer)
            {
                continue;
            }

            var definition = definitions.FirstOrDefault(candidate =>
                candidate.Spec is { } spec
                && string.Equals(spec.Group, resourceConfig.Kind.Group, StringComparison.Ordinal)
                && string.Equals(spec.Names?.Kind, resourceConfig.Kind.Kind, StringComparison.Ordinal)
                && string.Equals(spec.Names?.Plural, resourceConfig.Kind.PluralName, StringComparison.Ordinal));
            consumer.ConfigurePrinterColumns(definition);
        }
    }

    /// <summary>Runs snapshot reconciliation for a refresh event and logs failures outside shutdown.</summary>
    private async Task ReconcileApiDiscoverySnapshotFromRefreshAsync(ApiDiscoverySnapshot snapshot)
    {
        try
        {
            await ReconcileApiDiscoverySnapshotAsync(snapshot, evaluatePermissions: true).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (_disposeCancellation.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error reconciling API discovery resources for cluster {ClusterName}", Runtime.Name);
        }
    }

    /// <summary>Replaces discovery-backed configs with resources in the supplied complete snapshot.</summary>
    private async Task ReconcileApiDiscoverySnapshotAsync(
        ApiDiscoverySnapshot snapshot,
        bool evaluatePermissions)
    {
        try
        {
            await _discoveryReconcileGate.WaitAsync(_disposeCancellation.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (_disposeCancellation.IsCancellationRequested)
        {
            return;
        }

        try
        {
            if (_disposed || !Runtime.Connected)
            {
                return;
            }

            var resources = snapshot.PreferredResources
                .Where(resource => Runtime.ModelCatalog.IsCustomResource(resource.Kind)
                    && resource.Verbs.Contains("list")
                    && resource.Verbs.Contains("watch"))
                .ToArray();
            var targetKinds = resources
                .Select(static resource => (resource.Kind.Group, resource.Kind.Kind))
                .ToHashSet();

            var removedKinds = _resourceConfigs
                .Where(pair => pair.Value.IsCustomResource
                    && !targetKinds.Contains((pair.Key.Group, pair.Key.Kind)))
                .Select(static pair => pair.Key)
                .ToArray();
            foreach (var removedKind in removedKinds)
            {
                RemoveCustomResourceConfig(removedKind);
            }

            var factory = _serviceProvider.GetRequiredService<IClusterCustomResourceConfigFactory>();
            foreach (var resource in resources)
            {
                if (_resourceConfigs.TryGetValue(resource.Kind, out var existingConfig)
                    && existingConfig.IsCustomResource
                    && existingConfig.CustomResourceDefinition is null
                    && existingConfig.IsNamespaced == resource.IsNamespaced)
                {
                    continue;
                }

                var resourceConfig = factory.Create(resource);
                resourceConfig.Initialize(this);
                resourceConfig.Configure(resource);

                if (evaluatePermissions)
                {
                    await UpdateResourceConfigPermissionsAndEvaluateAsync(resourceConfig).ConfigureAwait(false);
                }

                if (_disposed || !Runtime.Connected)
                {
                    return;
                }

                RemoveOtherCustomResourceVersions(resourceConfig.Kind);
                _resourceConfigs[resourceConfig.Kind] = resourceConfig;
                if (evaluatePermissions)
                {
                    NotifyResourceConfigProcessed(resourceConfig);
                }
            }

            ApplyCachedCustomResourcePrinterColumns();
        }
        finally
        {
            _discoveryReconcileGate.Release();
        }
    }

    /// <summary>Removes older served-version configs for the same discovered custom-resource kind.</summary>
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
            RemoveCustomResourceConfig(previousKind);
        }
    }

    private void RemoveCustomResourceConfig(GroupApiVersionKind kind)
    {
        if (_resourceConfigs.TryRemove(kind, out _))
        {
            NotifyCustomResourceRemoved(kind);
        }
    }

    private void NotifyCustomResourceRemoved(GroupApiVersionKind kind)
    {
        CustomResourceRemoved?.Invoke(this, kind);
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
