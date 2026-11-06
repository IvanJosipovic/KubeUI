using Dock.Model.Core;
using FluentIcons.Common;
using KubeUI.Features.Clusters.Workspace;

namespace KubeUI.Features.Resources;

/// <summary>One stable group in a navigation path.</summary>
/// <param name="Id">Stable group identifier within its section.</param>
/// <param name="Name">Localized display name.</param>
/// <param name="Order">Sort position relative to sibling items.</param>
/// <param name="Icon">Optional group icon.</param>
/// <param name="SvgIcon">Optional light-theme SVG asset path.</param>
/// <param name="SvgIconDark">Optional dark-theme SVG asset path; <paramref name="SvgIcon"/> is the fallback.</param>
public sealed record NavigationGroup(string Id, string Name, int Order, Icon? Icon, string? SvgIcon = null, string? SvgIconDark = null);

/// <summary>Stable identity and display metadata for a top-level navigation section.</summary>
/// <param name="Id">Stable section identifier, unique among registered sections.</param>
/// <param name="Name">Localized display name.</param>
/// <param name="Order">Sort position relative to other navigation items.</param>
/// <param name="Icon">Optional section icon.</param>
/// <param name="SvgIcon">Optional asset path to an SVG icon; when set it is rendered instead of <paramref name="Icon"/>.</param>
/// <param name="SvgIconDark">Optional dark-theme SVG asset path; <paramref name="SvgIcon"/> is used as the fallback.</param>
public sealed record NavigationSection(string Id, string Name, int Order, Icon? Icon, string? SvgIcon = null, string? SvgIconDark = null);

/// <summary>Defines a custom document that can be shown under a navigation section.</summary>
/// <param name="Id">Stable document identifier, unique across sections.</param>
/// <param name="Name">Localized display name.</param>
/// <param name="Order">Sort position within its section.</param>
/// <param name="Icon">Optional icon.</param>
/// <param name="IsAvailable">Returns whether the document is available for a cluster.</param>
/// <param name="CreateDockable">Creates and initializes the document for a cluster.</param>
public sealed record NavigationDocument(
    string Id,
    string Name,
    int Order,
    Icon? Icon,
    Func<ClusterWorkspace, bool> IsAvailable,
    Func<IServiceProvider, ClusterWorkspace, IDockable> CreateDockable);

/// <summary>Creates navigation groups from Kubernetes API group names.</summary>
public static class NavigationGroupPath
{
    /// <summary>Builds the CRD group hierarchy for a custom resource group.</summary>
    /// <param name="group">Kubernetes API group, or null for core resources.</param>
    /// <returns>Groups from the navigation root to the specific API group.</returns>
    public static IReadOnlyList<NavigationGroup> CreateCustomResourceGroups(string? group)
        => CreateCustomResourceGroupPath(group)
            .Select(static name => new NavigationGroup(name, name, 0, null))
            .ToArray();

    /// <summary>Builds the path labels for a Kubernetes API group.</summary>
    /// <param name="group">Kubernetes API group, or null for core resources.</param>
    /// <returns>Path labels from the navigation root to the specific API group.</returns>
    public static IReadOnlyList<string> CreateCustomResourceGroupPath(string? group)
    {
        if (string.IsNullOrWhiteSpace(group))
        {
            return ["core"];
        }

        var levels = group.Split('.', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        if (levels.Length <= 2)
        {
            return [group];
        }

        var path = new List<string>(levels.Length - 1) { $"{levels[^2]}.{levels[^1]}" };
        for (var index = levels.Length - 3; index >= 0; index--)
        {
            path.Add(string.Join('.', levels, index, levels.Length - index));
        }

        return path;
    }
}

/// <summary>Defines ordering relative to built-in navigation sections.</summary>
public static class NavigationSectionOrder
{
    /// <summary>Gets the first section order after the built-in custom resources section.</summary>
    public static int AfterCustomResources => NavigationSections.AfterCustomResourcesOrder;
}

/// <summary>Stable ids of built-in sections available to feature navigation providers.</summary>
public static class NavigationSectionIds
{
    /// <summary>Network resources section id.</summary>
    public const string Network = "network";
}
