using KubeUI.Kubernetes.Tests.Infra;
using Xunit;

namespace KubeUI.Kubernetes.Tests.E2E;

[Trait("Category", "Kind")]
public sealed class ClusterRuntimeScenarioTests : ClusterScenarioAssertions
{
    protected override async Task<IClusterScenarioHarness> CreateHarnessAsync()
    {
        var harness = new KindClusterScenarioHarness();
        await harness.InitializeAsync();
        return harness;
    }

    [KindFact]
    public Task CreateObject() => CreateObjectCore();

    [KindFact]
    public Task CreateNamespacedObject() => CreateNamespacedObjectCore();

    [KindFact]
    public Task ReadObjects() => ReadObjectsCore();

    [KindFact]
    public Task UpdateObject() => UpdateObjectCore();

    [KindFact]
    public Task UpdateNamespacedObject() => UpdateNamespacedObjectCore();

    [KindFact]
    public Task DeleteObject() => DeleteObjectCore();

    [KindFact]
    public Task DeleteNamespacedObject() => DeleteNamespacedObjectCore();

    [KindFact]
    public Task ImportYaml() => ImportYamlCore();

    [KindFact]
    public Task HandleCRD() => HandleCrdCore();

    [KindFact]
    public Task RootAccessCanI() => RootAccessCanICore();

    [KindFact]
    public Task LimitedAccess() => LimitedAccessCore(includeNamespaceFallback: false);

    [KindFact]
    public Task LimitedAccessNoNamespace() => LimitedAccessCore(includeNamespaceFallback: true);

    [KindFact]
    public Task LimitedAccessCanI() => LimitedAccessCanICore();
}
