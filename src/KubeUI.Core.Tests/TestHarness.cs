using k8s;

namespace KubeUI.Core.Tests;

public class TestHarness : IDisposable
{
    public string Name { get; set; } = Guid.NewGuid().ToString();

    public string Version { get; set; } = "kindest/node:v1.26.3";

    public IServiceProvider ServiceProvider { get; set; }

    public ICluster Cluster { get; set; }

    public Kubernetes Kubernetes { get; set; }

    private Kind Kind { get; set; }

    public TestHarness()
    {
        Kind = new Kind();
        Kind.DownloadClient().Wait();
        Kind.CreateCluster(Name, Version).Wait();
        var kc = Kind.GetKubeConfig(Name).Result;

        IServiceCollection sc = new ServiceCollection();
        ConfigureServices.Configure(null, sc);

        ServiceProvider = sc.BuildServiceProvider();

        var cm = ServiceProvider.GetRequiredService<ClusterManager>();

        cm.LoadFromConfig(kc);

        Cluster = cm.GetCluster($"kind-{Name}");

        Kubernetes = Kind.GetKubernetesClient(Name).Result;
    }

    public void Dispose()
    {
        Kind.DeleteCluster(Name);
    }
}
