using KubernetesClient.Informer.Client;

namespace KubeUI.Kubernetes.Resources.Relationships;

public readonly record struct ResourceSeedPrerequisite
{
    public ResourceSeedPrerequisite(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        Type = type;
        Kind = null;
        AllowServedVersionFallback = false;
    }

    public ResourceSeedPrerequisite(GroupApiVersionKind kind, bool allowServedVersionFallback = false)
    {
        Type = null;
        Kind = kind;
        AllowServedVersionFallback = allowServedVersionFallback;
    }

    public Type? Type { get; }

    public GroupApiVersionKind? Kind { get; }

    public bool AllowServedVersionFallback { get; }
}
