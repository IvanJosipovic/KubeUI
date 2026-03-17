using KubeUI.Client;

namespace KubeUI;

public enum LocalThemeVariant
{
    Default,
    Light,
    Dark,
}

public sealed partial class Settings : ObservableObject
{
    [ObservableProperty]
    public partial LocalThemeVariant Theme { get; set; } = LocalThemeVariant.Dark;

    [ObservableProperty]
    public partial bool LoggingEnabled { get; set; }

    [ObservableProperty]
    public partial bool TelemetryEnabled { get; set; } = true;

    [ObservableProperty]
    public partial bool PreReleaseChannel { get; set; } = true;

    [ObservableProperty]
    public partial Dictionary<string, ClusterSettings> ClusterSettings { get; set; } = [];

    [ObservableProperty]
    public partial ObservableCollection<string> KubeConfigs { get; set; } = [];

    [ObservableProperty]
    public partial decimal FontSize { get; set; } = 13;

    [ObservableProperty]
    public partial decimal ConsoleFontSize { get; set; } = 12;

    [ObservableProperty]
    public partial decimal ListRowHeight { get; set; } = 30;

    public ClusterSettings GetClusterSettings(IClusterRuntime cluster)
    {
        var key = cluster.KubeConfigPath + " " + cluster.Name;

        if (ClusterSettings.TryGetValue(key, out var value))
        {
            return value;
        }

        var settings = new ClusterSettings();
        ClusterSettings[key] = settings;
        return settings;
    }
}

public sealed partial class ClusterSettings : ObservableObject
{
    [ObservableProperty]
    public partial ObservableCollection<string>? Namespaces { get; set; } = [];
}
