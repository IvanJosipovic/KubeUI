using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using Avalonia;
using DynamicData;
using DynamicData.Kernel;
using k8s;
using k8s.KubeConfigModels;
using k8s.Models;
using KubeUI.Client;
using KubeUI.ViewModels;
using KubernetesClient.Informer.Client;
using Mapster;
using Microsoft.Extensions.DependencyInjection;
using System.Reactive.Linq;

namespace KubeUI.Avalonia.Tests.Infra;

public sealed class TestCluster : IClusterRuntime, INotifyPropertyChanged
{
    private readonly ObservableCollection<V1Namespace> _namespaces = [];
    private readonly ReadOnlyObservableCollection<V1Namespace> _readonlyNamespaces;
    private ClusterWorkspaceViewModel? _workspace;
    private bool _connected;
    private bool _listNamespaces;
    private ClusterStatus _status;
    private IKubernetes? _client;
    private K8SConfiguration _kubeConfig = new();
    private ModelCache _modelCache;
    private string _kubeConfigPath = string.Empty;
    private string _name = "test";

    public TestCluster()
    {
        _readonlyNamespaces = new ReadOnlyObservableCollection<V1Namespace>(_namespaces);
        Status = ClusterStatus.Connected;
        Connected = true;
        ListNamespaces = true;
        ModelCache = Application.Current?.GetRequiredService<ModelCache>() ?? throw new InvalidOperationException("Avalonia Application.Current is not initialized.");
    }

    public event PropertyChangedEventHandler? PropertyChanged;

    public static ClusterWorkspaceViewModel Get()
    {
        var runtime = new TestCluster();

        var ns = new V1Namespace()
        {
            Metadata = new() { Name = "default" }
        };

        runtime.AddOrUpdateResource(ns).GetAwaiter().GetResult();

        var workspace = runtime.CreateWorkspace();
        workspace.SelectedNamespaces.Add(ns);

        return workspace;
    }

    public ClusterWorkspaceViewModel CreateWorkspace()
    {
        _workspace ??= ActivatorUtilities.CreateInstance<ClusterWorkspaceViewModel>(
            Application.Current?.GetRequiredService<IServiceProvider>() ?? throw new InvalidOperationException("Avalonia Application.Current is not initialized."),
            this);

        return _workspace;
    }

    public ConcurrentDictionary<GroupApiVersionKind, object> Objects { get; } = [];

    IReadOnlyDictionary<GroupApiVersionKind, object> IClusterRuntime.Objects => Objects;

    public bool Connected
    {
        get => _connected;
        set => SetProperty(ref _connected, value);
    }

    public bool ListNamespaces
    {
        get => _listNamespaces;
        set => SetProperty(ref _listNamespaces, value);
    }

    public ClusterStatus Status
    {
        get => _status;
        set => SetProperty(ref _status, value);
    }

    public bool IsMetricsAvailable => true;

    public IKubernetes? Client
    {
        get => _client;
        set => SetProperty(ref _client, value);
    }

    public ReadOnlyObservableCollection<V1Namespace> Namespaces => _readonlyNamespaces;

    public K8SConfiguration KubeConfig
    {
        get => _kubeConfig;
        set => SetProperty(ref _kubeConfig, value);
    }

    public ModelCache ModelCache
    {
        get => _modelCache;
        set => SetProperty(ref _modelCache, value);
    }

    public ObservableCollection<NodeMetrics> NodeMetrics { get; } = [];

    public ObservableCollection<PodMetrics> PodMetrics { get; } = [];

    public ObservableCollection<PortForwarder> PortForwarders { get; } = [];

    public string KubeConfigPath
    {
        get => _kubeConfigPath;
        set => SetProperty(ref _kubeConfigPath, value);
    }

    public string Name
    {
        get => _name;
        set => SetProperty(ref _name, value);
    }

    public event Action<WatchEventType, GroupApiVersionKind, IKubernetesObject<V1ObjectMeta>>? OnChange;

    public Task Connect()
    {
        Connected = true;
        Status = ClusterStatus.Connected;
        return Task.CompletedTask;
    }

    public bool CanI(Type type, Verb verb, string? @namespace = null, string? subresource = null)
    {
        return true;
    }

    public bool CanI<T>(Verb verb, string? @namespace = null, string? subresource = null) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        return true;
    }

    public bool CanIAnyNamespace(Type type, Verb verb, string? subresource = null)
    {
        return true;
    }

    public bool CanIAnyNamespace<T>(Verb verb, string? subresource = null) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        return true;
    }

    public bool IsResourceNamespaced(Type type)
    {
        return type != typeof(V1Namespace)
            && type != typeof(V1Node)
            && type != typeof(V1PersistentVolume)
            && type != typeof(V1StorageClass)
            && type != typeof(V1CustomResourceDefinition)
            && type != typeof(V1ClusterRole)
            && type != typeof(V1ClusterRoleBinding)
            && type != typeof(V1IngressClass)
            && type != typeof(V1PriorityClass)
            && type != typeof(V1RuntimeClass)
            && type != typeof(V1ValidatingWebhookConfiguration)
            && type != typeof(V1MutatingWebhookConfiguration);
    }

    public bool IsResourceNamespaced<T>()
    {
        return IsResourceNamespaced(typeof(T));
    }

    public Task<bool> UpdateCanI(Type type, Verb verb, string? @namespace = null, string? subresource = null)
    {
        return Task.FromResult(true);
    }

    public Task<bool> UpdateCanI<T>(Verb verb, string? @namespace = null, string? subresource = null) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        return Task.FromResult(true);
    }

    public Task<bool> UpdateCanIAnyNamespaceAsync(Type type, Verb verb, string? subresource = null)
    {
        return Task.FromResult(true);
    }

    public Task<bool> UpdateCanIAnyNamespaceAsync<T>(Verb verb, string? subresource = null) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        return Task.FromResult(true);
    }

    public Task UpdatePermissionsAllNamespaceAsync(Type type, Verb verb, string? subresource = null)
    {
        return Task.CompletedTask;
    }

    public Task UpdatePermissionsAllNamespaceAsync<T>(Verb verb, string? subresource = null) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        return Task.CompletedTask;
    }

    public PortForwarder AddPodPortForward(string @namespace, string podName, int containerPort)
    {
        var pf = new PortForwarder(this, @namespace);
        pf.SetPod(podName, containerPort);
        PortForwarders.Add(pf);
        return pf;
    }

    public PortForwarder AddServicePortForward(string @namespace, string serviceName, int servicePort)
    {
        var pf = new PortForwarder(this, @namespace);
        pf.SetService(serviceName, servicePort);
        PortForwarders.Add(pf);
        return pf;
    }

    public void RemovePortForward(PortForwarder pf)
    {
        PortForwarders.Remove(pf);
    }

    public async Task AddOrUpdateResource<T>(T item) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        await SeedResource<T>();

        var kind = GroupApiVersionKind.From<T>();
        var container = (ContainerClass<T>)Objects[kind];

        container.Items.Edit(o =>
        {
            var key = o.GetKey(item);
            var original = o.Lookup(key);

            if (original.HasValue)
            {
                item.Adapt(original.Value);
                o.Refresh(key);
                OnChange?.Invoke(WatchEventType.Modified, kind, original.Value);
            }
            else
            {
                o.AddOrUpdate(item);
                OnChange?.Invoke(WatchEventType.Added, kind, item);
            }
        });

        if (item is V1Namespace ns)
        {
            SyncNamespaceCollection(ns);
        }
    }

    public Task DeleteResource<T>(T item) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        var kind = GroupApiVersionKind.From<T>();
        var container = (ContainerClass<T>)Objects[kind];
        container.Items.Remove(item);

        if (item is V1Namespace ns)
        {
            var existing = _namespaces.FirstOrDefault(x => x.Name() == ns.Name());
            if (existing != null)
            {
                _namespaces.Remove(existing);
            }
        }

        return Task.CompletedTask;
    }

    public T? GetResource<T>(string? @namespace, string name) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        return GetResourceSourceCache<T>().Lookup(@namespace + "/" + name).ValueOrDefault();
    }

    public IReadOnlyList<T> GetResourceList<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        return GetResourceSourceCache<T>().Items;
    }

    public ISourceCache<T, string> GetResourceSourceCache<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        var kind = GroupApiVersionKind.From<T>();

        if (!Objects.TryGetValue(kind, out var obj) || obj is not ContainerClass<T> container)
        {
            throw new InvalidOperationException($"Container not seeded for: {kind}");
        }

        return container.Items;
    }

    public IObservable<int> GetResourceCount(Type type)
    {
        var method = GetType()
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .First(x => x.Name == nameof(GetResourceCount) && x.IsGenericMethodDefinition && x.GetParameters().Length == 0)
            .MakeGenericMethod(type);

        return (IObservable<int>)method.Invoke(this, null)!;
    }

    public IObservable<int> GetResourceCount<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        return GetResourceSourceCache<T>().Connect().Count();
    }

    public Task<bool> IsResourceReady<T>(CancellationToken? token) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        return Task.FromResult(true);
    }

    public Task SeedResource<T>(bool waitForReady = false) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        var kind = GroupApiVersionKind.From<T>();

        if (!Objects.TryGetValue(kind, out _))
        {
            var container = new ContainerClass<T>();

            if (!Objects.TryAdd(kind, container))
            {
                throw new InvalidOperationException($"Duplicate Object Set detected for: {kind}");
            }
        }

        return Task.CompletedTask;
    }

    public Task ImportFolder(string path)
    {
        throw new NotImplementedException();
    }

    public Task ImportYaml(Stream stream)
    {
        throw new NotImplementedException();
    }

    private void SyncNamespaceCollection(V1Namespace item)
    {
        var existing = _namespaces.FirstOrDefault(x => x.Name() == item.Name());

        if (existing == null)
        {
            _namespaces.Add(item);
            return;
        }

        var index = _namespaces.IndexOf(existing);
        _namespaces[index] = item;
    }

    private void SetProperty<T>(ref T field, T value, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, value))
        {
            return;
        }

        field = value;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
