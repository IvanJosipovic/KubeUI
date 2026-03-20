using Avalonia.Threading;
using KubeUI.Kubernetes;

namespace KubeUI.Desktop;

internal sealed class AvaloniaThreadDispatcher : IThreadDispatcher
{
    public void Invoke(Action action)
    {
        ArgumentNullException.ThrowIfNull(action);

        if (Dispatcher.UIThread.CheckAccess())
        {
            action();
            return;
        }

        Dispatcher.UIThread.InvokeAsync(action).GetAwaiter().GetResult();
    }
}
