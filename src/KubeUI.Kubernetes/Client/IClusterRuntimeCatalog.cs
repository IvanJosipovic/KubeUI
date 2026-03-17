using k8s.KubeConfigModels;

namespace KubeUI.Client;

public interface IClusterRuntimeCatalog
{
    IEnumerable<IClusterRuntime> Clusters { get; }
    IClusterRuntime? GetCluster(string name);
    IClusterRuntime? GetDefault();
    void LoadFromConfig(K8SConfiguration kubeConfig);
    void LoadFromConfigFromPath(string path);
    void RemoveCluster(IClusterRuntime cluster);
}
