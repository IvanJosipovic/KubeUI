using KubeUI.Kubernetes;
namespace KubeUI.Avalonia.Options;

public sealed partial class Settings : ObservableObject
{
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

    public void AddKubeConfig(string path)
    {
        if (KubeConfigs.Contains(path))
        {
            return;
        }

        KubeConfigs.Add(path);
        OnPropertyChanged(nameof(KubeConfigs));
    }

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

public sealed partial class ClusterSettings : ClusterMetricsSettings
{
    public const string DefaultDebugContainerImage = "docker.io/library/busybox:latest";

    [ObservableProperty]
    public partial ObservableCollection<string>? Namespaces { get; set; } = [];

    [ObservableProperty]
    public partial string DebugContainerImage { get; set; } = DefaultDebugContainerImage;
}
