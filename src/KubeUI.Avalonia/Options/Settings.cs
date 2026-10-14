using KubeUI.Kubernetes;
namespace KubeUI.Avalonia.Options;

public sealed partial class Settings : ObservableObject
{
    [ObservableProperty]
    public partial bool LoggingEnabled { get; set; }

    [ObservableProperty]
    public partial bool McpServerEnabled { get; set; }

    [ObservableProperty]
    public partial string? SelectedAgentId { get; set; }

    [ObservableProperty]
    public partial int McpServerPort { get; set; } = 62888;

    [ObservableProperty]
    public partial bool TelemetryEnabled { get; set; } = true;

    [ObservableProperty]
    public partial bool PreReleaseChannel { get; set; }

    [ObservableProperty]
    public partial Dictionary<string, ClusterSettings> ClusterSettings { get; set; } = [];

    [ObservableProperty]
    public partial ObservableCollection<string> KubeConfigs { get; set; } = [];

    [ObservableProperty]
    public partial bool KubeConfigAutoDiscoveryEnabled { get; set; } = true;

    public void AddKubeConfig(string path)
    {
        if (KubeConfigs.Contains(path))
        {
            return;
        }

        KubeConfigs.Add(path);
        OnPropertyChanged(nameof(KubeConfigs));
    }

    /// <summary>
    /// Removes a kubeconfig path from the persisted list.
    /// </summary>
    /// <param name="path">The kubeconfig path to remove.</param>
    public void RemoveKubeConfig(string path)
    {
        if (KubeConfigs.Remove(path))
        {
            OnPropertyChanged(nameof(KubeConfigs));
        }
    }

    public ClusterSettings GetClusterSettings(IClusterRuntime cluster)
    {
        var key = GetClusterSettingsKey(cluster);

        if (ClusterSettings.TryGetValue(key, out var value))
        {
            return value;
        }

        var settings = new ClusterSettings();
        ClusterSettings[key] = settings;
        return settings;
    }

    /// <summary>
    /// Gets existing settings for a cluster without creating a new entry.
    /// </summary>
    /// <param name="cluster">The cluster runtime.</param>
    /// <returns>The stored settings, or <see langword="null"/> when none exist.</returns>
    public ClusterSettings? FindClusterSettings(IClusterRuntime cluster)
    {
        return ClusterSettings.GetValueOrDefault(GetClusterSettingsKey(cluster));
    }

    private static string GetClusterSettingsKey(IClusterRuntime cluster) => cluster.KubeConfigPath + " " + cluster.Name;
}

public sealed partial class ClusterSettings : ObservableObject
{
    public const string DefaultDebugContainerImage = "docker.io/library/busybox:latest";

    [ObservableProperty]
    public partial ObservableCollection<string>? Namespaces { get; set; } = [];

    [ObservableProperty]
    public partial string DebugContainerImage { get; set; } = DefaultDebugContainerImage;

    /// <summary>
    /// Gets or sets whether the cluster is shown in navigation.
    /// </summary>
    [ObservableProperty]
    public partial bool Enabled { get; set; } = true;

    /// <summary>
    /// Gets or sets the KubeUI-only display name. The kubeconfig context name is used when empty.
    /// </summary>
    [ObservableProperty]
    public partial string? Alias { get; set; }
}
