using KubeUI.Kubernetes.Resources.Relationships;
using KubernetesClient.Informer.Client;

namespace KubeUI.Avalonia.Features.Resources.Visualization;

/// <summary>Immutable result published by visualization preparation stages.</summary>
internal sealed record VisualizationPipelineState(
    ResourceRelationshipGraph CompleteGraph,
    IReadOnlySet<UnresolvedResourceReference> PendingReferences,
    IReadOnlySet<GroupApiVersionKind> RequiredSeedKinds,
    IReadOnlySet<string> AvailableTypes,
    ResourceRelationshipGraph FilteredGraph);
