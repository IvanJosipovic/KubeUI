using System.ComponentModel;
using KubeUI.Features.Clusters.Settings;
using KubeUI.Kubernetes;

namespace KubeUI.Features.Clusters.Workspace;

/// <summary>Stores cluster workspace preferences in application-owned settings.</summary>
public interface IClusterPreferenceStore : INotifyPropertyChanged
{
    ObservableCollection<string> KubeConfigs { get; }
    ClusterSettings GetClusterSettings(IClusterRuntime cluster);
    ClusterSettings? FindClusterSettings(IClusterRuntime cluster);
    void SaveSettings();
}
