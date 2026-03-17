using System.Collections;
using System.Text;
using Avalonia;
using Avalonia.Headless.XUnit;
using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Client;
using Shouldly;

namespace KubeUI.Tests.E2E;

public class ClusterEndToEndTests
{
    [AvaloniaFact]
    public async Task CreateObject()
    {
        using var testHarness = new TestHarness();
        await testHarness.Initialize();

        await testHarness.Cluster.SeedResource<V1Namespace>(true);

        var ns = new V1Namespace()
        {
            ApiVersion = V1Namespace.KubeApiVersion,
            Kind = V1Namespace.KubeKind,
            Metadata = new V1ObjectMeta()
            {
                Name = "test"
            }
        };

        await testHarness.Cluster.AddOrUpdateResource(ns);

        var resource = await WaitForResourceAsync<V1Namespace>(testHarness.Cluster, null, "test");
        resource.ShouldNotBeNull();
        resource.Name().ShouldBe("test");
    }

    [AvaloniaFact]
    public async Task CreateNamespacedObject()
    {
        using var testHarness = new TestHarness();
        await testHarness.Initialize();

        await testHarness.Cluster.SeedResource<V1Secret>(true);

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

        await testHarness.Cluster.AddOrUpdateResource(secret);

        var resource = await WaitForResourceAsync<V1Secret>(testHarness.Cluster, "default", "test");
        resource.ShouldNotBeNull();
        resource.Name().ShouldBe("test");
        resource.Namespace().ShouldBe("default");
    }

    [AvaloniaFact]
    public async Task ReadObjects()
    {
        using var testHarness = new TestHarness();
        await testHarness.Initialize();

        await testHarness.Cluster.SeedResource<V1Namespace>(true);

        var resources = testHarness.Cluster.GetResourceList<V1Namespace>();
        resources.Count.ShouldBeGreaterThan(1);
    }

    [AvaloniaFact]
    public async Task UpdateObject()
    {
        using var testHarness = new TestHarness();
        await testHarness.Initialize();

        await testHarness.Cluster.SeedResource<V1Namespace>(true);

        var ns = new V1Namespace()
        {
            ApiVersion = V1Namespace.KubeApiVersion,
            Kind = V1Namespace.KubeKind,
            Metadata = new V1ObjectMeta()
            {
                Name = "test",
            }
        };

        ns = await testHarness.Kubernetes.CoreV1.CreateNamespaceAsync(ns);

        ns.Metadata.Labels = new Dictionary<string, string>
        {
            { "test", "test" }
        };

        await testHarness.Cluster.AddOrUpdateResource(ns);

        await Task.Delay(TimeSpan.FromSeconds(2));

        var resource = await WaitForResourceAsync<V1Namespace>(testHarness.Cluster, null, ns.Name());
        resource.ShouldNotBeNull();
        resource.Name().ShouldBe("test");
        resource.Metadata.Labels["test"].ShouldBe("test");
    }

    [AvaloniaFact]
    public async Task UpdateNamespacedObject()
    {
        using var testHarness = new TestHarness();
        await testHarness.Initialize();

        await testHarness.Cluster.SeedResource<V1Secret>(true);

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

        secret = await testHarness.Kubernetes.CoreV1.CreateNamespacedSecretAsync(secret, "default");

        secret.Metadata.Labels = new Dictionary<string, string>
        {
            { "test", "test" }
        };

        await testHarness.Cluster.AddOrUpdateResource(secret);

        await Task.Delay(TimeSpan.FromSeconds(2));

        var resource = await WaitForResourceAsync<V1Secret>(testHarness.Cluster, "default", "test");
        resource.ShouldNotBeNull();
        resource.Name().ShouldBe("test");
        resource.Namespace().ShouldBe("default");
        resource.Metadata.Labels["test"].ShouldBe("test");
    }

    [AvaloniaFact]
    public async Task DeleteObject()
    {
        using var testHarness = new TestHarness();
        await testHarness.Initialize();

        await testHarness.Cluster.SeedResource<V1Namespace>(true);

        var ns = new V1Namespace()
        {
            ApiVersion = V1Namespace.KubeApiVersion,
            Kind = V1Namespace.KubeKind,
            Metadata = new V1ObjectMeta()
            {
                Name = "test"
            }
        };

        await testHarness.Kubernetes.CoreV1.CreateNamespaceAsync(ns);

        await WaitForResourceAsync<V1Namespace>(testHarness.Cluster, null, "test");

        await testHarness.Cluster.DeleteResource(ns);

        await Task.Delay(TimeSpan.FromSeconds(10));

        testHarness.Cluster.GetResourceList<V1Namespace>().All(x => x.Name() != "test").ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task DeleteNamespacedObject()
    {
        using var testHarness = new TestHarness();
        await testHarness.Initialize();

        await testHarness.Cluster.SeedResource<V1Secret>(true);

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

        await WaitForResourceAsync<V1Secret>(testHarness.Cluster, "default", "test");

        await testHarness.Cluster.DeleteResource(secret);

        await Task.Delay(TimeSpan.FromSeconds(2));

        testHarness.Cluster.GetResourceList<V1Secret>().All(x => x.Name() != "test").ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task ImportYaml()
    {
        using var testHarness = new TestHarness();
        await testHarness.Initialize();

        await testHarness.Cluster.SeedResource<V1Namespace>(true);

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

        await testHarness.Cluster.ImportYaml(stream);

        var resource = await WaitForResourceAsync<V1Namespace>(testHarness.Cluster, null, "test");
        resource.ShouldNotBeNull();
        resource.Name().ShouldBe("test");
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

        await testHarness.Cluster.SeedResource<V1CustomResourceDefinition>(true);

        await testHarness.Kubernetes.CreateCustomResourceDefinitionAsync(KubernetesYaml.Deserialize<V1CustomResourceDefinition>(yamlCRD));

        await WaitForResourceAsync<V1CustomResourceDefinition>(testHarness.Cluster, null, "tests.kubeui.com");

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

        var _seedMethodInfo = testHarness.Cluster.GetType().GetMethod(nameof(Cluster.SeedResource));

        var fooRef = _seedMethodInfo.MakeGenericMethod(type);
        await (Task)fooRef.Invoke(testHarness.Cluster, [true]);

        var kind = testHarness.Cluster.Objects[GroupApiVersionKind.From(type)];

        var items = kind.GetType().GetProperty("Items").GetValue(kind);

        items.GetType().GetProperty("Count").GetValue(items).ShouldBe(1);

        foreach (object item in (IList)items.GetType().GetProperty("Items").GetValue(items))
        {
            var obj = (IKubernetesObject<V1ObjectMeta>)item;

            obj.Name().ShouldBe("test1");
            obj.Namespace().ShouldBe("default");
            var spec = obj.GetType().GetProperty("Spec").GetValue(obj);
            spec.GetType().GetProperty("SomeString").GetValue(spec).ShouldBe("myValue");
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

        await testHarness.Cluster.SeedResource<V1ServiceAccount>(true);

        await WaitForResourceAsync<V1ServiceAccount>(testHarness.Cluster, "my-app", "my-serviceaccount");

        var cluster = await testHarness.GetClusterFromServiceAccountSecret("my-app", "my-serviceaccount");

        await cluster.Connect();

        await cluster.SeedResource<V1Node>(true);

        await cluster.SeedResource<V1Secret>(true);

        var nodes = cluster.GetResourceList<V1Node>();
        nodes.Count.ShouldBe(1);

        var secrets = cluster.GetResourceList<V1Secret>();

        secrets.Count.ShouldBe(1);
        secrets[0].Namespace().ShouldBe("my-app");
        secrets[0].Name().ShouldBe("my-serviceaccount");
    }

    [AvaloniaFact]
    public async Task LimitedAccessNoNamespace()
    {
        using var testHarness = new TestHarness();
        await testHarness.Initialize();

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(yamlLimitedRbacNoNamespace));

        await testHarness.Cluster.ImportYaml(stream);

        await testHarness.Cluster.SeedResource<V1ServiceAccount>(true);

        await WaitForResourceAsync<V1ServiceAccount>(testHarness.Cluster, "my-app", "my-serviceaccount");

        var cluster = await testHarness.GetClusterFromServiceAccountSecret("my-app", "my-serviceaccount");

        var settings = Application.Current.GetRequiredService<ISettingsService>();

        settings.Settings = new();

        settings.Settings.GetClusterSettings(cluster).Namespaces.Add("my-app");

        await cluster.Connect();

        await cluster.SeedResource<V1Node>(true);

        await cluster.SeedResource<V1Secret>(true);

        var nodes = cluster.GetResourceList<V1Node>();
        nodes.Count.ShouldBe(1);

        var secrets = cluster.GetResourceList<V1Secret>();
        secrets.Count.ShouldBe(1);
        secrets[0].Namespace().ShouldBe("my-app");
        secrets[0].Name().ShouldBe("my-serviceaccount");
    }

    [AvaloniaFact]
    public async Task RootAccessCanI()
    {
        using var testHarness = new TestHarness();
        await testHarness.Initialize();

        var cluster = testHarness.Cluster;

        (await cluster.UpdateCanI<V1Pod>(Verb.Create)).ShouldBeTrue();
        (await cluster.UpdateCanI<V1Pod>(Verb.Delete)).ShouldBeTrue();
        (await cluster.UpdateCanI<V1Pod>(Verb.Get)).ShouldBeTrue();
        (await cluster.UpdateCanI<V1Pod>(Verb.List)).ShouldBeTrue();
        (await cluster.UpdateCanI<V1Pod>(Verb.Patch)).ShouldBeTrue();
        (await cluster.UpdateCanI<V1Pod>(Verb.Update)).ShouldBeTrue();
        (await cluster.UpdateCanI<V1Pod>(Verb.Watch)).ShouldBeTrue();

        (await cluster.UpdateCanI<V1Pod>(Verb.Get, "log")).ShouldBeTrue();
        (await cluster.UpdateCanI<V1Pod>(Verb.Create, "exec")).ShouldBeTrue();
        (await cluster.UpdateCanI<V1Pod>(Verb.Create, "portforward")).ShouldBeTrue();

        (await cluster.UpdateCanI<V1Pod>(Verb.Create, "default")).ShouldBeTrue();
        (await cluster.UpdateCanI<V1Pod>(Verb.Delete, "default")).ShouldBeTrue();
        (await cluster.UpdateCanI<V1Pod>(Verb.Get, "default")).ShouldBeTrue();
        (await cluster.UpdateCanI<V1Pod>(Verb.List, "default")).ShouldBeTrue();
        (await cluster.UpdateCanI<V1Pod>(Verb.Patch, "default")).ShouldBeTrue();
        (await cluster.UpdateCanI<V1Pod>(Verb.Update, "default")).ShouldBeTrue();
        (await cluster.UpdateCanI<V1Pod>(Verb.Watch, "default")).ShouldBeTrue();

        (await cluster.UpdateCanI<V1Pod>(Verb.Get, "default", "log")).ShouldBeTrue();
        (await cluster.UpdateCanI<V1Pod>(Verb.Create, "default", "exec")).ShouldBeTrue();
        (await cluster.UpdateCanI<V1Pod>(Verb.Create, "default", "portforward")).ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task LimitedAccessCanI()
    {
        using var testHarness = new TestHarness();
        await testHarness.Initialize();

        using var stream = new MemoryStream(Encoding.UTF8.GetBytes(yamlLimitedRbacNoNamespace));

        await testHarness.Cluster.ImportYaml(stream);
        await testHarness.Cluster.SeedResource<V1ServiceAccount>(true);
        await WaitForResourceAsync<V1ServiceAccount>(testHarness.Cluster, "my-app", "my-serviceaccount");

        var cluster = await testHarness.GetClusterFromServiceAccountSecret("my-app", "my-serviceaccount");

        var settings = Application.Current.GetRequiredService<ISettingsService>();

        settings.Settings = new();

        settings.Settings.GetClusterSettings(cluster).Namespaces.Add("my-app");

        await cluster.Connect();

        await cluster.SeedResource<V1Pod>(true);

        (await cluster.UpdateCanI<V1Namespace>(Verb.Create)).ShouldBeFalse();
        (await cluster.UpdateCanI<V1Namespace>(Verb.Delete)).ShouldBeFalse();
        (await cluster.UpdateCanI<V1Namespace>(Verb.Get)).ShouldBeFalse();
        (await cluster.UpdateCanI<V1Namespace>(Verb.List)).ShouldBeFalse();
        (await cluster.UpdateCanI<V1Namespace>(Verb.Patch)).ShouldBeFalse();
        (await cluster.UpdateCanI<V1Namespace>(Verb.Update)).ShouldBeFalse();
        (await cluster.UpdateCanI<V1Namespace>(Verb.Watch)).ShouldBeFalse();

        (await cluster.UpdateCanI<V1Pod>(Verb.Create)).ShouldBeFalse();
        (await cluster.UpdateCanI<V1Pod>(Verb.Delete)).ShouldBeFalse();
        (await cluster.UpdateCanI<V1Pod>(Verb.Get)).ShouldBeFalse();
        (await cluster.UpdateCanI<V1Pod>(Verb.List)).ShouldBeFalse();
        (await cluster.UpdateCanI<V1Pod>(Verb.Patch)).ShouldBeFalse();
        (await cluster.UpdateCanI<V1Pod>(Verb.Update)).ShouldBeFalse();
        (await cluster.UpdateCanI<V1Pod>(Verb.Watch)).ShouldBeFalse();

        (await cluster.UpdateCanI<V1Pod>(Verb.Get, "log")).ShouldBeFalse();
        (await cluster.UpdateCanI<V1Pod>(Verb.Create, "exec")).ShouldBeFalse();
        (await cluster.UpdateCanI<V1Pod>(Verb.Create, "portforward")).ShouldBeFalse();

        (await cluster.UpdateCanI<V1Pod>(Verb.Create, "my-app")).ShouldBeFalse();
        (await cluster.UpdateCanI<V1Pod>(Verb.Delete, "my-app")).ShouldBeTrue();
        (await cluster.UpdateCanI<V1Pod>(Verb.Get, "my-app")).ShouldBeTrue();
        (await cluster.UpdateCanI<V1Pod>(Verb.List, "my-app")).ShouldBeTrue();
        (await cluster.UpdateCanI<V1Pod>(Verb.Patch, "my-app")).ShouldBeFalse();
        (await cluster.UpdateCanI<V1Pod>(Verb.Update, "my-app")).ShouldBeFalse();
        (await cluster.UpdateCanI<V1Pod>(Verb.Watch, "my-app")).ShouldBeTrue();

        (await cluster.UpdateCanI<V1Pod>(Verb.Get, "my-app", "log")).ShouldBeTrue();
        (await cluster.UpdateCanI<V1Pod>(Verb.Create, "my-app", "exec")).ShouldBeTrue();
        (await cluster.UpdateCanI<V1Pod>(Verb.Create, "my-app", "portforward")).ShouldBeTrue();
    }

    #endregion

    private static async Task<T?> WaitForResourceAsync<T>(ICluster cluster, string? @namespace, string name, TimeSpan? timeout = null, int pollIntervalMs = 100) where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        var effectiveTimeout = timeout ?? TimeSpan.FromSeconds(30);
        var start = DateTime.UtcNow;

        while ((DateTime.UtcNow - start) < effectiveTimeout)
        {
            var resource = cluster.GetResource<T>(@namespace, name);
            if (resource != null)
                return resource;
            await Task.Delay(pollIntervalMs);
        }
        return null;
    }
}

