using Avalonia.Controls.ApplicationLifetimes;

namespace KubeUI.Avalonia;

public static class TopLevelAccessor
{
    public static TopLevel? GetCurrent()
    {
        if (Application.Current?.ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
        {
            return desktop.MainWindow is null ? null : TopLevel.GetTopLevel(desktop.MainWindow);
        }

        if (Application.Current?.ApplicationLifetime is ISingleViewApplicationLifetime singleView)
        {
            return singleView.MainView is null ? null : TopLevel.GetTopLevel(singleView.MainView);
        }

        return null;
    }

    public static TopLevel GetRequired()
    {
        return GetCurrent() ?? throw new InvalidOperationException("No active Avalonia TopLevel is available.");
    }
}

