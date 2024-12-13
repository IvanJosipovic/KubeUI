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
    public partial LocalThemeVariant Theme { get; set; }

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

    public ClusterSettings GetClusterSettings(ICluster cluster)
    {
        var _key = cluster.KubeConfigPath + " " + cluster.Name;

        if (ClusterSettings.TryGetValue(_key, out var value))
        {
            return value;
        }

        var settings = new ClusterSettings();

        ClusterSettings[_key] = settings;

        return settings;
    }
}

public sealed partial class ClusterSettings : ObservableObject
{
    [ObservableProperty]
    public partial ObservableCollection<string>? Namespaces { get; set; } = [];
}
