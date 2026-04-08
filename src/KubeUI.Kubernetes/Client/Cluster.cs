using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.Reactive.Linq;
using System.Reflection;
using System.Xml;
using DynamicData;
using DynamicData.Aggregation;
using DynamicData.Binding;
using DynamicData.Kernel;
using k8s;
using k8s.KubeConfigModels;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubernetesCRDModelGen;
using KubeUI.Kubernetes;
using Mapster;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace KubeUI.Kubernetes;

public sealed partial class Cluster : ObservableObject, IClusterRuntime
{
    private const int MaxQueuedCustomResourceDefinitionsPerBatch = 25;
    private ILoggerFactory _loggerFactory;

    private ILogger<Cluster> _logger;

    private IClusterSettingsStore _settings;

    private IGenerator _generator;

    private IServiceProvider _serviceProvider;

    public V2beta1APIGroupDiscoveryList NativeAPIGroupDiscoveryList { get; private set; }

    public V2beta1APIGroupDiscoveryList APIGroupDiscoveryList { get; private set; }

    public event Action<WatchEventType, GroupApiVersionKind, IKubernetesObject<V1ObjectMeta>>? OnChange;
    public event Action<V1CustomResourceDefinition>? OnCustomResourceDefinitionReady;

    private readonly SemaphoreSlim _connectionLimiter = new(1, 1);

    private readonly ConcurrentDictionary<GroupApiVersionKind, Lazy<Task>> _seedTasks = new();
    private readonly SemaphoreSlim _customResourceDefinitionSignal = new(0);
    private readonly ConcurrentDictionary<string, V1CustomResourceDefinition> _pendingCustomResourceDefinitions = new(StringComparer.Ordinal);
    private CancellationTokenSource? _resourceInformerCancellationTokenSource = new();

    public ConcurrentDictionary<GroupApiVersionKind, object> Objects { get; } = [];

    IReadOnlyDictionary<GroupApiVersionKind, object> IClusterRuntime.Objects => Objects;

    [ObservableProperty]
    public partial string Name { get; set; }

    [ObservableProperty]
    public partial string KubeConfigPath { get; set; }

    [ObservableProperty]
    public partial K8SConfiguration KubeConfig { get; set; }

    [ObservableProperty]
    public partial ClusterStatus Status { get; set; }

    [ObservableProperty]
    public partial string? LastError { get; set; }

    [ObservableProperty]
    public partial bool RequiresNamespaceSelectionPrompt { get; set; }

    [ObservableProperty]
    public partial bool Connected { get; set; }

    [ObservableProperty]
    public partial IKubernetes? Client { get; set; }

    [ObservableProperty]
    public partial ModelCache ModelCache { get; set; }

    [ObservableProperty]
    public partial ReadOnlyObservableCollection<V1Namespace> Namespaces { get; set; }

    public Cluster(ILogger<Cluster> logger, ILoggerFactory loggerFactory, ModelCache modelCache, IGenerator generator, IClusterSettingsStore settings, IServiceProvider serviceProvider)
    {
        _loggerFactory = loggerFactory;
        _logger = logger;
        _portForwardSessionFactory = new KubernetesPortForwardSessionFactory(this);
        ModelCache = modelCache;
        _generator = generator;
        _generator.SetEnumSupport(false);

        var kubeAssemblyXmlDoc = new XmlDocument();
        kubeAssemblyXmlDoc.Load(typeof(Generator).Assembly.GetManifestResourceStream("runtime.KubernetesClient.xml"));
        ModelCache.AddToCache(typeof(V1Deployment).Assembly, kubeAssemblyXmlDoc);
        _settings = settings;
        _serviceProvider = serviceProvider;

        _ = Task.Run(ProcessQueuedCustomResourceDefinitionsAsync);
    }

    public async Task Connect()
    {
        await _connectionLimiter.WaitAsync();
        _logger.LogInformation("Connecting to {name}", Name);

        try
        {
            if (!Connected)
            {
                try
                {
                    Status = ClusterStatus.Connecting;
                    LastError = null;
                    RequiresNamespaceSelectionPrompt = false;
                    KubernetesClientConfiguration config;

                    if (string.IsNullOrEmpty(KubeConfigPath))
                    {
                        config = KubernetesClientConfiguration.BuildConfigFromConfigObject(KubeConfig, Name);
                    }
                    else
                    {
                        config = KubernetesClientConfiguration.BuildConfigFromConfigFile(KubeConfigPath, Name);
                    }

                    // build a custom pipeline for HTTP calls
                    var pipe = new ResiliencePipelineBuilder<HttpResponseMessage>()
                    {
                        Name = "Cluster",
                        InstanceName = Name
                    }
                    .AddRetry(new HttpRetryStrategyOptions
                    {
                        MaxRetryAttempts = 3,
                    })
                    .ConfigureTelemetry(_loggerFactory);

                    var handler = new OperationKeyHandler()
                    {
                        InnerHandler = new ResilienceHandler(pipe.Build())
                    };

                    Client = new k8s.Kubernetes(config);
                    EnsureResourceInformerCancellationTokenSource();

                    NativeAPIGroupDiscoveryList = await GetAPIGroupDiscoveryList();

                    APIGroupDiscoveryList = await GetAPIGroupDiscoveryList(false);

                    await UpdateNamespacePermission();

                    await SeedResource<V1Namespace>();
                    var namespaceCache = GetResourceSourceCache<V1Namespace>();

                    // Cant list Namespaces
                    if (!ListNamespaces)
                    {
                        var namespaces = _settings.GetClusterNamespaces(this);

                        if (namespaces.Count == 0)
                        {
                            Connected = false;
                            Status = ClusterStatus.Errored;
                            LastError = "Unable to connect because the cluster cannot list namespaces and no fallback namespaces are configured.";
                            RequiresNamespaceSelectionPrompt = true;
                            _logger.LogWarning(
                                "Cluster {Name} cannot list namespaces and has no configured namespace fallback.",
                                Name);
                            return;
                        }

                        foreach (var item in namespaces)
                        {
                            namespaceCache.AddOrUpdate(new V1Namespace() { Metadata = new() { Name = item } });
                        }
                    }

                    namespaceCache
                    .Connect()
                    .SortAndBind(out var filteredObjects, SortExpressionComparer<V1Namespace>.Ascending(p => p.Name()))
                    .Subscribe((_) => { }, (y) => _logger.LogError(y, "Error Namespace Observable"));

                    Namespaces ??= filteredObjects;

                    Connected = true;
                    Status = ClusterStatus.Connected;
                    LastError = null;
                    RequiresNamespaceSelectionPrompt = false;

                    await InitMetrics();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error connecting to {name}", Name);

                    Connected = false;

                    Status = ClusterStatus.Errored;
                    LastError = ex.Message;
                    RequiresNamespaceSelectionPrompt = false;
                }
            }
        }
        finally
        {
            _connectionLimiter.Release();
            _logger.LogInformation("Connected to {name}", Name);
        }
    }

    public Task Disconnect()
    {
        _logger.LogInformation("Disconnecting from {name}", Name);

        StopMetrics();
        StopPortForwarders();
        StopResourceInformers();
        ClearDynamicCustomResourceDefinitions();
        ClearSeededResources();
        ClearSeedTasks();

        if (Client is IDisposable disposableClient)
        {
            disposableClient.Dispose();
        }

        Client = null;
        Connected = false;
        Status = ClusterStatus.None;
        LastError = null;
        RequiresNamespaceSelectionPrompt = false;

        _logger.LogInformation("Disconnected from {name}", Name);
        return Task.CompletedTask;
    }

    public async Task SeedResource<T>(bool waitForReady = false) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        var type = typeof(T);
        var kind = GroupApiVersionKind.From<T>();
        var createdSeedTask = false;
        if (!_seedTasks.TryGetValue(kind, out var seedTask))
        {
            var newSeedTask = new Lazy<Task>(() => SeedResourceCoreAsync<T>(), LazyThreadSafetyMode.ExecutionAndPublication);
            seedTask = _seedTasks.GetOrAdd(kind, newSeedTask);
            createdSeedTask = ReferenceEquals(seedTask, newSeedTask);
        }

        _logger.LogDebug(
            createdSeedTask ? "Seed requested for {type}; scheduling initialization." : "Seed requested for {type}; initialization already pending or completed.",
            type);

        try
        {
            await seedTask.Value.ConfigureAwait(false);
        }
        catch
        {
            if (_seedTasks.TryGetValue(kind, out var current) && ReferenceEquals(current, seedTask))
            {
                _seedTasks.TryRemove(kind, out _);
            }

            throw;
        }

        if (waitForReady)
        {
            _logger.LogDebug("Waiting for resource readiness for {type}.", typeof(T));
            await IsResourceReady<T>().ConfigureAwait(false);
            _logger.LogDebug("Resource readiness reached for {type}.", typeof(T));
        }
    }

    private async Task SeedResourceCoreAsync<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        _logger.LogDebug("Starting seed initialization for {type}.", typeof(T));

        var type = typeof(T);
        var kind = GroupApiVersionKind.From<T>();

        ContainerClass<T> container = (ContainerClass<T>)Objects.GetOrAdd(kind, _ => new ContainerClass<T>());
        container.Initialized = true;

        if (await UpdateCanI(type, Verb.List).ConfigureAwait(false) && await UpdateCanI(type, Verb.Watch).ConfigureAwait(false))
        {
            var informer = new ResourceInformer<T>(Client, _serviceProvider.GetRequiredService<IHostApplicationLifetime>(), _loggerFactory.CreateLogger<ResourceInformer<T>>());
            container.Informers.Add(informer);
            container.InformerRegistrations.Add(informer.Register(GetResourceInformerCallback<T>()));
            var informerCancellationToken = GetResourceInformerCancellationToken();
            _ = Task.Run(() =>
            {
                informer.StartWatching();
                _ = informer.RunInfinite(informerCancellationToken);
            });
        }
        else
        {
            if (!IsResourceNamespaced<T>())
            {
                return;
            }

            foreach (var item in GetResourceList<V1Namespace>())
            {
                string ns = item.Name();

                if (await UpdateCanI(type, Verb.List, ns).ConfigureAwait(false) && await UpdateCanI(type, Verb.Watch, ns).ConfigureAwait(false))
                {
                    var informer = new ResourceInformer<T>(Client, _serviceProvider.GetRequiredService<IHostApplicationLifetime>(), _loggerFactory.CreateLogger<ResourceInformer<T>>(), @namespace: ns);
                    container.Informers.Add(informer);
                    container.InformerRegistrations.Add(informer.Register(GetResourceInformerCallback<T>()));
                    var informerCancellationToken = GetResourceInformerCancellationToken();

                    _ = Task.Run(() =>
                    {
                        informer.StartWatching();
                        _ = informer.RunInfinite(informerCancellationToken);
                    });
                }
            }
        }

        _logger.LogDebug("Finished seed initialization for {type}.", typeof(T));
    }

    private ResourceInformerCallback<T> GetResourceInformerCallback<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        return new ResourceInformerCallback<T>((eventType, item) =>
        {
            var kind = GroupApiVersionKind.From<T>();
            ResourceInformerCallbackGuard.Execute(_logger, eventType, kind, item, () =>
            {
                var items = GetResourceSourceCache<T>();

                switch (eventType)
                {
                    case WatchEventType.Added:
                        if (item is V1CustomResourceDefinition crd)
                        {
                            items.AddOrUpdate(item);
                            QueueCustomResourceDefinition(crd);
                        }
                        else
                        {
                            items.AddOrUpdate(item);
                        }
                        break;
                    case WatchEventType.Modified:
                        items.Edit(o =>
                        {
                            var key = o.GetKey(item);
                            var original = o.Lookup(key);
                            if (original.HasValue)
                            {
                                item.Adapt(original.Value);
                                o.Refresh(key);
                            }
                            else
                            {
                                o.AddOrUpdate(item);
                            }
                        });

                        if (item is V1CustomResourceDefinition modifiedCrd)
                        {
                            QueueCustomResourceDefinition(modifiedCrd);
                        }
                        break;
                    case WatchEventType.Deleted:
                        items.Remove(item);
                        if (item is V1CustomResourceDefinition crd2)
                        {
                            RemoveQueuedCustomResourceDefinition(crd2);
                            RemoveCustomResourceDefinitionArtifacts(crd2);
                            _ = Task.Run(RefreshApiGroupDiscoveryListAsync);
                        }
                        break;
                }

                OnChange?.Invoke(eventType, kind, item);
            });
        });
    }

    private void QueueCustomResourceDefinition(V1CustomResourceDefinition crd)
    {
        _pendingCustomResourceDefinitions[GetCustomResourceDefinitionKey(crd)] = crd;
        _customResourceDefinitionSignal.Release();
    }

    private void RemoveQueuedCustomResourceDefinition(V1CustomResourceDefinition crd)
    {
        _pendingCustomResourceDefinitions.TryRemove(GetCustomResourceDefinitionKey(crd), out _);
    }

    private static string GetCustomResourceDefinitionKey(V1CustomResourceDefinition crd)
    {
        return crd.Name();
    }

    private async Task ProcessQueuedCustomResourceDefinitionsAsync()
    {
        while (true)
        {
            await _customResourceDefinitionSignal.WaitAsync().ConfigureAwait(false);

            var processed = 0;

            while (processed < MaxQueuedCustomResourceDefinitionsPerBatch && TryTakeQueuedCustomResourceDefinition(out var crd))
            {
                processed++;

                try
                {
                    _logger.LogInformation("Processing queued CRD {name}", crd.Name());

                    if (await ProcessNewCRD(crd).ConfigureAwait(false))
                    {
                        OnCustomResourceDefinitionReady?.Invoke(crd);
                        _logger.LogInformation("Completed processing queued CRD {name}", crd.Name());
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error processing queued CRD {name}", crd.Name());
                }
            }
        }
    }

    private bool TryTakeQueuedCustomResourceDefinition(out V1CustomResourceDefinition crd)
    {
        foreach (var pending in _pendingCustomResourceDefinitions)
        {
            if (_pendingCustomResourceDefinitions.TryRemove(pending.Key, out crd))
            {
                return true;
            }
        }

        crd = null!;
        return false;
    }

    private async Task RefreshApiGroupDiscoveryListAsync()
    {
        try
        {
            APIGroupDiscoveryList = await GetAPIGroupDiscoveryList(false).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to refresh API discovery for cluster {name}", Name);
        }
    }

    private async Task<bool> ProcessNewCRD(V1CustomResourceDefinition crd)
    {
        var result = _generator.GenerateAssembly(crd, "KubeUI.Models");

        if (!result.Success || result.Assembly == null || result.XmlDocumentation == null)
        {
            result.UnloadHandle?.Dispose();
            foreach (var diagnostic in result.Diagnostics)
            {
                _logger.LogWarning("CRD generation diagnostic for {name}: {id} {message}", crd.Name(), diagnostic.Id, diagnostic.Message);
            }

            if (result.Exception != null)
            {
                _logger.LogWarning(result.Exception, "Unable to generate CRD for {name}", crd.Name());
            }
            else
            {
                _logger.LogWarning("Unable to generate CRD for {name}", crd.Name());
            }

            return false;
        }

        var (previousType, currentType) = ModelCache.ReplaceCustomResourceDefinition(crd, result.Assembly, result.XmlDocumentation, result.UnloadHandle);
        if (currentType == null)
        {
            _logger.LogWarning("Unable to resolve generated type for CRD {name}", crd.Name());
            return false;
        }

        await ReplaceCustomResourceDefinitionArtifactsAsync(previousType, currentType).ConfigureAwait(false);

        await RefreshApiGroupDiscoveryListAsync().ConfigureAwait(false);

        return true;
    }

    private void RemoveCustomResourceDefinitionArtifacts(V1CustomResourceDefinition crd)
    {
        var removedType = ModelCache.RemoveCustomResourceDefinition(crd);
        if (removedType != null)
        {
            RemoveSeededResourceContainer(removedType);
        }
    }

    private async Task ReplaceCustomResourceDefinitionArtifactsAsync(Type? previousType, Type currentType)
    {
        var hadExistingContainer = previousType != null && RemoveSeededResourceContainer(previousType);
        if (!hadExistingContainer)
        {
            return;
        }

        await SeedResourceAsync(currentType).ConfigureAwait(false);
    }

    private bool RemoveSeededResourceContainer(Type resourceType)
    {
        var kind = GroupApiVersionKind.From(resourceType);
        if (!Objects.TryRemove(kind, out var existingContainer))
        {
            return false;
        }

        if (existingContainer is not IClearableResourceContainer resourceContainer)
        {
            return false;
        }

        ClearResourceContainer(resourceContainer);
        return true;
    }

    private Task SeedResourceAsync(Type resourceType, bool waitForReady = false)
    {
        var method = GetType()
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .First(x => x.Name == nameof(SeedResource) && x.IsGenericMethodDefinition && x.GetParameters().Length == 1)
            .MakeGenericMethod(resourceType);

        return (Task)method.Invoke(this, [waitForReady])!;
    }

    public async Task DeleteResource<T>(T item) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        using var client = Client.GetGenericClient<T>();

        if (string.IsNullOrEmpty(item.Namespace()))
        {
            await client.DeleteAsync<T>(item.Name());
        }
        else
        {
            await client.DeleteNamespacedAsync<T>(item.Namespace(), item.Name());
        }
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
        if (Objects.TryGetValue(GroupApiVersionKind.From<T>(), out var obj) && obj is ContainerClass<T> container)
        {
            return container.Items;
        }

        throw new Exception("Resource has not been Seeded " + typeof(T));
    }

    public IObservable<int> GetResourceCount<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        return GetResourceSourceCache<T>().Connect().Count();
    }

    // Find the generic definition of GetResourceCount<T>()
    private static readonly MethodInfo s_getResourceCountMethod = typeof(Cluster)
        .GetMethods(BindingFlags.Instance | BindingFlags.Public)
        .First(m =>
               m.Name == nameof(GetResourceCount) &&
               m.IsGenericMethodDefinition &&
               m.GetGenericArguments().Length == 1 &&
               m.GetParameters().Length == 0);

    public IObservable<int> GetResourceCount(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);

        // Validate generic constraints: class, IKubernetesObject<V1ObjectMeta>, new()
        if (!typeof(IKubernetesObject<V1ObjectMeta>).IsAssignableFrom(type))
            throw new ArgumentException($"Type {type.FullName} does not implement IKubernetesObject<V1ObjectMeta>.", nameof(type));

        if (type.IsAbstract)
            throw new ArgumentException($"Type {type.FullName} must be a concrete type.", nameof(type));

        if (type.GetConstructor(Type.EmptyTypes) == null)
            throw new ArgumentException($"Type {type.FullName} must have a public parameterless constructor.", nameof(type));

        var closedMethod = s_getResourceCountMethod.MakeGenericMethod(type);

        return (IObservable<int>)closedMethod.Invoke(this, null)!;
    }

    public async Task AddOrUpdateResource<T>(T item) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        using var client = Client.GetGenericClient<T>();

        if (IsResourceNamespaced<T>() && string.IsNullOrEmpty(item.Namespace()))
        {
            item.Metadata.NamespaceProperty = "default";
        }

        if (string.IsNullOrEmpty(item.Namespace()))
        {
            if (item.Metadata.Uid != null)
            {
                // update
                await client.ReplaceAsync<T>(item, item.Name());
            }
            else
            {
                // add
                await client.CreateAsync<T>(item);
            }
        }
        else
        {
            if (item.Metadata.Uid != null)
            {
                // update namespaced
                await client.ReplaceNamespacedAsync<T>(item, item.Namespace(), item.Name());
            }
            else
            {
                // add namespaced
                await client.CreateNamespacedAsync<T>(item, item.Namespace());
            }
        }
    }

    public async Task DryRunYaml(Stream stream)
    {
        var dryRunMethod = GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
            .First(x => x.Name == nameof(DryRunResourceAsync) && x.IsGenericMethod && x.GetParameters().Length == 1);

        var reader = new StreamReader(stream);
        var parser = new Parser(new StringReader(reader.ReadToEnd()));
        parser.Consume<StreamStart>();

        var exceptions = new List<Exception>();

        while (parser.Accept<DocumentStart>(out _))
        {
            var doc = Serialization.KubernetesYaml.Deserialize(parser);
            var yaml = Serialization.KubernetesYaml.Serialize(doc);

            var obj = Serialization.KubernetesYaml.Deserialize<KubernetesObject>(yaml);
            try
            {
                var type = ModelCache.GetResourceType(obj.ApiGroup(), obj.ApiGroupVersion(), obj.Kind);

                if (type == null)
                {
                    exceptions.Add(new Exception($"Unable to find Type for {obj.ApiVersion + "/" + obj.Kind}"));
                    continue;
                }

                var model = Serialization.KubernetesYaml.Deserialize(yaml, type);

                if (model != null)
                {
                    var fooRef = dryRunMethod.MakeGenericMethod(type);
                    await (Task)fooRef.Invoke(this, [model])!;
                }
            }
            catch (Exception ex)
            {
                exceptions.Add(ex);
            }
        }

        if (exceptions.Count > 0)
        {
            throw new AggregateException("Error dry running Yaml", exceptions);
        }
    }

    public async Task ImportYaml(Stream stream)
    {
        var mi = GetType().GetMethods().First(x => x.Name == nameof(AddOrUpdateResource) && x.IsGenericMethod && x.GetParameters().Length == 1);

        var reader = new StreamReader(stream);
        var parser = new Parser(new StringReader(reader.ReadToEnd()));
        parser.Consume<StreamStart>();

        var exceptions = new List<Exception>();

        while (parser.Accept<DocumentStart>(out _))
        {
            var doc = Serialization.KubernetesYaml.Deserialize(parser);
            var yaml = Serialization.KubernetesYaml.Serialize(doc);

            var obj = Serialization.KubernetesYaml.Deserialize<KubernetesObject>(yaml);
            try
            {
                var type = ModelCache.GetResourceType(obj.ApiGroup(), obj.ApiGroupVersion(), obj.Kind);

                if (type == null)
                {
                    exceptions.Add(new Exception($"Unable to find Type for {obj.ApiVersion + "/" + obj.Kind}"));

                    continue;
                }

                var model = Serialization.KubernetesYaml.Deserialize(yaml, type);

                if (model != null)
                {
                    var fooRef = mi.MakeGenericMethod(type);
                    await (Task)fooRef.Invoke(this, [model]);
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

    public async Task ImportFolder(string path)
    {
        if (Directory.Exists(path))
        {
            var files = new DirectoryInfo(path)
                .EnumerateFiles("*", SearchOption.AllDirectories)
                .Where(fi => fi.Extension.Equals(".yaml", StringComparison.OrdinalIgnoreCase) || fi.Extension.Equals(".yml", StringComparison.OrdinalIgnoreCase))
                .ToList();

            var exceptions = new List<Exception>();

            foreach (var file in files)
            {
                try
                {
                    await ImportYaml(file.OpenRead());
                }
                catch (Exception ex)
                {
                    exceptions.Add(ex);
                }
            }

            if (exceptions.Count > 0)
            {
                throw new AggregateException("Error importing Folder", exceptions);
            }
        }
    }

    private async Task DryRunResourceAsync<T>(T item) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        if (Client == null)
        {
            throw new InvalidOperationException("Cluster client is not connected.");
        }

        if (IsResourceNamespaced<T>() && string.IsNullOrEmpty(item.Namespace()))
        {
            item.Metadata.NamespaceProperty = "default";
        }

        var api = GroupApiVersionKind.From<T>();
        const string dryRun = "All";

        if (string.IsNullOrEmpty(item.Namespace()))
        {
            if (item.Metadata.Uid != null)
            {
                await Client.CustomObjects.ReplaceClusterCustomObjectWithHttpMessagesAsync<T>(
                    item,
                    api.Group,
                    api.ApiVersion,
                    api.PluralName,
                    item.Name(),
                    dryRun: dryRun);
            }
            else
            {
                await Client.CustomObjects.CreateClusterCustomObjectWithHttpMessagesAsync<T>(
                    item,
                    api.Group,
                    api.ApiVersion,
                    api.PluralName,
                    dryRun: dryRun);
            }
        }
        else
        {
            if (item.Metadata.Uid != null)
            {
                await Client.CustomObjects.ReplaceNamespacedCustomObjectWithHttpMessagesAsync<T>(
                    item,
                    api.Group,
                    api.ApiVersion,
                    item.Namespace(),
                    api.PluralName,
                    item.Name(),
                    dryRun: dryRun);
            }
            else
            {
                await Client.CustomObjects.CreateNamespacedCustomObjectWithHttpMessagesAsync<T>(
                    item,
                    api.Group,
                    api.ApiVersion,
                    item.Namespace(),
                    api.PluralName,
                    dryRun: dryRun);
            }
        }
    }

    public bool IsResourceNamespaced(Type type)
    {
        var api = GroupApiVersionKind.From(type);

        if (string.IsNullOrEmpty(api.Group))
        {
            var native = GetAPIGroupDiscoveryListItem(api, true);

            if (native != null)
            {
                return native.scope == "Namespaced";
            }
        }

        var ext = GetAPIGroupDiscoveryListItem(api);

        return ext?.scope == "Namespaced";
    }

    public V2beta1APIGroupDiscoveryListItemVersionResource? GetAPIGroupDiscoveryListItem(GroupApiVersionKind api, bool isNative = false)
    {
        var list = isNative ? NativeAPIGroupDiscoveryList : APIGroupDiscoveryList;

        if (list == null || list.items == null)
            return null;

        var groupName = string.IsNullOrEmpty(api.Group) ? string.Empty : api.Group;

        var group = list.items.FirstOrDefault(x =>
            string.Equals(x.metadata?.name ?? string.Empty, groupName, StringComparison.Ordinal));
        if (group == null || group.versions == null)
            return null;

        var versions = group.versions
            .Where(x => x.version == api.ApiVersion)
            .OrderByDescending(x => string.Equals(x.freshness, "Current", StringComparison.Ordinal));

        var version = versions.FirstOrDefault(x => x.resources != null);
        if (version == null || version.resources == null)
            return null;

        return version.resources.FirstOrDefault(z =>
            z.resource == api.PluralName &&
            (z.responseKind == null || string.IsNullOrEmpty(z.responseKind.kind) || z.responseKind.kind == api.Kind)
        );
    }

    public bool IsResourceNamespaced<T>()
    {
        return IsResourceNamespaced(typeof(T));
    }

    private async Task<V2beta1APIGroupDiscoveryList> GetAPIGroupDiscoveryList(bool native = true)
    {
        var mi = typeof(k8s.Kubernetes).GetMethod("SendRequest", BindingFlags.NonPublic | BindingFlags.Instance);

        var gen = mi.MakeGenericMethod([typeof(V2beta1APIGroupDiscoveryList)]);

        IReadOnlyDictionary<string, IReadOnlyList<string>> headers = new Dictionary<string, IReadOnlyList<string>>()
        {
            { "accept", new List<string>() { "application/json;g=apidiscovery.k8s.io;v=v2;as=APIGroupDiscoveryList,application/json;g=apidiscovery.k8s.io;v=v2beta1;as=APIGroupDiscoveryList,application/json" } }
        };

        //SendRequest(string relativeUri, HttpMethod method, IReadOnlyDictionary<string, IReadOnlyList<string>> customHeaders, T body, CancellationToken cancellationToken)
        var resp = await (Task<HttpResponseMessage>)gen.Invoke(Client, [$"/{(native ? "api" : "apis")}?timeout=32s", HttpMethod.Get, headers, null, CancellationToken.None]);

        return await resp.Content.ReadFromJsonAsync<V2beta1APIGroupDiscoveryList>().ConfigureAwait(false)
            ?? throw new InvalidOperationException("API group discovery response was empty.");
    }

    public async Task<bool> IsResourceReady<T>(CancellationToken? token = null) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        token ??= CancellationToken.None;

        var kind = GroupApiVersionKind.From<T>();

        if (Objects.TryGetValue(kind, out var obj) && obj is ContainerClass<T> container)
        {
            var tasks = container.Informers.Select(x => x.ReadyAsync(token.Value));
            await Task.WhenAll(tasks).WaitAsync(token.Value).ConfigureAwait(false);
        }

        return false;
    }

    private void StopPortForwarders()
    {
        foreach (var portForwarder in PortForwarders.ToList())
        {
            RemovePortForward(portForwarder);
        }
    }

    private void StopMetrics()
    {
        _metricsRefreshCancellationTokenSource?.Cancel();
        _metricsRefreshCancellationTokenSource?.Dispose();
        _metricsRefreshCancellationTokenSource = null;

        _metricsRefreshTimer?.Dispose();
        _metricsRefreshTimer = null;

        NodeMetrics.Clear();
        PodMetrics.Clear();
        IsMetricsAvailable = false;
    }

    private void StopResourceInformers()
    {
        _resourceInformerCancellationTokenSource?.Cancel();
        _resourceInformerCancellationTokenSource?.Dispose();
        _resourceInformerCancellationTokenSource = null;
    }

    private void ClearDynamicCustomResourceDefinitions()
    {
        var processedCustomResourceDefinitions = new HashSet<string>(StringComparer.Ordinal);

        if (Objects.TryGetValue(GroupApiVersionKind.From<V1CustomResourceDefinition>(), out var existing)
            && existing is ContainerClass<V1CustomResourceDefinition> container)
        {
            foreach (var crd in container.Items.Items.ToList())
            {
                processedCustomResourceDefinitions.Add(GetCustomResourceDefinitionKey(crd));
                RemoveCustomResourceDefinitionArtifacts(crd);
            }
        }

        foreach (var pending in _pendingCustomResourceDefinitions.Values.ToList())
        {
            if (!processedCustomResourceDefinitions.Add(GetCustomResourceDefinitionKey(pending)))
            {
                continue;
            }

            RemoveCustomResourceDefinitionArtifacts(pending);
        }

        _pendingCustomResourceDefinitions.Clear();

        while (_customResourceDefinitionSignal.Wait(0))
        {
        }
    }

    private void ClearSeededResources()
    {
        foreach (var container in Objects.Values)
        {
            if (container is IClearableResourceContainer resourceContainer)
            {
                ClearResourceContainer(resourceContainer);
            }
        }

        Objects.Clear();
    }

    private void ClearSeedTasks()
    {
        _seedTasks.Clear();
    }

    private static void ClearResourceContainer(IClearableResourceContainer container)
    {
        container.Clear();
    }

    private CancellationToken GetResourceInformerCancellationToken()
    {
        EnsureResourceInformerCancellationTokenSource();
        return _resourceInformerCancellationTokenSource!.Token;
    }

    private void EnsureResourceInformerCancellationTokenSource()
    {
        if (_resourceInformerCancellationTokenSource == null || _resourceInformerCancellationTokenSource.IsCancellationRequested)
        {
            _resourceInformerCancellationTokenSource?.Dispose();
            _resourceInformerCancellationTokenSource = new CancellationTokenSource();
        }
    }

    private static void DisposeInformerRegistrations(IList<IResourceInformerRegistration> registrations)
    {
        foreach (var registration in registrations)
        {
            registration.Dispose();
        }

        registrations.Clear();
    }

    private static void DisposeInformers(IList<IResourceInformer> informers)
    {
        foreach (var informer in informers.OfType<IDisposable>())
        {
            informer.Dispose();
        }

        informers.Clear();
    }
}

public interface IClearableResourceContainer
{
    void Clear();
}

public partial class ContainerClass<T> : ObservableObject, IClearableResourceContainer where T : class, IKubernetesObject<V1ObjectMeta>, new()
{
    public Type Type { get; } = typeof(T);

    public ISourceCache<T, string> Items { get; } = new SourceCache<T, string>(x => x.Namespace() + "/" + x.Name());

    [ObservableProperty]
    public partial List<IResourceInformer> Informers { get; set; } = [];

    [ObservableProperty]
    public partial List<IResourceInformerRegistration> InformerRegistrations { get; set; } = [];

    [ObservableProperty]
    public partial bool Initialized { get; set; }

    public void Clear()
    {
        foreach (var registration in InformerRegistrations)
        {
            registration.Dispose();
        }

        InformerRegistrations.Clear();

        foreach (var informer in Informers.OfType<IDisposable>())
        {
            informer.Dispose();
        }

        Informers.Clear();
        Items.Clear();
        Initialized = false;
    }
}

public sealed class OperationKeyHandler : DelegatingHandler
{
    protected override async Task<HttpResponseMessage> SendAsync(
        HttpRequestMessage request,
        CancellationToken cancellationToken)
    {
        var key = $"{request.Method}:{request.RequestUri?.PathAndQuery}";
        var ctx = ResilienceContextPool.Shared.Get(key, cancellationToken);

        request.SetResilienceContext(ctx);

        try
        {
            return await base.SendAsync(request, cancellationToken);
        }
        finally
        {
            ResilienceContextPool.Shared.Return(ctx);
        }
    }
}



