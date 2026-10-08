using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Dock.Avalonia.Controls;
using Dock.Model.Core;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Styles;

public sealed class HostWindowStyleTests
{
    [AvaloniaFact]
    public void floating_tool_host_window_inherits_application_theme()
    {
        var window = CreateHostWindow(isToolWindow: true);

        window.Show();

        Dispatcher.UIThread.RunJobs();

        window.RequestedThemeVariant.ShouldBe(Application.Current.RequestedThemeVariant);
        window.TransparencyLevelHint.ShouldContain(WindowTransparencyLevel.None);
        window.Opacity.ShouldBe(1.0);

        window.Close();
    }

    [AvaloniaFact]
    public void floating_document_host_window_inherits_application_theme()
    {
        var window = CreateHostWindow(isToolWindow: false);

        window.Show();

        Dispatcher.UIThread.RunJobs();

        window.RequestedThemeVariant.ShouldBe(Application.Current!.RequestedThemeVariant);

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
