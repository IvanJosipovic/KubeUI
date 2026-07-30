using KubeUI.Testing;
using KubeUI.Kubernetes;
using System.Diagnostics.CodeAnalysis;

namespace KubeUI.Avalonia.Tests.Infra;

internal sealed class KubernetesScenarioClusterScope : IDisposable, IAsyncDisposable
{
    private readonly KubernetesClusterScenarioHarness _harness;

    private KubernetesScenarioClusterScope(KubernetesClusterScenarioHarness harness)
    {
        _harness = harness;
    }

    public IClusterRuntime Cluster => _harness.Cluster;

    public KubernetesClusterScenarioHarness Harness => _harness;

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
