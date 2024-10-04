using Avalonia;
using k8s;
using k8s.KubeConfigModels;
using KubeUI.Client;

namespace KubeUI.Core.Tests;

public class TestHarness : IDisposable
{
    public string Name { get; set; } = Guid.NewGuid().ToString();

    public ICluster Cluster { get; set; }

    public ClusterManager ClusterManager { get; set; }

    public Kubernetes Kubernetes { get; set; }

    private Kind Kind { get; set; }

    public K8SConfiguration KubeConfig { get; set; }

    public async Task Initialize()
    {
        Kind = new Kind();
        await Kind.DownloadClient();
        await Kind.CreateCluster(Name);

        KubeConfig = await Kind.GetK8SConfiguration(Name);

        ClusterManager = Application.Current.GetRequiredService<ClusterManager>();

        ClusterManager.LoadFromConfig(KubeConfig);

        Cluster = ClusterManager.GetCluster($"kind-{Name}")!;

        Kubernetes = await Kind.GetKubernetesClient(Name);

        await Cluster.Connect();
    }

    public void Dispose()
    {
        Task.Run(() => Kind.DeleteCluster(Name)).GetAwaiter().GetResult();
    }
}
