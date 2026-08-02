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

    public IBrush Brush => Relationship.Kind switch
    {
        ResourceRelationshipKind.Owner => Brushes.DodgerBlue,
        ResourceRelationshipKind.Reference => Brushes.LightGray,
        ResourceRelationshipKind.Selector => Brushes.MediumPurple,
        ResourceRelationshipKind.Label => Brushes.Orange,
        ResourceRelationshipKind.Storage => Brushes.Teal,
        ResourceRelationshipKind.Identity => Brushes.ForestGreen,
        ResourceRelationshipKind.Rbac => Brushes.Crimson,
        ResourceRelationshipKind.Event => Brushes.Goldenrod,
        ResourceRelationshipKind.GitOps => Brushes.HotPink,
        _ => Brushes.LightGray,
    };

    public override string ToString()
        => string.IsNullOrWhiteSpace(Relationship.Label)
            ? RelationshipName
            : $"{RelationshipName}: {Relationship.Label}";
}
