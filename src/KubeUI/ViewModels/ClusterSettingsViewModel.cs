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
    public partial ClusterSettings ClusterSettings { get; set; }

    [ObservableProperty]
    public partial string Namespace { get; set; }

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
}
