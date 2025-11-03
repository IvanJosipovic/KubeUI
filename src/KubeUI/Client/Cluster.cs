using System.Net.Http.Json;
using System.Reflection;
using System.Xml;
using Avalonia.Collections;
using Dock.Model.Controls;
using Dock.Model.Core;
using FluentAvalonia.UI.Controls;
using HanumanInstitute.MvvmDialogs;
using HanumanInstitute.MvvmDialogs.Avalonia.Fluent;
using Humanizer;
using k8s;
using k8s.KubeConfigModels;
using k8s.Models;
using KubernetesCRDModelGen;
using Yarp.Kubernetes.Controller.Client;
using KubeUI.Resources;
using Microsoft.Extensions.Http.Resilience;
using Polly;
using Swordfish.NET.Collections;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using Yarp.Kubernetes.Controller;
using Microsoft.Extensions.Hosting;

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

    private IPriorityExecutor _priorityExecutor;

    public V2beta1APIGroupDiscoveryList NativeAPIGroupDiscoveryList { get; private set; }

    public V2beta1APIGroupDiscoveryList APIGroupDiscoveryList { get; private set; }

    public event Action<WatchEventType, GroupApiVersionKind, IKubernetesObject<V1ObjectMeta>>? OnChange;

    private readonly SemaphoreSlim _connectionLimiter = new(1);

    private readonly SemaphoreSlim _seedLimiter = new(1);

    public AvaloniaDictionary<GroupApiVersionKind, ContainerClass> Objects { get; } = [];

    private ResourceNavigationLink? _crdNavigationLink;

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
    public partial AvaloniaDictionary<NamespacedName, V1Namespace> Namespaces { get; set; } = [];

    [ObservableProperty]
    public partial ObservableCollection<V1Namespace> SelectedNamespaces { get; set; } = [];

    private ConcurrentObservableDictionary<GroupApiVersionKind, IResourceConfig> ResourceConfigs { get; set; } = [];

    public Cluster(ILogger<Cluster> logger, ILoggerFactory loggerFactory, ModelCache modelCache, IGenerator generator, ISettingsService settingsService, IDialogService dialogService, IServiceProvider serviceProvider)
    {
        _loggerFactory = loggerFactory;
        _logger = logger;
        ModelCache = modelCache;
        _generator = generator;
        _priorityExecutor = new PriorityExecutor(serviceProvider.GetRequiredService<ILogger<PriorityExecutor>>(), Environment.ProcessorCount);
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
                        MaxRetryAttempts = 5,
                    })
                    .ConfigureTelemetry(_loggerFactory);

                    var handler = new OperationKeyHandler()
                    {
                        InnerHandler = new ResilienceHandler(pipe.Build())
                    };

                    Client = new Kubernetes(config, handler);

                    NativeAPIGroupDiscoveryList = await GetAPIGroupDiscoveryList();

                    APIGroupDiscoveryList = await GetAPIGroupDiscoveryList(false);

                    Connected = true;

                    Status = ClusterStatus.Connected;

                    await GetPermissions();

                    await AddResourceConfigs();

                    if (!ListNamespaces)
                    {
                        var settings = _settingsService.Settings.GetClusterSettings(this);

                        if (settings.Namespaces.Count == 0)
                        {
                            Connected = false;

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

                            Connected = false;

                            return;
                        }

                        var items = await GetObjectDictionaryAsync<V1Namespace>();

                        foreach (var item in settings.Namespaces)
                        {
                            items[new NamespacedName(item)] = new V1Namespace() { Metadata = new() { Name = item } };
                        }

                        Namespaces = items;
                    }
                    else
                    {
                        Namespaces = await GetObjectDictionaryAsync<V1Namespace>();
                    }

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
        NavigationItems.Add(new NavigationLink() { Name = "Settings", ControlType = typeof(ClusterSettingsViewModel), Cluster = this, StyleIcon = "ic_fluent_settings_24_filled", Order = 0 });
        NavigationItems.Add(new NavigationLink() { Name = Assets.Resources.ClusterViewModel_Title, ControlType = typeof(ClusterViewModel), Cluster = this, SvgIcon = "/Assets/kube/infrastructure_components/unlabeled/control-plane.svg", Order = 1 });
        NavigationItems.Add(new NavigationLink() { Name = Assets.Resources.VisualizationViewModel_Title, ControlType = typeof(VisualizationViewModel), Cluster = this, StyleIcon = "ic_fluent_search_visual_24_filled", Order = 2 });
        NavigationItems.Add(new NavigationLink() { Name = "Load Yaml", Cluster = this, Id = "load-yaml", StyleIcon = "arrow_upload_regular", Order = 3 });
        NavigationItems.Add(new NavigationLink() { Name = "Load Folder", Cluster = this, Id = "load-folder", StyleIcon = "folder_add_regular", Order = 4 });

        NavigationItems.Add(new NavigationItem() { Name = "Workloads", Order = 8 });
        NavigationItems.Add(new NavigationItem() { Name = "Configuration", Order = 9 });

        var networkNavItem = new NavigationItem() { Name = "Network", Order = 10 };

        NavigationItems.Add(networkNavItem);
        NavigationItems.Add(new NavigationItem() { Name = "Storage", Order = 11 });
        NavigationItems.Add(new NavigationItem() { Name = "Access Control", Order = 12 });

        foreach (var config in ResourceConfigs)
        {
            if (await UpdateCanIAnyNamespaceAsync(config.Value.Type, Verb.List) && await UpdateCanIAnyNamespaceAsync(config.Value.Type, Verb.Watch))
            {
                var nav = new ResourceNavigationLink() { Name = config.Value.Name, ControlType = config.Value.Type, Cluster = this, Order = config.Value.Order };

                if (config.Value.Type == typeof(V1Namespace))
                {
                    nav.Objects = Objects[config.Value.Kind].Items;
                }

                if (config.Value.Type == typeof(V1Pod))
                {
                    if (await UpdateCanIAnyNamespaceAsync<V1Pod>(Verb.Create, "portforward"))
                    {
                        Dispatcher.UIThread.Post(() => networkNavItem.NavigationItems.Add(new NavigationLink() { Name = Assets.Resources.PortForwarderListViewModel_Title, ControlType = typeof(PortForwarderListViewModel), Cluster = this, StyleIcon = "ic_fluent_cloud_flow_filled", Order = 6 }), DispatcherPriority.Background);
                    }
                }

                if (config.Value.Type == typeof(V1CustomResourceDefinition))
                {
                    _crdNavigationLink = nav;
                    nav.NavigationItems = new ObservableSortedCollection<NavigationItem>(new NavigationItemNameComparer());
#if !DEBUG
                    await Seed<V1CustomResourceDefinition>();
                    nav.Objects = Objects[config.Value.Kind].Items;
#endif
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

    public async Task Seed<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        _logger.LogInformation("Starting Seed: {type}", typeof(T));

        var type = typeof(T);
        var kind = GroupApiVersionKind.From<T>();

        ContainerClass container;

        await _seedLimiter.WaitAsync();

        if (Objects.TryGetValue(kind, out var container2))
        {
            container = container2;
        }
        else
        {
            container = new ContainerClass
            {
                Type = type,
                Items = new AvaloniaDictionary<NamespacedName, T>()
            };

            Objects.Add(kind, container);
        }

        if (!container.Initialized)
        {
            container.Initialized = true;
            _seedLimiter.Release();

            await _priorityExecutor.Enqueue(async ct =>
            {
                var resourceConfig = GetResourceConfig(kind);

                await resourceConfig.UpdatePermissions();

                if (CanI(type, Verb.List) && CanI(type, Verb.Watch))
                {
                    var informer = new ResourceInformer<T>(Client, _serviceProvider.GetRequiredService<IHostApplicationLifetime>(), _loggerFactory.CreateLogger<ResourceInformer<T>>());
                    container.Informers.Add(informer);
                    informer.Register(GetResourceInformerCallback<T>());
                    informer.StartWatching();
                    _ = Task.Run(() => informer.StartAsync(CancellationToken.None));
                }
                else
                {
                    if (!IsNamespaced<T>())
                    {
                        return;
                    }

                    var namespaceDict = await GetObjectDictionaryAsync<V1Namespace>();

                    foreach (var item in namespaceDict)
                    {
                        string ns = item.Value.Name();

                        if (CanI(type, Verb.List, ns) && CanI(type, Verb.Watch, ns))
                        {
                            var informer = new ResourceInformer<T>(Client, _serviceProvider.GetRequiredService<IHostApplicationLifetime>(), _loggerFactory.CreateLogger<ResourceInformer<T>>(), @namespace: ns);
                            container.Informers.Add(informer);
                            informer.Register(GetResourceInformerCallback<T>());
                            informer.StartWatching();
                            _ = Task.Run(() => informer.StartAsync(CancellationToken.None));
                        }
                    }
                }
            }, WorkPriority.Normal);
        }
        else
        {
            _seedLimiter.Release();
        }

        _logger.LogInformation("Finished Seed: {type}", typeof(T));
    }

    private ResourceInformerCallback<T> GetResourceInformerCallback<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        return new ResourceInformerCallback<T>((x, item) =>
        {
            var kind = GroupApiVersionKind.From<T>();

            var items = (AvaloniaDictionary<NamespacedName, T>)Objects[kind].Items;

            var name = new NamespacedName(item.Namespace(), item.Name());

            switch (x)
            {
                case WatchEventType.Added:
                    if (item is V1CustomResourceDefinition crd)
                    {
                        _logger.LogInformation("Processing new CRD {name}", crd.Name());

                        _priorityExecutor.Enqueue(async _ =>
                        {
                            await ProcessNewCRD(crd)
                            .ContinueWith(t =>
                            {
                                if (t.Exception != null)
                                {
                                    _logger.LogError("Exception in ProcessNewCRD: {exception}", t.Exception.Flatten());
                                }
                                else
                                {
                                    Dispatcher.UIThread.Post(() => items[name] = item, DispatcherPriority.Background);
                                }
                                _logger.LogInformation("Completed processing new CRD {name}", crd.Name());
                            });
                        }, WorkPriority.Low).GetAwaiter().GetResult();
                    }
                    else
                    {
                        Dispatcher.UIThread.Post(() => items[name] = item, DispatcherPriority.Background);
                    }

                    break;
                case WatchEventType.Modified:
                    Dispatcher.UIThread.Post(() => items[name] = item, DispatcherPriority.Background);
                    break;
                case WatchEventType.Deleted:
                    Dispatcher.UIThread.Post(() => items.Remove(name), DispatcherPriority.Background);
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

            OnChange?.Invoke(x, kind, item);
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

            var genericMethod = s_processCustomObjectMethod.MakeGenericMethod(resourceType);
            await (Task)genericMethod.Invoke(this, [crd]);
        }

        return true;
    }

    private static readonly MethodInfo s_processCustomObjectMethod = typeof(Cluster).GetMethod(nameof(ProcessCustomObject), BindingFlags.Instance | BindingFlags.NonPublic);

    private async Task ProcessCustomObject<T>(V1CustomResourceDefinition crd) where T : class, IKubernetesObject<V1ObjectMeta>, new()
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

            //await resourceConfig.UpdatePermissions();

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

    public async Task Delete<T>(T item) where T : class, IKubernetesObject<V1ObjectMeta>, new()
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

    public async Task<T?> GetObjectAsync<T>(string @namespace, string name) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        await Seed<T>();

        var attribute = GroupApiVersionKind.From<T>();

        return ((AvaloniaDictionary<NamespacedName, T>)Objects[attribute].Items)[new NamespacedName(@namespace, name)];
    }

    public AvaloniaDictionary<NamespacedName, T> GetObjectDictionary<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        Seed<T>().GetAwaiter().GetResult();

        var attribute = GroupApiVersionKind.From<T>();

        return (AvaloniaDictionary<NamespacedName, T>)Objects[attribute].Items;
    }

    public async Task<AvaloniaDictionary<NamespacedName, T>> GetObjectDictionaryAsync<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        await Seed<T>();

        var attribute = GroupApiVersionKind.From<T>();

        return (AvaloniaDictionary<NamespacedName, T>)Objects[attribute].Items;
    }

    public async Task AddOrUpdate<T>(T item) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        var api = GroupApiVersionKind.From<T>();

        using var client = new GenericClient(Client, api.Group, api.ApiVersion, api.PluralName, false);

        if (IsNamespaced<T>() && string.IsNullOrEmpty(item.Namespace()))
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
        var mi = GetType().GetMethods().First(x => x.Name == nameof(AddOrUpdate) && x.IsGenericMethod && x.GetParameters().Length == 1);

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

    public bool IsNamespaced(Type type)
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

    public bool IsNamespaced<T>()
    {
        return IsNamespaced(typeof(T));
    }

    private async Task<V2beta1APIGroupDiscoveryList> GetAPIGroupDiscoveryList(bool native = true)
    {
        var mi = typeof(Kubernetes).GetMethod("SendRequest", BindingFlags.NonPublic | BindingFlags.Instance);

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
}

public partial class ContainerClass : ObservableObject
{
    [ObservableProperty]
    public partial Type Type { get; set; }

    [ObservableProperty]
    public partial List<IResourceInformer> Informers { get; set; } = [];

    [ObservableProperty]
    public partial IDictionary Items { get; set; }

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
