using KubeUI.Avalonia.Features.Clusters.Workspace;

namespace KubeUI.Avalonia.Infrastructure.Presentation;

/// <summary>
/// Exposes the cluster workspace associated with a view model or dockable.
/// </summary>
public interface IClusterWorkspaceContext
{
    /// <summary>
    /// Gets the associated cluster workspace, if one has been assigned.
    /// </summary>
    ClusterWorkspace? Cluster { get; }
}
