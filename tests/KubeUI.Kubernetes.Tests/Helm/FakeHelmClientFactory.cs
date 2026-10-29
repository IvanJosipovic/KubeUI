using Helm.NET;

namespace KubeUI.Kubernetes.Tests.Helm;

/// <summary>
/// <see cref="IHelmClientFactory"/> test double that records connections and creation counts.
/// </summary>
internal sealed class FakeHelmClientFactory(IHelmClient client) : IHelmClientFactory, IDisposable
{
    private int _createCount;

    public List<HelmConnection> Connections { get; } = [];

    /// <summary>
    /// Gets a signal completed when the factory has been called for the second time.
    /// </summary>
    public TaskCompletionSource SecondCreated { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);

    public IHelmClient Create(HelmConnection connection)
    {
        Connections.Add(connection);
        if (Interlocked.Increment(ref _createCount) == 2)
        {
            SecondCreated.TrySetResult();
        }

        return client;
    }

    public void Dispose()
    {
        SecondCreated.TrySetCanceled();
    }
}
