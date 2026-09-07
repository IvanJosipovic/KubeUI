using k8s;
using k8s.Models;
using KubeUI.Avalonia.Resources.Workloads.v1.Pod.ViewModels;
using KubeUI.Kubernetes;
using KubernetesClient.Informer.Client;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Pod.Services;

internal static class PodLogResourceLoader
{
    public static async Task EnsureScopeResourcesAsync(
        IClusterRuntime cluster,
        IKubernetesObject<V1ObjectMeta> resource)
    {
        if (resource is V1Deployment)
        {
            await cluster.SeedResource<V1ReplicaSet>(true);
        }
        else if (resource is V1CronJob)
        {
            await cluster.SeedResource<V1Job>(true);
        }

        await cluster.SeedResource<V1Pod>(true);
    }

    public static Task EnsureParentResourceAsync(
        IClusterRuntime cluster,
        IKubernetesObject<V1ObjectMeta> resource)
    {
        V1OwnerReference? ownerReference = PodLogFileNameExtensions.GetControllerReference(resource);
        if (ownerReference is null
            || !TryGetResourceKind(cluster, ownerReference, out GroupApiVersionKind ownerKind)
            || !cluster.Permissions.CanI(ownerKind, Verb.List, resource.Namespace()))
        {
            return Task.CompletedTask;
        }

        return cluster.SeedResource(ownerKind, true);
    }

    private static bool TryGetResourceKind(
        IClusterRuntime cluster,
        V1OwnerReference ownerReference,
        out GroupApiVersionKind resourceKind)
    {
        if (cluster.ModelCatalog.TryGetResourceKind(
            ownerReference.ApiVersion ?? string.Empty,
            ownerReference.Kind ?? string.Empty,
            out resourceKind))
        {
            return true;
        }

        resourceKind = ownerReference.Kind switch
        {
            V1Deployment.KubeKind => GroupApiVersionKind.From<V1Deployment>(),
            V1ReplicaSet.KubeKind => GroupApiVersionKind.From<V1ReplicaSet>(),
            V1DaemonSet.KubeKind => GroupApiVersionKind.From<V1DaemonSet>(),
            V1StatefulSet.KubeKind => GroupApiVersionKind.From<V1StatefulSet>(),
            V1Job.KubeKind => GroupApiVersionKind.From<V1Job>(),
            V1CronJob.KubeKind => GroupApiVersionKind.From<V1CronJob>(),
            _ => default,
        };
        return !resourceKind.Equals(default(GroupApiVersionKind));
    }
}
