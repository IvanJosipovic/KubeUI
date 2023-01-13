using k8s;
using k8s.KubeConfigModels;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KubeUI.Core.Tests;

public class ClusterTests
{
    [Fact]
    public async Task CreateObject()
    {
        using var testHarnes = new TestHarness();

        testHarnes.Cluster.Seed<V1Namespace>();

        var ns = new V1Namespace()
        {
            ApiVersion = V1Namespace.KubeApiVersion,
            Kind = V1Namespace.KubeKind,
            Metadata = new V1ObjectMeta()
            {
                Name = "test"
            }
        };

        await testHarnes.Cluster.AddOrUpdate(ns);

        var ns2 = await testHarnes.Kubernetes.CoreV1.ReadNamespaceAsync("test");
        ns2.Name().Should().Be("test");

        await Task.Delay(TimeSpan.FromSeconds(10));

        var ns3 = testHarnes.Cluster.GetObject<V1Namespace>(null, "test");
        ns3.Name().Should().Be("test");
    }

    [Fact]
    public async Task CreateNamespacedObject()
    {
        using var testHarnes = new TestHarness();

        testHarnes.Cluster.Seed<V1Secret>();

        var secret = new V1Secret()
        {
            ApiVersion = V1Secret.KubeApiVersion,
            Kind = V1Secret.KubeKind,
            Metadata = new V1ObjectMeta()
            {
                Name = "test",
                NamespaceProperty = "default"
            },
            StringData = new Dictionary<string, string>()
            {
                { "data1", "secret1" }
            }
        };

        await testHarnes.Cluster.AddOrUpdate(secret);

        var ns2 = await testHarnes.Kubernetes.CoreV1.ReadNamespacedSecretAsync("test", "default");
        ns2.Name().Should().Be("test");
        var ns3 = testHarnes.Cluster.GetObject<V1Secret>("default", "test");
        ns3.Name().Should().Be("test");
    }

    [Fact]
    public async Task ReadObject()
    {
        using var testHarnes = new TestHarness();

        testHarnes.Cluster.Seed<V1Namespace>();

        var ns = new V1Namespace()
        {
            ApiVersion = V1Namespace.KubeApiVersion,
            Kind = V1Namespace.KubeKind,
            Metadata = new V1ObjectMeta()
            {
                Name = "test"
            }
        };

        await testHarnes.Cluster.AddOrUpdate(ns);
        var ns2 = testHarnes.Cluster.GetObject<V1Namespace>(null, "test");
        ns2.Name().Should().Be("test");
    }

    [Fact]
    public async Task UpdateObject()
    {
        using var testHarnes = new TestHarness();

        testHarnes.Cluster.Seed<V1Namespace>();

        var ns = new V1Namespace()
        {
            ApiVersion = V1Namespace.KubeApiVersion,
            Kind = V1Namespace.KubeKind,
            Metadata = new V1ObjectMeta()
            {
                Name = "test",
            }
        };

        await testHarnes.Cluster.AddOrUpdate(ns);

        ns = await testHarnes.Kubernetes.CoreV1.ReadNamespaceAsync("test");

        ns.Metadata.Labels = new Dictionary<string, string>();
        ns.Metadata.Labels.Add("test", "test");

        await testHarnes.Cluster.AddOrUpdate(ns);

        var ns2 = testHarnes.Cluster.GetObject<V1Namespace>(null, "test");
        ns2.Name().Should().Be("test");
        ns2.Metadata.Labels["test"].Should().Be("test");
    }

    [Fact]
    public async Task UpdateNamespacedObject()
    {
        using var testHarnes = new TestHarness();

        testHarnes.Cluster.Seed<V1Secret>();

        var secret = new V1Secret()
        {
            ApiVersion = V1Secret.KubeApiVersion,
            Kind = V1Secret.KubeKind,
            Metadata = new V1ObjectMeta()
            {
                Name = "test",
                NamespaceProperty = "default"
            },
            StringData = new Dictionary<string, string>()
            {
                { "data1", "secret1" }
            }
        };

        await testHarnes.Cluster.AddOrUpdate(secret);

        secret = await testHarnes.Kubernetes.CoreV1.ReadNamespacedSecretAsync("test", "default");

        secret.Metadata.Labels = new Dictionary<string, string>();
        secret.Metadata.Labels.Add("test", "test");

        await testHarnes.Cluster.AddOrUpdate(secret);

        var ns2 = testHarnes.Cluster.GetObject<V1Secret>("default", "test");
        ns2.Name().Should().Be("test");
        ns2.Metadata.Labels["test"].Should().Be("test");
    }

    [Fact]
    public async Task DeleteObject()
    {
        using var testHarnes = new TestHarness();

        testHarnes.Cluster.Seed<V1Namespace>();

        var ns = new V1Namespace()
        {
            ApiVersion = V1Namespace.KubeApiVersion,
            Kind = V1Namespace.KubeKind,
            Metadata = new V1ObjectMeta()
            {
                Name = "test"
            }
        };

        await testHarnes.Cluster.AddOrUpdate(ns);

        ns = await testHarnes.Kubernetes.CoreV1.ReadNamespaceAsync("test");

        await testHarnes.Cluster.Delete(ns);

        ((Client.Cluster)testHarnes.Cluster).Objects[V1Namespace.KubeApiVersion.ToLower() + "/" + V1Namespace.KubeKind.ToLower()]
            .Values.All(x => x.Name() != "test").Should().BeTrue();
    }

    [Fact]
    public async Task DeleteNamespacedObject()
    {
        using var testHarnes = new TestHarness();

        testHarnes.Cluster.Seed<V1Secret>();

        var secret = new V1Secret()
        {
            ApiVersion = V1Secret.KubeApiVersion,
            Kind = V1Secret.KubeKind,
            Metadata = new V1ObjectMeta()
            {
                Name = "test",
                NamespaceProperty = "default"
            },
            StringData = new Dictionary<string, string>()
            {
                { "data1", "secret1" }
            }
        };

        await testHarnes.Cluster.AddOrUpdate(secret);

        secret = await testHarnes.Kubernetes.CoreV1.ReadNamespacedSecretAsync("test", "default");

        await testHarnes.Cluster.Delete(secret);

        ((Client.Cluster)testHarnes.Cluster).Objects[V1Secret.KubeApiVersion.ToLower() + "/" + V1Secret.KubeKind.ToLower()]
            .Values.All(x => x.Name() != "test").Should().BeTrue();
    }

    [Fact]
    public async Task ImportYaml()
    {
        using var testHarnes = new TestHarness();

        testHarnes.Cluster.Seed<V1Namespace>();

        var ns = new V1Namespace()
        {
            ApiVersion = V1Namespace.KubeApiVersion,
            Kind = V1Namespace.KubeKind,
            Metadata = new V1ObjectMeta()
            {
                Name = "test"
            }
        };

        var yaml = KubernetesYaml.Serialize(ns);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(yaml));

        await testHarnes.Cluster.ImportYaml(stream);

        var ns2 = await testHarnes.Kubernetes.CoreV1.ReadNamespaceAsync("test");
        ns2.Name().Should().Be("test");

        await Task.Delay(TimeSpan.FromSeconds(2));

        var ns3 = testHarnes.Cluster.GetObject<V1Namespace>(null, "test");
        ns3.Name().Should().Be("test");
    }
}

public class TestHarness : IDisposable
{
    public IServiceProvider ServiceProvider { get; set; }

    public string Name { get; set; } = Guid.NewGuid().ToString();

    public ICluster Cluster { get; set; }

    public Kubernetes Kubernetes { get; set; }

    private Kind Kind { get; set; }

    public TestHarness()
    {
        Kind = new Kind();
        Kind.CreateCluster(Name);
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

public class Kind
{
    public void CreateCluster(string name)
    {
        var p = new Process();
        p.StartInfo.FileName = "kind.exe";
        p.StartInfo.Arguments = $"create cluster --name {name}";
        p.StartInfo.RedirectStandardError = true;
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.CreateNoWindow = true;
        p.Start();
        var error = p.StandardError.ReadToEnd();

        if (!string.IsNullOrEmpty(error) && error.StartsWith("ERROR:"))
        {
            throw new Exception(error);
        }
    }

    public void DeleteCluster(string name)
    {
        var p = new Process();
        p.StartInfo.FileName = "kind.exe";
        p.StartInfo.Arguments = $"delete cluster --name {name}";
        p.StartInfo.RedirectStandardError = true;
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.CreateNoWindow = true;
        p.Start();
        var error = p.StandardError.ReadToEnd();

        if (!string.IsNullOrEmpty(error) && error.StartsWith("ERROR:"))
        {
            throw new Exception(error);
        }
    }

    public List<string> GetClusters()
    {
        var p = new Process();
        p.StartInfo.FileName = "kind.exe";
        p.StartInfo.Arguments = $"get clusters";
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.RedirectStandardError = true;
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.CreateNoWindow = true;
        p.Start();
        var result = p.StandardOutput.ReadToEnd();
        var error = p.StandardError.ReadToEnd();

        if (!string.IsNullOrEmpty(error))
        {
            throw new Exception(error);
        }

        return new List<string>(result.TrimEnd().Split("\n"));
    }

    public string GetKubeConfig(string name)
    {
        var p = new Process();
        p.StartInfo.FileName = "kind.exe";
        p.StartInfo.Arguments = $"get kubeconfig --name {name}";
        p.StartInfo.RedirectStandardOutput = true;
        p.StartInfo.RedirectStandardError = true;
        p.StartInfo.UseShellExecute = false;
        p.StartInfo.CreateNoWindow = true;
        p.Start();
        var result = p.StandardOutput.ReadToEnd();
        var error = p.StandardError.ReadToEnd();

        if (!string.IsNullOrEmpty(error))
        {
            throw new Exception(error);
        }

        return result;
    }

    public K8SConfiguration GetK8SConfiguration(string name)
    {
        return KubernetesYaml.Deserialize<K8SConfiguration>(GetKubeConfig(name));
    }

    public Kubernetes GetKubernetesClient(string name)
    {
        return new Kubernetes(KubernetesClientConfiguration.BuildConfigFromConfigObject(GetK8SConfiguration(name)));
    }

    public void DeleteAllClusters()
    {
        foreach (var cluster in GetClusters())
        {
            DeleteCluster(cluster);
        }
    }
}