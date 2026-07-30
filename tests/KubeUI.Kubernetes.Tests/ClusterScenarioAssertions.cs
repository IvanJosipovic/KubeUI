using System.Collections;
using System.Reflection;
using System.Text;
using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Testing;
using Shouldly;

namespace KubeUI.Kubernetes.Tests;

public abstract class ClusterScenarioAssertions
{
    protected abstract Task<IClusterScenarioHarness> CreateHarnessAsync(KubernetesBackend backend);

    protected async Task CreateObjectCore(KubernetesBackend backend)
    {
        await using var harness = await CreateHarnessAsync(backend);
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

    protected async Task CreateNamespacedObjectCore(KubernetesBackend backend)
    {
        await using var harness = await CreateHarnessAsync(backend);
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

    protected async Task ReadObjectsCore(KubernetesBackend backend)
    {
        await using var harness = await CreateHarnessAsync(backend);
        await harness.Cluster.SeedResource<V1Namespace>(true);

        await TestWait.UntilAsync(
            () => harness.Cluster.GetResourceList<V1Namespace>().Count > 0,
            TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken);
    }

    protected async Task UpdateObjectCore(KubernetesBackend backend)
    {
        await using var harness = await CreateHarnessAsync(backend);
        await harness.Cluster.SeedResource<V1Namespace>(true);

        var ns = await harness.CreateDirectAsync(new V1Namespace
        {
            ApiVersion = V1Namespace.KubeApiVersion,
            Kind = V1Namespace.KubeKind,
            Metadata = new V1ObjectMeta { Name = "test" }
        });

        ns.Metadata.Labels = new Dictionary<string, string> { ["test"] = "test" };

        await harness.Cluster.AddOrUpdateResource(ns);

        var resource = await WaitForResourceAsync<V1Namespace>(harness.Cluster, null, "test", predicate: item => item.Metadata.Labels?.TryGetValue("test", out string? value) == true && value == "test");
        resource.ShouldNotBeNull();
        resource.Metadata.Labels["test"].ShouldBe("test");
    }

    protected async Task UpdateNamespacedObjectCore(KubernetesBackend backend)
    {
        await using var harness = await CreateHarnessAsync(backend);
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

        var resource = await WaitForResourceAsync<V1Secret>(harness.Cluster, "default", "test", predicate: item => item.Metadata.Labels?.TryGetValue("test", out string? value) == true && value == "test");
        resource.ShouldNotBeNull();
        resource.Metadata.Labels["test"].ShouldBe("test");
    }

    protected async Task DeleteObjectCore(KubernetesBackend backend)
    {
        await using var harness = await CreateHarnessAsync(backend);
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

    protected async Task DeleteNamespacedObjectCore(KubernetesBackend backend)
    {
        await using var harness = await CreateHarnessAsync(backend);
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

    protected async Task ImportYamlCore(KubernetesBackend backend)
    {
        await using var harness = await CreateHarnessAsync(backend);
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

    protected async Task HandleCrdCore(KubernetesBackend backend)
    {
        await using var harness = await CreateHarnessAsync(backend);
        await harness.Cluster.SeedResource<V1CustomResourceDefinition>(true);

        var crd = KubeUI.Kubernetes.Serialization.KubernetesYaml.Deserialize<V1CustomResourceDefinition>(SharedScenarioData.CustomResourceDefinitionYaml);
        await harness.CreateCustomResourceDefinitionAsync(crd);

        await WaitForResourceAsync<V1CustomResourceDefinition>(harness.Cluster, null, "tests.kubeui.com");
        var generatedType = await WaitForGeneratedTypeAsync(harness.Cluster, "kubeui.com", "v1beta1", "Test");
        generatedType.ShouldNotBeNull();

        if (harness.Cluster is Cluster generatedCluster)
        {
            await generatedCluster.UpdateCanI(generatedType!, Verb.List);
            await generatedCluster.UpdateCanI(generatedType!, Verb.Watch);
        }

        await harness.Cluster.ImportYaml(new MemoryStream(Encoding.UTF8.GetBytes(SharedScenarioData.CustomResourceYaml)));

        var seedMethod = harness.Cluster.GetType()
            .GetMethods(BindingFlags.Instance | BindingFlags.Public)
            .Single(method => method.Name == nameof(IClusterRuntime.SeedResource)
                && method.IsGenericMethodDefinition
                && method.GetParameters().Length == 1);
        await (Task)seedMethod.MakeGenericMethod(generatedType!).Invoke(harness.Cluster, [true])!;

        var kind = harness.Cluster.Objects[GroupApiVersionKind.From(generatedType)];
        var items = kind.GetType().GetProperty("Items")!.GetValue(kind)!;
        await TestWait.UntilAsync(
            () => (int)items.GetType().GetProperty("Count")!.GetValue(items)! == 1,
            TimeSpan.FromSeconds(10),
            cancellationToken: TestContext.Current.CancellationToken);

        foreach (object item in (IList)items.GetType().GetProperty("Items")!.GetValue(items)!)
        {
            var obj = (IKubernetesObject<V1ObjectMeta>)item;
            obj.Name().ShouldBe("test1");
            obj.Namespace().ShouldBe("default");
            var spec = obj.GetType().GetProperty("Spec")!.GetValue(obj)!;
            spec.GetType().GetProperty("SomeString")!.GetValue(spec).ShouldBe("myValue");
        }
    }

    protected async Task RootAccessCanICore(KubernetesBackend backend)
    {
        await using var harness = await CreateHarnessAsync(backend);
        var cluster = harness.Cluster;

        cluster.Permissions.CanI<V1Pod>(Verb.Create).ShouldBeTrue();
        cluster.Permissions.CanI<V1Pod>(Verb.Delete).ShouldBeTrue();
        cluster.Permissions.CanI<V1Pod>(Verb.Get).ShouldBeTrue();
        cluster.Permissions.CanI<V1Pod>(Verb.List).ShouldBeTrue();
        cluster.Permissions.CanI<V1Pod>(Verb.Patch).ShouldBeTrue();
        cluster.Permissions.CanI<V1Pod>(Verb.Update).ShouldBeTrue();
        cluster.Permissions.CanI<V1Pod>(Verb.Watch).ShouldBeTrue();

        cluster.Permissions.CanI<V1Pod>(Verb.Get, subresource: "log").ShouldBeTrue();
        cluster.Permissions.CanI<V1Pod>(Verb.Create, subresource: "exec").ShouldBeTrue();
        cluster.Permissions.CanI<V1Pod>(Verb.Create, subresource: "portforward").ShouldBeTrue();

        cluster.Permissions.CanI<V1Pod>(Verb.Create, "default").ShouldBeTrue();
        cluster.Permissions.CanI<V1Pod>(Verb.Delete, "default").ShouldBeTrue();
        cluster.Permissions.CanI<V1Pod>(Verb.Get, "default").ShouldBeTrue();
        cluster.Permissions.CanI<V1Pod>(Verb.List, "default").ShouldBeTrue();
        cluster.Permissions.CanI<V1Pod>(Verb.Patch, "default").ShouldBeTrue();
        cluster.Permissions.CanI<V1Pod>(Verb.Update, "default").ShouldBeTrue();
        cluster.Permissions.CanI<V1Pod>(Verb.Watch, "default").ShouldBeTrue();

        cluster.Permissions.CanI<V1Pod>(Verb.Get, "default", "log").ShouldBeTrue();
        cluster.Permissions.CanI<V1Pod>(Verb.Create, "default", "exec").ShouldBeTrue();
        cluster.Permissions.CanI<V1Pod>(Verb.Create, "default", "portforward").ShouldBeTrue();
    }

    protected async Task LimitedAccessCore(KubernetesBackend backend, bool includeNamespaceFallback)
    {
        await using var harness = await CreateHarnessAsync(backend);
        var cluster = await harness.CreateLimitedAccessClusterAsync(includeNamespaceFallback);

        await cluster.Connect();
        await cluster.SeedResource<V1Node>(true);
        await cluster.SeedResource<V1Secret>(true);

        await WaitForResourceAsync<V1Node>(cluster, null, "node-1");
        await WaitForResourceAsync<V1Secret>(cluster, "my-app", "my-serviceaccount");

        cluster.GetResourceList<V1Node>().Count.ShouldBe(1);

        var secrets = cluster.GetResourceList<V1Secret>();
        secrets.Count.ShouldBe(1);
        secrets[0].Namespace().ShouldBe("my-app");
        secrets[0].Name().ShouldBe("my-serviceaccount");
    }

    protected async Task LimitedAccessCanICore(KubernetesBackend backend)
    {
        await using var harness = await CreateHarnessAsync(backend);
        var cluster = await harness.CreateLimitedAccessClusterAsync(includeNamespaceFallback: true);

        await cluster.Connect();
        await cluster.SeedResource<V1Pod>(true);

        cluster.Permissions.CanI<V1Namespace>(Verb.Create).ShouldBeFalse();
        cluster.Permissions.CanI<V1Namespace>(Verb.Delete).ShouldBeFalse();
        cluster.Permissions.CanI<V1Namespace>(Verb.Get).ShouldBeFalse();
        cluster.Permissions.CanI<V1Namespace>(Verb.List).ShouldBeFalse();
        cluster.Permissions.CanI<V1Namespace>(Verb.Patch).ShouldBeFalse();
        cluster.Permissions.CanI<V1Namespace>(Verb.Update).ShouldBeFalse();
        cluster.Permissions.CanI<V1Namespace>(Verb.Watch).ShouldBeFalse();

        cluster.Permissions.CanI<V1Pod>(Verb.Create).ShouldBeFalse();
        cluster.Permissions.CanI<V1Pod>(Verb.Delete).ShouldBeFalse();
        cluster.Permissions.CanI<V1Pod>(Verb.Get).ShouldBeFalse();
        cluster.Permissions.CanI<V1Pod>(Verb.List).ShouldBeFalse();
        cluster.Permissions.CanI<V1Pod>(Verb.Patch).ShouldBeFalse();
        cluster.Permissions.CanI<V1Pod>(Verb.Update).ShouldBeFalse();
        cluster.Permissions.CanI<V1Pod>(Verb.Watch).ShouldBeFalse();

        cluster.Permissions.CanI<V1Pod>(Verb.Get, subresource: "log").ShouldBeFalse();
        cluster.Permissions.CanI<V1Pod>(Verb.Create, subresource: "exec").ShouldBeFalse();
        cluster.Permissions.CanI<V1Pod>(Verb.Create, subresource: "portforward").ShouldBeFalse();

        cluster.Permissions.CanI<V1Pod>(Verb.Create, "my-app").ShouldBeFalse();
        cluster.Permissions.CanI<V1Pod>(Verb.Delete, "my-app").ShouldBeTrue();
        cluster.Permissions.CanI<V1Pod>(Verb.Get, "my-app").ShouldBeTrue();
        cluster.Permissions.CanI<V1Pod>(Verb.List, "my-app").ShouldBeTrue();
        cluster.Permissions.CanI<V1Pod>(Verb.Patch, "my-app").ShouldBeFalse();
        cluster.Permissions.CanI<V1Pod>(Verb.Update, "my-app").ShouldBeFalse();
        cluster.Permissions.CanI<V1Pod>(Verb.Watch, "my-app").ShouldBeTrue();

        cluster.Permissions.CanI<V1Pod>(Verb.Get, "my-app", "log").ShouldBeTrue();
        cluster.Permissions.CanI<V1Pod>(Verb.Create, "my-app", "exec").ShouldBeTrue();
        cluster.Permissions.CanI<V1Pod>(Verb.Create, "my-app", "portforward").ShouldBeTrue();

        cluster.Permissions.CanI<V1Deployment>(Verb.Create).ShouldBeFalse();
        cluster.Permissions.CanI<V1Deployment>(Verb.Delete).ShouldBeFalse();
        cluster.Permissions.CanI<V1Deployment>(Verb.Get).ShouldBeFalse();
        cluster.Permissions.CanI<V1Deployment>(Verb.List).ShouldBeFalse();
        cluster.Permissions.CanI<V1Deployment>(Verb.Patch).ShouldBeFalse();
        cluster.Permissions.CanI<V1Deployment>(Verb.Update).ShouldBeFalse();
        cluster.Permissions.CanI<V1Deployment>(Verb.Watch).ShouldBeFalse();

        cluster.Permissions.CanI<V1Deployment>(Verb.Get, "my-app").ShouldBeTrue();
        cluster.Permissions.CanI<V1Deployment>(Verb.List, "my-app").ShouldBeTrue();
        cluster.Permissions.CanI<V1Deployment>(Verb.Watch, "my-app").ShouldBeTrue();
    }

    protected async Task SeedNamespacedResourceAcrossKnownNamespacesCore(KubernetesBackend backend)
    {
        await using var harness = await CreateHarnessAsync(backend);
        var cluster = harness.Cluster;

        await cluster.SeedResource<V1Namespace>(true);
        await harness.CreateDirectAsync(new V1Namespace { Metadata = new() { Name = "team-a" } });
        await harness.CreateDirectAsync(new V1Namespace { Metadata = new() { Name = "team-b" } });
        await harness.CreateDirectAsync(new V1Pod { Metadata = new() { Name = "pod-a", NamespaceProperty = "team-a" } });
        await harness.CreateDirectAsync(new V1Pod { Metadata = new() { Name = "pod-b", NamespaceProperty = "team-b" } });

        await cluster.SeedResource<V1Pod>(true);

        await WaitForResourceAsync<V1Pod>(cluster, "team-a", "pod-a");
        await WaitForResourceAsync<V1Pod>(cluster, "team-b", "pod-b");

        var pods = cluster.GetResourceList<V1Pod>();
        pods.Select(x => x.Namespace()).Order(StringComparer.Ordinal).ShouldBe(["team-a", "team-b"]);
    }

    public static async Task<T?> WaitForResourceAsync<T>(IClusterRuntime cluster, string? @namespace, string name, TimeSpan? timeout = null, int pollIntervalMs = 100, Func<T, bool>? predicate = null, CancellationToken cancellationToken = default)
        where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        var effectiveTimeout = timeout ?? TimeSpan.FromSeconds(30);
        var start = DateTime.UtcNow;

        while ((DateTime.UtcNow - start) < effectiveTimeout)
        {
            var resource = cluster.GetResource<T>(@namespace, name);
            if (resource != null && (predicate == null || predicate(resource)))
            {
                return resource;
            }

            await WaitForNextPollAsync(pollIntervalMs, cancellationToken);
        }

        return null;
    }

    private static async Task WaitForDeletionAsync<T>(IClusterRuntime cluster, string? @namespace, string name, TimeSpan? timeout = null, int pollIntervalMs = 100, CancellationToken cancellationToken = default)
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

            await WaitForNextPollAsync(pollIntervalMs, cancellationToken);
        }

        throw new TimeoutException($"Timed out waiting for deletion of {typeof(T).Name} {@namespace}/{name}.");
    }

    private static async Task<Type?> WaitForGeneratedTypeAsync(IClusterRuntime cluster, string group, string version, string kind, TimeSpan? timeout = null, int pollIntervalMs = 100, CancellationToken cancellationToken = default)
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

            await WaitForNextPollAsync(pollIntervalMs, cancellationToken);
        }

        return null;
    }

    private static async Task WaitForNextPollAsync(int pollIntervalMs, CancellationToken cancellationToken = default)
    {
        cancellationToken = cancellationToken == default ? TestContext.Current.CancellationToken : cancellationToken;
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(pollIntervalMs));
        await timer.WaitForNextTickAsync(cancellationToken);
    }
}
