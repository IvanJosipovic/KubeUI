using k8s;

namespace KubeUI.Core.Tests;

public class TestHarness : IDisposable
{
    public IServiceProvider ServiceProvider { get; set; }

    public string Name { get; set; } = Guid.NewGuid().ToString();

    public ICluster Cluster { get; set; }

    public Kubernetes Kubernetes { get; set; }

    private Kind Kind { get; set; }

    public string Version { get; set; } = "kindest/node:v1.25.3";

    public TestHarness()
    {
        Kind = new Kind();
        Kind.DownloadClient().Wait();
        Kind.CreateCluster(Name, Version);
        var kc = Kind.GetKubeConfig(Name);

        IServiceCollection sc = new ServiceCollection();
        ConfigureServices.Configure(null, sc);

        ServiceProvider = sc.BuildServiceProvider();

        var cm = ServiceProvider.GetRequiredService<ClusterManager>();

        cm.LoadFromConfig(kc);

        Cluster = cm.GetCluster($"kind-{Name}");

        Kubernetes = Kind.GetKubernetesClient(Name);
    }

    public void Dispose()
    {
        Kind.DeleteCluster(Name);
    }
}
