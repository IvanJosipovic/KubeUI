using System.Collections.Concurrent;
using System.Diagnostics;
using System.Net.Http.Json;
using System.Reactive.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Channels;
using DynamicData;
using DynamicData.Binding;
using DynamicData.Kernel;
using k8s;
using k8s.KubeConfigModels;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubernetesCRDModelGen;
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

    private IClusterSettingsStore _settings;

    private IGenerator _generator;

    private IServiceProvider _serviceProvider;

    private IThreadDispatcher _dispatcher;

    public V2beta1APIGroupDiscoveryList NativeAPIGroupDiscoveryList { get; private set; }

    public V2beta1APIGroupDiscoveryList APIGroupDiscoveryList { get; private set; }

    public event Action<WatchEventType, GroupApiVersionKind, IKubernetesObject<V1ObjectMeta>>? OnChange;
    public event Action<IClusterRuntime>? NamespaceSelectionRequired;
    public event Action<IClusterRuntime, GroupApiVersionKind>? ResourceSeeded;
    public event Action<IClusterRuntime, GroupApiVersionKind>? ResourceUnseeded;
    public event Func<V1CustomResourceDefinition, Task>? OnCustomResourceDefinitionReady;

    private readonly SemaphoreSlim _connectionLimiter = new(1, 1);
    private readonly Channel<V1CustomResourceDefinition> _customResourceDefinitionQueue = Channel.CreateUnbounded<V1CustomResourceDefinition>(
        new UnboundedChannelOptions { SingleReader = true, SingleWriter = false });
    private readonly CancellationTokenSource _customResourceDefinitionCancellationTokenSource = new();
    private readonly Task _customResourceDefinitionTask;
    private int _disposeStarted;

    private readonly ConcurrentDictionary<string, string> _customResourceDefinitionSignatures = new(StringComparer.Ordinal);
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
    /// Gets or sets the model catalog used to resolve built-in and cluster-specific resource models.
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
    /// <param name="generator">Custom-resource model generator.</param>
    /// <param name="settings">Cluster settings store.</param>
    /// <param name="serviceProvider">Application service provider.</param>
    /// <param name="dispatcher">Dispatcher used for UI-bound observable updates.</param>
    public Cluster(ILogger<Cluster> logger, ILoggerFactory loggerFactory, ClusterModelCatalog modelCatalog, IGenerator generator, IClusterSettingsStore settings, IServiceProvider serviceProvider, IThreadDispatcher dispatcher)
    {
        _loggerFactory = loggerFactory;
        _logger = logger;
        _portForwardSessionFactory = new KubernetesPortForwardSessionFactory(this);
        ModelCatalog = modelCatalog;
        _generator = generator;
        _generator.SetEnumSupport(false);

        _settings = settings;
        _serviceProvider = serviceProvider;
        _dispatcher = dispatcher;
        _customResourceDefinitionTask = ProcessCustomResourceDefinitionQueueAsync();
    }

    public async ValueTask DisposeAsync()
    {
        if (Interlocked.Exchange(ref _disposeStarted, 1) != 0)
        {
            return;
        }

        _customResourceDefinitionQueue.Writer.TryComplete();
        _customResourceDefinitionCancellationTokenSource.Cancel();
        StopNamespaceSubscription();
        await StopResourceInformersAsync().ConfigureAwait(false);
        _connectionLimiter.Dispose();

        try
        {
            await _customResourceDefinitionTask.ConfigureAwait(false);
        }
        catch (OperationCanceledException) when (_customResourceDefinitionCancellationTokenSource.IsCancellationRequested)
        {
        }
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

                    if (KubernetesClientFactory is null)
                    {
                        Client = new k8s.Kubernetes(config, handler);
                    }
                    else
                    {
                        handler.Dispose();
                        Client = KubernetesClientFactory(config);
                    }
                    EnsureResourceInformerCancellationTokenSource();

                    NativeAPIGroupDiscoveryList = await GetAPIGroupDiscoveryList().ConfigureAwait(true);

                    APIGroupDiscoveryList = await GetAPIGroupDiscoveryList(false).ConfigureAwait(true);

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
                        .Sort(SortExpressionComparer<V1Namespace>.Ascending(p => p.Name()))
                        .ObserveOn(_dispatcher.Scheduler)
                        .Bind(out var filteredObjects)
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

        var type = typeof(T);
        var kind = GroupApiVersionKind.From<T>();
        var container = (ContainerClass<T>)Objects.GetOrAdd(kind, _ => new ContainerClass<T>());
        var seedTask = container.GetOrCreateSeedTask(() =>
            Task.Run(() => SeedResourceCoreAsync<T>()));

        _logger.LogDebug("Seed requested for {type}.", type);

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

    private async Task SeedResourceCoreAsync<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        _logger.LogDebug("Starting seed initialization for {type}.", typeof(T));

        var type = typeof(T);
        var kind = GroupApiVersionKind.From<T>();

        var container = (ContainerClass<T>)Objects.GetOrAdd(kind, _ => new ContainerClass<T>());

        if (CanI(type, Verb.List) && CanI(type, Verb.Watch))
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

                if (CanI(type, Verb.List, ns) && CanI(type, Verb.Watch, ns))
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
                        items.AddOrUpdate(item);

                        if (item is V1CustomResourceDefinition modifiedCrd)
                        {
                            QueueCustomResourceDefinition(modifiedCrd);
                        }
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

    private static string GetCustomResourceDefinitionKey(V1CustomResourceDefinition crd)
    {
        return crd.Name();
    }

    private static string GetCustomResourceDefinitionSignature(V1CustomResourceDefinition crd)
    {
        return KubernetesJson.Serialize(crd.Spec);
    }

    private void QueueCustomResourceDefinition(V1CustomResourceDefinition crd)
    {
        _customResourceDefinitionQueue.Writer.TryWrite(crd);
    }

    private async Task ProcessCustomResourceDefinitionQueueAsync()
    {
        using var activity = StartClusterActivity(
            nameof(ProcessCustomResourceDefinitionQueueAsync),
            ActivityKind.Consumer);

        await foreach (var crd in _customResourceDefinitionQueue.Reader.ReadAllAsync(_customResourceDefinitionCancellationTokenSource.Token).ConfigureAwait(false))
        {
            await ProcessCustomResourceDefinitionAsync(crd).ConfigureAwait(false);
        }
    }

    private async Task ProcessCustomResourceDefinitionAsync(V1CustomResourceDefinition crd)
    {
        using var activity = StartClusterActivity(nameof(ProcessCustomResourceDefinitionAsync));
        activity?.SetTag("kubernetes.crd.name", crd.Name());

        try
        {
            if (!Connected)
            {
                return;
            }

            if (await ProcessNewCRD(crd).ConfigureAwait(false))
            {
                await NotifyCustomResourceDefinitionReadyAsync(crd).ConfigureAwait(false);
            }
        }
        catch (Exception ex)
        {
            activity?.SetStatus(ActivityStatusCode.Error, ex.Message);
            _logger.LogError(ex, "Error processing CRD {name}", crd.Name());
        }
    }

    private async Task NotifyCustomResourceDefinitionReadyAsync(V1CustomResourceDefinition crd)
    {
        var handlers = OnCustomResourceDefinitionReady?.GetInvocationList();
        if (handlers is null)
        {
            return;
        }

        var tasks = new Task[handlers.Length];
        for (var index = 0; index < handlers.Length; index++)
        {
            tasks[index] = ((Func<V1CustomResourceDefinition, Task>)handlers[index])(crd);
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);
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

    private async Task<bool> ProcessNewCRD(V1CustomResourceDefinition crd)
    {
        using var activity = StartClusterActivity(nameof(ProcessNewCRD));
        activity?.SetTag("kubernetes.crd.name", crd.Name());

        var key = GetCustomResourceDefinitionKey(crd);
        var signature = GetCustomResourceDefinitionSignature(crd);

        if (_customResourceDefinitionSignatures.TryGetValue(key, out var existingSignature)
            && string.Equals(existingSignature, signature, StringComparison.Ordinal))
        {
            _logger.LogDebug("Skipping CRD {name} update because its signature is unchanged.", crd.Name());
            return false;
        }

        using var generationActivity = StartClusterActivity("GenerateCustomResourceDefinitionAssembly");
        generationActivity?.SetTag("kubernetes.crd.name", crd.Name());
        var result = _generator.GenerateAssembly(crd, "KubeUI.Models");
        generationActivity?.Stop();

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

        var (previousType, currentType) = ModelCatalog.ReplaceCustomResourceDefinition(crd, result.Assembly, result.XmlDocumentation, result.UnloadHandle);
        if (currentType == null)
        {
            _logger.LogWarning("Unable to resolve generated type for CRD {name}", crd.Name());
            return false;
        }

        _customResourceDefinitionSignatures[key] = signature;

        await ReplaceCustomResourceDefinitionArtifactsAsync(previousType, currentType).ConfigureAwait(false);

        await RefreshApiGroupDiscoveryListAsync().ConfigureAwait(false);

        return true;
    }

    private void RemoveCustomResourceDefinitionArtifacts(V1CustomResourceDefinition crd)
    {
        _customResourceDefinitionSignatures.TryRemove(GetCustomResourceDefinitionKey(crd), out _);
        var removedType = ModelCatalog.RemoveCustomResourceDefinition(crd);
        if (removedType != null)
        {
            InvalidateSeededResource(removedType);
        }
    }

    private async Task ReplaceCustomResourceDefinitionArtifactsAsync(Type? previousType, Type currentType)
    {
        using var activity = StartClusterActivity(nameof(ReplaceCustomResourceDefinitionArtifactsAsync));
        activity?.SetTag("kubernetes.resource.type", currentType.Name);
        activity?.SetTag("kubernetes.previous_resource.type", previousType?.Name);

        var invalidatedSeedState = previousType != null && InvalidateSeededResource(previousType);
        if (!invalidatedSeedState)
        {
            return;
        }

        await SeedResource(currentType).ConfigureAwait(false);
    }

    private bool InvalidateSeededResource(Type resourceType)
    {
        var kind = GroupApiVersionKind.From(resourceType);

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

    public Task SeedResource(Type resourceType, bool waitForReady = false)
    {
        ArgumentNullException.ThrowIfNull(resourceType);

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
        if (Objects.TryGetValue(GroupApiVersionKind.From<T>(), out var obj) && obj is ContainerClass<T> container)
        {
            return container.Items;
        }

        throw new Exception("Resource has not been Seeded " + typeof(T));
    }

    public IObservable<int> GetResourceCount<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        return Observable.Defer(() =>
        {
            var sourceCache = GetResourceSourceCache<T>();
            return sourceCache.Connect()
                .Select(_ => sourceCache.Items.Count)
                .StartWith(sourceCache.Items.Count)
                .DistinctUntilChanged();
        });
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
        var dryRunMethod = GetType().GetMethods(BindingFlags.Instance | BindingFlags.NonPublic)
            .First(x => x.Name == nameof(DryRunResourceAsync) && x.IsGenericMethod && x.GetParameters().Length == 1);

        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true);
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
                var type = ModelCatalog.GetResourceType(obj.ApiGroup(), obj.ApiGroupVersion(), obj.Kind);

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

        using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true, bufferSize: 1024, leaveOpen: true);
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
                var type = ModelCatalog.GetResourceType(obj.ApiGroup(), obj.ApiGroupVersion(), obj.Kind);

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
        using var activity = StartClusterActivity(nameof(GetAPIGroupDiscoveryList) + (native ? "Native" : ""));

        var mi = typeof(k8s.Kubernetes).GetMethod("SendRequest", BindingFlags.NonPublic | BindingFlags.Instance);

        var gen = mi.MakeGenericMethod([typeof(V2beta1APIGroupDiscoveryList)]);

        IReadOnlyDictionary<string, IReadOnlyList<string>> headers = new Dictionary<string, IReadOnlyList<string>>()
        {
            { "accept", new List<string>() { "application/json;g=apidiscovery.k8s.io;v=v2;as=APIGroupDiscoveryList,application/json;g=apidiscovery.k8s.io;v=v2beta1;as=APIGroupDiscoveryList,application/json" } }
        };

        //SendRequest(string relativeUri, HttpMethod method, IReadOnlyDictionary<string, IReadOnlyList<string>> customHeaders, T body, CancellationToken cancellationToken)
        var resp = await (Task<HttpResponseMessage>)gen.Invoke(Client, [$"/{(native ? "api" : "apis")}?timeout=32s", HttpMethod.Get, headers, null, CancellationToken.None]);

        return await resp.Content.ReadFromJsonAsync(CustomSourceGenerationContext.Default.V2beta1APIGroupDiscoveryList).ConfigureAwait(false)
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
    IReadOnlyList<IKubernetesObject<V1ObjectMeta>> Snapshot();
}

public interface IClearableResourceContainer : IResourceContainer
{
    void Clear();
}

public partial class ContainerClass<T> : ObservableObject, IClearableResourceContainer where T : class, IKubernetesObject<V1ObjectMeta>, new()
{
    private Lazy<Task>? _seedTask;

    public Type Type { get; } = typeof(T);

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
