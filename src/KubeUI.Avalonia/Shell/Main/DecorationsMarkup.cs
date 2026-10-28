using Avalonia.Controls.Chrome;
using Avalonia.Input;

namespace KubeUI.Avalonia.Shell.Main;

internal static class DecorationsMarkup
{
    internal static ControlTheme Theme(ControlTheme basedOn, Func<NameScope, WindowDrawnDecorationsContent> build) =>
        new(typeof(WindowDrawnDecorations))
        {
            BasedOn = basedOn,
            Setters = { new Setter(WindowDrawnDecorations.TemplateProperty, new DecorationsTemplate(build)) }
        };

    internal static TemplateBinding Template(AvaloniaProperty property) => new() { Property = property };

    internal static Button CaptionButton(NameScope names, ControlTheme theme, WindowDecorationsElementRole role, object content)
    {
        var name = role switch
        {
            WindowDecorationsElementRole.MinimizeButton => "PART_MinimizeButton",
            WindowDecorationsElementRole.MaximizeButton => "PART_MaximizeButton",
            WindowDecorationsElementRole.CloseButton => "PART_CloseButton",
            WindowDecorationsElementRole.FullScreenButton => "PART_FullScreenButton",
            _ => throw new ArgumentOutOfRangeException(nameof(role), role, "A caption button role is required.")
        };
        return new Button().Name(name, names).Theme(theme).Content(content).ElementRole(role);
    }
}
