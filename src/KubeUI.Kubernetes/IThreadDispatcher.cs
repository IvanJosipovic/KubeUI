using System.Reactive.Concurrency;

namespace KubeUI.Kubernetes;

public interface IThreadDispatcher
{
    /// <summary>
    /// Gets scheduler that dispatches work through this dispatcher.
    /// </summary>
    IScheduler Scheduler { get; }

    /// <summary>
    /// Executes action on dispatcher thread and waits for it to complete.
    /// </summary>
    /// <param name="action">Action to execute.</param>
    void Invoke(Action action);
}
