using System.Text;
using Avalonia;
using Avalonia.Headless.XUnit;
using FluentAssertions;
using k8s;
using k8s.KubeConfigModels;
using k8s.Models;
using KubeUI.Client.Informer;
using KubeUI.Core.Tests;

namespace KubeUI.Client.Tests;

public class ClusterEndToEndTests
{
    [AvaloniaFact]

    public async Task CreateObject()
    {
        using var testHarness = new TestHarness();
        await testHarness.Initialize();

        await testHarness.Cluster.Seed<V1Namespace>();

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

    [AvaloniaFact]
    public async Task CreateNamespacedObject()
    {
        using var testHarness = new TestHarness();
        await testHarness.Initialize();

        await testHarness.Cluster.Seed<V1Secret>();

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

    [AvaloniaFact]
    public async Task ReadObject()
    {
        using var testHarness = new TestHarness();
        await testHarness.Initialize();

        await testHarness.Cluster.Seed<V1Namespace>();

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

    [AvaloniaFact]
    public async Task ReadNamespacedObject()
    {
        using var testHarness = new TestHarness();
        await testHarness.Initialize();

        await testHarness.Cluster.Seed<V1Secret>();

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

    [AvaloniaFact]
    public async Task ReadObjects()
    {
        using var testHarness = new TestHarness();
        await testHarness.Initialize();

        await testHarness.Cluster.Seed<V1Namespace>();

        await Task.Delay(TimeSpan.FromSeconds(2));

        var ns = testHarness.Cluster.GetObjectDictionary<V1Namespace>().Values;
        ns.Count.Should().BeGreaterThan(1);
    }

    [AvaloniaFact]
    public async Task UpdateObject()
    {
        using var testHarness = new TestHarness();
        await testHarness.Initialize();

        await testHarness.Cluster.Seed<V1Namespace>();

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

    [AvaloniaFact]
    public async Task UpdateNamespacedObject()
    {
        using var testHarness = new TestHarness();
        await testHarness.Initialize();

        await testHarness.Cluster.Seed<V1Secret>();

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

    [AvaloniaFact]
    public async Task DeleteObject()
    {
        using var testHarness = new TestHarness();
        await testHarness.Initialize();

        await testHarness.Cluster.Seed<V1Namespace>();

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

        testHarness.Cluster.GetObjectDictionary<V1Namespace>().Values.All(x => x.Name() != "test").Should().BeTrue();
    }

    [AvaloniaFact]
    public async Task DeleteNamespacedObject()
    {
        using var testHarness = new TestHarness();
        await testHarness.Initialize();

        await testHarness.Cluster.Seed<V1Secret>();

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

        testHarness.Cluster.GetObjectDictionary<V1Secret>().Values.All(x => x.Name() != "test").Should().BeTrue();
    }

    [AvaloniaFact]
    public async Task ImportYaml()
    {
        using var testHarness = new TestHarness();
        await testHarness.Initialize();

        await testHarness.Cluster.Seed<V1Namespace>();

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

    [AvaloniaFact]
    public async Task HandleCRD()
    {
        using var testHarness = new TestHarness();
        await testHarness.Initialize();

        await testHarness.Cluster.Seed<V1CustomResourceDefinition>();

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
        var type = testHarness.Cluster.ModelCache.GetResourceType("kubeui.com", "v1beta1", "Test");

        var _seedMethodInfo = testHarness.Cluster.GetType().GetMethod(nameof(Cluster.Seed), [typeof(bool)]);

        var fooRef = _seedMethodInfo.MakeGenericMethod(type);
        fooRef.Invoke(testHarness.Cluster, [false]);

        await Task.Delay(TimeSpan.FromSeconds(5));

        var kind = testHarness.Cluster.Objects[GroupApiVersionKind.From(type)];
        kind.Type.Should().Be(type);
        kind.Items.Count.Should().Be(1);

        foreach(object item in kind.Items)
        {
            var key = (NamespacedName)(item.GetType().GetProperty("Key").GetValue(item));
            key.Name.Should().Be("test1");
            key.Namespace.Should().Be("default");

            var obj = (IKubernetesObject<V1ObjectMeta>)item.GetType().GetProperty("Value").GetValue(item);

            obj.Name().Should().Be("test1");
            obj.Namespace().Should().Be("default");
            var spec = obj.GetType().GetProperty("Spec").GetValue(obj);
            spec.GetType().GetProperty("SomeString").GetValue(spec).Should().Be("myValue");
        }
    }

    #region Limited Access

    private string yamlLimitedRbac = @"
---
apiVersion: v1
kind: Namespace
metadata:
  name: my-app
---
apiVersion: v1
kind: ServiceAccount
metadata:
  name: my-serviceaccount
  namespace: my-app
secrets:
  - name: my-serviceaccount
---
apiVersion: v1
kind: Secret
metadata:
  name: my-serviceaccount
  namespace: my-app
  annotations:
    kubernetes.io/service-account.name: my-serviceaccount
type: kubernetes.io/service-account-token
---
kind: ClusterRole
apiVersion: rbac.authorization.k8s.io/v1
metadata:
  name: my-serviceaccount
rules: []
---
apiVersion: rbac.authorization.k8s.io/v1
kind: ClusterRoleBinding
metadata:
  name: my-serviceaccount-additional
roleRef:
  apiGroup: rbac.authorization.k8s.io
  kind: ClusterRole
  name:  my-serviceaccount
subjects:
- kind: ServiceAccount
  name: my-serviceaccount
  namespace: my-app
---
apiVersion: rbac.authorization.k8s.io/v1
kind: ClusterRoleBinding
metadata:
  name: my-serviceaccount-viewer
roleRef:
  apiGroup: rbac.authorization.k8s.io
  kind: ClusterRole
  name: viewer
subjects:
- kind: ServiceAccount
  name: my-serviceaccount
  namespace: my-app
---
apiVersion: rbac.authorization.k8s.io/v1
kind: RoleBinding
metadata:
  name: my-serviceaccount-developer
  namespace: my-app
roleRef:
  apiGroup: rbac.authorization.k8s.io
  kind: ClusterRole
  name: developer
subjects:
- kind: ServiceAccount
  name: my-serviceaccount
  namespace: my-app
---
kind: ClusterRole
apiVersion: rbac.authorization.k8s.io/v1
metadata:
  name: developer
rules:
- verbs:
    - get
    - list
    - watch
  apiGroups:
    - '*'
  resources:
    - '*'

- verbs:
    - create
  apiGroups:
    - ''
  resources:
    - pods/attach
    - pods/exec
    - pods/portforward
    - pods/proxy

- verbs:
    - create
  apiGroups:
    - 'batch'
  resources:
    - job

- verbs:
    - create
  apiGroups:
    - ''
  resources:
    - services/proxy

- verbs:
    - delete
  apiGroups:
    - ''
  resources:
    - pods
---
kind: ClusterRole
apiVersion: rbac.authorization.k8s.io/v1
metadata:
  name: viewer
rules:
- apiGroups:
    - ''
  resources:
    - namespaces
  verbs:
    - get
    - list
    - watch

- apiGroups:
    - ''
  resources:
    - nodes
  verbs:
    - get
    - list
    - watch

- apiGroups:
    - apiextensions.k8s.io
  resources:
    - customresourcedefinitions
  verbs:
    - get
    - list
    - watch

- apiGroups:
    - storage.k8s.io
  resources:
    - storageclasses
  verbs:
    - get
    - list
    - watch

- apiGroups:
    - ''
  resources:
    - persistentvolumes
  verbs:
    - get
    - list
    - watch

- apiGroups:
    - 'metrics.k8s.io'
  resources:
    - nodes
    - pods
  verbs:
    - get
    - list

- verbs:
    - get
    - list
    - watch
  nonResourceURLs:
    - '*'
";

    private string yamlLimitedRbacNoNamespace = @"
---
apiVersion: v1
kind: Namespace
metadata:
  name: my-app
---
apiVersion: v1
kind: ServiceAccount
metadata:
  name: my-serviceaccount
  namespace: my-app
secrets:
  - name: my-serviceaccount
---
apiVersion: v1
kind: Secret
metadata:
  name: my-serviceaccount
  namespace: my-app
  annotations:
    kubernetes.io/service-account.name: my-serviceaccount
type: kubernetes.io/service-account-token
---
kind: ClusterRole
apiVersion: rbac.authorization.k8s.io/v1
metadata:
  name: my-serviceaccount
rules: []
---
apiVersion: rbac.authorization.k8s.io/v1
kind: ClusterRoleBinding
metadata:
  name: my-serviceaccount-additional
roleRef:
  apiGroup: rbac.authorization.k8s.io
  kind: ClusterRole
  name:  my-serviceaccount
subjects:
- kind: ServiceAccount
  name: my-serviceaccount
  namespace: my-app
---
apiVersion: rbac.authorization.k8s.io/v1
kind: ClusterRoleBinding
metadata:
  name: my-serviceaccount-viewer
roleRef:
  apiGroup: rbac.authorization.k8s.io
  kind: ClusterRole
  name: viewer
subjects:
- kind: ServiceAccount
  name: my-serviceaccount
  namespace: my-app
---
apiVersion: rbac.authorization.k8s.io/v1
kind: RoleBinding
metadata:
  name: my-serviceaccount-developer
  namespace: my-app
roleRef:
  apiGroup: rbac.authorization.k8s.io
  kind: ClusterRole
  name: developer
subjects:
- kind: ServiceAccount
  name: my-serviceaccount
  namespace: my-app
---
kind: ClusterRole
apiVersion: rbac.authorization.k8s.io/v1
metadata:
  name: developer
rules:
- verbs:
    - get
    - list
    - watch
  apiGroups:
    - '*'
  resources:
    - '*'

- verbs:
    - create
  apiGroups:
    - ''
  resources:
    - pods/attach
    - pods/exec
    - pods/portforward
    - pods/proxy

- verbs:
    - create
  apiGroups:
    - 'batch'
  resources:
    - job

- verbs:
    - create
  apiGroups:
    - ''
  resources:
    - services/proxy

- verbs:
    - delete
  apiGroups:
    - ''
  resources:
    - pods
---
kind: ClusterRole
apiVersion: rbac.authorization.k8s.io/v1
metadata:
  name: viewer
rules:
- apiGroups:
    - ''
  resources:
    - nodes
  verbs:
    - get
    - list
    - watch

- apiGroups:
    - apiextensions.k8s.io
  resources:
    - customresourcedefinitions
  verbs:
    - get
    - list
    - watch

- apiGroups:
    - storage.k8s.io
  resources:
    - storageclasses
  verbs:
    - get
    - list
    - watch

- apiGroups:
    - ''
  resources:
    - persistentvolumes
  verbs:
    - get
    - list
    - watch

- apiGroups:
    - 'metrics.k8s.io'
  resources:
    - nodes
    - pods
  verbs:
    - get
    - list

- verbs:
    - get
    - list
    - watch
  nonResourceURLs:
    - '*'
";

    [AvaloniaFact]
    public async Task LimitedAccess()
    {
        using var testHarness = new TestHarness();
        await testHarness.Initialize();

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(yamlLimitedRbac));

        await testHarness.Cluster.ImportYaml(stream);

        var cluster = await testHarness.GetClusterFromServiceAccount("my-app", "my-serviceaccount");

        await cluster.Connect();

        await cluster.Seed<V1Node>(true);

        await cluster.Seed<V1Secret>(true);

        await Task.Delay(TimeSpan.FromSeconds(5));

        var nodes = await cluster.GetObjectDictionaryAsync<V1Node>();
        nodes.Count.Should().Be(1);

        var secrets = await cluster.GetObjectDictionaryAsync<V1Secret>();
        secrets.Count.Should().Be(1);
        secrets.Values.First().Namespace().Should().Be("my-app");
        secrets.Values.First().Name().Should().Be("my-serviceaccount");
    }

    [AvaloniaFact]
    public async Task LimitedAccessNoNamespace()
    {
        using var testHarness = new TestHarness();
        await testHarness.Initialize();

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(yamlLimitedRbacNoNamespace));

        await testHarness.Cluster.ImportYaml(stream);

        var cluster = await testHarness.GetClusterFromServiceAccount("my-app", "my-serviceaccount");

        var settings = Application.Current.GetRequiredService<ISettingsService>();

        settings.Settings = new();

        settings.Settings.GetClusterSettings(cluster).Namespaces.Add("my-app");

        await cluster.Connect();

        await cluster.Seed<V1Node>(true);

        await cluster.Seed<V1Secret>(true);

        await Task.Delay(TimeSpan.FromSeconds(5));

        var nodes = await cluster.GetObjectDictionaryAsync<V1Node>();
        nodes.Count.Should().Be(1);

        var secrets = await cluster.GetObjectDictionaryAsync<V1Secret>();
        secrets.Count.Should().Be(1);
        secrets.Values.First().Namespace().Should().Be("my-app");
        secrets.Values.First().Name().Should().Be("my-serviceaccount");
    }

    [AvaloniaFact]
    public async Task RootAccessCanI()
    {
        using var testHarness = new TestHarness();
        await testHarness.Initialize();

        var cluster = testHarness.Cluster;

        await cluster.GetObjectDictionaryAsync<V1Pod>();

        cluster.CanI<V1Pod>(Cluster.Verb.Create).Should().BeTrue();
        cluster.CanI<V1Pod>(Cluster.Verb.Delete).Should().BeTrue();
        cluster.CanI<V1Pod>(Cluster.Verb.Get).Should().BeTrue();
        cluster.CanI<V1Pod>(Cluster.Verb.List).Should().BeTrue();
        cluster.CanI<V1Pod>(Cluster.Verb.Patch).Should().BeTrue();
        cluster.CanI<V1Pod>(Cluster.Verb.Update).Should().BeTrue();
        cluster.CanI<V1Pod>(Cluster.Verb.Watch).Should().BeTrue();

        cluster.CanI<V1Pod>(Cluster.Verb.Get, "log").Should().BeTrue();
        cluster.CanI<V1Pod>(Cluster.Verb.Create, "exec").Should().BeTrue();
        cluster.CanI<V1Pod>(Cluster.Verb.Create, "portforward").Should().BeTrue();

        cluster.CanI<V1Pod>(Cluster.Verb.Create, "default").Should().BeTrue();
        cluster.CanI<V1Pod>(Cluster.Verb.Delete, "default").Should().BeTrue();
        cluster.CanI<V1Pod>(Cluster.Verb.Get, "default").Should().BeTrue();
        cluster.CanI<V1Pod>(Cluster.Verb.List, "default").Should().BeTrue();
        cluster.CanI<V1Pod>(Cluster.Verb.Patch, "default").Should().BeTrue();
        cluster.CanI<V1Pod>(Cluster.Verb.Update, "default").Should().BeTrue();
        cluster.CanI<V1Pod>(Cluster.Verb.Watch, "default").Should().BeTrue();

        cluster.CanI<V1Pod>(Cluster.Verb.Get, "default", "log").Should().BeTrue();
        cluster.CanI<V1Pod>(Cluster.Verb.Create, "default", "exec").Should().BeTrue();
        cluster.CanI<V1Pod>(Cluster.Verb.Create, "default", "portforward").Should().BeTrue();
    }

    [AvaloniaFact]
    public async Task LimitedAccessCanI()
    {
        using var testHarness = new TestHarness();
        await testHarness.Initialize();

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(yamlLimitedRbacNoNamespace));

        await testHarness.Cluster.ImportYaml(stream);

        var cluster = await testHarness.GetClusterFromServiceAccount("my-app", "my-serviceaccount");

        var settings = Application.Current.GetRequiredService<ISettingsService>();

        settings.Settings = new();

        settings.Settings.GetClusterSettings(cluster).Namespaces.Add("my-app");

        await cluster.Connect();

        await cluster.GetObjectDictionaryAsync<V1Pod>();

        cluster.CanI<V1Namespace>(Cluster.Verb.Create).Should().BeFalse();
        cluster.CanI<V1Namespace>(Cluster.Verb.Delete).Should().BeFalse();
        cluster.CanI<V1Namespace>(Cluster.Verb.Get).Should().BeFalse();
        cluster.CanI<V1Namespace>(Cluster.Verb.List).Should().BeFalse();
        cluster.CanI<V1Namespace>(Cluster.Verb.Patch).Should().BeFalse();
        cluster.CanI<V1Namespace>(Cluster.Verb.Update).Should().BeFalse();
        cluster.CanI<V1Namespace>(Cluster.Verb.Watch).Should().BeFalse();

        cluster.CanI<V1Pod>(Cluster.Verb.Create).Should().BeFalse();
        cluster.CanI<V1Pod>(Cluster.Verb.Delete).Should().BeFalse();
        cluster.CanI<V1Pod>(Cluster.Verb.Get).Should().BeFalse();
        cluster.CanI<V1Pod>(Cluster.Verb.List).Should().BeFalse();
        cluster.CanI<V1Pod>(Cluster.Verb.Patch).Should().BeFalse();
        cluster.CanI<V1Pod>(Cluster.Verb.Update).Should().BeFalse();
        cluster.CanI<V1Pod>(Cluster.Verb.Watch).Should().BeFalse();

        cluster.CanI<V1Pod>(Cluster.Verb.Get, "log").Should().BeFalse();
        cluster.CanI<V1Pod>(Cluster.Verb.Create, "exec").Should().BeFalse();
        cluster.CanI<V1Pod>(Cluster.Verb.Create, "portforward").Should().BeFalse();

        cluster.CanI<V1Pod>(Cluster.Verb.Create, "my-app").Should().BeFalse();
        cluster.CanI<V1Pod>(Cluster.Verb.Delete, "my-app").Should().BeTrue();
        cluster.CanI<V1Pod>(Cluster.Verb.Get, "my-app").Should().BeTrue();
        cluster.CanI<V1Pod>(Cluster.Verb.List, "my-app").Should().BeTrue();
        cluster.CanI<V1Pod>(Cluster.Verb.Patch, "my-app").Should().BeFalse();
        cluster.CanI<V1Pod>(Cluster.Verb.Update, "my-app").Should().BeFalse();
        cluster.CanI<V1Pod>(Cluster.Verb.Watch, "my-app").Should().BeTrue();

        cluster.CanI<V1Pod>(Cluster.Verb.Get, "my-app", "log").Should().BeTrue();
        cluster.CanI<V1Pod>(Cluster.Verb.Create, "my-app", "exec").Should().BeTrue();
        cluster.CanI<V1Pod>(Cluster.Verb.Create, "my-app", "portforward").Should().BeTrue();
    }

    #endregion
}
