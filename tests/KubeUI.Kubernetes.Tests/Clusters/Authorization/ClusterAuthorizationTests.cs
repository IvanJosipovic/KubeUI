using System.Reflection;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubernetesCRDModelGen;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;

namespace KubeUI.Kubernetes.Tests.Clusters.Authorization;

public sealed class ClusterAuthorizationTests
{
    [Theory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task cani_returns_false_when_permission_review_has_not_been_cached_yet(KubernetesBackend backend)
    {
        TestClusterConfig config = new()
        {
            Type = backend,
        };
        await using var testCluster = await new TestClusterGenerator().CreateAsync(
            config,
            TestContext.Current.CancellationToken);

        testCluster.Cluster.CanI(typeof(V1Pod), Verb.Create, subresource: "portforward").ShouldBeFalse();
    }

    [Theory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task cani_any_namespace_uses_namespace_scoped_permission_when_cluster_scope_is_denied(KubernetesBackend backend)
    {
        await using var harness = await new TestClusterGenerator().CreateAsync(
            new TestClusterConfig { Type = backend },
            TestContext.Current.CancellationToken);
        var cluster = (Cluster)await harness.CreateLimitedAccessAsync(
            KubernetesTestData.LimitedAccessWithNamespaceFallback,
            useNamespaceFallback: true,
            cancellationToken: TestContext.Current.CancellationToken);
        await cluster.Permissions.UpdatePermissionsAllNamespaceAsync<V1Pod>(Verb.Create, "portforward");

        cluster.CanIAnyNamespace<V1Pod>(Verb.Create, "portforward").ShouldBeTrue();
    }

    [Theory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task globally_allowed_namespaced_permission_skips_namespace_reviews(KubernetesBackend backend)
    {
        await using var harness = await new TestClusterGenerator().CreateAsync(
            new TestClusterConfig
            {
                Type = backend,
                AuthenticatedUser = KubernetesRbac.ServiceAccountUser,
                InitialResources =
                [
                    (new V1Namespace { Metadata = new V1ObjectMeta { Name = "my-app" } }),
                    (new V1ServiceAccount
                    {
                        Metadata = new V1ObjectMeta
                        {
                            Name = KubernetesRbac.ServiceAccountName,
                            NamespaceProperty = KubernetesRbac.ServiceAccountNamespace,
                        },
                    }),
                    .. KubernetesRbac.ClusterWide(
                                new RbacRule("namespaces", "list"),
                                new RbacRule("namespaces", "watch"),
                                new RbacRule("pods", "list")),
                ],
            },
            TestContext.Current.CancellationToken);
        var cluster = harness.Cluster;
        await cluster.Connect();
        var authorizationRequestsBeforeUpdate = harness.AuthorizationRequestCount;

        await cluster.UpdatePermissionsAllNamespaceAsync<V1Pod>(Verb.List);

        cluster.CanI<V1Pod>(Verb.List, "my-app").ShouldBeTrue();
        if (backend == KubernetesBackend.Fake)
        {
            harness.AuthorizationRequestCount.ShouldBe(authorizationRequestsBeforeUpdate + 1);
        }
    }

    [Theory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task removing_seeded_resource_container_removes_the_container(KubernetesBackend backend)
    {
        await using var harness = await new TestClusterGenerator().CreateAsync(
            new TestClusterConfig { Type = backend },
            TestContext.Current.CancellationToken);
        var cluster = harness.Cluster;

        var kind = GroupApiVersionKind.From<V1Pod>();
        await cluster.SeedResource<V1Pod>(waitForReady: true);
        cluster.Objects.ContainsKey(kind).ShouldBeTrue();

        var invalidateSeededResourceMethod = typeof(Cluster).GetMethod("InvalidateSeededResource", BindingFlags.Instance | BindingFlags.NonPublic);
        invalidateSeededResourceMethod.ShouldNotBeNull();

        var invalidated = invalidateSeededResourceMethod!.Invoke(cluster, [typeof(V1Pod)]).ShouldBeOfType<bool>();

        invalidated.ShouldBeTrue();
        cluster.Objects.ContainsKey(kind).ShouldBeFalse();
    }

    [Fact]
    public async Task resource_container_runs_only_one_seed_factory_for_concurrent_requests()
    {
        var container = new ContainerClass<V1Pod>();
        var seedStarted = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseSeed = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var seedCount = 0;

        Task SeedAsync()
        {
            Interlocked.Increment(ref seedCount);
            seedStarted.SetResult(null);
            return releaseSeed.Task;
        }

        var requests = Enumerable.Range(0, 32)
            .Select(_ => Task.Run(
                async () => await container.GetOrCreateSeedTask(SeedAsync).Value,
                TestContext.Current.CancellationToken))
            .ToArray();

        await seedStarted.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
        seedCount.ShouldBe(1);

        releaseSeed.SetResult(null);
        await Task.WhenAll(requests);
        seedCount.ShouldBe(1);
    }

    [Fact]
    public async Task custom_resource_definition_processing_is_serialized()
    {
        using var loggerFactory = NullLoggerFactory.Instance;
        await using var cluster = new Cluster(
            NullLogger<Cluster>.Instance,
            loggerFactory,
            new ModelCache(),
            new Generator(),
            new KubernetesTestSettingsStore(),
            new ServiceCollection().BuildServiceProvider());
        cluster.Connected = true;

        var first = Serialization.KubernetesYaml.Deserialize<V1CustomResourceDefinition>(KubernetesTestData.CustomResourceDefinitionYaml);
        var second = Serialization.KubernetesYaml.Deserialize<V1CustomResourceDefinition>(KubernetesTestData.CustomResourceDefinitionYaml);
        second.Spec!.Names!.Kind = "UpdatedTest";

        var firstEntered = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var releaseFirst = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var secondReady = new TaskCompletionSource<object?>(TaskCreationOptions.RunContinuationsAsynchronously);
        var readyCount = 0;
        Func<V1CustomResourceDefinition, Task> handler = async crd =>
        {
            if (Interlocked.Increment(ref readyCount) == 1)
            {
                firstEntered.TrySetResult(null);
                await releaseFirst.Task.WaitAsync(TestContext.Current.CancellationToken);
            }
            else
            {
                secondReady.TrySetResult(null);
            }
        };
        cluster.OnCustomResourceDefinitionReady += handler;

        try
        {
            var queueMethod = typeof(Cluster).GetMethod("QueueCustomResourceDefinition", BindingFlags.Instance | BindingFlags.NonPublic);
            queueMethod.ShouldNotBeNull();

            queueMethod!.Invoke(cluster, [first]);
            await firstEntered.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
            queueMethod.Invoke(cluster, [second]);

            Volatile.Read(ref readyCount).ShouldBe(1);

            releaseFirst.TrySetResult(null);
            await secondReady.Task.WaitAsync(TimeSpan.FromSeconds(5), TestContext.Current.CancellationToken);
            readyCount.ShouldBe(2);
        }
        finally
        {
            releaseFirst.TrySetResult(null);
            cluster.OnCustomResourceDefinitionReady -= handler;
        }
    }

}
