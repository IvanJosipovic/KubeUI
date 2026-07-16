using Avalonia.Threading;
using k8s.Models;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Avalonia.Resources;
using KubeUI.Kubernetes;
using KubernetesClient.Informer.Client;
using KubernetesObject = k8s.IKubernetesObject<k8s.Models.V1ObjectMeta>;

namespace KubeUI.Avalonia.Features.Clusters.Workspace.ViewModels;

/// <summary>
/// Cluster-scoped permission orchestration facade.
/// Delegates authorization evaluation to the runtime while owning workspace-level refresh sequencing and notifications.
/// </summary>
public sealed class ClusterPermissionCache : IInitializeCluster, IDisposable
{
    private readonly ILogger<ClusterPermissionCache> _logger;
    private readonly SemaphoreSlim _resourcePermissionRefreshGate = new(1, 1);
    private readonly object _resourcePermissionRefreshQueueLock = new();
    private readonly HashSet<KubernetesClient.Informer.Client.GroupApiVersionKind> _pendingResourcePermissionRefreshKinds = [];
    private bool _suppressPermissionRefresh;
    private bool _disposed;
    private bool _resourcePermissionRefreshProcessorRunning;
    private bool _refreshAllResourceConfigPermissionsPending;

    public ClusterPermissionCache(ILogger<ClusterPermissionCache> logger)
    {
        _logger = logger;
    }

    public ClusterWorkspaceViewModel? Cluster { get; private set; }

    public event Action<ClusterWorkspaceViewModel>? ResourcePermissionsChanged;

    public event Action<ClusterWorkspaceViewModel, IResourceConfig>? ResourceConfigPermissionsUpdated;

    public void Initialize(ClusterWorkspaceViewModel cluster)
    {
        ArgumentNullException.ThrowIfNull(cluster);

        if (Cluster != null && !ReferenceEquals(Cluster, cluster))
        {
            throw new InvalidOperationException("Permission cache is already initialized for a different cluster.");
        }

        Cluster = cluster;
    }

    public void SetPermissionRefreshSuppressed(bool value)
    {
        _suppressPermissionRefresh = value;
    }

    public bool CanI(Type type, Verb verb, string? @namespace = null, string? subresource = null)
    {
        return GetCluster().Runtime.CanI(type, verb, @namespace, subresource);
    }

    public bool CanI<T>(Verb verb, string? @namespace = null, string? subresource = null) where T : class, KubernetesObject, new()
    {
        return GetCluster().Runtime.CanI<T>(verb, @namespace, subresource);
    }

    public bool CanIAnyNamespace(Type type, Verb verb, string? subresource = null)
    {
        return GetCluster().Runtime.CanIAnyNamespace(type, verb, subresource);
    }

    public bool CanIAnyNamespace<T>(Verb verb, string? subresource = null) where T : class, KubernetesObject, new()
    {
        return GetCluster().Runtime.CanIAnyNamespace<T>(verb, subresource);
    }

    public Task RefreshAuthorizationIndexAsync(IEnumerable<AuthorizationRequest> requests)
    {
        return GetCluster().Runtime.RefreshAuthorizationIndexAsync(requests);
    }

    public Task UpdatePermissionsAllNamespaceAsync(Type type, Verb verb, string? subresource = null)
    {
        return GetCluster().Runtime.UpdatePermissionsAllNamespaceAsync(type, verb, subresource);
    }

    public Task UpdatePermissionsAllNamespaceAsync<T>(Verb verb, string? subresource = null) where T : class, KubernetesObject, new()
    {
        return GetCluster().Runtime.UpdatePermissionsAllNamespaceAsync<T>(verb, subresource);
    }

    public Task<bool> UpdateCanI(Type type, Verb verb, string? @namespace = null, string? subresource = null)
    {
        return GetCluster().Runtime.UpdateCanI(type, verb, @namespace, subresource);
    }

    public Task<bool> UpdateCanI<T>(Verb verb, string? @namespace = null, string? subresource = null) where T : class, KubernetesObject, new()
    {
        return GetCluster().Runtime.UpdateCanI<T>(verb, @namespace, subresource);
    }

    public Task<bool> UpdateCanIAnyNamespaceAsync(Type type, Verb verb, string? subresource = null)
    {
        return GetCluster().Runtime.UpdateCanIAnyNamespaceAsync(type, verb, subresource);
    }

    public Task<bool> UpdateCanIAnyNamespaceAsync<T>(Verb verb, string? subresource = null) where T : class, KubernetesObject, new()
    {
        return GetCluster().Runtime.UpdateCanIAnyNamespaceAsync<T>(verb, subresource);
    }

    public void QueueResourceConfigPermissionsRefresh(IReadOnlyCollection<IResourceConfig>? resourceConfigs = null)
    {
        if (_suppressPermissionRefresh || _disposed || Cluster == null)
        {
            return;
        }

        var startProcessor = false;

        lock (_resourcePermissionRefreshQueueLock)
        {
            if (resourceConfigs == null)
            {
                _refreshAllResourceConfigPermissionsPending = true;
                _pendingResourcePermissionRefreshKinds.Clear();
            }
            else if (!_refreshAllResourceConfigPermissionsPending)
            {
                foreach (var resourceConfig in resourceConfigs)
                {
                    _pendingResourcePermissionRefreshKinds.Add(resourceConfig.Kind);
                }
            }

            if (!_resourcePermissionRefreshProcessorRunning)
            {
                _resourcePermissionRefreshProcessorRunning = true;
                startProcessor = true;
            }
        }

        if (startProcessor)
        {
            _ = Task.Run(ProcessQueuedResourceConfigPermissionsRefreshAsync);
        }
    }

    public async Task RefreshResourceConfigPermissionsAsync(
        IReadOnlyCollection<IResourceConfig>? resourceConfigs = null,
        Func<Task>? whenNoConfigs = null,
        Func<Task>? afterRefresh = null)
    {
        var cluster = GetCluster();
        var configSnapshot = resourceConfigs?.ToArray() ?? cluster.GetResourceConfigs().ToArray();
        var updateTasks = configSnapshot
            .Select(RefreshResourceConfigPermissionAsync)
            .ToArray();

        if (updateTasks.Length == 0)
        {
            if (whenNoConfigs != null)
            {
                await whenNoConfigs().ConfigureAwait(false);
            }

            NotifyResourcePermissionsChanged(cluster);
            return;
        }

        await RefreshAuthorizationIndexForConfigsAsync(configSnapshot).ConfigureAwait(false);
        await Task.WhenAll(updateTasks).ConfigureAwait(false);

        if (afterRefresh != null)
        {
            await afterRefresh().ConfigureAwait(false);
        }
    }

    public async Task RefreshResourceConfigPermissionAsync(IResourceConfig resourceConfig)
    {
        ArgumentNullException.ThrowIfNull(resourceConfig);

        var cluster = GetCluster();
        var previousIsVisible = resourceConfig.PermissionsLoaded && resourceConfig.CanListAndWatch;

        try
        {
            await resourceConfig.UpdatePermissions().ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Unable to refresh permissions for {Kind}", resourceConfig.Kind);
        }

        var currentIsVisible = resourceConfig.PermissionsLoaded && resourceConfig.CanListAndWatch;
        NotifyResourceConfigPermissionsUpdated(cluster, resourceConfig);

        if (previousIsVisible != currentIsVisible)
        {
            NotifyResourcePermissionsChanged(cluster);
        }
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;

        lock (_resourcePermissionRefreshQueueLock)
        {
            _resourcePermissionRefreshProcessorRunning = false;
            _refreshAllResourceConfigPermissionsPending = false;
            _pendingResourcePermissionRefreshKinds.Clear();
        }

        _resourcePermissionRefreshGate.Dispose();
    }

    private async Task ProcessQueuedResourceConfigPermissionsRefreshAsync()
    {
        while (true)
        {
            IReadOnlyCollection<IResourceConfig>? resourceConfigs;
            var refreshAllConfigs = false;

            lock (_resourcePermissionRefreshQueueLock)
            {
                if (_disposed)
                {
                    _resourcePermissionRefreshProcessorRunning = false;
                    return;
                }

                if (_refreshAllResourceConfigPermissionsPending)
                {
                    refreshAllConfigs = true;
                    _refreshAllResourceConfigPermissionsPending = false;
                    _pendingResourcePermissionRefreshKinds.Clear();
                    resourceConfigs = null;
                }
                else if (_pendingResourcePermissionRefreshKinds.Count > 0)
                {
                    var pendingKinds = _pendingResourcePermissionRefreshKinds.ToHashSet();
                    _pendingResourcePermissionRefreshKinds.Clear();
                    resourceConfigs = GetCluster().GetResourceConfigs()
                        .Where(config => pendingKinds.Contains(config.Kind))
                        .ToArray();
                }
                else
                {
                    _resourcePermissionRefreshProcessorRunning = false;
                    return;
                }
            }

            if (!refreshAllConfigs && resourceConfigs is { Count: 0 })
            {
                continue;
            }

            await _resourcePermissionRefreshGate.WaitAsync().ConfigureAwait(false);

            try
            {
                await RefreshResourceConfigPermissionsAsync(resourceConfigs).ConfigureAwait(false);
            }
            finally
            {
                _resourcePermissionRefreshGate.Release();
            }
        }
    }

    private async Task RefreshAuthorizationIndexForConfigsAsync(IEnumerable<IResourceConfig> resourceConfigs)
    {
        var requests = BuildAuthorizationRequests(resourceConfigs);
        if (requests.Length == 0)
        {
            return;
        }

        await GetCluster().Runtime.RefreshAuthorizationIndexAsync(requests).ConfigureAwait(false);
    }

    private static AuthorizationRequest[] BuildAuthorizationRequests(IEnumerable<IResourceConfig> resourceConfigs)
    {
        return resourceConfigs
            .SelectMany(static config => config.AuthorizationRequests())
            .Distinct()
            .ToArray();
    }

    private ClusterWorkspaceViewModel GetCluster()
    {
        return Cluster ?? throw new InvalidOperationException("Permission cache has not been initialized with a cluster.");
    }

    private void NotifyResourcePermissionsChanged(ClusterWorkspaceViewModel cluster)
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            _ = Dispatcher.UIThread.InvokeAsync(() => NotifyResourcePermissionsChanged(cluster));
            return;
        }

        ResourcePermissionsChanged?.Invoke(cluster);
    }

    private void NotifyResourceConfigPermissionsUpdated(ClusterWorkspaceViewModel cluster, IResourceConfig resourceConfig)
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            _ = Dispatcher.UIThread.InvokeAsync(() => NotifyResourceConfigPermissionsUpdated(cluster, resourceConfig));
            return;
        }

        ResourceConfigPermissionsUpdated?.Invoke(cluster, resourceConfig);
    }
}
