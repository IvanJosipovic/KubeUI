using k8s;
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
