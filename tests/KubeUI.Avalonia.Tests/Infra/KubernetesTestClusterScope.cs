using KubeUI.Testing;
using System.Diagnostics.CodeAnalysis;

namespace KubeUI.Avalonia.Tests.Infra;

internal sealed class KubernetesScenarioClusterScope : IAsyncDisposable
{
    private readonly IClusterScenarioHarness _harness;

    private KubernetesScenarioClusterScope(IClusterScenarioHarness harness)
    {
        _harness = harness;
    }

    public IClusterRuntime Cluster => _harness.Cluster;

    public FakeClusterScenarioHarness FakeHarness => (FakeClusterScenarioHarness)_harness;

    public IClusterScenarioHarness ScenarioHarness => _harness;

    public static async Task<KubernetesScenarioClusterScope> CreateAsync(KubernetesBackend backend)
    {
        return new KubernetesScenarioClusterScope(
            await KubernetesScenarioHarnessFactory.CreateAsync(backend, TestContext.Current.CancellationToken));
    }

    [SuppressMessage("Usage", "CA2000")]
    public static KubernetesScenarioClusterScope Create(Action<FakeClusterScenarioHarness>? configure = null)
    {
        var harness = new FakeClusterScenarioHarness();
        configure?.Invoke(harness);
        Task.Run(() => harness.InitializeAsync(TestContext.Current.CancellationToken)).GetAwaiter().GetResult();
        return new KubernetesScenarioClusterScope(harness);
    }

    [SuppressMessage("Usage", "CA2000")]
    public static async Task<KubernetesScenarioClusterScope> CreateAsync(Action<FakeClusterScenarioHarness>? configure = null)
    {
        var harness = new FakeClusterScenarioHarness();
        configure?.Invoke(harness);
        await harness.InitializeAsync(TestContext.Current.CancellationToken);
        return new KubernetesScenarioClusterScope(harness);
    }

    [SuppressMessage("Usage", "CA2000")]
    public static KubernetesScenarioClusterScope CreateDisconnected(Action<FakeClusterScenarioHarness>? configure = null)
    {
        var harness = new FakeClusterScenarioHarness();
        configure?.Invoke(harness);
        harness.InitializeDisconnected();
        return new KubernetesScenarioClusterScope(harness);
    }

    public ValueTask DisposeAsync()
        => _harness.DisposeAsync();
}
