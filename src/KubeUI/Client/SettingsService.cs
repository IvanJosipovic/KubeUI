using System.Text.Json;
using Avalonia.Styling;
using Scrutor;

namespace KubeUI;

[ServiceDescriptor<SettingsService>(ServiceLifetime.Singleton)]
public sealed partial class SettingsService : ObservableObject
{
    private readonly ILogger<SettingsService> _logger;

    public Settings Settings { get; private set; } = new Settings();

    public SettingsService()
    {
        _logger = Application.Current.GetRequiredService<ILogger<SettingsService>>();
    }

    public static string GetSettingsPath()
    {
        return Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), ".kubeui");
    }

    public static string GetSettingsFilePath()
    {
        return Path.Combine(GetSettingsPath(), "settings.json");
    }

    public void LoadSettings()
    {
        Settings = GetSettings();

        ApplySettings();
    }

    public static Settings GetSettings()
    {
        try
        {
            var path = GetSettingsFilePath();

            if (File.Exists(path))
            {
                using var fs = new FileStream(path, FileMode.Open, FileAccess.Read);

                if (fs.CanRead)
                {
                    return JsonSerializer.Deserialize<Settings>(fs);
                }
            }
        }
        catch (Exception){}

        return new Settings();
    }

    public static bool EnsureSettingDirExists()
    {
        try
        {
            if (!Directory.Exists(GetSettingsPath()))
            {
                Directory.CreateDirectory(GetSettingsPath());
            }

            return true;
        }
        catch (Exception)
        {
            return false;
        }
    }

    public void SaveSettings()
    {
        try
        {
            if (EnsureSettingDirExists())
            {
                using var fs = new FileStream(GetSettingsFilePath(), FileMode.OpenOrCreate, FileAccess.Write);

                if (fs.CanWrite)
                {
                    JsonSerializer.Serialize(fs, Settings, new JsonSerializerOptions(JsonSerializerDefaults.General)
                    {
                        WriteIndented = true
                    });
                }
            }
            else
            {
                _logger.LogError("Unable to create settings directory");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to save settings file");
        }

        ApplySettings();
    }

    public void ApplySettings()
    {
        switch (Settings.Theme)
        {
            case LocalThemeVariant.Default:
                Application.Current.RequestedThemeVariant = ThemeVariant.Default;
                break;
            case LocalThemeVariant.Dark:
                Application.Current.RequestedThemeVariant = ThemeVariant.Dark;
                break;
            case LocalThemeVariant.Light:
                Application.Current.RequestedThemeVariant = ThemeVariant.Light;
                break;
        }
    }
}
