using KubeUI.Avalonia.Features.Clusters.Workspace;

namespace KubeUI.Avalonia.Infrastructure.Presentation;

public interface IInitializeCluster : IClusterWorkspaceContext
{
    void Initialize(ClusterWorkspace cluster);
}
