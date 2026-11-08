using KubeUI.Features.Clusters.Workspace;

namespace KubeUI.Features.Clusters.Settings;

public sealed partial class ClusterSettingsViewModel : ViewModelBase, IInitializeCluster
{
    public IClusterPreferenceStore SettingsService { get; }

    public ClusterWorkspace? Cluster { get; set; }

    public ClusterSettingsViewModel(IClusterPreferenceStore settingsService)
    {
        Title = L10n.Instance.ClusterSettingsView_Title.CurrentValue()!;
        SettingsService = settingsService;
    }

    public void Initialize(ClusterWorkspace cluster)
    {
        Cluster = cluster;
        Id = nameof(ClusterSettingsViewModel) + Cluster.Runtime.Name;
        ClusterSettings = SettingsService.GetClusterSettings(cluster.Runtime);
        DebugContainerImage = ClusterSettings.DebugContainerImage;
    }

    [ObservableProperty]
    public partial ClusterSettings ClusterSettings { get; set; }

    [ObservableProperty]
    public partial string Namespace { get; set; }

    [ObservableProperty]
    public partial string DebugContainerImage { get; set; } = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(CanRefreshPermissions))]
    public partial bool IsRefreshingPermissions { get; set; }

    public bool CanRefreshPermissions => !IsRefreshingPermissions;

    public string ClusterTitle => string.Format(L10n.Instance.ClusterSettingsView_TitleFormat.CurrentValue()!, Cluster?.Runtime.Name);

    [RelayCommand]
    private async Task RefreshPermissionsAsync()
    {
        if (Cluster is null || IsRefreshingPermissions)
        {
            return;
        }

        IsRefreshingPermissions = true;
        try
        {
            await Cluster.RefreshPermissionsAsync();
        }
        finally
        {
            IsRefreshingPermissions = false;
        }
    }

    [RelayCommand]
    private void AddNamespace()
    {
        if (!string.IsNullOrEmpty(Namespace) && !ClusterSettings.Namespaces.Contains(Namespace))
        {
            ClusterSettings.Namespaces.Add(Namespace);

            SettingsService.SaveSettings();

            Namespace = "";
        }
    }

    [RelayCommand]
    private void RemoveNamespace(string ns)
    {
        if (!string.IsNullOrEmpty(ns) && ClusterSettings.Namespaces.Contains(ns))
        {
            ClusterSettings.Namespaces.Remove(ns);

            SettingsService.SaveSettings();
        }
    }

    partial void OnDebugContainerImageChanged(string value)
    {
        if (ClusterSettings == null)
        {
            return;
        }

        ClusterSettings.DebugContainerImage = value;
        SettingsService.SaveSettings();
    }
}
