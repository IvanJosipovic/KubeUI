using KubeUI.Kubernetes.Tests.Infra;

namespace KubeUI.Kubernetes.Tests.Mock;

public sealed class ClusterRuntimeScenarioTests : ClusterScenarioAssertions
{
    protected override async Task<IClusterScenarioHarness> CreateHarnessAsync()
    {
        var harness = new MockClusterScenarioHarness();
        await harness.InitializeAsync();
        return harness;
    }

    [Fact]
    public Task CreateObject() => CreateObjectCore();

    [Fact]
    public Task CreateNamespacedObject() => CreateNamespacedObjectCore();

    [Fact]
    public Task ReadObjects() => ReadObjectsCore();

    [Fact]
    public Task UpdateObject() => UpdateObjectCore();

    [Fact]
    public Task UpdateNamespacedObject() => UpdateNamespacedObjectCore();

    [Fact]
    public Task DeleteObject() => DeleteObjectCore();

    [Fact]
    public Task DeleteNamespacedObject() => DeleteNamespacedObjectCore();

    [Fact]
    public Task ImportYaml() => ImportYamlCore();

    [Fact]
    public Task HandleCRD() => HandleCrdCore();

    [Fact]
    public Task RootAccessCanI() => RootAccessCanICore();

    [Fact]
    public Task LimitedAccess() => LimitedAccessCore(includeNamespaceFallback: false);

    [Fact]
    public Task LimitedAccessNoNamespace() => LimitedAccessCore(includeNamespaceFallback: true);

    [Fact]
    public Task LimitedAccessCanI() => LimitedAccessCanICore();
}
