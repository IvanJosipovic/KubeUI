using KubernetesClient.Informer.Client;

namespace KubeUI.Kubernetes;

/// <summary>Describes one authorization request for a Kubernetes resource.</summary>
/// <param name="ResourceKind">Resource group, version, kind, and plural.</param>
/// <param name="Verb">Requested Kubernetes authorization verb.</param>
/// <param name="Subresource">Optional subresource; <see langword="null"/> means the main resource.</param>
public readonly record struct AuthorizationRequest(GroupApiVersionKind ResourceKind, Verb Verb, string? Subresource);
