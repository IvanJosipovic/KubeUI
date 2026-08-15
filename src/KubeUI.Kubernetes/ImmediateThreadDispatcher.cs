using System.Reactive.Concurrency;

namespace KubeUI.Kubernetes;

public sealed class ImmediateThreadDispatcher : IThreadDispatcher
{
    public IScheduler Scheduler => ImmediateScheduler.Instance;

    public void Invoke(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        action();
    }
}
