using KubernetesCRDModelGen;
using System.Collections.Concurrent;
using System.Text.Json;

namespace KubeUI.Core.Client;

public class Cluster : ClusterBase, ICluster
{
    private ILoggerFactory loggerFactory;

    public string KubeConfigPath { get; set; }

    public KubernetesClientConfiguration KubeConfig { get; set; }

    private KubernetesClientConfiguration? KubernetesClientConfiguration;

    public IKubernetes? Client;

    private KubernetesClientConfiguration GetClientConfiguration()
    {
        if (KubernetesClientConfiguration != null)
        {
            return KubernetesClientConfiguration;
        }

        if (!string.IsNullOrEmpty(KubeConfigPath))
        {
            KubernetesClientConfiguration = KubernetesClientConfiguration.BuildConfigFromConfigFile(KubeConfigPath, Name);
        }
        else
        {
            KubernetesClientConfiguration = KubeConfig;
        }

        return KubernetesClientConfiguration;
    }

    private ConcurrentDictionary<string, object> informers = new ConcurrentDictionary<string, object>();

    public Cluster(ILoggerFactory loggerFactory, ICRDGenerator cRDGenerator) : base(loggerFactory.CreateLogger<ClusterBase>(), cRDGenerator)
    {
        this.loggerFactory = loggerFactory;
        this.IsConnected = false;
    }

    private void Init()
    {
        if (Client == null)
        {
            Client = new Kubernetes(GetClientConfiguration());

            OnChange += Cluster_OnChange;

            Seed<V1Namespace>();

            Seed<V1Deployment>();
            Seed<V1DaemonSet>();

            Seed<V1CustomResourceDefinition>();
        }
    }

    private void Cluster_OnChange(WatchEventType eventType, GroupApiVersionKind type, IKubernetesObject<V1ObjectMeta> item)
    {
        switch (eventType)
        {
            case WatchEventType.Added:
                if (item is V1CustomResourceDefinition)
                {
#pragma warning disable CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
                    base.GenerateCRDAssembly((V1CustomResourceDefinition)item);
#pragma warning restore CS4014 // Because this call is not awaited, execution of the current method continues before the call is completed
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
        var key = $"{group}-{version}-{kind}";

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
                            AddObject(y);
                            break;
                        case WatchEventType.Modified:
                            UpdateObject(y);
                            break;
                        case WatchEventType.Deleted:
                            DeleteObject(y);
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

        GenericClient client;

        if (string.IsNullOrEmpty(api.Group))
        {
            client = new GenericClient(Client, api.ApiVersion, api.PluralName);
        }
        else
        {
            client = new GenericClient(Client, api.Group, api.ApiVersion, api.PluralName);
        }

        using (client)
        {
            if (string.IsNullOrEmpty(item.Namespace()))
            {
                await client.DeleteAsync<T>(item.Name());
            }
            else
            {
                await client.DeleteNamespacedAsync<T>(item.Namespace(), item.Name());
            }
        }
    }

    public async Task<V1APIGroupList> GetAPIs()
    {
        Init();

        var resp = await ((Kubernetes)Client).SendRequestRaw(null, new HttpRequestMessage(HttpMethod.Get, Client.BaseUri + "apis"), CancellationToken.None);

        if (resp.IsSuccessStatusCode)
        {
            var obj = KubernetesJson.Deserialize<V1APIGroupList>(await resp.Content.ReadAsStringAsync());
            return obj;
        }
        else
        {
            return new V1APIGroupList();
        }
    }

    public async Task<VersionInfo> GetVersion()
    {
        Init();

        return await Client.Version.GetCodeAsync();
    }
}
