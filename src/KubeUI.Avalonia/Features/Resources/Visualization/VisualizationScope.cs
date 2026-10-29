using KubeUI.Kubernetes.Resources.Relationships;

namespace KubeUI.Avalonia.Features.Resources.Visualization;

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
