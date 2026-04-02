using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using Avalonia.Media.Immutable;
using Avalonia.Threading;
using DynamicData;
using k8s;
using k8s.KubeConfigModels;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Features.Clusters.Workspace.ViewModels;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Avalonia.Resources;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.Clusters.Workspace.ViewModels;

public sealed partial class ClusterWorkspaceViewModel : ViewModelBase, IClusterRuntime, IDisposable
{
    private const int CustomResourceConfigBatchSize = 25;
    private static readonly TimeSpan CustomResourceConfigBatchWindow = TimeSpan.FromMilliseconds(200);

    private readonly IServiceProvider _serviceProvider;
    private readonly IClusterSettingsStore _clusterSettingsStore;
    private readonly ILogger<ClusterWorkspaceViewModel> _logger;
    private readonly ConcurrentDictionary<GroupApiVersionKind, IResourceConfig> _resourceConfigs = new();
    private readonly ConcurrentDictionary<string, PendingCustomResourceDefinitionChange> _pendingCustomResourceDefinitions = new(StringComparer.Ordinal);
    private readonly SemaphoreSlim _pendingCustomResourceDefinitionSignal = new(0);
    private readonly SemaphoreSlim _workspaceStateRefreshGate = new(1, 1);
    private readonly SemaphoreSlim _resourcePermissionRefreshGate = new(1, 1);
    private readonly CancellationTokenSource _disposeCancellation = new();
    private readonly Task _pendingCustomResourceDefinitionTask;
    private INotifyCollectionChanged? _runtimeNamespacesCollection;
    private bool _suppressPermissionRefresh;
    private bool _disposed;
    private bool _workspaceStateInitialized;

    public ClusterWorkspaceViewModel(
        IClusterRuntime runtime,
        IServiceProvider serviceProvider,
        IClusterSettingsStore clusterSettingsStore,
        ILogger<ClusterWorkspaceViewModel> logger)
    {
        Runtime = runtime;
        _serviceProvider = serviceProvider;
        _clusterSettingsStore = clusterSettingsStore;
        _logger = logger;

        Title = runtime.Name;
        Id = $"cluster-workspace-{runtime.Name}";
        BindRuntimeCollections();

        SubscribeRuntime();
        SubscribeNamespaceCollection(Runtime.Namespaces);
        UpdateClusterColor();
        _pendingCustomResourceDefinitionTask = Task.Run(ProcessPendingCustomResourceDefinitionsAsync);
    }

    [ObservableProperty]
    public partial IClusterRuntime Runtime { get; set; }

    [ObservableProperty]
    public partial bool IsExpanded { get; set; }

    [ObservableProperty]
    public partial IBrush ClusterColor { get; set; } = Brushes.Red;

    [ObservableProperty]
    public partial ObservableCollection<V1Namespace> SelectedNamespaces { get; set; } = [];

    [ObservableProperty]
    public partial ObservableCollection<NodeMetrics> NodeMetrics { get; set; } = [];

    [ObservableProperty]
    public partial ObservableCollection<PodMetrics> PodMetrics { get; set; } = [];

    [ObservableProperty]
    public partial ObservableCollection<PortForwarder> PortForwarders { get; set; } = [];

    [ObservableProperty]
    public partial int ResourceConfigVersion { get; set; }

    public IReadOnlyDictionary<GroupApiVersionKind, object> Objects => Runtime.Objects;
    public event Action<WatchEventType, GroupApiVersionKind, IKubernetesObject<V1ObjectMeta>>? OnChange;
    public event Action<V1CustomResourceDefinition>? OnCustomResourceDefinitionReady
    {
        add => Runtime.OnCustomResourceDefinitionReady += value;
        remove => Runtime.OnCustomResourceDefinitionReady -= value;
    }
    public event Action<ClusterWorkspaceViewModel>? ResourcePermissionsChanged;
    public event Action<ClusterWorkspaceViewModel, IResourceConfig>? ResourceConfigPermissionsUpdated;
    public event Action<ClusterWorkspaceViewModel, IReadOnlyList<PendingCustomResourceConfig>, IReadOnlyList<GroupApiVersionKind>>? CustomResourceDefinitionsChanged;

    public bool Connected
    {
        get => Runtime.Connected;
        set => Runtime.Connected = value;
    }

    public ClusterStatus Status
    {
        get => Runtime.Status;
        set => Runtime.Status = value;
    }

    public string? LastError
    {
        get => Runtime.LastError;
        set => Runtime.LastError = value;
    }

    public bool RequiresNamespaceSelectionPrompt
    {
        get => Runtime.RequiresNamespaceSelectionPrompt;
        set => Runtime.RequiresNamespaceSelectionPrompt = value;
    }

    public bool IsMetricsAvailable => Runtime.IsMetricsAvailable;

    public bool ListNamespaces
    {
        get => Runtime.ListNamespaces;
        set
        {
            Runtime.ListNamespaces = value;
            OnPropertyChanged();
        }
    }

    public IKubernetes? Client
    {
        get => Runtime.Client;
        set => Runtime.Client = value;
    }

    public K8SConfiguration KubeConfig
    {
        get => Runtime.KubeConfig;
        set => Runtime.KubeConfig = value;
    }

    public ModelCache ModelCache
    {
        get => Runtime.ModelCache;
        set => Runtime.ModelCache = value;
    }

    public string KubeConfigPath
    {
        get => Runtime.KubeConfigPath;
        set => Runtime.KubeConfigPath = value;
    }

    public string Name
    {
        get => Runtime.Name;
        set => Runtime.Name = value;
    }

    public ReadOnlyObservableCollection<V1Namespace> Namespaces => Runtime.Namespaces;

    ObservableCollection<NodeMetrics> IClusterRuntime.NodeMetrics => Runtime.NodeMetrics;
    ObservableCollection<PodMetrics> IClusterRuntime.PodMetrics => Runtime.PodMetrics;
    ObservableCollection<PortForwarder> IClusterRuntime.PortForwarders => Runtime.PortForwarders;
    ReadOnlyObservableCollection<V1Namespace> IClusterRuntime.Namespaces => Runtime.Namespaces;

    public async Task Connect()
    {
        await Runtime.Connect();
        if (!Runtime.Connected)
        {
            return;
        }

        await EnsureWorkspaceStateInitializedAsync().ConfigureAwait(false);
    }

    public async Task Disconnect()
    {
        _suppressPermissionRefresh = true;

        try
        {
            await Runtime.Disconnect().ConfigureAwait(false);
            ResetWorkspaceState();
        }
        finally
        {
            _suppressPermissionRefresh = false;
        }
    }

    public Task EnsureWorkspaceStateInitializedAsync()
    {
        return RefreshWorkspaceStateAsync();
    }

    public Task AddOrUpdateResource<T>(T item) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        return Runtime.AddOrUpdateResource(item);
    }

    public Task DeleteResource<T>(T item) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        return Runtime.DeleteResource(item);
    }

    public Task ImportFolder(string path)
    {
        return Runtime.ImportFolder(path);
    }

    public Task ImportYaml(Stream stream)
    {
        return Runtime.ImportYaml(stream);
    }

    public Task DryRunYaml(Stream stream)
    {
        return Runtime.DryRunYaml(stream);
    }

    public Task SeedResource<T>(bool waitForReady = false) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        return SeedResourceCoreAsync<T>(waitForReady);
    }

    public Task<bool> IsResourceReady<T>(CancellationToken? token = null) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        return Runtime.IsResourceReady<T>(token);
    }

    public T? GetResource<T>(string? @namespace, string name) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        return Runtime.GetResource<T>(@namespace, name);
    }

    public IReadOnlyList<T> GetResourceList<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        return Runtime.GetResourceList<T>();
    }

    public ISourceCache<T, string> GetResourceSourceCache<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        return Runtime.GetResourceSourceCache<T>();
    }

    public IObservable<int> GetResourceCount(Type type)
    {
        return Runtime.GetResourceCount(type);
    }

    public IObservable<int> GetResourceCount<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        return Runtime.GetResourceCount<T>();
    }

    public bool CanI(Type type, Verb verb, string? @namespace = null, string? subresource = null)
    {
        return Runtime.CanI(type, verb, @namespace, subresource);
    }

    public bool CanI<T>(Verb verb, string? @namespace = null, string? subresource = null) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        return Runtime.CanI<T>(verb, @namespace, subresource);
    }

    public bool CanIAnyNamespace(Type type, Verb verb, string? subresource = null)
    {
        return Runtime.CanIAnyNamespace(type, verb, subresource);
    }

    public bool CanIAnyNamespace<T>(Verb verb, string? subresource = null) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        return Runtime.CanIAnyNamespace<T>(verb, subresource);
    }

    public bool IsResourceNamespaced(Type type)
    {
        return Runtime.IsResourceNamespaced(type);
    }

    public bool IsResourceNamespaced<T>()
    {
        return Runtime.IsResourceNamespaced<T>();
    }

    public bool CanReadEvents(IKubernetesObject<V1ObjectMeta> resource)
    {
        ArgumentNullException.ThrowIfNull(resource);

        if (!Runtime.Connected)
        {
            return false;
        }

        if (CanReadEventsInNamespace(null))
        {
            return true;
        }

        var @namespace = resource.Metadata?.NamespaceProperty;
        return !string.IsNullOrWhiteSpace(@namespace) && CanReadEventsInNamespace(@namespace);
    }

    public PortForwarder AddPodPortForward(string @namespace, string podName, int containerPort)
    {
        return Runtime.AddPodPortForward(@namespace, podName, containerPort);
    }

    public PortForwarder AddServicePortForward(string @namespace, string serviceName, int servicePort)
    {
        return Runtime.AddServicePortForward(@namespace, serviceName, servicePort);
    }

    public void RemovePortForward(PortForwarder pf)
    {
        Runtime.RemovePortForward(pf);
    }

    public Task UpdatePermissionsAllNamespaceAsync(Type type, Verb verb, string? subresource = null)
    {
        return Runtime.UpdatePermissionsAllNamespaceAsync(type, verb, subresource);
    }

    public Task UpdatePermissionsAllNamespaceAsync<T>(Verb verb, string? subresource = null) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        return Runtime.UpdatePermissionsAllNamespaceAsync<T>(verb, subresource);
    }

    public Task<bool> UpdateCanI(Type type, Verb verb, string? @namespace = null, string? subresource = null)
    {
        return Runtime.UpdateCanI(type, verb, @namespace, subresource);
    }

    public Task<bool> UpdateCanI<T>(Verb verb, string? @namespace = null, string? subresource = null) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        return Runtime.UpdateCanI<T>(verb, @namespace, subresource);
    }

    public Task<bool> UpdateCanIAnyNamespaceAsync(Type type, Verb verb, string? subresource = null)
    {
        return Runtime.UpdateCanIAnyNamespaceAsync(type, verb, subresource);
    }

    public Task<bool> UpdateCanIAnyNamespaceAsync<T>(Verb verb, string? subresource = null) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        return Runtime.UpdateCanIAnyNamespaceAsync<T>(verb, subresource);
    }

    public IResourceConfig GetResourceConfig(GroupApiVersionKind kind)
    {
        return _resourceConfigs[kind];
    }

    public IResourceConfig GetResourceConfig<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        return GetResourceConfig(GroupApiVersionKind.From<T>());
    }

    public IEnumerable<IResourceConfig> GetResourceConfigs()
    {
        return _resourceConfigs.Values.ToList();
    }

    internal void AddResourceConfigForTest(IResourceConfig resourceConfig)
    {
        _resourceConfigs[resourceConfig.Kind] = resourceConfig;
        if (resourceConfig.IsCustomResource)
        {
            NotifyCustomResourceDefinitionsChanged([new PendingCustomResourceConfig(resourceConfig.Kind, resourceConfig)], []);
            return;
        }

        NotifyResourceConfigPermissionsUpdated(resourceConfig);
        NotifyResourcePermissionsChanged();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        Runtime.OnChange -= OnRuntimeChange;
        Runtime.OnCustomResourceDefinitionReady -= HandleCustomResourceDefinitionReady;
        UnsubscribeNamespaceCollection();

        if (Runtime is INotifyPropertyChanged propertyChanged)
        {
            propertyChanged.PropertyChanged -= OnRuntimePropertyChanged;
        }

        _disposeCancellation.Cancel();
        _pendingCustomResourceDefinitions.Clear();
        _pendingCustomResourceDefinitionSignal.Release();

        try
        {
            _pendingCustomResourceDefinitionTask.GetAwaiter().GetResult();
        }
        catch (OperationCanceledException)
        {
        }

        _disposeCancellation.Dispose();
        _pendingCustomResourceDefinitionSignal.Dispose();
    }

    private async Task RefreshWorkspaceStateAsync()
    {
        await _workspaceStateRefreshGate.WaitAsync().ConfigureAwait(false);

        try
        {
            if (_workspaceStateInitialized)
            {
                return;
            }

            await EnsureBuiltInResourceConfigsAsync();
            await EnsureConfiguredNamespacesAvailableAsync().ConfigureAwait(false);
            await EnsureDynamicResourceConfigsAsync().ConfigureAwait(false);
            await RefreshResourceConfigPermissionsAsync().ConfigureAwait(false);
            _workspaceStateInitialized = true;
            UpdateClusterColor();
            NotifyRuntimeStateChanged();
        }
        finally
        {
            _workspaceStateRefreshGate.Release();
        }
    }

    private void NotifyRuntimeStateChanged()
    {
        OnPropertyChanged(nameof(Connected));
        OnPropertyChanged(nameof(Status));
        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(ListNamespaces));
        OnPropertyChanged(nameof(Namespaces));
        OnPropertyChanged(nameof(IsMetricsAvailable));
    }

    private void BindRuntimeCollections()
    {
        NodeMetrics = Runtime.NodeMetrics;
        PodMetrics = Runtime.PodMetrics;
        PortForwarders = Runtime.PortForwarders;
    }

    private async Task EnsureBuiltInResourceConfigsAsync()
    {
        if (_resourceConfigs.Count == 0)
        {
            var serviceDescriptors = _serviceProvider.GetRequiredService<ServiceDescriptor[]>();
            var types = serviceDescriptors
                .Select(x => x.ServiceType)
                .Where(t => !t.IsAbstract
                    && !t.ContainsGenericParameters
                    && typeof(IResourceConfig).IsAssignableFrom(t))
                .Distinct()
                .ToList();

            foreach (var type in types)
            {
                var resourceConfig = (IResourceConfig)_serviceProvider.GetRequiredService(type);
                resourceConfig.Initialize(this);
                _resourceConfigs[resourceConfig.Kind] = resourceConfig;
            }
        }
    }

    private async Task EnsureConfiguredNamespacesAvailableAsync()
    {
        if (Runtime.ListNamespaces)
        {
            return;
        }

        var configuredNamespaces = _clusterSettingsStore.GetClusterNamespaces(Runtime);
        if (configuredNamespaces.Count == 0)
        {
            return;
        }

        var existingNamespaces = new HashSet<string>(
            Runtime.Namespaces
                .Select(x => x.Name())
                .Where(x => !string.IsNullOrWhiteSpace(x))!,
            StringComparer.Ordinal);

        foreach (var configuredNamespace in configuredNamespaces)
        {
            if (string.IsNullOrWhiteSpace(configuredNamespace) || !existingNamespaces.Add(configuredNamespace))
            {
                continue;
            }

            await Runtime.AddOrUpdateResource(new V1Namespace
            {
                Metadata = new V1ObjectMeta
                {
                    Name = configuredNamespace
                }
            }).ConfigureAwait(false);
        }
    }

    private async Task SeedResourceCoreAsync<T>(bool waitForReady) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        if (Runtime.IsResourceNamespaced<T>())
        {
            await EnsureConfiguredNamespacesAvailableAsync().ConfigureAwait(false);
        }

        await Runtime.SeedResource<T>(waitForReady).ConfigureAwait(false);
    }

    private void QueueResourceConfigPermissionsRefresh()
    {
        if (_suppressPermissionRefresh || _disposed)
        {
            return;
        }

        _ = Task.Run(RefreshResourceConfigPermissionsQueuedAsync);
    }

    private async Task RefreshResourceConfigPermissionsQueuedAsync()
    {
        await _resourcePermissionRefreshGate.WaitAsync().ConfigureAwait(false);

        try
        {
            await RefreshResourceConfigPermissionsAsync().ConfigureAwait(false);
        }
        finally
        {
            _resourcePermissionRefreshGate.Release();
        }
    }

    private async Task EnsureDynamicResourceConfigsAsync()
    {
        if (!Runtime.Objects.ContainsKey(GroupApiVersionKind.From<V1CustomResourceDefinition>()))
        {
            return;
        }

        foreach (var crd in Runtime.GetResourceList<V1CustomResourceDefinition>())
        {
            QueueCustomResourceDefinitionChange(crd, CustomResourceDefinitionChangeKind.Upsert);
        }
    }

    private async Task<PendingCustomResourceConfig?> BuildCustomResourceConfigAsync(V1CustomResourceDefinition crd)
    {
        var version = crd.Spec?.Versions?.FirstOrDefault(x => x.Served && x.Storage);
        if (version == null)
        {
            return null;
        }

        var resourceType = Runtime.ModelCache.GetResourceType(crd.Spec.Group, version.Name, crd.Spec.Names.Kind);
        if (resourceType == null)
        {
            return null;
        }

        if (!await Runtime.UpdateCanIAnyNamespaceAsync(resourceType, Verb.List).ConfigureAwait(false)
            || !await Runtime.UpdateCanIAnyNamespaceAsync(resourceType, Verb.Watch).ConfigureAwait(false))
        {
            return null;
        }

        var api = GroupApiVersionKind.From(resourceType);
        var resourceConfigType = typeof(CRDResourceConfig<>).MakeGenericType(resourceType);
        var resourceConfig = (IResourceConfig)_serviceProvider.GetRequiredService(resourceConfigType);

        resourceConfig.Initialize(this);
        await resourceConfig.UpdatePermissions().ConfigureAwait(false);
        ((dynamic)resourceConfig).Generate(crd);

        return new PendingCustomResourceConfig(api, resourceConfig);
    }

    private async Task RefreshResourceConfigPermissionsAsync()
    {
        var updateTasks = _resourceConfigs.Values
            .Select(RefreshResourceConfigPermissionAsync)
            .ToArray();

        if (updateTasks.Length == 0)
        {
            await EnsureEventResourceSeededAsync().ConfigureAwait(false);
            NotifyResourcePermissionsChanged();
            return;
        }

        await Task.WhenAll(updateTasks).ConfigureAwait(false);
        await EnsureEventResourceSeededAsync().ConfigureAwait(false);
    }

    private bool CanReadEventsInNamespace(string? @namespace)
    {
        return Runtime.CanI<Corev1Event>(Verb.List, @namespace)
            && Runtime.CanI<Corev1Event>(Verb.Watch, @namespace);
    }

    private async Task EnsureEventResourceSeededAsync()
    {
        if (Runtime.Objects.ContainsKey(GroupApiVersionKind.From<Corev1Event>()))
        {
            return;
        }

        if (await CanSeedEventsAsync().ConfigureAwait(false))
        {
            await SeedResource<Corev1Event>().ConfigureAwait(false);
        }
    }

    private async Task<bool> CanSeedEventsAsync()
    {
        if (!Runtime.Connected)
        {
            return false;
        }

        if (await Runtime.UpdateCanI<Corev1Event>(Verb.List).ConfigureAwait(false)
            && await Runtime.UpdateCanI<Corev1Event>(Verb.Watch).ConfigureAwait(false))
        {
            return true;
        }

        foreach (var item in Runtime.Namespaces)
        {
            var @namespace = item.Name();
            if (string.IsNullOrWhiteSpace(@namespace))
            {
                continue;
            }

            if (await Runtime.UpdateCanI<Corev1Event>(Verb.List, @namespace).ConfigureAwait(false)
                && await Runtime.UpdateCanI<Corev1Event>(Verb.Watch, @namespace).ConfigureAwait(false))
            {
                return true;
            }
        }

        return false;
    }

    private void SubscribeNamespaceCollection(ReadOnlyObservableCollection<V1Namespace>? namespaces)
    {
        if (namespaces is not INotifyCollectionChanged collection || ReferenceEquals(_runtimeNamespacesCollection, collection))
        {
            return;
        }

        UnsubscribeNamespaceCollection();
        _runtimeNamespacesCollection = collection;
        _runtimeNamespacesCollection.CollectionChanged += OnRuntimeNamespacesCollectionChanged;
    }

    private void UnsubscribeNamespaceCollection()
    {
        if (_runtimeNamespacesCollection == null)
        {
            return;
        }

        _runtimeNamespacesCollection.CollectionChanged -= OnRuntimeNamespacesCollectionChanged;
        _runtimeNamespacesCollection = null;
    }

    private void OnRuntimeNamespacesCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        QueueResourceConfigPermissionsRefresh();
    }

    private async Task RefreshResourceConfigPermissionAsync(IResourceConfig resourceConfig)
    {
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
        NotifyResourceConfigPermissionsUpdated(resourceConfig);

        if (previousIsVisible != currentIsVisible)
        {
            NotifyResourcePermissionsChanged();
        }
    }

    private void SubscribeRuntime()
    {
        Runtime.OnChange += OnRuntimeChange;
        Runtime.OnCustomResourceDefinitionReady += HandleCustomResourceDefinitionReady;

        if (Runtime is INotifyPropertyChanged propertyChanged)
        {
            propertyChanged.PropertyChanged += OnRuntimePropertyChanged;
        }
    }

    private void OnRuntimePropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        switch (e.PropertyName)
        {
            case nameof(IClusterRuntime.Connected):
                _workspaceStateInitialized = false;

                if (!Runtime.Connected)
                {
                    ResetWorkspaceState();
                }

                if (Runtime.Connected)
                {
                    QueueWorkspaceStateRefresh();
                }

                PostToUiThread(() => OnPropertyChanged(nameof(Connected)));
                break;
            case nameof(IClusterRuntime.Status):
                if (Runtime.Status == ClusterStatus.Connecting)
                {
                    _workspaceStateInitialized = false;
                }

                PostToUiThread(() =>
                {
                    UpdateClusterColor();
                    OnPropertyChanged(nameof(Status));
                    OnPropertyChanged(nameof(LastError));
                    OnPropertyChanged(nameof(RequiresNamespaceSelectionPrompt));
                    OnPropertyChanged(nameof(IsMetricsAvailable));
                });
                break;
            case nameof(IClusterRuntime.LastError):
                PostToUiThread(() => OnPropertyChanged(nameof(LastError)));
                break;
            case nameof(IClusterRuntime.RequiresNamespaceSelectionPrompt):
                PostToUiThread(() => OnPropertyChanged(nameof(RequiresNamespaceSelectionPrompt)));
                break;
            case nameof(IClusterRuntime.Name):
                PostToUiThread(() =>
                {
                    Title = Runtime.Name;
                    OnPropertyChanged(nameof(Name));
                });
                break;
            case nameof(IClusterRuntime.ListNamespaces):
                PostToUiThread(() => OnPropertyChanged(nameof(ListNamespaces)));
                break;
            case nameof(IClusterRuntime.Namespaces):
                PostToUiThread(() =>
                {
                    SubscribeNamespaceCollection(Runtime.Namespaces);
                    OnPropertyChanged(nameof(Namespaces));
                });
                break;
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
            _resourceConfigs.TryRemove(removedKind, out _);
        }

        NotifyCustomResourceDefinitionsChanged([], removedCustomResourceKinds);
    }

    private void QueueWorkspaceStateRefresh()
    {
        _ = Task.Run(async () =>
        {
            try
            {
                await RefreshWorkspaceStateAsync().ConfigureAwait(false);
            }
            catch (OperationCanceledException) when (_disposeCancellation.IsCancellationRequested)
            {
            }
            catch (ObjectDisposedException) when (_disposed)
            {
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error refreshing workspace state for cluster {Cluster}", Runtime.Name);
            }
        });
    }

    private static void PostToUiThread(Action action)
    {
        if (Dispatcher.UIThread.CheckAccess())
        {
            action();
            return;
        }

        Dispatcher.UIThread.Post(action);
    }

    private void OnRuntimeChange(WatchEventType eventType, GroupApiVersionKind kind, IKubernetesObject<V1ObjectMeta> item)
    {
        OnChange?.Invoke(eventType, kind, item);

        if (item is not V1CustomResourceDefinition crd)
        {
            return;
        }

        if (eventType == WatchEventType.Deleted)
        {
            QueueCustomResourceDefinitionChange(crd, CustomResourceDefinitionChangeKind.Delete);
            return;
        }
    }

    private void HandleCustomResourceDefinitionReady(V1CustomResourceDefinition crd)
    {
        QueueCustomResourceDefinitionChange(crd, CustomResourceDefinitionChangeKind.Upsert);
    }

    private void QueueCustomResourceDefinitionChange(V1CustomResourceDefinition crd, CustomResourceDefinitionChangeKind kind)
    {
        if (_disposed)
        {
            return;
        }

        _pendingCustomResourceDefinitions[crd.Name()] = new PendingCustomResourceDefinitionChange(kind, crd);
        _pendingCustomResourceDefinitionSignal.Release();
    }

    private async Task ProcessPendingCustomResourceDefinitionsAsync()
    {
        try
        {
            while (!_disposeCancellation.IsCancellationRequested)
            {
                await _pendingCustomResourceDefinitionSignal.WaitAsync(_disposeCancellation.Token).ConfigureAwait(false);

                if (_disposeCancellation.IsCancellationRequested)
                {
                    return;
                }

                var changes = await DrainPendingCustomResourceDefinitionChangesAsync().ConfigureAwait(false);
                if (changes.Count == 0)
                {
                    continue;
                }

                var builtConfigs = new List<PendingCustomResourceConfig>(changes.Count);
                var removedKinds = new List<GroupApiVersionKind>();

                foreach (var change in changes)
                {
                    try
                    {
                        if (change.Kind == CustomResourceDefinitionChangeKind.Delete)
                        {
                            var removedKind = TryResolveCustomResourceKind(change.Crd);
                            if (removedKind != null)
                            {
                                removedKinds.Add(removedKind.Value);
                            }

                            continue;
                        }

                        var builtConfig = await BuildCustomResourceConfigAsync(change.Crd).ConfigureAwait(false);
                        if (builtConfig != null)
                        {
                            builtConfigs.Add(builtConfig);
                            continue;
                        }

                        var unresolvedKind = TryResolveCustomResourceKind(change.Crd);
                        if (unresolvedKind != null)
                        {
                            removedKinds.Add(unresolvedKind.Value);
                        }
                    }
                    catch (Exception ex) when (ex is not OperationCanceledException)
                    {
                        _logger.LogError(ex, "Error processing queued custom resource definition {Crd}", change.Crd.Name());
                    }
                }

                if (builtConfigs.Count == 0 && removedKinds.Count == 0)
                {
                    continue;
                }

                await Dispatcher.UIThread.InvokeAsync(() =>
                {
                    foreach (var removedKind in removedKinds)
                    {
                        _resourceConfigs.TryRemove(removedKind, out _);
                    }

                    foreach (var builtConfig in builtConfigs)
                    {
                        _resourceConfigs[builtConfig.Kind] = builtConfig.ResourceConfig;
                    }

                    NotifyCustomResourceDefinitionsChanged(builtConfigs, removedKinds);
                });
            }
        }
        catch (OperationCanceledException) when (_disposeCancellation.IsCancellationRequested)
        {
        }
    }

    private async Task<List<PendingCustomResourceDefinitionChange>> DrainPendingCustomResourceDefinitionChangesAsync()
    {
        var changes = new List<PendingCustomResourceDefinitionChange>(CustomResourceConfigBatchSize);
        CollectPendingCustomResourceDefinitionChanges(changes);

        var deadline = DateTime.UtcNow + CustomResourceConfigBatchWindow;
        while (changes.Count < CustomResourceConfigBatchSize)
        {
            var remaining = deadline - DateTime.UtcNow;
            if (remaining <= TimeSpan.Zero)
            {
                break;
            }

            if (!await _pendingCustomResourceDefinitionSignal.WaitAsync(remaining, _disposeCancellation.Token).ConfigureAwait(false))
            {
                break;
            }

            CollectPendingCustomResourceDefinitionChanges(changes);
        }

        return changes;
    }

    private void CollectPendingCustomResourceDefinitionChanges(List<PendingCustomResourceDefinitionChange> changes)
    {
        foreach (var pending in _pendingCustomResourceDefinitions)
        {
            if (changes.Count >= CustomResourceConfigBatchSize)
            {
                break;
            }

            if (_pendingCustomResourceDefinitions.TryRemove(pending.Key, out var change))
            {
                changes.Add(change);
            }
        }
    }

    private GroupApiVersionKind? TryResolveCustomResourceKind(V1CustomResourceDefinition crd)
    {
        var version = crd.Spec?.Versions?.FirstOrDefault(x => x.Served && x.Storage);
        if (version == null)
        {
            return null;
        }

        var resourceType = Runtime.ModelCache.GetResourceType(crd.Spec.Group, version.Name, crd.Spec.Names.Kind);
        if (resourceType != null)
        {
            return GroupApiVersionKind.From(resourceType);
        }

        foreach (var resourceKind in _resourceConfigs.Keys)
        {
            if (string.Equals(resourceKind.Group, crd.Spec.Group, StringComparison.Ordinal)
                && string.Equals(resourceKind.ApiVersion, version.Name, StringComparison.Ordinal)
                && string.Equals(resourceKind.Kind, crd.Spec.Names.Kind, StringComparison.Ordinal))
            {
                return resourceKind;
            }
        }

        return null;
    }

    private void NotifyResourcePermissionsChanged()
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            _ = Dispatcher.UIThread.InvokeAsync(NotifyResourcePermissionsChanged);
            return;
        }

        ResourceConfigVersion++;
        ResourcePermissionsChanged?.Invoke(this);
    }

    private void NotifyResourceConfigPermissionsUpdated(IResourceConfig resourceConfig)
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            _ = Dispatcher.UIThread.InvokeAsync(() => NotifyResourceConfigPermissionsUpdated(resourceConfig));
            return;
        }

        ResourceConfigPermissionsUpdated?.Invoke(this, resourceConfig);
    }

    private void NotifyCustomResourceDefinitionsChanged(IReadOnlyList<PendingCustomResourceConfig> addedConfigs, IReadOnlyList<GroupApiVersionKind> removedKinds)
    {
        if (!Dispatcher.UIThread.CheckAccess())
        {
            _ = Dispatcher.UIThread.InvokeAsync(() => NotifyCustomResourceDefinitionsChanged(addedConfigs, removedKinds));
            return;
        }

        ResourceConfigVersion++;
        CustomResourceDefinitionsChanged?.Invoke(this, addedConfigs, removedKinds);
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
        if (ClusterColor != Brushes.Red && ClusterColor != Brushes.Orange)
        {
            return ClusterColor;
        }

        var properties = typeof(Brushes)
            .GetProperties(BindingFlags.Public | BindingFlags.Static)
            .Where(x => x.Name != nameof(Brushes.Red) && x.Name != nameof(Brushes.Orange))
            .ToArray();

        return (IBrush)properties[Random.Shared.Next(properties.Length)].GetValue(null)!;
    }
}

internal enum CustomResourceDefinitionChangeKind
{
    Upsert,
    Delete,
}

internal sealed record PendingCustomResourceDefinitionChange(CustomResourceDefinitionChangeKind Kind, V1CustomResourceDefinition Crd);

public sealed record PendingCustomResourceConfig(GroupApiVersionKind Kind, IResourceConfig ResourceConfig);

