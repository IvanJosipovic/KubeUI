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
            var typeMap = _sharedCatalog.GetYamlTypeMap();
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
                return CreateYamlExceptionDiagnostic(ex, yamlException);
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
                return CreateYamlExceptionDiagnostic(ex, yamlException);
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

    private static IReadOnlyList<YamlDiagnostic> CreateYamlExceptionDiagnostic(Exception sourceException, YamlException yamlException)
    {
        return
        [
            new YamlDiagnostic(
                (int)Math.Max(1L, yamlException.Start.Line),
                (int)Math.Max(1L, yamlException.Start.Column),
                (int)Math.Max(1L, yamlException.End.Line),
                (int)Math.Max(1L, yamlException.End.Column),
                Utilities.GetMeaningfulExceptionMessage(sourceException),
                YamlDiagnosticSeverity.Error),
        ];
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
