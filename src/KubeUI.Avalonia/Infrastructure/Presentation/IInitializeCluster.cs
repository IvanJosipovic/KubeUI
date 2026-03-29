using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.ViewModels;

public interface IInitializeCluster
{
    void Initialize(ClusterWorkspaceViewModel cluster);
}

