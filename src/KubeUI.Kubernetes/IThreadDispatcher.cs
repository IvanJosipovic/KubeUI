using System.Reactive.Concurrency;

namespace KubeUI.Kubernetes;

public interface IThreadDispatcher
{
    IScheduler Scheduler { get; }

    void Invoke(Action action);
}
