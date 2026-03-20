using System.ComponentModel;
using System.Text.Json;
using Avalonia.Styling;
using Microsoft.Extensions.Configuration;

namespace KubeUI.Avalonia;

public class SettingsService : ObservableObject, ISettingsService, IClusterSettingsStore
{
    private readonly ILogger<SettingsService> _logger;
    private Settings? _settings;
    private AppearanceSettings? _appearance;

    public Settings Settings
    {
        get
        {
            if (_settings is not null)
            {
                return _settings;
            }

            _settings = LoadPersistedSettings().Settings;
            HookSettings(_settings);
            return _settings;
        }
        set
        {
            if (_settings is not null)
            {
                _settings.PropertyChanged -= Settings_PropertyChanged;
            }

            _settings = value;
            HookSettings(_settings);
            SaveSettings();
            OnPropertyChanged(nameof(Settings));
        }
    }

    public AppearanceSettings Appearance
    {
        get
        {
            if (_appearance is not null)
            {
                return _appearance;
            }

            _appearance = LoadPersistedSettings().Appearance;
            HookAppearance(_appearance);
            return _appearance;
        }
        set
        {
            if (_appearance is not null)
            {
                _appearance.PropertyChanged -= Appearance_PropertyChanged;
            }

            _appearance = value;
            HookAppearance(_appearance);
            SaveSettings();
            OnPropertyChanged(nameof(Appearance));
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

    public static Settings LoadSettingsFromFile()
    {
        return LoadPersistedSettings().Settings;
    }

    public IClusterSettingsStore Clusters => this;

    IReadOnlyCollection<string> IClusterSettingsStore.KubeConfigPaths => Settings.KubeConfigs;

    public void AddKubeConfigPath(string path)
    {
        Settings.AddKubeConfig(path);
    }

    public IReadOnlyCollection<string> GetClusterNamespaces(IClusterRuntime cluster)
    {
        return Settings.GetClusterSettings(cluster).Namespaces ?? [];
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

    public virtual void SaveSettings()
    {
        try
        {
            if (EnsureSettingDirExists())
            {
                File.WriteAllText(GetSettingsFilePath(), JsonSerializer.Serialize(new PersistedSettings
                {
                    Settings = Settings,
                    Appearance = Appearance,
                }, new JsonSerializerOptions(JsonSerializerDefaults.General)
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

    public virtual void ApplySettings()
    {
        switch (Appearance.Theme)
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

        if (Application.Current is not null)
        {
            Application.Current.Resources["DataGridRowHeight"] = Convert.ToDouble(Appearance.ListRowHeight);
            Application.Current.Resources["DataGridColumnHeaderMinHeight"] = Convert.ToDouble(Appearance.ListRowHeight + 4m);
            Application.Current.Resources["DataGridFontSize"] = Convert.ToDouble(Appearance.FontSize);
        }

        App.TopLevel?.FontSize = Convert.ToDouble(Appearance.FontSize);
    }

    private static PersistedSettings LoadPersistedSettings()
    {
        try
        {
            var path = GetSettingsFilePath();

            if (File.Exists(path))
            {
                var json = File.ReadAllText(path);
                var persisted = JsonSerializer.Deserialize<PersistedSettings>(json);

                if (json.Contains("\"Settings\"", StringComparison.Ordinal) || json.Contains("\"Appearance\"", StringComparison.Ordinal))
                {
                    return persisted ?? new PersistedSettings();
                }

                var legacy = JsonSerializer.Deserialize<Settings>(json);
                if (legacy is not null)
                {
                    return new PersistedSettings
                    {
                        Settings = legacy,
                        Appearance = new AppearanceSettings(),
                    };
                }
            }
        }
        catch (Exception)
        {
        }

        return new PersistedSettings();
    }

    private void HookSettings(Settings settings)
    {
        settings.PropertyChanged -= Settings_PropertyChanged;
        settings.PropertyChanged += Settings_PropertyChanged;
    }

    private void HookAppearance(AppearanceSettings appearance)
    {
        appearance.PropertyChanged -= Appearance_PropertyChanged;
        appearance.PropertyChanged += Appearance_PropertyChanged;
    }

    private void Settings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        SaveSettings();
    }

    private void Appearance_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        SaveSettings();
    }

    private sealed class PersistedSettings
    {
        public Settings Settings { get; set; } = new();

        public AppearanceSettings Appearance { get; set; } = new();
    }
}
