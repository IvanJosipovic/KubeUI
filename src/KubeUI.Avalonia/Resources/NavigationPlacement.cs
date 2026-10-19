using KubeUI.Avalonia.Shell.Navigation;

namespace KubeUI.Avalonia.Resources;

/// <summary>One canonical or alias location in the navigation tree.</summary>
/// <param name="SectionId">Stable owning section id, or null for the cluster root.</param>
/// <param name="Groups">Ordered groups from the section root outward.</param>
/// <param name="Name">Optional display name override.</param>
/// <param name="Order">Optional sibling order override.</param>
public sealed record NavigationPlacement(
    string? SectionId,
    IReadOnlyList<NavigationGroup> Groups,
    string? Name = null,
    int? Order = null)
{
    /// <summary>Places an entry at cluster root.</summary>
    public static NavigationPlacement Root { get; } = new(null, []);

    /// <summary>Places an entry in a section, optionally inside groups.</summary>
    /// <param name="sectionId">Stable section identifier.</param>
    /// <param name="groups">Optional group path.</param>
    /// <param name="name">Optional display name override.</param>
    /// <param name="order">Optional sibling order override.</param>
    public static NavigationPlacement InSection(
        string sectionId,
        IReadOnlyList<NavigationGroup>? groups = null,
        string? name = null,
        int? order = null)
        => new(sectionId, groups ?? [], name, order);
}
