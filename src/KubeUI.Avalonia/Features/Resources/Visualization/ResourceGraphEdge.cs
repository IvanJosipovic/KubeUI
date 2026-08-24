using KubeUI.Kubernetes.Resources.Relationships;
using Westermo.GraphX.Common.Models;

namespace KubeUI.Avalonia.Features.Resources.Visualization;

public sealed class ResourceGraphEdge : EdgeBase<ResourceGraphVertex>
{
    public ResourceGraphEdge(ResourceGraphVertex source, ResourceGraphVertex target, ResourceRelationship relationship)
        : base(source, target)
    {
        Relationship = relationship;

        // SkipProcessing = relationship.Kind == ResourceRelationshipKind.Owner
        //     ? ProcessingOptionEnum.Default
        //     : ProcessingOptionEnum.Exclude;
    }

    public ResourceRelationship Relationship { get; }

    public string RelationshipName => Relationship.Kind.ToString();

    internal string ThemeClass => Relationship.Kind switch
    {
        ResourceRelationshipKind.Owner => "RelationshipOwner",
        ResourceRelationshipKind.Reference => "RelationshipReference",
        ResourceRelationshipKind.Selector => "RelationshipSelector",
        ResourceRelationshipKind.Label => "RelationshipLabel",
        ResourceRelationshipKind.Storage => "RelationshipStorage",
        ResourceRelationshipKind.Identity => "RelationshipIdentity",
        ResourceRelationshipKind.Rbac => "RelationshipRbac",
        ResourceRelationshipKind.Event => "RelationshipEvent",
        ResourceRelationshipKind.GitOps => "RelationshipGitOps",
        _ => "RelationshipDefault",
    };

    public override string ToString()
        => string.IsNullOrWhiteSpace(Relationship.Label)
            ? RelationshipName
            : $"{RelationshipName}: {Relationship.Label}";
}
