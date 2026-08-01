using k8s;
using k8s.Models;

namespace KubeUI.Kubernetes.Resources.Relationships.Providers;

public sealed class OwnerReferenceRelationshipProvider : IResourceRelationshipProvider
{
    public void AddRelationships(IKubernetesObject<V1ObjectMeta> resource, ResourceRelationshipContext context, ICollection<ResourceRelationship> relationships)
    {
        foreach (var owner in resource.Metadata?.OwnerReferences ?? [])
        {
            if (owner.Kind == V1Namespace.KubeKind || !context.TryGetByUid(owner.Uid, out var target) || target == null)
            {
                continue;
            }

            context.Add(relationships, target, resource, ResourceRelationshipKind.Owner);
        }
    }
}
