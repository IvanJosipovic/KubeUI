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

    public FakeClusterScenarioHarness FakeHarness => (FakeClusterScenarioHarness)_harness;

    public IClusterScenarioHarness ScenarioHarness => _harness;

    public static async Task<KubernetesTestWorkspaceScope> CreateAsync(IServiceProvider services, KubernetesBackend backend)
    {
        IClusterScenarioHarness harness = await KubernetesScenarioHarnessFactory.CreateAsync(
            backend,
            TestContext.Current.CancellationToken).ConfigureAwait(false);
        var workspace = ActivatorUtilities.CreateInstance<ClusterWorkspace>(services, harness.Cluster);
        return new KubernetesTestWorkspaceScope(harness, workspace);
    }

    public static async Task<KubernetesTestWorkspaceScope> CreateFakeAsync(
        IServiceProvider services,
        Action<FakeClusterScenarioHarness> configure)
    {
        var harness = new FakeClusterScenarioHarness();
        configure(harness);
        await harness.InitializeAsync(TestContext.Current.CancellationToken).ConfigureAwait(false);
        var workspace = ActivatorUtilities.CreateInstance<ClusterWorkspace>(services, harness.Cluster);
        return new KubernetesTestWorkspaceScope(harness, workspace);
    }

    public static KubernetesTestWorkspaceScope CreateFake(IServiceProvider services)
    {
        var harness = new FakeClusterScenarioHarness();
        Task.Run(() => harness.InitializeAsync(TestContext.Current.CancellationToken)).GetAwaiter().GetResult();
        var workspace = ActivatorUtilities.CreateInstance<ClusterWorkspace>(services, harness.Cluster);
        return new KubernetesTestWorkspaceScope(harness, workspace);
    }

    public void Dispose()
    {
        Workspace.Dispose();
        Task.Run(() => _harness.DisposeAsync().AsTask()).GetAwaiter().GetResult();
    }

    public async ValueTask DisposeAsync()
    {
        Workspace.Dispose();
        await _harness.DisposeAsync().ConfigureAwait(false);
    }
}
