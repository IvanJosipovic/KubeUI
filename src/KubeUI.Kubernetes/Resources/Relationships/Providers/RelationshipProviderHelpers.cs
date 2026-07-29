using k8s;
using k8s.Models;

namespace KubeUI.Kubernetes.Resources.Relationships.Providers;

internal static class RelationshipProviderHelpers
{
    public static V1PodSpec? PodSpec(IKubernetesObject<V1ObjectMeta> resource)
        => resource switch
        {
            V1Deployment x => x.Spec?.Template?.Spec,
            V1ReplicaSet x => x.Spec?.Template?.Spec,
            V1StatefulSet x => x.Spec?.Template?.Spec,
            V1DaemonSet x => x.Spec?.Template?.Spec,
            V1Job x => x.Spec?.Template?.Spec,
            V1CronJob x => x.Spec?.JobTemplate?.Spec?.Template?.Spec,
            V1Pod x => x.Spec,
            _ => null,
        };

    public static void AddByName(
        ResourceRelationshipContext context,
        ICollection<ResourceRelationship> relationships,
        IKubernetesObject<V1ObjectMeta> source,
        string apiVersion,
        string kind,
        string? namespaceName,
        string? name,
        ResourceRelationshipKind relationshipKind,
        string? label = null)
    {
        if (context.TryGet(apiVersion, kind, namespaceName, name, out IKubernetesObject<V1ObjectMeta>? target)
            && target != null)
        {
            context.Add(relationships, source, target, relationshipKind, label);
        }
    }
}
