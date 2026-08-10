using k8s;
using k8s.Models;

namespace KubeUI.Kubernetes.Resources.Relationships.Providers;

public sealed class EventRelationshipProvider : IResourceRelationshipProvider
{
    public IEnumerable<ResourceSeedPrerequisite> SeedPrerequisites => [new(typeof(Corev1Event))];

    public void AddRelationships(IKubernetesObject<V1ObjectMeta> resource, ResourceRelationshipContext context, ICollection<ResourceRelationship> relationships)
    {
        if (resource is Corev1Event @event && context.TryGetByUid(@event.InvolvedObject?.Uid, out var target) && target != null)
        {
            context.Add(relationships, resource, target, ResourceRelationshipKind.Event);
        }
    }
}
