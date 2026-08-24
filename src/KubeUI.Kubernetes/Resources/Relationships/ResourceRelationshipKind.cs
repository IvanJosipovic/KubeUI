namespace KubeUI.Kubernetes.Resources.Relationships;

public enum ResourceRelationshipKind
{
    Owner,
    Reference,
    Selector,
    Label,
    Storage,
    Identity,
    Rbac,
    Event,
    GitOps
}
