using k8s;
using System.IO;
using System.IO.Compression;
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

        await Task.Delay(TimeSpan.FromSeconds(2));

        await testHarnes.Cluster.AddOrUpdate(ns);

        var ns2 = await testHarnes.Kubernetes.CoreV1.ReadNamespaceAsync("test");
        ns2.Name().Should().Be("test");

        await Task.Delay(TimeSpan.FromSeconds(2));

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

        await Task.Delay(TimeSpan.FromSeconds(2));

        await testHarnes.Cluster.AddOrUpdate(secret);

        var ns2 = await testHarnes.Kubernetes.CoreV1.ReadNamespacedSecretAsync("test", "default");
        ns2.Name().Should().Be("test");

        await Task.Delay(TimeSpan.FromSeconds(2));

        var ns3 = testHarnes.Cluster.GetObject<V1Secret>("default", "test");
        ns3.Name().Should().Be("test");
        ns3.Namespace().Should().Be("default");
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

        await Task.Delay(TimeSpan.FromSeconds(2));

        await testHarnes.Kubernetes.CoreV1.CreateNamespaceAsync(ns);

        await Task.Delay(TimeSpan.FromSeconds(2));

        var ns2 = testHarnes.Cluster.GetObject<V1Namespace>(null, "test");
        ns2.Name().Should().Be("test");
    }

    [Fact]
    public async Task ReadNamespacedObject()
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

        await Task.Delay(TimeSpan.FromSeconds(2));

        secret = await testHarnes.Kubernetes.CoreV1.CreateNamespacedSecretAsync(secret, "default");

        await Task.Delay(TimeSpan.FromSeconds(2));

        var ns2 = testHarnes.Cluster.GetObject<V1Secret>("default", "test");
        ns2.Name().Should().Be("test");
        ns2.Namespace().Should().Be("default");
    }

    [Fact]
    public async Task ReadObjects()
    {
        using var testHarnes = new TestHarness();

        testHarnes.Cluster.Seed<V1Namespace>();

        await Task.Delay(TimeSpan.FromSeconds(2));

        var ns = testHarnes.Cluster.GetObjects<V1Namespace>();
        ns.Count().Should().BeGreaterThan(1);
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

        await Task.Delay(TimeSpan.FromSeconds(2));

        ns = await testHarnes.Kubernetes.CoreV1.CreateNamespaceAsync(ns);

        ns.Metadata.Labels = new Dictionary<string, string>();
        ns.Metadata.Labels.Add("test", "test");

        await testHarnes.Cluster.AddOrUpdate(ns);

        await Task.Delay(TimeSpan.FromSeconds(2));

        var ns2 = testHarnes.Cluster.GetObject<V1Namespace>(null, ns.Name());
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

        await Task.Delay(TimeSpan.FromSeconds(2));

        secret = await testHarnes.Kubernetes.CoreV1.CreateNamespacedSecretAsync(secret, "default");

        secret.Metadata.Labels = new Dictionary<string, string>();
        secret.Metadata.Labels.Add("test", "test");

        await testHarnes.Cluster.AddOrUpdate(secret);

        await Task.Delay(TimeSpan.FromSeconds(2));

        var ns2 = testHarnes.Cluster.GetObject<V1Secret>("default", "test");
        ns2.Name().Should().Be("test");
        ns2.Namespace().Should().Be("default");
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

        await Task.Delay(TimeSpan.FromSeconds(2));

        await testHarnes.Kubernetes.CoreV1.CreateNamespaceAsync(ns);

        await Task.Delay(TimeSpan.FromSeconds(10));

        await testHarnes.Cluster.Delete(ns);

        await Task.Delay(TimeSpan.FromSeconds(10));

        ((Cluster)testHarnes.Cluster).Objects[V1Namespace.KubeApiVersion + "/" + V1Namespace.KubeKind]
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

        await testHarnes.Kubernetes.CoreV1.CreateNamespacedSecretAsync(secret, "default");

        await testHarnes.Cluster.Delete(secret);

        await Task.Delay(TimeSpan.FromSeconds(2));

        ((Cluster)testHarnes.Cluster).Objects[V1Secret.KubeApiVersion+ "/" + V1Secret.KubeKind]
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

        await Task.Delay(TimeSpan.FromSeconds(2));

        var yaml = KubernetesYaml.Serialize(ns);

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(yaml));

        await testHarnes.Cluster.ImportYaml(stream);

        await Task.Delay(TimeSpan.FromSeconds(2));

        var ns2 = await testHarnes.Kubernetes.CoreV1.ReadNamespaceAsync("test");
        ns2.Name().Should().Be("test");

        await Task.Delay(TimeSpan.FromSeconds(5));

        var ns3 = testHarnes.Cluster.GetObject<V1Namespace>(null, "test");
        ns3.Name().Should().Be("test");
    }

    [Fact]
    public async Task ImportZip()
    {
        using var testHarnes = new TestHarness();

        testHarnes.Cluster.Seed<V1Namespace>();

        await Task.Delay(TimeSpan.FromSeconds(2));

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

        using var memoryStream = new MemoryStream();

        using (var archive = new ZipArchive(memoryStream, ZipArchiveMode.Create, true))
        {
            var firstFile = archive.CreateEntry("ns.yaml");

            using (var entryStream = firstFile.Open())
            using (var streamWriter = new StreamWriter(entryStream))
            {
                streamWriter.Write(yaml);
            }
        }

        memoryStream.Seek(0, SeekOrigin.Begin);

        await testHarnes.Cluster.ImportZip(memoryStream);

        await Task.Delay(TimeSpan.FromSeconds(2));

        var ns3 = testHarnes.Cluster.GetObject<V1Namespace>(null, "test");
        ns3.Name().Should().Be("test");
    }

    private string yamlCRD = @"
apiVersion: apiextensions.k8s.io/v1
kind: CustomResourceDefinition
metadata:
  name: tests.kubeui.com
spec:
  group: kubeui.com
  names:
    plural: tests
    singular: test
    kind: Test
    listKind: TestList
  scope: Namespaced
  versions:
    - name: v1beta1
      served: true
      storage: true
      schema:
        openAPIV3Schema:
          type: object
          properties:
            apiVersion:
              type: string
            kind:
              type: string
            metadata:
              type: object
            spec:
              type: object
              properties:
                someString:
                  type: string
";

    [Fact]
    public async Task HandleCRD()
    {
        using var testHarnes = new TestHarness();
        testHarnes.Cluster.Seed<V1CustomResourceDefinition>();

        await Task.Delay(TimeSpan.FromSeconds(2));

        await testHarnes.Kubernetes.CreateCustomResourceDefinitionAsync(KubernetesYaml.LoadFromString<V1CustomResourceDefinition>(yamlCRD));

        await Task.Delay(TimeSpan.FromSeconds(5));

        var yaml = @"
apiVersion: kubeui.com/v1beta1
kind: Test
metadata:
  name: test1
  namespace: default
spec:
  someString: myValue
";
        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(yaml));

        await testHarnes.Cluster.ImportYaml(stream);

        await Task.Delay(TimeSpan.FromSeconds(5));

        ((Cluster)testHarnes.Cluster).Objects["kubeui.com/v1beta1/Test"].Values.Any(x => x.Name() == "test1").Should().BeTrue();
    }
}