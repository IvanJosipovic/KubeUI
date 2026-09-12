namespace KubeUI.Kubernetes.Resources.Relationships;

public sealed record ResourceRelationship(
    ResourceIdentity Source,
    ResourceIdentity Target,
    ResourceRelationshipKind Kind,
    string? Label = null);
