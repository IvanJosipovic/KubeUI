using k8s;
using k8s.Models;
using KubeUI.Avalonia.Features.Clusters.Workspace;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Pod.Services;

/// <summary>Opens pod-log sessions in the docking workspace.</summary>
public interface IPodLogsLauncher
{
    /// <summary>Creates, docks, and connects a pod-log view for a resource.</summary>
    Task LaunchAsync(ClusterWorkspace cluster, IKubernetesObject<V1ObjectMeta> resource, string resourceKind);

    /// <summary>Creates, docks, and connects one pod-log view for multiple resources of a kind.</summary>
    Task LaunchAsync(
        ClusterWorkspace cluster,
        IReadOnlyList<IKubernetesObject<V1ObjectMeta>> resources,
        string resourceKind);

    /// <summary>Returns whether the cluster's active bottom-docked tool is a compatible pod-log view.</summary>
    bool CanAddToActive(ClusterWorkspace cluster);

    /// <summary>Adds resources to the active compatible pod-log view or launches a new view.</summary>
    Task AddToActiveAsync(
        ClusterWorkspace cluster,
        IReadOnlyList<IKubernetesObject<V1ObjectMeta>> resources,
        string resourceKind);
}
