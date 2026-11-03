namespace KubeUI.Controls.Threading;

public interface IUiRefreshClock
{
    IDisposable Subscribe(Action callback);
}
