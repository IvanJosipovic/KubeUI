namespace KubeUI.Kubernetes;

public interface IThreadDispatcher
{
    void Invoke(Action action);
}
