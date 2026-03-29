using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Infrastructure.Presentation;

public interface IInitializeCluster
{
    void Initialize(ClusterWorkspaceViewModel cluster);
}

