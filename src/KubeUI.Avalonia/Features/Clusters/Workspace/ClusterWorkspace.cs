using System.Collections.Concurrent;
using System.Diagnostics;
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
    private readonly ConcurrentDictionary<string, long> _customResourceDefinitionGenerations = new(StringComparer.Ordinal);
    private readonly CancellationTokenSource _disposeCancellation = new();
    private readonly object _connectLock = new();
    private bool _disposed;
    private bool _workspaceStateInitialized;
    private Task? _connectTask;

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
        UpdateClusterColor();
    }

    [ObservableProperty]
    public partial IClusterRuntime Runtime { get; set; }

    public string Title => Runtime.Name;

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

    private async Task UpdateResourceConfigsPermissionsAndEvaluateAsync(
        IReadOnlyCollection<IResourceConfig>? resourceConfigs = null)
    {
        using var activity = StartWorkspaceActivity(nameof(UpdateResourceConfigsPermissionsAndEvaluateAsync));

        var categoryBatches = (resourceConfigs ?? GetResourceConfigs())
            .GroupBy(static config => config.Category, StringComparer.Ordinal)
            .OrderBy(static category => ResourceCategories.GetOrder(category.Key, category.Min(config => config.Order)))
            .ThenBy(static category => category.Key, StringComparer.Ordinal);

        foreach (var categoryBatch in categoryBatches)
        {
            foreach (var orderBatch in categoryBatch.GroupBy(static config => config.Order).OrderBy(static batch => batch.Key))
            {
                await Parallel.ForEachAsync(
                    orderBatch,
                    new ParallelOptions { MaxDegreeOfParallelism = 4 },
                    async (resourceConfig, _) => await UpdateResourceConfigPermissionsAndEvaluateAsync(resourceConfig).ConfigureAwait(false)).ConfigureAwait(false);
            }
        }
    }

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

    private async Task UpdateResourceConfigPermissionsAndEvaluateAsync(
        IResourceConfig resourceConfig,
        bool notifyProcessed = true)
    {
        await UpdateResourceConfigPermissionsAsync(resourceConfig).ConfigureAwait(false);
        await EvaluateResourceConfigAccessCoreAsync(resourceConfig, notifyProcessed).ConfigureAwait(false);
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
        _resourceConfigs[resourceConfig.Kind] = resourceConfig;

        ProcessResourceConfigPermissionsUpdated(resourceConfig);
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        Runtime.OnChange -= OnRuntimeChange;
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

    private async Task EvaluateResourceConfigAccessCoreAsync(
        IResourceConfig resourceConfig,
        bool notifyProcessed = true)
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

        if (notifyProcessed)
        {
            ProcessResourceConfigPermissionsUpdated(resourceConfig);
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

    private void ProcessResourceConfigPermissionsUpdated(IResourceConfig resourceConfig)
    {
        using var activity = StartWorkspaceActivity(nameof(ProcessResourceConfigPermissionsUpdated));

        ResourceConfigProcessed?.Invoke(this, resourceConfig);
    }

    private void SubscribeRuntime()
    {
        Runtime.OnChange += OnRuntimeChange;
        if (Runtime is INotifyPropertyChanged runtime)
        {
            runtime.PropertyChanged += OnRuntimePropertyChanged;
        }
    }

    private void OnRuntimePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IClusterRuntime.Status))
        {
            Dispatcher.UIThread.Post(UpdateClusterColor);
        }
    }

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
                NotifyCustomResourceDefinitionRemoved(removedKind);
            }
        }
    }

    private void OnRuntimeChange(WatchEventType eventType, GroupApiVersionKind resourceKind, IKubernetesObject<V1ObjectMeta> item)
    {
        if (item is not V1CustomResourceDefinition crd)
        {
            return;
        }

        if (!crd.TryGetResourceKind(out var kind))
        {
            return;
        }

        var definitionName = crd.Name() ?? string.Empty;
        var generation = _customResourceDefinitionGenerations.AddOrUpdate(
            definitionName,
            1,
            static (_, current) => current + 1);

        if (eventType == WatchEventType.Deleted)
        {
            RemoveCustomResourceDefinition(kind);
        }
        else
        {
            _ = ProcessCustomResourceDefinitionAsync(crd, definitionName, generation);
        }
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
        long generation)
    {
        using var activity = StartWorkspaceActivity(nameof(ProcessCustomResourceDefinitionAsync));
        activity?.SetTag("kubernetes.crd.name", crd.Name());

        try
        {
            var resourceConfig = _serviceProvider.GetRequiredService<CRDResourceConfig>();
            resourceConfig.Initialize(this);
            resourceConfig.Configure(crd);

            _logger.LogDebug(
                "Custom resource definition discovered for {ClusterName}: Definition={DefinitionName}; ResourceKind={ResourceKind}; Generation={Generation}",
                Runtime.Name,
                definitionName,
                resourceConfig.Kind,
                generation);

            await UpdateResourceConfigPermissionsAndEvaluateAsync(resourceConfig, notifyProcessed: false).ConfigureAwait(false);

            _logger.LogDebug(
                "Custom resource definition access evaluated for {ClusterName}: Definition={DefinitionName}; ResourceKind={ResourceKind}; PermissionsLoaded={PermissionsLoaded}; CanListAndWatch={CanListAndWatch}; Generation={Generation}",
                Runtime.Name,
                definitionName,
                resourceConfig.Kind,
                resourceConfig.PermissionsLoaded,
                resourceConfig.CanListAndWatch,
                generation);

            if (!resourceConfig.CanListAndWatch)
            {
                _logger.LogDebug(
                    "Custom resource definition skipped for {ClusterName}: list/watch access denied; Definition={DefinitionName}; ResourceKind={ResourceKind}",
                    Runtime.Name,
                    definitionName,
                    resourceConfig.Kind);
                return;
            }

            if (!_customResourceDefinitionGenerations.TryGetValue(definitionName, out var currentGeneration)
                || currentGeneration != generation)
            {
                _logger.LogDebug(
                    "Custom resource definition skipped for {ClusterName}: stale generation; Definition={DefinitionName}; ResourceKind={ResourceKind}; ExpectedGeneration={CurrentGeneration}; Generation={Generation}",
                    Runtime.Name,
                    definitionName,
                    resourceConfig.Kind,
                    currentGeneration,
                    generation);
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
            ProcessResourceConfigPermissionsUpdated(resourceConfig);
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
