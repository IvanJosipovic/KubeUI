using k8s.Models;
using KubeUI.Features.Clusters.Workspace;

namespace KubeUI.Features.Resources.Workloads.v1.Pod.Services;

/// <summary>Shows a container file in the bottom tool dock.</summary>
public interface IContainerFilePreviewLauncher
{
    /// <summary>Shows a file, focusing an existing preview for the same path.</summary>
    /// <param name="cluster">The cluster the file is read from.</param>
    /// <param name="pod">The pod the file is read from.</param>
    /// <param name="containerName">The container the file is read from.</param>
    /// <param name="entry">The file to preview.</param>
    Task ShowAsync(ClusterWorkspace cluster, V1Pod pod, string containerName, ContainerFileEntry entry);
}
