using k8s;
using k8s.Models;

namespace KubeUI.Kubernetes.Resources.Relationships;

/// <summary>
/// Projects complete relationship graphs into visualization scopes without UI dependencies.
/// </summary>
public static class ResourceGraphProjection
{
    /// <summary>Projects graph to resources reachable from root, including ancestors and descendants.</summary>
    public static ResourceRelationshipGraph ToRootResource(
        ResourceRelationshipGraph graph,
        IKubernetesObject<V1ObjectMeta> root)
    {
        ArgumentNullException.ThrowIfNull(graph);
        ArgumentNullException.ThrowIfNull(root);

        ResourceIdentity rootIdentity = Identity(root);
        Dictionary<ResourceIdentity, List<ResourceIdentity>> parentsByChild = [];
        Dictionary<ResourceIdentity, List<ResourceIdentity>> childrenByParent = [];
        foreach (var relationship in graph.Relationships)
        {
            parentsByChild.TryAdd(relationship.Target, []);
            parentsByChild[relationship.Target].Add(relationship.Source);
            childrenByParent.TryAdd(relationship.Source, []);
            childrenByParent[relationship.Source].Add(relationship.Target);
        }

        HashSet<ResourceIdentity> ancestors = [rootIdentity];
        Queue<ResourceIdentity> parents = new([rootIdentity]);
        while (parents.Count > 0)
        {
            var current = parents.Dequeue();
            if (!parentsByChild.TryGetValue(current, out var parentIdentities))
            {
                continue;
            }

            foreach (var parent in parentIdentities)
            {
                if (ancestors.Add(parent))
                {
                    parents.Enqueue(parent);
                }
            }
        }

        HashSet<ResourceIdentity> included = [.. ancestors];
        Queue<ResourceIdentity> descendants = new([rootIdentity]);
        HashSet<ResourceIdentity> visitedDescendants = [];
        while (descendants.Count > 0)
        {
            var current = descendants.Dequeue();
            if (!visitedDescendants.Add(current) || current != rootIdentity && ancestors.Contains(current))
            {
                continue;
            }

            if (!childrenByParent.TryGetValue(current, out var childIdentities))
            {
                continue;
            }

            foreach (var child in childIdentities)
            {
                if (included.Add(child))
                {
                    descendants.Enqueue(child);
                }
            }
        }

        return new ResourceRelationshipGraph(
            graph.Resources.Where(resource => included.Contains(Identity(resource))).ToArray(),
            graph.Relationships.Where(relationship => included.Contains(relationship.Source) && included.Contains(relationship.Target)).ToArray(),
            graph.UnresolvedReferences,
            graph.RequiredSeedPrerequisites);
    }

    /// <summary>Projects graph to selected namespaces and directly related resources.</summary>
    public static ResourceRelationshipGraph ToSelectedNamespaces(
        ResourceRelationshipGraph graph,
        IReadOnlySet<string> selectedNamespaces)
    {
        ArgumentNullException.ThrowIfNull(graph);
        ArgumentNullException.ThrowIfNull(selectedNamespaces);

        var selected = graph.Resources
            .Select(Identity)
            .Where(identity => selectedNamespaces.Contains(identity.Namespace ?? string.Empty))
            .ToHashSet();
        HashSet<ResourceIdentity> included = [.. selected];

        foreach (var relationship in graph.Relationships)
        {
            if (relationship.Kind == ResourceRelationshipKind.GitOps
                && selected.Contains(relationship.Target))
            {
                included.Add(relationship.Source);
                continue;
            }

            if (relationship.Kind == ResourceRelationshipKind.GitOps)
            {
                continue;
            }

            if (relationship.Kind == ResourceRelationshipKind.Reference
                && (selected.Contains(relationship.Source) || selected.Contains(relationship.Target)))
            {
                included.Add(relationship.Source);
                included.Add(relationship.Target);
            }

            if (relationship.Kind != ResourceRelationshipKind.GitOps
                && ConnectsSelectedNamespaceToClusterResource(relationship, selected, selectedNamespaces))
            {
                included.Add(relationship.Source);
                included.Add(relationship.Target);
            }
        }

        IncludeSelectedNamespaceOwners(graph.Relationships, selectedNamespaces, included);
        return ProjectIncluded(graph, included);
    }

    /// <summary>Projects an incremental relationship delta against the current graph identities.</summary>
    public static ResourceRelationshipGraph ToSelectedNamespacesIncremental(
        ResourceRelationshipGraph delta,
        IReadOnlySet<string> selectedNamespaces,
        IReadOnlySet<ResourceIdentity> currentIdentities)
    {
        ArgumentNullException.ThrowIfNull(delta);
        ArgumentNullException.ThrowIfNull(selectedNamespaces);
        ArgumentNullException.ThrowIfNull(currentIdentities);

        var included = delta.Resources
            .Select(Identity)
            .Where(identity => selectedNamespaces.Contains(identity.Namespace ?? string.Empty)
                || string.IsNullOrEmpty(identity.Namespace)
                    && delta.Relationships.Any(relationship =>
                        (relationship.Source == identity || relationship.Target == identity)
                        && (currentIdentities.Contains(relationship.Source) || currentIdentities.Contains(relationship.Target))))
            .ToHashSet();

        foreach (var relationship in delta.Relationships)
        {
            if (relationship.Kind == ResourceRelationshipKind.GitOps)
            {
                if (currentIdentities.Contains(relationship.Target))
                {
                    included.Add(relationship.Source);
                }

                continue;
            }

            var sourceInScope = currentIdentities.Contains(relationship.Source)
                || selectedNamespaces.Contains(relationship.Source.Namespace ?? string.Empty);
            var targetInScope = currentIdentities.Contains(relationship.Target)
                || selectedNamespaces.Contains(relationship.Target.Namespace ?? string.Empty);
            if (!sourceInScope && !targetInScope)
            {
                continue;
            }

            if (sourceInScope && (selectedNamespaces.Contains(relationship.Source.Namespace ?? string.Empty)
                || string.IsNullOrEmpty(relationship.Source.Namespace)
                    && IsClusterScoped(relationship.Target)))
            {
                included.Add(relationship.Target);
            }

            if (targetInScope && (selectedNamespaces.Contains(relationship.Target.Namespace ?? string.Empty)
                || string.IsNullOrEmpty(relationship.Target.Namespace)
                    && IsClusterScoped(relationship.Source)))
            {
                included.Add(relationship.Source);
            }
        }

        IncludeSelectedNamespaceOwners(delta.Relationships, selectedNamespaces, included);
        return ProjectIncluded(delta, included);
    }

    private static void IncludeSelectedNamespaceOwners(
        IReadOnlyList<ResourceRelationship> relationships,
        IReadOnlySet<string> selectedNamespaces,
        HashSet<ResourceIdentity> included)
    {
        bool changed;
        do
        {
            changed = false;
            foreach (var relationship in relationships)
            {
                if (relationship.Kind != ResourceRelationshipKind.Owner
                    || !included.Contains(relationship.Target)
                    || (!string.IsNullOrEmpty(relationship.Source.Namespace)
                        && !selectedNamespaces.Contains(relationship.Source.Namespace)))
                {
                    continue;
                }

                changed |= included.Add(relationship.Source);
            }
        }
        while (changed);
    }

    private static ResourceRelationshipGraph ProjectIncluded(
        ResourceRelationshipGraph graph,
        IReadOnlySet<ResourceIdentity> included)
        => new(
            graph.Resources.Where(resource => included.Contains(Identity(resource))).ToArray(),
            graph.Relationships.Where(relationship => included.Contains(relationship.Source) && included.Contains(relationship.Target)).ToArray(),
            graph.UnresolvedReferences,
            graph.RequiredSeedPrerequisites);

    private static bool ConnectsSelectedNamespaceToClusterResource(
        ResourceRelationship relationship,
        IReadOnlySet<ResourceIdentity> selected,
        IReadOnlySet<string> selectedNamespaces)
        => selected.Contains(relationship.Source) && IsClusterScoped(relationship.Target)
            || selected.Contains(relationship.Target) && IsClusterScoped(relationship.Source)
            || selectedNamespaces.Contains(relationship.Source.Namespace ?? string.Empty) && IsClusterScoped(relationship.Target)
            || selectedNamespaces.Contains(relationship.Target.Namespace ?? string.Empty) && IsClusterScoped(relationship.Source);

    private static bool IsClusterScoped(ResourceIdentity identity)
        => string.IsNullOrEmpty(identity.Namespace);

    private static ResourceIdentity Identity(IKubernetesObject<V1ObjectMeta> resource)
        => new(resource.ApiVersion ?? string.Empty, resource.Kind ?? string.Empty, resource.Namespace(), resource.Name() ?? string.Empty, resource.Uid());
}
