using System.Collections.Frozen;
using k8s;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Kubernetes;
using KubernetesClient.Informer.Client;
using YamlDotNet.Core;
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
            var meaningfulException = Utilities.GetMeaningfulException(ex);
            if (meaningfulException is KeyNotFoundException)
            {
                return CreateUnknownTypeDiagnostic(yaml);
            }

            if (meaningfulException is YamlException yamlException)
            {
                return CreateYamlExceptionDiagnostic(yaml, ex, yamlException);
            }

            if (TryGetExceptionLocation(ex, out var location))
            {
                return
                [
                    new YamlDiagnostic(
                        location.StartLine,
                        location.StartColumn,
                        location.EndLine,
                        location.EndColumn,
                        Utilities.GetMeaningfulExceptionMessage(ex),
                        YamlDiagnosticSeverity.Error),
                ];
            }

            return
            [
                new YamlDiagnostic(
                    1,
                    1,
                    1,
                    1,
                    Utilities.GetMeaningfulExceptionMessage(ex),
                    YamlDiagnosticSeverity.Error),
            ];
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

    private static IReadOnlyList<YamlDiagnostic> CreateUnknownTypeDiagnostic(string yaml)
    {
        try
        {
            var manifest = KubernetesYamlSerializer.Deserialize<KubernetesObject>(yaml);
            var location = FindHeaderLocation(yaml, "kind") ?? FindHeaderLocation(yaml, "apiVersion");
            return
            [
                new YamlDiagnostic(
                    location?.StartLine ?? 1,
                    location?.StartColumn ?? 1,
                    location?.EndLine ?? 1,
                    location?.EndColumn ?? 1,
                    $"Unable to resolve Kubernetes type for {manifest.ApiVersion}/{manifest.Kind}.",
                    YamlDiagnosticSeverity.Error),
            ];
        }
        catch (Exception ex)
        {
            var meaningfulException = Utilities.GetMeaningfulException(ex);
            if (meaningfulException is YamlException yamlException)
            {
                return CreateYamlExceptionDiagnostic(yaml, ex, yamlException);
            }

            if (TryGetExceptionLocation(ex, out var location))
            {
                return
                [
                    new YamlDiagnostic(
                        location.StartLine,
                        location.StartColumn,
                        location.EndLine,
                        location.EndColumn,
                        Utilities.GetMeaningfulExceptionMessage(ex),
                        YamlDiagnosticSeverity.Error),
                ];
            }

            return
            [
                new YamlDiagnostic(
                    1,
                    1,
                    1,
                    1,
                    Utilities.GetMeaningfulExceptionMessage(ex),
                    YamlDiagnosticSeverity.Error),
            ];
        }
    }

    private static IReadOnlyList<YamlDiagnostic> CreateYamlExceptionDiagnostic(
        string yaml,
        Exception sourceException,
        YamlException yamlException)
    {
        var message = Utilities.GetMeaningfulExceptionMessage(sourceException);
        if (message.Contains("duplicate key", StringComparison.OrdinalIgnoreCase)
            && TryGetDuplicateKeyLocation(yaml, yamlException.Start.Line, out var duplicateLocation))
        {
            return
            [
                new YamlDiagnostic(
                    duplicateLocation.StartLine,
                    duplicateLocation.StartColumn,
                    duplicateLocation.EndLine,
                    duplicateLocation.EndColumn,
                    message,
                    YamlDiagnosticSeverity.Error),
            ];
        }

        return
        [
            new YamlDiagnostic(
                (int)Math.Max(1L, yamlException.Start.Line),
                (int)Math.Max(1L, yamlException.Start.Column),
                (int)Math.Max(1L, yamlException.End.Line),
                (int)Math.Max(1L, yamlException.End.Column),
                message,
                YamlDiagnosticSeverity.Error),
        ];
    }

    private static bool TryGetDuplicateKeyLocation(string yaml, long firstKeyLine, out YamlDiagnosticLocation location)
    {
        var lines = yaml.ReplaceLineEndings("\n").Split('\n');
        var firstIndex = (int)firstKeyLine - 1;
        if (firstIndex < 0 || firstIndex >= lines.Length || !TryGetMappingKey(lines[firstIndex], out var key, out var indent))
        {
            location = default!;
            return false;
        }

        for (var i = firstIndex + 1; i < lines.Length; i++)
        {
            if (!TryGetMappingKey(lines[i], out var candidate, out var candidateIndent))
            {
                continue;
            }

            if (candidateIndent < indent)
            {
                break;
            }

            if (candidateIndent != indent
                || !string.Equals(candidate, key, StringComparison.Ordinal))
            {
                continue;
            }

            location = new YamlDiagnosticLocation(
                i + 1,
                indent + 1,
                i + 1,
                indent + key.Length + 1);
            return true;
        }

        location = default!;
        return false;
    }

    private static bool TryGetMappingKey(string line, out string key, out int indent)
    {
        var trimmed = line.TrimStart();
        indent = line.Length - trimmed.Length;
        var separator = trimmed.IndexOf(':');
        if (separator <= 0 || trimmed.StartsWith('#'))
        {
            key = string.Empty;
            return false;
        }

        key = trimmed[..separator].Trim();
        return key.Length > 0;
    }

    private static bool TryGetExceptionLocation(Exception exception, out YamlDiagnosticLocation location)
    {
        for (var current = exception; current != null; current = current.InnerException)
        {
            if (current is not YamlException yamlException)
            {
                continue;
            }

            location = new YamlDiagnosticLocation(
                (int)Math.Max(1L, yamlException.Start.Line),
                (int)Math.Max(1L, yamlException.Start.Column),
                (int)Math.Max(1L, yamlException.End.Line),
                (int)Math.Max(1L, yamlException.End.Column));
            return true;
        }

        location = default!;
        return false;
    }

    private static YamlDiagnosticLocation? FindHeaderLocation(string yaml, string key)
    {
        var lines = yaml.ReplaceLineEndings("\n").Split('\n');
        for (var i = 0; i < lines.Length; i++)
        {
            var line = lines[i];
            if (string.IsNullOrWhiteSpace(line))
            {
                continue;
            }

            var trimmed = line.TrimStart();
            if (trimmed.StartsWith('#'))
            {
                continue;
            }

            var indent = line.Length - trimmed.Length;
            if (indent != 0)
            {
                continue;
            }

            if (!trimmed.StartsWith(key + ":", StringComparison.Ordinal))
            {
                continue;
            }

            return new YamlDiagnosticLocation(
                i + 1,
                indent + 1,
                i + 1,
                indent + key.Length + 1);
        }

        return null;
    }

    private sealed record YamlDiagnosticLocation(int StartLine, int StartColumn, int EndLine, int EndColumn);
}
