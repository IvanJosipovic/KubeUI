using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;

namespace KubeUI.Kubernetes.Resources.Relationships.Providers;

public sealed class ReferencedResourceRelationshipProvider : IResourceRelationshipProvider
{
    public IEnumerable<ResourceSeedPrerequisite> SeedPrerequisites =>
    [
        new(GroupApiVersionKind.From<V2HorizontalPodAutoscaler>()),
        new(GroupApiVersionKind.From<V1Deployment>()),
        new(GroupApiVersionKind.From<V1StatefulSet>()),
        new(GroupApiVersionKind.From<V1ReplicaSet>()),
        new(GroupApiVersionKind.From<V1DaemonSet>()),
        new(GroupApiVersionKind.From<V1PriorityClass>()),
        new(GroupApiVersionKind.From<V1RuntimeClass>()),
        new(GroupApiVersionKind.From<V1Pod>()),
        new(GroupApiVersionKind.From<V1ValidatingWebhookConfiguration>()),
        new(GroupApiVersionKind.From<V1MutatingWebhookConfiguration>()),
        new(GroupApiVersionKind.From<V1Service>()),
        new(GroupApiVersionKind.From<V1ConfigMap>()),
        new(GroupApiVersionKind.From<V1Secret>()),
        new(GroupApiVersionKind.From<V1ServiceAccount>()),
    ];

    public void AddRelationships(IKubernetesObject<V1ObjectMeta> resource, ResourceRelationshipContext context, ICollection<ResourceRelationship> relationships)
    {
        if (resource is V2HorizontalPodAutoscaler horizontalPodAutoscaler)
        {
            AddScaleTarget(horizontalPodAutoscaler, context, relationships);
        }

        var podSpec = RelationshipProviderHelpers.PodSpec(resource);
        if (podSpec != null)
        {
            RelationshipProviderHelpers.AddByName(context, relationships, resource, V1PriorityClass.KubeApiVersion, V1PriorityClass.KubeKind, null, podSpec.PriorityClassName, ResourceRelationshipKind.Reference);
            RelationshipProviderHelpers.AddByName(context, relationships, resource, V1RuntimeClass.KubeApiVersion, V1RuntimeClass.KubeKind, null, podSpec.RuntimeClassName, ResourceRelationshipKind.Reference);
            AddProjectedVolumeReferences(resource, podSpec, context, relationships);
        }

        if (resource is V1ValidatingWebhookConfiguration validating)
        {
            foreach (var webhook in validating.Webhooks ?? [])
            {
                AddWebhookService(resource, webhook.ClientConfig?.Service, context, relationships);
            }
        }

        if (resource is V1MutatingWebhookConfiguration mutating)
        {
            foreach (var webhook in mutating.Webhooks ?? [])
            {
                AddWebhookService(resource, webhook.ClientConfig?.Service, context, relationships);
            }
        }
    }

    private static void AddScaleTarget(V2HorizontalPodAutoscaler autoscaler, ResourceRelationshipContext context, ICollection<ResourceRelationship> relationships)
    {
        var targetReference = autoscaler.Spec?.ScaleTargetRef;
        if (targetReference == null)
        {
            return;
        }

        RelationshipProviderHelpers.AddByName(context, relationships, autoscaler, targetReference.ApiVersion ?? "apps/v1", targetReference.Kind, autoscaler.Namespace(), targetReference.Name, ResourceRelationshipKind.Reference);
    }

    private static void AddProjectedVolumeReferences(IKubernetesObject<V1ObjectMeta> resource, V1PodSpec podSpec, ResourceRelationshipContext context, ICollection<ResourceRelationship> relationships)
    {
        foreach (var projection in (podSpec.Volumes ?? []).SelectMany(static volume => volume.Projected?.Sources ?? []))
        {
            RelationshipProviderHelpers.AddByName(context, relationships, resource, V1ConfigMap.KubeApiVersion, V1ConfigMap.KubeKind, resource.Namespace(), projection.ConfigMap?.Name, ResourceRelationshipKind.Reference);
            RelationshipProviderHelpers.AddByName(context, relationships, resource, V1Secret.KubeApiVersion, V1Secret.KubeKind, resource.Namespace(), projection.Secret?.Name, ResourceRelationshipKind.Reference);
            RelationshipProviderHelpers.AddByName(context, relationships, resource, V1ServiceAccount.KubeApiVersion, V1ServiceAccount.KubeKind, resource.Namespace(), projection.ServiceAccountToken == null ? null : podSpec.ServiceAccountName, ResourceRelationshipKind.Identity);
        }
    }

    private static void AddWebhookService(IKubernetesObject<V1ObjectMeta> source, Admissionregistrationv1ServiceReference? service, ResourceRelationshipContext context, ICollection<ResourceRelationship> relationships)
        => RelationshipProviderHelpers.AddByName(context, relationships, source, V1Service.KubeApiVersion, V1Service.KubeKind, service?.NamespaceProperty, service?.Name, ResourceRelationshipKind.Reference);
}
