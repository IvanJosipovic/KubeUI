using k8s.KubeConfigModels;
using KubernetesCRDModelGen;
using System.Collections.Concurrent;

namespace KubeUI.Core.Client;

public class Cluster : ClusterBase, ICluster
{
    private ILoggerFactory loggerFactory;

    public string KubeConfigPath { get; set; }

    public K8SConfiguration KubeConfig { get; set; }

    public IKubernetes? Client;

    private KubernetesClientConfiguration GetClientConfiguration()
    {
        if (!string.IsNullOrEmpty(KubeConfigPath))
        {
            return KubernetesClientConfiguration.BuildConfigFromConfigFile(KubeConfigPath, Name);
        }
        else
        {
            return KubernetesClientConfiguration.BuildConfigFromConfigObject(KubeConfig, Name);
        }

        throw new Exception("Unable to find KubeConfig");
    }

    private readonly ConcurrentDictionary<string, object> informers = new();

    public Cluster(ILoggerFactory loggerFactory, ICRDGenerator cRDGenerator) : base(loggerFactory.CreateLogger<ClusterBase>(), cRDGenerator)
    {
        this.loggerFactory = loggerFactory;
    }

    private void Init()
    {
        if (Client == null)
        {
            Client = new Kubernetes(GetClientConfiguration());

            OnChange += Cluster_OnChange;
        }
    }

    private void Cluster_OnChange(WatchEventType eventType, GroupApiVersionKind type, IKubernetesObject<V1ObjectMeta> item)
    {
        switch (eventType)
        {
            case WatchEventType.Added:
                if (item is V1CustomResourceDefinition v1CustomResourceDefinition)
                {
                    Task.Run(async () =>
                    {
                        var assembly = await GenerateCRDAssembly(v1CustomResourceDefinition).ConfigureAwait(false);

                        Seed(assembly);
                    });
                }
                break;
            case WatchEventType.Modified:
                break;
            case WatchEventType.Deleted:
                break;
            case WatchEventType.Error:
                break;
            case WatchEventType.Bookmark:
                break;
            default:
                break;
        }
    }

    public override void Seed<T>(string version, string kind, string group = "")
    {
        var key = $"{group}/{version}/{kind}".TrimStart('/');

        if (!informers.ContainsKey(key))
        {
            Init();

            var informer = new ResourceInformer<T>(loggerFactory.CreateLogger<ResourceInformer<T>>(), Client);

            if (informers.TryAdd(key, informer))
            {
                informer.Register(new ResourceInformerCallback<T>((x, y) =>
                {
                    switch (x)
                    {
                        case WatchEventType.Added:
                            AddInternalObject(y);
                            break;
                        case WatchEventType.Modified:
                            UpdateInternalObject(y);
                            break;
                        case WatchEventType.Deleted:
                            DeleteInternalObject(y);
                            break;
                        case WatchEventType.Error:
                            break;
                        case WatchEventType.Bookmark:
                            break;
                        default:
                            break;
                    }
                    //_logger.LogInformation("{type} - {type} - {name}", typeof(V1Pod), x, y.Metadata.Name);
                }));

#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                informer.RunAsync(new CancellationToken());
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
            }
        }
    }

    public async Task Delete<T>(T item) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        var api = GroupApiVersionKind.From<T>();

        using var client = new GenericClient(Client, api.Group, api.ApiVersion, api.PluralName);

        if (string.IsNullOrEmpty(item.Namespace()))
        {
            await client.DeleteAsync<T>(item.Name());
        }
        else
        {
            await client.DeleteNamespacedAsync<T>(item.Namespace(), item.Name());
        }
    }

    public async Task<V1APIGroupList> GetAPIs()
    {
        Init();

        return await Client.Apis.GetAPIVersionsAsync();
    }

    public async Task<VersionInfo> GetVersion()
    {
        Init();

        return await Client.Version.GetCodeAsync();
    }

    public override async Task AddOrUpdate<T>(T item)
    {
        var api = GroupApiVersionKind.From<T>();

        using var client = new GenericClient(Client, api.Group, api.ApiVersion, api.PluralName);

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
}
