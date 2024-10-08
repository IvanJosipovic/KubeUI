using KubeUI.Client;

namespace KubeUI.ViewModels;

public sealed partial class ClusterSettingsViewModel : ViewModelBase, IInitializeCluster
{
    public ISettingsService SettingsService { get; }

    public ICluster? Cluster { get; set; }

    public ClusterSettingsViewModel()
    {
        Title = Resources.ClusterSettingsViewModel_Title;

        SettingsService = Application.Current.GetRequiredService<ISettingsService>();
    }

    public void Initialize(ICluster cluster)
    {
        Cluster = cluster;
        Id = nameof(ClusterSettingsViewModel) + Cluster.Name;
        ClusterSettings = SettingsService.Settings.GetClusterSettings(cluster);
    }

    [ObservableProperty]
    private ClusterSettings _clusterSettings;

    [ObservableProperty]
    private string _namespace;

    [RelayCommand]
    private void AddNamespace()
    {
        if (!string.IsNullOrEmpty(_namespace) && !ClusterSettings.Namespaces.Contains(_namespace))
        {
            ClusterSettings.Namespaces.Add(_namespace);

            SettingsService.SaveSettings();
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
}
