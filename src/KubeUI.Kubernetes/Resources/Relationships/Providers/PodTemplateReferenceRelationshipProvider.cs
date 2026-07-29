using k8s;
using k8s.Models;

namespace KubeUI.Kubernetes.Resources.Relationships.Providers;

public sealed class PodTemplateReferenceRelationshipProvider : IResourceRelationshipProvider
{
    public IEnumerable<ResourceSeedPrerequisite> SeedPrerequisites =>
    [
        new(typeof(V1Pod)),
        new(typeof(V1Node)),
        new(typeof(V1Deployment)),
        new(typeof(V1ReplicaSet)),
        new(typeof(V1StatefulSet)),
        new(typeof(V1DaemonSet)),
        new(typeof(V1Job)),
        new(typeof(V1CronJob)),
        new(typeof(V1ConfigMap)),
        new(typeof(V1Secret)),
        new(typeof(V1PersistentVolumeClaim)),
    ];

    public void AddRelationships(IKubernetesObject<V1ObjectMeta> resource, ResourceRelationshipContext context, ICollection<ResourceRelationship> relationships)
    {
        V1PodSpec? podSpec = RelationshipProviderHelpers.PodSpec(resource);
        if (podSpec == null)
        {
            return;
        }

        RelationshipProviderHelpers.AddByName(
            context,
            relationships,
            resource,
            V1Node.KubeApiVersion,
            V1Node.KubeKind,
            null,
            podSpec.NodeName,
            ResourceRelationshipKind.Reference);

        foreach (V1Container container in (podSpec.Containers ?? []).Concat(podSpec.InitContainers ?? []))
        {
            foreach (V1EnvVar env in container.Env ?? [])
            {
                RelationshipProviderHelpers.AddByName(context, relationships, resource, V1ConfigMap.KubeApiVersion, V1ConfigMap.KubeKind, resource.Namespace(), env.ValueFrom?.ConfigMapKeyRef?.Name, ResourceRelationshipKind.Reference);
                RelationshipProviderHelpers.AddByName(context, relationships, resource, V1Secret.KubeApiVersion, V1Secret.KubeKind, resource.Namespace(), env.ValueFrom?.SecretKeyRef?.Name, ResourceRelationshipKind.Reference);
            }

            foreach (V1EnvFromSource envFrom in container.EnvFrom ?? [])
            {
                RelationshipProviderHelpers.AddByName(context, relationships, resource, V1ConfigMap.KubeApiVersion, V1ConfigMap.KubeKind, resource.Namespace(), envFrom.ConfigMapRef?.Name, ResourceRelationshipKind.Reference);
                RelationshipProviderHelpers.AddByName(context, relationships, resource, V1Secret.KubeApiVersion, V1Secret.KubeKind, resource.Namespace(), envFrom.SecretRef?.Name, ResourceRelationshipKind.Reference);
            }
        }

        foreach (V1Volume volume in podSpec.Volumes ?? [])
        {
            RelationshipProviderHelpers.AddByName(context, relationships, resource, V1ConfigMap.KubeApiVersion, V1ConfigMap.KubeKind, resource.Namespace(), volume.ConfigMap?.Name, ResourceRelationshipKind.Reference);
            RelationshipProviderHelpers.AddByName(context, relationships, resource, V1Secret.KubeApiVersion, V1Secret.KubeKind, resource.Namespace(), volume.Secret?.SecretName, ResourceRelationshipKind.Reference);
            RelationshipProviderHelpers.AddByName(context, relationships, resource, V1PersistentVolumeClaim.KubeApiVersion, V1PersistentVolumeClaim.KubeKind, resource.Namespace(), volume.PersistentVolumeClaim?.ClaimName, ResourceRelationshipKind.Storage);
        }

        foreach (V1LocalObjectReference imagePullSecret in podSpec.ImagePullSecrets ?? [])
        {
            RelationshipProviderHelpers.AddByName(context, relationships, resource, V1Secret.KubeApiVersion, V1Secret.KubeKind, resource.Namespace(), imagePullSecret.Name, ResourceRelationshipKind.Reference);
        }
    }
}
