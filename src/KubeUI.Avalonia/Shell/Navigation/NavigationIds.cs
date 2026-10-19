using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Shell.Navigation;

/// <summary>Builds stable navigation node identifiers.</summary>
internal static class NavigationIds
{
    public static string Section(ClusterWorkspace cluster, string sectionId)
        => $"{cluster.Runtime.Name}-section-{sectionId}";

    public static string Group(ClusterWorkspace cluster, string sectionId)
        => $"{cluster.Runtime.Name}-section-group-{sectionId}";

    public static string Canonical(ClusterWorkspace cluster, GroupApiVersionKind kind)
        => $"{cluster.Runtime.Name}-{kind}";

    public static string Alias(ClusterWorkspace cluster, string sectionId, GroupApiVersionKind kind)
        => $"{cluster.Runtime.Name}-section-resource-{sectionId}-{kind.Group}/{kind.Kind}";

    public static string Document(ClusterWorkspace cluster, string sectionId, string documentId)
        => $"{cluster.Runtime.Name}-section-document-{sectionId}-{documentId}";

    public static string Builtin(ClusterWorkspace cluster, string key)
        => $"{cluster.Runtime.Name}-{key}";
}
