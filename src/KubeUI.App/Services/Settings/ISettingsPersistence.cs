using AppAppearanceSettings = KubeUI.App.Options.AppearanceSettings;
using AppSettings = KubeUI.App.Options.Settings;

namespace KubeUI.App.Services.Settings;

public interface ISettingsPersistence
{
    string SettingsDirectory { get; }
    bool EnsureDirectoryExists();
    SettingsPersistenceData Load();
    bool Save(SettingsPersistenceData data);
}

public sealed class SettingsPersistenceData
{
    public AppSettings Settings { get; set; } = new();

    public AppAppearanceSettings Appearance { get; set; } = new();
}
