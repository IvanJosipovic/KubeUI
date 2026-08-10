using System.Collections.Frozen;
using System.Reflection;
using System.Xml;
using KubernetesCRDModelGen;
using k8s.Models;
using KubernetesClient.Informer.Client;

namespace KubeUI.Kubernetes;

/// <summary>
/// Resolves built-in Kubernetes and cluster-specific custom-resource models and documentation.
/// </summary>
public sealed class ClusterModelCatalog
{
    private readonly KubernetesModelCatalog _sharedCatalog;
    private readonly Lock _gate = new();
    private FrozenDictionary<string, Type>? _yamlTypeMap;
    private FrozenDictionary<string, Type>? _crdYamlTypeMap;

    /// <summary>
    /// Gets the catalog containing models generated for the connected cluster's CRDs.
    /// </summary>
    public ClusterCrdModelCatalog CrdModels { get; }

    /// <summary>
    /// Gets the shared catalog containing built-in Kubernetes models and documentation.
    /// </summary>
    public KubernetesModelCatalog SharedCatalog => _sharedCatalog;

    /// <summary>
    /// Initializes a catalog for a cluster using the shared built-in model catalog.
    /// </summary>
    /// <param name="sharedCatalog">The catalog containing built-in Kubernetes models.</param>
    public ClusterModelCatalog(KubernetesModelCatalog sharedCatalog)
    {
        _sharedCatalog = sharedCatalog;
        CrdModels = new ClusterCrdModelCatalog();
    }

    /// <summary>
    /// Resolves a resource model by group, version, and kind, preferring a cluster CRD model.
    /// </summary>
    /// <param name="type">The group, version, and kind to resolve.</param>
    /// <returns>The matching model type, or <see langword="null"/> when no model is registered.</returns>
    public Type? GetResourceType(GroupApiVersionKind type)
    {
        return CrdModels.GetResourceType(type)
            ?? _sharedCatalog.GetResourceType(type);
    }

    /// <summary>
    /// Resolves a resource model by group, version, and kind, preferring a cluster CRD model.
    /// </summary>
    /// <param name="group">The API group, or an empty string for core resources.</param>
    /// <param name="version">The API version.</param>
    /// <param name="kind">The resource kind.</param>
    /// <returns>The matching model type, or <see langword="null"/> when no model is registered.</returns>
    public Type? GetResourceType(string group, string version, string kind)
    {
        return GetResourceType(new GroupApiVersionKind(group, version, kind, string.Empty));
    }

    /// <summary>
    /// Gets the YAML type mappings for models registered in the cluster CRD catalog.
    /// </summary>
    /// <returns>A map from YAML resource keys to model types.</returns>
    public FrozenDictionary<string, Type> GetYamlTypeMap()
    {
        lock (_gate)
        {
            var crdYamlTypeMap = CrdModels.GetYamlTypeMap();
            if (_yamlTypeMap is not null && ReferenceEquals(_crdYamlTypeMap, crdYamlTypeMap))
            {
                return _yamlTypeMap;
            }

            var map = new Dictionary<string, Type>(_sharedCatalog.GetYamlTypeMap(), StringComparer.Ordinal);
            foreach (var pair in crdYamlTypeMap)
            {
                map[pair.Key] = pair.Value;
            }

            _crdYamlTypeMap = crdYamlTypeMap;
            return _yamlTypeMap = map.ToFrozenDictionary(StringComparer.Ordinal);
        }
    }

    /// <summary>
    /// Replaces the generated model associated with a custom-resource definition.
    /// </summary>
    /// <param name="crd">The custom-resource definition that owns the generated model.</param>
    /// <param name="assembly">The generated model assembly.</param>
    /// <param name="xmlDocument">The generated model documentation.</param>
    /// <param name="unloadHandle">An optional handle used to unload the previous generated assembly.</param>
    /// <returns>The previous and current model types associated with the CRD.</returns>
    public (Type? previousType, Type? currentType) ReplaceCustomResourceDefinition(
        V1CustomResourceDefinition crd,
        Assembly assembly,
        XmlDocument xmlDocument,
        GeneratedAssemblyUnloadHandle? unloadHandle = null)
    {
        return CrdModels.ReplaceCustomResourceDefinition(crd, assembly, xmlDocument, unloadHandle);
    }

    /// <summary>
    /// Removes the generated model associated with a custom-resource definition.
    /// </summary>
    /// <param name="crd">The custom-resource definition whose generated model should be removed.</param>
    /// <returns>The removed model type, or <see langword="null"/> when none was registered.</returns>
    public Type? RemoveCustomResourceDefinition(V1CustomResourceDefinition crd)
    {
        return CrdModels.RemoveCustomResourceDefinition(crd);
    }

    /// <summary>
    /// Removes all generated custom-resource models from the cluster catalog.
    /// </summary>
    public void RemoveAllCustomResourceDefinitions()
    {
        CrdModels.RemoveAllCustomResourceDefinitions();
    }

    /// <summary>
    /// Determines whether a generated model is registered for a custom-resource definition.
    /// </summary>
    /// <param name="crd">The custom-resource definition to check.</param>
    /// <returns><see langword="true"/> when a generated model is registered; otherwise, <see langword="false"/>.</returns>
    public bool CheckIfCRDExists(V1CustomResourceDefinition crd)
    {
        return CrdModels.CheckIfCRDExists(crd);
    }

    /// <summary>
    /// Gets XML documentation for a reflected member, preferring cluster-generated documentation.
    /// </summary>
    /// <param name="memberInfo">The member whose documentation should be resolved.</param>
    /// <returns>The documentation element, or <see langword="null"/> when unavailable.</returns>
    public XmlElement? GetDocumentation(MemberInfo memberInfo)
    {
        return CrdModels.GetDocumentation(memberInfo) ?? _sharedCatalog.GetDocumentation(memberInfo);
    }

    /// <summary>
    /// Gets XML documentation for a reflected method, preferring cluster-generated documentation.
    /// </summary>
    /// <param name="methodInfo">The method whose documentation should be resolved.</param>
    /// <returns>The documentation element, or <see langword="null"/> when unavailable.</returns>
    public XmlElement? GetDocumentation(MethodInfo methodInfo)
    {
        return CrdModels.GetDocumentation(methodInfo) ?? _sharedCatalog.GetDocumentation(methodInfo);
    }

    /// <summary>
    /// Gets XML documentation for a reflected type, preferring cluster-generated documentation.
    /// </summary>
    /// <param name="type">The type whose documentation should be resolved.</param>
    /// <returns>The documentation element, or <see langword="null"/> when unavailable.</returns>
    public XmlElement? GetDocumentation(Type type)
    {
        return CrdModels.GetDocumentation(type) ?? _sharedCatalog.GetDocumentation(type);
    }
}
