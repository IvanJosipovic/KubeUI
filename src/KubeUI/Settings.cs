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
    private LocalThemeVariant _theme;

    [ObservableProperty]
    private bool _loggingEnabled;

    [ObservableProperty]
    private bool _telemetryEnabled = true;

    [ObservableProperty]
    private bool _preReleaseChannel = true;

    [ObservableProperty]
    private Dictionary<string, ClusterSettings> _clusterSettings = [];

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
    private ObservableCollection<string>? _namespaces = [];
}
