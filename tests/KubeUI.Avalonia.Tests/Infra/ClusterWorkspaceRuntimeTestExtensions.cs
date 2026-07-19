using k8s;
using k8s.Models;

namespace KubeUI.Avalonia.Tests.Infra;

internal static class ClusterWorkspaceRuntimeTestExtensions
{
    public static Task AddOrUpdateResource<T>(this ClusterWorkspace cluster, T item)
        where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        return cluster.Runtime.AddOrUpdateResource(item);
    }

    public static Task DeleteResource<T>(this ClusterWorkspace cluster, T item)
        where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        return cluster.Runtime.DeleteResource(item);
    }

    public static Task SeedResource<T>(this ClusterWorkspace cluster, bool waitForReady = false)
        where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        return cluster.Runtime.SeedResource<T>(waitForReady);
    }

    public static Task AddPodEphemeralDebugContainer(
        this ClusterWorkspace cluster,
        V1Pod pod,
        string? targetContainerName,
        string image)
    {
        return cluster.Runtime.AddPodEphemeralDebugContainer(pod, targetContainerName, image);
    }
}
