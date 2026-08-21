using System.Collections.Frozen;
using System.Collections.Generic;
using KubernetesClient.Informer.Client;

namespace KubeUI.Kubernetes;

/// <summary>
/// Runtime registry of resource models supplied by resource configurations.
/// </summary>
public sealed class KubernetesModelCatalog
{
    private readonly Dictionary<GroupApiVersionKind, Type> _types = [];
    private readonly object _sync = new();

    /// <summary>
    /// Gets a snapshot of registered models keyed by YAML API version and kind.
    /// </summary>
    /// <returns>The registered YAML model map.</returns>
    public FrozenDictionary<string, Type> GetYamlTypeMap()
    {
        lock (_sync)
        {
            return _types.ToFrozenDictionary(
                pair => CreateYamlKey(pair.Key),
                pair => pair.Value,
                StringComparer.Ordinal);
        }
    }

    /// <summary>
    /// Registers a resource model for an API kind.
    /// </summary>
    /// <param name="resourceKind">API group, version, kind, and plural name.</param>
    /// <param name="resourceType">CLR model type used for the resource.</param>
    public void Register(GroupApiVersionKind resourceKind, Type resourceType)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(resourceKind.Kind);
        ArgumentNullException.ThrowIfNull(resourceType);

        lock (_sync)
        {
            _types[resourceKind] = resourceType;
        }
    }

    /// <summary>
    /// Resolves a registered resource API kind.
    /// </summary>
    public bool TryGetResourceKind(string group, string version, string kind, out GroupApiVersionKind resourceKind)
    {
        lock (_sync)
        {
            foreach (var candidate in _types.Keys)
            {
                if (string.Equals(candidate.Group, group, StringComparison.Ordinal)
                    && string.Equals(candidate.ApiVersion, version, StringComparison.Ordinal)
                    && string.Equals(candidate.Kind, kind, StringComparison.Ordinal))
                {
                    resourceKind = candidate;
                    return true;
                }
            }
        }

        resourceKind = default;
        return false;
    }

    public bool TryGetResourceType(GroupApiVersionKind resourceKind, out Type resourceType)
    {
        lock (_sync)
        {
            return _types.TryGetValue(resourceKind, out resourceType!);
        }
    }

    private static string CreateYamlKey(GroupApiVersionKind key)
    {
        var groupPrefix = string.IsNullOrEmpty(key.Group) ? string.Empty : $"{key.Group}/";
        return $"{groupPrefix}{key.ApiVersion}/{key.Kind}";
    }
}
