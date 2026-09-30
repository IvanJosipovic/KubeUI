using System.Windows.Input;
using Avalonia.Input;
using KubeUI.Avalonia.Options;
using KubeUI.Avalonia.Services.Settings;

namespace KubeUI.Avalonia.Infrastructure;

internal static class CodeFontSizeKeyBindings
{
    public static KeyBinding[] Create(ISettingsService settingsService)
    {
        ArgumentNullException.ThrowIfNull(settingsService);

        ICommand increase = new RelayCommand(() => Adjust(settingsService, 1));
        ICommand decrease = new RelayCommand(() => Adjust(settingsService, -1));

        return
        [
            CreateBinding(Key.OemPlus, increase),
            CreateBinding(Key.Add, increase),
            CreateBinding(Key.OemMinus, decrease),
            CreateBinding(Key.Subtract, decrease)
        ];
    }

    private static KeyBinding CreateBinding(Key key, ICommand command) => new()
    {
        Gesture = new KeyGesture(key, KeyModifiers.Control),
        Command = command
    };

    private static void Adjust(ISettingsService settingsService, int change)
    {
        var appearance = settingsService.Appearance;
        appearance.ConsoleFontSize = Math.Clamp(
            appearance.ConsoleFontSize + change,
            AppearanceSettings.MinimumFontSize,
            AppearanceSettings.MaximumFontSize);
    }
}
