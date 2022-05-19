using k8s;
using k8s.Models;
using KubeCRDGenerator;

namespace KubeUI.Core.Client;

public class GitOpsCluster : ClusterBase, ICluster
{
    public string Path { get; set; }

    public GitOpsCluster(ICRDGenerator cRDGenerator) : base(cRDGenerator)
    {

        this.OnChange += Cluster_OnChange;
    }

    private void Cluster_OnChange(WatchEventType eventType, GroupApiVersionKind type, IKubernetesObject<V1ObjectMeta> item)
    {
        switch (eventType)
        {
            case WatchEventType.Added:
                if (item is V1CustomResourceDefinition)
                {
                    Task.Run(() => base.GenerateCRDAssembly((V1CustomResourceDefinition)item));
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
        DeleteObject(item);

        return Task.CompletedTask;
    }

    public override void Seed<T>(string version, string kind, string group = "")
    {
    }

    public Task<V1APIGroupList> GetAPIs()
    {
        throw new NotImplementedException();
    }

    public Task<KubeVersion> GetVersion()
    {
        return Task.FromResult(new KubeVersion());
    }
}
