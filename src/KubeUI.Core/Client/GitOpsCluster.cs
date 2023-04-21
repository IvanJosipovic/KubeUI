using k8s.Models;
using KubernetesCRDModelGen;
using System.Collections.Concurrent;

namespace KubeUI.Core.Client;

public class GitOpsCluster : ClusterBase, ICluster
{
    private ILogger<GitOpsCluster> Logger { get; set; }

    public string Path { get; set; }

    public GitOpsCluster(ILogger<GitOpsCluster> logger, ICRDGenerator cRDGenerator) : base(logger, cRDGenerator)
    {
        Logger = logger;
        this.IsConnected = true;
        OnChange += Cluster_OnChange;
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
                        await GenerateCRDAssembly(v1CustomResourceDefinition).ConfigureAwait(false);
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

    public Task Delete<T>(T item) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        DeleteInternalObject(item);

        return Task.CompletedTask;
    }

    public override void Seed<T>(string version, string kind, string group = "")
    {
    }

    public Task<V1APIGroupList> GetAPIs()
    {
        throw new NotImplementedException();
    }

    public Task<VersionInfo> GetVersion()
    {
        return Task.FromResult(new VersionInfo());
    }

    public override Task AddOrUpdate<T>(T item)
    {
        var key = item.ApiVersion + "/" + item.Kind;

        if (!Objects.ContainsKey(key))
        {
            Objects[key] = new ConcurrentDictionary<string, IKubernetesObject<V1ObjectMeta>>();
        }

        var itemKey = $"{item.Namespace()}|{item.Name()}";

        var exists = Objects[key].ContainsKey(itemKey);

        Objects[key][itemKey] = item;

        if (exists)
        {
            base.NotifyStateChanged(WatchEventType.Modified, GroupApiVersionKind.From(item.GetType()), item);
        }
        else
        {
            base.NotifyStateChanged(WatchEventType.Added, GroupApiVersionKind.From(item.GetType()), item);
        }

        return Task.CompletedTask;
    }
}
