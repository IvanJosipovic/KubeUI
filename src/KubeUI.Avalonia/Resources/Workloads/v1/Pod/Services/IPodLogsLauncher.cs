using k8s;
using k8s.Models;
using KubeUI.Avalonia.Features.Clusters.Workspace;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Pod.Services;

/// <summary>Opens pod-log sessions in the docking workspace.</summary>
public interface IPodLogsLauncher
{
    /// <summary>Creates, docks, and connects a pod-log view for a resource.</summary>
    Task LaunchAsync(ClusterWorkspace cluster, IKubernetesObject<V1ObjectMeta> resource, string resourceKind);
}
