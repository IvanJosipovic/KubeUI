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
        OnChange += Cluster_OnChange;
    }

    private void Cluster_OnChange(WatchEventType eventType, GroupApiVersionKind type, IKubernetesObject<V1ObjectMeta> item)
    {
        switch (eventType)
        {
            case WatchEventType.Added:
                if (item is V1CustomResourceDefinition)
                {
                    Task.Run(() => GenerateCRDAssembly((V1CustomResourceDefinition)item));
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
        var key = item.ApiVersion.ToLower() + "/" + item.Kind.ToLower();

        if (!Objects.ContainsKey(key))
        {
            Objects[key] = new ConcurrentDictionary<string, IKubernetesObject<V1ObjectMeta>>();
        }

        var exists = Objects[key].ContainsKey($"{item.Namespace()}|{item.Name()}");

        Objects[key][$"{item.Namespace()}|{item.Name()}"] = item;

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
