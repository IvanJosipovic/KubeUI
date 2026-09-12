using KubernetesClient.Informer.Client;
using KubeUI.Kubernetes.Resources.Relationships;

namespace KubeUI.Avalonia.Features.Resources.Visualization;

/// <summary>Immutable result published by visualization preparation stages.</summary>
internal sealed record VisualizationPipelineState(
    ResourceRelationshipGraph CompleteGraph,
    IReadOnlySet<UnresolvedResourceReference> PendingReferences,
    IReadOnlySet<GroupApiVersionKind> RequiredSeedKinds,
    IReadOnlySet<string> AvailableTypes,
    ResourceRelationshipGraph FilteredGraph);
