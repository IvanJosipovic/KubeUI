using KubeUI.Testing;
using System.Diagnostics.CodeAnalysis;

namespace KubeUI.Avalonia.Tests.Infra;

internal sealed class KubernetesScenarioClusterScope : IDisposable, IAsyncDisposable
{
    private readonly IClusterScenarioHarness _harness;

    private KubernetesScenarioClusterScope(IClusterScenarioHarness harness)
    {
        _harness = harness;
    }

    public IClusterRuntime Cluster => _harness.Cluster;

    public KubernetesClusterScenarioHarness Harness => (KubernetesClusterScenarioHarness)_harness;

    public IClusterScenarioHarness ScenarioHarness => _harness;

    public static async Task<KubernetesScenarioClusterScope> CreateAsync(KubernetesBackend backend)
    {
        return new KubernetesScenarioClusterScope(
            await KubernetesScenarioHarnessFactory.CreateAsync(backend, TestContext.Current.CancellationToken));
    }

    [SuppressMessage("Usage", "CA2000")]
    public static KubernetesScenarioClusterScope Create(Action<KubernetesClusterScenarioHarness>? configure = null)
    {
        var harness = new KubernetesClusterScenarioHarness();
        configure?.Invoke(harness);
        Task.Run(() => harness.InitializeAsync(TestContext.Current.CancellationToken)).GetAwaiter().GetResult();
        return new KubernetesScenarioClusterScope(harness);
    }

    [SuppressMessage("Usage", "CA2000")]
    public static async Task<KubernetesScenarioClusterScope> CreateAsync(Action<KubernetesClusterScenarioHarness>? configure = null)
    {
        var harness = new KubernetesClusterScenarioHarness();
        configure?.Invoke(harness);
        await harness.InitializeAsync(TestContext.Current.CancellationToken);
        return new KubernetesScenarioClusterScope(harness);
    }

    [SuppressMessage("Usage", "CA2000")]
    public static KubernetesScenarioClusterScope CreateDisconnected(Action<KubernetesClusterScenarioHarness>? configure = null)
    {
        var harness = new KubernetesClusterScenarioHarness();
        configure?.Invoke(harness);
        harness.InitializeDisconnected();
        return new KubernetesScenarioClusterScope(harness);
    }

    public void Dispose()
        => _harness.DisposeAsync().AsTask().GetAwaiter().GetResult();

    public ValueTask DisposeAsync()
        => _harness.DisposeAsync();
}
