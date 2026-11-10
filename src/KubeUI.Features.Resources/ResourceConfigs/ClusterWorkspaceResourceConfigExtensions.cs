using System.Diagnostics.CodeAnalysis;
using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Features.Clusters.Workspace;
using KubeUI.Features.Resources;

namespace KubeUI.Features.Clusters.Workspace;

public static class ClusterWorkspaceResourceConfigExtensions
{
    public static ResourceConfigBase<T> GetResourceConfig<[DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] T>(this ClusterWorkspace workspace, GroupApiVersionKind kind)
        where T : class, IKubernetesObject<V1ObjectMeta>, new()
    {
        ArgumentNullException.ThrowIfNull(workspace);
        return workspace.GetResourceConfig(kind) as ResourceConfigBase<T>
            ?? throw new InvalidOperationException($"Resource config {kind} is not configured for {typeof(T).Name}.");
    }
}
