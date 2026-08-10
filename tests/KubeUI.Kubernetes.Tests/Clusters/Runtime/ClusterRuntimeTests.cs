namespace KubeUI.Kubernetes.Tests.Clusters.Runtime;

using Shouldly;

[Trait("Category", "Kind")]
public sealed class ClusterRuntimeTests : ClusterRuntimeAssertions
{
    [Fact]
    public async Task DisconnectStopsAllResourceInformerTasks()
    {
        await using var clusterScope = await new TestClusterGenerator().CreateAsync(new TestClusterConfig(), TestContext.Current.CancellationToken);
        var cluster = clusterScope.Cluster;
        await cluster.Connect();

        await cluster.SeedResource<k8s.Models.V1Pod>(waitForReady: true);
        await cluster.Disconnect();

        cluster.ActiveResourceInformerTaskCount.ShouldBe(0);
    }

    [Fact]
    public async Task ScenarioSeedsTypedInitialResources()
    {
        await using var clusterScope = await new TestClusterGenerator().CreateAsync(
            new TestClusterConfig
            {
                Resources =
                [
                    new k8s.Models.V1Namespace
                    {
                        Metadata = new k8s.Models.V1ObjectMeta { Name = "scenario" },
                    },
                ],
            },
            TestContext.Current.CancellationToken);
        await clusterScope.Cluster.Connect();

        using var client = clusterScope.Cluster.Client!.GetGenericClient<k8s.Models.V1Namespace>();
        var seeded = await client.ReadAsync<k8s.Models.V1Namespace>(
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

        await cluster.UpdateCanI<k8s.Models.V1Pod>(Verb.List);
        await cluster.UpdateCanI<k8s.Models.V1Pod>(Verb.Watch);
        await cluster.UpdateCanI<k8s.Models.V1Service>(Verb.List);

        cluster.Permissions.CanIAnyNamespace<k8s.Models.V1Pod>(Verb.List).ShouldBeTrue();
        cluster.Permissions.CanIAnyNamespace<k8s.Models.V1Pod>(Verb.Watch).ShouldBeTrue();
        cluster.Permissions.CanIAnyNamespace<k8s.Models.V1Service>(Verb.List).ShouldBeFalse();
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
