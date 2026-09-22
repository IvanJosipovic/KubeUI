using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;

namespace KubeUI.Kubernetes.Resources.Relationships.Providers;

public sealed class ServiceAccountRelationshipProvider : IResourceRelationshipProvider
{
    public IEnumerable<ResourceSeedPrerequisite> SeedPrerequisites =>
    [
        new(GroupApiVersionKind.From<V1Pod>()),
        new(GroupApiVersionKind.From<V1Deployment>()),
        new(GroupApiVersionKind.From<V1ReplicaSet>()),
        new(GroupApiVersionKind.From<V1StatefulSet>()),
        new(GroupApiVersionKind.From<V1DaemonSet>()),
        new(GroupApiVersionKind.From<V1Job>()),
        new(GroupApiVersionKind.From<V1CronJob>()),
        new(GroupApiVersionKind.From<V1ServiceAccount>()),
        new(GroupApiVersionKind.From<V1Secret>()),
    ];

    public void AddRelationships(IKubernetesObject<V1ObjectMeta> resource, ResourceRelationshipContext context, ICollection<ResourceRelationship> relationships)
    {
        var podSpec = RelationshipProviderHelpers.PodSpec(resource);
        if (podSpec != null)
        {
            RelationshipProviderHelpers.AddByName(context, relationships, resource, V1ServiceAccount.KubeApiVersion, V1ServiceAccount.KubeKind, resource.Namespace(), podSpec.ServiceAccountName, ResourceRelationshipKind.Identity);
        }

        if (resource is V1ServiceAccount serviceAccount)
        {
            foreach (var secret in serviceAccount.Secrets ?? [])
            {
                if (context.TryGetByUid(secret.Uid, out var target) && target is V1Secret && target.Namespace() == resource.Namespace())
                {
                    context.Add(relationships, resource, target, ResourceRelationshipKind.Identity);
                }
            }
        }
    }
}
