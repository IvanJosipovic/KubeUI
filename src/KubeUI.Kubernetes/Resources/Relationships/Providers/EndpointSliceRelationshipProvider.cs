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
            V1ObjectReference? targetReference = endpoint.TargetRef;
            if (targetReference == null)
            {
                continue;
            }

            string? targetName = targetReference.Name;
            if (string.IsNullOrWhiteSpace(targetName))
            {
                continue;
            }

            string? targetNamespace = string.IsNullOrWhiteSpace(targetReference.NamespaceProperty)
                ? endpointSlice.Namespace()
                : targetReference.NamespaceProperty;
            if (!context.TryGetByGroupAndKind(string.Empty, V1Pod.KubeKind, out IReadOnlyList<IKubernetesObject<V1ObjectMeta>> pods))
            {
                continue;
            }

            IKubernetesObject<V1ObjectMeta>? target = pods.FirstOrDefault(pod => pod is V1Pod
                && string.Equals(pod.Name(), targetName, StringComparison.Ordinal)
                && string.Equals(pod.Namespace(), targetNamespace, StringComparison.Ordinal));
            if (target is V1Pod)
            {
                context.Add(relationships, resource, target, ResourceRelationshipKind.Reference);
            }
        }
    }
}
