using System.Text.Json;
using System.Text.Json.Serialization;

namespace KubeUI.Avalonia.Services.Settings;

internal sealed class FileSettingsPersistence : ISettingsPersistence
{
    public string SettingsDirectory => SettingsPersistenceLoader.SettingsDirectory;

    private readonly ILogger<FileSettingsPersistence> _logger;

    public FileSettingsPersistence(ILogger<FileSettingsPersistence> logger)
    {
        _logger = logger;
    }

    public SettingsPersistenceData Load()
    {
        return SettingsPersistenceLoader.Load(_logger);
    }

    public bool EnsureDirectoryExists()
    {
        if (!SettingsPersistenceLoader.EnsureDirectoryExists(_logger))
        {
            return false;
        }

        return true;
    }

    public void Save(SettingsPersistenceData data)
    {
        try
        {
            string directory = SettingsDirectory;
            if (!EnsureDirectoryExists())
            {
                return;
            }

            File.WriteAllText(
                SettingsPersistenceLoader.SettingsFilePath,
                JsonSerializer.Serialize(data, SettingsPersistenceSourceGenerationContext.Default.SettingsPersistenceData));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to save settings file");
        }
    }

}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(SettingsPersistenceData))]
internal partial class SettingsPersistenceSourceGenerationContext : JsonSerializerContext { }
