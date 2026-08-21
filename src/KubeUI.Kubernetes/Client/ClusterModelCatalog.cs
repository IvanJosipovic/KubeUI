using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;

namespace KubeUI.Kubernetes;

/// <summary>
/// Resolves registered Kubernetes and cluster-specific custom-resource models and documentation.
/// </summary>
public sealed class ClusterModelCatalog
{
    private readonly KubernetesModelCatalog _sharedCatalog;
    private readonly Lock _gate = new();
    private readonly Dictionary<(string Group, string Version, string Kind), GroupApiVersionKind> _customResourceKinds = [];
    private readonly Dictionary<string, GroupApiVersionKind> _customResourceKindsByDefinitionName = new(StringComparer.Ordinal);

    public KubernetesOpenApiSchemaCatalog OpenApiSchemas { get; } = new();

    /// <summary>
    /// Initializes a catalog for a cluster using the shared registered model catalog.
    /// </summary>
    /// <param name="sharedCatalog">The catalog containing registered Kubernetes models.</param>
    public ClusterModelCatalog(KubernetesModelCatalog sharedCatalog)
    {
        _sharedCatalog = sharedCatalog;
    }

    public bool Contains(GroupApiVersionKind kind)
    {
        return IsCustomResource(kind)
            || _sharedCatalog.TryGetResourceKind(kind.Group, kind.ApiVersion, kind.Kind, out _);
    }

    /// <summary>Determines whether API key belongs to a cluster custom resource.</summary>
    public bool IsCustomResource(GroupApiVersionKind kind)
    {
        lock (_gate)
        {
            return _customResourceKinds.ContainsKey(CreateLookupKey(kind));
        }
    }

    public bool TryGetResourceType(GroupApiVersionKind kind, out Type resourceType)
    {
        if (IsCustomResource(kind))
        {
            resourceType = typeof(GenericKubernetesObject);
            return true;
        }

        return _sharedCatalog.TryGetResourceType(kind, out resourceType!);
    }

    /// <summary>Registers a CLR resource model supplied by a resource configuration.</summary>
    /// <param name="resourceKind">API group, version, kind, and plural name.</param>
    /// <param name="resourceType">CLR model type used for the resource.</param>
    public void RegisterResource(GroupApiVersionKind resourceKind, Type resourceType)
    {
        _sharedCatalog.Register(resourceKind, resourceType);
    }

    public bool TryGetResourceKind(string apiVersion, string kind, out GroupApiVersionKind resourceKind)
    {
        var lookupKey = CreateLookupKey(apiVersion, kind);
        lock (_gate)
        {
            if (_customResourceKinds.TryGetValue(lookupKey, out resourceKind))
            {
                return true;
            }
        }

        var separator = apiVersion.IndexOf('/');
        var group = separator < 0 ? string.Empty : apiVersion[..separator];
        var version = separator < 0 ? apiVersion : apiVersion[(separator + 1)..];
        return _sharedCatalog.TryGetResourceKind(group, version, kind, out resourceKind);
    }

    /// <summary>Resolves a resource API key from its payload without inspecting its CLR type.</summary>
    /// <param name="resource">Resource whose API version and kind should be resolved.</param>
    /// <param name="resourceKind">Resolved resource API key.</param>
    /// <returns><see langword="true"/> when the resource API key is registered.</returns>
    public bool TryGetResourceKind(
        IKubernetesObject<V1ObjectMeta> resource,
        out GroupApiVersionKind resourceKind)
    {
        ArgumentNullException.ThrowIfNull(resource);

        return TryGetResourceKind(resource.ApiVersion ?? string.Empty, resource.Kind ?? string.Empty, out resourceKind);
    }

    public void RegisterOpenApiSchema(Microsoft.OpenApi.OpenApiDocument document)
    {
        OpenApiSchemas.Register(document);
    }

    public void RegisterCustomResourceDefinition(GroupApiVersionKind kind)
    {
        lock (_gate)
        {
            _customResourceKinds[CreateLookupKey(kind)] = kind;
        }
    }

    public GroupApiVersionKind? RegisterCustomResourceDefinition(string definitionName, GroupApiVersionKind kind)
    {
        lock (_gate)
        {
            GroupApiVersionKind? previous = null;
            if (_customResourceKindsByDefinitionName.TryGetValue(definitionName, out var previousKind)
                && previousKind != kind)
            {
                _customResourceKinds.Remove(CreateLookupKey(previousKind));
                previous = previousKind;
            }

            _customResourceKindsByDefinitionName[definitionName] = kind;
            _customResourceKinds[CreateLookupKey(kind)] = kind;
            return previous;
        }
    }

    public bool RemoveCustomResourceDefinition(GroupApiVersionKind kind)
    {
        lock (_gate)
        {
            return _customResourceKinds.Remove(CreateLookupKey(kind));
        }
    }

    public GroupApiVersionKind? RemoveCustomResourceDefinition(string definitionName)
    {
        lock (_gate)
        {
            if (!_customResourceKindsByDefinitionName.Remove(definitionName, out var kind))
            {
                return null;
            }

            _customResourceKinds.Remove(CreateLookupKey(kind));
            return kind;
        }
    }

    /// <summary>Removes all registered custom-resource keys.</summary>
    public void RemoveAllCustomResourceDefinitions()
    {
        lock (_gate)
        {
            _customResourceKinds.Clear();
            _customResourceKindsByDefinitionName.Clear();
        }
    }

    private static (string Group, string Version, string Kind) CreateLookupKey(GroupApiVersionKind kind)
        => (kind.Group, kind.ApiVersion, kind.Kind);

    private static (string Group, string Version, string Kind) CreateLookupKey(string apiVersion, string kind)
    {
        var separator = apiVersion.IndexOf('/');
        return separator < 0
            ? (string.Empty, apiVersion, kind)
            : (apiVersion[..separator], apiVersion[(separator + 1)..], kind);
    }

}
