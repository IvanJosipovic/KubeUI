namespace KubeUI.Testing;

public static class KubernetesScenarioHarnessFactory
{
    public static async Task<IClusterScenarioHarness> CreateAsync(
        KubernetesBackend backend,
        CancellationToken cancellationToken = default)
    {
        IClusterScenarioHarness harness = backend switch
        {
            KubernetesBackend.Fake => new KubernetesClusterScenarioHarness(),
            KubernetesBackend.Kind => new KindClusterScenarioHarness(),
            _ => throw new ArgumentOutOfRangeException(nameof(backend), backend, null),
        };

        try
        {
            await harness.InitializeAsync(cancellationToken).ConfigureAwait(false);
            return harness;
        }
        catch
        {
            await harness.DisposeAsync().ConfigureAwait(false);
            throw;
        }
    }
}
