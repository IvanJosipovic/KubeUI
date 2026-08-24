using System.Collections;
using System.Net;
using System.Reflection;
using System.Reactive.Linq;
using System.Text;
using System.Text.Json;
using k8s;
using k8s.Autorest;
using k8s.Models;
using KubernetesClient.Informer.Client;
using Shouldly;

namespace KubeUI.Kubernetes.Tests.Clusters.Runtime;

public abstract class ClusterRuntimeAssertions
{
    protected abstract Task<TestCluster> CreateHarnessAsync(KubernetesBackend backend);

    protected async Task InitializationExposesConnectedClusterCore(KubernetesBackend backend)
    {
        await using var harness = await CreateHarnessAsync(backend);
        await RefreshPermissionsAsync<V1Pod>(harness.Cluster, Verb.Get, Verb.List);

        harness.Cluster.Connected.ShouldBeTrue();
        harness.Cluster.Status.ShouldNotBe(ClusterStatus.None);
        harness.Cluster.Name.ShouldNotBeNullOrWhiteSpace();
        harness.Cluster.Client.ShouldNotBeNull();

        harness.Cluster.Permissions.CanI<V1Pod>(Verb.Get).ShouldBeTrue();
        harness.Cluster.Permissions.CanI<V1Pod>(Verb.List, "default").ShouldBeTrue();
    }

    protected async Task DisconnectAndReconnectRestoresClusterCore(KubernetesBackend backend)
    {
        await using var harness = await CreateHarnessAsync(backend);
        IClusterRuntime cluster = harness.Cluster;

        await cluster.Disconnect();

        cluster.Connected.ShouldBeFalse();
        cluster.Client.ShouldBeNull();

        await cluster.Connect();

        cluster.Connected.ShouldBeTrue();
        cluster.Client.ShouldNotBeNull();
        cluster.Status.ShouldBe(ClusterStatus.Connected);
    }

    protected async Task GlobalPermissionsReflectDeniedAndAllowedOperationsCore(KubernetesBackend backend)
    {
        await using var harness = await CreateHarnessAsync(backend);
        var limitedCluster = await harness.CreateLimitedAccessAsync(
            KubernetesTestData.LimitedAccessWithNamespaceFallback,
            cancellationToken: TestContext.Current.CancellationToken);
        await RefreshPermissionsAsync<V1Pod>(limitedCluster, Verb.Get, Verb.List, Verb.Watch);
        await RefreshPermissionsAsync<V1Pod>(harness.Cluster, Verb.Get, Verb.List, Verb.Watch);

        limitedCluster.Permissions.CanI<V1Pod>(Verb.Get).ShouldBeFalse();
        limitedCluster.Permissions.CanI<V1Pod>(Verb.List).ShouldBeFalse();
        limitedCluster.Permissions.CanI<V1Pod>(Verb.Watch).ShouldBeFalse();

        harness.Cluster.Permissions.CanI<V1Pod>(Verb.Get).ShouldBeTrue();
        harness.Cluster.Permissions.CanI<V1Pod>(Verb.List).ShouldBeTrue();
        harness.Cluster.Permissions.CanI<V1Pod>(Verb.Watch).ShouldBeTrue();
    }

    protected async Task NamespacedPermissionsReflectDeniedAndAllowedOperationsCore(KubernetesBackend backend)
    {
        await using var harness = await CreateHarnessAsync(backend);
        var limitedCluster = await harness.CreateLimitedAccessAsync(
            KubernetesTestData.LimitedAccessWithNamespaceFallback,
            cancellationToken: TestContext.Current.CancellationToken);
        await TestWait.UntilAsync(
            () => limitedCluster.IsResourceNamespaced<V1Pod>(),
            TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken);
        await RefreshPermissionsAsync<V1Pod>(limitedCluster, Verb.Get, Verb.List, Verb.Watch);

        limitedCluster.Permissions.CanI<V1Pod>(Verb.Get, "default").ShouldBeFalse();
        limitedCluster.Permissions.CanI<V1Pod>(Verb.List, "default").ShouldBeFalse();
        limitedCluster.Permissions.CanI<V1Pod>(Verb.Watch, "default").ShouldBeFalse();

        limitedCluster.Permissions.CanI<V1Pod>(Verb.Get, "my-app").ShouldBeTrue();
        limitedCluster.Permissions.CanI<V1Pod>(Verb.List, "my-app").ShouldBeTrue();
        limitedCluster.Permissions.CanI<V1Pod>(Verb.Watch, "my-app").ShouldBeTrue();
    }

    protected async Task PodSubresourcePermissionsCoverLogExecAndPortforwardCore(KubernetesBackend backend)
    {
        await using var harness = await CreateHarnessAsync(backend);
        IClusterRuntime rootCluster = harness.Cluster;
        var limitedCluster = await harness.CreateLimitedAccessAsync(
            KubernetesTestData.LimitedAccessWithNamespaceFallback,
            cancellationToken: TestContext.Current.CancellationToken);
        await RefreshPermissionsAsync<V1Pod>(rootCluster, (Verb.Get, "log"), (Verb.Create, "exec"), (Verb.Create, "portforward"));
        await RefreshPermissionsAsync<V1Pod>(limitedCluster, (Verb.Get, "log"), (Verb.Create, "exec"), (Verb.Create, "portforward"));

        await TestWait.UntilAsync(
            () => limitedCluster.Permissions.CanI<V1Pod>(Verb.Get, "my-app", "log")
                && limitedCluster.Permissions.CanI<V1Pod>(Verb.Create, "my-app", "exec")
                && limitedCluster.Permissions.CanI<V1Pod>(Verb.Create, "my-app", "portforward"),
            TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken);

        rootCluster.Permissions.CanI<V1Pod>(Verb.Get, "my-app", "log").ShouldBeTrue();
        rootCluster.Permissions.CanI<V1Pod>(Verb.Create, "my-app", "exec").ShouldBeTrue();
        rootCluster.Permissions.CanI<V1Pod>(Verb.Create, "my-app", "portforward").ShouldBeTrue();

        limitedCluster.Permissions.CanI<V1Pod>(Verb.Get, "my-app", "log").ShouldBeTrue();
        limitedCluster.Permissions.CanI<V1Pod>(Verb.Create, "my-app", "exec").ShouldBeTrue();
        limitedCluster.Permissions.CanI<V1Pod>(Verb.Create, "my-app", "portforward").ShouldBeTrue();

        rootCluster.Permissions.CanI<V1Pod>(Verb.Get, "default", "log").ShouldBeTrue();
        rootCluster.Permissions.CanI<V1Pod>(Verb.Create, "default", "exec").ShouldBeTrue();
        rootCluster.Permissions.CanI<V1Pod>(Verb.Create, "default", "portforward").ShouldBeTrue();
    }

    protected async Task DirectCrudMethodsRoundTripResourcesCore(KubernetesBackend backend)
    {
        await using var harness = await CreateHarnessAsync(backend);
        await SeedResourceAsync<V1Namespace>(harness.Cluster);
        await SeedResourceAsync<V1Secret>(harness.Cluster);

        var createdNamespace = await harness.CreateAsync(
            new V1Namespace { Metadata = new V1ObjectMeta { Name = "direct-crud" } },
            TestContext.Current.CancellationToken);
        createdNamespace.Name().ShouldBe("direct-crud");

        var createdSecret = await harness.CreateAsync(
            new V1Secret
            {
                Metadata = new V1ObjectMeta { Name = "direct-secret", NamespaceProperty = "default" },
                StringData = new Dictionary<string, string> { ["value"] = "before" },
            },
            TestContext.Current.CancellationToken);
        Encoding.UTF8.GetString(createdSecret.Data!["value"]).ShouldBe("before");

        createdSecret.Data["value"] = Encoding.UTF8.GetBytes("after");
        var replacedSecret = await harness.ReplaceAsync(createdSecret, TestContext.Current.CancellationToken);
        Encoding.UTF8.GetString(replacedSecret.Data!["value"]).ShouldBe("after");

        await harness.DeleteAsync(replacedSecret, TestContext.Current.CancellationToken);
        await WaitForDeletionAsync<V1Secret>(harness.Cluster, "default", "direct-secret", cancellationToken: TestContext.Current.CancellationToken);

        await harness.DeleteAsync(createdNamespace, TestContext.Current.CancellationToken);
        await WaitForDeletionAsync<V1Namespace>(harness.Cluster, null, "direct-crud", cancellationToken: TestContext.Current.CancellationToken);
    }

    protected async Task DirectCrudOperationsAreObservedByInformerCacheCore(KubernetesBackend backend)
    {
        await using var harness = await CreateHarnessAsync(backend);
        await SeedResourceAsync<V1ConfigMap>(harness.Cluster);

        var configMap = await harness.CreateAsync(
            new V1ConfigMap
            {
                Metadata = new V1ObjectMeta { Name = "informer-observed", NamespaceProperty = "default" },
                Data = new Dictionary<string, string> { ["state"] = "created" },
            },
            TestContext.Current.CancellationToken);
        (await WaitForResourceAsync<V1ConfigMap>(harness.Cluster, "default", configMap.Name(), cancellationToken: TestContext.Current.CancellationToken)).ShouldNotBeNull();

        configMap.Data!["state"] = "replaced";
        await harness.ReplaceAsync(configMap, TestContext.Current.CancellationToken);
        var replaced = (await WaitForResourceAsync<V1ConfigMap>(
            harness.Cluster,
            "default",
            configMap.Name(),
            predicate: item => item.Data?.TryGetValue("state", out var value) == true && value == "replaced",
            cancellationToken: TestContext.Current.CancellationToken))!;
        replaced.Data!["state"].ShouldBe("replaced");

        await harness.DeleteAsync(replaced, TestContext.Current.CancellationToken);
        await WaitForDeletionAsync<V1ConfigMap>(harness.Cluster, "default", configMap.Name(), cancellationToken: TestContext.Current.CancellationToken);
    }

    protected async Task StaleResourceVersionUpdatesAreRejectedCore(KubernetesBackend backend)
    {
        await using var harness = await CreateHarnessAsync(backend);
        await SeedResourceAsync<V1ConfigMap>(harness.Cluster);

        var created = await harness.CreateAsync(
            new V1ConfigMap
            {
                Metadata = new V1ObjectMeta { Name = "stale-update", NamespaceProperty = "default" },
                Data = new Dictionary<string, string> { ["state"] = "created" },
            },
            TestContext.Current.CancellationToken);

        using var client = harness.Cluster.Client!.GetGenericClient<V1ConfigMap>();
        var staleResourceVersion = created.Metadata.ResourceVersion!;
        var externalUpdate = await client.ReadNamespacedAsync<V1ConfigMap>(
            "default",
            created.Name(),
            TestContext.Current.CancellationToken);
        externalUpdate.Data!["state"] = "external";
        await client.ReplaceNamespacedAsync(
            externalUpdate,
            "default",
            externalUpdate.Name(),
            TestContext.Current.CancellationToken);

        var stale = new V1ConfigMap
        {
            ApiVersion = V1ConfigMap.KubeApiVersion,
            Kind = V1ConfigMap.KubeKind,
            Metadata = new V1ObjectMeta
            {
                Name = created.Name(),
                NamespaceProperty = "default",
                ResourceVersion = staleResourceVersion,
            },
            Data = new Dictionary<string, string> { ["state"] = "stale" },
        };

        var exception = await Should.ThrowAsync<HttpOperationException>(
            () => client.ReplaceNamespacedAsync(stale, "default", stale.Name(), TestContext.Current.CancellationToken));

        exception.Response.StatusCode.ShouldBe(HttpStatusCode.Conflict);
    }

    protected async Task ReplaceDirectRefreshesResourceVersionCore(KubernetesBackend backend)
    {
        await using var harness = await CreateHarnessAsync(backend);
        await SeedResourceAsync<V1ConfigMap>(harness.Cluster);

        var original = await harness.CreateAsync(
            new V1ConfigMap
            {
                Metadata = new V1ObjectMeta { Name = "refresh-version", NamespaceProperty = "default" },
                Data = new Dictionary<string, string> { ["state"] = "created" },
            },
            TestContext.Current.CancellationToken);

        using var client = harness.Cluster.Client!.GetGenericClient<V1ConfigMap>();
        var externalUpdate = await client.ReadNamespacedAsync<V1ConfigMap>(
            "default",
            original.Name(),
            TestContext.Current.CancellationToken);
        externalUpdate.Data!["state"] = "external";
        await client.ReplaceNamespacedAsync(
            externalUpdate,
            "default",
            externalUpdate.Name(),
            TestContext.Current.CancellationToken);

        original.Data!["state"] = "harness";
        var replaced = await harness.ReplaceAsync(original, TestContext.Current.CancellationToken);

        replaced.Data!["state"].ShouldBe("harness");
    }

    protected async Task CreateObjectCore(KubernetesBackend backend)
    {
        await using var harness = await CreateHarnessAsync(backend);
        await SeedResourceAsync<V1Namespace>(harness.Cluster);

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
        await SeedResourceAsync<V1Secret>(harness.Cluster);

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
        await SeedResourceAsync<V1Namespace>(harness.Cluster);

        await TestWait.UntilAsync(
            () => harness.Cluster.GetResourceList<V1Namespace>().Count > 0,
            TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken);
    }

    protected async Task UpdateObjectCore(KubernetesBackend backend)
    {
        await using var harness = await CreateHarnessAsync(backend);
        await SeedResourceAsync<V1Namespace>(harness.Cluster);

        var ns = await harness.CreateAsync(new V1Namespace
        {
            ApiVersion = V1Namespace.KubeApiVersion,
            Kind = V1Namespace.KubeKind,
            Metadata = new V1ObjectMeta { Name = "test" }
        }, TestContext.Current.CancellationToken);

        ns.Metadata.Labels = new Dictionary<string, string> { ["test"] = "test" };

        await harness.Cluster.AddOrUpdateResource(ns);

        var resource = await WaitForResourceAsync<V1Namespace>(harness.Cluster, null, "test", predicate: item => item.Metadata.Labels?.TryGetValue("test", out var value) == true && value == "test");
        resource.ShouldNotBeNull();
        resource.Metadata.Labels["test"].ShouldBe("test");
    }

    protected async Task UpdateNamespacedObjectCore(KubernetesBackend backend)
    {
        await using var harness = await CreateHarnessAsync(backend);
        await SeedResourceAsync<V1Secret>(harness.Cluster);

        var secret = await harness.CreateAsync(new V1Secret
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
        }, TestContext.Current.CancellationToken);

        secret.Metadata.Labels = new Dictionary<string, string> { ["test"] = "test" };

        await harness.Cluster.AddOrUpdateResource(secret);

        var resource = await WaitForResourceAsync<V1Secret>(harness.Cluster, "default", "test", predicate: item => item.Metadata.Labels?.TryGetValue("test", out var value) == true && value == "test");
        resource.ShouldNotBeNull();
        resource.Metadata.Labels["test"].ShouldBe("test");
    }

    protected async Task DeleteObjectCore(KubernetesBackend backend)
    {
        await using var harness = await CreateHarnessAsync(backend);
        await SeedResourceAsync<V1Namespace>(harness.Cluster);

        var ns = await harness.CreateAsync(new V1Namespace
        {
            ApiVersion = V1Namespace.KubeApiVersion,
            Kind = V1Namespace.KubeKind,
            Metadata = new V1ObjectMeta { Name = "test" }
        }, TestContext.Current.CancellationToken);

        await WaitForResourceAsync<V1Namespace>(harness.Cluster, null, "test");
        await harness.Cluster.DeleteResource(ns);
        await WaitForDeletionAsync<V1Namespace>(harness.Cluster, null, "test");

        harness.Cluster.GetResourceList<V1Namespace>().All(x => x.Name() != "test").ShouldBeTrue();
    }

    protected async Task DeleteNamespacedObjectCore(KubernetesBackend backend)
    {
        await using var harness = await CreateHarnessAsync(backend);
        await SeedResourceAsync<V1Secret>(harness.Cluster);

        var secret = await harness.CreateAsync(new V1Secret
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
        }, TestContext.Current.CancellationToken);

        await WaitForResourceAsync<V1Secret>(harness.Cluster, "default", "test");
        await harness.Cluster.DeleteResource(secret);
        await WaitForDeletionAsync<V1Secret>(harness.Cluster, "default", "test");

        harness.Cluster.GetResourceList<V1Secret>().All(x => x.Name() != "test").ShouldBeTrue();
    }

    protected async Task ImportYamlCore(KubernetesBackend backend)
    {
        await using var harness = await CreateHarnessAsync(backend);
        try
        {
            await SeedResourceAsync<V1Namespace>(harness.Cluster);
            await SeedResourceAsync<V1Pod>(harness.Cluster);
            await SeedResourceAsync<V1CustomResourceDefinition>(harness.Cluster);

        var namespaceYaml = Serialization.KubernetesYaml.Serialize(new V1Namespace
        {
            ApiVersion = V1Namespace.KubeApiVersion,
            Kind = V1Namespace.KubeKind,
            Metadata = new V1ObjectMeta { Name = "test" }
        });
        var podYaml = Serialization.KubernetesYaml.Serialize(new V1Pod
        {
            ApiVersion = V1Pod.KubeApiVersion,
            Kind = V1Pod.KubeKind,
            Metadata = new V1ObjectMeta
            {
                Name = "imported-pod",
                NamespaceProperty = "default"
            },
            Spec = new V1PodSpec
            {
                Containers =
                [
                    new V1Container
                    {
                        Name = "app",
                        Image = "busybox:1.36"
                    }
                ]
            }
        });
        var crdYaml = KubernetesTestData.CustomResourceDefinitionYaml;

        using var yamlStream = new MemoryStream(Encoding.UTF8.GetBytes(
            $"{namespaceYaml}\n---\n{podYaml}\n---\n{crdYaml}"));
        await harness.Cluster.ImportYaml(yamlStream);

        var resource = await WaitForResourceAsync<V1Namespace>(harness.Cluster, null, "test");
        resource.ShouldNotBeNull();
        resource.Name().ShouldBe("test");

        var pod = await WaitForResourceAsync<V1Pod>(harness.Cluster, "default", "imported-pod");
        pod.ShouldNotBeNull();
        pod.Name().ShouldBe("imported-pod");

        var crd = await WaitForResourceAsync<V1CustomResourceDefinition>(
            harness.Cluster,
            null,
            "tests.kubeui.com");
        crd.ShouldNotBeNull();
        crd.Name().ShouldBe("tests.kubeui.com");

        harness.Cluster.ModelCatalog.IsCustomResource(
            new GroupApiVersionKind("kubeui.com", "v1beta1", "Test", "tests"))
            .ShouldBeTrue();

        }
        finally
        {
            await harness.Cluster.Disconnect();
        }
    }

    protected async Task ImportYamlCrdInstanceCore(KubernetesBackend backend)
    {
        await using var harness = await CreateHarnessAsync(backend);
        try
        {
            await SeedResourceAsync<V1CustomResourceDefinition>(harness.Cluster);

        var crd = Serialization.KubernetesYaml.Deserialize<V1CustomResourceDefinition>(KubernetesTestData.CustomResourceDefinitionYaml);
        await harness.CreateAsync(crd, TestContext.Current.CancellationToken);

        await WaitForResourceAsync<V1CustomResourceDefinition>(harness.Cluster, null, "tests.kubeui.com");
        var version = crd.Spec.Versions.First(version => version.Served && version.Storage).Name;
        var kind = new GroupApiVersionKind(crd.Spec.Group, version, crd.Spec.Names.Kind, crd.Spec.Names.Plural);

        await TestWait.UntilAsync(
            () => harness.Cluster.ModelCatalog.IsCustomResource(kind),
            TimeSpan.FromSeconds(10),
            cancellationToken: TestContext.Current.CancellationToken);

        await harness.Cluster.Permissions.UpdatePermissionsAllNamespaceAsync(kind, namespaced: true, verb: Verb.List);
        await harness.Cluster.Permissions.UpdatePermissionsAllNamespaceAsync(kind, namespaced: true, verb: Verb.Watch);

        await WaitForCustomResourceApiAsync(
            harness.Cluster,
            kind,
            TestContext.Current.CancellationToken);

        await harness.Cluster.ImportYaml(new MemoryStream(Encoding.UTF8.GetBytes(KubernetesTestData.CustomResourceYaml)));

        await harness.Cluster.SeedResource(kind, waitForReady: true);

        var items = harness.Cluster.GetResourceSourceCache<GenericKubernetesObject>(kind);
        await TestWait.UntilAsync(
            () => items.Count == 1,
            TimeSpan.FromSeconds(10),
            cancellationToken: TestContext.Current.CancellationToken);

        var observedCount = 0;
        using var countSubscription = harness.Cluster.GetResourceCount(kind).Subscribe(count => observedCount = count);
        await TestWait.UntilAsync(
            () => observedCount == 1,
            TimeSpan.FromSeconds(5),
            cancellationToken: TestContext.Current.CancellationToken);

        var item = items.Items.Single();
        var updated = new GenericKubernetesObject
        {
            ApiVersion = item.ApiVersion,
            Kind = item.Kind,
            Metadata = item.Metadata,
            Properties = new Dictionary<string, JsonElement>
            {
                ["spec"] = JsonSerializer.SerializeToElement(new { someString = "updatedValue" }),
            },
        };

        await harness.Cluster.AddOrUpdateResource(updated);
        await TestWait.UntilAsync(
            () => items.Items.Any(candidate =>
                candidate.Properties.TryGetValue("spec", out var spec)
                && spec.TryGetProperty("someString", out var value)
                && value.GetString() == "updatedValue"),
            TimeSpan.FromSeconds(10),
            cancellationToken: TestContext.Current.CancellationToken);

        var observed = items.Items.Single(candidate =>
            candidate.Properties.TryGetValue("spec", out var spec)
            && spec.TryGetProperty("someString", out var value)
            && value.GetString() == "updatedValue");
        observed.Name().ShouldBe("test1");
        observed.Namespace().ShouldBe("default");
        observed.Properties.ShouldNotBeNull();
        await harness.Cluster.DeleteResource(updated);

        await TestWait.UntilAsync(
            () => items.Items.Count == 0,
            TimeSpan.FromSeconds(10),
            cancellationToken: TestContext.Current.CancellationToken);

        }
        finally
        {
            await harness.Cluster.Disconnect();
        }
    }

    protected async Task DryRunYamlResolvesRegisteredNamespacedGenericResourceCore(KubernetesBackend backend)
    {
        await using var harness = await CreateHarnessAsync(backend);
        try
        {
            await SeedResourceAsync<V1CustomResourceDefinition>(harness.Cluster);

            var crd = Serialization.KubernetesYaml.Deserialize<V1CustomResourceDefinition>(KubernetesTestData.CustomResourceDefinitionYaml);
            await harness.CreateAsync(crd, TestContext.Current.CancellationToken);
            var kind = new GroupApiVersionKind("kubeui.com", "v1beta1", "Test", "tests");
            await TestWait.UntilAsync(
                () => harness.Cluster.ModelCatalog.IsCustomResource(kind),
                TimeSpan.FromSeconds(10),
                cancellationToken: TestContext.Current.CancellationToken);
            await TestWait.UntilAsync(
                () => harness.Cluster.IsResourceNamespaced(kind),
                TimeSpan.FromSeconds(10),
                cancellationToken: TestContext.Current.CancellationToken);

            var requestCount = harness.FakeApi?.RequestUris.Count ?? 0;
            using var yamlStream = new MemoryStream(Encoding.UTF8.GetBytes(KubernetesTestData.CustomResourceYaml));
            await harness.Cluster.DryRunYaml(yamlStream);

            if (backend == KubernetesBackend.Fake)
            {
                harness.FakeApi.ShouldNotBeNull();
                var request = harness.FakeApi!.RequestUris
                    .Skip(requestCount)
                    .Single(uri => uri?.Query.Contains("dryRun=All", StringComparison.OrdinalIgnoreCase) == true);
                request!.AbsolutePath.ShouldBe("/apis/kubeui.com/v1beta1/namespaces/default/tests");
            }
        }
        finally
        {
            await harness.Cluster.Disconnect();
        }
    }

    protected async Task RootAccessCanICore(KubernetesBackend backend)
    {
        await using var harness = await CreateHarnessAsync(backend);
        var cluster = harness.Cluster;
        await RefreshPermissionsAsync<V1Pod>(cluster, Enum.GetValues<Verb>());
        await RefreshPermissionsAsync<V1Pod>(cluster, (Verb.Get, "log"), (Verb.Create, "exec"), (Verb.Create, "portforward"));

        cluster.Permissions.CanI<V1Pod>(Verb.Create).ShouldBeTrue();
        cluster.Permissions.CanI<V1Pod>(Verb.Delete).ShouldBeTrue();
        cluster.Permissions.CanI<V1Pod>(Verb.Get).ShouldBeTrue();
        cluster.Permissions.CanI<V1Pod>(Verb.List).ShouldBeTrue();
        cluster.Permissions.CanI<V1Pod>(Verb.Patch).ShouldBeTrue();
        cluster.Permissions.CanI<V1Pod>(Verb.Update).ShouldBeTrue();
        cluster.Permissions.CanI<V1Pod>(Verb.Watch).ShouldBeTrue();

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
        var scenario = includeNamespaceFallback
              ? KubernetesTestData.LimitedAccessWithNamespaceFallback
              : KubernetesTestData.LimitedAccessWithNamespacePermissions;
        var cluster = await harness.CreateLimitedAccessAsync(scenario, includeNamespaceFallback, TestContext.Current.CancellationToken);

        await cluster.Connect();
        await EnsureNodeAsync(harness);
        await SeedResourceAsync<V1Node>(cluster);
        await SeedResourceAsync<V1Secret>(cluster);

        await WaitForResourceAsync<V1Node>(cluster, null, "node-1");
        await WaitForResourceAsync<V1Secret>(cluster, "my-app", "my-serviceaccount");

        cluster.GetResourceList<V1Node>().Count.ShouldBeGreaterThanOrEqualTo(1);
        cluster.GetResource<V1Node>(null, "node-1").ShouldNotBeNull();

        var secrets = cluster.GetResourceList<V1Secret>()
            .Where(secret => secret.Namespace() != "kube-system")
            .ToList();
        secrets.ShouldAllBe(secret => secret.Namespace() == "my-app");
        secrets.ShouldContain(secret => secret.Name() == "my-serviceaccount");
    }

    protected async Task LimitedAccessCanICore(KubernetesBackend backend)
    {
        await using var harness = await CreateHarnessAsync(backend);
        var cluster = await harness.CreateLimitedAccessAsync(
            KubernetesTestData.LimitedAccessWithNamespaceFallback,
            cancellationToken: TestContext.Current.CancellationToken);

        await cluster.Connect();
        await RefreshPermissionsAsync<V1Namespace>(cluster, Enum.GetValues<Verb>());
        await RefreshPermissionsAsync<V1Pod>(cluster, Enum.GetValues<Verb>());
        await RefreshPermissionsAsync<V1Pod>(cluster, (Verb.Get, "log"), (Verb.Create, "exec"), (Verb.Create, "portforward"));
        await RefreshPermissionsAsync<V1Deployment>(cluster, Enum.GetValues<Verb>());
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

        await SeedResourceAsync<V1Namespace>(cluster);
        await harness.CreateAsync(new V1Namespace { Metadata = new() { Name = "team-a" } }, TestContext.Current.CancellationToken);
        await harness.CreateAsync(new V1Namespace { Metadata = new() { Name = "team-b" } }, TestContext.Current.CancellationToken);
        await EnsureServiceAccountAsync(harness, "team-a");
        await EnsureServiceAccountAsync(harness, "team-b");
        await harness.CreateAsync(new V1Pod
        {
            Metadata = new() { Name = "pod-a", NamespaceProperty = "team-a" },
            Spec = new V1PodSpec { Containers = [new V1Container { Name = "app", Image = "busybox" }] },
        }, TestContext.Current.CancellationToken);
        await harness.CreateAsync(new V1Pod
        {
            Metadata = new() { Name = "pod-b", NamespaceProperty = "team-b" },
            Spec = new V1PodSpec { Containers = [new V1Container { Name = "app", Image = "busybox" }] },
        }, TestContext.Current.CancellationToken);

        await SeedResourceAsync<V1Pod>(cluster);

        await WaitForResourceAsync<V1Pod>(cluster, "team-a", "pod-a");
        await WaitForResourceAsync<V1Pod>(cluster, "team-b", "pod-b");

        var pods = cluster.GetResourceList<V1Pod>();
        pods
            .Where(x => x.Namespace() is "team-a" or "team-b")
            .Select(x => x.Namespace())
            .Order(StringComparer.Ordinal)
            .ShouldBe(["team-a", "team-b"]);
    }

    private static async Task EnsureServiceAccountAsync(TestCluster harness, string @namespace)
    {
        try
        {
            await harness.CreateAsync(new V1ServiceAccount
            {
                Metadata = new() { Name = "default", NamespaceProperty = @namespace },
            }, TestContext.Current.CancellationToken);
        }
        catch (HttpOperationException exception) when (exception.Response.StatusCode == HttpStatusCode.Conflict)
        {
        }
    }

    private static async Task EnsureNodeAsync(TestCluster harness)
    {
        try
        {
            await harness.CreateAsync(new V1Node
            {
                Metadata = new() { Name = "node-1" },
            }, TestContext.Current.CancellationToken);
        }
        catch (HttpOperationException exception) when (exception.Response.StatusCode == HttpStatusCode.Conflict)
        {
        }
    }

    private static async Task SeedResourceAsync<T>(IClusterRuntime cluster)
        where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        await RefreshPermissionsAsync<T>(cluster, Verb.List, Verb.Watch);
        await cluster.SeedResource<T>(true).WaitAsync(TestContext.Current.CancellationToken);
    }

    private static Task RefreshPermissionsAsync<T>(IClusterRuntime cluster, params Verb[] verbs)
        where T : class, IKubernetesObject<V1ObjectMeta>, new()
        => RefreshPermissionsAsync<T>(cluster, verbs.Select(static verb => (verb, (string?)null)).ToArray());

    private static async Task RefreshPermissionsAsync<T>(
        IClusterRuntime cluster,
        params (Verb Verb, string? Subresource)[] requests)
        where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        foreach (var request in requests)
        {
            await cluster.Permissions
                .UpdatePermissionsAllNamespaceAsync<T>(request.Verb, request.Subresource)
                .WaitAsync(TestContext.Current.CancellationToken);
        }
    }

    private static async Task WaitForCustomResourceApiAsync(
        IClusterRuntime cluster,
        GroupApiVersionKind kind,
        CancellationToken cancellationToken)
    {
        using var client = cluster.Client!.GetGenericClient(kind);
        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(25));
        var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(10);

        while (true)
        {
            cancellationToken.ThrowIfCancellationRequested();

            try
            {
                await client.ListNamespacedAsync<GenericKubernetesObject>(
                    "default").WaitAsync(cancellationToken);
                return;
            }
            catch (HttpOperationException exception) when (exception.Response.StatusCode == HttpStatusCode.NotFound)
            {
            }

            var remaining = deadline - DateTime.UtcNow;
            if (remaining <= TimeSpan.Zero || !await timer.WaitForNextTickAsync(cancellationToken))
            {
                throw new TimeoutException($"Custom resource API {kind} was not ready within 00:00:10.");
            }
        }
    }

    public static async Task<T?> WaitForResourceAsync<T>(IClusterRuntime cluster, string? @namespace, string name, TimeSpan? timeout = null, int pollIntervalMs = 100, Func<T, bool>? predicate = null, CancellationToken cancellationToken = default)
        where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        return await TestWait.UntilValueAsync(
            () =>
            {
                var resource = cluster.GetResource<T>(@namespace, name);
                return resource != null && (predicate == null || predicate(resource)) ? resource : null;
            },
            timeout ?? TimeSpan.FromSeconds(30),
            TimeSpan.FromMilliseconds(pollIntervalMs),
            cancellationToken == default ? TestContext.Current.CancellationToken : cancellationToken);
    }

    private static async Task WaitForDeletionAsync<T>(IClusterRuntime cluster, string? @namespace, string name, TimeSpan? timeout = null, int pollIntervalMs = 100, CancellationToken cancellationToken = default)
        where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        await TestWait.UntilAsync(
            () => cluster.GetResource<T>(@namespace, name) == null,
            timeout ?? TimeSpan.FromSeconds(30),
            TimeSpan.FromMilliseconds(pollIntervalMs),
            cancellationToken == default ? TestContext.Current.CancellationToken : cancellationToken);
    }

}
