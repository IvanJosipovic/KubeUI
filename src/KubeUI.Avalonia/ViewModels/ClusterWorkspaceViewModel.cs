using System.Collections.Concurrent;
using System.ComponentModel;
using System.Reflection;
using Avalonia.Media.Immutable;
using DynamicData;
using Avalonia.Threading;
using k8s;
using k8s.KubeConfigModels;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Resources;

namespace KubeUI.Avalonia.ViewModels;

public sealed partial class ClusterWorkspaceViewModel : ViewModelBase, IClusterRuntime, IDisposable
{
    private const int CustomResourceConfigBatchSize = 25;
    private static readonly TimeSpan CustomResourceConfigBatchWindow = TimeSpan.FromMilliseconds(200);

    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ClusterWorkspaceViewModel> _logger;
    private readonly ConcurrentDictionary<GroupApiVersionKind, IResourceConfig> _resourceConfigs = new();
    private readonly ConcurrentDictionary<string, PendingCustomResourceDefinitionChange> _pendingCustomResourceDefinitions = new(StringComparer.Ordinal);
    private readonly SemaphoreSlim _pendingCustomResourceDefinitionSignal = new(0);
    private readonly CancellationTokenSource _disposeCancellation = new();
    private readonly Task _pendingCustomResourceDefinitionTask;
    private bool _disposed;

    public ClusterWorkspaceViewModel(
        IClusterRuntime runtime,
        IServiceProvider serviceProvider,
        ILogger<ClusterWorkspaceViewModel> logger)
    {
        Runtime = runtime;
        _serviceProvider = serviceProvider;
        _logger = logger;

        Title = runtime.Name;
        Id = $"cluster-workspace-{runtime.Name}";
        BindRuntimeCollections();

        SubscribeRuntime();
        UpdateClusterColor();
        _pendingCustomResourceDefinitionTask = Task.Run(ProcessPendingCustomResourceDefinitionsAsync);
        _ = RefreshWorkspaceStateAsync();
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

    public Task SeedResource<T>(bool waitForReady = false) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        return Runtime.SeedResource<T>(waitForReady);
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
        await EnsureBuiltInResourceConfigsAsync();
        await EnsureDynamicResourceConfigsAsync().ConfigureAwait(false);
        UpdateClusterColor();
        NotifyRuntimeStateChanged();
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
                .Where(t => t.ServiceType.IsGenericType
                    && t.ServiceType.GetGenericTypeDefinition() == typeof(ResourceConfigBase<>)
                    && t.ServiceType.GenericTypeArguments.Length == 1)
                .Select(x => x.ServiceType)
                .Distinct()
                .ToList();

            foreach (var type in types)
            {
                var resourceConfig = (IResourceConfig)_serviceProvider.GetRequiredService(type);
                resourceConfig.Initialize(this);
                _resourceConfigs[resourceConfig.Kind] = resourceConfig;
            }
        }

        // Permissions can change across reconnects; always refresh cached resource config permissions.
        await RefreshResourceConfigPermissionsAsync();
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
        foreach (var resourceConfig in _resourceConfigs.Values)
        {
            try
            {
                await resourceConfig.UpdatePermissions();
            }
            catch (Exception ex)
            {
                _logger.LogDebug(ex, "Unable to refresh permissions for {Kind}", resourceConfig.Kind);
            }
        }

        NotifyResourcePermissionsChanged();
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
        Dispatcher.UIThread.Post(async () =>
        {
            switch (e.PropertyName)
            {
                case nameof(IClusterRuntime.Connected):
                    await RefreshWorkspaceStateAsync();
                    break;
                case nameof(IClusterRuntime.Status):
                    UpdateClusterColor();
                    OnPropertyChanged(nameof(Status));
                    OnPropertyChanged(nameof(IsMetricsAvailable));
                    break;
                case nameof(IClusterRuntime.Name):
                    Title = Runtime.Name;
                    OnPropertyChanged(nameof(Name));
                    break;
                case nameof(IClusterRuntime.ListNamespaces):
                    OnPropertyChanged(nameof(ListNamespaces));
                    break;
                case nameof(IClusterRuntime.Namespaces):
                    OnPropertyChanged(nameof(Namespaces));
                    break;
            }
        });
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

