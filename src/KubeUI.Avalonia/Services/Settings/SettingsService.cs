using KubeUI.Avalonia.Options;
using KubeUI.Avalonia.Styles;
using KubeUI.Kubernetes;
using AppAppearanceSettings = KubeUI.Avalonia.Options.AppearanceSettings;
using AppSettings = KubeUI.Avalonia.Options.Settings;

namespace KubeUI.Avalonia.Services.Settings;

public class SettingsService : ObservableObject, ISettingsService, IClusterSettingsStore
{
    private readonly ISettingsPersistence _persistence;
    private AppSettings? _settings;
    private AppSettings? _hookedSettings;
    private FeatureSettings? _hookedFeatures;
    private AppAppearanceSettings? _appearance;

    public AppSettings Settings
    {
        get
        {
            if (_settings is not null)
            {
                return _settings;
            }

            _settings = _persistence.Load().Settings;
            HookSettings(_settings);
            return _settings;
        }
        set
        {
            _settings = value;
            HookSettings(_settings);
            SaveSettings();
            OnPropertyChanged(nameof(Settings));
        }
    }

    public AppAppearanceSettings Appearance
    {
        get
        {
            if (_appearance is not null)
            {
                return _appearance;
            }

            _appearance = _persistence.Load().Appearance;
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

    public SettingsService(ILogger<SettingsService> logger, ISettingsPersistence persistence)
    {
        _ = logger;
        _persistence = persistence;
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

    public virtual void SaveSettings()
    {
        _ = TrySaveSettings();
    }

    public virtual bool TrySaveSettings()
    {
        var saved = _persistence.Save(new SettingsPersistenceData
        {
            Settings = Settings,
            Appearance = Appearance,
        });

        if (Dispatcher.UIThread.CheckAccess())
        {
            ApplySettings();
        }
        else
        {
            Dispatcher.UIThread.Post(ApplySettings);
        }
        return saved;
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
            var appFontSize = Convert.ToDouble(Appearance.FontSize);
            Application.Current.Resources[Typography.AppFontSizeResourceKey] = appFontSize;
            Application.Current.Resources[Typography.DockFontSizeResourceKey] = appFontSize;
            Application.Current.Resources[Typography.CodeFontSizeResourceKey] = Convert.ToDouble(Appearance.ConsoleFontSize);
        }

        App.TopLevel?.FontSize = Convert.ToDouble(Appearance.FontSize);
    }

    private void HookSettings(AppSettings settings)
    {
        if (_hookedSettings is not null)
        {
            _hookedSettings.PropertyChanged -= Settings_PropertyChanged;
        }

        _hookedSettings = settings;
        settings.PropertyChanged += Settings_PropertyChanged;
        if (settings.Features is null)
        {
            settings.Features = new();
        }
        HookFeatures(settings.Features);
    }

    private void HookFeatures(FeatureSettings? features)
    {
        if (_hookedFeatures is not null)
        {
            _hookedFeatures.PropertyChanged -= Features_PropertyChanged;
        }

        _hookedFeatures = features;
        if (features is not null)
        {
            features.PropertyChanged += Features_PropertyChanged;
        }
    }

    private void HookAppearance(AppAppearanceSettings appearance)
    {
        appearance.PropertyChanged -= Appearance_PropertyChanged;
        appearance.PropertyChanged += Appearance_PropertyChanged;
    }

    private void Settings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(AppSettings.Features) && sender is AppSettings settings)
        {
            HookFeatures(settings.Features);
        }

        SaveSettings();
    }

    private void Features_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        SaveSettings();
    }

    private void Appearance_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        SaveSettings();
    }

}
