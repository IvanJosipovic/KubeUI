using System.Collections.Frozen;
using k8s;
using KubeUI.Kubernetes;
using KubernetesClient.Informer.Client;
using KubernetesYamlSerializer = KubeUI.Kubernetes.Serialization.KubernetesYaml;

namespace KubeUI.Avalonia.Features.Resources.Yaml;

public sealed class YamlSyntaxValidationService : IYamlValidationService
{
    private readonly KubernetesModelCatalog _sharedCatalog;
    private FrozenDictionary<string, Type>? _typeMap;
    private long _typeMapVersion = -1;

    public YamlSyntaxValidationService(KubernetesModelCatalog sharedCatalog)
    {
        _sharedCatalog = sharedCatalog;
    }

    /// <summary>
    /// Validates YAML using the optional cluster model catalog for custom-resource resolution.
    /// </summary>
    /// <param name="yaml">The YAML document to validate.</param>
    /// <param name="modelCatalog">An optional cluster model catalog used for custom-resource types.</param>
    /// <returns>The validation diagnostics; an empty list indicates valid YAML.</returns>
    public IReadOnlyList<YamlDiagnostic> Validate(string yaml, ClusterModelCatalog? modelCatalog = null)
    {
        if (string.IsNullOrWhiteSpace(yaml))
        {
            return [];
        }

        try
        {
            var typeMap = GetTypeMap();
            KubernetesYamlSerializer.LoadAllFromString(
                yaml,
                key => typeMap.TryGetValue(key, out var type)
                    ? type
                    : ResolveCustomResourceType(key, modelCatalog),
                strict: true);

            return [];
        }
        catch (Exception ex)
        {
            if (YamlDotNetDiagnosticFactory.IsUnknownTypeException(ex))
            {
                return YamlDotNetDiagnosticFactory.CreateUnknownTypeDiagnostic(yaml);
            }

            return YamlDotNetDiagnosticFactory.Create(yaml, ex);
        }
    }

    private FrozenDictionary<string, Type> GetTypeMap()
    {
        var version = _sharedCatalog.Version;
        var typeMap = Volatile.Read(ref _typeMap);
        if (typeMap is not null && Volatile.Read(ref _typeMapVersion) == version)
        {
            return typeMap;
        }

        typeMap = _sharedCatalog.GetYamlTypeMap();
        Volatile.Write(ref _typeMap, typeMap);
        Volatile.Write(ref _typeMapVersion, version);
        return typeMap;
    }

    private static Type ResolveCustomResourceType(string key, ClusterModelCatalog? modelCatalog)
    {
        if (modelCatalog is null)
        {
            throw new KeyNotFoundException(key);
        }

        var separator = key.LastIndexOf('/');
        if (separator > 0
            && modelCatalog.TryGetResourceKind(key[..separator], key[(separator + 1)..], out var resourceKind)
            && modelCatalog.IsCustomResource(resourceKind))
        {
            return typeof(GenericKubernetesObject);
        }

        throw new KeyNotFoundException(key);
    }

}
