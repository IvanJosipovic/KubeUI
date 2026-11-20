using System.Collections;
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
    private readonly Lock _changedNamespacesLock = new();
    private HashSet<string>? _changedNamespaces;
    private readonly PermissionRefreshCoordinator _permissionRefreshCoordinator;
    private long _customResourceDefinitionRevision;
    private long _connectGeneration;
    private volatile bool _disposed;
    private volatile bool _workspaceStateInitialized;
    private Task? _connectTask;
    private Task? _disconnectTask;
    private long _connectTaskGeneration;
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
            exception =>
            {
                // Avoid allocating the logging arguments when debug logging is disabled.
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug(exception, "Unable to refresh permissions for cluster {ClusterName}", Runtime.Name);
                }
            });

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
            if (_connectTask is { IsCompleted: false } && _connectTaskGeneration == _connectGeneration)
            {
                return _connectTask;
            }

            var generation = _connectGeneration;
            _connectTaskGeneration = generation;
            _connectTask = _disconnectTask is { IsCompleted: false } disconnectTask
                ? ConnectAfterDisconnectAsync(disconnectTask, generation)
                : Task.Run(() => ConnectCoreAsync(generation));
            return _connectTask;
        }
    }

    private async Task ConnectAfterDisconnectAsync(Task disconnectTask, long generation)
    {
        await disconnectTask.ConfigureAwait(false);
        await ConnectCoreAsync(generation).ConfigureAwait(false);
    }

    private async Task ConnectCoreAsync(long generation)
    {
        using var activity = StartWorkspaceActivity(nameof(Connect), ActivityKind.Client);
        try
        {
            if (!IsConnectGenerationCurrent(generation))
            {
                await WaitForDisabledDisconnectAsync().ConfigureAwait(false);
                return;
            }

            EnsureBuiltInResourceConfigs();
            await Runtime.Connect().ConfigureAwait(false);
            if (!IsConnectGenerationCurrent(generation))
            {
                await WaitForDisabledDisconnectAsync().ConfigureAwait(false);
                return;
            }

            if (!Runtime.Connected)
            {
                return;
            }

            if (!IsEnabled || _disposed)
            {
                await DisconnectCoreAsync().ConfigureAwait(false);
                return;
            }

            if (_workspaceStateInitialized)
            {
                return;
            }

            await UpdateResourceConfigsPermissionsAndEvaluateAsync(connectGeneration: generation).ConfigureAwait(false);
            if (!IsConnectGenerationCurrent(generation))
            {
                await WaitForDisabledDisconnectAsync().ConfigureAwait(false);
                return;
            }

            if (!Runtime.Connected || !IsEnabled || _disposed)
            {
                return;
            }

            await SeedResourcesConfiguredForConnectAsync(generation).ConfigureAwait(false);
            if (!IsConnectGenerationCurrent(generation))
            {
                await WaitForDisabledDisconnectAsync().ConfigureAwait(false);
                return;
            }

            if (!Runtime.Connected || !IsEnabled || _disposed)
            {
                return;
            }

            await SeedPermissionWatchersAsync(generation).ConfigureAwait(false);
            if (!IsConnectGenerationCurrent(generation))
            {
                await WaitForDisabledDisconnectAsync().ConfigureAwait(false);
                return;
            }

            if (!Runtime.Connected || !IsEnabled || _disposed)
            {
                return;
            }

            var initialized = false;
            lock (_connectLock)
            {
                if (IsConnectGenerationCurrent(generation)
                    && Runtime.Connected
                    && IsEnabled
                    && !_disposed)
                {
                    _workspaceStateInitialized = true;
                    initialized = true;
                }
            }

            if (!initialized)
            {
                await WaitForDisabledDisconnectAsync().ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            if (!IsConnectGenerationCurrent(generation))
            {
                await WaitForDisabledDisconnectAsync().ConfigureAwait(false);
                // Avoid allocating the logging arguments when debug logging is disabled.
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug(ex, "Ignoring error from stale connection attempt for cluster {ClusterName}", Runtime.Name);
                }

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
            HashSet<string>? changedNamespaces;
            lock (_changedNamespacesLock)
            {
                changedNamespaces = _changedNamespaces;
                _changedNamespaces = null;
            }

            // A namespace-set change only invalidates the namespaces that actually changed.
            // If the change could not be resolved, discard every namespace-scoped decision.
            if (changedNamespaces is { Count: > 0 })
            {
                Runtime.Permissions.InvalidateNamespaces(changedNamespaces);
            }
            else
            {
                Runtime.Permissions.InvalidateNamespacePermissions();
            }
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
            if (connectGeneration.HasValue && !IsConnectGenerationCurrent(connectGeneration.Value))
            {
                return;
            }

            await Parallel.ForEachAsync(
                orderBatch,
                new ParallelOptions { MaxDegreeOfParallelism = 4 },
                async (resourceConfig, _) =>
                {
                    if (connectGeneration.HasValue && !IsConnectGenerationCurrent(connectGeneration.Value))
                    {
                        return;
                    }

                    await UpdateResourceConfigPermissionsAndEvaluateAsync(resourceConfig).ConfigureAwait(false);
                    if (connectGeneration.HasValue && !IsConnectGenerationCurrent(connectGeneration.Value))
                    {
                        return;
                    }

                    NotifyResourceConfigProcessed(resourceConfig);
                }).ConfigureAwait(false);
        }
    }

    private async Task UpdateResourceConfigPermissionsAsync(IClusterResourceConfig resourceConfig)
    {
        // Navigation visibility and informer seeding only depend on List and Watch.
        // Command verbs (create/delete/patch/update) are requested on demand by the
        // resource config's command enablement instead of being reviewed for every config
        // on connect.
        foreach (var request in resourceConfig.ListWatchAuthorizationRequests().Distinct())
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

    public Task Disconnect()
    {
        lock (_connectLock)
        {
            _connectGeneration++;
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

    private bool IsConnectGenerationCurrent(long generation)
    {
        return Volatile.Read(ref _connectGeneration) == generation;
    }

    private async Task WaitForDisabledDisconnectAsync()
    {
        if (!IsEnabled || _disposed)
        {
            await Disconnect().ConfigureAwait(false);
        }
    }

    private async Task DisconnectCoreAsync()
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
            // Avoid allocating the logging arguments when debug logging is disabled.
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug(ex, "Unable to evaluate permissions for {Kind}", resourceConfig.Kind);
            }
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
            if (!IsConnectGenerationCurrent(generation))
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
            if (generation is long currentGeneration && !IsConnectGenerationCurrent(currentGeneration))
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

        if (generation is long currentGeneration && !IsConnectGenerationCurrent(currentGeneration))
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
        var changedNamespaces = GetChangedNamespaceNames(e);
        lock (_changedNamespacesLock)
        {
            if (changedNamespaces is null || changedNamespaces.Count == 0)
            {
                // A Reset (or an event without usable items) does not identify the affected
                // namespaces, so the refresh must discard all namespace-scoped decisions.
                _changedNamespaces = null;
            }
            else
            {
                _changedNamespaces ??= new HashSet<string>(StringComparer.Ordinal);
                _changedNamespaces.UnionWith(changedNamespaces);
            }
        }

        _permissionRefreshCoordinator.Request(PermissionRefreshScope.Namespaces);
    }

    private static HashSet<string>? GetChangedNamespaceNames(NotifyCollectionChangedEventArgs e)
    {
        HashSet<string>? names = null;
        AddNamespaceNames(e.OldItems, ref names);
        AddNamespaceNames(e.NewItems, ref names);
        return names;
    }

    private static void AddNamespaceNames(IList? items, ref HashSet<string>? names)
    {
        if (items is null)
        {
            return;
        }

        foreach (var item in items)
        {
            if (item is not V1Namespace @namespace)
            {
                continue;
            }

            var name = @namespace.Name();
            if (string.IsNullOrWhiteSpace(name))
            {
                continue;
            }

            (names ??= new HashSet<string>(StringComparer.Ordinal)).Add(name);
        }
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

            // Avoid allocating the logging arguments when debug logging is disabled.
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug(
                    "Custom resource definition discovered for {ClusterName}: Definition={DefinitionName}; ResourceKind={ResourceKind}; Revision={Revision}",
                    Runtime.Name,
                    definitionName,
                    resourceConfig.Kind,
                    revision);
            }

            await UpdateResourceConfigPermissionsAndEvaluateAsync(resourceConfig).ConfigureAwait(false);

            // Avoid allocating the logging arguments when debug logging is disabled.
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug(
                    "Custom resource definition access evaluated for {ClusterName}: Definition={DefinitionName}; ResourceKind={ResourceKind}; PermissionsLoaded={PermissionsLoaded}; CanListAndWatch={CanListAndWatch}; Revision={Revision}",
                    Runtime.Name,
                    definitionName,
                    resourceConfig.Kind,
                    resourceConfig.PermissionsLoaded,
                    resourceConfig.CanListAndWatch,
                    revision);
            }

            if (!resourceConfig.CanListAndWatch)
            {
                // Avoid allocating the logging arguments when debug logging is disabled.
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug(
                        "Custom resource definition skipped for {ClusterName}: list/watch access denied; Definition={DefinitionName}; ResourceKind={ResourceKind}",
                        Runtime.Name,
                        definitionName,
                        resourceConfig.Kind);
                }

                return;
            }

            if (!_customResourceDefinitionRevisions.TryGetValue(definitionName, out var currentRevision)
                || currentRevision != revision)
            {
                // Avoid allocating the logging arguments when debug logging is disabled.
                if (_logger.IsEnabled(LogLevel.Debug))
                {
                    _logger.LogDebug(
                        "Custom resource definition skipped for {ClusterName}: stale revision; Definition={DefinitionName}; ResourceKind={ResourceKind}; CurrentRevision={CurrentRevision}; Revision={Revision}",
                        Runtime.Name,
                        definitionName,
                        resourceConfig.Kind,
                        currentRevision,
                        revision);
                }

                return;
            }

            RemoveOtherCustomResourceVersions(resourceConfig.Kind);
            _resourceConfigs[resourceConfig.Kind] = resourceConfig;
            // Avoid allocating the logging arguments when debug logging is disabled.
            if (_logger.IsEnabled(LogLevel.Debug))
            {
                _logger.LogDebug(
                    "Custom resource definition registered for {ClusterName}: Definition={DefinitionName}; ResourceKind={ResourceKind}; ConfigCount={ConfigCount}",
                    Runtime.Name,
                    definitionName,
                    resourceConfig.Kind,
                    _resourceConfigs.Count);
            }

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
