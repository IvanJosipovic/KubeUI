using k8s;
using k8s.Models;
using KubeUI.Avalonia.Features.Clusters.Workspace;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Pod.Services;

public interface IPodLogsLauncher
{
    Task LaunchAsync(ClusterWorkspace cluster, IKubernetesObject<V1ObjectMeta> resource, string resourceKind);
}
