using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Infrastructure.Presentation;

public interface IInitializeCluster
{
    ClusterWorkspace? Cluster { get; }

    void Initialize(ClusterWorkspace cluster);
}

