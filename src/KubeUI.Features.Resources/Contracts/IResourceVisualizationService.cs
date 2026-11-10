using k8s;
using k8s.Models;
using KubeUI.Features.Clusters.Workspace;
using KubeUI.Kubernetes.Resources.Relationships;

namespace KubeUI.Features.Resources.Contracts;

/// <summary>Opens resource visualizations without exposing their ViewModel or Dock implementation.</summary>
public interface IResourceVisualizationService
{
    /// <summary>Opens a live visualization for a named resource scope.</summary>
    /// <param name="cluster">Cluster containing the scope.</param>
    /// <param name="scope">Resource selection and display settings.</param>
    void OpenScope(ClusterWorkspace cluster, VisualizationScope scope);

    /// <summary>Opens an isolated preview from rendered Kubernetes resources.</summary>
    /// <param name="cluster">Cluster providing preview presentation context.</param>
    /// <param name="releaseName">Name shown for the preview.</param>
    /// <param name="resources">Rendered resources to visualize.</param>
    void OpenPreview(ClusterWorkspace cluster, string releaseName, IEnumerable<IKubernetesObject<V1ObjectMeta>> resources);
}

/// <summary>Describes a named live resource selection independently of its originating feature.</summary>
/// <param name="Namespace">Initial local namespace selection.</param>
/// <param name="ResourceDisplay">Localized resource description shown in the graph and document title.</param>
/// <param name="Selector">Annotations selecting graph roots, combined with AND semantics.</param>
/// <param name="IsNamespaceReadOnly">Whether namespace selection and linking are disabled.</param>
public sealed record VisualizationScope(
    string Namespace,
    string ResourceDisplay,
    ResourceAnnotationSelector Selector,
    bool IsNamespaceReadOnly);
