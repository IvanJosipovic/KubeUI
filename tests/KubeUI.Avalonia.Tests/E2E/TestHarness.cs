using System.Text;
using Avalonia;
using k8s;
using k8s.KubeConfigModels;
using k8s.Models;
using KubeUI.Client;

namespace KubeUI.Avalonia.Tests.E2E;

public class TestHarness : IDisposable
{
    public string Name { get; set; } = Guid.NewGuid().ToString();

    public IClusterRuntime Cluster { get; set; }

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

    public async Task<IClusterRuntime> GetClusterFromServiceAccountSecret(string @namespace, string name)
    {
        var timeout = TimeSpan.FromMinutes(2);
        var start = DateTime.UtcNow;
        V1Secret secret;
        while (true)
        {
            secret = await Kubernetes.CoreV1.ReadNamespacedSecretAsync(name, @namespace);
            if (secret.Data != null && secret.Data.ContainsKey("token"))
            {
                break;
            }
            if (DateTime.UtcNow - start > timeout)
            {
                throw new InvalidOperationException($"Secret '{name}' in namespace '{@namespace}' does not contain a 'token' entry after waiting 2 minutes.");
            }
            await Task.Delay(TimeSpan.FromSeconds(2));
        }

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
