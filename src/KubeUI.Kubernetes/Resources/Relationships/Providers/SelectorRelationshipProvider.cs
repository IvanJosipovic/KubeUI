using k8s;
using k8s.Models;

namespace KubeUI.Kubernetes.Resources.Relationships.Providers;

public sealed class SelectorRelationshipProvider : IResourceRelationshipProvider
{
    public IEnumerable<ResourceSeedPrerequisite> SeedPrerequisites =>
    [
        new(typeof(V1Service)),
        new(typeof(V1EndpointSlice)),
        new(typeof(V1Pod)),
        new(typeof(V1Deployment)),
        new(typeof(V1ReplicaSet)),
        new(typeof(V1StatefulSet)),
        new(typeof(V1DaemonSet)),
        new(typeof(V1Job)),
        new(typeof(V1PodDisruptionBudget)),
        new(typeof(V1NetworkPolicy)),
        new(typeof(V1Namespace)),
    ];

    public void AddRelationships(
        IKubernetesObject<V1ObjectMeta> resource,
        ResourceRelationshipContext context,
        ICollection<ResourceRelationship> relationships)
    {
        switch (resource)
        {
            case V1Service service:
                RelationshipProviderHelpers.AddBySelector(
                    context,
                    relationships,
                    resource,
                    string.Empty,
                    V1Pod.KubeKind,
                    ToSelector(service.Spec?.Selector),
                    resource.Namespace(),
                    ResourceRelationshipKind.Selector);
                break;
            case V1Deployment deployment:
                AddPodsBySelector(deployment.Spec?.Selector, resource, context, relationships);
                break;
            case V1ReplicaSet replicaSet:
                AddPodsBySelector(replicaSet.Spec?.Selector, resource, context, relationships);
                break;
            case V1StatefulSet statefulSet:
                AddPodsBySelector(statefulSet.Spec?.Selector, resource, context, relationships);
                break;
            case V1DaemonSet daemonSet:
                AddPodsBySelector(daemonSet.Spec?.Selector, resource, context, relationships);
                break;
            case V1Job job:
                AddPodsBySelector(job.Spec?.Selector, resource, context, relationships);
                break;
            case V1PodDisruptionBudget podDisruptionBudget:
                RelationshipProviderHelpers.AddBySelector(
                    context,
                    relationships,
                    resource,
                    string.Empty,
                    V1Pod.KubeKind,
                    podDisruptionBudget.Spec?.Selector,
                    resource.Namespace(),
                    ResourceRelationshipKind.Selector);
                break;
            case V1NetworkPolicy networkPolicy:
                AddNetworkPolicyTargets(networkPolicy, context, relationships);
                break;
        }
    }

    private static void AddPodsBySelector(
        V1LabelSelector? selector,
        IKubernetesObject<V1ObjectMeta> source,
        ResourceRelationshipContext context,
        ICollection<ResourceRelationship> relationships)
        => RelationshipProviderHelpers.AddBySelector(
            context,
            relationships,
            source,
            string.Empty,
            V1Pod.KubeKind,
            selector,
            source.Namespace(),
            ResourceRelationshipKind.Selector);

    private static void AddNetworkPolicyTargets(
        V1NetworkPolicy policy,
        ResourceRelationshipContext context,
        ICollection<ResourceRelationship> relationships)
    {
        foreach (var peer in (policy.Spec?.Ingress ?? []).SelectMany(static rule => rule.FromProperty ?? [])
            .Concat((policy.Spec?.Egress ?? []).SelectMany(static rule => rule.To ?? [])))
        {
            if (peer.PodSelector == null && peer.NamespaceSelector == null)
            {
                continue;
            }

            var namespaces = peer.NamespaceSelector == null
                ? [policy.Namespace()]
                : context.SelectNamespaces(peer.NamespaceSelector).Select(static namespaceResource => namespaceResource.Name());

            foreach (var namespaceName in namespaces)
            {
                RelationshipProviderHelpers.AddBySelector(
                    context,
                    relationships,
                    policy,
                    string.Empty,
                    V1Pod.KubeKind,
                    peer.PodSelector ?? new V1LabelSelector(),
                    namespaceName,
                    ResourceRelationshipKind.Selector);
            }
        }
    }

    private static V1LabelSelector? ToSelector(IDictionary<string, string>? selector)
        => selector == null ? null : new V1LabelSelector { MatchLabels = selector };
}
