using k8s;
using k8s.Models;
using KubeUI.Kubernetes.Resources.Relationships;

namespace KubeUI.Features.Resources.Visualization;

/// <summary>Serializes incremental resource updates and prepares merged visualization graphs.</summary>
internal sealed class VisualizationIncrementalUpdateCoordinator : IDisposable
{
    internal sealed record PendingChange(
        ResourceKey Key,
        IKubernetesObject<V1ObjectMeta> Resource,
        bool ReplaceExisting);

    private readonly Lock _gate = new();
    private readonly VisualizationResourceStore _resourceStore;
    private readonly IResourceRelationshipBuilder _relationshipBuilder;
    private readonly Dictionary<ResourceKey, PendingChange> _pendingChanges = [];
    private bool _isRunning;
    private bool _disposed;

    public VisualizationIncrementalUpdateCoordinator(
        VisualizationResourceStore resourceStore,
        IResourceRelationshipBuilder relationshipBuilder)
    {
        _resourceStore = resourceStore;
        _relationshipBuilder = relationshipBuilder;
    }

    public bool IsRunning
    {
        get
        {
            lock (_gate)
            {
                return _isRunning;
            }
        }
    }

    public int PendingCount
    {
        get
        {
            lock (_gate)
            {
                return _pendingChanges.Count;
            }
        }
    }

    public bool TryBegin()
    {
        lock (_gate)
        {
            if (_disposed || _isRunning)
            {
                return false;
            }

            _isRunning = true;
            return true;
        }
    }

    public void Enqueue(ResourceKey key, IKubernetesObject<V1ObjectMeta> resource, bool replaceExisting)
    {
        lock (_gate)
        {
            if (!_disposed)
            {
                _pendingChanges[key] = new PendingChange(key, resource, replaceExisting);
            }
        }
    }

    public bool TryTakePending(out PendingChange? change)
    {
        lock (_gate)
        {
            if (_disposed || _pendingChanges.Count == 0)
            {
                change = null;
                return false;
            }

            var first = _pendingChanges.First();
            _pendingChanges.Remove(first.Key);
            change = first.Value;
            return true;
        }
    }

    public void ClearPending()
    {
        lock (_gate)
        {
            _pendingChanges.Clear();
        }
    }

    public void Complete()
    {
        lock (_gate)
        {
            _isRunning = false;
        }
    }

    public Task<ResourceRelationshipGraph> BuildAdditionDeltaAsync(
        ResourceKey key,
        IReadOnlySet<string> namespaces,
        bool hideNoise,
        CancellationToken cancellationToken)
    {
        var source = _resourceStore.Snapshot();
        return Task.Run(
            () => _relationshipBuilder.BuildAdditionDelta(source, key, namespaces, hideNoise),
            cancellationToken);
    }

    public ResourceRelationshipGraph? MergeAdditionDelta(
        ResourceRelationshipGraph current,
        ResourceRelationshipGraph delta,
        IKubernetesObject<V1ObjectMeta> resource,
        bool replaceExisting,
        IReadOnlySet<string> namespaces,
        IKubernetesObject<V1ObjectMeta>? rootResource,
        ResourceAnnotationSelector? selector,
        out bool requiresRebuild)
    {
        requiresRebuild = false;
        var identity = GetIdentity(resource);
        var currentResources = replaceExisting
            ? current.Resources.Where(item => GetIdentity(item) != identity).ToArray()
            : current.Resources;
        var currentRelationships = replaceExisting
            ? current.Relationships.Where(relationship => relationship.Source != identity && relationship.Target != identity).ToArray()
            : current.Relationships;
        var currentIdentities = currentResources.Select(GetIdentity).ToHashSet();

        if (rootResource is not null
            && identity != GetIdentity(rootResource)
            && !delta.Relationships.Any(relationship =>
                currentIdentities.Contains(relationship.Source) || currentIdentities.Contains(relationship.Target)))
        {
            requiresRebuild = true;
            return null;
        }

        if (rootResource is null && selector is null)
        {
            delta = ResourceGraphProjection.ToSelectedNamespacesIncremental(delta, namespaces, currentIdentities);
            if (!replaceExisting && delta.Resources.Count == 0)
            {
                return null;
            }
        }

        IReadOnlyList<IKubernetesObject<V1ObjectMeta>> resources = currentResources
            .Concat(delta.Resources)
            .GroupBy(GetIdentity)
            .Select(group => group.First())
            .ToArray();
        var relationships = ResourceRelationshipBuilder.SimplifyRelationships(
            currentRelationships.Concat(delta.Relationships));
        ResourceRelationshipGraph merged = new(
            resources,
            relationships,
            current.PendingReferences.Union(delta.PendingReferences).ToHashSet(),
            current.RequiredSeedPrerequisites.Union(delta.RequiredSeedPrerequisites).ToHashSet());

        return selector is { } resourceSelector
            ? ResourceGraphProjection.ToSelector(merged, resourceSelector)
            : rootResource is { } root
                ? ResourceGraphProjection.ToRootResource(merged, root)
                : ResourceGraphProjection.ToSelectedNamespaces(merged, namespaces);
    }

    public void Dispose()
    {
        lock (_gate)
        {
            if (_disposed)
            {
                return;
            }

            _disposed = true;
            _pendingChanges.Clear();
        }
    }

    private static ResourceIdentity GetIdentity(IKubernetesObject<V1ObjectMeta> resource)
        => new(
            resource.ApiVersion ?? string.Empty,
            resource.Kind ?? string.Empty,
            resource.Namespace(),
            resource.Name() ?? string.Empty,
            resource.Uid());
}
