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
        if (context.TryGet(apiVersion, kind, namespaceName, name, out var target)
            && target != null)
        {
            context.Add(relationships, source, target, relationshipKind, label);
        }
    }

    public static void AddByTargetReference(
        ResourceRelationshipContext context,
        ICollection<ResourceRelationship> relationships,
        IKubernetesObject<V1ObjectMeta> source,
        V1ObjectReference? targetReference,
        string? defaultNamespace,
        Type targetType,
        ResourceRelationshipKind relationshipKind)
    {
        IKubernetesObject<V1ObjectMeta>? target = null;
        if (!string.IsNullOrWhiteSpace(targetReference?.Uid))
        {
            context.TryGetByUid(targetReference.Uid, out target);
        }

        if (target == null && targetReference != null)
        {
            var apiVersion = string.IsNullOrWhiteSpace(targetReference.ApiVersion)
                ? V1Pod.KubeApiVersion
                : targetReference.ApiVersion;
            context.TryGet(apiVersion, targetReference.Kind ?? string.Empty, targetReference.NamespaceProperty ?? defaultNamespace, targetReference.Name, out target);
        }

        if (target == null
            && targetReference != null
            && context.TryGetByGroupAndKind(string.Empty, string.IsNullOrWhiteSpace(targetReference.Kind) ? V1Pod.KubeKind : targetReference.Kind, out var candidates))
        {
            target = candidates.FirstOrDefault(candidate =>
                string.Equals(candidate.Namespace(), targetReference.NamespaceProperty ?? defaultNamespace, StringComparison.Ordinal)
                && string.Equals(candidate.Name(), targetReference.Name, StringComparison.Ordinal));
        }

        if (target != null && targetType.IsInstanceOfType(target))
        {
            context.Add(relationships, source, target, relationshipKind);
        }
    }

    public static void AddBySelector(
        ResourceRelationshipContext context,
        ICollection<ResourceRelationship> relationships,
        IKubernetesObject<V1ObjectMeta> source,
        string apiGroup,
        string kind,
        V1LabelSelector? selector,
        string? namespaceName,
        ResourceRelationshipKind relationshipKind)
    {
        foreach (var target in context.SelectByLabelSelector(apiGroup, kind, selector, namespaceName))
        {
            context.Add(relationships, source, target, relationshipKind);
        }
    }
}
