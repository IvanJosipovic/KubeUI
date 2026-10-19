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
public sealed record NavigationSection(string Id, string Name, int Order, Icon? Icon);

/// <summary>Creates and updates shared navigation nodes used by the navigation tree.</summary>
internal static class NavigationTreeEditor
{
    public static NavigationItem EnsureNode(
        ObservableCollection<NavigationItem> items,
        string id,
        string name,
        int order,
        Icon? icon = null)
    {
        var existing = items.FirstOrDefault(item => string.Equals(item.Id, id, StringComparison.Ordinal));
        if (existing is not null)
        {
            existing.Name = name;
            existing.Order = order;
            existing.FluentIcon = icon;
            return existing;
        }

        var created = new NavigationItem { Id = id, Name = name, Order = order, FluentIcon = icon };
        items.Add(created);
        return created;
    }

    public static ObservableCollection<NavigationItem> EnsurePath(
        ObservableCollection<NavigationItem> items,
        string idPrefix,
        IReadOnlyList<NavigationGroup> path,
        string firstSegmentSeparator = "/")
    {
        var target = items;
        var key = idPrefix;
        for (var index = 0; index < path.Count; index++)
        {
            var part = path[index];
            key += (index == 0 ? firstSegmentSeparator : "/") + part.Id;
            target = EnsureNode(target, key, part.Name, part.Order, part.Icon).NavigationItems;
        }

        return target;
    }

    public static bool Remove(ObservableCollection<NavigationItem> items, string id)
    {
        for (var index = items.Count - 1; index >= 0; index--)
        {
            if (string.Equals(items[index].Id, id, StringComparison.Ordinal))
            {
                items.RemoveAt(index);
                return true;
            }

            if (Remove(items[index].NavigationItems, id))
            {
                return true;
            }
        }

        return false;
    }

    public static void RemoveEmptyGroups(ObservableCollection<NavigationItem> items, Func<NavigationItem, bool> isGeneratedGroup)
    {
        for (var index = items.Count - 1; index >= 0; index--)
        {
            var item = items[index];
            RemoveEmptyGroups(item.NavigationItems, isGeneratedGroup);
            if (item.NavigationItems.Count == 0 && isGeneratedGroup(item))
            {
                items.RemoveAt(index);
            }
        }
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
