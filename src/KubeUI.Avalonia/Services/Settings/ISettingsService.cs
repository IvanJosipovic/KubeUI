using AppSettings = KubeUI.Avalonia.Options.Settings;
using AppAppearanceSettings = KubeUI.Avalonia.Options.AppearanceSettings;

namespace KubeUI.Avalonia.Services.Settings;

public interface ISettingsService : IClusterSettingsStore
{
    AppSettings Settings { get; set; }
    AppAppearanceSettings Appearance { get; set; }
    IClusterSettingsStore Clusters { get; }
    void ApplySettings();
    void SaveSettings();
}
