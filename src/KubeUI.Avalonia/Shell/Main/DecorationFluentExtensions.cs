using Avalonia.Automation;
using Avalonia.Controls.Chrome;
using Avalonia.Controls.Primitives;
using Avalonia.Input;

namespace KubeUI.Avalonia.Shell.Main;

internal static class DecorationFluentExtensions
{
    internal static T ElementRole<T>(this T control, WindowDecorationsElementRole role) where T : Visual
    {
        WindowDecorationProperties.SetElementRole(control, role);
        return control;
    }

    internal static T AutomationName<T>(this T control, string? name) where T : StyledElement
    {
        AutomationProperties.SetName(control, name);
        return control;
    }

    internal static T AttachedFlyout<T>(this T control, FlyoutBase flyout) where T : Control
    {
        FlyoutBase.SetAttachedFlyout(control, flyout);
        return control;
    }
}
