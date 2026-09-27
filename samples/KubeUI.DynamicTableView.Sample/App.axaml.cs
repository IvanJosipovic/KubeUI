using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;

namespace KubeUI.DynamicTableView.Sample;

public sealed partial class App : Application
{
    public override void OnFrameworkInitializationCompleted()
    {
        if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            desktop.MainWindow = new MainWindow();

        base.OnFrameworkInitializationCompleted();
    }
}
