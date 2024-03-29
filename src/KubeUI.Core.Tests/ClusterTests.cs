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
        using var testHarness = new TestHarness();

        testHarness.Cluster.Seed<V1Namespace>();

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

        await testHarness.Cluster.AddOrUpdate(ns);

        var ns2 = await testHarness.Kubernetes.CoreV1.ReadNamespaceAsync("test");
        ns2.Name().Should().Be("test");

        await Task.Delay(TimeSpan.FromSeconds(2));

        var ns3 = testHarness.Cluster.GetObject<V1Namespace>(null, "test");
        ns3.Name().Should().Be("test");
    }

    [Fact]
    public async Task CreateNamespacedObject()
    {
        using var testHarness = new TestHarness();

        testHarness.Cluster.Seed<V1Secret>();

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

        await testHarness.Cluster.AddOrUpdate(secret);

        var ns2 = await testHarness.Kubernetes.CoreV1.ReadNamespacedSecretAsync("test", "default");
        ns2.Name().Should().Be("test");

        await Task.Delay(TimeSpan.FromSeconds(2));

        var ns3 = testHarness.Cluster.GetObject<V1Secret>("default", "test");
        ns3.Name().Should().Be("test");
        ns3.Namespace().Should().Be("default");
    }

    [Fact]
    public async Task ReadObject()
    {
        using var testHarness = new TestHarness();

        testHarness.Cluster.Seed<V1Namespace>();

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

        await testHarness.Kubernetes.CoreV1.CreateNamespaceAsync(ns);

        await Task.Delay(TimeSpan.FromSeconds(2));

        var ns2 = testHarness.Cluster.GetObject<V1Namespace>(null, "test");
        ns2.Name().Should().Be("test");
    }

    [Fact]
    public async Task ReadNamespacedObject()
    {
        using var testHarness = new TestHarness();

        testHarness.Cluster.Seed<V1Secret>();

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

        secret = await testHarness.Kubernetes.CoreV1.CreateNamespacedSecretAsync(secret, "default");

        await Task.Delay(TimeSpan.FromSeconds(2));

        var ns2 = testHarness.Cluster.GetObject<V1Secret>("default", "test");
        ns2.Name().Should().Be("test");
        ns2.Namespace().Should().Be("default");
    }

    [Fact]
    public async Task ReadObjects()
    {
        using var testHarness = new TestHarness();

        testHarness.Cluster.Seed<V1Namespace>();

        await Task.Delay(TimeSpan.FromSeconds(2));

        var ns = testHarness.Cluster.GetObjects<V1Namespace>();
        ns.Count().Should().BeGreaterThan(1);
    }

    [Fact]
    public async Task UpdateObject()
    {
        using var testHarness = new TestHarness();

        testHarness.Cluster.Seed<V1Namespace>();

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

        ns = await testHarness.Kubernetes.CoreV1.CreateNamespaceAsync(ns);

        ns.Metadata.Labels = new Dictionary<string, string>();
        ns.Metadata.Labels.Add("test", "test");

        await testHarness.Cluster.AddOrUpdate(ns);

        await Task.Delay(TimeSpan.FromSeconds(2));

        var ns2 = testHarness.Cluster.GetObject<V1Namespace>(null, ns.Name());
        ns2.Name().Should().Be("test");
        ns2.Metadata.Labels["test"].Should().Be("test");
    }

    [Fact]
    public async Task UpdateNamespacedObject()
    {
        using var testHarness = new TestHarness();

        testHarness.Cluster.Seed<V1Secret>();

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

        secret = await testHarness.Kubernetes.CoreV1.CreateNamespacedSecretAsync(secret, "default");

        secret.Metadata.Labels = new Dictionary<string, string>();
        secret.Metadata.Labels.Add("test", "test");

        await testHarness.Cluster.AddOrUpdate(secret);

        await Task.Delay(TimeSpan.FromSeconds(2));

        var ns2 = testHarness.Cluster.GetObject<V1Secret>("default", "test");
        ns2.Name().Should().Be("test");
        ns2.Namespace().Should().Be("default");
        ns2.Metadata.Labels["test"].Should().Be("test");
    }

    [Fact]
    public async Task DeleteObject()
    {
        using var testHarness = new TestHarness();

        testHarness.Cluster.Seed<V1Namespace>();

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

        await testHarness.Kubernetes.CoreV1.CreateNamespaceAsync(ns);

        await Task.Delay(TimeSpan.FromSeconds(10));

        await testHarness.Cluster.Delete(ns);

        await Task.Delay(TimeSpan.FromSeconds(10));

        ((Cluster)testHarness.Cluster).Objects[V1Namespace.KubeApiVersion + "/" + V1Namespace.KubeKind]
            .Values.All(x => x.Name() != "test").Should().BeTrue();
    }

    [Fact]
    public async Task DeleteNamespacedObject()
    {
        using var testHarness = new TestHarness();

        testHarness.Cluster.Seed<V1Secret>();

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

        await testHarness.Kubernetes.CoreV1.CreateNamespacedSecretAsync(secret, "default");

        await testHarness.Cluster.Delete(secret);

        await Task.Delay(TimeSpan.FromSeconds(2));

        ((Cluster)testHarness.Cluster).Objects[V1Secret.KubeApiVersion + "/" + V1Secret.KubeKind]
            .Values.All(x => x.Name() != "test").Should().BeTrue();
    }

    [Fact]
    public async Task ImportYaml()
    {
        using var testHarness = new TestHarness();

        testHarness.Cluster.Seed<V1Namespace>();

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

        await testHarness.Cluster.ImportYaml(stream);

        await Task.Delay(TimeSpan.FromSeconds(2));

        var ns2 = await testHarness.Kubernetes.CoreV1.ReadNamespaceAsync("test");
        ns2.Name().Should().Be("test");

        await Task.Delay(TimeSpan.FromSeconds(5));

        var ns3 = testHarness.Cluster.GetObject<V1Namespace>(null, "test");
        ns3.Name().Should().Be("test");
    }

    [Fact]
    public async Task ImportZip()
    {
        using var testHarness = new TestHarness();

        testHarness.Cluster.Seed<V1Namespace>();

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

        await testHarness.Cluster.ImportZip(memoryStream);

        await Task.Delay(TimeSpan.FromSeconds(2));

        var ns3 = testHarness.Cluster.GetObject<V1Namespace>(null, "test");
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
        using var testHarness = new TestHarness();
        testHarness.Cluster.Seed<V1CustomResourceDefinition>();

        await Task.Delay(TimeSpan.FromSeconds(2));

        await testHarness.Kubernetes.CreateCustomResourceDefinitionAsync(KubernetesYaml.Deserialize<V1CustomResourceDefinition>(yamlCRD));

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

        await testHarness.Cluster.ImportYaml(stream);
        testHarness.Cluster.Seed("v1beta1", "Test", "kubeui.com");

        await Task.Delay(TimeSpan.FromSeconds(5));

        ((Cluster)testHarness.Cluster).Objects["kubeui.com/v1beta1/Test"].Values.Any(x => x.Name() == "test1").Should().BeTrue();
    }
}