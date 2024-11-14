using KubeUI.Client;

namespace KubeUI.ViewModels;

public sealed partial class ClusterSettingsViewModel : ViewModelBase, IInitializeCluster, IDisposable
{
    public ISettingsService SettingsService { get; }

    public ICluster? Cluster { get; set; }

    public ClusterSettingsViewModel()
    {
        Title = Resources.ClusterSettingsViewModel_Title;

        SettingsService = Application.Current.GetRequiredService<ISettingsService>();
    }

    private void Settings_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        SettingsService.SaveSettings();
    }

    public void Initialize(ICluster cluster)
    {
        Cluster = cluster;
        Id = nameof(ClusterSettingsViewModel) + Cluster.Name;
        ClusterSettings = SettingsService.Settings.GetClusterSettings(cluster);

        ClusterSettings.PropertyChanged += Settings_PropertyChanged;
    }

    [ObservableProperty]
    private ClusterSettings _clusterSettings;

    [ObservableProperty]
    private string _namespace;

    [RelayCommand]
    private void AddNamespace()
    {
        if (ClusterSettings.Namespaces == null)
        {
            ClusterSettings.Namespaces = [];
        }

        if (!string.IsNullOrEmpty(Namespace) && !ClusterSettings.Namespaces.Contains(Namespace))
        {
            ClusterSettings.Namespaces.Add(Namespace);

            Namespace = "";
        }
    }

    [RelayCommand]
    private void RemoveNamespace(string ns)
    {
        if (!string.IsNullOrEmpty(ns) && ClusterSettings.Namespaces.Contains(ns))
        {
            ClusterSettings.Namespaces.Remove(ns);
        }
    }

    public void Dispose()
    {
        ClusterSettings.PropertyChanged -= Settings_PropertyChanged;
    }
}
