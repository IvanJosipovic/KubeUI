using KubeUI.Testing;

namespace KubeUI.Avalonia.Tests.Infra;

internal sealed class KubernetesTestWorkspaceScope : IDisposable, IAsyncDisposable
{
    private readonly IClusterScenarioHarness _harness;

    private KubernetesTestWorkspaceScope(IClusterScenarioHarness harness, ClusterWorkspace workspace)
    {
        _harness = harness;
        Workspace = workspace;
    }

    public ClusterWorkspace Workspace { get; }

    public FakeClusterScenarioHarness Harness => (FakeClusterScenarioHarness)_harness;

    public IClusterScenarioHarness ScenarioHarness => _harness;

    public static async Task<KubernetesTestWorkspaceScope> CreateAsync(IServiceProvider services)
    {
        var harness = new FakeClusterScenarioHarness();
        await harness.InitializeAsync(TestContext.Current.CancellationToken);
        var workspace = ActivatorUtilities.CreateInstance<ClusterWorkspace>(services, harness.Cluster);
        return new KubernetesTestWorkspaceScope(harness, workspace);
    }

    public static async Task<KubernetesTestWorkspaceScope> CreateAsync(IServiceProvider services, KubernetesBackend backend)
    {
        IClusterScenarioHarness harness = await KubernetesScenarioHarnessFactory.CreateAsync(
            backend,
            TestContext.Current.CancellationToken);
        var workspace = ActivatorUtilities.CreateInstance<ClusterWorkspace>(services, harness.Cluster);
        return new KubernetesTestWorkspaceScope(harness, workspace);
    }

    public static KubernetesTestWorkspaceScope Create(IServiceProvider services)
    {
        var harness = new FakeClusterScenarioHarness();
        Task.Run(() => harness.InitializeAsync(TestContext.Current.CancellationToken)).GetAwaiter().GetResult();
        var workspace = ActivatorUtilities.CreateInstance<ClusterWorkspace>(services, harness.Cluster);
        return new KubernetesTestWorkspaceScope(harness, workspace);
    }

    public void Dispose()
    {
        Workspace.Dispose();
        _harness.DisposeAsync().AsTask().GetAwaiter().GetResult();
    }

    public async ValueTask DisposeAsync()
    {
        Workspace.Dispose();
        await _harness.DisposeAsync();
    }
}
