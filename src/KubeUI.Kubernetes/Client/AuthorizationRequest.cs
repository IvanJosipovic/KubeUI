using KubernetesClient.Informer.Client;

namespace KubeUI.Kubernetes;

public readonly record struct AuthorizationRequest(GroupApiVersionKind ResourceKind, Verb Verb, string? Subresource);
