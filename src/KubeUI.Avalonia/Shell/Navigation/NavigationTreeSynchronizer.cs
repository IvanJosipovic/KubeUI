using System.Reactive.Linq;
using System.Windows.Input;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Infrastructure.Threading;
using KubeUI.Avalonia.Resources;
using KubeUI.Avalonia.Services.Icons;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Shell.Navigation;

/// <summary>Applies a desired navigation entry set to a cluster navigation node while preserving link identity.</summary>
internal sealed class NavigationTreeSynchronizer
{
    private readonly IResourceIconService _iconService;
    private readonly ICommand? _openCommand;
    private readonly ICommand? _openInNewTabCommand;

    public NavigationTreeSynchronizer(
        IResourceIconService iconService,
        ICommand? openCommand,
        ICommand? openInNewTabCommand)
    {
        _iconService = iconService;
        _openCommand = openCommand;
        _openInNewTabCommand = openInNewTabCommand;
    }

    public void Reconcile(ClusterNavigationNode node, IReadOnlyList<NavigationEntry> entries)
    {
        var cluster = node.Cluster;
        var desiredIds = new HashSet<string>(StringComparer.Ordinal);
        foreach (var entry in entries)
        {
            desiredIds.Add(entry.Id);
            var target = ResolveTarget(node, cluster, entry, desiredIds);
            Upsert(node, target, cluster, entry);
        }

        PruneGenerated(node.NavigationItems, cluster.Runtime.Name, desiredIds);
        AttachCounts(node, cluster);
    }

    private static ObservableCollection<NavigationItem> ResolveTarget(
        ClusterNavigationNode node,
        ClusterWorkspace cluster,
        NavigationEntry entry,
        HashSet<string> desiredIds)
    {
        if (entry.Section is not { } section)
        {
            return node.NavigationItems;
        }

        var sectionId = NavigationIds.Section(cluster, section.Id);
        desiredIds.Add(sectionId);
        var root = NavigationTreeEditor.EnsureNode(
            node.NavigationItems,
            sectionId,
            section.Name,
            section.Order,
            section.Icon);

        if (entry.Groups.Count == 0)
        {
            return root.NavigationItems;
        }

        var prefix = NavigationIds.Group(cluster, section.Id);
        var target = NavigationTreeEditor.EnsurePath(root.NavigationItems, prefix, entry.Groups);
        var key = prefix;
        foreach (var group in entry.Groups)
        {
            key += "/" + group.Id;
            desiredIds.Add(key);
        }

        return target;
    }

    private void Upsert(
        ClusterNavigationNode node,
        ObservableCollection<NavigationItem> target,
        ClusterWorkspace cluster,
        NavigationEntry entry)
    {
        var existing = Find(node.NavigationItems, entry.Id) as NavigationLink;
        if (existing is null)
        {
            var created = new NavigationLink { Id = entry.Id };
            ApplyLink(created, cluster, entry);
            target.Add(created);
            return;
        }

        var parent = FindParentCollection(node.NavigationItems, entry.Id);
        if (!ReferenceEquals(parent, target))
        {
            parent?.Remove(existing);
            target.Add(existing);
        }

        ApplyLink(existing, cluster, entry);
    }

    private void ApplyLink(NavigationLink link, ClusterWorkspace cluster, NavigationEntry entry)
    {
        link.Name = entry.Name;
        link.Order = entry.Order;
        link.Cluster = cluster;

        switch (entry)
        {
            case ResourceEntry resource:
                link.ResourceKind = resource.Kind;
                link.ResourceIcon = _iconService.GetIcon(resource.Kind);
                link.FluentIcon = null;
                link.ViewModelKey = null;
                link.DocumentId = null;
                link.OpenCommand = _openCommand;
                link.OpenInNewTabCommand = _openInNewTabCommand;
                break;

            case DocumentEntry document:
                link.DocumentId = document.DocumentId;
                link.FluentIcon = document.Icon;
                link.ViewModelKey = null;
                link.ResourceKind = null;
                link.ResourceIcon = null;
                break;

            case BuiltinEntry builtin:
                link.ViewModelKey = builtin.ViewModelKey;
                link.FluentIcon = builtin.Icon;
                link.ResourceKind = null;
                link.DocumentId = null;
                link.ResourceIcon = null;
                break;
        }
    }

    /// <summary>Attaches count streams for a seeded resource kind to matching links.</summary>
    public void AttachResourceCount(ClusterNavigationNode node, GroupApiVersionKind kind)
    {
        var cluster = node.Cluster;
        if (!cluster.Runtime.Objects.TryGetValue(kind, out var container)
            || container is not IResourceContainer { IsSeeded: true })
        {
            return;
        }

        foreach (var link in EnumerateLinks(node.NavigationItems))
        {
            if (link.ResourceKind == kind && link.Count is null)
            {
                link.Count = CreateResourceCountStream(cluster, kind);
            }
        }
    }

    private void AttachCounts(ClusterNavigationNode node, ClusterWorkspace cluster)
    {
        foreach (var link in EnumerateLinks(node.NavigationItems))
        {
            if (link.ResourceKind is { } kind
                && link.Count is null
                && cluster.Runtime.Objects.TryGetValue(kind, out var container)
                && container is IResourceContainer { IsSeeded: true })
            {
                link.Count = CreateResourceCountStream(cluster, kind);
            }
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

    private static void PruneGenerated(ObservableCollection<NavigationItem> items, string clusterName, HashSet<string> desiredIds)
    {
        for (var index = items.Count - 1; index >= 0; index--)
        {
            var item = items[index];
            if (item is NavigationLink link)
            {
                if (!desiredIds.Contains(link.Id) && !IsFixedClusterLink(clusterName, link.Id))
                {
                    items.RemoveAt(index);
                }

                continue;
            }

            PruneGenerated(item.NavigationItems, clusterName, desiredIds);
            if (item.NavigationItems.Count == 0
                && item.Id.StartsWith($"{clusterName}-section", StringComparison.Ordinal)
                && !desiredIds.Contains(item.Id))
            {
                items.RemoveAt(index);
            }
        }
    }

    private static bool IsFixedClusterLink(string clusterName, string id)
    {
        return id == $"{clusterName}-{NavigationTargets.ClusterWorkspace}"
            || id == $"{clusterName}-{NavigationTargets.Visualization}"
            || id == $"{clusterName}-{NavigationTargets.ClusterSettings}"
            || id == $"{clusterName}-{NavigationTargets.LoadYaml}"
            || id == $"{clusterName}-{NavigationTargets.LoadFolder}";
    }

    private static NavigationItem? Find(IEnumerable<NavigationItem> items, string id)
    {
        foreach (var item in items)
        {
            if (string.Equals(item.Id, id, StringComparison.Ordinal))
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

    private static ObservableCollection<NavigationItem>? FindParentCollection(ObservableCollection<NavigationItem> items, string id)
    {
        if (items.Any(item => string.Equals(item.Id, id, StringComparison.Ordinal)))
        {
            return items;
        }

        foreach (var item in items)
        {
            var nested = FindParentCollection(item.NavigationItems, id);
            if (nested is not null)
            {
                return nested;
            }
        }

        return null;
    }

    private static IObservable<int> CreateResourceCountStream(ClusterWorkspace cluster, GroupApiVersionKind kind) =>
        cluster.Runtime.GetResourceCount(kind).Sample(TimeSpan.FromMilliseconds(100), AvaloniaScheduler.Instance);
}
