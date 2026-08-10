namespace KubeUI.Kubernetes.Resources.Relationships;

public sealed record ResourceIdentity(
    string ApiVersion,
    string Kind,
    string? Namespace,
    string Name,
    string? Uid);
