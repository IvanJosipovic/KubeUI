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
            new V1Role
            {
                ApiVersion = "rbac.authorization.k8s.io/v1",
                Kind = V1Role.KubeKind,
                Metadata = new V1ObjectMeta { NamespaceProperty = @namespace, Name = roleName },
                Rules = rules.Select(ToPolicyRule).ToList(),
            },
            new V1RoleBinding
            {
                ApiVersion = "rbac.authorization.k8s.io/v1",
                Kind = V1RoleBinding.KubeKind,
                Metadata = new V1ObjectMeta { NamespaceProperty = @namespace, Name = "kubeui-test-namespace-reader-binding" },
                RoleRef = new() { ApiGroup = "rbac.authorization.k8s.io", Kind = "Role", Name = roleName },
                Subjects = [new Rbacv1Subject { Kind = "ServiceAccount", NamespaceProperty = ServiceAccountNamespace, Name = ServiceAccountName }],
            },
        ];
    }

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
