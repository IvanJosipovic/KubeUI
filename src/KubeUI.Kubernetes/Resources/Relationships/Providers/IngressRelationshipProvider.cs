using k8s;
using k8s.Models;

namespace KubeUI.Kubernetes.Resources.Relationships.Providers;

public sealed class IngressRelationshipProvider : IResourceRelationshipProvider
{
    public IEnumerable<ResourceSeedPrerequisite> SeedPrerequisites =>
    [
        new(typeof(V1Ingress)),
        new(typeof(V1IngressClass)),
        new(typeof(V1Service)),
    ];

    public void AddRelationships(IKubernetesObject<V1ObjectMeta> resource, ResourceRelationshipContext context, ICollection<ResourceRelationship> relationships)
    {
        if (resource is not V1Ingress ingress)
        {
            return;
        }

        RelationshipProviderHelpers.AddByName(
            context,
            relationships,
            resource,
            $"{V1IngressClass.KubeGroup}/{V1IngressClass.KubeApiVersion}",
            V1IngressClass.KubeKind,
            null,
            ingress.Spec?.IngressClassName,
            ResourceRelationshipKind.Reference);

        foreach (V1HTTPIngressPath path in ingress.Spec?.Rules?.SelectMany(x => x.Http?.Paths ?? []) ?? [])
        {
            RelationshipProviderHelpers.AddByName(context, relationships, resource, "v1", V1Service.KubeKind, resource.Namespace(), path.Backend?.Service?.Name, ResourceRelationshipKind.Reference);
        }

        RelationshipProviderHelpers.AddByName(context, relationships, resource, "v1", V1Service.KubeKind, resource.Namespace(), ingress.Spec?.DefaultBackend?.Service?.Name, ResourceRelationshipKind.Reference);
    }
}
