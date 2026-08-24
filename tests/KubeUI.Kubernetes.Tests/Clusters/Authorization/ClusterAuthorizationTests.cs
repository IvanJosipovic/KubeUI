using System.Reflection;
using System.Net;
using k8s.Models;
using KubernetesClient.Informer.Client;
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

        testCluster.Cluster.CanI(GroupApiVersionKind.From<V1Pod>(), Verb.Create, subresource: "portforward").ShouldBeFalse();
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
        await TestWait.UntilAsync(
            () => cluster.Namespaces.Any(item => item.Name() == "my-app"),
            TimeSpan.FromSeconds(10),
            cancellationToken: TestContext.Current.CancellationToken);
        await TestWait.UntilAsync(
            () => cluster.IsResourceNamespaced<V1Pod>(),
            TimeSpan.FromSeconds(10),
            cancellationToken: TestContext.Current.CancellationToken);
        await cluster.Permissions.UpdatePermissionsAllNamespaceAsync<V1Pod>(Verb.Create, "portforward");

        await TestWait.UntilAsync(
            () => cluster.CanIAnyNamespace<V1Pod>(Verb.Create, "portforward"),
            TimeSpan.FromSeconds(10),
            cancellationToken: TestContext.Current.CancellationToken);
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

        var invalidated = invalidateSeededResourceMethod!.Invoke(cluster, [kind]).ShouldBeOfType<bool>();

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
    public async Task openapi_schema_loading_retries_after_connection_failures()
    {
        await using var harness = await new TestClusterGenerator().CreateAsync(
            new TestClusterConfig { Type = KubernetesBackend.Fake },
            TestContext.Current.CancellationToken);

        harness.FakeApi.ShouldNotBeNull();
        harness.FakeApi!.OpenApiV3IndexFailuresRemaining = 2;

        await harness.Cluster.Connect();
        harness.FakeApi.OpenApiV3IndexFailuresRemaining = 0;

        await harness.Cluster.EnsureOpenApiSchemasAsync();
        await harness.Cluster.SeedResource<V1Pod>();

        harness.Cluster.ModelCatalog.OpenApiSchemas
            .GetSchema(GroupApiVersionKind.From<V1Pod>())
            .ShouldNotBeNull();
    }

    [Fact]
    public async Task connect_succeeds_when_openapi_v3_is_forbidden()
    {
        await using var harness = await new TestClusterGenerator().CreateAsync(
            new TestClusterConfig { Type = KubernetesBackend.Fake },
            TestContext.Current.CancellationToken);

        harness.FakeApi.ShouldNotBeNull();
        harness.FakeApi!.OpenApiV3IndexStatusCode = HttpStatusCode.Forbidden;

        await harness.Cluster.Connect();

        harness.Cluster.Connected.ShouldBeTrue($"LastError: {harness.Cluster.LastError}; Requests: {string.Join(",", harness.FakeApi.RequestUris.Select(uri => uri?.PathAndQuery))}");
        harness.Cluster.Status.ShouldBe(ClusterStatus.Connected);
    }

    [Fact]
    public async Task connect_succeeds_when_an_openapi_v3_document_is_forbidden()
    {
        await using var harness = await new TestClusterGenerator().CreateAsync(
            new TestClusterConfig { Type = KubernetesBackend.Fake },
            TestContext.Current.CancellationToken);

        harness.FakeApi.ShouldNotBeNull();
        harness.FakeApi!.OpenApiV3DocumentStatusCode = HttpStatusCode.Forbidden;

        await harness.Cluster.Connect();

        harness.Cluster.Connected.ShouldBeTrue($"LastError: {harness.Cluster.LastError}; Requests: {string.Join(",", harness.FakeApi.RequestUris.Select(uri => uri?.PathAndQuery))}");
        harness.Cluster.Status.ShouldBe(ClusterStatus.Connected);
    }

    [Fact]
    public async Task connect_succeeds_when_an_openapi_v3_document_returns_bad_gateway()
    {
        await using var harness = await new TestClusterGenerator().CreateAsync(
            new TestClusterConfig { Type = KubernetesBackend.Fake },
            TestContext.Current.CancellationToken);

        harness.FakeApi.ShouldNotBeNull();
        harness.FakeApi!.OpenApiV3DocumentStatusCode = HttpStatusCode.BadGateway;

        await harness.Cluster.Connect();

        harness.Cluster.Connected.ShouldBeTrue();
        harness.Cluster.Status.ShouldBe(ClusterStatus.Connected);
    }

    [Fact]
    public async Task connect_authenticates_api_discovery_requests()
    {
        await using var harness = await new TestClusterGenerator().CreateAsync(
            new TestClusterConfig { Type = KubernetesBackend.Fake },
            TestContext.Current.CancellationToken);

        harness.FakeApi.ShouldNotBeNull();
        harness.FakeApi!.RequireAuthorizationForDiscovery = true;

        await harness.Cluster.Connect();

        harness.Cluster.Connected.ShouldBeTrue($"LastError: {harness.Cluster.LastError}; Requests: {string.Join(",", harness.FakeApi.RequestUris.Select(uri => uri?.PathAndQuery))}");
        harness.Cluster.Status.ShouldBe(ClusterStatus.Connected);
    }

    [Fact]
    public void custom_resource_definition_resolves_its_served_storage_version()
    {
        var crd = Serialization.KubernetesYaml.Deserialize<V1CustomResourceDefinition>(KubernetesTestData.CustomResourceDefinitionYaml);

        crd.TryGetResourceKind(out var kind).ShouldBeTrue();
        kind.ShouldBe(new GroupApiVersionKind("kubeui.com", "v1beta1", "Test", "tests"));
    }

}
