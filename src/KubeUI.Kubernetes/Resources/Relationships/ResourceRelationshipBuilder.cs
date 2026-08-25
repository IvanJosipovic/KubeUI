using k8s;
using k8s.Models;

namespace KubeUI.Kubernetes.Resources.Relationships;

public sealed record ResourceRelationshipGraph(
    IReadOnlyList<IKubernetesObject<V1ObjectMeta>> Resources,
    IReadOnlyList<ResourceRelationship> Relationships,
    IReadOnlySet<UnresolvedResourceReference>? UnresolvedReferences = null,
    IReadOnlySet<ResourceSeedPrerequisite>? SeedPrerequisites = null)
{
    public static ResourceRelationshipGraph Empty { get; } = new([], []);

    public IReadOnlySet<UnresolvedResourceReference> PendingReferences
        => UnresolvedReferences ?? EmptyUnresolvedReferences;

    public IReadOnlySet<ResourceSeedPrerequisite> RequiredSeedPrerequisites
        => SeedPrerequisites ?? EmptySeedPrerequisites;

    private static IReadOnlySet<UnresolvedResourceReference> EmptyUnresolvedReferences { get; } = new HashSet<UnresolvedResourceReference>();

    private static IReadOnlySet<ResourceSeedPrerequisite> EmptySeedPrerequisites { get; } = new HashSet<ResourceSeedPrerequisite>();
}

public readonly record struct ResourceKey(string ApiVersion, string Kind, string? Namespace, string Name);

public sealed record UnresolvedResourceReference(
    string ApiGroup,
    string? ApiVersion,
    string Kind,
    string? Namespace,
    string Name);

public sealed class ResourceRelationshipContext
{
    private readonly Dictionary<string, IKubernetesObject<V1ObjectMeta>> _resourcesByUid;
    private readonly Dictionary<ResourceKey, IKubernetesObject<V1ObjectMeta>> _resourcesByKey;
    private readonly Dictionary<(string ApiGroup, string Kind), IReadOnlyList<IKubernetesObject<V1ObjectMeta>>> _resourcesByGroupAndKind;
    private readonly HashSet<UnresolvedResourceReference> _unresolvedReferences = [];

    internal ResourceRelationshipContext(
        Dictionary<string, IKubernetesObject<V1ObjectMeta>> resourcesByUid,
        Dictionary<ResourceKey, IKubernetesObject<V1ObjectMeta>> resourcesByKey,
        Dictionary<(string ApiGroup, string Kind), IReadOnlyList<IKubernetesObject<V1ObjectMeta>>>? resourcesByGroupAndKind = null)
    {
        _resourcesByUid = resourcesByUid;
        _resourcesByKey = resourcesByKey;
        _resourcesByGroupAndKind = resourcesByGroupAndKind ?? [];
    }

    public bool TryGetByUid(string? uid, out IKubernetesObject<V1ObjectMeta>? resource)
    {
        resource = null;
        return !string.IsNullOrWhiteSpace(uid) && _resourcesByUid.TryGetValue(uid, out resource);
    }

    public bool TryGet(string apiVersion, string kind, string? namespaceName, string? name, out IKubernetesObject<V1ObjectMeta>? resource)
    {
        resource = null;
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        if (_resourcesByKey.TryGetValue(new ResourceKey(apiVersion, kind, namespaceName, name), out resource))
        {
            return true;
        }

        RecordUnresolvedApiVersion(apiVersion, kind, namespaceName, name);
        return false;
    }

    public bool TryGetByGroupAndKind(string apiGroup, string kind, out IReadOnlyList<IKubernetesObject<V1ObjectMeta>> resources)
    {
        if (_resourcesByGroupAndKind.TryGetValue((apiGroup, kind), out resources!))
        {
            return true;
        }

        resources = _resourcesByGroupAndKind
            .Where(entry => string.Equals(entry.Key.Kind, kind, StringComparison.Ordinal))
            .SelectMany(entry => entry.Value)
            .ToArray();
        return resources.Count > 0;
    }

    internal bool TryGetByExactGroupAndKind(string apiGroup, string kind, out IReadOnlyList<IKubernetesObject<V1ObjectMeta>> resources)
        => _resourcesByGroupAndKind.TryGetValue((apiGroup, kind), out resources!);

    public bool TryGetByName(string apiGroup, string kind, string? namespaceName, string? name, out IKubernetesObject<V1ObjectMeta>? resource)
    {
        resource = null;
        return TryGetByGroupAndKind(apiGroup, kind, out var resources)
            && (resource = resources.FirstOrDefault(candidate =>
                string.Equals(candidate.Namespace(), namespaceName, StringComparison.Ordinal)
                && string.Equals(candidate.Name(), name, StringComparison.Ordinal))) != null;
    }

    public bool TryGetUniqueByName(string kind, string? namespaceName, string? name, out IKubernetesObject<V1ObjectMeta>? resource)
    {
        resource = null;
        if (string.IsNullOrWhiteSpace(name))
        {
            return false;
        }

        var matches = _resourcesByGroupAndKind.Values
            .SelectMany(static resources => resources)
            .Where(candidate => string.Equals(candidate.Kind, kind, StringComparison.Ordinal)
                && string.Equals(candidate.Namespace(), namespaceName, StringComparison.Ordinal)
                && string.Equals(candidate.Name(), name, StringComparison.Ordinal))
            .Take(2)
            .ToArray();
        if (matches.Length != 1)
        {
            return false;
        }

        resource = matches[0];
        return true;
    }

    public IEnumerable<IKubernetesObject<V1ObjectMeta>> SelectByLabelSelector(
        string apiGroup,
        string kind,
        V1LabelSelector? selector,
        string? namespaceName = null)
    {
        if (selector == null || !TryGetByGroupAndKind(apiGroup, kind, out var resources))
        {
            return [];
        }

        return resources.Where(resource =>
            (namespaceName == null || string.Equals(resource.Namespace(), namespaceName, StringComparison.Ordinal))
            && MatchesSelector(resource.Metadata?.Labels, selector));
    }

    public IEnumerable<IKubernetesObject<V1ObjectMeta>> SelectNamespaces(V1LabelSelector? selector)
        => SelectByLabelSelector(string.Empty, V1Namespace.KubeKind, selector);

    public void RecordUnresolved(string apiGroup, string kind, string? namespaceName, string? name, string? apiVersion = null)
    {
        if (!string.IsNullOrWhiteSpace(name))
        {
            _unresolvedReferences.Add(new UnresolvedResourceReference(apiGroup, apiVersion, kind, namespaceName, name));
        }
    }

    internal void RecordUnresolvedApiVersion(string apiVersion, string kind, string? namespaceName, string? name)
    {
        var slash = apiVersion.IndexOf('/');
        var apiGroup = slash < 0 ? string.Empty : apiVersion[..slash];
        var version = slash < 0 ? apiVersion : apiVersion[(slash + 1)..];
        RecordUnresolved(apiGroup, kind, namespaceName, name, version);
    }

    internal IReadOnlySet<UnresolvedResourceReference> UnresolvedReferences => _unresolvedReferences;

    private static bool MatchesSelector(IDictionary<string, string>? labels, V1LabelSelector selector)
    {
        labels ??= new Dictionary<string, string>();

        if (selector.MatchLabels?.Any(match => !labels.TryGetValue(match.Key, out var value) || value != match.Value) == true)
        {
            return false;
        }

        foreach (var requirement in selector.MatchExpressions ?? [])
        {
            labels.TryGetValue(requirement.Key, out var value);
            switch (requirement.OperatorProperty)
            {
                case "In" when requirement.Values?.Contains(value) != true:
                case "NotIn" when requirement.Values?.Contains(value) == true:
                case "Exists" when value == null:
                case "DoesNotExist" when value != null:
                    return false;
            }
        }

        return true;
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
            new Providers.CrossplaneUsageRelationshipProvider(),
            new Providers.ProviderConfigUsageRelationshipProvider(),
            new Providers.GatewayApiRelationshipProvider(),
            new Providers.IngressRelationshipProvider(),
            new Providers.EndpointSliceRelationshipProvider(),
            new Providers.SelectorRelationshipProvider(),
            new Providers.ReferencedResourceRelationshipProvider(),
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
        foreach (var resource in resources)
        {
            var namespaceName = resource.Namespace();
            if (hideNoise && (resource is Corev1Event
                || resource is V1ReplicaSet replicaSet && replicaSet.Status?.Replicas == 0
                || resource is V1Pod pod && pod.Status?.Phase == "Succeeded"))
            {
                continue;
            }

            visible.Add(resource);
        }

        Dictionary<string, IKubernetesObject<V1ObjectMeta>> byUid = new(StringComparer.Ordinal);
        Dictionary<ResourceKey, IKubernetesObject<V1ObjectMeta>> byKey = [];
        Dictionary<(string ApiGroup, string Kind), List<IKubernetesObject<V1ObjectMeta>>> byGroupAndKind = [];
        foreach (var resource in visible)
        {
            var uid = resource.Uid();
            if (!string.IsNullOrWhiteSpace(uid))
            {
                byUid.TryAdd(uid, resource);
            }

            byKey.TryAdd(new(resource.ApiVersion ?? string.Empty, resource.Kind ?? string.Empty, resource.Namespace(), resource.Name() ?? string.Empty), resource);
            var apiGroup = GetApiGroup(resource.ApiVersion);
            if (!byGroupAndKind.TryGetValue((apiGroup, resource.Kind ?? string.Empty), out var groupKindResources))
            {
                groupKindResources = [];
                byGroupAndKind.Add((apiGroup, resource.Kind ?? string.Empty), groupKindResources);
            }

            groupKindResources.Add(resource);
        }

        ResourceRelationshipContext context = new(
            byUid,
            byKey,
            byGroupAndKind.ToDictionary(x => x.Key, x => (IReadOnlyList<IKubernetesObject<V1ObjectMeta>>)x.Value));
        HashSet<ResourceSeedPrerequisite> seedPrerequisites = [.. _providers.SelectMany(provider => provider.SeedPrerequisites)];
        HashSet<ResourceRelationship> relationshipSet = [];
        foreach (var resource in visible)
        {
            foreach (var provider in _providers)
            {
                provider.AddRelationships(resource, context, relationshipSet);
            }
        }

        var relationships = SimplifyRelationships(relationshipSet);
        if (selectedNamespaces.Count == 0)
        {
            return new ResourceRelationshipGraph(visible, relationships, context.UnresolvedReferences, seedPrerequisites);
        }

        HashSet<ResourceIdentity> included = [];
        foreach (var resource in visible)
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
        Dictionary<ResourceIdentity, List<ResourceRelationship>> targetsBySource = [];
        Dictionary<ResourceIdentity, List<ResourceRelationship>> relationshipsByTarget = [];
        foreach (var relationship in relationships)
        {
            targetsBySource.TryAdd(relationship.Source, []);
            targetsBySource[relationship.Source].Add(relationship);
            relationshipsByTarget.TryAdd(relationship.Target, []);
            relationshipsByTarget[relationship.Target].Add(relationship);

            if (relationship.Kind == ResourceRelationshipKind.Owner
                && string.IsNullOrEmpty(relationship.Source.Namespace)
                && string.IsNullOrEmpty(relationship.Target.Namespace)
                && (included.Contains(relationship.Source) || included.Contains(relationship.Target)))
            {
                included.Add(relationship.Source);
                included.Add(relationship.Target);
            }
        }

        do
        {
            changed = false;
            foreach (var source in included.ToArray())
            {
                if (!targetsBySource.TryGetValue(source, out var outgoingRelationships))
                {
                    continue;
                }

                foreach (var relationship in outgoingRelationships)
                {
                    var target = relationship.Target;
                    if (CanTraverse(relationship, source, target) && included.Add(target))
                    {
                        changed = true;
                    }
                }
            }

            foreach (var target in included.ToArray())
            {
                if (!relationshipsByTarget.TryGetValue(target, out var incomingRelationships))
                {
                    continue;
                }

                foreach (var relationship in incomingRelationships)
                {
                    if (CanTraverseBackwards(relationship, target) && included.Add(relationship.Source))
                    {
                        changed = true;
                    }
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
        return new ResourceRelationshipGraph(visible, relationships, context.UnresolvedReferences, seedPrerequisites);
    }

    public ResourceRelationshipGraph BuildAdditionDelta(
        IEnumerable<IKubernetesObject<V1ObjectMeta>> resources,
        ResourceKey addedResource,
        IReadOnlySet<string> selectedNamespaces,
        bool hideNoise)
    {
        ArgumentNullException.ThrowIfNull(resources);
        ArgumentNullException.ThrowIfNull(selectedNamespaces);

        var graph = Build(resources, selectedNamespaces, hideNoise);
        var added = graph.Resources.FirstOrDefault(resource =>
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
            var current = parents.Dequeue();
            foreach (var relationship in graph.Relationships)
            {
                if (relationship.Target == current
                    && (current == addedIdentity || CanTraverseBackwards(relationship, current))
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
            var current = descendants.Dequeue();
            if (!visitedDescendants.Add(current) || current != addedIdentity && ancestors.Contains(current))
            {
                continue;
            }

            foreach (var relationship in graph.Relationships)
            {
                if (relationship.Source == current
                    && CanTraverse(relationship, relationship.Source, relationship.Target)
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
            graph.Relationships.Where(relationship => included.Contains(relationship.Source) && included.Contains(relationship.Target)).ToArray(),
            graph.PendingReferences,
            graph.RequiredSeedPrerequisites);
    }

    private static bool CanTraverse(ResourceRelationship relationship, ResourceIdentity source, ResourceIdentity target)
        => (!string.IsNullOrEmpty(source.Namespace) || string.IsNullOrEmpty(target.Namespace))
            && NamespacesAreCompatible(source, target)
            || relationship.Kind is ResourceRelationshipKind.Reference or ResourceRelationshipKind.GitOps;

    private static bool CanTraverseBackwards(ResourceRelationship relationship, ResourceIdentity target)
        => (NamespacesAreCompatible(relationship.Source, target)
            && (!string.IsNullOrEmpty(target.Namespace) || string.IsNullOrEmpty(relationship.Source.Namespace))
            && relationship.Kind is not (ResourceRelationshipKind.Reference
                or ResourceRelationshipKind.Storage
                or ResourceRelationshipKind.Identity
                or ResourceRelationshipKind.Event))
            || relationship.Kind is ResourceRelationshipKind.GitOps;

    private static bool NamespacesAreCompatible(ResourceIdentity source, ResourceIdentity target)
        => string.IsNullOrEmpty(source.Namespace)
            || string.IsNullOrEmpty(target.Namespace)
            || string.Equals(source.Namespace, target.Namespace, StringComparison.Ordinal);

    public static IReadOnlyList<ResourceRelationship> SimplifyRelationships(IEnumerable<ResourceRelationship> relationships)
    {
        ArgumentNullException.ThrowIfNull(relationships);

        List<ResourceRelationship> unique = [];
        HashSet<(ResourceIdentity Source, ResourceIdentity Target, ResourceRelationshipKind Kind, string? Label)> seen = [];
        foreach (var relationship in relationships)
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
        foreach (var relationship in unique)
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
        foreach ((var target, var sources) in sourcesByTarget)
        {
            foreach (var source in sources)
            {
                if (!ownerChildren.TryGetValue(source, out var children))
                {
                    continue;
                }

                Queue<ResourceIdentity> queue = new(children);
                HashSet<ResourceIdentity> visited = [];
                while (queue.Count > 0)
                {
                    var descendant = queue.Dequeue();
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
                        foreach (var child in children)
                        {
                            queue.Enqueue(child);
                        }
                    }
                }
            }
        }

        Dictionary<(ResourceIdentity Source, ResourceRelationshipKind Kind, string? Label), List<ResourceIdentity>> relationshipTargets = [];
        foreach (var relationship in unique)
        {
            relationshipTargets.TryAdd((relationship.Source, relationship.Kind, relationship.Label), []);
            relationshipTargets[(relationship.Source, relationship.Kind, relationship.Label)].Add(relationship.Target);
        }

        HashSet<ResourceRelationship> transitive = [];
        foreach (var relationship in unique)
        {
            if (!relationshipTargets.TryGetValue((relationship.Source, relationship.Kind, relationship.Label), out var initialTargets))
            {
                continue;
            }

            Queue<ResourceIdentity> queue = new(initialTargets.Where(target => target != relationship.Target));
            HashSet<ResourceIdentity> visited = [];
            while (queue.Count > 0)
            {
                var current = queue.Dequeue();
                if (current == relationship.Source || !visited.Add(current))
                {
                    continue;
                }

                if (current == relationship.Target)
                {
                    transitive.Add(relationship);
                    break;
                }

                if (relationshipTargets.TryGetValue((current, relationship.Kind, relationship.Label), out var nextTargets))
                {
                    foreach (var target in nextTargets)
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

    private static string GetApiGroup(string? apiVersion)
    {
        var slash = apiVersion?.IndexOf('/') ?? -1;
        return slash < 0 ? string.Empty : apiVersion![..slash];
    }
}
