using System.Net;
using System.Reflection;
using k8s;
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
    public async Task namespace_rules_are_reused_across_resources_verbs_and_versions(KubernetesBackend backend)
    {
        await using var harness = await new TestClusterGenerator().CreateAsync(
            new TestClusterConfig
            {
                Type = backend,
                AuthenticatedUser = KubernetesRbac.ServiceAccountUser,
                InitialResources =
                [
                    new V1Namespace { Metadata = new V1ObjectMeta { Name = "my-app" } },
                    new V1ServiceAccount
                    {
                        Metadata = new V1ObjectMeta
                        {
                            Name = KubernetesRbac.ServiceAccountName,
                            NamespaceProperty = KubernetesRbac.ServiceAccountNamespace,
                        },
                    },
                    .. KubernetesRbac.ClusterWide(
                        new RbacRule("namespaces", "list"),
                        new RbacRule("namespaces", "watch")),
                    .. KubernetesRbac.InNamespace(
                        "default",
                        new RbacRule("pods", "get"),
                        new RbacRule("pods", "list"),
                        new RbacRule("pods", "watch"),
                        new RbacRule("deployments", "list", "apps")),
                    .. KubernetesRbac.InNamespace(
                        "my-app",
                        new RbacRule("pods", "get"),
                        new RbacRule("pods", "list"),
                        new RbacRule("pods", "watch"),
                        new RbacRule("deployments", "list", "apps")),
                ],
            },
            TestContext.Current.CancellationToken);
        var cluster = harness.Cluster;
        await cluster.Connect();

        await cluster.Permissions.UpdatePermissionsAllNamespaceAsync<V1Pod>(Verb.List);
        await cluster.Permissions.UpdatePermissionsAllNamespaceAsync<V1Pod>(Verb.Watch);
        await cluster.Permissions.UpdatePermissionsAllNamespaceAsync<V1Deployment>(Verb.List);

        cluster.Permissions.CanI<V1Pod>(Verb.List, "default").ShouldBeTrue();
        cluster.Permissions.CanI<V1Pod>(Verb.Watch, "my-app").ShouldBeTrue();
        cluster.Permissions.CanI<V1Deployment>(Verb.List, "default").ShouldBeTrue();
        cluster.Permissions
            .CanI(new GroupApiVersionKind("", "v9", "Pod", "pods"), Verb.List, "default")
            .ShouldBeTrue();
    }

    [Theory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task namespace_rules_do_not_cross_namespace_boundaries(KubernetesBackend backend)
    {
        await using var harness = await new TestClusterGenerator().CreateAsync(
            new TestClusterConfig
            {
                Type = backend,
                AuthenticatedUser = KubernetesRbac.ServiceAccountUser,
                InitialResources =
                [
                    new V1Namespace { Metadata = new V1ObjectMeta { Name = "my-app" } },
                    new V1ServiceAccount
                    {
                        Metadata = new V1ObjectMeta
                        {
                            Name = KubernetesRbac.ServiceAccountName,
                            NamespaceProperty = KubernetesRbac.ServiceAccountNamespace,
                        },
                    },
                    .. KubernetesRbac.ClusterWide(
                        new RbacRule("namespaces", "list"),
                        new RbacRule("namespaces", "watch")),
                    .. KubernetesRbac.InNamespace("my-app", new RbacRule("secrets", "list")),
                ],
            },
            TestContext.Current.CancellationToken);
        var cluster = harness.Cluster;
        await cluster.Connect();

        await cluster.Permissions.UpdatePermissionsAllNamespaceAsync<V1Secret>(Verb.List);

        cluster.Permissions.CanI<V1Secret>(Verb.List, "my-app").ShouldBeTrue();
        cluster.Permissions.CanI<V1Secret>(Verb.List, "default").ShouldBeFalse();
    }

    // Fake-only server fault injection: a real API server cannot return incomplete rules reviews on demand.
    [Fact]
    public async Task incomplete_namespace_rules_fall_back_to_individual_access_reviews()
    {
        await using var harness = await new TestClusterGenerator().CreateAsync(
            new TestClusterConfig
            {
                AuthenticatedUser = KubernetesRbac.ServiceAccountUser,
                InitialResources =
                [
                    new V1Namespace { Metadata = new V1ObjectMeta { Name = "my-app" } },
                    new V1ServiceAccount
                    {
                        Metadata = new V1ObjectMeta
                        {
                            Name = KubernetesRbac.ServiceAccountName,
                            NamespaceProperty = KubernetesRbac.ServiceAccountNamespace,
                        },
                    },
                    .. KubernetesRbac.ClusterWide(
                        new RbacRule("namespaces", "list"),
                        new RbacRule("namespaces", "watch")),
                    .. KubernetesRbac.InNamespace("my-app", new RbacRule("pods", "list")),
                ],
            },
            TestContext.Current.CancellationToken);
        var cluster = harness.Cluster;
        await cluster.Connect();
        harness.FakeApi!.SelfSubjectRulesReviewIncomplete = true;
        var authorizationBefore = harness.FakeApi.AuthorizationRequestCount;

        await cluster.Permissions.UpdatePermissionsAllNamespaceAsync<V1Pod>(Verb.List);

        cluster.Permissions.CanI<V1Pod>(Verb.List, "my-app").ShouldBeTrue();
        cluster.Permissions.CanI<V1Pod>(Verb.List, "default").ShouldBeFalse();
        harness.FakeApi.AuthorizationRequestCount.ShouldBeGreaterThan(authorizationBefore + 1);
    }

    // Fake-only server fault injection: a real API server cannot fail rules reviews on demand.
    [Fact]
    public async Task failed_rules_review_falls_back_to_individual_access_reviews()
    {
        await using var harness = await new TestClusterGenerator().CreateAsync(
            new TestClusterConfig
            {
                AuthenticatedUser = KubernetesRbac.ServiceAccountUser,
                InitialResources =
                [
                    new V1Namespace { Metadata = new V1ObjectMeta { Name = "my-app" } },
                    new V1ServiceAccount
                    {
                        Metadata = new V1ObjectMeta
                        {
                            Name = KubernetesRbac.ServiceAccountName,
                            NamespaceProperty = KubernetesRbac.ServiceAccountNamespace,
                        },
                    },
                    .. KubernetesRbac.ClusterWide(
                        new RbacRule("namespaces", "list"),
                        new RbacRule("namespaces", "watch")),
                    .. KubernetesRbac.InNamespace("my-app", new RbacRule("pods", "get")),
                ],
            },
            TestContext.Current.CancellationToken);
        var cluster = harness.Cluster;
        await cluster.Connect();
        harness.FakeApi!.SelfSubjectRulesReviewStatusCode = HttpStatusCode.Forbidden;

        await cluster.Permissions.UpdatePermissionsAllNamespaceAsync<V1Pod>(Verb.Get);

        cluster.Permissions.CanI<V1Pod>(Verb.Get, "my-app").ShouldBeTrue();
        cluster.Permissions.CanI<V1Pod>(Verb.Get, "default").ShouldBeFalse();
    }

    [Theory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task resource_names_do_not_grant_unnamed_checks(KubernetesBackend backend)
    {
        await using var harness = await new TestClusterGenerator().CreateAsync(
            new TestClusterConfig
            {
                Type = backend,
                AuthenticatedUser = KubernetesRbac.ServiceAccountUser,
                InitialResources =
                [
                    new V1Namespace { Metadata = new V1ObjectMeta { Name = "my-app" } },
                    new V1ServiceAccount
                    {
                        Metadata = new V1ObjectMeta
                        {
                            Name = KubernetesRbac.ServiceAccountName,
                            NamespaceProperty = KubernetesRbac.ServiceAccountNamespace,
                        },
                    },
                    new V1Role
                    {
                        ApiVersion = "rbac.authorization.k8s.io/v1",
                        Kind = V1Role.KubeKind,
                        Metadata = new V1ObjectMeta { Name = "named-pods", NamespaceProperty = "my-app" },
                        Rules =
                        [
                            new V1PolicyRule
                            {
                                ApiGroups = [""],
                                Resources = ["pods"],
                                ResourceNames = ["only-this-pod"],
                                Verbs = ["get"],
                            },
                        ],
                    },
                    new V1RoleBinding
                    {
                        ApiVersion = "rbac.authorization.k8s.io/v1",
                        Kind = V1RoleBinding.KubeKind,
                        Metadata = new V1ObjectMeta { Name = "named-pods", NamespaceProperty = "my-app" },
                        RoleRef = new V1RoleRef
                        {
                            ApiGroup = "rbac.authorization.k8s.io",
                            Kind = "Role",
                            Name = "named-pods",
                        },
                        Subjects =
                        [
                            new Rbacv1Subject
                            {
                                Kind = "ServiceAccount",
                                NamespaceProperty = KubernetesRbac.ServiceAccountNamespace,
                                Name = KubernetesRbac.ServiceAccountName,
                            },
                        ],
                    },
                    .. KubernetesRbac.ClusterWide(
                        new RbacRule("namespaces", "list"),
                        new RbacRule("namespaces", "watch")),
                ],
            },
            TestContext.Current.CancellationToken);
        var cluster = harness.Cluster;
        await cluster.Connect();

        await cluster.Permissions.UpdatePermissionsAllNamespaceAsync<V1Pod>(Verb.Get);

        cluster.Permissions.CanI<V1Pod>(Verb.Get, "my-app").ShouldBeFalse();
    }

    [Theory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task concurrent_namespace_checks_return_consistent_results(KubernetesBackend backend)
    {
        await using var harness = await new TestClusterGenerator().CreateAsync(
            new TestClusterConfig { Type = backend },
            TestContext.Current.CancellationToken);
        var cluster = harness.Cluster;
        await cluster.Connect();

        var checks = Enumerable.Range(0, 32)
            .Select(_ => cluster.Permissions.UpdateCanI(GroupApiVersionKind.From<V1Pod>(), Verb.List, "default"))
            .ToArray();
        await Task.WhenAll(checks);

        checks.ShouldAllBe(check => check.Result);
    }

    [Theory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task cluster_scope_checks_return_access_review_result(KubernetesBackend backend)
    {
        await using var harness = await new TestClusterGenerator().CreateAsync(
            new TestClusterConfig { Type = backend },
            TestContext.Current.CancellationToken);
        var cluster = harness.Cluster;
        await cluster.Connect();

        var allowed = await cluster.Permissions.UpdateCanI(GroupApiVersionKind.From<V1Pod>(), Verb.List);

        allowed.ShouldBeTrue();
    }

    [Theory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task invalidation_reloads_granted_and_revoked_permissions(KubernetesBackend backend)
    {
        await using var harness = await new TestClusterGenerator().CreateAsync(
            CreateRestrictedClusterConfig(backend),
            TestContext.Current.CancellationToken);
        var cluster = harness.Cluster;
        await cluster.Connect();
        var kind = GroupApiVersionKind.From<V1Pod>();

        (await cluster.Permissions.UpdateCanI(kind, Verb.List, "default")).ShouldBeFalse();

        await GrantPodListAsync(harness, "default", TestContext.Current.CancellationToken);
        await WaitForPermissionAsync(cluster, kind, Verb.List, "default", expected: true, TestContext.Current.CancellationToken);

        await RevokePodListAsync(harness, "default", TestContext.Current.CancellationToken);
        await WaitForPermissionAsync(cluster, kind, Verb.List, "default", expected: false, TestContext.Current.CancellationToken);
    }

    [Theory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task namespace_invalidation_retains_cluster_scope_decisions(KubernetesBackend backend)
    {
        await using var harness = await new TestClusterGenerator().CreateAsync(
            CreateRestrictedClusterConfig(
                backend,
                KubernetesRbac.InNamespace("default", new RbacRule("pods", "list"))),
            TestContext.Current.CancellationToken);
        var cluster = harness.Cluster;
        await cluster.Connect();
        var kind = GroupApiVersionKind.From<V1Pod>();

        (await cluster.Permissions.UpdateCanI(kind, Verb.List)).ShouldBeFalse();
        (await cluster.Permissions.UpdateCanI(kind, Verb.List, "default")).ShouldBeTrue();

        cluster.Permissions.InvalidateNamespacePermissions();

        cluster.Permissions.CanI(kind, Verb.List).ShouldBeFalse();
        cluster.Permissions.CanI(kind, Verb.List, "default").ShouldBeFalse();

        (await cluster.Permissions.UpdateCanI(kind, Verb.List, "default")).ShouldBeTrue();
        cluster.Permissions.CanI(kind, Verb.List).ShouldBeFalse();
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

        await cluster.UpdatePermissionsAllNamespaceAsync<V1Pod>(Verb.List);

        cluster.CanI<V1Pod>(Verb.List, "my-app").ShouldBeTrue();
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

    private const string PodListRoleName = "kubeui-test-pod-list";

    private static TestClusterConfig CreateRestrictedClusterConfig(
        KubernetesBackend backend,
        IEnumerable<IKubernetesObject<V1ObjectMeta>>? additionalResources = null)
    {
        List<IKubernetesObject<V1ObjectMeta>> resources =
        [
            new V1Namespace { Metadata = new V1ObjectMeta { Name = "my-app" } },
            new V1ServiceAccount
            {
                Metadata = new V1ObjectMeta
                {
                    Name = KubernetesRbac.ServiceAccountName,
                    NamespaceProperty = KubernetesRbac.ServiceAccountNamespace,
                },
            },
        ];
        resources.AddRange(KubernetesRbac.ClusterWide(
            new RbacRule("namespaces", "list"),
            new RbacRule("namespaces", "watch")));
        if (additionalResources is not null)
        {
            resources.AddRange(additionalResources);
        }

        return new TestClusterConfig
        {
            Type = backend,
            AuthenticatedUser = KubernetesRbac.ServiceAccountUser,
            InitialResources = resources,
        };
    }

    private static async Task GrantPodListAsync(
        TestCluster harness,
        string @namespace,
        CancellationToken cancellationToken)
    {
        await harness.CreateAsAdminAsync(
            KubernetesRbac.NamespacedRole(@namespace, PodListRoleName, new RbacRule("pods", "list")),
            cancellationToken).ConfigureAwait(false);
        await harness.CreateAsAdminAsync(
            KubernetesRbac.NamespacedRoleBinding(@namespace, PodListRoleName, PodListRoleName),
            cancellationToken).ConfigureAwait(false);
    }

    private static Task RevokePodListAsync(
        TestCluster harness,
        string @namespace,
        CancellationToken cancellationToken)
    {
        return harness.DeleteAsAdminAsync(
            KubernetesRbac.NamespacedRoleBinding(@namespace, PodListRoleName, PodListRoleName),
            cancellationToken);
    }

    private static async Task WaitForPermissionAsync(
        Cluster cluster,
        GroupApiVersionKind kind,
        Verb verb,
        string @namespace,
        bool expected,
        CancellationToken cancellationToken)
    {
        var deadline = DateTime.UtcNow + TimeSpan.FromSeconds(10);
        while (true)
        {
            cluster.Permissions.InvalidatePermissions();
            var actual = await cluster.Permissions.UpdateCanI(kind, verb, @namespace).ConfigureAwait(false);
            if (actual == expected || DateTime.UtcNow >= deadline)
            {
                actual.ShouldBe(expected);
                return;
            }

            cancellationToken.ThrowIfCancellationRequested();
            await TestWait.NextPollAsync(TimeSpan.FromMilliseconds(25), cancellationToken).ConfigureAwait(false);
        }
    }
}
