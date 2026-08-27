using k8s;
using k8s.Models;
using KubeUI.Kubernetes.Resources.Relationships;

namespace KubeUI.Avalonia.Features.Resources.Visualization;

/// <summary>Owns the keyed resource snapshot and owner-reference index used by visualization builds.</summary>
internal sealed class VisualizationResourceStore
{
    private readonly Dictionary<ResourceKey, IKubernetesObject<V1ObjectMeta>> _resources = [];
    private readonly Dictionary<string, HashSet<ResourceKey>> _resourcesByOwnerUid = new(StringComparer.Ordinal);

    public int Count => _resources.Count;

    public IReadOnlyList<IKubernetesObject<V1ObjectMeta>> Snapshot() => _resources.Values.ToArray();

    public bool TryGet(ResourceKey key, out IKubernetesObject<V1ObjectMeta>? resource)
        => _resources.TryGetValue(key, out resource);

    public bool HasOwnerReferencesTo(IKubernetesObject<V1ObjectMeta> resource)
    {
        var uid = resource.Uid();
        return !string.IsNullOrWhiteSpace(uid) && _resourcesByOwnerUid.ContainsKey(uid);
    }

    public void Replace(
        IReadOnlyDictionary<ResourceKey, IKubernetesObject<V1ObjectMeta>> resources,
        IReadOnlyDictionary<string, HashSet<ResourceKey>> resourcesByOwnerUid)
    {
        _resources.Clear();
        foreach (var pair in resources)
        {
            _resources.Add(pair.Key, pair.Value);
        }

        _resourcesByOwnerUid.Clear();
        foreach (var pair in resourcesByOwnerUid)
        {
            _resourcesByOwnerUid.Add(pair.Key, [.. pair.Value]);
        }
    }

    public bool Remove(ResourceKey key, IKubernetesObject<V1ObjectMeta> resource)
    {
        RemoveOwnerReferenceIndex(_resources.TryGetValue(key, out var stored) ? stored : resource, key);
        return _resources.Remove(key);
    }

    public void Upsert(ResourceKey key, IKubernetesObject<V1ObjectMeta> resource)
    {
        if (_resources.TryGetValue(key, out var previous))
        {
            RemoveOwnerReferenceIndex(previous, key);
        }

        _resources[key] = resource;
        AddOwnerReferenceIndex(resource, key);
    }

    public void Clear()
    {
        _resources.Clear();
        _resourcesByOwnerUid.Clear();
    }

    private void AddOwnerReferenceIndex(IKubernetesObject<V1ObjectMeta> resource, ResourceKey key)
    {
        foreach (var owner in resource.Metadata?.OwnerReferences ?? [])
        {
            if (string.IsNullOrWhiteSpace(owner.Uid))
            {
                continue;
            }

            if (!_resourcesByOwnerUid.TryGetValue(owner.Uid, out var resources))
            {
                resources = [];
                _resourcesByOwnerUid.Add(owner.Uid, resources);
            }

            resources.Add(key);
        }
    }

    private void RemoveOwnerReferenceIndex(IKubernetesObject<V1ObjectMeta> resource, ResourceKey key)
    {
        foreach (var owner in resource.Metadata?.OwnerReferences ?? [])
        {
            if (string.IsNullOrWhiteSpace(owner.Uid)
                || !_resourcesByOwnerUid.TryGetValue(owner.Uid, out var resources))
            {
                continue;
            }

            resources.Remove(key);
            if (resources.Count == 0)
            {
                _resourcesByOwnerUid.Remove(owner.Uid);
            }
        }
    }
}
