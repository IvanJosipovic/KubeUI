using System.Collections.Concurrent;
using System.Net.Http.Json;
using System.Reflection;
using System.Text.Json;
using System.Xml;
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
using KubeUI.Client.Informer;
using Scrutor;
using Swordfish.NET.Collections;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace KubeUI.Client;

[ServiceDescriptor<ICluster>(ServiceLifetime.Transient)]
public sealed partial class Cluster : ObservableObject, ICluster
{
    private ILoggerFactory _loggerFactory;

    private ILogger<Cluster> _logger;

    private ISettingsService _settingsService;

    private IDialogService _dialogService;

    [ObservableProperty]
    public partial string Name { get; set; }

    [ObservableProperty]
    public partial string KubeConfigPath { get; set; }

    [ObservableProperty]
    public partial K8SConfiguration KubeConfig { get; set; }

    [ObservableProperty]
    public partial bool Connected { get; set; }

    [ObservableProperty]
    public partial IKubernetes? Client { get; set; }

    [ObservableProperty]
    public partial ObservableCollection<NavigationItem> NavigationItems { get; set; } = [];

    [ObservableProperty]
    public partial ModelCache ModelCache { get; set; }

    private IGenerator _generator;

    [ObservableProperty]
    public partial bool IsExpanded { get; set; }

    [ObservableProperty]
    public partial ConcurrentObservableDictionary<NamespacedName, V1Namespace> Namespaces { get; set; } = [];

    [ObservableProperty]
    public partial ObservableCollection<V1Namespace> SelectedNamespaces { get; set; } = [];
    public V2beta1APIGroupDiscoveryList NativeAPIGroupDiscoveryList { get; private set; }

    public V2beta1APIGroupDiscoveryList APIGroupDiscoveryList { get; private set; }

    public event Action<WatchEventType, GroupApiVersionKind, IKubernetesObject<V1ObjectMeta>>? OnChange;

    private readonly SemaphoreSlim _semaphoreSlim = new(1);

    public ConcurrentDictionary<GroupApiVersionKind, ContainerClass> Objects { get; } = new();

    private ResourceNavigationLink _crdNavigationLink;

    public Cluster(ILogger<Cluster> logger, ILoggerFactory loggerFactory, ModelCache modelCache, IGenerator generator, ISettingsService settingsService, IDialogService dialogService)
    {
        _loggerFactory = loggerFactory;
        _logger = logger;
        ModelCache = modelCache;
        _generator = generator;

        var kubeAssemblyXmlDoc = new XmlDocument();
        kubeAssemblyXmlDoc.Load(typeof(Generator).Assembly.GetManifestResourceStream("runtime.KubernetesClient.xml"));
        ModelCache.AddToCache(typeof(V1Deployment).Assembly, kubeAssemblyXmlDoc);
        _settingsService = settingsService;
        _dialogService = dialogService;
    }

    public async Task Connect()
    {
        await _semaphoreSlim.WaitAsync();

        try
        {
            if (!Connected)
            {
                try
                {
                    KubernetesClientConfiguration config;

                    if (string.IsNullOrEmpty(KubeConfigPath))
                    {
                        config =  KubernetesClientConfiguration.BuildConfigFromConfigObject(KubeConfig, Name);
                    }
                    else
                    {
                        config = KubernetesClientConfiguration.BuildConfigFromConfigFile(KubeConfigPath, Name);
                    }

                    Client = new Kubernetes(config);

                    NativeAPIGroupDiscoveryList = await GetAPIGroupDiscoveryList();

                    APIGroupDiscoveryList = await GetAPIGroupDiscoveryList(false);

                    Connected = true;

                    await GetPermissions();

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
                                Title = Resources.Cluster_Missing_Namespace_Permission_Title,
                                Content = Resources.Cluster_Missing_Namespace_Permission_Content,
                                PrimaryButtonText = Resources.Cluster_Missing_Namespace_Permission_Primary,
                                DefaultButton = ContentDialogButton.Primary
                            };

                            var result = await _dialogService.ShowContentDialogAsync(this, dialogSettings);

                            Connected = false;

                            return;
                        }

                        var items = GetObjectDictionary<V1Namespace>();

                        foreach (var item in settings.Namespaces)
                        {
                            items[new NamespacedName(item)] = new V1Namespace() { Metadata = new() { Name = item } };
                        }

                        Namespaces = items;
                    }
                    else
                    {
                        Namespaces = await GetObjectDictionaryAsync<V1Namespace>();

                        while (Namespaces.Count == 0)
                        {
                            await Task.Delay(100);
                        }
                    }

                    await InitMetrics();

                    await AddDefaultNavigation();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error connecting to {name}", Name);

                    Connected = false;

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
            _semaphoreSlim.Release();
        }
    }

    private async Task AddDefaultNavigation()
    {
        NavigationItems.Add(new NavigationLink() { Name = "Settings", ControlType = typeof(ClusterSettingsViewModel), Cluster = this, StyleIcon = "ic_fluent_settings_24_filled" });
        NavigationItems.Add(new NavigationLink() { Name = Resources.ClusterViewModel_Title, ControlType = typeof(ClusterViewModel), Cluster = this, SvgIcon = "/Assets/kube/infrastructure_components/unlabeled/control-plane.svg" });
        NavigationItems.Add(new NavigationLink() { Name = Resources.VisualizationViewModel_Title, ControlType = typeof(VisualizationViewModel), Cluster = this, StyleIcon = "ic_fluent_search_visual_24_filled" });
        NavigationItems.Add(new NavigationLink() { Name = "Load Yaml", Cluster = this, Id = "load-yaml", StyleIcon = "arrow_upload_regular" });
        NavigationItems.Add(new NavigationLink() { Name = "Load Folder", Cluster = this, Id = "load-folder", StyleIcon = "folder_add_regular" });

        if (await UpdateCanIListWatchAnyNamespaceAsync<V1Node>())
        {
            NavigationItems.Add(new ResourceNavigationLink() { Name = "Nodes", ControlType = typeof(V1Node), Cluster = this });
            await UpdateCanIAnyNamespaceAsync<V1Node>(Verb.Patch);
            await UpdateCanIAnyNamespaceAsync<V1Pod>(Verb.Create, "eviction");
        }
        if (ListNamespaces)
        {
            NavigationItems.Add(new ResourceNavigationLink() { Name = "Namespaces", ControlType = typeof(V1Namespace), Cluster = this });
        }
        if (await UpdateCanIListWatchAnyNamespaceAsync<Corev1Event>())
        {
            NavigationItems.Add(new ResourceNavigationLink() { Name = "Events", ControlType = typeof(Corev1Event), Cluster = this });
        }

        var workloads = new NavigationItem()
        {
            Name = "Workloads",
        };
        NavigationItems.Add(workloads);

        if (await UpdateCanIListWatchAnyNamespaceAsync<V1Pod>())
        {
            workloads.NavigationItems.Add(new ResourceNavigationLink() { Name = "Pods", ControlType = typeof(V1Pod), Cluster = this });
            await UpdateCanIAnyNamespaceAsync<V1Pod>(Verb.Get, "log");
            await UpdateCanIAnyNamespaceAsync<V1Pod>(Verb.Create, "exec");
            await UpdateCanIAnyNamespaceAsync<V1Pod>(Verb.Create, "portforward");
        }
        if (await UpdateCanIListWatchAnyNamespaceAsync<V1Deployment>())
        {
            workloads.NavigationItems.Add(new ResourceNavigationLink() { Name = "Deployments", ControlType = typeof(V1Deployment), Cluster = this });
        }
        if (await UpdateCanIListWatchAnyNamespaceAsync<V1DaemonSet>())
        {
            workloads.NavigationItems.Add(new ResourceNavigationLink() { Name = "Daemon Sets", ControlType = typeof(V1DaemonSet), Cluster = this });
        }
        if (await UpdateCanIListWatchAnyNamespaceAsync<V1StatefulSet>())
        {
            workloads.NavigationItems.Add(new ResourceNavigationLink() { Name = "Stateful Sets", ControlType = typeof(V1StatefulSet), Cluster = this });
        }
        if (await UpdateCanIListWatchAnyNamespaceAsync<V1ReplicaSet>())
        {
            workloads.NavigationItems.Add(new ResourceNavigationLink() { Name = "Replica Sets", ControlType = typeof(V1ReplicaSet), Cluster = this });
        }
        if (await UpdateCanIListWatchAnyNamespaceAsync<V1Job>())
        {
            workloads.NavigationItems.Add(new ResourceNavigationLink() { Name = "Jobs", ControlType = typeof(V1Job), Cluster = this });
        }
        if (await UpdateCanIListWatchAnyNamespaceAsync<V1CronJob>())
        {
            workloads.NavigationItems.Add(new ResourceNavigationLink() { Name = "Cron Jobs", ControlType = typeof(V1CronJob), Cluster = this });
        }

        var configuration = new NavigationItem()
        {
            Name = "Configuration"
        };
        NavigationItems.Add(configuration);

        if (await UpdateCanIListWatchAnyNamespaceAsync<V1ConfigMap>())
        {
            configuration.NavigationItems.Add(new ResourceNavigationLink() { Name = "Config Maps", ControlType = typeof(V1ConfigMap), Cluster = this });
        }
        if (await UpdateCanIListWatchAnyNamespaceAsync<V1Secret>())
        {
            configuration.NavigationItems.Add(new ResourceNavigationLink() { Name = "Secrets", ControlType = typeof(V1Secret), Cluster = this });
        }
        if (await UpdateCanIListWatchAnyNamespaceAsync<V1ResourceQuota>())
        {
            configuration.NavigationItems.Add(new ResourceNavigationLink() { Name = "Resource Quotas", ControlType = typeof(V1ResourceQuota), Cluster = this });
        }
        if (await UpdateCanIListWatchAnyNamespaceAsync<V1LimitRange>())
        {
            configuration.NavigationItems.Add(new ResourceNavigationLink() { Name = "Limit Ranges", ControlType = typeof(V1LimitRange), Cluster = this });
        }
        if (await UpdateCanIListWatchAnyNamespaceAsync<V2HorizontalPodAutoscaler>())
        {
            configuration.NavigationItems.Add(new ResourceNavigationLink() { Name = "Horizontal Pod Auto Scalers", ControlType = typeof(V2HorizontalPodAutoscaler), Cluster = this });
        }
        if (await UpdateCanIListWatchAnyNamespaceAsync<V1PodDisruptionBudget>())
        {
            configuration.NavigationItems.Add(new ResourceNavigationLink() { Name = "Pod Disruption Budget", ControlType = typeof(V1PodDisruptionBudget), Cluster = this }); // Needs SvgIcon
        }
        if (await UpdateCanIListWatchAnyNamespaceAsync<V1PriorityClass>())
        {
            configuration.NavigationItems.Add(new ResourceNavigationLink() { Name = "Priority Classes", ControlType = typeof(V1PriorityClass), Cluster = this }); // Needs SvgIcon
        }
        if (await UpdateCanIListWatchAnyNamespaceAsync<V1RuntimeClass>())
        {
            configuration.NavigationItems.Add(new ResourceNavigationLink() { Name = "Runtime Classes", ControlType = typeof(V1RuntimeClass), Cluster = this }); // Needs SvgIcon
        }
        if (await UpdateCanIListWatchAnyNamespaceAsync<V1Lease>())
        {
            configuration.NavigationItems.Add(new ResourceNavigationLink() { Name = "Leases", ControlType = typeof(V1Lease), Cluster = this });  // Needs SvgIcon
        }
        if (await UpdateCanIListWatchAnyNamespaceAsync<V1MutatingWebhookConfiguration>())
        {
            configuration.NavigationItems.Add(new ResourceNavigationLink() { Name = "Mutating Webhook Configs", ControlType = typeof(V1MutatingWebhookConfiguration), Cluster = this }); // Needs SvgIcon
        }
        if (await UpdateCanIListWatchAnyNamespaceAsync<V1ValidatingWebhookConfiguration>())
        {
            configuration.NavigationItems.Add(new ResourceNavigationLink() { Name = "Validating Webhook Configs", ControlType = typeof(V1ValidatingWebhookConfiguration), Cluster = this }); // Needs SvgIcon
        }

        var network = new NavigationItem()
        {
            Name = "Network"
        };
        NavigationItems.Add(network);

        if (await UpdateCanIListWatchAnyNamespaceAsync<V1Service>())
        {
            network.NavigationItems.Add(new ResourceNavigationLink() { Name = "Services", ControlType = typeof(V1Service), Cluster = this });
        }
        if (await UpdateCanIListWatchAnyNamespaceAsync<V1Endpoints>())
        {
            network.NavigationItems.Add(new ResourceNavigationLink() { Name = "Endpoints", ControlType = typeof(V1Endpoints), Cluster = this });
        }
        if (await UpdateCanIListWatchAnyNamespaceAsync<V1EndpointSlice>())
        {
            network.NavigationItems.Add(new ResourceNavigationLink() { Name = "Endpoint Slices", ControlType = typeof(V1EndpointSlice), Cluster = this });
        }
        if (await UpdateCanIListWatchAnyNamespaceAsync<V1Ingress>())
        {
            network.NavigationItems.Add(new ResourceNavigationLink() { Name = "Ingresses", ControlType = typeof(V1Ingress), Cluster = this });
        }
        if (await UpdateCanIListWatchAnyNamespaceAsync<V1IngressClass>())
        {
            network.NavigationItems.Add(new ResourceNavigationLink() { Name = "Ingress Classes", ControlType = typeof(V1IngressClass), Cluster = this }); // Needs SvgIcon
        }
        if (await UpdateCanIListWatchAnyNamespaceAsync<V1NetworkPolicy>())
        {
            network.NavigationItems.Add(new ResourceNavigationLink() { Name = "Network Policies", ControlType = typeof(V1NetworkPolicy), Cluster = this });
        }
        if (await UpdateCanIListWatchAnyNamespaceAsync<V1Pod>() && await UpdateCanIAnyNamespaceAsync<V1Pod>(Verb.Create, "portforward"))
        {
            network.NavigationItems.Add(new NavigationLink() { Name = Resources.PortForwarderListViewModel_Title, ControlType = typeof(PortForwarderListViewModel), Cluster = this, StyleIcon = "ic_fluent_cloud_flow_filled" });
        }

        var storage = new NavigationItem()
        {
            Name = "Storage"
        };
        NavigationItems.Add(storage);

        if (await UpdateCanIListWatchAnyNamespaceAsync<V1PersistentVolumeClaim>())
        {
            storage.NavigationItems.Add(new ResourceNavigationLink() { Name = "Persistent Volume Claims", ControlType = typeof(V1PersistentVolumeClaim), Cluster = this });
        }
        if (await UpdateCanIListWatchAnyNamespaceAsync<V1PersistentVolume>())
        {
            storage.NavigationItems.Add(new ResourceNavigationLink() { Name = "Persistent Volumes", ControlType = typeof(V1PersistentVolume), Cluster = this });
        }
        if (await UpdateCanIListWatchAnyNamespaceAsync<V1StorageClass>())
        {
            storage.NavigationItems.Add(new ResourceNavigationLink() { Name = "Storage Classes", ControlType = typeof(V1StorageClass), Cluster = this });
        }

        var accessControl = new NavigationItem()
        {
            Name = "Access Control"
        };
        NavigationItems.Add(accessControl);

        if (await UpdateCanIListWatchAnyNamespaceAsync<V1ServiceAccount>())
        {
            accessControl.NavigationItems.Add(new ResourceNavigationLink() { Name = "Service Accounts", ControlType = typeof(V1ServiceAccount), Cluster = this });
        }
        if (await UpdateCanIListWatchAnyNamespaceAsync<V1ClusterRole>())
        {
            accessControl.NavigationItems.Add(new ResourceNavigationLink() { Name = "Cluster Roles", ControlType = typeof(V1ClusterRole), Cluster = this });
        }
        if (await UpdateCanIListWatchAnyNamespaceAsync<V1Role>())
        {
            accessControl.NavigationItems.Add(new ResourceNavigationLink() { Name = "Roles", ControlType = typeof(V1Role), Cluster = this });
        }
        if (await UpdateCanIListWatchAnyNamespaceAsync<V1ClusterRoleBinding>())
        {
            accessControl.NavigationItems.Add(new ResourceNavigationLink() { Name = "Cluster Role Bindings", ControlType = typeof(V1ClusterRoleBinding), Cluster = this });
        }
        if (await UpdateCanIListWatchAnyNamespaceAsync<V1RoleBinding>())
        {
            accessControl.NavigationItems.Add(new ResourceNavigationLink() { Name = "Role Bindings", ControlType = typeof(V1RoleBinding), Cluster = this });
        }

        if (await UpdateCanIListWatchAnyNamespaceAsync<V1CustomResourceDefinition>())
        {
            _crdNavigationLink = new ResourceNavigationLink
            {
                Name = "Custom Resource Definitions",
                ControlType = typeof(V1CustomResourceDefinition),
                Cluster = this,
                NavigationItems = new ObservableSortedCollection<NavigationItem>(new NavigationItemComparer())
            };

            NavigationItems.Add(_crdNavigationLink);
        }
    }

    public async Task Seed<T>(bool waitForReady = false) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        var type = typeof(T);
        var kind = GroupApiVersionKind.From<T>();

        ContainerClass container;

        if (Objects.TryGetValue(kind, out var container2))
        {
            container = container2;
        }
        else
        {
            container = new ContainerClass
            {
                Type = type,
                Items = new ConcurrentObservableDictionary<NamespacedName, T>()
            };

            Objects.TryAdd(kind, container);
        }

        if (!container.Initialised)
        {
            container.Initialised = true;

            await GetSelfSubjectAccessReview(type, Verb.Create);
            await GetSelfSubjectAccessReview(type, Verb.Delete);
            await GetSelfSubjectAccessReview(type, Verb.Get);
            await GetSelfSubjectAccessReview(type, Verb.List);
            await GetSelfSubjectAccessReview(type, Verb.Patch);
            await GetSelfSubjectAccessReview(type, Verb.Update);
            await GetSelfSubjectAccessReview(type, Verb.Watch);

            if (CanI(type, Verb.List) && CanI(type, Verb.Watch))
            {
                var informer = new ResourceInformer<T>(_loggerFactory.CreateLogger<ResourceInformer<T>>(), Client);

                container.Informers.Add(informer);

                var inf = informer.Register(GetResourceInformerCallback<T>());

                _ = Task.Run(() => informer.RunAsync(new CancellationToken()));

                if (waitForReady)
                {
                    await inf.ReadyAsync(new CancellationToken());
                    await Task.Delay(1000);
                }
            }
            else
            {
                if (!IsNamespaced<T>())
                {
                    return;
                }

                foreach (var item in GetObjectDictionary<V1Namespace>())
                {
                    await GetSelfSubjectAccessReview(type, Verb.Create, item.Value.Name());
                    await GetSelfSubjectAccessReview(type, Verb.Delete, item.Value.Name());
                    await GetSelfSubjectAccessReview(type, Verb.Get, item.Value.Name());
                    await GetSelfSubjectAccessReview(type, Verb.List, item.Value.Name());
                    await GetSelfSubjectAccessReview(type, Verb.Patch, item.Value.Name());
                    await GetSelfSubjectAccessReview(type, Verb.Update, item.Value.Name());
                    await GetSelfSubjectAccessReview(type, Verb.Watch, item.Value.Name());

                    if (CanI(type, Verb.List, item.Value.Name()) && CanI(type, Verb.Watch, item.Value.Name()))
                    {
                        var informer = new ResourceInformer<T>(_loggerFactory.CreateLogger<ResourceInformer<T>>(), Client, item.Value.Name());

                        container.Informers.Add(informer);

                        var inf = informer.Register(GetResourceInformerCallback<T>());

                        _ = Task.Run(() => informer.RunAsync(new CancellationToken()));

                        if (waitForReady)
                        {
                            await inf.ReadyAsync(new CancellationToken());
                            await Task.Delay(1000);
                        }
                    }
                }
            }
        }
    }

    private ResourceInformerCallback<T> GetResourceInformerCallback<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        return new ResourceInformerCallback<T>((x, item) =>
        {
            var kind = GroupApiVersionKind.From<T>();

            var items = (ConcurrentObservableDictionary<NamespacedName, T>)Objects[kind].Items;

            var name = new NamespacedName(item.Namespace(), item.Name());

            switch (x)
            {
                case WatchEventType.Added:
                    if (typeof(T) == typeof(V1CustomResourceDefinition))
                    {
                        var crd = (item as V1CustomResourceDefinition);

                        if (!ModelCache.CheckIfCRDExists(crd))
                        {
                            var assembly = _generator.GenerateAssembly(crd, "KubeUI.Models");

                            if (assembly.Item1 == null || assembly.Item2 == null)
                            {
                                _logger.LogWarning("Unable to generate CRD for {name}", name);

                                return;
                            }

                            ModelCache.AddToCache(assembly.Item1, assembly.Item2);

                            var version = crd.Spec.Versions.First(x => x.Served && x.Storage);

                            var type = ModelCache.GetResourceType(crd.Spec.Group, version.Name, crd.Spec.Names.Kind);

                            APIGroupDiscoveryList = GetAPIGroupDiscoveryList(false).GetAwaiter().GetResult();

                            var task = UpdateCanListWatchAnyNamespaceAsync(type);

                            if (task.GetAwaiter().GetResult())
                            {
                                var nav = new ResourceNavigationLink() { Name = crd.Spec.Names.Kind.Humanize(LetterCasing.Title), ControlType = type, Cluster = this, NavigationItems = new ObservableSortedCollection<NavigationItem>(new NavigationItemComparer()) };

                                var group = crd.Spec.Group;

                                var fqdnlist = ConstructFQDNList(group);

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
                                        navItem = new NavigationItem() { Name = fqdn, NavigationItems = new ObservableSortedCollection<NavigationItem>(new NavigationItemComparer()) };
                                        list.Add(navItem);
                                        list = navItem.NavigationItems;
                                    }
                                }

                                navItem!.NavigationItems.Add(nav);
                            }
                        }
                    }

                    Dispatcher.UIThread.Post(() => items[name] = item, DispatcherPriority.Background);
                    break;
                case WatchEventType.Modified:
                    Dispatcher.UIThread.Post(() => items[name] = item, DispatcherPriority.Background);
                    break;
                case WatchEventType.Deleted:
                    Dispatcher.UIThread.Post(() => items.Remove(name), DispatcherPriority.Background);

                    if (typeof(T) == typeof(V1CustomResourceDefinition))
                    {
                        var crd = (item as V1CustomResourceDefinition);

                        APIGroupDiscoveryList = GetAPIGroupDiscoveryList(false).GetAwaiter().GetResult();

                        var title = Name = crd.Spec.Names.Kind.Humanize(LetterCasing.Title);

                        var group = crd.Spec.Group;

                        var fqdnlist = ConstructFQDNList(group);
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
                case WatchEventType.Error:
                    break;
                case WatchEventType.Bookmark:
                    break;
            }

            OnChange?.Invoke(x, kind, item);
        });
    }

    private static List<string> ConstructFQDNList(string domain)
    {
        List<string> fqdnList = new List<string>();
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

    public T? GetObject<T>(string @namespace, string name) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        _ = Seed<T>();

        var attribute = GroupApiVersionKind.From<T>();

        if (!Objects.TryGetValue(attribute, out var container))
        {
            container = new ContainerClass
            {
                Type = typeof(T),
                Items = new ConcurrentObservableDictionary<NamespacedName, T>()
            };

            Objects.TryAdd(attribute, container);
        }

        return ((ConcurrentObservableDictionary<NamespacedName, T>)Objects[attribute].Items)[new NamespacedName(@namespace, name)];
    }

    public async Task<T?> GetObjectAsync<T>(string @namespace, string name) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        await Seed<T>(true);

        var attribute = GroupApiVersionKind.From<T>();

        return ((ConcurrentObservableDictionary<NamespacedName, T>)Objects[attribute].Items)[new NamespacedName(@namespace, name)];
    }

    public ConcurrentObservableDictionary<NamespacedName, T> GetObjectDictionary<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        _ = Task.Run(() => Seed<T>());

        var attribute = GroupApiVersionKind.From<T>();

        if (!Objects.TryGetValue(attribute, out var container))
        {
            container = new ContainerClass
            {
                Type = typeof(T),
                Items = new ConcurrentObservableDictionary<NamespacedName, T>()
            };

            Objects.TryAdd(attribute, container);
        }

        return (ConcurrentObservableDictionary<NamespacedName, T>)Objects[attribute].Items;
    }

    public async Task<ConcurrentObservableDictionary<NamespacedName, T>> GetObjectDictionaryAsync<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        await Seed<T>(true);

        var attribute = GroupApiVersionKind.From<T>();

        return (ConcurrentObservableDictionary<NamespacedName, T>)Objects[attribute].Items;
    }

    public async Task AddOrUpdate<T>(T item) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        var api = GroupApiVersionKind.From<T>();

        using var client = new GenericClient(Client, api.Group, api.ApiVersion, api.PluralName, false);

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

    private bool IsNamespaced(Type type)
    {
        var api = GroupApiVersionKind.From(type);

        if (string.IsNullOrEmpty(api.Group))
        {
            var native = NativeAPIGroupDiscoveryList.items[0].versions.First(x => x.freshness == "Current").resources.FirstOrDefault(z =>
                z.resource == api.PluralName &&
                z.singularResource == api.Kind.ToLower() &&
                z.responseKind.kind == api.Kind
            );

            if (native != null)
            {
                return native.scope == "Namespaced";
            }
        }

        var group = APIGroupDiscoveryList.items.First(x => x.metadata.name == api.Group);

        var ver = group.versions.First(x => x.freshness == "Current" && x.version == api.ApiVersion);

        var ext = ver.resources.FirstOrDefault(z =>
            z.resource == api.PluralName &&
            z.singularResource == api.Kind.ToLower() &&
            z.responseKind.kind == api.Kind
        );

        return ext.scope == "Namespaced";
    }

    private bool IsNamespaced<T>()
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
}

public partial class ContainerClass : ObservableObject
{
    [ObservableProperty]
    public partial Type Type { get; set; }

    [ObservableProperty]
    public partial List<IResourceInformer> Informers { get; set; } = [];

    [ObservableProperty]
    public partial ICollection Items { get; set; }

    [ObservableProperty]
    public partial bool Initialised { get; set; }
}
