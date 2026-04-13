namespace KubeUI.Avalonia.Tests.Infra;

public abstract class AvaloniaTestBase : IDisposable
{
    protected AvaloniaTestBase()
    {
        TestApp.ResetForTest();
    }

    public virtual void Dispose()
    {
        TestApp.CleanupAfterTest();
    }
}
