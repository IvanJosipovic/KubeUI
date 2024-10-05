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
}

public sealed partial class ClusterSettings : ObservableObject
{
    [ObservableProperty]
    private ObservableCollection<string>? _namespaces;
}
