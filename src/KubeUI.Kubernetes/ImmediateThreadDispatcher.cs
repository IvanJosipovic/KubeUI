namespace KubeUI.Kubernetes;

public sealed class ImmediateThreadDispatcher : IThreadDispatcher
{
    public void Invoke(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);
        action();
    }
}
