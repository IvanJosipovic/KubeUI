using System.Collections.Concurrent;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Xml;
using DynamicData;
using DynamicData.Kernel;
using k8s;
using k8s.KubeConfigModels;
using k8s.Models;
using KubeUI.Kubernetes;
using KubernetesClient.Informer.Client;
using KubernetesCRDModelGen;
using Mapster;
using Microsoft.Extensions.Logging.Abstractions;
using System.Reactive.Linq;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace KubeUI.Testing;

public class TestClusterRuntime : IClusterRuntime, INotifyPropertyChanged
{
    private readonly ObservableCollection<V1Namespace> _namespaces = [];
    private readonly ReadOnlyObservableCollection<V1Namespace> _readonlyNamespaces;
    private readonly ConcurrentDictionary<string, bool> _permissions = new(StringComparer.Ordinal);
    private static readonly Lazy<IGenerator> s_sharedGenerator = new(() =>
    {
        var g = new Generator(NullLoggerFactory.Instance);
        g.SetEnumSupport(false);
        return (IGenerator)g;
    });

    private static readonly Lazy<ModelCache> s_sharedModelCache = new(() =>
    {
        var cache = new ModelCache();
        var kubeAssemblyXmlDoc = new XmlDocument();
        using var stream = typeof(Generator).Assembly.GetManifestResourceStream("runtime.KubernetesClient.xml")
            ?? throw new InvalidOperationException("Unable to load Kubernetes client XML documentation.");
        kubeAssemblyXmlDoc.Load(stream);
        cache.AddToCache(typeof(V1Deployment).Assembly, kubeAssemblyXmlDoc);
        return cache;
    });

    private readonly IGenerator _generator = s_sharedGenerator.Value;
    private bool _connected;
    private bool _defaultPermissionAllowed = true;
    private bool _listNamespaces;
    private ClusterStatus _status;
    private IKubernetes? _client;
    private K8SConfiguration _kubeConfig = new();
    private ModelCache _modelCache;
    private string _kubeConfigPath = string.Empty;
    private string _name = "test";

    public TestClusterRuntime()
    {
        _readonlyNamespaces = new ReadOnlyObservableCollection<V1Namespace>(_namespaces);
        // Use shared generator and model cache to avoid expensive repeated reflection
        // and dynamic generation across many test instances.
        ModelCache = s_sharedModelCache.Value;
        Status = ClusterStatus.Connected;
        Connected = true;
        ListNamespaces = true;
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public event Action<V1CustomResourceDefinition>? OnCustomResourceDefinitionReady;
    public event Action<WatchEventType, GroupApiVersionKind, IKubernetesObject<V1ObjectMeta>>? OnChange;

    public Func<Task>? ConnectBehavior { get; set; }

    public bool CanCreatePodPortForward { get; set; } = true;

    public ConcurrentDictionary<GroupApiVersionKind, object> Objects { get; } = [];

    IReadOnlyDictionary<GroupApiVersionKind, object> IClusterRuntime.Objects => Objects;

    public bool DefaultPermissionAllowed
    {
        get => _defaultPermissionAllowed;
        set => _defaultPermissionAllowed = value;
    }

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

    public Task Connect()
    {
        if (ConnectBehavior != null)
        {
            return ConnectBehavior();
        }

        Connected = true;
        Status = ClusterStatus.Connected;
        return Task.CompletedTask;
    }

    public void SetPermission(Type type, Verb verb, bool allowed, string? @namespace = null, string? subresource = null)
    {
        _permissions[BuildPermissionKey(type, verb, @namespace, subresource)] = allowed;
    }

    public void SetPermission<T>(Verb verb, bool allowed, string? @namespace = null, string? subresource = null)
        where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        SetPermission(typeof(T), verb, allowed, @namespace, subresource);
    }

    public bool CanI(Type type, Verb verb, string? @namespace = null, string? subresource = null)
    {
        if (!string.IsNullOrEmpty(@namespace) && _permissions.TryGetValue(BuildPermissionKey(type, verb, null, subresource), out var globalAllowed) && globalAllowed)
        {
            return true;
        }

        if (_permissions.TryGetValue(BuildPermissionKey(type, verb, @namespace, subresource), out var allowed))
        {
            return allowed;
        }

        if (type == typeof(V1Pod) && verb == Verb.Create && string.Equals(subresource, "portforward", StringComparison.Ordinal))
        {
            return CanCreatePodPortForward;
        }

        return DefaultPermissionAllowed;
    }

    public bool CanI<T>(Verb verb, string? @namespace = null, string? subresource = null) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        return CanI(typeof(T), verb, @namespace, subresource);
    }

    public bool CanIAnyNamespace(Type type, Verb verb, string? subresource = null)
    {
        if (CanI(type, verb, subresource: subresource))
        {
            return true;
        }

        if (!IsResourceNamespaced(type))
        {
            return false;
        }

        foreach (var item in Namespaces)
        {
            if (CanI(type, verb, item.Name(), subresource))
            {
                return true;
            }
        }

        return false;
    }

    public bool CanIAnyNamespace<T>(Verb verb, string? subresource = null) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        return CanIAnyNamespace(typeof(T), verb, subresource);
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
        return Task.FromResult(CanI(type, verb, @namespace, subresource));
    }

    public Task<bool> UpdateCanI<T>(Verb verb, string? @namespace = null, string? subresource = null) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        return Task.FromResult(CanI<T>(verb, @namespace, subresource));
    }

    public Task<bool> UpdateCanIAnyNamespaceAsync(Type type, Verb verb, string? subresource = null)
    {
        return Task.FromResult(CanIAnyNamespace(type, verb, subresource));
    }

    public Task<bool> UpdateCanIAnyNamespaceAsync<T>(Verb verb, string? subresource = null) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        return Task.FromResult(CanIAnyNamespace<T>(verb, subresource));
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

        container.Items.Edit(list =>
        {
            var key = list.GetKey(item);
            var original = list.Lookup(key);

            if (original.HasValue)
            {
                item.Adapt(original.Value);
                list.Refresh(key);
                OnChange?.Invoke(WatchEventType.Modified, kind, original.Value);
            }
            else
            {
                list.AddOrUpdate(item);
                OnChange?.Invoke(WatchEventType.Added, kind, item);
            }
        });

        if (item is V1Namespace ns)
        {
            SyncNamespaceCollection(ns);
        }

        if (item is V1CustomResourceDefinition crd)
        {
            await ProcessCustomResourceDefinitionAsync(crd);
        }
    }

    public Task DeleteResource<T>(T item) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        var kind = GroupApiVersionKind.From<T>();

        if (Objects.TryGetValue(kind, out var obj) && obj is ContainerClass<T> container)
        {
            container.Items.Remove(item);
            OnChange?.Invoke(WatchEventType.Deleted, kind, item);
        }

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

    public Task<bool> IsResourceReady<T>(CancellationToken? token = null) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        return Task.FromResult(true);
    }

    public Task SeedResource<T>(bool waitForReady = false) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        var kind = GroupApiVersionKind.From<T>();

        if (!Objects.ContainsKey(kind))
        {
            if (!Objects.TryAdd(kind, new ContainerClass<T>()))
            {
                throw new InvalidOperationException($"Duplicate Object Set detected for: {kind}");
            }
        }

        return Task.CompletedTask;
    }

    public async Task ImportFolder(string path)
    {
        if (!Directory.Exists(path))
        {
            return;
        }

        foreach (var file in Directory.EnumerateFiles(path, "*.*", SearchOption.AllDirectories)
                     .Where(x => x.EndsWith(".yaml", StringComparison.OrdinalIgnoreCase) || x.EndsWith(".yml", StringComparison.OrdinalIgnoreCase)))
        {
            await using var stream = File.OpenRead(file);
            await ImportYaml(stream);
        }
    }

    public async Task ImportYaml(Stream stream)
    {
        var addOrUpdateMethod = GetType()
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .First(x => x.Name == nameof(AddOrUpdateResource) && x.IsGenericMethod && x.GetParameters().Length == 1);

        using var reader = new StreamReader(stream, System.Text.Encoding.UTF8, detectEncodingFromByteOrderMarks: true, leaveOpen: true);
        var parser = new Parser(new StringReader(await reader.ReadToEndAsync()));
        parser.Consume<StreamStart>();

        var exceptions = new List<Exception>();

        while (parser.Accept<DocumentStart>(out _))
        {
            var doc = KubeUI.Kubernetes.Serialization.KubernetesYaml.Deserializer.Deserialize(parser);
            var yaml = KubeUI.Kubernetes.Serialization.KubernetesYaml.Serialize(doc);
            var obj = KubeUI.Kubernetes.Serialization.KubernetesYaml.Deserialize<KubernetesObject>(yaml);

            try
            {
                var type = ModelCache.GetResourceType(obj.ApiGroup(), obj.ApiGroupVersion(), obj.Kind);

                if (type == null)
                {
                    exceptions.Add(new InvalidOperationException($"Unable to find Type for {obj.ApiVersion}/{obj.Kind}"));
                    continue;
                }

                var model = KubeUI.Kubernetes.Serialization.KubernetesYaml.Deserializer.Deserialize(yaml, type);

                if (model != null)
                {
                    var genericMethod = addOrUpdateMethod.MakeGenericMethod(type);
                    await (Task)genericMethod.Invoke(this, [model])!;
                }
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        }

        if (exceptions.Count > 0)
        {
            throw new AggregateException("Error importing Yaml", exceptions);
        }
    }

    private async Task ProcessCustomResourceDefinitionAsync(V1CustomResourceDefinition crd)
    {
        if (!ModelCache.CheckIfCRDExists(crd))
        {
            var assembly = _generator.GenerateAssembly(crd, "KubeUI.Models");

            if (assembly.Item1 == null || assembly.Item2 == null)
            {
                throw new InvalidOperationException($"Unable to generate CRD for {crd.Name()}");
            }

            ModelCache.AddToCache(assembly.Item1, assembly.Item2);
        }

        await Task.Yield();
        OnCustomResourceDefinitionReady?.Invoke(crd);
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

    private static string BuildPermissionKey(Type type, Verb verb, string? @namespace, string? subresource)
    {
        var kind = GroupApiVersionKind.From(type);
        return $"{verb}:{kind.Group}:{kind.PluralName}:{@namespace}:{subresource}:{kind.ApiVersion}";
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


