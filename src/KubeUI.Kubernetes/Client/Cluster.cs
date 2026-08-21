using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Reflection;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json;
using DynamicData;
using DynamicData.Binding;
using DynamicData.Kernel;
using k8s;
using k8s.KubeConfigModels;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Kubernetes.Client;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace KubeUI.Kubernetes;

public sealed partial class Cluster : ObservableObject, IClusterRuntime, IClusterAuthorization, IAsyncDisposable
{
    public IClusterAuthorization Permissions => this;

    private ILoggerFactory _loggerFactory;

    private ILogger<Cluster> _logger;

    private int _openApiSchemasLoadAttempted;
    private readonly SemaphoreSlim _openApiSchemaLoadGate = new(1, 1);

    private IClusterSettingsStore _settings;

    private IServiceProvider _serviceProvider;

    private IThreadDispatcher _dispatcher;

    public V2beta1APIGroupDiscoveryList NativeAPIGroupDiscoveryList { get; private set; }

    public V2beta1APIGroupDiscoveryList APIGroupDiscoveryList { get; private set; }

    public event Action<WatchEventType, GroupApiVersionKind, IKubernetesObject<V1ObjectMeta>>? OnChange;
    public event Action<IClusterRuntime>? NamespaceSelectionRequired;
    public event Action<IClusterRuntime, GroupApiVersionKind>? ResourceSeeded;
    public event Action<IClusterRuntime, GroupApiVersionKind>? ResourceUnseeded;

    private readonly SemaphoreSlim _connectionLimiter = new(1, 1);
    private int _disposeStarted;

    private CancellationTokenSource? _resourceInformerCancellationTokenSource = new();
    private ConcurrentBag<Task> _resourceInformerTasks = [];
    private IDisposable? _namespaceSubscription;

    public ConcurrentDictionary<GroupApiVersionKind, object> Objects { get; } = [];

    internal int ActiveResourceInformerTaskCount => _resourceInformerTasks.Count(static task => !task.IsCompleted);

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
    public partial bool Connected { get; set; }

    [ObservableProperty]
    public partial IKubernetes? Client { get; set; }

    /// <summary>
    /// Creates the Kubernetes client used by <see cref="Connect"/>.
    /// </summary>
    public Func<KubernetesClientConfiguration, IKubernetes>? KubernetesClientFactory { get; set; }

    [ObservableProperty]
    /// <summary>
    /// Gets or sets the model catalog used to resolve registered and cluster-specific resource models.
    /// </summary>
    public partial ClusterModelCatalog ModelCatalog { get; set; }

    [ObservableProperty]
    public partial ReadOnlyObservableCollection<V1Namespace> Namespaces { get; set; }

    /// <summary>
    /// Initializes a cluster runtime and its model catalog.
    /// </summary>
    /// <param name="logger">Cluster logger.</param>
    /// <param name="loggerFactory">Logger factory used by resource informers.</param>
    /// <param name="modelCatalog">Model catalog owned by this cluster.</param>
    /// <param name="settings">Cluster settings store.</param>
    /// <param name="serviceProvider">Application service provider.</param>
    /// <param name="dispatcher">Dispatcher used for UI-bound observable updates.</param>
    public Cluster(ILogger<Cluster> logger, ILoggerFactory loggerFactory, ClusterModelCatalog modelCatalog, IClusterSettingsStore settings, IServiceProvider serviceProvider, IThreadDispatcher dispatcher)
    {
        _loggerFactory = loggerFactory;
        _logger = logger;
        _portForwardSessionFactory = new KubernetesPortForwardSessionFactory(this);
        ModelCatalog = modelCatalog;
        _settings = settings;
        _serviceProvider = serviceProvider;
        _dispatcher = dispatcher;
    }

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposeStarted, 1) != 0)
        {
            return;
        }

        StopNamespaceSubscription();
        await StopResourceInformersAsync().ConfigureAwait(false);
        _openApiSchemaLoadGate.Dispose();
        _connectionLimiter.Dispose();
    }

    private Activity? StartClusterActivity(string activityName, ActivityKind activityKind = ActivityKind.Internal)
    {
        var activity = KubeInstrumentation.Source.StartActivity(activityName, activityKind);
        activity?.SetTag("kubernetes.cluster.name", Name);
        return activity;
    }

    public async Task Connect()
    {
        using var activity = StartClusterActivity(nameof(Connect));

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
                    ResetAuthorizationIndex();
                    KubernetesClientConfiguration config;

                    if (string.IsNullOrEmpty(KubeConfigPath))
                    {
                        config = KubernetesClientConfiguration.BuildConfigFromConfigObject(KubeConfig, Name);
                    }
                    else
                    {
                        config = KubernetesClientConfiguration.BuildConfigFromConfigFile(KubeConfigPath, Name);
                    }



                    if (KubernetesClientFactory is null)
                    {
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

                        Client = new k8s.Kubernetes(config, handler);
                    }
                    else
                    {
                        Client = KubernetesClientFactory(config);
                    }

                    EnsureResourceInformerCancellationTokenSource();

                    NativeAPIGroupDiscoveryList = await GetAPIGroupDiscoveryList().ConfigureAwait(true);

                    APIGroupDiscoveryList = await GetAPIGroupDiscoveryList(false).ConfigureAwait(true);

                    await EnsureOpenApiSchemasAsync().ConfigureAwait(true);

                    await UpdateNamespacePermission().ConfigureAwait(true);

                    await SeedResource<V1Namespace>(true).ConfigureAwait(true);
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
                            NamespaceSelectionRequired?.Invoke(this);
                            _logger.LogWarning(
                                "Cluster {Name} cannot list namespaces and has no configured namespace fallback.",
                                Name);
                            return;
                        }

                        foreach (var item in namespaces)
                        {
                            namespaceCache.AddOrUpdate(new V1Namespace() { Metadata = new() { Name = item, Uid = item } });
                        }
                    }

                    StopNamespaceSubscription();
                    _namespaceSubscription = namespaceCache
                        .Connect()
                        .ObserveOn(_dispatcher.Scheduler)
                        .SortAndBind(out var filteredObjects, SortExpressionComparer<V1Namespace>.Ascending(p => p.Name()))
                        .Subscribe((_) => { }, (y) => _logger.LogError(y, "Error Namespace Observable"));

                    Namespaces = filteredObjects;

                    Connected = true;
                    Status = ClusterStatus.Connected;
                    LastError = null;

                    await InitMetrics().ConfigureAwait(false);
                }
                catch (Exception ex)
                {
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    _logger.LogError(ex, "Error connecting to {name}", Name);

                    Connected = false;

                    Status = ClusterStatus.Errored;
                    LastError = ex.Message;
                }
            }
        }
        finally
        {
            _connectionLimiter.Release();
            _logger.LogInformation("Connected to {name}", Name);
        }
    }

    public async Task Disconnect()
    {
        await _connectionLimiter.WaitAsync().ConfigureAwait(false);
        _logger.LogInformation("Disconnecting from {name}", Name);

        try
        {
            StopMetrics();
            StopPortForwarders();
            StopNamespaceSubscription();

            await StopResourceInformersAsync().ConfigureAwait(false);
            ClearDynamicCustomResourceDefinitions();
            ClearSeededResources();

            if (Client is IDisposable disposableClient)
            {
                disposableClient.Dispose();
            }

            Client = null;
            Connected = false;
            Volatile.Write(ref _openApiSchemasLoadAttempted, 0);
            Status = ClusterStatus.None;
            LastError = null;
            ResetAuthorizationIndex();
        }
        finally
        {
            _connectionLimiter.Release();
        }

        _logger.LogInformation("Disconnected from {name}", Name);
    }

    public async Task SeedResource<T>(bool waitForReady = false) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        using var activity = StartClusterActivity(nameof(SeedResource) + "<" + typeof(T).Name + ">");

        var kind = GroupApiVersionKind.From<T>();
        ModelCatalog.RegisterResource(kind, typeof(T));
        var container = (ContainerClass<T>)Objects.GetOrAdd(kind, _ => new ContainerClass<T>());
        var seedTask = container.GetOrCreateSeedTask(() =>
            Task.Run(() => SeedResourceCoreAsync<T>()));

        _logger.LogDebug("Seed requested for {kind}.", kind);

        try
        {
            await seedTask.Value.ConfigureAwait(false);
        }
        catch
        {
            container.RemoveSeedTask(seedTask);

            throw;
        }

        if (waitForReady)
        {
            _logger.LogDebug("Waiting for resource readiness for {type}.", typeof(T));
            await IsResourceReady<T>().ConfigureAwait(false);
            _logger.LogDebug("Resource readiness reached for {type}.", typeof(T));
        }

        if (!container.IsSeeded)
        {
            container.RemoveSeedTask(seedTask);
        }
    }

    public async Task SeedResource(GroupApiVersionKind kind, bool waitForReady = false)
    {
        if (!ModelCatalog.IsCustomResource(kind))
        {
            if (!ModelCatalog.TryGetResourceType(kind, out var resourceType))
            {
                throw new ArgumentException($"Unknown resource kind {kind}.", nameof(kind));
            }

            await SeedResource(resourceType, waitForReady).ConfigureAwait(false);
            return;
        }

        var container = (ContainerClass<GenericKubernetesObject>)Objects.GetOrAdd(
            kind,
            _ => new ContainerClass<GenericKubernetesObject>());
        var seedTask = container.GetOrCreateSeedTask(async () =>
        {
            var informer = new ResourceInformer<GenericKubernetesObject>(
                Client ?? throw new InvalidOperationException("Cluster is not connected."),
                _serviceProvider.GetRequiredService<IHostApplicationLifetime>(),
                _loggerFactory.CreateLogger<ResourceInformer<GenericKubernetesObject>>(),
                resourceListLimit: 10000,
                groupApiVersionKind: kind);

            container.Informers.Add(informer);
            container.InformerRegistrations.Add(informer.Register(GetGenericResourceInformerCallback(kind)));

            informer.StartWatching();

            _resourceInformerTasks.Add(informer.RunInfinite(GetResourceInformerCancellationToken()));
            await informer.ReadyAsync(GetResourceInformerCancellationToken()).ConfigureAwait(false);

            if (container.IsSeeded)
            {
                ResourceSeeded?.Invoke(this, kind);
            }
        });

        try
        {
            await seedTask.Value.ConfigureAwait(false);
        }
        catch
        {
            container.RemoveSeedTask(seedTask);
            throw;
        }

        if (waitForReady)
        {
            await IsResourceReady<GenericKubernetesObject>(kind).ConfigureAwait(false);
        }

        if (!container.IsSeeded)
        {
            container.RemoveSeedTask(seedTask);
        }
    }

    private Task SeedResource(Type resourceType, bool waitForReady)
    {
        ArgumentNullException.ThrowIfNull(resourceType);

        var method = GetType()
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .First(candidate => candidate.Name == nameof(SeedResource)
                && candidate.IsGenericMethodDefinition
                && candidate.GetParameters().Length == 1)
            .MakeGenericMethod(resourceType);

        return (Task)method.Invoke(this, [waitForReady])!;
    }

    private ResourceInformerCallback<IKubernetesObject<V1ObjectMeta>> GetGenericResourceInformerCallback(GroupApiVersionKind kind)
    {
        return new ResourceInformerCallback<IKubernetesObject<V1ObjectMeta>>((eventType, item) =>
        {
            if (item is not GenericKubernetesObject generic)
            {
                return;
            }

            ResourceInformerCallbackGuard.Execute(_logger, eventType, kind, generic, () =>
            {
                if (!Objects.TryGetValue(kind, out var objectContainer)
                    || objectContainer is not ContainerClass<GenericKubernetesObject> container)
                {
                    return;
                }

                switch (eventType)
                {
                    case WatchEventType.Added:
                    case WatchEventType.Modified:
                        container.Items.AddOrUpdate(generic);
                        break;
                    case WatchEventType.Deleted:
                        container.Items.Remove(generic);
                        break;
                }

                OnChange?.Invoke(eventType, kind, generic);
            });
        });
    }

    private async Task SeedResourceCoreAsync<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        _logger.LogDebug("Starting seed initialization for {type}.", typeof(T));

        var kind = GroupApiVersionKind.From<T>();

        var container = (ContainerClass<T>)Objects.GetOrAdd(kind, _ => new ContainerClass<T>());

        if (CanI<T>(Verb.List) && CanI<T>(Verb.Watch))
        {
            var informer = new ResourceInformer<T>(Client, _serviceProvider.GetRequiredService<IHostApplicationLifetime>(), _loggerFactory.CreateLogger<ResourceInformer<T>>(), resourceListLimit: 10000);
            container.Informers.Add(informer);
            container.InformerRegistrations.Add(informer.Register(GetResourceInformerCallback<T>()));
            informer.StartWatching();
            _ = informer.RunInfinite(GetResourceInformerCancellationToken());
        }
        else
        {
            if (!IsResourceNamespaced<T>())
            {
                return;
            }

            foreach (var item in GetResourceList<V1Namespace>())
            {
                var ns = item.Name();

                if (CanI<T>(Verb.List, ns) && CanI<T>(Verb.Watch, ns))
                {
                    var informer = new ResourceInformer<T>(Client, _serviceProvider.GetRequiredService<IHostApplicationLifetime>(), _loggerFactory.CreateLogger<ResourceInformer<T>>(), @namespace: ns, resourceListLimit: 10000);
                    container.Informers.Add(informer);
                    container.InformerRegistrations.Add(informer.Register(GetResourceInformerCallback<T>()));
                    informer.StartWatching();
                    _resourceInformerTasks.Add(informer.RunInfinite(GetResourceInformerCancellationToken()));
                }
            }
        }

        _logger.LogDebug("Finished seed initialization for {type}.", typeof(T));

        if (container.IsSeeded)
        {
            ResourceSeeded?.Invoke(this, kind);
        }
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
                        items.AddOrUpdate(item);
                        RegisterCustomResourceDefinition(item as V1CustomResourceDefinition);
                        break;
                    case WatchEventType.Modified:
                        items.AddOrUpdate(item);
                        RegisterCustomResourceDefinition(item as V1CustomResourceDefinition);
                        break;
                    case WatchEventType.Deleted:
                        items.Remove(item);
                        if (item is V1CustomResourceDefinition crd2)
                        {
                            RemoveCustomResourceDefinitionArtifacts(crd2);
                            _ = RefreshApiGroupDiscoveryListAsync();
                        }
                        break;
                }

                OnChange?.Invoke(eventType, kind, item);
            });
        });
    }

    private async Task RefreshApiGroupDiscoveryListAsync()
    {
        try
        {
            APIGroupDiscoveryList = await GetAPIGroupDiscoveryList(false).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            Activity.Current?.SetStatus(ActivityStatusCode.Error, ex.Message);
            _logger.LogError(ex, "Unable to refresh API discovery for cluster {name}", Name);
        }
    }

    public async Task EnsureOpenApiSchemasAsync()
    {
        using var activity = StartClusterActivity(nameof(EnsureOpenApiSchemasAsync));
        activity?.SetTag("kubernetes.openapi.schema.count", ModelCatalog.OpenApiSchemas.Count);

        if ((Volatile.Read(ref _openApiSchemasLoadAttempted) != 0
                && ModelCatalog.OpenApiSchemas.Count > 0)
            || Client is not k8s.Kubernetes kubernetesClient)
        {
            activity?.SetTag("kubernetes.openapi.schema.load.skipped", true);
            return;
        }

        await _openApiSchemaLoadGate.WaitAsync().ConfigureAwait(false);
        try
        {
            if (Volatile.Read(ref _openApiSchemasLoadAttempted) != 0)
            {
                activity?.SetTag("kubernetes.openapi.schema.load.skipped", true);
                return;
            }

            const int maxAttempts = 3;
            for (var attempt = 1; attempt <= maxAttempts; attempt++)
            {
                try
                {
                    await ModelCatalog.OpenApiSchemas.LoadAsync(
                        kubernetesClient,
                        _resourceInformerCancellationTokenSource?.Token ?? CancellationToken.None).ConfigureAwait(false);

                    Volatile.Write(ref _openApiSchemasLoadAttempted, 1);
                    activity?.SetTag("kubernetes.openapi.schema.count", ModelCatalog.OpenApiSchemas.Count);
                    activity?.SetStatus(ActivityStatusCode.Ok);
                    return;
                }
                catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException or JsonException)
                {
                    activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
                    _logger.LogDebug(
                        ex,
                        "Unable to load Kubernetes OpenAPI v3 schemas for {name} on attempt {attempt} of {maxAttempts}.",
                        Name,
                        attempt,
                        maxAttempts);
                }
            }
        }
        finally
        {
            _openApiSchemaLoadGate.Release();
        }
    }

    private void RegisterCustomResourceDefinition(V1CustomResourceDefinition? crd)
    {
        if (crd is null || !crd.TryGetResourceKind(out var kind))
        {
            return;
        }

        var previousKind = ModelCatalog.RegisterCustomResourceDefinition(crd.Name(), kind);
        if (previousKind is { } previous && previous != kind)
        {
            InvalidateSeededResource(previous);
        }

        _ = RefreshApiGroupDiscoveryListAsync();
    }

    private void RemoveCustomResourceDefinitionArtifacts(V1CustomResourceDefinition crd)
    {
        var kind = ModelCatalog.RemoveCustomResourceDefinition(crd.Name());
        if (kind is null && crd.TryGetResourceKind(out var currentKind))
        {
            ModelCatalog.RemoveCustomResourceDefinition(currentKind);
            kind = currentKind;
        }

        if (kind is { } removedKind)
        {
            InvalidateSeededResource(removedKind);
        }
    }

    private bool InvalidateSeededResource(GroupApiVersionKind kind)
    {
        if (!Objects.TryRemove(kind, out var existingContainer))
        {
            return false;
        }

        ResourceUnseeded?.Invoke(this, kind);

        if (existingContainer is IClearableResourceContainer resourceContainer)
        {
            ClearResourceContainer(resourceContainer);
        }

        return true;
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
        return GetResourceSourceCache<T>().Items.FirstOrDefault(item =>
            string.Equals(item.Namespace(), @namespace, StringComparison.Ordinal)
            && string.Equals(item.Name(), name, StringComparison.Ordinal));
    }

    public IReadOnlyList<T> GetResourceList<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        return GetResourceSourceCache<T>().Items;
    }

    public ISourceCache<T, string> GetResourceSourceCache<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        return GetResourceSourceCache<T>(GroupApiVersionKind.From<T>());
    }

    public ISourceCache<T, string> GetResourceSourceCache<T>(GroupApiVersionKind kind) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        if (Objects.TryGetValue(kind, out var obj) && obj is ContainerClass<T> container)
        {
            return container.Items;
        }

        throw new Exception("Resource has not been Seeded " + kind);
    }

    public IObservable<int> GetResourceCount<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        return GetResourceCount(GroupApiVersionKind.From<T>());
    }

    public IObservable<int> GetResourceCount(GroupApiVersionKind kind)
    {
        if (Objects.TryGetValue(kind, out var obj) && obj is IResourceContainer container)
        {
            return container.ConnectCount();
        }

        throw new InvalidOperationException($"Resource has not been seeded {kind}.");
    }

    public async Task AddOrUpdateResource<T>(T item) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        var genericItem = item as GenericKubernetesObject;
        var genericKind = default(GroupApiVersionKind);
        if (genericItem is not null
            && !ModelCatalog.TryGetResourceKind(genericItem.ApiVersion ?? string.Empty, genericItem.Kind ?? string.Empty, out genericKind))
        {
            throw new InvalidOperationException($"Unable to resolve resource kind for {genericItem.ApiVersion}/{genericItem.Kind}.");
        }

        using var client = genericItem == null
            ? Client.GetGenericClient<T>()
            : Client.GetGenericClient(genericKind);
        var isNamespaced = genericItem == null
            ? IsResourceNamespaced<T>()
            : IsResourceNamespaced(genericKind);

        if (isNamespaced && string.IsNullOrEmpty(item.Namespace()))
        {
            item.Metadata.NamespaceProperty = "default";
        }

        if (string.IsNullOrEmpty(item.Namespace()))
        {
            if (item.Metadata.Uid != null)
            {
                // update
                var updated = await client.ReplaceAsync(item, item.Name());
                item.Metadata = updated.Metadata;
            }
            else
            {
                // add
                var created = await client.CreateAsync(item);
                item.Metadata = created.Metadata;
            }
        }
        else
        {
            if (item.Metadata.Uid != null)
            {
                // update namespaced
                var updated = await client.ReplaceNamespacedAsync(item, item.Namespace(), item.Name());
                item.Metadata = updated.Metadata;
            }
            else
            {
                // add namespaced
                var created = await client.CreateNamespacedAsync(item, item.Namespace());
                item.Metadata = created.Metadata;
            }
        }
    }

    public async Task DryRunYaml(Stream stream)
    {
        var exceptions = new List<Exception>();
        var resources = LoadGenericResources(stream);
        foreach (var generic in resources)
        {
            try
            {
                if (!ModelCatalog.TryGetResourceKind(generic.ApiVersion ?? string.Empty, generic.Kind ?? string.Empty, out var resourceKind))
                {
                    exceptions.Add(new Exception($"Unable to find resource model for {generic.ApiVersion + "/" + generic.Kind}"));
                    continue;
                }

                await DryRunResourceAsync(generic).ConfigureAwait(false);
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
        var exceptions = new List<Exception>();
        var resources = LoadGenericResources(stream);
        foreach (var generic in resources)
        {
            try
            {
                if (!ModelCatalog.TryGetResourceKind(generic.ApiVersion ?? string.Empty, generic.Kind ?? string.Empty, out var resourceKind))
                {
                    exceptions.Add(new Exception($"Unable to find resource model for {generic.ApiVersion + "/" + generic.Kind}"));
                    continue;
                }

                await AddOrUpdateResource(generic).ConfigureAwait(false);
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

    private static List<GenericKubernetesObject> LoadGenericResources(Stream stream)
    {
        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true);
        var parser = new Parser(new StringReader(reader.ReadToEnd()));
        parser.Consume<StreamStart>();
        var resources = new List<GenericKubernetesObject>();

        while (parser.Accept<DocumentStart>(out _))
        {
            var document = Serialization.KubernetesYaml.Deserialize(parser);
            resources.Add(Serialization.KubernetesYaml.Deserialize<GenericKubernetesObject>(
                Serialization.KubernetesYaml.Serialize(document!)));
        }

        return resources;
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
                    await using var stream = file.OpenRead();
                    await ImportYaml(stream);
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

        GroupApiVersionKind api;
        if (item is GenericKubernetesObject generic)
        {
            if (!ModelCatalog.TryGetResourceKind(generic.ApiVersion ?? string.Empty, generic.Kind ?? string.Empty, out api))
            {
                throw new InvalidOperationException($"Unable to resolve resource kind for {generic.ApiVersion}/{generic.Kind}.");
            }
        }
        else
        {
            api = GroupApiVersionKind.From<T>();
        }
        var isNamespaced = item is GenericKubernetesObject
            ? IsResourceNamespaced(api)
            : IsResourceNamespaced<T>();

        if (isNamespaced && string.IsNullOrEmpty(item.Namespace()))
        {
            item.Metadata.NamespaceProperty = "default";
        }

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

    public bool IsResourceNamespaced(GroupApiVersionKind kind)
    {
        if (string.IsNullOrEmpty(kind.Group))
        {
            var native = GetAPIGroupDiscoveryListItem(kind, true);

            if (native != null)
            {
                return native.scope == "Namespaced";
            }
        }

        return GetAPIGroupDiscoveryListItem(kind)?.scope == "Namespaced";
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
        return IsResourceNamespaced(GroupApiVersionKind.From<T>());
    }

    private async Task<V2beta1APIGroupDiscoveryList> GetAPIGroupDiscoveryList(bool native = true)
    {
        using var activity = StartClusterActivity(nameof(GetAPIGroupDiscoveryList) + (native ? "Native" : ""));

        var kubernetesClient = Client as k8s.Kubernetes
            ?? throw new InvalidOperationException("Cluster client is not connected.");
        using var request = new HttpRequestMessage(
            HttpMethod.Get,
            new Uri(kubernetesClient.BaseUri, $"{(native ? "api" : "apis")}?timeout=32s"));
        request.Headers.TryAddWithoutValidation(
            "Accept",
            "application/json;g=apidiscovery.k8s.io;v=v2;as=APIGroupDiscoveryList,application/json;g=apidiscovery.k8s.io;v=v2beta1;as=APIGroupDiscoveryList,application/json");
        using var resp = await kubernetesClient.SendAuthenticatedAsync(
            request,
            _resourceInformerCancellationTokenSource?.Token ?? CancellationToken.None).ConfigureAwait(false);
        resp.EnsureSuccessStatusCode();

        return await resp.Content.ReadFromJsonAsync(CustomSourceGenerationContext.Default.V2beta1APIGroupDiscoveryList).ConfigureAwait(false)
            ?? throw new InvalidOperationException("API group discovery response was empty.");
    }

    public async Task<bool> IsResourceReady<T>(CancellationToken? token = null) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        token ??= CancellationToken.None;

        return await IsResourceReady<T>(GroupApiVersionKind.From<T>(), token).ConfigureAwait(false);
    }

    private async Task<bool> IsResourceReady<T>(GroupApiVersionKind kind, CancellationToken? token = null)
        where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        token ??= CancellationToken.None;

        if (Objects.TryGetValue(kind, out var obj) && obj is ContainerClass<T> container)
        {
            var tasks = container.Informers.Select(x => x.ReadyAsync(token.Value));
            await Task.WhenAll(tasks).WaitAsync(token.Value).ConfigureAwait(false);
        }

        return true;
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

    private void StopNamespaceSubscription()
    {
        Interlocked.Exchange(ref _namespaceSubscription, null)?.Dispose();
    }

    private async Task StopResourceInformersAsync()
    {
        var cancellationTokenSource = Interlocked.Exchange(ref _resourceInformerCancellationTokenSource, null);
        cancellationTokenSource?.Cancel();

        if (Client is IDisposable disposableClient)
        {
            disposableClient.Dispose();
            Client = null;
        }

        var informerTasks = Interlocked.Exchange(ref _resourceInformerTasks, []).ToArray();
        try
        {
            using var shutdownTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(10));
            await Task.WhenAll(informerTasks).WaitAsync(shutdownTimeout.Token).ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (cancellationTokenSource?.IsCancellationRequested == true)
        {
        }

        foreach (var container in Objects.Values.OfType<IClearableResourceContainer>())
        {
            container.Clear();
        }

        cancellationTokenSource?.Dispose();
    }

    private void ClearDynamicCustomResourceDefinitions()
    {
        if (Objects.TryGetValue(GroupApiVersionKind.From<V1CustomResourceDefinition>(), out var existing)
            && existing is ContainerClass<V1CustomResourceDefinition> container)
        {
            foreach (var crd in container.Items.Items.ToList())
            {
                RemoveCustomResourceDefinitionArtifacts(crd);
            }
        }

        ModelCatalog.RemoveAllCustomResourceDefinitions();
    }

    private void ClearSeededResources()
    {
        foreach (var pair in Objects)
        {
            ResourceUnseeded?.Invoke(this, pair.Key);

            if (pair.Value is IClearableResourceContainer resourceContainer)
            {
                ClearResourceContainer(resourceContainer);
            }
        }

        Objects.Clear();
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

}

public interface IResourceContainer
{
    int InformerCount { get; }
    bool IsSeeded { get; }
    IObservable<ResourceChange> ConnectChanges(GroupApiVersionKind kind);
    IObservable<int> ConnectCount();
    IReadOnlyList<IKubernetesObject<V1ObjectMeta>> Snapshot();
}

public interface IClearableResourceContainer : IResourceContainer
{
    void Clear();
}

public partial class ContainerClass<T> : ObservableObject, IClearableResourceContainer where T : class, IKubernetesObject<V1ObjectMeta>, new()
{
    private Lazy<Task>? _seedTask;

    public int InformerCount => Informers.Count;

    public bool IsSeeded => InformerCount > 0;

    public ISourceCache<T, string> Items { get; } = new SourceCache<T, string>(GetResourceCacheKey);

    private static string GetResourceCacheKey(T resource)
    {
        return resource.Uid() ?? throw new InvalidOperationException(
            $"Resource {typeof(T).Name} '{resource.Namespace()}/{resource.Name()}' has no metadata UID.");
    }

    public IObservable<ResourceChange> ConnectChanges(GroupApiVersionKind kind)
    {
        return Items.Connect()
            .SelectMany(changes => changes)
            .Select(change => new ResourceChange(
                change.Reason switch
                {
                    ChangeReason.Add => WatchEventType.Added,
                    ChangeReason.Remove => WatchEventType.Deleted,
                    _ => WatchEventType.Modified,
                },
                kind,
                change.Current));
    }

    public IObservable<int> ConnectCount()
    {
        return Observable.Defer(() => Items.Connect()
            .Select(_ => Items.Items.Count)
            .StartWith(Items.Items.Count)
            .DistinctUntilChanged());
    }

    public IReadOnlyList<IKubernetesObject<V1ObjectMeta>> Snapshot()
        => Items.Items.Cast<IKubernetesObject<V1ObjectMeta>>().ToArray();

    [ObservableProperty]
    public partial List<IResourceInformer> Informers { get; set; } = [];

    [ObservableProperty]
    public partial List<IResourceInformerRegistration> InformerRegistrations { get; set; } = [];

    [ObservableProperty]
    public partial bool Initialized { get; set; }

    internal Lazy<Task> GetOrCreateSeedTask(Func<Task> seedFactory)
    {
        var newSeedTask = new Lazy<Task>(seedFactory, LazyThreadSafetyMode.ExecutionAndPublication);
        var seedTask = Interlocked.CompareExchange(ref _seedTask, newSeedTask, null) ?? newSeedTask;
        Initialized = true;
        return seedTask;
    }

    internal void RemoveSeedTask(Lazy<Task> seedTask)
    {
        if (ReferenceEquals(Interlocked.CompareExchange(ref _seedTask, null, seedTask), seedTask))
        {
            Initialized = false;
        }
    }

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
        Interlocked.Exchange(ref _seedTask, null);
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
