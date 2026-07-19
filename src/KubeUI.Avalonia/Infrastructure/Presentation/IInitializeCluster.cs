using KubeUI.Avalonia.Features.Clusters.Workspace;

namespace KubeUI.Avalonia.Infrastructure.Presentation;

public interface IInitializeCluster
{
    ClusterWorkspace? Cluster { get; }

    void Initialize(ClusterWorkspace cluster);
}

