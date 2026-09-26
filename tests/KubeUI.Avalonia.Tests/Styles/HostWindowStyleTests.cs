using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Threading;
using Dock.Avalonia.Controls;
using Dock.Model.Core;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Styles;

public sealed class HostWindowStyleTests
{
    [AvaloniaFact]
    public void floating_host_window_uses_themed_background()
    {
        var window = CreateHostWindow(isToolWindow: true);

        window.Show();

        Dispatcher.UIThread.RunJobs();

        var found = Application.Current!.TryFindResource("SystemRegionBrush", out var brush);
        found.ShouldBeTrue();
        brush.ShouldBeOfType<SolidColorBrush>();
        window.Background.ShouldBeOfType<SolidColorBrush>();

        ((SolidColorBrush)window.Background!).Color.ShouldBe(((SolidColorBrush)brush).Color);
        window.RequestedThemeVariant.ShouldBe(Application.Current.RequestedThemeVariant);
        window.TransparencyLevelHint.ShouldContain(WindowTransparencyLevel.None);
        window.Opacity.ShouldBe(1.0);

        window.Close();
    }

    [AvaloniaFact]
    public void floating_document_host_window_uses_themed_background()
    {
        var window = CreateHostWindow(isToolWindow: false);

        window.Show();

        Dispatcher.UIThread.RunJobs();

        Application.Current!.TryFindResource("SystemRegionBrush", out var brush).ShouldBeTrue();
        window.Background.ShouldBeOfType<SolidColorBrush>();
        ((SolidColorBrush)window.Background!).Color.ShouldBe(((SolidColorBrush)brush!).Color);

        window.Close();
    }

    private static HostWindow CreateHostWindow(bool isToolWindow)
    {
        var factory = Application.Current.GetTestServices().GetRequiredService<IFactory>();
        var window = factory.HostWindowLocator[nameof(IDockWindow)]().ShouldBeOfType<HostWindow>();
        window.IsToolWindow = isToolWindow;
        return window;
    }
}
