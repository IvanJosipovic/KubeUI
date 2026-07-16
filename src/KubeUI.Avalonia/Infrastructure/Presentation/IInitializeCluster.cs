using KubeUI.Avalonia.Features.Clusters.Workspace.ViewModels;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Infrastructure.Presentation;

public interface IInitializeCluster
{
    ClusterWorkspaceViewModel? Cluster { get; }

    void Initialize(ClusterWorkspaceViewModel cluster);
}

