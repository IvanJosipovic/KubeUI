
namespace KubeUI.Client
{
    public interface IPriorityExecutor
    {
        ValueTask DisposeAsync();
        ValueTask Enqueue(Func<CancellationToken, Task> work, WorkPriority priority = WorkPriority.Normal);
    }
}