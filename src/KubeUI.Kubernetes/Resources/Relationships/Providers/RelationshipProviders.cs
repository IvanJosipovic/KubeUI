using k8s;
using k8s.Models;

namespace KubeUI.Kubernetes.Resources.Relationships.Providers;

internal static class RelationshipProviderHelpers
{
    public static V1PodSpec? PodSpec(IKubernetesObject<V1ObjectMeta> resource)
        => resource switch
        {
            V1Deployment x => x.Spec?.Template?.Spec,
            V1ReplicaSet x => x.Spec?.Template?.Spec,
            V1StatefulSet x => x.Spec?.Template?.Spec,
            V1DaemonSet x => x.Spec?.Template?.Spec,
            V1Job x => x.Spec?.Template?.Spec,
            V1CronJob x => x.Spec?.JobTemplate?.Spec?.Template?.Spec,
            V1Pod x => x.Spec,
            _ => null,
        };

    public static void AddByName(
        ResourceRelationshipContext context,
        ICollection<ResourceRelationship> relationships,
        IKubernetesObject<V1ObjectMeta> source,
        string apiVersion,
        string kind,
        string? namespaceName,
        string? name,
        ResourceRelationshipKind relationshipKind,
        string? label = null)
    {
        if (context.TryGet(apiVersion, kind, namespaceName, name, out IKubernetesObject<V1ObjectMeta>? target)
            && target != null)
        {
            context.Add(relationships, source, target, relationshipKind, label);
        }
    }

}

public sealed class OwnerReferenceRelationshipProvider : IResourceRelationshipProvider
{
    public void AddRelationships(IKubernetesObject<V1ObjectMeta> resource, ResourceRelationshipContext context, ICollection<ResourceRelationship> relationships)
    {
        foreach (V1OwnerReference owner in resource.Metadata?.OwnerReferences ?? [])
        {
            if (owner.Kind == V1Namespace.KubeKind || !context.TryGetByUid(owner.Uid, out IKubernetesObject<V1ObjectMeta>? target) || target == null)
            {
                continue;
            }

            context.Add(relationships, target, resource, ResourceRelationshipKind.Owner);
        }
    }
}

public sealed class IngressRelationshipProvider : IResourceRelationshipProvider
{
    public void AddRelationships(IKubernetesObject<V1ObjectMeta> resource, ResourceRelationshipContext context, ICollection<ResourceRelationship> relationships)
    {
        if (resource is not V1Ingress ingress)
        {
            return;
        }

        foreach (V1HTTPIngressPath path in ingress.Spec?.Rules?.SelectMany(x => x.Http?.Paths ?? []) ?? [])
        {
            RelationshipProviderHelpers.AddByName(context, relationships, resource, "v1", V1Service.KubeKind, resource.Namespace(), path.Backend?.Service?.Name, ResourceRelationshipKind.Reference);
        }

        RelationshipProviderHelpers.AddByName(context, relationships, resource, "v1", V1Service.KubeKind, resource.Namespace(), ingress.Spec?.DefaultBackend?.Service?.Name, ResourceRelationshipKind.Reference);
    }
}

public sealed class EndpointSliceRelationshipProvider : IResourceRelationshipProvider
{
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

public sealed class PodTemplateReferenceRelationshipProvider : IResourceRelationshipProvider
{
    public void AddRelationships(IKubernetesObject<V1ObjectMeta> resource, ResourceRelationshipContext context, ICollection<ResourceRelationship> relationships)
    {
        V1PodSpec? podSpec = RelationshipProviderHelpers.PodSpec(resource);
        if (podSpec == null)
        {
            return;
        }

        foreach (V1Container container in (podSpec.Containers ?? []).Concat(podSpec.InitContainers ?? []))
        {
            foreach (V1EnvVar env in container.Env ?? [])
            {
                RelationshipProviderHelpers.AddByName(context, relationships, resource, "v1", V1ConfigMap.KubeKind, resource.Namespace(), env.ValueFrom?.ConfigMapKeyRef?.Name, ResourceRelationshipKind.Reference);
                RelationshipProviderHelpers.AddByName(context, relationships, resource, "v1", V1Secret.KubeKind, resource.Namespace(), env.ValueFrom?.SecretKeyRef?.Name, ResourceRelationshipKind.Reference);
            }

            foreach (V1EnvFromSource envFrom in container.EnvFrom ?? [])
            {
                RelationshipProviderHelpers.AddByName(context, relationships, resource, "v1", V1ConfigMap.KubeKind, resource.Namespace(), envFrom.ConfigMapRef?.Name, ResourceRelationshipKind.Reference);
                RelationshipProviderHelpers.AddByName(context, relationships, resource, "v1", V1Secret.KubeKind, resource.Namespace(), envFrom.SecretRef?.Name, ResourceRelationshipKind.Reference);
            }
        }

        foreach (V1Volume volume in podSpec.Volumes ?? [])
        {
            RelationshipProviderHelpers.AddByName(context, relationships, resource, "v1", V1ConfigMap.KubeKind, resource.Namespace(), volume.ConfigMap?.Name, ResourceRelationshipKind.Reference);
            RelationshipProviderHelpers.AddByName(context, relationships, resource, "v1", V1Secret.KubeKind, resource.Namespace(), volume.Secret?.SecretName, ResourceRelationshipKind.Reference);
            RelationshipProviderHelpers.AddByName(context, relationships, resource, "v1", V1PersistentVolumeClaim.KubeKind, resource.Namespace(), volume.PersistentVolumeClaim?.ClaimName, ResourceRelationshipKind.Storage);
        }

        foreach (V1LocalObjectReference imagePullSecret in podSpec.ImagePullSecrets ?? [])
        {
            RelationshipProviderHelpers.AddByName(context, relationships, resource, "v1", V1Secret.KubeKind, resource.Namespace(), imagePullSecret.Name, ResourceRelationshipKind.Reference);
        }
    }
}

public sealed class ServiceAccountRelationshipProvider : IResourceRelationshipProvider
{
    public void AddRelationships(IKubernetesObject<V1ObjectMeta> resource, ResourceRelationshipContext context, ICollection<ResourceRelationship> relationships)
    {
        V1PodSpec? podSpec = RelationshipProviderHelpers.PodSpec(resource);
        if (podSpec != null)
        {
            RelationshipProviderHelpers.AddByName(context, relationships, resource, "v1", V1ServiceAccount.KubeKind, resource.Namespace(), podSpec.ServiceAccountName, ResourceRelationshipKind.Identity);
        }

        if (resource is V1ServiceAccount serviceAccount)
        {
            foreach (V1ObjectReference secret in serviceAccount.Secrets ?? [])
            {
                if (context.TryGetByUid(secret.Uid, out IKubernetesObject<V1ObjectMeta>? target) && target is V1Secret && target.Namespace() == resource.Namespace())
                {
                    context.Add(relationships, resource, target, ResourceRelationshipKind.Identity);
                }
            }
        }
    }
}

public sealed class StorageRelationshipProvider : IResourceRelationshipProvider
{
    public void AddRelationships(IKubernetesObject<V1ObjectMeta> resource, ResourceRelationshipContext context, ICollection<ResourceRelationship> relationships)
    {
        if (resource is V1PersistentVolumeClaim claim)
        {
            RelationshipProviderHelpers.AddByName(context, relationships, resource, "v1", V1PersistentVolume.KubeKind, null, claim.Spec?.VolumeName, ResourceRelationshipKind.Storage);
        }
    }
}

public sealed class RbacRelationshipProvider : IResourceRelationshipProvider
{
    public void AddRelationships(IKubernetesObject<V1ObjectMeta> resource, ResourceRelationshipContext context, ICollection<ResourceRelationship> relationships)
    {
        V1RoleBinding? roleBinding = resource as V1RoleBinding;
        V1ClusterRoleBinding? clusterRoleBinding = resource as V1ClusterRoleBinding;
        if (roleBinding == null && clusterRoleBinding == null)
        {
            return;
        }

        IEnumerable<dynamic> subjects = roleBinding?.Subjects ?? clusterRoleBinding?.Subjects ?? [];
        foreach (var subject in subjects)
        {
            RelationshipProviderHelpers.AddByName(context, relationships, resource, "v1", V1ServiceAccount.KubeKind, subject.NamespaceProperty ?? resource.Namespace(), subject.Name, ResourceRelationshipKind.Rbac);
        }

        var roleRef = roleBinding?.RoleRef ?? clusterRoleBinding?.RoleRef;
        if (roleRef == null)
        {
            return;
        }

        string? namespaceName = roleBinding != null ? resource.Namespace() : null;
        string? apiVersion = roleRef.ApiGroup == "rbac.authorization.k8s.io" ? "rbac.authorization.k8s.io/v1" : null;
        if (apiVersion == null)
        {
            return;
        }

        if (context.TryGet(apiVersion, roleRef.Kind, namespaceName, roleRef.Name, out IKubernetesObject<V1ObjectMeta>? role)
            && role != null)
        {
            context.Add(relationships, role, resource, ResourceRelationshipKind.Rbac);
        }
    }
}

public sealed class EventRelationshipProvider : IResourceRelationshipProvider
{
    public void AddRelationships(IKubernetesObject<V1ObjectMeta> resource, ResourceRelationshipContext context, ICollection<ResourceRelationship> relationships)
    {
        if (resource is Corev1Event @event && context.TryGetByUid(@event.InvolvedObject?.Uid, out IKubernetesObject<V1ObjectMeta>? target) && target != null)
        {
            context.Add(relationships, resource, target, ResourceRelationshipKind.Event);
        }
    }
}

public sealed class GitOpsRelationshipProvider : IResourceRelationshipProvider
{
    public void AddRelationships(IKubernetesObject<V1ObjectMeta> resource, ResourceRelationshipContext context, ICollection<ResourceRelationship> relationships)
    {
        string? trackingId = TryGet(resource.Metadata?.Annotations, "argocd.argoproj.io/tracking-id");
        if (trackingId != null)
        {
            string? name = trackingId.Split(':', 2).FirstOrDefault();
            if (context.TryGetByName("argoproj.io/v1alpha1", "Application", name, out IKubernetesObject<V1ObjectMeta>? application)
                && application != null)
            {
                context.Add(relationships, application, resource, ResourceRelationshipKind.GitOps);
            }
        }

        AddFlux(context, relationships, resource, "kustomize.toolkit.fluxcd.io/name", "kustomize.toolkit.fluxcd.io/namespace", "kustomize.toolkit.fluxcd.io/v1", "Kustomization");
        AddFlux(context, relationships, resource, "helm.toolkit.fluxcd.io/name", "helm.toolkit.fluxcd.io/namespace", "helm.toolkit.fluxcd.io/v2", "HelmRelease");
    }

    private static void AddFlux(ResourceRelationshipContext context, ICollection<ResourceRelationship> relationships, IKubernetesObject<V1ObjectMeta> resource, string nameKey, string namespaceKey, string apiVersion, string kind)
    {
        string? name = TryGet(resource.Metadata?.Labels, nameKey);
        string? namespaceName = TryGet(resource.Metadata?.Labels, namespaceKey);
        if (name != null
            && context.TryGet(apiVersion, kind, namespaceName, name, out IKubernetesObject<V1ObjectMeta>? controller)
            && controller != null
            && !ReferenceEquals(controller, resource))
        {
            context.Add(relationships, controller, resource, ResourceRelationshipKind.GitOps);
        }
    }

    private static string? TryGet(IDictionary<string, string>? values, string key)
        => values != null && values.TryGetValue(key, out string? value) ? value : null;
}
