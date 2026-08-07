using k8s;
using k8s.Models;

namespace KubeUI.Kubernetes.Resources.Relationships.Providers;

public sealed class ServiceAccountRelationshipProvider : IResourceRelationshipProvider
{
    public IEnumerable<ResourceSeedPrerequisite> SeedPrerequisites =>
    [
        new(typeof(V1Pod)),
        new(typeof(V1Deployment)),
        new(typeof(V1ReplicaSet)),
        new(typeof(V1StatefulSet)),
        new(typeof(V1DaemonSet)),
        new(typeof(V1Job)),
        new(typeof(V1CronJob)),
        new(typeof(V1ServiceAccount)),
        new(typeof(V1Secret)),
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
