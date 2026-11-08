namespace KubeUI.Kubernetes;

public interface IClusterSettingsStore
{
    IReadOnlyCollection<string> KubeConfigPaths { get; }

    /// <summary>
    /// Gets or sets whether kubeconfig files in the kubeconfig directory are discovered automatically.
    /// </summary>
    bool KubeConfigAutoDiscoveryEnabled { get; set; }

    void AddKubeConfigPath(string path);

    /// <summary>
    /// Removes a manually added kubeconfig path from the persisted list.
    /// </summary>
    /// <param name="path">The kubeconfig path to remove.</param>
    void RemoveKubeConfigPath(string path);

    IReadOnlyCollection<string> GetClusterNamespaces(IClusterRuntime cluster);

    /// <summary>Returns persisted metrics configuration for a cluster.</summary>
    /// <param name="cluster">Cluster whose settings should be read.</param>
    /// <returns>The cluster metrics settings.</returns>
    ClusterMetricsSettings GetClusterMetricsSettings(IClusterRuntime cluster) => new();
}
