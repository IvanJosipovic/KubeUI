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
    /// <remarks>
    /// This blocks the calling thread until the dispatcher has run the action. It must not be used
    /// from a path that has to complete without the dispatcher making progress, for example during
    /// application shutdown or from a background thread that the dispatcher itself depends on.
    /// </remarks>
    /// <param name="action">Action to execute.</param>
    void Invoke(Action action);

    /// <summary>
    /// Queues action for execution on the dispatcher thread without waiting for it to complete.
    /// </summary>
    /// <remarks>
    /// Use this for work that must happen on the dispatcher thread but whose caller must never block
    /// on the dispatcher. The action is executed in the order it was queued.
    /// </remarks>
    /// <param name="action">Action to execute.</param>
    void Post(Action action);
}
