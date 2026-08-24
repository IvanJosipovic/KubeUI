using System.Reactive.Linq;
using System.Windows.Input;
using FluentIcons.Common;
using k8s.Models;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Resources;
using KubeUI.Avalonia.Services.Icons;
using KubeUI.Avalonia.Infrastructure.Threading;
using KubeUI.Kubernetes;
using KubernetesClient.Informer.Client;

namespace KubeUI.Avalonia.Shell.Navigation;

internal sealed class NavigationResourceSynchronizer
{
    private readonly IResourceIconService _iconService;
    private readonly ICommand? _openCommand;
    private readonly ICommand? _openInNewTabCommand;
    private readonly ILogger _logger;

    public NavigationResourceSynchronizer(
        IResourceIconService iconService,
        ICommand? openCommand,
        ICommand? openInNewTabCommand,
        ILogger logger)
    {
        _iconService = iconService;
        _openCommand = openCommand;
        _openInNewTabCommand = openInNewTabCommand;
        _logger = logger;
    }

    public void Apply(
        ClusterWorkspace cluster,
        IResourceConfig resourceConfig,
        IReadOnlyCollection<IResourceConfig>? resourceConfigs,
        IReadOnlyDictionary<ClusterWorkspace, ClusterNavigationNode> clusterNodes)
    {
        if (!clusterNodes.TryGetValue(cluster, out var node)
            || cluster.Runtime.Status != ClusterStatus.Connected)
        {
            return;
        }

        if (resourceConfig.Kind == GroupApiVersionKind.From<V1CustomResourceDefinition>() || resourceConfig.IsCustomResource)
        {
            UpdateCustomResourceNavigation(node, cluster, resourceConfig, resourceConfigs);
        }
        else
        {
            UpdateStandardResourceNavigation(node, cluster, resourceConfig);
        }

        if (resourceConfig.Kind == GroupApiVersionKind.From<V1Pod>())
        {
            UpdatePortForwardersNavigation(node);
        }

        AttachResourceCount(cluster, resourceConfig.Kind, clusterNodes);
    }

    public void AttachResourceCount(
        ClusterWorkspace cluster,
        GroupApiVersionKind kind,
        IReadOnlyDictionary<ClusterWorkspace, ClusterNavigationNode> clusterNodes)
    {
        if (!clusterNodes.TryGetValue(cluster, out var node))
        {
            return;
        }

        var link = FindResourceNavigationLink(node.NavigationItems, kind);
        if (link?.ResourceKind is { } resourceKind
            && cluster.Runtime.Objects.TryGetValue(kind, out var container)
            && container is IResourceContainer { IsSeeded: true })
        {
            link.Count ??= CreateResourceCountStream(cluster, resourceKind);
        }
    }

    public void RemoveCustomResourceDefinition(ClusterNavigationNode node, GroupApiVersionKind removedKind)
    {
        RemoveNavigationItem(node.NavigationItems, $"{node.Cluster.Runtime.Name}-{removedKind}");
        RemoveEmptyCategories(node.NavigationItems, node.Cluster);
    }

    private static ResourceNavigationLink? FindResourceNavigationLink(IEnumerable<NavigationItem> items, GroupApiVersionKind kind)
    {
        foreach (var item in items.ToArray())
        {
            if (item is ResourceNavigationLink link && link.ResourceKind == kind)
            {
                return link;
            }

            var nested = FindResourceNavigationLink(item.NavigationItems, kind);
            if (nested != null)
            {
                return nested;
            }
        }

        return null;
    }

    private void UpdateStandardResourceNavigation(ClusterNavigationNode node, ClusterWorkspace cluster, IResourceConfig config)
    {
        var resourceId = $"{cluster.Runtime.Name}-{config.Kind}";
        if (!config.PermissionsLoaded || !config.CanListAndWatch)
        {
            RemoveNavigationItem(node.NavigationItems, resourceId);
            RemoveEmptyCategories(node.NavigationItems, cluster);
            return;
        }

        var target = node.NavigationItems;
        if (!string.IsNullOrWhiteSpace(config.Category))
        {
            target = EnsureNavigationCategory(node.NavigationItems, cluster, config.Category, config.Order).NavigationItems;
        }

        var existingParent = FindNavigationParentCollection(node.NavigationItems, resourceId);
        var existing = existingParent?.FirstOrDefault(item => item.Id == resourceId);
        var desired = CreateResourceNavigationLink(cluster, config);

        if (existing is null)
        {
            target.Add(desired);
        }
        else if (ReferenceEquals(existingParent, target))
        {
            UpdateNavigationItem(existing, desired);
        }
        else
        {
            existingParent!.Remove(existing);
            target.Add(existing);
            UpdateNavigationItem(existing, desired);
            RemoveEmptyCategories(node.NavigationItems, cluster);
        }
    }

    private void UpdateCustomResourceNavigation(
        ClusterNavigationNode node,
        ClusterWorkspace cluster,
        IResourceConfig changedConfig,
        IReadOnlyCollection<IResourceConfig>? resourceConfigs)
    {
        var configs = resourceConfigs ?? cluster.GetResourceConfigs().ToArray();
        var definition = cluster.GetResourceConfig(GroupApiVersionKind.From<V1CustomResourceDefinition>());
        var rootId = $"{cluster.Runtime.Name}-custom-resource-definitions";
        var root = node.NavigationItems.FirstOrDefault(item => item.Id == rootId);

        _logger.LogDebug(
            "Custom resource navigation evaluating for {ClusterName}: Changed={ChangedKind}; DefinitionPermissionsLoaded={DefinitionPermissionsLoaded}; DefinitionCanListAndWatch={DefinitionCanListAndWatch}; RootPresent={RootPresent}; ConfigCount={ConfigCount}",
            cluster.Runtime.Name,
            changedConfig.Kind,
            definition.PermissionsLoaded,
            definition.CanListAndWatch,
            root is not null,
            configs.Count);

        if (definition is not { PermissionsLoaded: true, CanListAndWatch: true })
        {
            if (root != null)
            {
                node.NavigationItems.Remove(root);
            }

            _logger.LogDebug(
                "Custom resource navigation hidden for {ClusterName}: Definition access unavailable; RootRemoved={RootRemoved}",
                cluster.Runtime.Name,
                root is not null);

            return;
        }

        if (root == null)
        {
            root = new NavigationItem
            {
                Id = rootId,
                Name = ResourceCategories.CustomResourceDefinitions,
                Order = ResourceCategories.CustomResourceDefinitionsNavigationOrder,
            };
            node.NavigationItems.Add(root);

            _logger.LogDebug(
                "Custom resource navigation root created for {ClusterName}: RootId={RootId}",
                cluster.Runtime.Name,
                rootId);

            UpdateCustomResourceLink(root, cluster, definition);
            foreach (var config in configs
                         .Where(config => config.IsCustomResource && config.PermissionsLoaded && config.CanListAndWatch)
                         .OrderBy(config => config.Order)
                         .ThenBy(config => config.Name, StringComparer.Ordinal))
            {
                UpdateCustomResourceLink(root, cluster, config);
            }
        }
        else if (changedConfig.Kind == GroupApiVersionKind.From<V1CustomResourceDefinition>())
        {
            UpdateCustomResourceLink(root, cluster, definition);
        }
        else if (changedConfig.IsCustomResource)
        {
            if (changedConfig.PermissionsLoaded && changedConfig.CanListAndWatch)
            {
                UpdateCustomResourceLink(root, cluster, changedConfig);
            }
            else
            {
                RemoveNavigationItem(root.NavigationItems, $"{cluster.Runtime.Name}-{changedConfig.Kind}");
                RemoveEmptyCategories(root.NavigationItems, cluster);
            }
        }

        _logger.LogDebug(
            "Custom resource navigation updated for {ClusterName}: RootPresent={RootPresent}; RootChildren={RootChildren}; TopLevelItems={TopLevelItems}",
            cluster.Runtime.Name,
            node.NavigationItems.Any(item => item.Id == rootId),
            root.NavigationItems.Count,
            node.NavigationItems.Count);
    }

    private void UpdatePortForwardersNavigation(ClusterNavigationNode node)
    {
        var id = $"{node.Cluster.Runtime.Name}-{NavigationTargets.PortForwarders}";
        RemoveNavigationItem(node.NavigationItems, id);

        var podConfig = node.Cluster.GetResourceConfig(GroupApiVersionKind.From<V1Pod>());
        if (podConfig is not { PermissionsLoaded: true, CanListAndWatch: true } || !CanCreatePortForward(node.Cluster))
        {
            RemoveEmptyCategories(node.NavigationItems, node.Cluster);
            return;
        }

        EnsureNavigationCategory(node.NavigationItems, node.Cluster, ResourceCategories.Network, 10)
            .NavigationItems
            .Add(CreateNavigationLink(node.Cluster, NavigationTargets.PortForwarders, Assets.Resources.PortForwarderListView_Title, -450));
    }

    private bool CanCreatePortForward(ClusterWorkspace cluster)
    {
        try
        {
            return cluster.Runtime.Permissions.CanIAnyNamespace(
                GroupApiVersionKind.From<V1Pod>(), true, Verb.Create, "portforward");
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Unable to evaluate port forward permissions for cluster {Cluster}", cluster.Runtime.Name);
            return false;
        }
    }

    private void UpdateCustomResourceLink(NavigationItem root, ClusterWorkspace cluster, IResourceConfig config)
    {
        var resourceId = $"{cluster.Runtime.Name}-{config.Kind}";
        var target = root.NavigationItems;
        if (config.Kind != GroupApiVersionKind.From<V1CustomResourceDefinition>())
        {
            var path = ConstructCustomResourceGroupPath(config.Kind.Group);
            var parts = new List<string>(path.Count);
            foreach (var part in path)
            {
                parts.Add(part);
                var id = $"{cluster.Runtime.Name}-crd-group-{string.Join("/", parts)}";
                var group = target.FirstOrDefault(item => item.Id == id);
                if (group == null)
                {
                    group = new NavigationItem { Id = id, Name = part, Order = 0 };
                    target.Add(group);
                }

                target = group.NavigationItems;
            }
        }

        var existingParent = FindNavigationParentCollection(root.NavigationItems, resourceId);
        var existing = existingParent?.FirstOrDefault(item => item.Id == resourceId);
        var desired = CreateResourceNavigationLink(cluster, config);
        if (config.Kind == GroupApiVersionKind.From<V1CustomResourceDefinition>())
        {
            desired.Name = "Definitions";
            desired.Order = -1;
        }

        if (existing == null)
        {
            target.Add(desired);
        }
        else if (!ReferenceEquals(existingParent, target))
        {
            existingParent!.Remove(existing);
            target.Add(existing);
            UpdateNavigationItem(existing, desired);
            RemoveEmptyCategories(root.NavigationItems, cluster);
        }
        else
        {
            UpdateNavigationItem(existing, desired);
        }
    }

    private ResourceNavigationLink CreateResourceNavigationLink(ClusterWorkspace cluster, IResourceConfig config) => new()
    {
        Cluster = cluster,
        Id = $"{cluster.Runtime.Name}-{config.Kind}",
        Name = config.Name,
        ResourceKind = config.Kind,
        ResourceIcon = _iconService.GetIcon(config.Kind),
        Order = config.Order,
        OpenCommand = _openCommand,
        OpenInNewTabCommand = _openInNewTabCommand,
    };

    private static bool RemoveNavigationItem(ObservableCollection<NavigationItem> items, string id)
    {
        for (var i = items.Count - 1; i >= 0; i--)
        {
            if (items[i].Id == id)
            {
                items.RemoveAt(i);
                return true;
            }

            if (RemoveNavigationItem(items[i].NavigationItems, id))
            {
                return true;
            }
        }

        return false;
    }

    private static void RemoveEmptyCategories(ObservableCollection<NavigationItem> items, ClusterWorkspace cluster)
    {
        for (var i = items.Count - 1; i >= 0; i--)
        {
            var item = items[i];
            RemoveEmptyCategories(item.NavigationItems, cluster);
            if (item.NavigationItems.Count == 0
                && (item.Id.StartsWith($"{cluster.Runtime.Name}-category-", StringComparison.Ordinal)
                    || item.Id.StartsWith($"{cluster.Runtime.Name}-crd-group-", StringComparison.Ordinal)))
            {
                items.RemoveAt(i);
            }
        }
    }

    private static ObservableCollection<NavigationItem>? FindNavigationParentCollection(ObservableCollection<NavigationItem> items, string id)
    {
        if (items.Any(item => item.Id == id))
        {
            return items;
        }

        foreach (var item in items)
        {
            var parent = FindNavigationParentCollection(item.NavigationItems, id);
            if (parent != null)
            {
                return parent;
            }
        }

        return null;
    }

    private static void UpdateNavigationItem(NavigationItem current, NavigationItem desired)
    {
        current.Name = desired.Name;
        current.Order = desired.Order;
        current.SvgIcon = desired.SvgIcon;
        current.StyleIcon = desired.StyleIcon;
        current.FluentIcon = desired.FluentIcon;
        if (current is ResourceNavigationLink currentResource && desired is ResourceNavigationLink desiredResource)
        {
            currentResource.Cluster = desiredResource.Cluster;
            currentResource.ResourceKind = desiredResource.ResourceKind;
            currentResource.OpenCommand = desiredResource.OpenCommand;
            currentResource.OpenInNewTabCommand = desiredResource.OpenInNewTabCommand;
            currentResource.ResourceIcon = desiredResource.ResourceIcon;
        }
    }

    private static NavigationItem EnsureNavigationCategory(ObservableCollection<NavigationItem> items, ClusterWorkspace cluster, string name, int order)
    {
        var id = $"{cluster.Runtime.Name}-category-{name}";
        var existing = items.FirstOrDefault(item => item.Id == id);
        if (existing != null)
        {
            return existing;
        }

        var category = new NavigationItem { Id = id, Name = name, Order = ResourceCategories.GetOrder(name, order) };
        items.Add(category);
        return category;
    }

    private static IReadOnlyList<string> ConstructCustomResourceGroupPath(string? group)
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
        for (var i = levels.Length - 3; i >= 0; i--)
        {
            path.Add(string.Join('.', levels, i, levels.Length - i));
        }

        return path;
    }

    private static IObservable<int> CreateResourceCountStream(ClusterWorkspace cluster, GroupApiVersionKind kind) =>
        cluster.Runtime.GetResourceCount(kind).Sample(TimeSpan.FromMilliseconds(100), AvaloniaScheduler.Instance);

    private static NavigationLink CreateNavigationLink(ClusterWorkspace cluster, string id, string name, int order = 0) => new()
    {
        Cluster = cluster,
        Id = $"{cluster.Runtime.Name}-{id}",
        Name = name,
        ViewModelKey = id,
        Order = order,
        SvgIcon = null,
        FluentIcon = id switch
        {
            NavigationTargets.ClusterWorkspace => Icon.Desktop,
            NavigationTargets.Visualization => Icon.DataUsage,
            NavigationTargets.ClusterSettings => Icon.Settings,
            NavigationTargets.PortForwarders => Icon.CloudFlow,
            NavigationTargets.LoadYaml => Icon.ArrowUpload,
            NavigationTargets.LoadFolder => Icon.FolderAdd,
            _ => null,
        }
    };
}
