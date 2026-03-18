namespace KubeUI.Avalonia;

public interface ISettingsService : IClusterSettingsStore
{
    Settings Settings { get; set; }
    AppearanceSettings Appearance { get; set; }
    IClusterSettingsStore Clusters { get; }
    void ApplySettings();
    void SaveSettings();
}
