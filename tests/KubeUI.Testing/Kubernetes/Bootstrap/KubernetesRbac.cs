using k8s;
using k8s.Models;

namespace KubeUI.Testing.Kubernetes.Bootstrap;

public static class KubernetesRbac
{
    public const string ServiceAccountNamespace = "my-app";
    public const string ServiceAccountName = "my-serviceaccount";
    public const string ServiceAccountUser = "system:serviceaccount:my-app:my-serviceaccount";

    public static IReadOnlyCollection<IKubernetesObject<V1ObjectMeta>> ClusterWide(params RbacRule[] rules)
    {
        const string roleName = "kubeui-test-cluster-reader";
        return
        [
            new V1ClusterRole
            {
                ApiVersion = "rbac.authorization.k8s.io/v1",
                Kind = V1ClusterRole.KubeKind,
                Metadata = new V1ObjectMeta { Name = roleName },
                Rules = rules.Select(ToPolicyRule).ToList(),
            },
            new V1ClusterRoleBinding
            {
                ApiVersion = "rbac.authorization.k8s.io/v1",
                Kind = V1ClusterRoleBinding.KubeKind,
                Metadata = new V1ObjectMeta { Name = "kubeui-test-cluster-reader-binding" },
                RoleRef = new() { ApiGroup = "rbac.authorization.k8s.io", Kind = "ClusterRole", Name = roleName },
                Subjects = [new Rbacv1Subject { Kind = "ServiceAccount", NamespaceProperty = ServiceAccountNamespace, Name = ServiceAccountName }],
            },
        ];
    }

    public static IReadOnlyCollection<IKubernetesObject<V1ObjectMeta>> InNamespace(string @namespace, params RbacRule[] rules)
    {
        const string roleName = "kubeui-test-namespace-reader";
        return
        [
            NamespacedRole(@namespace, roleName, rules),
            NamespacedRoleBinding(@namespace, "kubeui-test-namespace-reader-binding", roleName),
        ];
    }

    /// <summary>
    /// Creates a namespaced role bound to the shared test service account.
    /// </summary>
    public static V1Role NamespacedRole(string @namespace, string name, params RbacRule[] rules)
        => new()
        {
            ApiVersion = "rbac.authorization.k8s.io/v1",
            Kind = V1Role.KubeKind,
            Metadata = new V1ObjectMeta { NamespaceProperty = @namespace, Name = name },
            Rules = rules.Select(ToPolicyRule).ToList(),
        };

    /// <summary>
    /// Creates a namespaced role binding for the shared test service account.
    /// </summary>
    public static V1RoleBinding NamespacedRoleBinding(string @namespace, string name, string roleName)
        => new()
        {
            ApiVersion = "rbac.authorization.k8s.io/v1",
            Kind = V1RoleBinding.KubeKind,
            Metadata = new V1ObjectMeta { NamespaceProperty = @namespace, Name = name },
            RoleRef = new() { ApiGroup = "rbac.authorization.k8s.io", Kind = "Role", Name = roleName },
            Subjects = [new Rbacv1Subject { Kind = "ServiceAccount", NamespaceProperty = ServiceAccountNamespace, Name = ServiceAccountName }],
        };

    private static V1PolicyRule ToPolicyRule(RbacRule rule)
        => new()
        {
            ApiGroups = [rule.ApiGroup],
            Resources = [string.IsNullOrEmpty(rule.Subresource) ? rule.Resource : $"{rule.Resource}/{rule.Subresource}"],
            Verbs = [rule.Verb],
        };
}

public sealed record RbacRule(
    string Resource,
    string Verb,
    string ApiGroup = "",
    string? Subresource = null);
