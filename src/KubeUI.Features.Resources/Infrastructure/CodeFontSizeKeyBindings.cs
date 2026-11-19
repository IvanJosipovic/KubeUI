using System.Windows.Input;
using Avalonia.Input;
using KubeUI.Features.Resources.Settings;

namespace KubeUI.Features.Resources.Infrastructure;

internal static class CodeFontSizeKeyBindings
{
    /// <summary>Adds the shared code-font size shortcuts to a control's <c>KeyBindings</c> collection.</summary>
    /// <typeparam name="T">The control type, returned for chaining.</typeparam>
    /// <param name="control">The control to receive the shortcuts.</param>
    /// <param name="settingsService">Service that applies the font size change.</param>
    /// <returns>The same control, for chaining.</returns>
    /// <remarks>
    /// <c>KeyBindings</c> is a get-only CLR collection with no styled property, so these shortcuts
    /// cannot be supplied by a style; this extension fills the same role as the generated
    /// <c>KeyBindings(...)</c> setter in projects that generate Avalonia markup extensions.
    /// </remarks>
    public static T WithCodeFontSizeKeyBindings<T>(this T control, IResourceAppearanceSettings settingsService)
        where T : InputElement
    {
        ArgumentNullException.ThrowIfNull(control);
        ArgumentNullException.ThrowIfNull(settingsService);

        ICommand increase = new RelayCommand(() => settingsService.AdjustConsoleFontSize(1));
        ICommand decrease = new RelayCommand(() => settingsService.AdjustConsoleFontSize(-1));

        control.KeyBindings.Add(CreateBinding(Key.OemPlus, increase));
        control.KeyBindings.Add(CreateBinding(Key.Add, increase));
        control.KeyBindings.Add(CreateBinding(Key.OemMinus, decrease));
        control.KeyBindings.Add(CreateBinding(Key.Subtract, decrease));
        return control;
    }

    private static KeyBinding CreateBinding(Key key, ICommand command) => new()
    {
        Gesture = new KeyGesture(key, KeyModifiers.Control),
        Command = command
    };
}
