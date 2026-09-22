namespace KubeUI.Avalonia.Infrastructure.Threading;

public interface IUiRefreshClock
{
    IDisposable Subscribe(Action callback);
}
