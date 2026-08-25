using KubernetesClient.Informer.Client;

namespace KubeUI.Kubernetes.Resources.Relationships;

public readonly record struct ResourceSeedPrerequisite
{
    public ResourceSeedPrerequisite(
        GroupApiVersionKind kind,
        bool allowServedVersionFallback = false,
        bool matchAnyApiGroup = false)
    {
        Kind = kind;
        AllowServedVersionFallback = allowServedVersionFallback;
        MatchAnyApiGroup = matchAnyApiGroup;
    }

    public GroupApiVersionKind Kind { get; }

    public bool AllowServedVersionFallback { get; }

    public bool MatchAnyApiGroup { get; }
}
