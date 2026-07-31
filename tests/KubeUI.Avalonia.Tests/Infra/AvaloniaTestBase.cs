namespace KubeUI.Avalonia.Tests.Infra;

[Collection("Avalonia")]
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
