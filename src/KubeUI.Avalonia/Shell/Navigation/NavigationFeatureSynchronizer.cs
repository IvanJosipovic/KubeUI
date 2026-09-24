using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Resources;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Shell.Navigation;

internal sealed class NavigationFeatureSynchronizer
{
    private readonly NavigationFeatureCatalog _catalog;

    public NavigationFeatureSynchronizer(NavigationFeatureCatalog catalog)
    {
        _catalog = catalog;
    }

    public void Update(ClusterWorkspace cluster, ClusterNavigationNode node)
    {
        foreach (var definition in _catalog.Definitions)
        {
            var featureId = GetFeatureId(cluster, definition);
            var existing = Find(node.NavigationItems, featureId);
            if (!cluster.Runtime.Connected
                || cluster.Runtime.Status != ClusterStatus.Connected
                || !definition.IsAvailable(new NavigationFeatureContext(cluster)))
            {
                Remove(node.NavigationItems, featureId);
                RemoveEmptyFeatureGroups(node.NavigationItems, cluster);
                continue;
            }

            var target = EnsurePath(node, cluster, definition);
            var desired = new NavigationLink
            {
                Cluster = cluster,
                Id = featureId,
                Name = definition.Name,
                ViewModelKey = definition.Id,
                Order = definition.Order,
                FluentIcon = definition.Icon
            };

            if (existing is null)
            {
                target.Add(desired);
            }
            else if (!ReferenceEquals(FindParent(node.NavigationItems, featureId), target))
            {
                Remove(node.NavigationItems, featureId);
                target.Add(desired);
            }
            else
            {
                existing.Name = desired.Name;
                existing.Order = desired.Order;
                existing.FluentIcon = desired.FluentIcon;
                if (existing is NavigationLink existingLink)
                {
                    existingLink.ViewModelKey = desired.ViewModelKey;
                }
                if (existing is NavigationLink existingLink2)
                {
                    existingLink2.Cluster = desired.Cluster;
                }
            }
        }
    }

    private static ObservableCollection<NavigationItem> EnsurePath(
        ClusterNavigationNode node,
        ClusterWorkspace cluster,
        INavigationFeatureDefinition definition)
    {
        var target = node.NavigationItems;
        if (definition.Placement == NavigationFeaturePlacement.CustomResourceDefinitions)
        {
            target = EnsureNode(target, $"{cluster.Runtime.Name}-custom-resource-definitions", ResourceCategories.CustomResourceDefinitions, ResourceCategories.CustomResourceDefinitionsNavigationOrder).NavigationItems;
        }

        var parts = definition.Path;
        var prefix = definition.Placement == NavigationFeaturePlacement.CustomResourceDefinitions
            ? $"{cluster.Runtime.Name}-crd-group-"
            : $"{cluster.Runtime.Name}-feature-group-";
        var groupOrder = definition.Placement == NavigationFeaturePlacement.CustomResourceDefinitions
            ? 0
            : ResourceCategories.CustomResourceDefinitionsNavigationOrder + 1;
        var path = string.Empty;
        foreach (var part in parts)
        {
            path = string.IsNullOrEmpty(path) ? part : $"{path}/{part}";
            target = EnsureNode(target, prefix + path, part, groupOrder).NavigationItems;
        }

        return target;
    }

    private static NavigationItem EnsureNode(ObservableCollection<NavigationItem> items, string id, string name, int order)
    {
        var existing = items.FirstOrDefault(item => item.Id == id);
        if (existing is not null)
        {
            return existing;
        }

        var created = new NavigationItem { Id = id, Name = name, Order = order };
        items.Add(created);
        return created;
    }

    private static NavigationItem? Find(IEnumerable<NavigationItem> items, string id)
    {
        foreach (var item in items)
        {
            if (item.Id == id)
            {
                return item;
            }

            var nested = Find(item.NavigationItems, id);
            if (nested is not null)
            {
                return nested;
            }
        }

        return null;
    }

    private static ObservableCollection<NavigationItem>? FindParent(ObservableCollection<NavigationItem> items, string id)
    {
        if (items.Any(item => item.Id == id))
        {
            return items;
        }

        foreach (var item in items)
        {
            var nested = FindParent(item.NavigationItems, id);
            if (nested is not null)
            {
                return nested;
            }
        }

        return null;
    }

    private static bool Remove(ObservableCollection<NavigationItem> items, string id)
    {
        for (var index = items.Count - 1; index >= 0; index--)
        {
            if (items[index].Id == id)
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

    private static void RemoveEmptyFeatureGroups(ObservableCollection<NavigationItem> items, ClusterWorkspace cluster)
    {
        for (var index = items.Count - 1; index >= 0; index--)
        {
            var item = items[index];
            RemoveEmptyFeatureGroups(item.NavigationItems, cluster);
            if (item.NavigationItems.Count == 0
                && (item.Id.StartsWith($"{cluster.Runtime.Name}-crd-group-", StringComparison.Ordinal)
                    || item.Id.StartsWith($"{cluster.Runtime.Name}-feature-group-", StringComparison.Ordinal)))
            {
                items.RemoveAt(index);
            }
        }
    }

    private static string GetFeatureId(ClusterWorkspace cluster, INavigationFeatureDefinition definition) =>
        $"{cluster.Runtime.Name}-feature-{definition.Id}";
}
