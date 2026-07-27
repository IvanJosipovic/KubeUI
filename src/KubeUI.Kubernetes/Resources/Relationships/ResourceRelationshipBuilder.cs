using k8s;
using k8s.Models;

namespace KubeUI.Kubernetes.Resources.Relationships;

public sealed record ResourceRelationshipGraph(
    IReadOnlyList<IKubernetesObject<V1ObjectMeta>> Resources,
    IReadOnlyList<ResourceRelationship> Relationships)
{
    public static ResourceRelationshipGraph Empty { get; } = new([], []);
}

public readonly record struct ResourceKey(string ApiVersion, string Kind, string? Namespace, string Name);

public sealed class ResourceRelationshipContext
{
    private readonly Dictionary<string, IKubernetesObject<V1ObjectMeta>> _resourcesByUid;
    private readonly Dictionary<ResourceKey, IKubernetesObject<V1ObjectMeta>> _resourcesByKey;
    private readonly Dictionary<string, IReadOnlyList<IKubernetesObject<V1ObjectMeta>>> _resourcesByKind;

    internal ResourceRelationshipContext(
        Dictionary<string, IKubernetesObject<V1ObjectMeta>> resourcesByUid,
        Dictionary<ResourceKey, IKubernetesObject<V1ObjectMeta>> resourcesByKey,
        Dictionary<string, IReadOnlyList<IKubernetesObject<V1ObjectMeta>>> resourcesByKind)
    {
        _resourcesByUid = resourcesByUid;
        _resourcesByKey = resourcesByKey;
        _resourcesByKind = resourcesByKind;
    }

    public bool TryGetByUid(string? uid, out IKubernetesObject<V1ObjectMeta>? resource)
    {
        resource = null;
        return !string.IsNullOrWhiteSpace(uid) && _resourcesByUid.TryGetValue(uid, out resource);
    }

    public bool TryGet(string apiVersion, string kind, string? namespaceName, string? name, out IKubernetesObject<V1ObjectMeta>? resource)
    {
        resource = null;
        return !string.IsNullOrWhiteSpace(name)
            && _resourcesByKey.TryGetValue(new ResourceKey(apiVersion, kind, namespaceName, name), out resource);
    }

    public bool TryGetByKind(string kind, out IReadOnlyList<IKubernetesObject<V1ObjectMeta>> resources)
        => _resourcesByKind.TryGetValue(kind, out resources!);

    public bool TryGetByName(string apiVersion, string kind, string? name, out IKubernetesObject<V1ObjectMeta>? resource)
    {
        resource = null;
        if (string.IsNullOrWhiteSpace(name) || !_resourcesByKind.TryGetValue(kind, out IReadOnlyList<IKubernetesObject<V1ObjectMeta>>? resources))
        {
            return false;
        }

        foreach (IKubernetesObject<V1ObjectMeta> candidate in resources)
        {
            if (string.Equals(candidate.ApiVersion, apiVersion, StringComparison.Ordinal)
                && string.Equals(candidate.Name(), name, StringComparison.Ordinal))
            {
                resource = candidate;
                return true;
            }
        }

        return false;
    }

    public ResourceIdentity Identity(IKubernetesObject<V1ObjectMeta> resource)
        => new(resource.ApiVersion ?? string.Empty, resource.Kind ?? string.Empty, resource.Namespace(), resource.Name() ?? string.Empty, resource.Uid());

    public void Add(
        ICollection<ResourceRelationship> relationships,
        IKubernetesObject<V1ObjectMeta> source,
        IKubernetesObject<V1ObjectMeta> target,
        ResourceRelationshipKind kind,
        string? label = null)
        => relationships.Add(new ResourceRelationship(Identity(source), Identity(target), kind, label));
}

public interface IResourceRelationshipBuilder
{
    ResourceRelationshipGraph Build(
        IEnumerable<IKubernetesObject<V1ObjectMeta>> resources,
        IReadOnlySet<string> selectedNamespaces,
        bool hideNoise);

    ResourceRelationshipGraph BuildAdditionDelta(
        IEnumerable<IKubernetesObject<V1ObjectMeta>> resources,
        ResourceKey addedResource,
        IReadOnlySet<string> selectedNamespaces,
        bool hideNoise);
}

public sealed class ResourceRelationshipBuilder : IResourceRelationshipBuilder
{
    private readonly IReadOnlyList<IResourceRelationshipProvider> _providers;

    public ResourceRelationshipBuilder(IEnumerable<IResourceRelationshipProvider>? providers = null)
    {
        _providers = providers?.ToArray() ??
        [
            new Providers.OwnerReferenceRelationshipProvider(),
            new Providers.IngressRelationshipProvider(),
            new Providers.EndpointSliceRelationshipProvider(),
            new Providers.PodTemplateReferenceRelationshipProvider(),
            new Providers.ServiceAccountRelationshipProvider(),
            new Providers.StorageRelationshipProvider(),
            new Providers.RbacRelationshipProvider(),
            new Providers.EventRelationshipProvider(),
            new Providers.GitOpsRelationshipProvider(),
        ];
    }

    public ResourceRelationshipGraph Build(
        IEnumerable<IKubernetesObject<V1ObjectMeta>> resources,
        IReadOnlySet<string> selectedNamespaces,
        bool hideNoise)
    {
        ArgumentNullException.ThrowIfNull(resources);
        ArgumentNullException.ThrowIfNull(selectedNamespaces);

        List<IKubernetesObject<V1ObjectMeta>> visible = [];
        foreach (IKubernetesObject<V1ObjectMeta> resource in resources)
        {
            string? namespaceName = resource.Namespace();
            if (hideNoise && (resource is Corev1Event || resource is V1ReplicaSet replicaSet && replicaSet.Status?.Replicas == 0))
            {
                continue;
            }

            visible.Add(resource);
        }

        Dictionary<string, IKubernetesObject<V1ObjectMeta>> byUid = new(StringComparer.Ordinal);
        Dictionary<ResourceKey, IKubernetesObject<V1ObjectMeta>> byKey = [];
        Dictionary<string, List<IKubernetesObject<V1ObjectMeta>>> byKind = new(StringComparer.Ordinal);
        foreach (IKubernetesObject<V1ObjectMeta> resource in visible)
        {
            string? uid = resource.Uid();
            if (!string.IsNullOrWhiteSpace(uid))
            {
                byUid.TryAdd(uid, resource);
            }

            byKey.TryAdd(new(resource.ApiVersion ?? string.Empty, resource.Kind ?? string.Empty, resource.Namespace(), resource.Name() ?? string.Empty), resource);
            if (!byKind.TryGetValue(resource.Kind ?? string.Empty, out List<IKubernetesObject<V1ObjectMeta>>? kindResources))
            {
                kindResources = [];
                byKind.Add(resource.Kind ?? string.Empty, kindResources);
            }

            kindResources.Add(resource);
        }

        ResourceRelationshipContext context = new(byUid, byKey, byKind.ToDictionary(x => x.Key, x => (IReadOnlyList<IKubernetesObject<V1ObjectMeta>>)x.Value, StringComparer.Ordinal));
        HashSet<ResourceRelationship> relationshipSet = [];
        foreach (IKubernetesObject<V1ObjectMeta> resource in visible)
        {
            foreach (IResourceRelationshipProvider provider in _providers)
            {
                provider.AddRelationships(resource, context, relationshipSet);
            }
        }

        IReadOnlyList<ResourceRelationship> relationships = SimplifyRelationships(relationshipSet);
        if (selectedNamespaces.Count == 0)
        {
            return new ResourceRelationshipGraph(visible, relationships);
        }

        HashSet<ResourceIdentity> included = [];
        foreach (IKubernetesObject<V1ObjectMeta> resource in visible)
        {
            if (selectedNamespaces.Contains(resource.Namespace() ?? string.Empty))
            {
                included.Add(new ResourceIdentity(
                    resource.ApiVersion ?? string.Empty,
                    resource.Kind ?? string.Empty,
                    resource.Namespace(),
                    resource.Name() ?? string.Empty,
                    resource.Uid()));
            }
        }

        bool changed;
        do
        {
            changed = false;
            foreach (ResourceRelationship relationship in relationships)
            {
                if (included.Contains(relationship.Source)
                    && CanTraverse(relationship.Source, relationship.Target)
                    && included.Add(relationship.Target)
                    || included.Contains(relationship.Target)
                    && CanTraverseBackwards(relationship.Target)
                    && included.Add(relationship.Source))
                {
                    changed = true;
                }
            }
        } while (changed);

        visible = visible.Where(resource => included.Contains(new ResourceIdentity(
            resource.ApiVersion ?? string.Empty,
            resource.Kind ?? string.Empty,
            resource.Namespace(),
            resource.Name() ?? string.Empty,
            resource.Uid()))).ToList();
        relationships = relationships.Where(relationship => included.Contains(relationship.Source) && included.Contains(relationship.Target)).ToArray();
        return new ResourceRelationshipGraph(visible, relationships);
    }

    public ResourceRelationshipGraph BuildAdditionDelta(
        IEnumerable<IKubernetesObject<V1ObjectMeta>> resources,
        ResourceKey addedResource,
        IReadOnlySet<string> selectedNamespaces,
        bool hideNoise)
    {
        ArgumentNullException.ThrowIfNull(resources);
        ArgumentNullException.ThrowIfNull(selectedNamespaces);

        ResourceRelationshipGraph graph = Build(resources, selectedNamespaces, hideNoise);
        IKubernetesObject<V1ObjectMeta>? added = graph.Resources.FirstOrDefault(resource =>
            new ResourceKey(resource.ApiVersion ?? string.Empty, resource.Kind ?? string.Empty, resource.Namespace(), resource.Name() ?? string.Empty) == addedResource);
        if (added == null)
        {
            return ResourceRelationshipGraph.Empty;
        }

        ResourceIdentity addedIdentity = new(added.ApiVersion ?? string.Empty, added.Kind ?? string.Empty, added.Namespace(), added.Name() ?? string.Empty, added.Uid());
        HashSet<ResourceIdentity> ancestors = [addedIdentity];
        Queue<ResourceIdentity> parents = new([addedIdentity]);
        while (parents.Count > 0)
        {
            ResourceIdentity current = parents.Dequeue();
            foreach (ResourceRelationship relationship in graph.Relationships)
            {
                if (relationship.Target == current
                    && (CanTraverseBackwards(current) || current == addedIdentity)
                    && ancestors.Add(relationship.Source))
                {
                    parents.Enqueue(relationship.Source);
                }
            }
        }

        HashSet<ResourceIdentity> included = [.. ancestors];
        Queue<ResourceIdentity> descendants = new([addedIdentity]);
        HashSet<ResourceIdentity> visitedDescendants = [];
        while (descendants.Count > 0)
        {
            ResourceIdentity current = descendants.Dequeue();
            if (!visitedDescendants.Add(current) || current != addedIdentity && ancestors.Contains(current))
            {
                continue;
            }

            foreach (ResourceRelationship relationship in graph.Relationships)
            {
                if (relationship.Source == current
                    && (CanTraverse(relationship.Source, relationship.Target) || current == addedIdentity)
                    && included.Add(relationship.Target))
                {
                    descendants.Enqueue(relationship.Target);
                }
            }
        }

        return new ResourceRelationshipGraph(
            graph.Resources.Where(resource => included.Contains(new ResourceIdentity(
                resource.ApiVersion ?? string.Empty,
                resource.Kind ?? string.Empty,
                resource.Namespace(),
                resource.Name() ?? string.Empty,
                resource.Uid()))).ToArray(),
            graph.Relationships.Where(relationship => included.Contains(relationship.Source) && included.Contains(relationship.Target)).ToArray());
    }

    private static bool CanTraverse(ResourceIdentity source, ResourceIdentity target)
        => !string.IsNullOrEmpty(source.Namespace) || string.IsNullOrEmpty(target.Namespace);

    private static bool CanTraverseBackwards(ResourceIdentity target)
        => !string.IsNullOrEmpty(target.Namespace);

    public static IReadOnlyList<ResourceRelationship> SimplifyRelationships(IEnumerable<ResourceRelationship> relationships)
    {
        ArgumentNullException.ThrowIfNull(relationships);

        List<ResourceRelationship> unique = [];
        HashSet<(ResourceIdentity Source, ResourceIdentity Target, ResourceRelationshipKind Kind, string? Label)> seen = [];
        foreach (ResourceRelationship relationship in relationships)
        {
            if (seen.Add((relationship.Source, relationship.Target, relationship.Kind, relationship.Label)))
            {
                unique.Add(relationship);
            }
        }

        if (unique.Count < 2)
        {
            return unique;
        }

        Dictionary<ResourceIdentity, List<ResourceIdentity>> ownerChildren = [];
        Dictionary<ResourceIdentity, HashSet<ResourceIdentity>> sourcesByTarget = [];
        foreach (ResourceRelationship relationship in unique)
        {
            if (relationship.Kind == ResourceRelationshipKind.Owner)
            {
                ownerChildren.TryAdd(relationship.Source, []);
                ownerChildren[relationship.Source].Add(relationship.Target);
            }
            else
            {
                sourcesByTarget.TryAdd(relationship.Target, []);
                sourcesByTarget[relationship.Target].Add(relationship.Source);
            }
        }

        HashSet<(ResourceIdentity Source, ResourceIdentity Target)> remove = [];
        foreach ((ResourceIdentity target, HashSet<ResourceIdentity> sources) in sourcesByTarget)
        {
            foreach (ResourceIdentity source in sources)
            {
                if (!ownerChildren.TryGetValue(source, out List<ResourceIdentity>? children))
                {
                    continue;
                }

                Queue<ResourceIdentity> queue = new(children);
                HashSet<ResourceIdentity> visited = [];
                while (queue.Count > 0)
                {
                    ResourceIdentity descendant = queue.Dequeue();
                    if (!visited.Add(descendant))
                    {
                        continue;
                    }

                    if (sources.Contains(descendant))
                    {
                        remove.Add((source, target));
                    }

                    if (ownerChildren.TryGetValue(descendant, out children))
                    {
                        foreach (ResourceIdentity child in children)
                        {
                            queue.Enqueue(child);
                        }
                    }
                }
            }
        }

        Dictionary<(ResourceIdentity Source, ResourceRelationshipKind Kind, string? Label), List<ResourceIdentity>> relationshipTargets = [];
        foreach (ResourceRelationship relationship in unique)
        {
            relationshipTargets.TryAdd((relationship.Source, relationship.Kind, relationship.Label), []);
            relationshipTargets[(relationship.Source, relationship.Kind, relationship.Label)].Add(relationship.Target);
        }

        HashSet<ResourceRelationship> transitive = [];
        foreach (ResourceRelationship relationship in unique)
        {
            if (!relationshipTargets.TryGetValue((relationship.Source, relationship.Kind, relationship.Label), out List<ResourceIdentity>? initialTargets))
            {
                continue;
            }

            Queue<ResourceIdentity> queue = new(initialTargets.Where(target => target != relationship.Target));
            HashSet<ResourceIdentity> visited = [];
            while (queue.Count > 0)
            {
                ResourceIdentity current = queue.Dequeue();
                if (!visited.Add(current))
                {
                    continue;
                }

                if (current == relationship.Target)
                {
                    transitive.Add(relationship);
                    break;
                }

                if (relationshipTargets.TryGetValue((current, relationship.Kind, relationship.Label), out List<ResourceIdentity>? nextTargets))
                {
                    foreach (ResourceIdentity target in nextTargets)
                    {
                        queue.Enqueue(target);
                    }
                }
            }
        }

        return unique
            .Where(relationship => !remove.Contains((relationship.Source, relationship.Target)) && !transitive.Contains(relationship))
            .ToArray();
    }
}
