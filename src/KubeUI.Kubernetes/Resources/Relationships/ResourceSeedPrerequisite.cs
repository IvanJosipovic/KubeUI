using KubernetesClient.Informer.Client;

namespace KubeUI.Kubernetes.Resources.Relationships;

public readonly record struct ResourceSeedPrerequisite
{
    public ResourceSeedPrerequisite(GroupApiVersionKind kind, bool allowServedVersionFallback = false)
    {
        Kind = kind;
        AllowServedVersionFallback = allowServedVersionFallback;
    }

    public GroupApiVersionKind Kind { get; }

    public bool AllowServedVersionFallback { get; }
}
