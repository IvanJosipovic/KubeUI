using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Dock.Avalonia.Controls;

namespace KubeUI.Avalonia.Controls;

public sealed class ThemedHostWindow : HostWindow
{
    public ThemedHostWindow()
    {
        UpdateBackground();
        Application.Current!.ActualThemeVariantChanged += OnActualThemeVariantChanged;
    }

    protected override void OnClosed(EventArgs e)
    {
        Application.Current!.ActualThemeVariantChanged -= OnActualThemeVariantChanged;
        base.OnClosed(e);
    }

    private void OnActualThemeVariantChanged(object? sender, EventArgs e)
    {
        UpdateBackground();
    }

    private void UpdateBackground()
    {
        if (Application.Current is null)
        {
            return;
        }

        if (Application.Current.TryFindResource("SystemRegionBrush", out object? resource) && resource is IBrush brush)
        {
            Background = brush;
        }

        TransparencyLevelHint = [WindowTransparencyLevel.None];
        Opacity = 1.0;
    }
}
