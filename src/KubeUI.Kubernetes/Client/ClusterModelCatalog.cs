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
    private readonly Dictionary<GroupApiVersionKind, Func<bool, CancellationToken, Task>> _seeders = [];

    /// <summary>Gets the cluster's OpenAPI schemas.</summary>
    public KubernetesOpenApiSchemaCatalog OpenApiSchemas { get; } = new();

    /// <summary>
    /// Initializes a catalog for a cluster using the shared registered model catalog.
    /// </summary>
    /// <param name="sharedCatalog">The catalog containing registered Kubernetes models.</param>
    public ClusterModelCatalog(KubernetesModelCatalog sharedCatalog)
    {
        _sharedCatalog = sharedCatalog;
    }

    /// <summary>Determines whether the catalog contains a built-in or cluster-specific resource kind.</summary>
    /// <param name="kind">The API group, version, kind, and plural name.</param>
    /// <returns><see langword="true"/> when the resource kind is registered.</returns>
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

    /// <summary>Resolves the CLR model type registered for a resource kind.</summary>
    /// <param name="kind">The resource kind to resolve.</param>
    /// <param name="resourceType">The registered CLR model type.</param>
    /// <returns><see langword="true"/> when the catalog contains the resource kind.</returns>
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
        RemoveDiscoveredResource(resourceKind);
    }

    /// <summary>Registers a CLR resource model and its typed seed operation.</summary>
    /// <param name="resourceKind">API group, version, kind, and plural name.</param>
    /// <param name="resourceType">CLR model type used for the resource.</param>
    /// <param name="seeder">Operation that seeds the model and optionally waits for readiness.</param>
    public void RegisterResource(GroupApiVersionKind resourceKind, Type resourceType, Func<bool, CancellationToken, Task> seeder)
    {
        ArgumentNullException.ThrowIfNull(seeder);
        RegisterResource(resourceKind, resourceType);
        lock (_gate)
        {
            _seeders[resourceKind] = seeder;
        }
    }

    /// <summary>Gets the typed seeder registered for a resource kind.</summary>
    /// <param name="resourceKind">The resource kind to resolve.</param>
    /// <param name="seeder">The registered seeding operation.</param>
    /// <returns><see langword="true"/> when a typed seeder is registered.</returns>
    public bool TryGetResourceSeeder(GroupApiVersionKind resourceKind, out Func<bool, CancellationToken, Task> seeder)
    {
        lock (_gate)
        {
            return _seeders.TryGetValue(resourceKind, out seeder!);
        }
    }

    /// <summary>Resolves a resource kind from its API version and kind name.</summary>
    /// <param name="apiVersion">The API version, optionally prefixed by its group.</param>
    /// <param name="kind">The Kubernetes kind name.</param>
    /// <param name="resourceKind">The resolved resource kind.</param>
    /// <returns><see langword="true"/> when the kind is registered.</returns>
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

    /// <summary>Registers an OpenAPI document with this cluster's schema catalog.</summary>
    /// <param name="document">The document to register.</param>
    public void RegisterOpenApiSchema(Microsoft.OpenApi.OpenApiDocument document)
    {
        OpenApiSchemas.Register(document);
    }

    /// <summary>Registers a cluster-specific custom-resource kind.</summary>
    /// <param name="kind">The resource kind to register.</param>
    public void RegisterCustomResourceDefinition(GroupApiVersionKind kind)
    {
        lock (_gate)
        {
            _customResourceKinds[CreateLookupKey(kind)] = kind;
        }
    }

    /// <summary>Registers a custom-resource kind against its CRD name.</summary>
    /// <param name="definitionName">The CRD metadata name.</param>
    /// <param name="kind">The resource kind served by the CRD.</param>
    /// <returns>The previously registered kind when this CRD changed identity.</returns>
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

    /// <summary>Removes a cluster-specific custom-resource kind.</summary>
    /// <param name="kind">The resource kind to remove.</param>
    /// <returns><see langword="true"/> when the kind was registered.</returns>
    public bool RemoveCustomResourceDefinition(GroupApiVersionKind kind)
    {
        lock (_gate)
        {
            return _customResourceKinds.Remove(CreateLookupKey(kind));
        }
    }

    /// <summary>Removes the custom-resource kind registered for a CRD name.</summary>
    /// <param name="definitionName">The CRD metadata name.</param>
    /// <returns>The removed resource kind, or <see langword="null"/> when no kind was registered.</returns>
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

    /// <summary>Replaces dynamic resource keys from a complete API discovery snapshot.</summary>
    /// <param name="resources">Served resource endpoints from the snapshot.</param>
    /// <returns>Previously registered dynamic kinds absent from the new snapshot.</returns>
    public IReadOnlyList<GroupApiVersionKind> ReplaceDiscoveredCustomResources(
        IEnumerable<ApiDiscoveryResource> resources)
    {
        ArgumentNullException.ThrowIfNull(resources);
        var discoveredKinds = resources
            .Select(static resource => resource.Kind)
            .Where(kind => !_sharedCatalog.ContainsResourceIdentity(kind.Group, kind.Kind))
            .Distinct()
            .ToArray();

        lock (_gate)
        {
            var previousKinds = _customResourceKinds.Values.ToHashSet();
            _customResourceKinds.Clear();
            _customResourceKindsByDefinitionName.Clear();

            foreach (var kind in discoveredKinds)
            {
                _customResourceKinds[CreateLookupKey(kind)] = kind;
            }

            previousKinds.ExceptWith(discoveredKinds);
            return previousKinds.ToArray();
        }
    }

    private static (string Group, string Version, string Kind) CreateLookupKey(GroupApiVersionKind kind)
        => (kind.Group, kind.ApiVersion, kind.Kind);

    private void RemoveDiscoveredResource(GroupApiVersionKind kind)
    {
        lock (_gate)
        {
            _customResourceKinds.Remove(CreateLookupKey(kind));
            var definitionNames = _customResourceKindsByDefinitionName
                .Where(pair => pair.Value == kind)
                .Select(static pair => pair.Key)
                .ToArray();
            foreach (var definitionName in definitionNames)
            {
                _customResourceKindsByDefinitionName.Remove(definitionName);
            }
        }
    }

    private static (string Group, string Version, string Kind) CreateLookupKey(string apiVersion, string kind)
    {
        var separator = apiVersion.IndexOf('/');
        return separator < 0
            ? (string.Empty, apiVersion, kind)
            : (apiVersion[..separator], apiVersion[(separator + 1)..], kind);
    }

}
