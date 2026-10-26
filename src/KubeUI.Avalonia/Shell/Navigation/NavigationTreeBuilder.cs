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

    /// <summary>
    /// State for a single reconcile pass. The builder is shared by every cluster, so this
    /// state must not live on the builder instance.
    /// </summary>
    private sealed class ReconcileContext
    {
        public ReconcileContext(ClusterNavigationNode node)
        {
            Node = node;
            Index(node.NavigationItems, node.NavigationItems);
        }

        public ClusterNavigationNode Node { get; }

        public HashSet<string> DesiredIds { get; } = new(StringComparer.Ordinal);

        public Dictionary<string, ItemLocation> Existing { get; } = new(StringComparer.Ordinal);

        private void Index(ObservableCollection<NavigationItem> items, ObservableCollection<NavigationItem> parent)
        {
            foreach (var item in items)
            {
                Existing.TryAdd(item.Id, new ItemLocation(item, parent));
                Index(item.NavigationItems, item.NavigationItems);
            }
        }
    }

    /// <summary>Applies the current workspace state to a cluster node while preserving link identity.</summary>
    public void Reconcile(ClusterNavigationNode node)
    {
        var cluster = node.Cluster;
        var context = new ReconcileContext(node);

        foreach (var target in NavigationTargets.FixedClusterLinks)
        {
            context.DesiredIds.Add(LinkId(cluster, target.Key));
        }

        if (cluster.Runtime.Status == ClusterStatus.Connected)
        {
            AddResourceLinks(context, cluster);
            AddDocuments(context, cluster);
            AddPortForwarders(context, cluster);
        }

        Prune(node.NavigationItems, cluster.Runtime.Name, context.DesiredIds);
    }

    /// <summary>Attaches count streams for a seeded resource kind to matching links.</summary>
    public void AttachResourceCount(ClusterNavigationNode node, GroupApiVersionKind kind)
    {
        // Take a snapshot of links to avoid issues if the tree is modified during iteration
        var links = EnumerateLinks(node.NavigationItems).ToArray();
        foreach (var link in links)
        {
            if (link.ResourceKind == kind)
            {
                AttachCount(link, node.Cluster, kind);
            }
        }
    }

    private void AddResourceLinks(ReconcileContext context, ClusterWorkspace cluster)
    {
        var canListDefinitions = CanListCustomResourceDefinitions(cluster);
        foreach (var resource in cluster.GetResourceConfigs())
        {
            if (!resource.PermissionsLoaded || !resource.CanListAndWatch)
            {
                continue;
            }

            if (resource.IsCustomResource && !canListDefinitions)
            {
                continue;
            }

            AddResource(
                context,
                cluster,
                resource,
                resource.Navigation,
                LinkId(cluster, $"resource-{ResourceKey(resource.Kind)}"));

            if (resource.IsCustomResource)
            {
                var customResources = NavigationPlacement.InSection(
                    NavigationSections.CustomResourcesId,
                    NavigationTreeEditor.ConstructCustomResourceGroups(resource.Kind.Group));
                if (!SameLocation(customResources, resource.Navigation))
                {
                    AddResource(
                        context,
                        cluster,
                        resource,
                        customResources,
                        AliasId(cluster, "crd", resource, customResources));
                }
            }

            foreach (var provider in _sections.Providers)
            {
                if (provider.Place(resource) is { } alias && !SameLocation(alias, resource.Navigation))
                {
                    AddResource(
                        context,
                        cluster,
                        resource,
                        alias,
                        AliasId(cluster, $"provider-{provider.Section.Id}", resource, alias));
                }
            }
        }
    }

    private void AddResource(
        ReconcileContext context,
        ClusterWorkspace cluster,
        IResourceConfig resource,
        NavigationPlacement placement,
        string id)
    {
        var target = ResolveTarget(context, cluster, placement);
        context.DesiredIds.Add(id);
        Upsert(
            context,
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

    private void AddDocuments(ReconcileContext context, ClusterWorkspace cluster)
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
                var target = ResolveTarget(context, cluster, NavigationPlacement.InSection(provider.Section.Id));
                context.DesiredIds.Add(id);
                Upsert(
                    context,
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

    private void AddPortForwarders(ReconcileContext context, ClusterWorkspace cluster)
    {
        var podKind = GroupApiVersionKind.From<V1Pod>();
        if (!cluster.TryGetResourceConfig(podKind, out var podConfig)
            || podConfig is not { PermissionsLoaded: true, CanListAndWatch: true }
            || !CanCreatePortForward(cluster, podKind))
        {
            return;
        }

        var id = LinkId(cluster, NavigationTargets.PortForwarders);
        var target = ResolveTarget(context, cluster, NavigationPlacement.InSection(NavigationSections.NetworkId));
        context.DesiredIds.Add(id);
        Upsert(
            context,
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
        ReconcileContext context,
        ClusterWorkspace cluster,
        NavigationPlacement placement)
    {
        var node = context.Node;
        if (placement.SectionId is not { } sectionId)
        {
            return node.NavigationItems;
        }

        if (_sections.Find(sectionId) is not { } section)
        {
            _logger.LogWarning(
                "Navigation placement targets unknown section {SectionId}; placing resource in the cluster root",
                sectionId);
            return node.NavigationItems;
        }

        var nodeId = $"{cluster.Runtime.Name}-section-{section.Id}";
        context.DesiredIds.Add(nodeId);
        var root = NavigationTreeEditor.EnsureNode(node.NavigationItems, nodeId, section.Name, section.Order, section.Icon, section.SvgIcon, section.SvgIconDark);
        if (placement.Groups.Count == 0)
        {
            return root.NavigationItems;
        }

        var prefix = $"{cluster.Runtime.Name}-section-group-{section.Id}";
        var target = NavigationTreeEditor.EnsurePath(root.NavigationItems, prefix, placement.Groups);
        foreach (var group in placement.Groups)
        {
            prefix += "/" + group.Id;
            context.DesiredIds.Add(prefix);
        }

        return target;
    }

    private void Upsert(
        ReconcileContext context,
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
        var isNew = !context.Existing.TryGetValue(id, out var location) || location.Item is not NavigationLink;
        var isMoving = !isNew && !ReferenceEquals(location.Parent, target);
        NavigationLink link;
        if (isNew)
        {
            link = new NavigationLink { Id = id };
        }
        else
        {
            link = (NavigationLink)location.Item;
            if (isMoving)
            {
                location.Parent.Remove(link);
            }
        }

        link.Name = name;
        link.Order = order;
        link.Cluster = cluster;
        link.FluentIcon = icon;
        link.ViewModelKey = viewModelKey;
        link.DocumentId = documentId;
        if (link.ResourceKind != resource?.Kind)
        {
            link.Count = null;
            link.CountResourceKind = null;
        }

        link.ResourceKind = resource?.Kind;
        link.ResourceIcon = resource is null ? null : _icons.GetIcon(resource.Kind);
        link.OpenCommand = resource is null ? null : _openCommand;
        link.OpenInNewTabCommand = resource is null ? null : _openInNewTabCommand;
        if (resource is not null)
        {
            AttachCount(link, cluster, resource.Kind);
        }

        if (isNew || isMoving)
        {
            target.Add(link);
            context.Existing[id] = new ItemLocation(link, target);
        }
    }

    private static void AttachCount(NavigationLink link, ClusterWorkspace cluster, GroupApiVersionKind kind)
    {
        if (link.Count is not null
            && (link.CountResourceKind is null || link.CountResourceKind == kind))
        {
            return;
        }

        if (cluster.Runtime.Objects.TryGetValue(kind, out var container)
            && container is IResourceContainer { IsSeeded: true })
        {
            link.Count = cluster.Runtime.GetResourceCount(kind)
                .Sample(TimeSpan.FromMilliseconds(100), AvaloniaScheduler.Instance);
            link.CountResourceKind = kind;
        }
    }

    private void Prune(ObservableCollection<NavigationItem> items, string clusterName, HashSet<string> desiredIds)
    {
        for (var index = items.Count - 1; index >= 0; index--)
        {
            var item = items[index];
            if (item is NavigationLink link)
            {
                if (!desiredIds.Contains(link.Id))
                {
                    items.RemoveAt(index);
                }

                continue;
            }

            Prune(item.NavigationItems, clusterName, desiredIds);
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

    private static string ResourceKey(GroupApiVersionKind kind)
        => $"{(string.IsNullOrEmpty(kind.Group) ? "core" : kind.Group)}/{kind.Kind}";

    private static string AliasId(
        ClusterWorkspace cluster,
        string slot,
        IResourceConfig resource,
        NavigationPlacement placement)
        => $"{cluster.Runtime.Name}-section-resource-{placement.SectionId ?? "root"}-{slot}-{ResourceKey(resource.Kind)}";

    private static bool CanListCustomResourceDefinitions(ClusterWorkspace cluster)
    {
        return cluster.TryGetResourceConfig(
                   GroupApiVersionKind.From<V1CustomResourceDefinition>(),
                   out var definitionConfig)
               && definitionConfig is { PermissionsLoaded: true, CanListAndWatch: true };
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

    private readonly record struct ItemLocation(NavigationItem Item, ObservableCollection<NavigationItem> Parent);
}
