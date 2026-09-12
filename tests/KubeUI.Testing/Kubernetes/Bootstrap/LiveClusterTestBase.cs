using Xunit;

namespace KubeUI.Testing.Kubernetes.Bootstrap;

/// <summary>
/// Base fixture for temporary tests that need to connect to a real cluster from the host kubeconfig.
/// </summary>
public abstract class LiveClusterTestBase : IAsyncLifetime
{
    private TestClusterGenerator? _generator;

    /// <summary>
    /// Gets the kubeconfig context to use for the live test.
    /// </summary>
    protected abstract string LiveClusterName { get; }

    /// <summary>
    /// Gets the connected live test cluster.
    /// </summary>
    protected TestCluster LiveCluster => _cluster
        ?? throw new InvalidOperationException("The live cluster fixture has not been initialized.");

    private TestCluster? _cluster;

    public async ValueTask InitializeAsync()
    {
        _generator = new TestClusterGenerator();
        _cluster = await _generator.CreateAsync(
            TestClusterConfig.Live(LiveClusterName),
            CancellationToken.None).ConfigureAwait(false);
    }

    public async ValueTask DisposeAsync()
    {
        if (_generator is not null)
        {
            await _generator.ResetAsync().ConfigureAwait(false);
            _generator = null;
            _cluster = null;
        }
    }
}
