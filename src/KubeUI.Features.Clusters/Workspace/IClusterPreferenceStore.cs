using System.ComponentModel;
using KubeUI.Features.Clusters.Settings;
using KubeUI.Kubernetes;

namespace KubeUI.Features.Clusters.Workspace;

/// <summary>Stores cluster workspace preferences in application-owned settings.</summary>
public interface IClusterPreferenceStore : INotifyPropertyChanged
{
    /// <summary>Gets the observable list of kubeconfig paths known to the store.</summary>
    ObservableCollection<string> KubeConfigs { get; }

    /// <summary>Returns the settings for a cluster, creating them if they do not exist yet.</summary>
    /// <param name="cluster">The cluster runtime to resolve settings for.</param>
    /// <returns>The cluster settings.</returns>
    ClusterSettings GetClusterSettings(IClusterRuntime cluster);

    /// <summary>Finds the settings for a cluster without creating them.</summary>
    /// <param name="cluster">The cluster runtime to resolve settings for.</param>
    /// <returns>The cluster settings, or <see langword="null"/> when none exist yet.</returns>
    ClusterSettings? FindClusterSettings(IClusterRuntime cluster);

    /// <summary>Persists pending preference changes to the application settings.</summary>
    void SaveSettings();
}
