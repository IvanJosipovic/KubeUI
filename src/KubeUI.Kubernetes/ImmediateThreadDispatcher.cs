using System.Reactive.Concurrency;

namespace KubeUI.Kubernetes;

public sealed class ImmediateThreadDispatcher : IThreadDispatcher
{
    /// <inheritdoc />
    public IScheduler Scheduler => ImmediateScheduler.Instance;

    /// <inheritdoc />
    public void Invoke(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        action();
    }

    /// <inheritdoc />
    public void Post(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        action();
    }
}
