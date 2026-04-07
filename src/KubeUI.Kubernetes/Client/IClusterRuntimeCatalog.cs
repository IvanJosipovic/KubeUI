using k8s.KubeConfigModels;

namespace KubeUI.Kubernetes;

public interface IClusterRuntimeCatalog
{
    IEnumerable<IClusterRuntime> Clusters { get; }
    IClusterRuntime? GetCluster(string name);
    IClusterRuntime? GetDefault();
    void LoadFromConfig(K8SConfiguration kubeConfig);
    void ImportIntoKubeConfig(K8SConfiguration kubeConfig);
    void LoadFromConfigFromPath(string path);
    void RemoveCluster(IClusterRuntime cluster);
}

