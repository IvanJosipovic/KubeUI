using System.Net.Http.Json;
using System.Reactive.Linq;
using System.Reflection;
using System.Xml;
using Avalonia.Collections;
using Dock.Model.Controls;
using Dock.Model.Core;
using DynamicData;
using DynamicData.Aggregation;
using DynamicData.Binding;
using DynamicData.Kernel;
using FluentAvalonia.UI.Controls;
using FluentIcons.Common;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using Humanizer;
using k8s;
using k8s.KubeConfigModels;
using k8s.Models;
using KubernetesCRDModelGen;
using KubeUI;
using KubeUI.Resources;
using Mapster;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Swordfish.NET.Collections;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using Kubernetes.Controller;
using Kubernetes.Controller.Client;

namespace KubeUI.Client;

public enum ClusterStatus
{
    None,
    Connecting,
    Errored,
    Connected
}

public sealed partial class Cluster : ObservableObject, ICluster
{
    private ILoggerFactory _loggerFactory;

    private ILogger<Cluster> _logger;

    private ISettingsService _settingsService;

    private IDialogService _dialogService;

    private IGenerator _generator;

    private IServiceProvider _serviceProvider;

    public V2beta1APIGroupDiscoveryList NativeAPIGroupDiscoveryList { get; private set; }

    public V2beta1APIGroupDiscoveryList APIGroupDiscoveryList { get; private set; }

    public event Action<WatchEventType, GroupApiVersionKind, IKubernetesObject<V1ObjectMeta>>? OnChange;

    private readonly SemaphoreSlim _connectionLimiter = new(1,1);

    private readonly SemaphoreSlim _seedLimiter = new(1,1);

    public AvaloniaDictionary<GroupApiVersionKind, object> Objects { get; } = [];

    private NavigationItem? _crdNavigationLink;

    [ObservableProperty]
    public partial string Name { get; set; }

    [ObservableProperty]
    public partial string KubeConfigPath { get; set; }

    [ObservableProperty]
    public partial K8SConfiguration KubeConfig { get; set; }

    [ObservableProperty]
    public partial ClusterStatus Status { get; set; }

    [ObservableProperty]
    public partial bool Connected { get; set; }

    [ObservableProperty]
    public partial IKubernetes? Client { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<NavigationItem> NavigationItems { get; set; } = new ObservableSortedCollection<NavigationItem>(new NavigationItemOrderComparer());

    [ObservableProperty]
    public partial ModelCache ModelCache { get; set; }

    [ObservableProperty]
    public partial bool IsExpanded { get; set; }

    [ObservableProperty]
    public partial IReadOnlyList<V1Namespace> Namespaces { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<V1Namespace> SelectedNamespaces { get; set; } = [];

    private ConcurrentObservableDictionary<GroupApiVersionKind, IResourceConfig> ResourceConfigs { get; set; } = [];

    public Cluster(ILogger<Cluster> logger, ILoggerFactory loggerFactory, ModelCache modelCache, IGenerator generator, ISettingsService settingsService, IDialogService dialogService, IServiceProvider serviceProvider)
    {
        _loggerFactory = loggerFactory;
        _logger = logger;
        ModelCache = modelCache;
        _generator = generator;
        _generator.SetEnumSupport(false);

        var kubeAssemblyXmlDoc = new XmlDocument();
        kubeAssemblyXmlDoc.Load(typeof(Generator).Assembly.GetManifestResourceStream("runtime.KubernetesClient.xml"));
        ModelCache.AddToCache(typeof(V1Deployment).Assembly, kubeAssemblyXmlDoc);
        _settingsService = settingsService;
        _dialogService = dialogService;
        _serviceProvider = serviceProvider;
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

                    NativeAPIGroupDiscoveryList = await GetAPIGroupDiscoveryList();

                    APIGroupDiscoveryList = await GetAPIGroupDiscoveryList(false);

                    await AddResourceConfigs();

                    await UpdateNamespacePermission();

                    await SeedResource<V1Namespace>();
                    var namespaceCache = GetResourceSourceCache<V1Namespace>();

                    // Cant list Namespaces
                    if (!ListNamespaces)
                    {
                        var settings = _settingsService.Settings.GetClusterSettings(this);

                        if (settings.Namespaces.Count == 0)
                        {
                            Connected = false;
                            Status = ClusterStatus.Errored;

                            var vm = Application.Current.GetRequiredService<ClusterSettingsViewModel>();

                            if (vm is IInitializeCluster init)
                            {
                                init.Initialize(this);
                            }

                            Dispatcher.UIThread.Post(() => Application.Current.GetRequiredService<IFactory>().AddToDocuments(vm));

                            ContentDialogSettings dialogSettings = new()
                            {
                                Title = Assets.Resources.Cluster_Missing_Namespace_Permission_Title,
                                Content = Assets.Resources.Cluster_Missing_Namespace_Permission_Content,
                                PrimaryButtonText = Assets.Resources.Cluster_Missing_Namespace_Permission_Primary,
                                DefaultButton = ContentDialogButton.Primary
                            };

                            var result = await _dialogService.ShowContentDialogAsync(this, dialogSettings);

                            return;
                        }

                        foreach (var item in settings.Namespaces)
                        {
                            namespaceCache.AddOrUpdate(new V1Namespace() { Metadata = new() { Name = item } });
                        }
                    }

                    namespaceCache
                    .Connect()
                    .ObserveOn(AvaloniaScheduler.Instance)
                    .SortAndBind(out var filteredObjects, SortExpressionComparer<V1Namespace>.Ascending(p => p.Name()))
                    .Subscribe((_) => { }, (y) => _logger.LogError(y, "Error Namespace Observable"));

                    Namespaces = filteredObjects;

                    Connected = true;
                    Status = ClusterStatus.Connected;

                    await AddDefaultNavigation();
                    await InitMetrics();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error connecting to {name}", Name);

                    Connected = false;

                    Status = ClusterStatus.Errored;

                    var factory = Application.Current.GetRequiredService<IFactory>();

                    var doc = factory.GetDockable<IDocumentDock>("Documents");

                    if (doc != null)
                    {
                        var vm = new ClusterErrorViewModel()
                        {
                            Id = "cluster-error",
                            Error = ex.Message
                        };

                        var existingDock = doc.VisibleDockables.FirstOrDefault(x => x.Id == vm.Id);

                        if (existingDock != null)
                        {
                            factory?.CloseDockable(existingDock);
                        }

                        factory?.AddDockable(doc, vm);
                        factory?.SetActiveDockable(vm);
                        factory?.SetFocusedDockable(doc, vm);
                    }
                }
            }
        }
        finally
        {
            _connectionLimiter.Release();
            _logger.LogInformation("Connected to {name}", Name);
        }
    }

    private async Task AddDefaultNavigation()
    {
        NavigationItems.Add(new NavigationLink() { Name = "Settings", ControlType = typeof(ClusterSettingsViewModel), Cluster = this, FluentIcon = Icon.Settings, Order = 0 });
        NavigationItems.Add(new NavigationLink() { Name = Assets.Resources.ClusterViewModel_Title, ControlType = typeof(ClusterViewModel), Cluster = this, SvgIcon = "/Assets/kube/infrastructure_components/unlabeled/control-plane.svg", Order = 1 });
        NavigationItems.Add(new NavigationLink() { Name = Assets.Resources.VisualizationViewModel_Title, ControlType = typeof(VisualizationViewModel), Cluster = this, FluentIcon = Icon.SearchVisual, Order = 2 });
        NavigationItems.Add(new NavigationLink() { Name = "Load Yaml", Cluster = this, Id = "load-yaml", FluentIcon = Icon.ArrowUpload, Order = 3 });
        NavigationItems.Add(new NavigationLink() { Name = "Load Folder", Cluster = this, Id = "load-folder", FluentIcon = Icon.FolderAdd, Order = 4 });

        NavigationItems.Add(new NavigationItem() { Name = "Workloads", Order = 8 });
        NavigationItems.Add(new NavigationItem() { Name = "Configuration", Order = 9 });

        var networkNavItem = new NavigationItem() { Name = "Network", Order = 10 };

        NavigationItems.Add(networkNavItem);
        NavigationItems.Add(new NavigationItem() { Name = "Storage", Order = 11 });
        NavigationItems.Add(new NavigationItem() { Name = "Access Control", Order = 12 });

        _crdNavigationLink = new NavigationItem
        {
            Name = "Custom Resource Definitions",
            Order = 13,
            NavigationItems = new ObservableSortedCollection<NavigationItem>(new NavigationItemNameComparer())
        };

        NavigationItems.Add(_crdNavigationLink);

        foreach (var config in ResourceConfigs)
        {
            await config.Value.UpdatePermissions();

            if ((config.Value.Type == typeof(V1Namespace) && !ListNamespaces) ||
                (CanI(config.Value.Type, Verb.List) && CanI(config.Value.Type, Verb.Watch)))
            {
                var nav = new ResourceNavigationLink() { Name = config.Value.Name, ControlType = config.Value.Type, Cluster = this, Order = config.Value.Order };

                if (config.Value.Type == typeof(V1Namespace))
                {
                    nav.Count = GetResourceCount<V1Namespace>();
                }

                if (config.Value.Type == typeof(V1Pod))
                {
                    if (CanI<V1Pod>(Verb.Create, "portforward"))
                    {
                        Dispatcher.UIThread.Post(() => networkNavItem.NavigationItems.Add(new NavigationLink() { Name = Assets.Resources.PortForwarderListViewModel_Title, ControlType = typeof(PortForwarderListViewModel), Cluster = this, FluentIcon = Icon.CloudFlow, Order = 6 }), DispatcherPriority.Background);
                    }
                }

                if (config.Value.Type == typeof(V1CustomResourceDefinition))
                {
                    nav.Name = "Definitions";
                    nav.NavigationItems = new ObservableSortedCollection<NavigationItem>(new NavigationItemNameComparer());

#if !DEBUG
                    await SeedResource<V1CustomResourceDefinition>();
                    nav.Count = GetResourceCount<V1CustomResourceDefinition>();
#endif

                    Dispatcher.UIThread.Post(() => _crdNavigationLink.NavigationItems.Add(nav), DispatcherPriority.Background);
                    continue;
                }

                if (string.IsNullOrEmpty(config.Value.Category))
                {
                    Dispatcher.UIThread.Post(() => NavigationItems.Add(nav), DispatcherPriority.Background);
                }
                else
                {
                    var category = NavigationItems.First(x => x is not null && x.Name == config.Value.Category);

                    Dispatcher.UIThread.Post(() => category.NavigationItems.Add(nav), DispatcherPriority.Background);
                }
            }

        }
    }

    private async Task AddResourceConfigs()
    {
        var types = _serviceProvider.GetRequiredService<ServiceDescriptor[]>().Where(t => t.ServiceType.IsGenericType
                                                                                       && t.ServiceType.GetGenericTypeDefinition() == typeof(ResourceConfigBase<>)
                                                                                       && t.ServiceType.GenericTypeArguments.Length == 1
                                                                                    ).Select(x => x.ServiceType)
                                                                                     .ToList();
        List<IResourceConfig> configs = [];

        foreach (var type in types)
        {
            var resourceConfig = (IResourceConfig)_serviceProvider.GetRequiredService(type);

            if (resourceConfig is IInitializeCluster init)
            {
                init.Initialize(this);
            }

            ResourceConfigs[resourceConfig.Kind] = resourceConfig;

            configs.Add(resourceConfig);
        }
    }

    public async Task SeedResource<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        _logger.LogDebug("Starting Seed: {type}", typeof(T));

        var type = typeof(T);
        var kind = GroupApiVersionKind.From<T>();

        ContainerClass<T> container;

        await _seedLimiter.WaitAsync();

        if (Objects.TryGetValue(kind, out var obj) && obj is ContainerClass<T> container2)
        {
            container = container2;
        }
        else
        {
            container = new ContainerClass<T>();

            if (!Objects.TryAdd(kind, container))
            {
                throw new Exception($"Duplicate Object Set detected for: {kind}");
            }
        }

        if (!container.Initialized)
        {
            container.Initialized = true;
            _seedLimiter.Release();

            if (await UpdateCanI(type, Verb.List) && await UpdateCanI(type, Verb.Watch))
            {
                var informer = new ResourceInformer<T>(Client, _serviceProvider.GetRequiredService<IHostApplicationLifetime>(), _loggerFactory.CreateLogger<ResourceInformer<T>>());
                container.Informers.Add(informer);
                informer.Register(GetResourceInformerCallback<T>());
                informer.StartWatching();
                _ = informer.RunInfinite(CancellationToken.None);
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

                    if (await UpdateCanI(type, Verb.List, ns) && await UpdateCanI(type, Verb.Watch, ns))
                    {
                        var informer = new ResourceInformer<T>(Client, _serviceProvider.GetRequiredService<IHostApplicationLifetime>(), _loggerFactory.CreateLogger<ResourceInformer<T>>(), @namespace: ns);
                        container.Informers.Add(informer);
                        informer.Register(GetResourceInformerCallback<T>());
                        informer.StartWatching();
                        _ = informer.RunInfinite(CancellationToken.None);
                    }
                }
            }
        }
        else
        {
            _seedLimiter.Release();
        }

        _logger.LogDebug("Finished Seed: {type}", typeof(T));
    }

    private ResourceInformerCallback<T> GetResourceInformerCallback<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        return new ResourceInformerCallback<T>((eventType, item) =>
        {
            var kind = GroupApiVersionKind.From<T>();

            var items = GetResourceSourceCache<T>();

            switch (eventType)
            {
                case WatchEventType.Added:
                    if (item is V1CustomResourceDefinition crd)
                    {
                        _logger.LogInformation("Processing new CRD {name}", crd.Name());

                        ProcessNewCRD(crd)
                        .ContinueWith(t =>
                        {
                            if (t.Exception != null)
                            {
                                _logger.LogError("Exception in ProcessNewCRD: {exception}", t.Exception.Flatten());
                            }
                            else
                            {
                                items.AddOrUpdate(item);

                                _logger.LogInformation("Completed processing new CRD {name}", crd.Name());
                            }

                        }).GetAwaiter().GetResult();
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
                    break;
                case WatchEventType.Deleted:
                    items.Remove(item);
                    if (item is V1CustomResourceDefinition crd2)
                    {
                        APIGroupDiscoveryList = GetAPIGroupDiscoveryList(false).GetAwaiter().GetResult();

                        var title = crd2.Spec.Names.Kind.Humanize(LetterCasing.Title);

                        var fqdnlist = ConstructFQDNList(crd2.Spec.Group);
                        var list = _crdNavigationLink.NavigationItems;

                        NavigationItem? navItem = null;

                        foreach (var fqdn in fqdnlist)
                        {
                            navItem = list.FirstOrDefault(x => x.Name == fqdn);
                            if (navItem != null)
                            {
                                list = navItem.NavigationItems;
                            }
                            else
                            {
                                break;
                            }
                        }

                        if (navItem != null)
                        {
                            var nav = navItem.NavigationItems.FirstOrDefault(x => x.Name == title);
                            if (nav != null)
                            {
                                list.Remove(nav);
                            }
                        }
                    }
                    break;
            }

            OnChange?.Invoke(eventType, kind, item);
        });
    }

    private async Task<bool> ProcessNewCRD(V1CustomResourceDefinition crd)
    {
        if (!ModelCache.CheckIfCRDExists(crd))
        {
            var assembly = _generator.GenerateAssembly(crd, "KubeUI.Models");

            if (assembly.Item1 == null || assembly.Item2 == null)
            {
                _logger.LogWarning("Unable to generate CRD for {name}", crd.Name());
                return false;
            }

            ModelCache.AddToCache(assembly.Item1, assembly.Item2);

            var version = crd.Spec.Versions.First(x => x.Served && x.Storage);
            var resourceType = ModelCache.GetResourceType(crd.Spec.Group, version.Name, crd.Spec.Names.Kind);

            var genericMethod = s_processCustomResourceDefinitionMethod.MakeGenericMethod(resourceType);
            await (Task)genericMethod.Invoke(this, [crd]);
        }

        return true;
    }

    private static readonly MethodInfo s_processCustomResourceDefinitionMethod = typeof(Cluster).GetMethod(nameof(ProcessCustomResourceDefinition), BindingFlags.Instance | BindingFlags.NonPublic);

    private async Task ProcessCustomResourceDefinition<T>(V1CustomResourceDefinition crd) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        var api = GroupApiVersionKind.From<T>();

        if (GetAPIGroupDiscoveryListItem(api) == null)
        {
            APIGroupDiscoveryList = await GetAPIGroupDiscoveryList(false);
        }

        if (await UpdateCanIAnyNamespaceAsync<T>(Verb.List) && await UpdateCanIAnyNamespaceAsync<T>(Verb.Watch))
        {
            //Generate new Resource Configuration
            var resourceConfig = Application.Current.GetRequiredService<CRDResourceConfig<T>>();

            resourceConfig.Initialize(this);

            await resourceConfig.UpdatePermissions();

            resourceConfig.Generate(crd);

            ResourceConfigs[api] = resourceConfig;

            Dispatcher.UIThread.Post(() =>
            {
                var nav = new ResourceNavigationLink() { Name = api.Kind.Pluralize().Humanize(LetterCasing.Title), ControlType = typeof(T), Cluster = this, NavigationItems = new ObservableSortedCollection<NavigationItem>(new NavigationItemNameComparer()) };

                var fqdnlist = ConstructFQDNList(api.Group);

                var list = _crdNavigationLink!.NavigationItems;
                NavigationItem? navItem = null;

                foreach (var fqdn in fqdnlist)
                {
                    navItem = list.FirstOrDefault(x => x.Name == fqdn);

                    if (navItem != null)
                    {
                        list = navItem.NavigationItems;
                    }
                    else
                    {
                        navItem = new NavigationItem()
                        {
                            Name = fqdn,
                            NavigationItems = new ObservableSortedCollection<NavigationItem>(new NavigationItemNameComparer())
                        };

                        list.Add(navItem);

                        list = navItem.NavigationItems;
                    }
                }

                navItem!.NavigationItems.Add(nav);
            }, DispatcherPriority.Background);
        }
    }

    private static List<string> ConstructFQDNList(string domain)
    {
        List<string> fqdnList = [];
        string[] levels = domain.Split('.');

        if (levels.Length <= 2)
        {
            fqdnList.Add(domain);
            return fqdnList;
        }

        fqdnList.Add(levels[^2] + "." + levels[^1]);

        for (int i = levels.Length - 3; i >= 0; i--)
        {
            string groupedDomain = string.Join('.', levels, i, levels.Length - i);

            fqdnList.Add(groupedDomain);
        }

        return fqdnList;
    }

    public async Task DeleteResource<T>(T item) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        var api = GroupApiVersionKind.From<T>();

        using var client = new GenericClient(Client, api.Group, api.ApiVersion, api.PluralName, false);

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
        var api = GroupApiVersionKind.From<T>();

        using var client = new GenericClient(Client, api.Group, api.ApiVersion, api.PluralName, false);

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

    public async Task ImportYaml(Stream stream)
    {
        var mi = GetType().GetMethods().First(x => x.Name == nameof(AddOrUpdateResource) && x.IsGenericMethod && x.GetParameters().Length == 1);

        var reader = new StreamReader(stream);
        var parser = new Parser(new StringReader(reader.ReadToEnd()));
        parser.Consume<StreamStart>();

        var exceptions = new List<Exception>();

        while (parser.Accept<DocumentStart>(out _))
        {
            var doc = Serialization.KubernetesYaml.Deserializer.Deserialize(parser);
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

                var model = Serialization.KubernetesYaml.Deserializer.Deserialize(yaml, type);

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

        return ext.scope == "Namespaced";
    }

    public V2beta1APIGroupDiscoveryListItemVersionResource? GetAPIGroupDiscoveryListItem(GroupApiVersionKind api, bool isNative = false)
    {
        var list = isNative ? NativeAPIGroupDiscoveryList : APIGroupDiscoveryList;

        if (list == null || list.items == null)
            return null;

        var group = list.items.FirstOrDefault(x => x.metadata.name == (string.IsNullOrEmpty(api.Group) ? null : api.Group));
        if (group == null || group.versions == null)
            return null;

        var version = group.versions.FirstOrDefault(x => x.freshness == "Current" && x.version == api.ApiVersion);
        if (version == null || version.resources == null)
            return null;

        return version.resources.FirstOrDefault(z =>
            z.resource == api.PluralName &&
            z.responseKind.kind == api.Kind
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

        return await resp.Content.ReadFromJsonAsync<V2beta1APIGroupDiscoveryList>();
    }

    public IResourceConfig GetResourceConfig(GroupApiVersionKind kind)
    {
        return ResourceConfigs[kind];
    }

    public IResourceConfig GetResourceConfig<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        return GetResourceConfig(GroupApiVersionKind.From<T>());
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
}

public partial class ContainerClass<T> : ObservableObject where T : class, IKubernetesObject<V1ObjectMeta>, new()
{
    public Type Type { get; } = typeof(T);

    public ISourceCache<T, string> Items { get; } = new SourceCache<T, string>(x => x.Namespace() + "/" + x.Name());

    [ObservableProperty]
    public partial List<IResourceInformer> Informers { get; set; } = [];

    [ObservableProperty]
    public partial bool Initialized { get; set; }
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
