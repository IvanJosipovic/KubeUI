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
using KubeUI.Resources;

namespace KubeUI.ViewModels;

public sealed partial class ClusterWorkspaceViewModel : ViewModelBase, IClusterRuntime, IDisposable
{
    private readonly IServiceProvider _serviceProvider;
    private readonly ILogger<ClusterWorkspaceViewModel> _logger;
    private readonly ConcurrentDictionary<GroupApiVersionKind, IResourceConfig> _resourceConfigs = new();

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
    private bool _navigationReady;

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
        PulseNavigationReady();
    }

    public void Dispose()
    {
        Runtime.OnChange -= OnRuntimeChange;

        if (Runtime is INotifyPropertyChanged propertyChanged)
        {
            propertyChanged.PropertyChanged -= OnRuntimePropertyChanged;
        }
    }

    private async Task RefreshWorkspaceStateAsync()
    {
        NavigationReady = false;

        await EnsureBuiltInResourceConfigsAsync();
        await EnsureDynamicResourceConfigsAsync();
        UpdateClusterColor();
        NotifyRuntimeStateChanged();

        NavigationReady = true;
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

        if (!await Runtime.UpdateCanIAnyNamespaceAsync(resourceType, Verb.List).ConfigureAwait(false)
            || !await Runtime.UpdateCanIAnyNamespaceAsync(resourceType, Verb.Watch).ConfigureAwait(false))
        {
            return;
        }

        var api = GroupApiVersionKind.From(resourceType);
        var resourceConfigType = typeof(CRDResourceConfig<>).MakeGenericType(resourceType);
        var resourceConfig = (IResourceConfig)_serviceProvider.GetRequiredService(resourceConfigType);

        resourceConfig.Initialize(this);
        await resourceConfig.UpdatePermissions().ConfigureAwait(false);
        ((dynamic)resourceConfig).Generate(crd);

        _resourceConfigs[api] = resourceConfig;

        PulseNavigationReady();
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
            finally
            {
                PulseNavigationReady();
            }
        }
    }

    private void SubscribeRuntime()
    {
        Runtime.OnChange += OnRuntimeChange;

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
            RemoveCustomResourceConfig(crd);
            return;
        }

        _ = HandleCustomResourceDefinitionUpdateAsync(crd);
    }

    private async Task HandleCustomResourceDefinitionUpdateAsync(V1CustomResourceDefinition crd)
    {
        try
        {
            await EnsureCustomResourceConfigAsync(crd).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating custom resource config for {Crd}", crd.Name());
        }
    }

    private void RemoveCustomResourceConfig(V1CustomResourceDefinition crd)
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

        _resourceConfigs.TryRemove(GroupApiVersionKind.From(resourceType), out _);
        PulseNavigationReady();
    }

    private void PulseNavigationReady()
    {
        void Pulse()
        {
            NavigationReady = true;
            NavigationReady = false;
        }

        if (Dispatcher.UIThread.CheckAccess())
        {
            Pulse();
            return;
        }

        _ = Dispatcher.UIThread.InvokeAsync(Pulse);
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
