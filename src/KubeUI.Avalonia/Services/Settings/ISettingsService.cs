using KubeUI.Avalonia.Services.Settings;
using KubeUI.Kubernetes;
using AppAppearanceSettings = KubeUI.Avalonia.Options.AppearanceSettings;
using AppSettings = KubeUI.Avalonia.Options.Settings;

namespace KubeUI.Avalonia.Services.Settings;

public interface ISettingsService : IClusterSettingsStore
{
    AppSettings Settings { get; set; }
    AppAppearanceSettings Appearance { get; set; }
    IClusterSettingsStore Clusters { get; }
    void ApplySettings();
    void SaveSettings();
}
