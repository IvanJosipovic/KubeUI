using KubeUI.Testing;

namespace KubeUI.Kubernetes.Tests;

[Trait("Category", "Kind")]
public sealed class ClusterRuntimeScenarioTests : ClusterScenarioAssertions
{
    protected override async Task<IClusterScenarioHarness> CreateHarnessAsync(KubernetesBackend backend)
        => await KubernetesScenarioHarnessFactory.CreateAsync(backend, TestContext.Current.CancellationToken);

    [Theory, KubernetesBackendDataAttribute]
    public Task CreateObject(KubernetesBackend backend) => CreateObjectCore(backend);
    [Theory, KubernetesBackendDataAttribute]
    public Task CreateNamespacedObject(KubernetesBackend backend) => CreateNamespacedObjectCore(backend);
    [Theory, KubernetesBackendDataAttribute]
    public Task ReadObjects(KubernetesBackend backend) => ReadObjectsCore(backend);
    [Theory, KubernetesBackendDataAttribute]
    public Task UpdateObject(KubernetesBackend backend) => UpdateObjectCore(backend);
    [Theory, KubernetesBackendDataAttribute]
    public Task UpdateNamespacedObject(KubernetesBackend backend) => UpdateNamespacedObjectCore(backend);
    [Theory, KubernetesBackendDataAttribute]
    public Task DeleteObject(KubernetesBackend backend) => DeleteObjectCore(backend);
    [Theory, KubernetesBackendDataAttribute]
    public Task DeleteNamespacedObject(KubernetesBackend backend) => DeleteNamespacedObjectCore(backend);
    [Theory, KubernetesBackendDataAttribute]
    public Task ImportYaml(KubernetesBackend backend) => ImportYamlCore(backend);
    [Theory, KubernetesBackendDataAttribute]
    public Task HandleCRD(KubernetesBackend backend) => HandleCrdCore(backend);
    [Theory, KubernetesBackendDataAttribute]
    public Task RootAccessCanI(KubernetesBackend backend) => RootAccessCanICore(backend);
    [Theory, KubernetesBackendDataAttribute]
    public Task LimitedAccess(KubernetesBackend backend) => LimitedAccessCore(backend, false);
    [Theory, KubernetesBackendDataAttribute]
    public Task LimitedAccessNoNamespace(KubernetesBackend backend) => LimitedAccessCore(backend, true);
    [Theory, KubernetesBackendDataAttribute]
    public Task LimitedAccessCanI(KubernetesBackend backend) => LimitedAccessCanICore(backend);
    [Theory, KubernetesBackendDataAttribute]
    public Task SeedNamespacedResourceAcrossKnownNamespaces(KubernetesBackend backend) => SeedNamespacedResourceAcrossKnownNamespacesCore(backend);
}
