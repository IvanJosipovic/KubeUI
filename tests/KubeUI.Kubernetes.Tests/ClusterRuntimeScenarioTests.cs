using KubeUI.Testing;

namespace KubeUI.Kubernetes.Tests;

[Trait("Category", "Kind")]
public sealed class ClusterRuntimeScenarioTests : ClusterScenarioAssertions
{
    protected override async Task<IClusterScenarioHarness> CreateHarnessAsync(KubernetesBackend backend)
        => await KubernetesScenarioHarnessFactory.CreateAsync(backend, TestContext.Current.CancellationToken);

    [Theory, MemberData(nameof(KubernetesBackendData.Enabled), MemberType = typeof(KubernetesBackendData))]
    public Task CreateObject(KubernetesBackend backend) => CreateObjectCore(backend);
    [Theory, MemberData(nameof(KubernetesBackendData.Enabled), MemberType = typeof(KubernetesBackendData))]
    public Task CreateNamespacedObject(KubernetesBackend backend) => CreateNamespacedObjectCore(backend);
    [Theory, MemberData(nameof(KubernetesBackendData.Enabled), MemberType = typeof(KubernetesBackendData))]
    public Task ReadObjects(KubernetesBackend backend) => ReadObjectsCore(backend);
    [Theory, MemberData(nameof(KubernetesBackendData.Enabled), MemberType = typeof(KubernetesBackendData))]
    public Task UpdateObject(KubernetesBackend backend) => UpdateObjectCore(backend);
    [Theory, MemberData(nameof(KubernetesBackendData.Enabled), MemberType = typeof(KubernetesBackendData))]
    public Task UpdateNamespacedObject(KubernetesBackend backend) => UpdateNamespacedObjectCore(backend);
    [Theory, MemberData(nameof(KubernetesBackendData.Enabled), MemberType = typeof(KubernetesBackendData))]
    public Task DeleteObject(KubernetesBackend backend) => DeleteObjectCore(backend);
    [Theory, MemberData(nameof(KubernetesBackendData.Enabled), MemberType = typeof(KubernetesBackendData))]
    public Task DeleteNamespacedObject(KubernetesBackend backend) => DeleteNamespacedObjectCore(backend);
    [Theory, MemberData(nameof(KubernetesBackendData.Enabled), MemberType = typeof(KubernetesBackendData))]
    public Task ImportYaml(KubernetesBackend backend) => ImportYamlCore(backend);
    [Theory, MemberData(nameof(KubernetesBackendData.Enabled), MemberType = typeof(KubernetesBackendData))]
    public Task HandleCRD(KubernetesBackend backend) => HandleCrdCore(backend);
    [Theory, MemberData(nameof(KubernetesBackendData.Enabled), MemberType = typeof(KubernetesBackendData))]
    public Task RootAccessCanI(KubernetesBackend backend) => RootAccessCanICore(backend);
    [Theory, MemberData(nameof(KubernetesBackendData.Enabled), MemberType = typeof(KubernetesBackendData))]
    public Task LimitedAccess(KubernetesBackend backend) => LimitedAccessCore(backend, false);
    [Theory, MemberData(nameof(KubernetesBackendData.Enabled), MemberType = typeof(KubernetesBackendData))]
    public Task LimitedAccessNoNamespace(KubernetesBackend backend) => LimitedAccessCore(backend, true);
    [Theory, MemberData(nameof(KubernetesBackendData.Enabled), MemberType = typeof(KubernetesBackendData))]
    public Task LimitedAccessCanI(KubernetesBackend backend) => LimitedAccessCanICore(backend);
    [Theory, MemberData(nameof(KubernetesBackendData.Enabled), MemberType = typeof(KubernetesBackendData))]
    public Task SeedNamespacedResourceAcrossKnownNamespaces(KubernetesBackend backend) => SeedNamespacedResourceAcrossKnownNamespacesCore(backend);
}
