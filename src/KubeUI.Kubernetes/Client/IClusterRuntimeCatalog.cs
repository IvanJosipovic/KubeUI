using k8s.KubeConfigModels;

namespace KubeUI.Kubernetes;

public interface IClusterRuntimeCatalog
{
    IEnumerable<IClusterRuntime> Clusters { get; }

    /// <summary>
    /// Gets whether kubeconfig directory auto-discovery is enabled.
    /// </summary>
    bool KubeConfigAutoDiscoveryEnabled { get; }

    void AddCluster(IClusterRuntime cluster);
    IClusterRuntime? GetCluster(string name);
    IClusterRuntime? GetDefault();
    void LoadFromConfig(K8SConfiguration kubeConfig);
    void ImportIntoKubeConfig(K8SConfiguration kubeConfig);
    void LoadFromConfigFromPath(string path);

    /// <summary>
    /// Stops tracking a manually added kubeconfig file and removes its clusters. The file itself is not modified.
    /// </summary>
    /// <param name="path">The kubeconfig path to stop tracking.</param>
    void RemoveKubeConfig(string path);

    /// <summary>
    /// Enables or disables kubeconfig directory auto-discovery, persisting the choice and loading or unloading discovered clusters.
    /// </summary>
    /// <param name="enabled">Whether discovery should be enabled.</param>
    void SetKubeConfigAutoDiscovery(bool enabled);

    void RemoveCluster(IClusterRuntime cluster);
}
