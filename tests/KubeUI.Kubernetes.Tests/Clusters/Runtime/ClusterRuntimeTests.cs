namespace KubeUI.Kubernetes.Tests.Clusters.Runtime;

[Trait("Category", "Kind")]
public sealed class ClusterRuntimeTests : ClusterRuntimeAssertions
{
    protected override async Task<IClusterScenarioHarness> CreateHarnessAsync(KubernetesBackend backend)
        => await KubernetesScenarioHarnessFactory.CreateAsync(backend, TestContext.Current.CancellationToken);

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
    public Task HandleCRD(KubernetesBackend backend) => HandleCrdCore(backend);
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
