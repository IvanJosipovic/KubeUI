using AppAppearanceSettings = KubeUI.Avalonia.Options.AppearanceSettings;
using AppSettings = KubeUI.Avalonia.Options.Settings;

namespace KubeUI.Avalonia.Services.Settings;

public interface ISettingsPersistence
{
    string SettingsDirectory { get; }
    bool EnsureDirectoryExists();
    SettingsPersistenceData Load();
    void Save(SettingsPersistenceData data);
}

public sealed class SettingsPersistenceData
{
    public AppSettings Settings { get; set; } = new();

    public AppAppearanceSettings Appearance { get; set; } = new();
}
