
using KubeUI.Client;

namespace KubeUI.ViewModels;

public sealed partial class ClusterSettingsViewModel : ViewModelBase, IInitializeCluster
{
    public SettingsService SettingsService { get; }

    public ICluster Cluster { get; set; }

    public ClusterSettingsViewModel()
    {
        Title = Resources.SettingsView_Title;

        SettingsService = Application.Current.GetRequiredService<SettingsService>();
    }

    public void Initialize(ICluster cluster)
    {
        Cluster = cluster;
        Id = nameof(ClusterSettingsViewModel) + Cluster.Name;
    }
}
