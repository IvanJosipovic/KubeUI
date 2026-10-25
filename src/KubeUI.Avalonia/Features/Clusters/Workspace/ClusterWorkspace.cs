using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using System.Security.Cryptography;
using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Infrastructure.Platform;
using KubeUI.Avalonia.Resources;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.Clusters.Workspace;

public sealed partial class ClusterWorkspace : ObservableObject, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ClusterWorkspace> _logger;
    private readonly ConcurrentDictionary<GroupApiVersionKind, IResourceConfig> _resourceConfigs = new();
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

    private const int PermissionRefreshNone = 0;
    private const int PermissionRefreshAll = 1;
    private const int PermissionRefreshNamespaces = 2;
    private static readonly TimeSpan PermissionRefreshDebounce = TimeSpan.FromMilliseconds(500);

    private readonly Lock _connectLock = new();
    private readonly SemaphoreSlim _permissionRefreshGate = new(1, 1);
    private long _customResourceDefinitionRevision;
    private int _permissionRefreshRequested;
    private int _permissionRefreshWorkerRunning;
    private int _permissionRefreshInProgress;
    private volatile bool _disposed;
    private volatile bool _workspaceStateInitialized;
    private Task? _connectTask;
    private INotifyCollectionChanged? _namespaces;

    private Instrumentation _instrumentation;

    public ClusterWorkspace(
        IClusterRuntime runtime,
        IServiceProvider serviceProvider,
        ILogger<ClusterWorkspace> logger, Instrumentation instrumentation)
    {
        Runtime = runtime;
        _serviceProvider = serviceProvider;
        _logger = logger;
        _instrumentation = instrumentation;

        SubscribeRuntime();
        SubscribeNamespaces();
        UpdateClusterColor();
    }

    [ObservableProperty]
    public partial IClusterRuntime Runtime { get; set; }

    public string Title => Runtime.Name;

    internal IServiceProvider ServiceProvider => _serviceProvider;

    [ObservableProperty]
    public partial IBrush ClusterColor { get; set; } = Brushes.Red;

    [ObservableProperty]
    public partial ObservableCollection<V1Namespace> SelectedNamespaces { get; set; } = [];

    /// <summary>
    /// Raised after a resource config has been permission-evaluated and registered by the workspace.
    /// </summary>
    public event Action<ClusterWorkspace, IResourceConfig>? ResourceConfigProcessed;
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
    public async Task RefreshPermissionsAsync()
    {
        if (!Runtime.Connected || !_workspaceStateInitialized || _disposed)
        {
            return;
        }

        await RefreshPermissionsCoreAsync(PermissionRefreshAll, CancellationToken.None).ConfigureAwait(false);
    }

    private void OnPermissionsInvalidated()
    {
        if (_disposed
            || !_workspaceStateInitialized
            || !Runtime.Connected
            || Volatile.Read(ref _permissionRefreshInProgress) != 0)
        {
            return;
        }

        RequestPermissionRefresh(PermissionRefreshAll);
    }

    private void RequestPermissionRefresh(int scope)
    {
        if (_disposed || !_workspaceStateInitialized || !Runtime.Connected)
        {
            return;
        }

        Interlocked.Or(ref _permissionRefreshRequested, scope);
        SchedulePermissionRefreshWorker();
    }

    private void SchedulePermissionRefreshWorker()
    {
        if (Interlocked.CompareExchange(ref _permissionRefreshWorkerRunning, 1, 0) != 0)
        {
            return;
        }

        _ = Task.Run(RunPermissionRefreshWorkerAsync);
    }

    private async Task RunPermissionRefreshWorkerAsync()
    {
        var cancellationToken = _disposeCancellation.Token;
        try
        {
            while (true)
            {
                var scope = Interlocked.Exchange(ref _permissionRefreshRequested, PermissionRefreshNone);
                if (scope == PermissionRefreshNone)
                {
                    break;
                }

                await Task.Delay(PermissionRefreshDebounce, cancellationToken).ConfigureAwait(false);
                if (!Runtime.Connected || !_workspaceStateInitialized || _disposed)
                {
                    continue;
                }

                await RefreshPermissionsCoreAsync(scope, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Unable to refresh permissions for cluster {ClusterName}", Runtime.Name);
        }
        finally
        {
            Interlocked.Exchange(ref _permissionRefreshWorkerRunning, 0);
            if (!_disposed && Volatile.Read(ref _permissionRefreshRequested) != PermissionRefreshNone)
            {
                SchedulePermissionRefreshWorker();
            }
        }
    }

    private async Task RefreshPermissionsCoreAsync(int scope, CancellationToken cancellationToken)
    {
        var isFullRefresh = (scope & PermissionRefreshAll) != 0;
        await _permissionRefreshGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        if (isFullRefresh)
        {
            Interlocked.Exchange(ref _permissionRefreshInProgress, 1);
        }

        try
        {
            if (isFullRefresh)
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
        finally
        {
            if (isFullRefresh)
            {
                Interlocked.Exchange(ref _permissionRefreshInProgress, 0);
            }

            _permissionRefreshGate.Release();
        }
    }

    private async Task UpdateResourceConfigsPermissionsAndEvaluateAsync(
        IReadOnlyCollection<IResourceConfig>? resourceConfigs = null)
    {
        using var activity = StartWorkspaceActivity(nameof(UpdateResourceConfigsPermissionsAndEvaluateAsync));

        var sectionBatches = (resourceConfigs ?? GetResourceConfigs())
            .GroupBy(static config => config.Navigation.SectionId, StringComparer.Ordinal)
            .OrderBy(static section => ResolveSectionOrder(section.Key, section.Min(config => config.Order)))
            .ThenBy(static section => section.Key, StringComparer.Ordinal);

        foreach (var sectionBatch in sectionBatches)
        {
            foreach (var orderBatch in sectionBatch.GroupBy(static config => config.Order).OrderBy(static batch => batch.Key))
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
    }

    private static int ResolveSectionOrder(string? sectionId, int fallbackOrder)
        => NavigationSections.Find(sectionId)?.Order ?? fallbackOrder;

    private async Task UpdateResourceConfigPermissionsAsync(IResourceConfig resourceConfig)
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

    private async Task UpdateResourceConfigPermissionsAndEvaluateAsync(IResourceConfig resourceConfig)
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

    public IResourceConfig GetResourceConfig(GroupApiVersionKind kind)
    {
        return _resourceConfigs[kind];
    }

    /// <summary>Attempts to get the resource configuration registered for a resource kind.</summary>
    /// <param name="kind">The resource kind to look up.</param>
    /// <param name="resourceConfig">The registered configuration, when found.</param>
    /// <returns><see langword="true"/> when a configuration is registered for <paramref name="kind"/>.</returns>
    public bool TryGetResourceConfig(GroupApiVersionKind kind, [NotNullWhen(true)] out IResourceConfig? resourceConfig)
    {
        return _resourceConfigs.TryGetValue(kind, out resourceConfig);
    }

    public ResourceConfigBase<T> GetResourceConfig<T>(GroupApiVersionKind kind)
        where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        return _resourceConfigs[kind] as ResourceConfigBase<T>
            ?? throw new InvalidOperationException($"Resource config {kind} is not configured for {typeof(T).Name}.");
    }

    public IResourceConfig? GetResourceConfig(IKubernetesObject<V1ObjectMeta> resource)
    {
        ArgumentNullException.ThrowIfNull(resource);
        return Runtime.ModelCatalog.TryGetResourceKind(resource, out var kind)
            ? _resourceConfigs.GetValueOrDefault(kind)
            : null;
    }

    public IEnumerable<IResourceConfig> GetResourceConfigs()
    {
        return _resourceConfigs.Values.ToList();
    }

    internal void AddResourceConfigForTest(IResourceConfig resourceConfig)
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
        if (_disposed)
        {
            return;
        }

        _disposed = true;
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

        _disposeCancellation.Cancel();
        _disposeCancellation.Dispose();
    }

    private void EnsureBuiltInResourceConfigs()
    {
        using var activity = StartWorkspaceActivity(nameof(EnsureBuiltInResourceConfigs));

        foreach (var resourceConfig in _serviceProvider.GetServices<IResourceConfig>())
        {
            if (resourceConfig.IsCustomResource)
            {
                continue;
            }

            resourceConfig.Initialize(this);
            _resourceConfigs[resourceConfig.Kind] = resourceConfig;
        }
    }

    private async Task EvaluateResourceConfigAccessCoreAsync(IResourceConfig resourceConfig)
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

    private async Task EnsureResourceSeededAsync(IResourceConfig resourceConfig)
    {
        using var activity = StartWorkspaceActivity(nameof(EnsureResourceSeededAsync));

        if (Runtime.Objects.TryGetValue(resourceConfig.Kind, out var existing)
            && existing is IResourceContainer { IsSeeded: true })
        {
            return;
        }

        await resourceConfig.SeedResource().ConfigureAwait(false);
    }

    private void NotifyResourceConfigProcessed(IResourceConfig resourceConfig)
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
        RequestPermissionRefresh(PermissionRefreshNamespaces);
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
            RequestPermissionRefresh(PermissionRefreshAll);
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
            var resourceConfig = CreateCustomResourceConfig(crd);
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

    private CustomResourceConfig CreateCustomResourceConfig(V1CustomResourceDefinition crd)
    {
        return CustomResourceConfig.Select(crd, _serviceProvider.GetServices<CustomResourceConfig>());
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
        IResourceConfig resourceConfig,
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
