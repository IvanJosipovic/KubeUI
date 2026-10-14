using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace KubeUI.Avalonia.Services.Settings;

internal sealed class FileSettingsPersistence : ISettingsPersistence
{
    public string SettingsDirectory => SettingsPersistenceLoader.SettingsDirectory;

    private readonly ILogger<FileSettingsPersistence> _logger;
    private SettingsPersistenceData? _data;

    public FileSettingsPersistence(ILogger<FileSettingsPersistence> logger)
    {
        _logger = logger;
    }

    public SettingsPersistenceData Load()
    {
        return _data ??= SettingsPersistenceLoader.Load(_logger);
    }

    public bool EnsureDirectoryExists()
    {
        if (!SettingsPersistenceLoader.EnsureDirectoryExists(_logger))
        {
            return false;
        }

        return true;
    }

    public bool Save(SettingsPersistenceData data)
    {
        try
        {
            var directory = SettingsDirectory;
            if (!EnsureDirectoryExists())
            {
                return false;
            }

            var serialized = JsonSerializer.Serialize(
                data,
                SettingsPersistenceSourceGenerationContext.Default.SettingsPersistenceData);
            if (OperatingSystem.IsWindows())
            {
                File.WriteAllText(SettingsPersistenceLoader.SettingsFilePath, serialized);
            }
            else
            {
                using var stream = new FileStream(SettingsPersistenceLoader.SettingsFilePath, new FileStreamOptions
                {
                    Mode = FileMode.Create,
                    Access = FileAccess.Write,
                    Share = FileShare.None,
                    UnixCreateMode = UnixFileMode.UserRead | UnixFileMode.UserWrite
                });
                using (var writer = new StreamWriter(stream, new UTF8Encoding(encoderShouldEmitUTF8Identifier: false), leaveOpen: true))
                    writer.Write(serialized);
                stream.Flush(flushToDisk: true);
                File.SetUnixFileMode(SettingsPersistenceLoader.SettingsFilePath,
                    UnixFileMode.UserRead | UnixFileMode.UserWrite);
            }
            _data = data;
            return true;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to save settings file");
            return false;
        }
    }

}

[JsonSourceGenerationOptions(WriteIndented = true)]
[JsonSerializable(typeof(SettingsPersistenceData))]
internal partial class SettingsPersistenceSourceGenerationContext : JsonSerializerContext { }
