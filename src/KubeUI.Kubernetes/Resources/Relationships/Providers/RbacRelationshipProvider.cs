using k8s;
using k8s.Models;

namespace KubeUI.Kubernetes.Resources.Relationships.Providers;

public sealed class RbacRelationshipProvider : IResourceRelationshipProvider
{
    public IEnumerable<ResourceSeedPrerequisite> SeedPrerequisites =>
    [
        new(typeof(V1RoleBinding)),
        new(typeof(V1ClusterRoleBinding)),
        new(typeof(V1ServiceAccount)),
        new(typeof(V1Role)),
        new(typeof(V1ClusterRole)),
    ];

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
            if (subject.Kind == V1ServiceAccount.KubeKind)
            {
                RelationshipProviderHelpers.AddByName(context, relationships, resource, V1ServiceAccount.KubeApiVersion, V1ServiceAccount.KubeKind, subject.NamespaceProperty ?? resource.Namespace(), subject.Name, ResourceRelationshipKind.Rbac);
            }
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
