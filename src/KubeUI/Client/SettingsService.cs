using System.Text.Json;
using Avalonia.Styling;
using Microsoft.Extensions.Configuration;

namespace KubeUI.Client;

public sealed partial class SettingsService : ObservableObject, ISettingsService
{
    private readonly ILogger<SettingsService> _logger;

    private Settings? _settings;

    public Settings Settings
    {
        get
        {
            if (_settings != null)
            {
                return _settings;
            }

            _settings = Application.Current.GetRequiredService<IConfiguration>().Get<Settings>() ?? new Settings();

            return _settings;
        }
        set
        {
            _settings = value;
            SaveSettings();
            OnPropertyChanged(nameof(Settings));
        }
    }

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

        App.TopLevel?.FontSize = Convert.ToDouble(Settings.FontSize);
    }
}
