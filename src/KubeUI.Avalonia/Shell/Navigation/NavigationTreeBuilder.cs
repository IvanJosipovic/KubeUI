using System.Reactive.Linq;
using System.Windows.Input;
using FluentIcons.Common;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Infrastructure.Threading;
using KubeUI.Avalonia.Resources;
using KubeUI.Avalonia.Services.Icons;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Shell.Navigation;

/// <summary>Computes and applies the navigation links for a connected cluster in one pass.</summary>
internal sealed class NavigationTreeBuilder
{
    private readonly NavigationSectionCatalog _sections;
    private readonly IResourceIconService _icons;
    private readonly ICommand? _openCommand;
    private readonly ICommand? _openInNewTabCommand;
    private readonly ILogger _logger;
    private readonly HashSet<string> _desiredIds = new(StringComparer.Ordinal);
    private readonly Dictionary<string, ItemLocation> _existing = new(StringComparer.Ordinal);

    public NavigationTreeBuilder(
        NavigationSectionCatalog sections,
        IResourceIconService icons,
        ICommand? openCommand,
        ICommand? openInNewTabCommand,
        ILogger logger)
    {
        _sections = sections;
        _icons = icons;
        _openCommand = openCommand;
        _openInNewTabCommand = openInNewTabCommand;
        _logger = logger;
    }

    /// <summary>Applies the current workspace state to a cluster node while preserving link identity.</summary>
    public void Reconcile(ClusterNavigationNode node)
    {
        var cluster = node.Cluster;
        _desiredIds.Clear();
        _existing.Clear();
        Index(node.NavigationItems, node.NavigationItems);

        foreach (var target in NavigationTargets.FixedClusterLinks)
        {
            _desiredIds.Add(LinkId(cluster, target.Key));
        }

        if (cluster.Runtime.Status == ClusterStatus.Connected)
        {
            AddResourceLinks(node, cluster);
            AddDocuments(node, cluster);
            AddPortForwarders(node, cluster);
        }

        Prune(node.NavigationItems, cluster.Runtime.Name);
    }

    /// <summary>Attaches count streams for a seeded resource kind to matching links.</summary>
    public void AttachResourceCount(ClusterNavigationNode node, GroupApiVersionKind kind)
    {
        foreach (var link in EnumerateLinks(node.NavigationItems))
        {
            if (link.ResourceKind == kind)
            {
                AttachCount(link, node.Cluster, kind);
            }
        }
    }

    private void AddResourceLinks(ClusterNavigationNode node, ClusterWorkspace cluster)
    {
        var canListDefinitions = CanListCustomResourceDefinitions(cluster);
        foreach (var resource in cluster.GetResourceConfigs())
        {
            if (!resource.PermissionsLoaded || !resource.CanListAndWatch)
            {
                continue;
            }

            if (resource.IsCustomResource() && !canListDefinitions)
            {
                continue;
            }

            AddResource(node, cluster, resource, resource.Navigation, canonical: true);

            if (resource.IsCustomResource())
            {
                var customResources = NavigationPlacement.InSection(
                    NavigationSections.CustomResourcesId,
                    NavigationTreeEditor.ConstructCustomResourceGroups(resource.Kind.Group));
                if (!SameLocation(customResources, resource.Navigation))
                {
                    AddResource(node, cluster, resource, customResources, canonical: false);
                }
            }

            foreach (var provider in _sections.Providers)
            {
                if (provider.Place(resource) is { } alias && !SameLocation(alias, resource.Navigation))
                {
                    AddResource(node, cluster, resource, alias, canonical: false);
                }
            }
        }
    }

    private void AddResource(
        ClusterNavigationNode node,
        ClusterWorkspace cluster,
        IResourceConfig resource,
        NavigationPlacement placement,
        bool canonical)
    {
        var id = canonical
            ? LinkId(cluster, resource.Kind.ToString()!)
            : $"{cluster.Runtime.Name}-section-resource-{placement.SectionId ?? "root"}-{resource.Kind.Group}/{resource.Kind.Kind}";
        var target = ResolveTarget(node, cluster, placement);
        _desiredIds.Add(id);
        Upsert(
            target,
            cluster,
            id,
            placement.Name ?? resource.Name,
            placement.Order ?? resource.Order,
            icon: null,
            resource,
            documentId: null,
            viewModelKey: null);
    }

    private void AddDocuments(ClusterNavigationNode node, ClusterWorkspace cluster)
    {
        foreach (var provider in _sections.Providers)
        {
            foreach (var document in provider.Documents)
            {
                if (!document.IsAvailable(cluster))
                {
                    continue;
                }

                var id = $"{cluster.Runtime.Name}-section-document-{provider.Section.Id}-{document.Id}";
                var target = ResolveTarget(node, cluster, NavigationPlacement.InSection(provider.Section.Id));
                _desiredIds.Add(id);
                Upsert(
                    target,
                    cluster,
                    id,
                    document.Name,
                    document.Order,
                    document.Icon,
                    resource: null,
                    document.Id,
                    viewModelKey: null);
            }
        }
    }

    private void AddPortForwarders(ClusterNavigationNode node, ClusterWorkspace cluster)
    {
        var podKind = GroupApiVersionKind.From<V1Pod>();
        var podConfig = cluster.GetResourceConfigs().FirstOrDefault(config => config.Kind == podKind);
        if (podConfig is not { PermissionsLoaded: true, CanListAndWatch: true } || !CanCreatePortForward(cluster, podKind))
        {
            return;
        }

        var id = LinkId(cluster, NavigationTargets.PortForwarders);
        var target = ResolveTarget(node, cluster, NavigationPlacement.InSection(NavigationSections.NetworkId));
        _desiredIds.Add(id);
        Upsert(
            target,
            cluster,
            id,
            Assets.Resources.PortForwarderListView_Title!,
            -450,
            Icon.CloudFlow,
            resource: null,
            documentId: null,
            NavigationTargets.PortForwarders);
    }

    private ObservableCollection<NavigationItem> ResolveTarget(
        ClusterNavigationNode node,
        ClusterWorkspace cluster,
        NavigationPlacement placement)
    {
        if (placement.SectionId is not { } sectionId)
        {
            return node.NavigationItems;
        }

        if (_sections.Find(sectionId) is not { } section)
        {
            throw new InvalidOperationException($"Navigation placement targets unknown section '{sectionId}'.");
        }

        var nodeId = $"{cluster.Runtime.Name}-section-{section.Id}";
        _desiredIds.Add(nodeId);
        var root = NavigationTreeEditor.EnsureNode(node.NavigationItems, nodeId, section.Name, section.Order, section.Icon);
        if (placement.Groups.Count == 0)
        {
            return root.NavigationItems;
        }

        var prefix = $"{cluster.Runtime.Name}-section-group-{section.Id}";
        var target = NavigationTreeEditor.EnsurePath(root.NavigationItems, prefix, placement.Groups);
        foreach (var group in placement.Groups)
        {
            prefix += "/" + group.Id;
            _desiredIds.Add(prefix);
        }

        return target;
    }

    private void Upsert(
        ObservableCollection<NavigationItem> target,
        ClusterWorkspace cluster,
        string id,
        string name,
        int order,
        Icon? icon,
        IResourceConfig? resource,
        string? documentId,
        string? viewModelKey)
    {
        if (!_existing.TryGetValue(id, out var location) || location.Item is not NavigationLink link)
        {
            link = new NavigationLink { Id = id };
            target.Add(link);
            location = new ItemLocation(link, target);
            _existing[id] = location;
        }
        else if (!ReferenceEquals(location.Parent, target))
        {
            location.Parent.Remove(link);
            target.Add(link);
            _existing[id] = new ItemLocation(link, target);
        }

        link.Name = name;
        link.Order = order;
        link.Cluster = cluster;
        link.FluentIcon = icon;
        link.ViewModelKey = viewModelKey;
        link.DocumentId = documentId;
        link.ResourceKind = resource?.Kind;
        link.ResourceIcon = resource is null ? null : _icons.GetIcon(resource.Kind);
        link.OpenCommand = resource is null ? null : _openCommand;
        link.OpenInNewTabCommand = resource is null ? null : _openInNewTabCommand;
        if (resource is not null)
        {
            AttachCount(link, cluster, resource.Kind);
        }
    }

    private static void AttachCount(NavigationLink link, ClusterWorkspace cluster, GroupApiVersionKind kind)
    {
        if (link.Count is null
            && cluster.Runtime.Objects.TryGetValue(kind, out var container)
            && container is IResourceContainer { IsSeeded: true })
        {
            link.Count = cluster.Runtime.GetResourceCount(kind)
                .Sample(TimeSpan.FromMilliseconds(100), AvaloniaScheduler.Instance);
        }
    }

    private void Prune(ObservableCollection<NavigationItem> items, string clusterName)
    {
        for (var index = items.Count - 1; index >= 0; index--)
        {
            var item = items[index];
            if (item is NavigationLink link)
            {
                if (!_desiredIds.Contains(link.Id))
                {
                    items.RemoveAt(index);
                }

                continue;
            }

            Prune(item.NavigationItems, clusterName);
            if (item.NavigationItems.Count == 0
                && item.Id.StartsWith($"{clusterName}-section", StringComparison.Ordinal))
            {
                items.RemoveAt(index);
            }
        }
    }

    private static bool SameLocation(NavigationPlacement left, NavigationPlacement right)
    {
        if (!string.Equals(left.SectionId, right.SectionId, StringComparison.Ordinal)
            || left.Groups.Count != right.Groups.Count)
        {
            return false;
        }

        for (var index = 0; index < left.Groups.Count; index++)
        {
            if (!string.Equals(left.Groups[index].Id, right.Groups[index].Id, StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
    }

    private static string LinkId(ClusterWorkspace cluster, string key)
        => $"{cluster.Runtime.Name}-{key}";

    private static bool CanListCustomResourceDefinitions(ClusterWorkspace cluster)
    {
        var definitionConfig = cluster.GetResourceConfigs()
            .FirstOrDefault(config => config.Kind == GroupApiVersionKind.From<V1CustomResourceDefinition>());
        return definitionConfig is { PermissionsLoaded: true, CanListAndWatch: true };
    }

    private bool CanCreatePortForward(ClusterWorkspace cluster, GroupApiVersionKind podKind)
    {
        try
        {
            return cluster.Runtime.Permissions.CanIAnyNamespace(podKind, true, Verb.Create, "portforward");
        }
        catch (Exception ex)
        {
            _logger.LogDebug(ex, "Unable to evaluate port forward permissions for cluster {Cluster}", cluster.Runtime.Name);
            return false;
        }
    }

    private static IEnumerable<NavigationLink> EnumerateLinks(IEnumerable<NavigationItem> items)
    {
        foreach (var item in items)
        {
            if (item is NavigationLink link)
            {
                yield return link;
            }

            foreach (var nested in EnumerateLinks(item.NavigationItems))
            {
                yield return nested;
            }
        }
    }

    private void Index(ObservableCollection<NavigationItem> items, ObservableCollection<NavigationItem> parent)
    {
        foreach (var item in items)
        {
            _existing.TryAdd(item.Id, new ItemLocation(item, parent));
            Index(item.NavigationItems, item.NavigationItems);
        }
    }

    private readonly record struct ItemLocation(NavigationItem Item, ObservableCollection<NavigationItem> Parent);
}
