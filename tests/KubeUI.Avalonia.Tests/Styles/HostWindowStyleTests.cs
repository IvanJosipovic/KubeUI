using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Avalonia.Media;
using KubeUI.Avalonia.Tests.Infra;
using KubeUI.Avalonia.Controls;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Styles;

public sealed class HostWindowStyleTests : AvaloniaTestBase
{
    [AvaloniaFact]
    public void floating_host_window_uses_themed_background()
    {
        var window = new ThemedHostWindow();
        window.Show();

        Dispatcher.UIThread.RunJobs();

        bool found = Application.Current!.TryFindResource("SystemRegionBrush", out object? brush);
        found.ShouldBeTrue();
        brush.ShouldBeOfType<SolidColorBrush>();
        window.Background.ShouldBeOfType<SolidColorBrush>();

        ((SolidColorBrush)window.Background!).Color.ShouldBe(((SolidColorBrush)brush).Color);
        window.TransparencyLevelHint.ShouldContain(WindowTransparencyLevel.None);

        window.Close();
    }
}
