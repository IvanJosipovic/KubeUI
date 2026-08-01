using System.Text.Json;

namespace KubeUI.Avalonia.Services.Settings;

public static class SettingsPersistenceLoader
{
    public static string SettingsDirectory => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.UserProfile),
        ".kubeui");

    public static string SettingsFilePath => Path.Combine(SettingsDirectory, "settings.json");

    public static bool EnsureDirectoryExists(ILogger? logger = null)
    {
        try
        {
            Directory.CreateDirectory(SettingsDirectory);
            return true;
        }
        catch (Exception exception)
        {
            logger?.LogError(exception, "Unable to create settings directory");
            return false;
        }
    }

    public static SettingsPersistenceData Load(ILogger? logger = null)
        => Load(SettingsFilePath, logger);

    internal static SettingsPersistenceData Load(string settingsFilePath, ILogger? logger = null)
    {
        try
        {
            if (File.Exists(settingsFilePath))
            {
                using var json = File.OpenRead(settingsFilePath);
                var settings = JsonSerializer.Deserialize(
                    json,
                    SettingsPersistenceSourceGenerationContext.Default.SettingsPersistenceData);
                if (settings is not null)
                {
                    return settings;
                }
            }
        }
        catch (Exception exception)
        {
            logger?.LogError(exception, "Unable to load settings file");
        }

        return new SettingsPersistenceData();
    }
}
