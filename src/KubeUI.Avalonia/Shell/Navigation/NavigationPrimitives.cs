using FluentIcons.Common;

namespace KubeUI.Avalonia.Shell.Navigation;

/// <summary>One stable group in a navigation path.</summary>
/// <param name="Id">Stable group identifier within its section.</param>
/// <param name="Name">Localized display name.</param>
/// <param name="Order">Sort position relative to sibling items.</param>
/// <param name="Icon">Optional group icon.</param>
public sealed record NavigationGroup(string Id, string Name, int Order, Icon? Icon);

/// <summary>Stable identity and display metadata for a top-level navigation section.</summary>
/// <param name="Id">Stable section identifier, unique among registered sections.</param>
/// <param name="Name">Localized display name.</param>
/// <param name="Order">Sort position relative to other navigation items.</param>
/// <param name="Icon">Optional section icon.</param>
/// <param name="SvgIcon">Optional asset path to an SVG icon; when set it is rendered instead of <paramref name="Icon"/>.</param>
/// <param name="SvgIconDark">Optional dark-theme SVG asset path; <paramref name="SvgIcon"/> is used as the fallback.</param>
public sealed record NavigationSection(string Id, string Name, int Order, Icon? Icon, string? SvgIcon = null, string? SvgIconDark = null);

/// <summary>Creates and updates the shared navigation nodes used by the navigation tree.</summary>
internal static class NavigationTreeEditor
{
    public static NavigationItem EnsureNode(
        ObservableCollection<NavigationItem> items,
        string id,
        string name,
        int order,
        Icon? icon = null,
        string? svgIcon = null,
        string? svgIconDark = null)
    {
        var existing = items.FirstOrDefault(item => string.Equals(item.Id, id, StringComparison.Ordinal));
        if (existing is not null)
        {
            existing.Name = name;
            existing.Order = order;
            ApplyIcon(existing, icon, svgIcon, svgIconDark);
            return existing;
        }

        var created = new NavigationItem { Id = id, Name = name, Order = order };
        ApplyIcon(created, icon, svgIcon, svgIconDark);
        items.Add(created);
        return created;
    }

    /// <summary>Applies the section icon to a node; the SVG icon wins so a node never renders two icons.</summary>
    private static void ApplyIcon(NavigationItem item, Icon? icon, string? svgIcon, string? svgIconDark)
    {
        item.FluentIcon = svgIcon is null ? icon : null;
        item.SvgIcon = svgIcon;
        item.SvgIconDark = svgIcon is null ? null : svgIconDark;
    }

    public static ObservableCollection<NavigationItem> EnsurePath(
        ObservableCollection<NavigationItem> items,
        string idPrefix,
        IReadOnlyList<NavigationGroup> path)
    {
        var target = items;
        var key = idPrefix;
        for (var index = 0; index < path.Count; index++)
        {
            var part = path[index];
            key += "/" + part.Id;
            target = EnsureNode(target, key, part.Name, part.Order, part.Icon).NavigationItems;
        }

        return target;
    }

    /// <summary>Builds the CRD group hierarchy for a custom resource group.</summary>
    public static IReadOnlyList<NavigationGroup> ConstructCustomResourceGroups(string? group)
    {
        return ConstructCustomResourceGroupPath(group)
            .Select(static name => new NavigationGroup(name, name, 0, null))
            .ToArray();
    }

    public static IReadOnlyList<string> ConstructCustomResourceGroupPath(string? group)
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
