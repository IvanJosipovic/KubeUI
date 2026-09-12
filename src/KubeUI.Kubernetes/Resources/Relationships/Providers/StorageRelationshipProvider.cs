using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;

namespace KubeUI.Kubernetes.Resources.Relationships.Providers;

public sealed class StorageRelationshipProvider : IResourceRelationshipProvider
{
    public IEnumerable<ResourceSeedPrerequisite> SeedPrerequisites =>
    [
        new(GroupApiVersionKind.From<V1PersistentVolume>()),
        new(GroupApiVersionKind.From<V1PersistentVolumeClaim>()),
        new(GroupApiVersionKind.From<V1StorageClass>()),
    ];

    public void AddRelationships(IKubernetesObject<V1ObjectMeta> resource, ResourceRelationshipContext context, ICollection<ResourceRelationship> relationships)
    {
        if (resource is V1PersistentVolumeClaim claim)
        {
            RelationshipProviderHelpers.AddByName(context, relationships, resource, V1PersistentVolume.KubeApiVersion, V1PersistentVolume.KubeKind, null, claim.Spec?.VolumeName, ResourceRelationshipKind.Storage);
            RelationshipProviderHelpers.AddByName(context, relationships, resource, $"{V1StorageClass.KubeGroup}/{V1StorageClass.KubeApiVersion}", V1StorageClass.KubeKind, null, claim.Spec?.StorageClassName, ResourceRelationshipKind.Storage);
        }

        if (resource is V1PersistentVolume volume)
        {
            RelationshipProviderHelpers.AddByName(context, relationships, resource, V1PersistentVolumeClaim.KubeApiVersion, V1PersistentVolumeClaim.KubeKind, volume.Spec?.ClaimRef?.NamespaceProperty, volume.Spec?.ClaimRef?.Name, ResourceRelationshipKind.Storage);
            RelationshipProviderHelpers.AddByName(context, relationships, resource, $"{V1StorageClass.KubeGroup}/{V1StorageClass.KubeApiVersion}", V1StorageClass.KubeKind, null, volume.Spec?.StorageClassName, ResourceRelationshipKind.Storage);
        }

        if (resource is V1PersistentVolumeClaim claimWithDataSource)
        {
            var apiVersion = claimWithDataSource.Spec?.DataSourceRef?.ApiGroup is { Length: > 0 } group
                ? $"{group}/v1"
                : claimWithDataSource.Spec?.DataSource?.Kind == V1PersistentVolumeClaim.KubeKind ? V1PersistentVolumeClaim.KubeApiVersion : null;
            var kind = claimWithDataSource.Spec?.DataSourceRef?.Kind ?? claimWithDataSource.Spec?.DataSource?.Kind;
            var name = claimWithDataSource.Spec?.DataSourceRef?.Name ?? claimWithDataSource.Spec?.DataSource?.Name;
            if (apiVersion != null && kind != null)
            {
                RelationshipProviderHelpers.AddByName(context, relationships, resource, apiVersion, kind, claimWithDataSource.Namespace(), name, ResourceRelationshipKind.Storage);
            }
        }
    }
}
