using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;

namespace KubeUI.Kubernetes.Resources.Relationships.Providers;

public sealed class EndpointSliceRelationshipProvider : IResourceRelationshipProvider
{
    public IEnumerable<ResourceSeedPrerequisite> SeedPrerequisites =>
    [
        new(GroupApiVersionKind.From<V1EndpointSlice>()),
        new(GroupApiVersionKind.From<V1Pod>()),
    ];

    public void AddRelationships(IKubernetesObject<V1ObjectMeta> resource, ResourceRelationshipContext context, ICollection<ResourceRelationship> relationships)
    {
        if (resource is not V1EndpointSlice endpointSlice)
        {
            return;
        }

        foreach (var endpoint in endpointSlice.Endpoints ?? [])
        {
            var targetReference = endpoint.TargetRef;
            if (targetReference == null)
            {
                continue;
            }

            var targetName = targetReference.Name;
            if (string.IsNullOrWhiteSpace(targetName))
            {
                continue;
            }

            var targetNamespace = string.IsNullOrWhiteSpace(targetReference.NamespaceProperty)
                ? endpointSlice.Namespace()
                : targetReference.NamespaceProperty;
            if (!context.TryGetByGroupAndKind(string.Empty, V1Pod.KubeKind, out var pods))
            {
                continue;
            }

            var target = pods.FirstOrDefault(pod => pod is V1Pod
                && string.Equals(pod.Name(), targetName, StringComparison.Ordinal)
                && string.Equals(pod.Namespace(), targetNamespace, StringComparison.Ordinal));
            if (target is V1Pod)
            {
                context.Add(relationships, resource, target, ResourceRelationshipKind.Reference);
            }
        }
    }
}
