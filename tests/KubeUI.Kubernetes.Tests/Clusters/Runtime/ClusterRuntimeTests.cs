namespace KubeUI.Kubernetes.Tests.Clusters.Runtime;

using System.Reactive.Linq;
using System.Reactive.Threading.Tasks;
using DynamicData;
using KubernetesClient.Informer.Client;
using Shouldly;
using V1Namespace = k8s.Models.V1Namespace;
using V1ObjectMeta = k8s.Models.V1ObjectMeta;
using V1Pod = k8s.Models.V1Pod;
using V1Service = k8s.Models.V1Service;
using V1ServiceAccount = k8s.Models.V1ServiceAccount;

[Trait("Category", "Kind")]
public sealed class ClusterRuntimeTests : ClusterRuntimeAssertions
{
    [Fact]
    public async Task DisconnectStopsAllResourceInformerTasks()
    {
        await using var clusterScope = await new TestClusterGenerator().CreateAsync(new TestClusterConfig(), TestContext.Current.CancellationToken);
        var cluster = clusterScope.Cluster;
        await cluster.Connect();

        await cluster.SeedResource<V1Pod>(waitForReady: true);
        await cluster.Disconnect();

        cluster.ActiveResourceInformerTaskCount.ShouldBe(0);
    }

    [Fact]
    public async Task TypedResourceInformerIsTrackedForShutdown()
    {
        await using var clusterScope = await new TestClusterGenerator().CreateAsync(new TestClusterConfig(), TestContext.Current.CancellationToken);
        var cluster = clusterScope.Cluster;
        var activeInformerTaskCountBeforeSeeding = cluster.ActiveResourceInformerTaskCount;

        await cluster.Connect();
        await cluster.SeedResource<V1Pod>(waitForReady: true);

        cluster.ActiveResourceInformerTaskCount.ShouldBeGreaterThan(activeInformerTaskCountBeforeSeeding);
    }

    [Fact]
    public async Task ScenarioSeedsTypedInitialResources()
    {
        await using var clusterScope = await new TestClusterGenerator().CreateAsync(
            new TestClusterConfig
            {
                Resources =
                [
                    new V1Namespace
                    {
                        Metadata = new V1ObjectMeta { Name = "scenario" },
                    },
                ],
            },
            TestContext.Current.CancellationToken);
        await clusterScope.Cluster.Connect();

        using var client = clusterScope.Cluster.Client!.GetGenericClient<V1Namespace>();
        var seeded = await client.ReadAsync<V1Namespace>(
            "scenario",
            TestContext.Current.CancellationToken);

        seeded.Metadata.Name.ShouldBe("scenario");
    }

    [Fact]
    public async Task ScenarioPermissionsControlProductionAuthorization()
    {
        await using var clusterScope = await new TestClusterGenerator().CreateAsync(
            new TestClusterConfig
            {
                AuthenticatedUser = KubernetesRbac.ServiceAccountUser,
                InitialResources = KubernetesRbac.ClusterWide(
                    new RbacRule("namespaces", "list"),
                    new RbacRule("namespaces", "watch"),
                    new RbacRule("pods", "list"),
                    new RbacRule("pods", "watch")),
            },
            TestContext.Current.CancellationToken);
        var cluster = clusterScope.Cluster;
        await cluster.Connect();

        await cluster.UpdateCanI<V1Pod>(Verb.List);
        await cluster.UpdateCanI<V1Pod>(Verb.Watch);
        await cluster.UpdateCanI<V1Service>(Verb.List);

        cluster.Permissions.CanIAnyNamespace<V1Pod>(Verb.List).ShouldBeTrue();
        cluster.Permissions.CanIAnyNamespace<V1Pod>(Verb.Watch).ShouldBeTrue();
        cluster.Permissions.CanIAnyNamespace<V1Service>(Verb.List).ShouldBeFalse();
    }

    [Fact]
    [System.Diagnostics.CodeAnalysis.SuppressMessage(
        "Reliability",
        "CA2000",
        Justification = "The HTTP handler ownership transfers to the generated client pipeline.")]
    public async Task SeedingNamespacedResourceWaitsForApiDiscovery()
    {
        var discoveryState = new DiscoveryGateState();
        await using var clusterScope = await new TestClusterGenerator().CreateAsync(
            new TestClusterConfig
            {
                Type = KubernetesBackend.Fake,
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
                        "my-app",
                        new RbacRule("pods", "list"),
                        new RbacRule("pods", "watch")),
                ],
                HttpHandlerFactory = () => [new DiscoveryGateHandler(discoveryState)],
            },
            TestContext.Current.CancellationToken);
        var cluster = clusterScope.Cluster;
        await cluster.Connect();
        cluster.Connected.ShouldBeTrue();

        await TestWait.UntilAsync(
            () => discoveryState.RequestCount > 0,
            TimeSpan.FromSeconds(10),
            cancellationToken: TestContext.Current.CancellationToken);

        var podKind = GroupApiVersionKind.From<V1Pod>();
        await cluster.Permissions.UpdatePermissionsAllNamespaceAsync(podKind, true, Verb.List);
        await cluster.Permissions.UpdatePermissionsAllNamespaceAsync(podKind, true, Verb.Watch);

        try
        {
            var seedTask = cluster.SeedResource<V1Pod>(waitForReady: true);

            await TestWait.NextPollAsync(
                TimeSpan.FromMilliseconds(50),
                TestContext.Current.CancellationToken);
            seedTask.IsCompleted.ShouldBeFalse(
                $"globalPods={cluster.Permissions.CanI<V1Pod>(Verb.List)}; namespaced={cluster.IsResourceNamespaced<V1Pod>()}; fault={seedTask.Exception}");

            discoveryState.Gate.SetResult();
            await seedTask.WaitAsync(TimeSpan.FromSeconds(10), TestContext.Current.CancellationToken);
        }
        finally
        {
            discoveryState.Gate.TrySetResult();
        }

        var container = cluster.Objects[podKind].ShouldBeOfType<ContainerClass<V1Pod>>();
        container.InformerCount.ShouldBe(
            1,
            $"connected={cluster.Connected}; namespaced={cluster.IsResourceNamespaced<V1Pod>()}; requests={discoveryState.RequestCount}");
    }

    private sealed class DiscoveryGateState
    {
        private int _requestCount;

        public TaskCompletionSource Gate { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

        public int RequestCount => Volatile.Read(ref _requestCount);

        public void OnDiscoveryRequest() => Interlocked.Increment(ref _requestCount);
    }

    private sealed class DiscoveryGateHandler(DiscoveryGateState state) : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            var path = request.RequestUri?.AbsolutePath;
            if (string.Equals(path, "/api", StringComparison.Ordinal)
                || string.Equals(path, "/apis", StringComparison.Ordinal))
            {
                state.OnDiscoveryRequest();
                await state.Gate.Task.WaitAsync(cancellationToken);
            }

            return await base.SendAsync(request, cancellationToken);
        }
    }

    protected override async Task<TestCluster> CreateHarnessAsync(KubernetesBackend backend)
    {
        var harness = await new TestClusterGenerator().CreateAsync(
            new TestClusterConfig { Type = backend },
            TestContext.Current.CancellationToken);
        await harness.Cluster.Connect();
        return harness;
    }

    [Theory, KubernetesBackendData]
    public Task InitializationExposesConnectedCluster(KubernetesBackend backend) => InitializationExposesConnectedClusterCore(backend);
    [Theory, KubernetesBackendData]
    public Task DisconnectAndReconnectRestoresCluster(KubernetesBackend backend) => DisconnectAndReconnectRestoresClusterCore(backend);
    [Theory, KubernetesBackendData]
    public Task GlobalPermissionsReflectDeniedAndAllowedOperations(KubernetesBackend backend) => GlobalPermissionsReflectDeniedAndAllowedOperationsCore(backend);
    [Theory, KubernetesBackendData]
    public Task NamespacedPermissionsReflectDeniedAndAllowedOperations(KubernetesBackend backend) => NamespacedPermissionsReflectDeniedAndAllowedOperationsCore(backend);
    [Theory, KubernetesBackendData]
    public Task PodSubresourcePermissionsCoverLogExecAndPortforward(KubernetesBackend backend) => PodSubresourcePermissionsCoverLogExecAndPortforwardCore(backend);
    [Theory, KubernetesBackendData]
    public Task DirectCrudMethodsRoundTripResources(KubernetesBackend backend) => DirectCrudMethodsRoundTripResourcesCore(backend);
    [Theory, KubernetesBackendData]
    public Task DirectCrudOperationsAreObservedByInformerCache(KubernetesBackend backend) => DirectCrudOperationsAreObservedByInformerCacheCore(backend);
    [Theory, KubernetesBackendData]
    public Task StaleResourceVersionUpdatesAreRejected(KubernetesBackend backend) => StaleResourceVersionUpdatesAreRejectedCore(backend);
    [Theory, KubernetesBackendData]
    public Task ReplaceDirectRefreshesResourceVersion(KubernetesBackend backend) => ReplaceDirectRefreshesResourceVersionCore(backend);

    [Theory, KubernetesBackendData]
    public async Task Modified_resource_replaces_the_existing_cache_instance(KubernetesBackend backend)
    {
        await using var harness = await new TestClusterGenerator().CreateAsync(
            new TestClusterConfig { Type = backend },
            TestContext.Current.CancellationToken);
        await harness.Cluster.Connect();
        await harness.Cluster.SeedResource<V1Namespace>(true);

        var created = await harness.CreateAsync(
            new V1Namespace { Metadata = new V1ObjectMeta { Name = "refresh-test" } },
            TestContext.Current.CancellationToken);
        var original = await WaitForResourceAsync<V1Namespace>(
            harness.Cluster,
            null,
            created.Metadata.Name,
            cancellationToken: TestContext.Current.CancellationToken);
        original.ShouldNotBeNull();

        var update = harness.Cluster.GetResourceSourceCache<V1Namespace>()
            .Connect()
            .WhereReasonsAre(ChangeReason.Update)
            .FirstAsync()
            .ToTask(TestContext.Current.CancellationToken);

        created.Metadata.Labels = new Dictionary<string, string> { ["test"] = "updated" };
        await harness.ReplaceAsync(created, TestContext.Current.CancellationToken);

        var changes = await update;
        var updated = changes.Single().Current;

        updated.ShouldNotBeSameAs(original);
        updated.Metadata.Labels!["test"].ShouldBe("updated");
    }

    [Theory, KubernetesBackendData]
    public Task CreateObject(KubernetesBackend backend) => CreateObjectCore(backend);
    [Theory, KubernetesBackendData]
    public Task CreateNamespacedObject(KubernetesBackend backend) => CreateNamespacedObjectCore(backend);
    [Theory, KubernetesBackendData]
    public Task ReadObjects(KubernetesBackend backend) => ReadObjectsCore(backend);
    [Theory, KubernetesBackendData]
    public Task UpdateObject(KubernetesBackend backend) => UpdateObjectCore(backend);
    [Theory, KubernetesBackendData]
    public Task UpdateNamespacedObject(KubernetesBackend backend) => UpdateNamespacedObjectCore(backend);
    [Theory, KubernetesBackendData]
    public Task DeleteObject(KubernetesBackend backend) => DeleteObjectCore(backend);
    [Theory, KubernetesBackendData]
    public Task DeleteNamespacedObject(KubernetesBackend backend) => DeleteNamespacedObjectCore(backend);
    [Theory, KubernetesBackendData]
    public Task ImportYaml(KubernetesBackend backend) => ImportYamlCore(backend);
    [Theory, KubernetesBackendData]
    public Task ImportYamlCrdInstance(KubernetesBackend backend) => ImportYamlCrdInstanceCore(backend);
    [Theory, KubernetesBackendData]
    public Task DryRunYamlResolvesRegisteredNamespacedGenericResource(KubernetesBackend backend) => DryRunYamlResolvesRegisteredNamespacedGenericResourceCore(backend);
    [Theory, KubernetesBackendData]
    public Task RootAccessCanI(KubernetesBackend backend) => RootAccessCanICore(backend);
    [Theory, KubernetesBackendData]
    public Task LimitedAccess(KubernetesBackend backend) => LimitedAccessCore(backend, false);
    [Theory, KubernetesBackendData]
    public Task LimitedAccessNoNamespace(KubernetesBackend backend) => LimitedAccessCore(backend, true);
    [Theory, KubernetesBackendData]
    public Task LimitedAccessCanI(KubernetesBackend backend) => LimitedAccessCanICore(backend);
    [Theory, KubernetesBackendData]
    public Task SeedNamespacedResourceAcrossKnownNamespaces(KubernetesBackend backend) => SeedNamespacedResourceAcrossKnownNamespacesCore(backend);
}
