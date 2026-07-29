using k8s;
using k8s.Models;

namespace KubeUI.Kubernetes.Resources.Relationships.Providers;

public sealed class StorageRelationshipProvider : IResourceRelationshipProvider
{
    public IEnumerable<ResourceSeedPrerequisite> SeedPrerequisites =>
    [
        new(typeof(V1PersistentVolume)),
        new(typeof(V1PersistentVolumeClaim)),
        new(typeof(V1StorageClass)),
    ];

    public void AddRelationships(IKubernetesObject<V1ObjectMeta> resource, ResourceRelationshipContext context, ICollection<ResourceRelationship> relationships)
    {
        if (resource is V1PersistentVolumeClaim claim)
        {
            RelationshipProviderHelpers.AddByName(context, relationships, resource, "v1", V1PersistentVolume.KubeKind, null, claim.Spec?.VolumeName, ResourceRelationshipKind.Storage);
            RelationshipProviderHelpers.AddByName(context, relationships, resource, "storage.k8s.io/v1", V1StorageClass.KubeKind, null, claim.Spec?.StorageClassName, ResourceRelationshipKind.Storage);
        }

        if (resource is V1PersistentVolume volume)
        {
            RelationshipProviderHelpers.AddByName(context, relationships, resource, "storage.k8s.io/v1", V1StorageClass.KubeKind, null, volume.Spec?.StorageClassName, ResourceRelationshipKind.Storage);
        }
    }
}
