using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Dock.Avalonia.Controls;
using Dock.Model.Core;
using Shouldly;

namespace KubeUI.Avalonia.Tests.App.Styles;

public sealed class HostWindowStyleTests
{
    [AvaloniaFact]
    public void floating_tool_host_window_inherits_application_theme()
    {
        var window = CreateHostWindow(isToolWindow: true);

        try
        {
            window.Show();

            Dispatcher.UIThread.RunJobs();

            window.RequestedThemeVariant.ShouldBe(Application.Current.RequestedThemeVariant);
            window.TransparencyLevelHint.ShouldContain(WindowTransparencyLevel.None);
            window.Opacity.ShouldBe(1.0);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void floating_document_host_window_inherits_application_theme()
    {
        var window = CreateHostWindow(isToolWindow: false);

        try
        {
            window.Show();

            Dispatcher.UIThread.RunJobs();

            window.RequestedThemeVariant.ShouldBe(Application.Current!.RequestedThemeVariant);
        }
        finally
        {
            window.Close();
        }
    }

    private static HostWindow CreateHostWindow(bool isToolWindow)
    {
        var factory = Application.Current.GetTestServices().GetRequiredService<IFactory>();
        var window = factory.HostWindowLocator[nameof(IDockWindow)]().ShouldBeOfType<HostWindow>();
        window.IsToolWindow = isToolWindow;
        return window;
    }
}
