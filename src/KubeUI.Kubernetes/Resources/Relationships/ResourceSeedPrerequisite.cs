using KubernetesClient.Informer.Client;

namespace KubeUI.Kubernetes.Resources.Relationships;

public readonly record struct ResourceSeedPrerequisite
{
    /// <summary>Creates a seed prerequisite for a resource kind.</summary>
    /// <param name="kind">Resource kind that must be seeded.</param>
    /// <param name="allowServedVersionFallback">Whether another served version may satisfy the prerequisite.</param>
    /// <param name="matchAnyApiGroup">Whether the prerequisite kind may match any API group.</param>
    public ResourceSeedPrerequisite(
        GroupApiVersionKind kind,
        bool allowServedVersionFallback = false,
        bool matchAnyApiGroup = false)
    {
        Kind = kind;
        AllowServedVersionFallback = allowServedVersionFallback;
        MatchAnyApiGroup = matchAnyApiGroup;
    }

    /// <summary>Resource kind that must be seeded.</summary>
    public GroupApiVersionKind Kind { get; }

    /// <summary>Gets whether another served version may satisfy the prerequisite.</summary>
    public bool AllowServedVersionFallback { get; }

    /// <summary>Gets whether the prerequisite kind may match any API group.</summary>
    public bool MatchAnyApiGroup { get; }
}
