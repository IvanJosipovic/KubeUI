using System.Text.Json;
using Avalonia.Styling;
using Scrutor;

namespace KubeUI.Client;

[ServiceDescriptor<ISettingsService>(ServiceLifetime.Singleton)]
public sealed partial class SettingsService : ObservableObject, ISettingsService
{
    private readonly ILogger<SettingsService> _logger;

    public Settings Settings { get; set; }

    public SettingsService(ILogger<SettingsService> logger)
    {
        _logger = logger;
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
        Settings = LoadSettingsFromFile();

        ApplySettings();
    }

    public static Settings LoadSettingsFromFile()
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
        catch (Exception) { }

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
                File.WriteAllText(GetSettingsFilePath(), JsonSerializer.Serialize(Settings, new JsonSerializerOptions(JsonSerializerDefaults.General)
                {
                    WriteIndented = true,
                }));
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

    private void ApplySettings()
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
