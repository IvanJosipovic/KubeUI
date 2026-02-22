using System.Text;
using Avalonia;
using k8s;
using k8s.KubeConfigModels;
using KubeUI.Client;

namespace KubeUI.Tests.E2E;

public class TestHarness : IDisposable
{
    public string Name { get; set; } = Guid.NewGuid().ToString();

    public ICluster Cluster { get; set; }

    public ClusterManager ClusterManager { get; set; }

    public k8s.Kubernetes Kubernetes { get; set; }

    public K8SConfiguration KubeConfig { get; set; }

    public async Task Initialize()
    {
        await Kind.DownloadClient();
        await Kind.CreateCluster(Name);

        KubeConfig = await Kind.GetK8SConfiguration(Name);

        ClusterManager = Application.Current.GetRequiredService<ClusterManager>();

        ClusterManager.LoadFromConfig(KubeConfig);

        Cluster = ClusterManager.GetCluster($"kind-{Name}")!;

        Kubernetes = await Kind.GetKubernetesClient(Name);

        await Cluster.Connect();
    }

    public async Task<ICluster> GetClusterFromServiceAccountSecret(string @namespace, string name)
    {
        var secret = await Kubernetes.CoreV1.ReadNamespacedSecretAsync(name, @namespace);

        // Test is prepped, generate KubeConfig

        var config = KubernetesYaml.Deserialize<K8SConfiguration>(KubernetesYaml.Serialize(KubeConfig));

        var clusterName = "test1";

        config.Clusters.First().Name = clusterName;
        var context = config.Contexts.First();

        context.Name = clusterName;
        context.ContextDetails.Cluster = clusterName;
        context.ContextDetails.User = clusterName;

        var user = config.Users.First();

        user.UserCredentials = new()
        {
            Token = Encoding.UTF8.GetString(secret.Data["token"])
        };
        user.Name = clusterName;

        ClusterManager.LoadFromConfig(config);

        var cluster = ClusterManager.GetCluster(clusterName);

        return cluster;
    }

    public void Dispose()
    {
        Task.Run(() => Kind.DeleteCluster(Name)).GetAwaiter().GetResult();
    }
}
