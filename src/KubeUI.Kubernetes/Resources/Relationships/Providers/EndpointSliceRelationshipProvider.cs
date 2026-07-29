using k8s;
using k8s.Models;

namespace KubeUI.Kubernetes.Resources.Relationships.Providers;

public sealed class EndpointSliceRelationshipProvider : IResourceRelationshipProvider
{
    public IEnumerable<ResourceSeedPrerequisite> SeedPrerequisites =>
    [
        new(typeof(V1EndpointSlice)),
        new(typeof(V1Pod)),
    ];

    public void AddRelationships(IKubernetesObject<V1ObjectMeta> resource, ResourceRelationshipContext context, ICollection<ResourceRelationship> relationships)
    {
        if (resource is not V1EndpointSlice endpointSlice)
        {
            return;
        }

        foreach (V1Endpoint endpoint in endpointSlice.Endpoints ?? [])
        {
            if (endpoint.TargetRef?.Uid is not { Length: > 0 } uid || !context.TryGetByUid(uid, out IKubernetesObject<V1ObjectMeta>? target) || target is not V1Pod)
            {
                continue;
            }

            context.Add(relationships, resource, target, ResourceRelationshipKind.Reference);
        }
    }
}
