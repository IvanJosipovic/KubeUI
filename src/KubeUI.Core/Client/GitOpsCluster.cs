using k8s;
using k8s.Models;
using KubeCRDGenerator;

namespace KubeUI.Core.Client;

public class GitOpsCluster : ClusterBase, ICluster
{
    public string Path { get; set; }

    public bool IsConnected { get; set; } = true;

    public GitOpsCluster(ICRDGenerator cRDGenerator) : base(cRDGenerator)
    {
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
