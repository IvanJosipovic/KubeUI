using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Dock.Avalonia.Controls;
using KubeUI.Avalonia.Tests.Infra;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Styles;

public sealed class HostWindowStyleTests : AvaloniaTestBase
{
    [AvaloniaFact]
    public void floating_host_window_uses_themed_background()
    {
        var window = new HostWindow
        {
            IsToolWindow = true
        };

        window.Show();

        Dispatcher.UIThread.RunJobs();

        bool found = Application.Current!.TryFindResource("SystemRegionBrush", out object? brush);
        found.ShouldBeTrue();
        brush.ShouldBeOfType<SolidColorBrush>();
        window.Background.ShouldBeOfType<SolidColorBrush>();

        ((SolidColorBrush)window.Background!).Color.ShouldBe(((SolidColorBrush)brush).Color);
        window.RequestedThemeVariant.ShouldBe(Application.Current.RequestedThemeVariant);
        window.TransparencyLevelHint.ShouldContain(WindowTransparencyLevel.None);
        window.Opacity.ShouldBe(1.0);

        window.Close();
    }
}
