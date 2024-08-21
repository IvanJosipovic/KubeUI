using System.Collections.Concurrent;
using System.Text.Json;
using System.Xml;
using Dock.Model.Controls;
using Dock.Model.Core;
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

[ServiceDescriptor<Cluster>(ServiceLifetime.Transient)]
public sealed partial class Cluster : ObservableObject
{
    private ILoggerFactory _loggerFactory;

    private ILogger<Cluster> _logger;

    [ObservableProperty]
    public string _name;

    [ObservableProperty]
    private string _kubeConfigPath;

    [ObservableProperty]
    private K8SConfiguration _kubeConfig;

    [ObservableProperty]
    private bool _connected;

    [ObservableProperty]
    private IKubernetes? _client;

    [ObservableProperty]
    private ObservableCollection<NavigationItem> _navigationItems = [];

    [ObservableProperty]
    private ModelCache _modelCache;

    private IGenerator _generator;

    [ObservableProperty]
    private bool _isExpanded = true;

    [ObservableProperty]
    private ConcurrentObservableDictionary<NamespacedName, V1Namespace> _namespaces = [];

    [ObservableProperty]
    private ObservableCollection<V1Namespace> _selectedNamespaces = [];

    public V1APIGroupList APIGroups { get; private set; }

    public event Action<WatchEventType, GroupApiVersionKind, IKubernetesObject<V1ObjectMeta>>? OnChange;

    private readonly SemaphoreSlim _semaphoreSlim = new(1);

    public ConcurrentDictionary<GroupApiVersionKind, ContainerClass> Objects { get; } = new();

    private ResourceNavigationLink _crdNavigationLink;

    public Cluster(ILogger<Cluster> logger, ILoggerFactory loggerFactory, ModelCache modelCache, IGenerator generator)
    {
        _loggerFactory = loggerFactory;
        _logger = logger;
        _modelCache = modelCache;
        _generator = generator;

        var kubeAssemblyXmlDoc = new XmlDocument();
        kubeAssemblyXmlDoc.Load(typeof(Generator).Assembly.GetManifestResourceStream("runtime.KubernetesClient.xml"));

        _modelCache.AddToCache(typeof(V1Deployment).Assembly, kubeAssemblyXmlDoc);
    }

    private KubernetesClientConfiguration _getClientConfiguration()
    {
        if (KubeConfig != null)
        {
            return KubernetesClientConfiguration.BuildConfigFromConfigObject(KubeConfig, Name);
        }
        else
        {
            return KubernetesClientConfiguration.BuildConfigFromConfigFile(KubeConfigPath, Name);
        }
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
                    Client = new Kubernetes(_getClientConfiguration());

                    APIGroups = await Client.Apis.GetAPIVersionsAsync();

                    AddDefaultNavigation();

                    Connected = true;

                    Namespaces = GetObjectDictionary<V1Namespace>();

                    if (IsMetricsAvailable)
                    {
                        _metricsRefreshTimer =  new(TimeSpan.FromSeconds(30), DispatcherPriority.Background, SyncData);
                        _metricsRefreshTimer.Start();
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError("Error connecting to {name} - {ex}", Name, ex);

                    var factory = Application.Current.GetRequiredService<IFactory>();

                    var doc = factory.GetDockable<IDocumentDock>("Documents");

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
        finally
        {
            _semaphoreSlim.Release();
        }
    }

    private void AddDefaultNavigation()
    {
        NavigationItems.Add(new NavigationLink() { Name = Resources.ClusterViewModel_Title, ControlType = typeof(ClusterViewModel), Cluster = this, SvgIcon = "/Assets/kube/infrastructure_components/unlabeled/control-plane.svg" });
        NavigationItems.Add(new NavigationLink() { Name = Resources.VisualizationViewModel_Title, ControlType = typeof(VisualizationViewModel), Cluster = this, SvgIcon = "/Assets/kube/infrastructure_components/unlabeled/control-plane.svg" });
        NavigationItems.Add(new NavigationLink() { Name = "Load Yaml", Cluster = this, Id = "load-yaml", StyleIcon = "arrow_upload_regular" });
        NavigationItems.Add(new NavigationLink() { Name = "Load Folder", Cluster = this, Id = "load-folder", StyleIcon = "folder_add_regular" });

        NavigationItems.Add(new ResourceNavigationLink() { Name = "Nodes", ControlType = typeof(V1Node), Cluster = this, SvgIcon = "/Assets/kube/infrastructure_components/unlabeled/node.svg" });
        NavigationItems.Add(new ResourceNavigationLink() { Name = "Namespaces", ControlType = typeof(V1Namespace), Cluster = this, SvgIcon = "/Assets/kube/resources/unlabeled/ns.svg" });
        NavigationItems.Add(new ResourceNavigationLink() { Name = "Events", ControlType = typeof(Corev1Event), Cluster = this, SvgIcon = "/Assets/kube/infrastructure_components/unlabeled/etcd.svg" });

        NavigationItems.Add(new NavigationItem()
        {
            Name = "Workloads",
            NavigationItems = [
                new ResourceNavigationLink() { Name = "Pods", ControlType = typeof(V1Pod), Cluster = this, SvgIcon = "/Assets/kube/resources/unlabeled/pod.svg" },
                new ResourceNavigationLink() { Name = "Deployments", ControlType = typeof(V1Deployment), Cluster = this, SvgIcon = "/Assets/kube/resources/unlabeled/deploy.svg" },
                new ResourceNavigationLink() { Name = "Daemon Sets", ControlType = typeof(V1DaemonSet), Cluster = this, SvgIcon = "/Assets/kube/resources/unlabeled/ds.svg" },
                new ResourceNavigationLink() { Name = "Stateful Sets", ControlType = typeof(V1StatefulSet), Cluster = this, SvgIcon = "/Assets/kube/resources/unlabeled/sts.svg" },
                new ResourceNavigationLink() { Name = "Replica Sets", ControlType = typeof(V1ReplicaSet), Cluster = this, SvgIcon = "/Assets/kube/resources/unlabeled/rs.svg" },
                new ResourceNavigationLink() { Name = "Replication Controllers", ControlType = typeof(V1ReplicationController), Cluster = this, SvgIcon = "/Assets/kube/resources/unlabeled/rs.svg" }, // Needs SvgIcon
                new ResourceNavigationLink() { Name = "Jobs", ControlType = typeof(V1Job), Cluster = this, SvgIcon = "/Assets/kube/resources/unlabeled/job.svg" },
                new ResourceNavigationLink() { Name = "Cron Jobs", ControlType = typeof(V1CronJob), Cluster = this, SvgIcon = "/Assets/kube/resources/unlabeled/cronjob.svg" },
            ]
        });

        NavigationItems.Add(new NavigationItem()
        {
            Name = "Configuration",
            NavigationItems = [
                new ResourceNavigationLink() { Name = "Config Maps", ControlType = typeof(V1ConfigMap), Cluster = this, SvgIcon = "/Assets/kube/resources/unlabeled/cm.svg" },
                new ResourceNavigationLink() { Name = "Secrets", ControlType = typeof(V1Secret), Cluster = this, SvgIcon = "/Assets/kube/resources/unlabeled/secret.svg" },
                new ResourceNavigationLink() { Name = "Resource Quotas", ControlType = typeof(V1ResourceQuota), Cluster = this, SvgIcon = "/Assets/kube/resources/unlabeled/quota.svg" },
                new ResourceNavigationLink() { Name = "Limit Ranges", ControlType = typeof(V1LimitRange), Cluster = this, SvgIcon = "/Assets/kube/resources/unlabeled/limits.svg" },
                new ResourceNavigationLink() { Name = "Horizontal Pod Auto Scalers", ControlType = typeof(V1HorizontalPodAutoscaler), Cluster = this, SvgIcon = "/Assets/kube/resources/unlabeled/hpa.svg" },
                new ResourceNavigationLink() { Name = "Pod Disruption Budget", ControlType = typeof(V1PodDisruptionBudget), Cluster = this, SvgIcon = "/Assets/kube/resources/unlabeled/job.svg" }, // Needs SvgIcon
                new ResourceNavigationLink() { Name = "Priority Classes", ControlType = typeof(V1PriorityClass), Cluster = this, SvgIcon = "/Assets/kube/resources/unlabeled/job.svg" }, // Needs SvgIcon
                new ResourceNavigationLink() { Name = "Runtime Classes", ControlType = typeof(V1RuntimeClass), Cluster = this, SvgIcon = "/Assets/kube/resources/unlabeled/job.svg" }, // Needs SvgIcon
                new ResourceNavigationLink() { Name = "Leases", ControlType = typeof(V1Lease), Cluster = this, SvgIcon = "/Assets/kube/resources/unlabeled/job.svg" }, // Needs SvgIcon
                new ResourceNavigationLink() { Name = "Mutating Webhook Configs", ControlType = typeof(V1MutatingWebhookConfiguration), Cluster = this, SvgIcon = "/Assets/kube/resources/unlabeled/netpol.svg" }, // Needs SvgIcon
                new ResourceNavigationLink() { Name = "Validating Webhook Configs", ControlType = typeof(V1ValidatingWebhookConfiguration), Cluster = this, SvgIcon = "/Assets/kube/resources/unlabeled/netpol.svg" }, // Needs SvgIcon
            ]
        });

        NavigationItems.Add(new NavigationItem()
        {
            Name = "Network",
            NavigationItems = [
                new ResourceNavigationLink() { Name = "Services", ControlType = typeof(V1Service), Cluster = this, SvgIcon = "/Assets/kube/resources/unlabeled/sc.svg" },
                new ResourceNavigationLink() { Name = "Endpoints", ControlType = typeof(V1Endpoints), Cluster = this, SvgIcon = "/Assets/kube/resources/unlabeled/ep.svg"  },
                new ResourceNavigationLink() { Name = "Endpoint Slices", ControlType = typeof(V1EndpointSlice), Cluster = this, SvgIcon = "/Assets/kube/resources/unlabeled/ep.svg"  },
                new ResourceNavigationLink() { Name = "Ingresses", ControlType = typeof(V1Ingress), Cluster = this, SvgIcon = "/Assets/kube/resources/unlabeled/ing.svg"  },
                new ResourceNavigationLink() { Name = "Ingress Classes", ControlType = typeof(V1IngressClass), Cluster = this, SvgIcon = "/Assets/kube/resources/unlabeled/ing.svg"  }, // Needs SvgIcon
                new ResourceNavigationLink() { Name = "Network Policies", ControlType = typeof(V1NetworkPolicy), Cluster = this, SvgIcon = "/Assets/kube/resources/unlabeled/netpol.svg"  },
                new NavigationLink() { Name = Resources.PortForwarderListViewModel_Title, ControlType = typeof(PortForwarderListViewModel), Cluster = this, StyleIcon = "ic_fluent_cloud_flow_filled" }
            ]
        });

        NavigationItems.Add(new NavigationItem()
        {
            Name = "Storage",
            NavigationItems = [
                new ResourceNavigationLink() { Name = "Persistent Volume Claims", ControlType = typeof(V1PersistentVolumeClaim), Cluster = this, SvgIcon = "/Assets/kube/resources/unlabeled/pvc.svg" },
                new ResourceNavigationLink() { Name = "Persistent Volumes", ControlType = typeof(V1PersistentVolume), Cluster = this, SvgIcon = "/Assets/kube/resources/unlabeled/pv.svg" },
                new ResourceNavigationLink() { Name = "Storage Classes", ControlType = typeof(V1StorageClass), Cluster = this, SvgIcon = "/Assets/kube/resources/unlabeled/sc.svg" },
            ]
        });

        NavigationItems.Add(new NavigationItem()
        {
            Name = "Access Control",
            NavigationItems = [
                new ResourceNavigationLink() { Name = "Service Accounts", ControlType = typeof(V1ServiceAccount), Cluster = this, SvgIcon = "/Assets/kube/resources/unlabeled/sa.svg" },
                new ResourceNavigationLink() { Name = "Cluster Roles", ControlType = typeof(V1ClusterRole), Cluster = this, SvgIcon = "/Assets/kube/resources/unlabeled/c-role.svg" },
                new ResourceNavigationLink() { Name = "Roles", ControlType = typeof(V1Role), Cluster = this, SvgIcon = "/Assets/kube/resources/unlabeled/role.svg" },
                new ResourceNavigationLink() { Name = "Cluster Role Bindings", ControlType = typeof(V1ClusterRoleBinding), Cluster = this, SvgIcon = "/Assets/kube/resources/unlabeled/crb.svg" },
                new ResourceNavigationLink() { Name = "Role Bindings", ControlType = typeof(V1RoleBinding), Cluster = this, SvgIcon = "/Assets/kube/resources/unlabeled/rb.svg" },
            ]
        });

        _crdNavigationLink = new ResourceNavigationLink
        {
            Name = "Custom Resource Definitions",
            ControlType = typeof(V1CustomResourceDefinition),
            Cluster = this,
            SvgIcon = "/Assets/kube/resources/unlabeled/crd.svg",
            NavigationItems = new ObservableSortedCollection<NavigationItem>(new NavigationItemComparer())
        };

        NavigationItems.Add(_crdNavigationLink);
    }

    public void Seed<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        if (!Connected)
        {
            Task.Run(Connect).GetAwaiter().GetResult();
        }

        var type = GroupApiVersionKind.From<T>();

        if (!Objects.TryGetValue(type, out var container))
        {
            container = new ContainerClass
            {
                Type = typeof(T),
                Items = new ConcurrentObservableDictionary<NamespacedName, T>()
            };

            Objects.TryAdd(type, container);
        }

        if (container.Informer == null)
        {
            var informer = new ResourceInformer<T>(_loggerFactory.CreateLogger<ResourceInformer<T>>(), Client);

            container.Informer = informer;

            informer.Register(new ResourceInformerCallback<T>((x, item) =>
            {
                var items = (ConcurrentObservableDictionary<NamespacedName, T>)(Objects[type].Items);

                var name = new NamespacedName(item.Namespace(), item.Name());

                switch (x)
                {
                    case WatchEventType.Added:
                        if (typeof(T) == typeof(V1CustomResourceDefinition))
                        {
                            var crd = (item as V1CustomResourceDefinition);

                            var assembly = _generator.GenerateAssembly(crd, "KubeUI.Models");

                            if (assembly.Item1 == null || assembly.Item2 == null)
                            {
                                _logger.LogWarning("Unable to generate CRD for {name}", name);

                                return;
                            }

                            ModelCache.AddToCache(assembly.Item1, assembly.Item2);

                            var version = crd.Spec.Versions.First(x => x.Served && x.Storage);

                            var type = ModelCache.GetResourceType(crd.Spec.Group, version.Name, crd.Spec.Names.Kind);

                            var nav = new ResourceNavigationLink() { Name = crd.Spec.Names.Kind, ControlType = type, Cluster = this, NavigationItems = new ObservableSortedCollection<NavigationItem>(new NavigationItemComparer()), SvgIcon = "/Assets/kube/resources/unlabeled/crd.svg" };

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
                                    navItem = new NavigationItem() { Name = fqdn };
                                    list.Add(navItem);
                                    list = navItem.NavigationItems;
                                }
                            }

                            navItem!.NavigationItems.Add(nav);
                        }

                        Dispatcher.UIThread.Post(() => items[name] = item);
                        break;
                    case WatchEventType.Modified:
                        Dispatcher.UIThread.Post(() => items[name] = item);
                        break;
                    case WatchEventType.Deleted:
                        Dispatcher.UIThread.Post(() => items.Remove(name));
                        //todo Check if CRD and remove from menu etc
                        break;
                    case WatchEventType.Error:
                        break;
                    case WatchEventType.Bookmark:
                        break;
                }

                OnChange?.Invoke(x, type, item);
            }));

            _ = Task.Run(() => informer.RunAsync(new CancellationToken()));
        }
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

    public async Task<bool> Delete<T>(T item) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        var api = GroupApiVersionKind.From<T>();

        using var client = new GenericClient(Client, api.Group, api.ApiVersion, api.PluralName, false);

        try
        {
            if (string.IsNullOrEmpty(item.Namespace()))
            {
                await client.DeleteAsync<T>(item.Name());
            }
            else
            {
                await client.DeleteNamespacedAsync<T>(item.Namespace(), item.Name());
            }

            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to delete");
        }

        return false;
    }

    public async Task<bool> CanDelete<T>(T item) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        var kind = GroupApiVersionKind.From<T>();

        var model = new V1SelfSubjectAccessReview()
        {
            ApiVersion = kind.GroupApiVersion,
            Kind = V1SelfSubjectAccessReview.KubeKind,
            Spec = new()
            {
                ResourceAttributes = new()
                {
                    Group = kind.Group,
                    NamespaceProperty = item.Namespace(),
                    Resource = kind.PluralName,
                    Verb = "delete"
                }
            }
        };

        var kube = Client as Kubernetes;

        var resp = await kube.CreateSelfSubjectAccessReviewAsync(model);

        return resp.Status.Allowed;
    }

    public T? GetObject<T>(string @namespace, string name) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        Seed<T>();

        var attribute = GroupApiVersionKind.From<T>();

        return ((ConcurrentObservableDictionary<NamespacedName,T>)Objects[attribute].Items)[new NamespacedName(@namespace, name)];
    }

    public ConcurrentObservableDictionary<NamespacedName, T> GetObjectDictionary<T>() where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        Seed<T>();

        var attribute = GroupApiVersionKind.From<T>();

        return (ConcurrentObservableDictionary<NamespacedName, T>)Objects[attribute].Items;
    }

    public async Task AddOrUpdate<T>(T item) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        var api = GroupApiVersionKind.From<T>();

        using var client = new GenericClient(Client, api.Group, api.ApiVersion, api.PluralName, false);

        try
        {
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
        catch (JsonException ex)
        {
            _logger.LogError(ex, "Failed to AddOrUpdate");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to AddOrUpdate");
            throw;
        }
    }

    public void ImportYaml(Stream stream)
    {
        var mi = GetType().GetMethods().First(x => x.Name == nameof(AddOrUpdate) && x.IsGenericMethod && x.GetParameters().Length == 1);

        var reader = new StreamReader(stream);
        var parser = new Parser(new StringReader(reader.ReadToEnd()));
        parser.Consume<StreamStart>();

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
                    _logger.LogWarning("Unable to find Type for {kind}", obj.ApiVersion + "/" + obj.Kind);

                    continue;
                }

                var model = Serialization.KubernetesYaml.Deserializer.Deserialize(yaml, type);

                if (model != null)
                {
                    var fooRef = mi.MakeGenericMethod(type);
                    fooRef.Invoke(this, [model]);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error Deserializing {kind}", obj.ApiVersion + "/" + obj.Kind);
            }
        }
    }

    public void ImportFolder(string path)
    {
        if (Directory.Exists(path))
        {
            var files = new DirectoryInfo(path)
                .EnumerateFiles("*", SearchOption.AllDirectories)
                .Where(fi => fi.Extension.Equals(".yaml", StringComparison.OrdinalIgnoreCase) || fi.Extension.Equals(".yml", StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var file in files)
            {
                try
                {
                    ImportYaml(file.OpenRead());
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error parsing Yaml {filename}", file.FullName);
                }
            }
        }
    }
}

public partial class ContainerClass : ObservableObject
{
    [ObservableProperty]
    private Type _type;

    [ObservableProperty]
    private IResourceInformer _informer;

    [ObservableProperty]
    private ICollection _items;
}
