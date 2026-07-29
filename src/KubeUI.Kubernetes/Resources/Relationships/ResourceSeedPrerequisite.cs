using KubernetesClient.Informer.Client;

namespace KubeUI.Kubernetes.Resources.Relationships;

public readonly record struct ResourceSeedPrerequisite
{
    public ResourceSeedPrerequisite(Type type)
    {
        ArgumentNullException.ThrowIfNull(type);
        Type = type;
        Kind = null;
    }

    public ResourceSeedPrerequisite(GroupApiVersionKind kind)
    {
        Type = null;
        Kind = kind;
    }

    public Type? Type { get; }

    public GroupApiVersionKind? Kind { get; }
}
