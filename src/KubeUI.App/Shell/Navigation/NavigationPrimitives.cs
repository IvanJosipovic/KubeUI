using FluentIcons.Common;
using KubeUI.Features.Resources;

namespace KubeUI.App.Shell.Navigation;

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
            target = EnsureNode(target, key, part.Name, part.Order, part.Icon, part.SvgIcon, part.SvgIconDark).NavigationItems;
        }

        return target;
    }

    /// <summary>Builds the CRD group hierarchy for a custom resource group.</summary>
    public static IReadOnlyList<NavigationGroup> ConstructCustomResourceGroups(string? group)
        => NavigationGroupPath.CreateCustomResourceGroups(group);

    public static IReadOnlyList<string> ConstructCustomResourceGroupPath(string? group)
        => NavigationGroupPath.CreateCustomResourceGroupPath(group);
}
