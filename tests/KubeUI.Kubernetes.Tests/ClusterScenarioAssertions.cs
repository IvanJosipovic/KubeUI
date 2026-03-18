using System.Collections;
using System.Text;
using k8s;
using k8s.Models;
using KubeUI.Kubernetes;
using KubeUI.Kubernetes.Tests.Infra;
using KubernetesClient.Informer.Client;
using Shouldly;

namespace KubeUI.Kubernetes.Tests;

public abstract class ClusterScenarioAssertions
{
    protected abstract Task<IClusterScenarioHarness> CreateHarnessAsync();

    protected async Task CreateObjectCore()
    {
        await using var harness = await CreateHarnessAsync();
        await harness.Cluster.SeedResource<V1Namespace>(true);

        await harness.Cluster.AddOrUpdateResource(new V1Namespace
        {
            ApiVersion = V1Namespace.KubeApiVersion,
            Kind = V1Namespace.KubeKind,
            Metadata = new V1ObjectMeta { Name = "test" }
        });

        var resource = await WaitForResourceAsync<V1Namespace>(harness.Cluster, null, "test");
        resource.ShouldNotBeNull();
        resource.Name().ShouldBe("test");
    }

    protected async Task CreateNamespacedObjectCore()
    {
        await using var harness = await CreateHarnessAsync();
        await harness.Cluster.SeedResource<V1Secret>(true);

        await harness.Cluster.AddOrUpdateResource(new V1Secret
        {
            ApiVersion = V1Secret.KubeApiVersion,
            Kind = V1Secret.KubeKind,
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default"
            },
            StringData = new Dictionary<string, string>
            {
                ["data1"] = "secret1"
            }
        });

        var resource = await WaitForResourceAsync<V1Secret>(harness.Cluster, "default", "test");
        resource.ShouldNotBeNull();
        resource.Name().ShouldBe("test");
        resource.Namespace().ShouldBe("default");
    }

    protected async Task ReadObjectsCore()
    {
        await using var harness = await CreateHarnessAsync();
        await harness.Cluster.SeedResource<V1Namespace>(true);

        harness.Cluster.GetResourceList<V1Namespace>().Count.ShouldBeGreaterThan(0);
    }

    protected async Task UpdateObjectCore()
    {
        await using var harness = await CreateHarnessAsync();
        await harness.Cluster.SeedResource<V1Namespace>(true);

        var ns = await harness.CreateDirectAsync(new V1Namespace
        {
            ApiVersion = V1Namespace.KubeApiVersion,
            Kind = V1Namespace.KubeKind,
            Metadata = new V1ObjectMeta { Name = "test" }
        });

        ns.Metadata.Labels = new Dictionary<string, string> { ["test"] = "test" };

        await harness.Cluster.AddOrUpdateResource(ns);

        var resource = await WaitForResourceAsync<V1Namespace>(harness.Cluster, null, "test");
        resource.ShouldNotBeNull();
        resource.Metadata.Labels["test"].ShouldBe("test");
    }

    protected async Task UpdateNamespacedObjectCore()
    {
        await using var harness = await CreateHarnessAsync();
        await harness.Cluster.SeedResource<V1Secret>(true);

        var secret = await harness.CreateDirectAsync(new V1Secret
        {
            ApiVersion = V1Secret.KubeApiVersion,
            Kind = V1Secret.KubeKind,
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default"
            },
            StringData = new Dictionary<string, string>
            {
                ["data1"] = "secret1"
            }
        });

        secret.Metadata.Labels = new Dictionary<string, string> { ["test"] = "test" };

        await harness.Cluster.AddOrUpdateResource(secret);

        var resource = await WaitForResourceAsync<V1Secret>(harness.Cluster, "default", "test");
        resource.ShouldNotBeNull();
        resource.Metadata.Labels["test"].ShouldBe("test");
    }

    protected async Task DeleteObjectCore()
    {
        await using var harness = await CreateHarnessAsync();
        await harness.Cluster.SeedResource<V1Namespace>(true);

        var ns = await harness.CreateDirectAsync(new V1Namespace
        {
            ApiVersion = V1Namespace.KubeApiVersion,
            Kind = V1Namespace.KubeKind,
            Metadata = new V1ObjectMeta { Name = "test" }
        });

        await WaitForResourceAsync<V1Namespace>(harness.Cluster, null, "test");
        await harness.Cluster.DeleteResource(ns);
        await WaitForDeletionAsync<V1Namespace>(harness.Cluster, null, "test");

        harness.Cluster.GetResourceList<V1Namespace>().All(x => x.Name() != "test").ShouldBeTrue();
    }

    protected async Task DeleteNamespacedObjectCore()
    {
        await using var harness = await CreateHarnessAsync();
        await harness.Cluster.SeedResource<V1Secret>(true);

        var secret = await harness.CreateDirectAsync(new V1Secret
        {
            ApiVersion = V1Secret.KubeApiVersion,
            Kind = V1Secret.KubeKind,
            Metadata = new V1ObjectMeta
            {
                Name = "test",
                NamespaceProperty = "default"
            },
            StringData = new Dictionary<string, string>
            {
                ["data1"] = "secret1"
            }
        });

        await WaitForResourceAsync<V1Secret>(harness.Cluster, "default", "test");
        await harness.Cluster.DeleteResource(secret);
        await WaitForDeletionAsync<V1Secret>(harness.Cluster, "default", "test");

        harness.Cluster.GetResourceList<V1Secret>().All(x => x.Name() != "test").ShouldBeTrue();
    }

    protected async Task ImportYamlCore()
    {
        await using var harness = await CreateHarnessAsync();
        await harness.Cluster.SeedResource<V1Namespace>(true);

        var yaml = KubeUI.Kubernetes.Serialization.KubernetesYaml.Serialize(new V1Namespace
        {
            ApiVersion = V1Namespace.KubeApiVersion,
            Kind = V1Namespace.KubeKind,
            Metadata = new V1ObjectMeta { Name = "test" }
        });

        await harness.Cluster.ImportYaml(new MemoryStream(Encoding.UTF8.GetBytes(yaml)));

        var resource = await WaitForResourceAsync<V1Namespace>(harness.Cluster, null, "test");
        resource.ShouldNotBeNull();
        resource.Name().ShouldBe("test");
    }

    protected async Task HandleCrdCore()
    {
        await using var harness = await CreateHarnessAsync();
        await harness.Cluster.SeedResource<V1CustomResourceDefinition>(true);

        var crd = KubeUI.Kubernetes.Serialization.KubernetesYaml.Deserialize<V1CustomResourceDefinition>(SharedScenarioData.CustomResourceDefinitionYaml);
        await harness.CreateCustomResourceDefinitionAsync(crd);

        await WaitForResourceAsync<V1CustomResourceDefinition>(harness.Cluster, null, "tests.kubeui.com");

        await harness.Cluster.ImportYaml(new MemoryStream(Encoding.UTF8.GetBytes(SharedScenarioData.CustomResourceYaml)));
        var type = await WaitForGeneratedTypeAsync(harness.Cluster, "kubeui.com", "v1beta1", "Test");

        type.ShouldNotBeNull();

        var seedMethod = harness.Cluster.GetType().GetMethod(nameof(IClusterRuntime.SeedResource))!;
        await (Task)seedMethod.MakeGenericMethod(type!).Invoke(harness.Cluster, [true])!;

        var kind = harness.Cluster.Objects[GroupApiVersionKind.From(type)];
        var items = kind.GetType().GetProperty("Items")!.GetValue(kind)!;
        items.GetType().GetProperty("Count")!.GetValue(items).ShouldBe(1);

        foreach (object item in (IList)items.GetType().GetProperty("Items")!.GetValue(items)!)
        {
            var obj = (IKubernetesObject<V1ObjectMeta>)item;
            obj.Name().ShouldBe("test1");
            obj.Namespace().ShouldBe("default");
            var spec = obj.GetType().GetProperty("Spec")!.GetValue(obj)!;
            spec.GetType().GetProperty("SomeString")!.GetValue(spec).ShouldBe("myValue");
        }
    }

    protected async Task RootAccessCanICore()
    {
        await using var harness = await CreateHarnessAsync();
        var cluster = harness.Cluster;

        (await cluster.UpdateCanI<V1Pod>(Verb.Create)).ShouldBeTrue();
        (await cluster.UpdateCanI<V1Pod>(Verb.Delete)).ShouldBeTrue();
        (await cluster.UpdateCanI<V1Pod>(Verb.Get)).ShouldBeTrue();
        (await cluster.UpdateCanI<V1Pod>(Verb.List)).ShouldBeTrue();
        (await cluster.UpdateCanI<V1Pod>(Verb.Patch)).ShouldBeTrue();
        (await cluster.UpdateCanI<V1Pod>(Verb.Update)).ShouldBeTrue();
        (await cluster.UpdateCanI<V1Pod>(Verb.Watch)).ShouldBeTrue();

        (await cluster.UpdateCanI<V1Pod>(Verb.Get, subresource: "log")).ShouldBeTrue();
        (await cluster.UpdateCanI<V1Pod>(Verb.Create, subresource: "exec")).ShouldBeTrue();
        (await cluster.UpdateCanI<V1Pod>(Verb.Create, subresource: "portforward")).ShouldBeTrue();

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

    protected async Task LimitedAccessCore(bool includeNamespaceFallback)
    {
        await using var harness = await CreateHarnessAsync();
        var cluster = await harness.CreateLimitedAccessClusterAsync(includeNamespaceFallback);

        await cluster.Connect();
        await cluster.SeedResource<V1Node>(true);
        await cluster.SeedResource<V1Secret>(true);

        cluster.GetResourceList<V1Node>().Count.ShouldBe(1);

        var secrets = cluster.GetResourceList<V1Secret>();
        secrets.Count.ShouldBe(1);
        secrets[0].Namespace().ShouldBe("my-app");
        secrets[0].Name().ShouldBe("my-serviceaccount");
    }

    protected async Task LimitedAccessCanICore()
    {
        await using var harness = await CreateHarnessAsync();
        var cluster = await harness.CreateLimitedAccessClusterAsync(includeNamespaceFallback: true);

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

        (await cluster.UpdateCanI<V1Pod>(Verb.Get, subresource: "log")).ShouldBeFalse();
        (await cluster.UpdateCanI<V1Pod>(Verb.Create, subresource: "exec")).ShouldBeFalse();
        (await cluster.UpdateCanI<V1Pod>(Verb.Create, subresource: "portforward")).ShouldBeFalse();

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

    public static async Task<T?> WaitForResourceAsync<T>(IClusterRuntime cluster, string? @namespace, string name, TimeSpan? timeout = null, int pollIntervalMs = 100)
        where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        var effectiveTimeout = timeout ?? TimeSpan.FromSeconds(30);
        var start = DateTime.UtcNow;

        while ((DateTime.UtcNow - start) < effectiveTimeout)
        {
            var resource = cluster.GetResource<T>(@namespace, name);
            if (resource != null)
            {
                return resource;
            }

            await Task.Delay(pollIntervalMs);
        }

        return null;
    }

    private static async Task WaitForDeletionAsync<T>(IClusterRuntime cluster, string? @namespace, string name, TimeSpan? timeout = null, int pollIntervalMs = 100)
        where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        var effectiveTimeout = timeout ?? TimeSpan.FromSeconds(30);
        var start = DateTime.UtcNow;

        while ((DateTime.UtcNow - start) < effectiveTimeout)
        {
            if (cluster.GetResource<T>(@namespace, name) == null)
            {
                return;
            }

            await Task.Delay(pollIntervalMs);
        }

        throw new TimeoutException($"Timed out waiting for deletion of {typeof(T).Name} {@namespace}/{name}.");
    }

    private static async Task<Type?> WaitForGeneratedTypeAsync(IClusterRuntime cluster, string group, string version, string kind, TimeSpan? timeout = null, int pollIntervalMs = 100)
    {
        var effectiveTimeout = timeout ?? TimeSpan.FromSeconds(30);
        var start = DateTime.UtcNow;

        while ((DateTime.UtcNow - start) < effectiveTimeout)
        {
            var type = cluster.ModelCache.GetResourceType(group, version, kind);
            if (type != null)
            {
                return type;
            }

            await Task.Delay(pollIntervalMs);
        }

        return null;
    }
}


