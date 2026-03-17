using System.Collections.Specialized;
using System.ComponentModel;
using System.Reflection;
using Avalonia.Media.Immutable;
using DynamicData;
using k8s;
using k8s.KubeConfigModels;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Resources;

namespace KubeUI.ViewModels;

public sealed partial class ClusterWorkspaceViewModel : ViewModelBase, IClusterRuntime, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ClusterWorkspaceViewModel> _logger;
    private readonly Dictionary<GroupApiVersionKind, IResourceConfig> _resourceConfigs = [];

    private INotifyCollectionChanged? _runtimeNamespacesCollection;
    private INotifyCollectionChanged? _runtimeNodeMetricsCollection;
    private INotifyCollectionChanged? _runtimePodMetricsCollection;
    private INotifyCollectionChanged? _runtimePortForwardersCollection;

    private readonly ObservableCollection<V1Namespace> _namespaces = [];

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
        Namespaces = new ReadOnlyObservableCollection<V1Namespace>(_namespaces);

        SubscribeRuntime();
        UpdateClusterColor();
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

    public IReadOnlyDictionary<GroupApiVersionKind, object> Objects => Runtime.Objects;
    public event Action<WatchEventType, GroupApiVersionKind, IKubernetesObject<V1ObjectMeta>>? OnChange;

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

    public ReadOnlyObservableCollection<V1Namespace> Namespaces { get; }

    ObservableCollection<NodeMetrics> IClusterRuntime.NodeMetrics => Runtime.NodeMetrics;
    ObservableCollection<PodMetrics> IClusterRuntime.PodMetrics => Runtime.PodMetrics;
    ObservableCollection<PortForwarder> IClusterRuntime.PortForwarders => Runtime.PortForwarders;
    ReadOnlyObservableCollection<V1Namespace> IClusterRuntime.Namespaces => Runtime.Namespaces;

    public async Task Connect()
    {
        await Runtime.Connect();
        await RefreshWorkspaceStateAsync();
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
        var result = Runtime.AddPodPortForward(@namespace, podName, containerPort);
        SyncPortForwarders();
        return result;
    }

    public PortForwarder AddServicePortForward(string @namespace, string serviceName, int servicePort)
    {
        var result = Runtime.AddServicePortForward(@namespace, serviceName, servicePort);
        SyncPortForwarders();
        return result;
    }

    public void RemovePortForward(PortForwarder pf)
    {
        Runtime.RemovePortForward(pf);
        SyncPortForwarders();
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
        return _resourceConfigs.Values;
    }

    public void Dispose()
    {
        Runtime.OnChange -= Runtime_OnChange;

        if (Runtime is INotifyPropertyChanged propertyChanged)
        {
            propertyChanged.PropertyChanged -= Runtime_PropertyChanged;
        }

        UnsubscribeRuntimeCollection(_runtimeNamespacesCollection, RuntimeCollectionChanged);
        UnsubscribeRuntimeCollection(_runtimeNodeMetricsCollection, RuntimeCollectionChanged);
        UnsubscribeRuntimeCollection(_runtimePodMetricsCollection, RuntimeCollectionChanged);
        UnsubscribeRuntimeCollection(_runtimePortForwardersCollection, RuntimeCollectionChanged);
    }

    private async Task RefreshWorkspaceStateAsync()
    {
        await EnsureBuiltInResourceConfigsAsync();
        await EnsureDynamicResourceConfigsAsync();
        SyncAllCollections();
        UpdateClusterColor();
        OnPropertyChanged(nameof(Connected));
        OnPropertyChanged(nameof(Status));
        OnPropertyChanged(nameof(Name));
        OnPropertyChanged(nameof(ListNamespaces));
        OnPropertyChanged(nameof(IsMetricsAvailable));
    }

    private async Task EnsureBuiltInResourceConfigsAsync()
    {
        if (_resourceConfigs.Count > 0)
        {
            return;
        }

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
            await EnsureCustomResourceConfigAsync(crd);
        }
    }

    private async Task EnsureCustomResourceConfigAsync(V1CustomResourceDefinition crd)
    {
        var version = crd.Spec?.Versions?.FirstOrDefault(x => x.Served && x.Storage);
        if (version == null)
        {
            return;
        }

        var resourceType = Runtime.ModelCache.GetResourceType(crd.Spec.Group, version.Name, crd.Spec.Names.Kind);
        if (resourceType == null)
        {
            return;
        }

        if (!await Runtime.UpdateCanIAnyNamespaceAsync(resourceType, Verb.List)
            || !await Runtime.UpdateCanIAnyNamespaceAsync(resourceType, Verb.Watch))
        {
            return;
        }

        var api = GroupApiVersionKind.From(resourceType);
        var resourceConfigType = typeof(CRDResourceConfig<>).MakeGenericType(resourceType);
        var resourceConfig = (IResourceConfig)_serviceProvider.GetRequiredService(resourceConfigType);

        resourceConfig.Initialize(this);
        await resourceConfig.UpdatePermissions();
        ((dynamic)resourceConfig).Generate(crd);

        _resourceConfigs[api] = resourceConfig;
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
    }

    private void SubscribeRuntime()
    {
        Runtime.OnChange += Runtime_OnChange;

        if (Runtime is INotifyPropertyChanged propertyChanged)
        {
            propertyChanged.PropertyChanged += Runtime_PropertyChanged;
        }

        SubscribeRuntimeCollection(ref _runtimeNamespacesCollection, Runtime.Namespaces);
        SubscribeRuntimeCollection(ref _runtimeNodeMetricsCollection, Runtime.NodeMetrics);
        SubscribeRuntimeCollection(ref _runtimePodMetricsCollection, Runtime.PodMetrics);
        SubscribeRuntimeCollection(ref _runtimePortForwardersCollection, Runtime.PortForwarders);
    }

    private void Runtime_PropertyChanged(object? sender, PropertyChangedEventArgs e)
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
                    break;
                case nameof(IClusterRuntime.Name):
                    Title = Runtime.Name;
                    OnPropertyChanged(nameof(Name));
                    break;
                case nameof(IClusterRuntime.ListNamespaces):
                    OnPropertyChanged(nameof(ListNamespaces));
                    break;
                case nameof(IClusterRuntime.Namespaces):
                    SubscribeRuntimeCollection(ref _runtimeNamespacesCollection, Runtime.Namespaces);
                    SyncNamespaces();
                    break;
                case nameof(IClusterRuntime.NodeMetrics):
                    SubscribeRuntimeCollection(ref _runtimeNodeMetricsCollection, Runtime.NodeMetrics);
                    SyncNodeMetrics();
                    break;
                case nameof(IClusterRuntime.PodMetrics):
                    SubscribeRuntimeCollection(ref _runtimePodMetricsCollection, Runtime.PodMetrics);
                    SyncPodMetrics();
                    break;
                case nameof(IClusterRuntime.PortForwarders):
                    SubscribeRuntimeCollection(ref _runtimePortForwardersCollection, Runtime.PortForwarders);
                    SyncPortForwarders();
                    break;
            }
        });
    }

    private void Runtime_OnChange(WatchEventType eventType, GroupApiVersionKind kind, IKubernetesObject<V1ObjectMeta> item)
    {
        OnChange?.Invoke(eventType, kind, item);

        if (item is not V1CustomResourceDefinition crd)
        {
            return;
        }

        _ = Dispatcher.UIThread.InvokeAsync(async () =>
        {
            if (eventType == WatchEventType.Deleted)
            {
                var version = crd.Spec?.Versions?.FirstOrDefault(x => x.Served && x.Storage);
                if (version == null)
                {
                    return;
                }

                var resourceType = Runtime.ModelCache.GetResourceType(crd.Spec.Group, version.Name, crd.Spec.Names.Kind);
                if (resourceType == null)
                {
                    return;
                }

                _resourceConfigs.Remove(GroupApiVersionKind.From(resourceType));
                return;
            }

            await EnsureCustomResourceConfigAsync(crd);
        });
    }

    private void RuntimeCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        Dispatcher.UIThread.Post(SyncAllCollections);
    }

    private void SyncAllCollections()
    {
        SyncNamespaces();
        SyncNodeMetrics();
        SyncPodMetrics();
        SyncPortForwarders();
    }

    private void SyncNamespaces()
    {
        ReplaceCollection(_namespaces, Runtime.Namespaces);
    }

    private void SyncNodeMetrics()
    {
        ReplaceCollection(NodeMetrics, Runtime.NodeMetrics);
    }

    private void SyncPodMetrics()
    {
        ReplaceCollection(PodMetrics, Runtime.PodMetrics);
    }

    private void SyncPortForwarders()
    {
        ReplaceCollection(PortForwarders, Runtime.PortForwarders);
    }

    private static void ReplaceCollection<T>(ObservableCollection<T> target, IEnumerable<T> source)
    {
        target.Clear();

        foreach (var item in source)
        {
            target.Add(item);
        }
    }

    private void SubscribeRuntimeCollection(ref INotifyCollectionChanged? target, object? collection)
    {
        UnsubscribeRuntimeCollection(target, RuntimeCollectionChanged);
        target = collection as INotifyCollectionChanged;
        target?.CollectionChanged += RuntimeCollectionChanged;
    }

    private static void UnsubscribeRuntimeCollection(INotifyCollectionChanged? collection, NotifyCollectionChangedEventHandler handler)
    {
        if (collection != null)
        {
            collection.CollectionChanged -= handler;
        }
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
