namespace KubeUI.Features.Clusters.Workspace;

public interface IInitializeCluster : IClusterWorkspaceContext
{
    void Initialize(ClusterWorkspace cluster);
}
