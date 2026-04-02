using k8s;
using KubeUI.Avalonia.Features.Resources.Yaml;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Kubernetes;
using YamlDotNet.Core;
using KubernetesYamlSerializer = KubeUI.Kubernetes.Serialization.KubernetesYaml;

namespace KubeUI.Avalonia.Features.Resources.Yaml;

public sealed class YamlSyntaxValidationService : IYamlValidationService
{
    public IReadOnlyList<YamlDiagnostic> Validate(string yaml, ModelCache? modelCache = null)
    {
        if (string.IsNullOrWhiteSpace(yaml))
        {
            return [];
        }

        try
        {
            KubernetesYamlSerializer.LoadAllFromString(yaml, modelCache?.TypeCache, strict: true);

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
        for (Exception? current = exception; current != null; current = current.InnerException)
        {
            if (!TryGetMarkLocation(current, "Start", out var startLine, out var startColumn))
            {
                continue;
            }

            if (!TryGetMarkLocation(current, "End", out var endLine, out var endColumn))
            {
                endLine = startLine;
                endColumn = startColumn;
            }

            location = new YamlDiagnosticLocation(startLine, startColumn, endLine, endColumn);
            return true;
        }

        location = default!;
        return false;
    }

    private static bool TryGetMarkLocation(Exception exception, string propertyName, out int line, out int column)
    {
        line = 0;
        column = 0;

        var property = exception.GetType().GetProperty(propertyName);
        var mark = property?.GetValue(exception);
        if (mark == null)
        {
            return false;
        }

        var lineProperty = mark.GetType().GetProperty(nameof(Mark.Line));
        var columnProperty = mark.GetType().GetProperty(nameof(Mark.Column));
        if (lineProperty?.GetValue(mark) is not long rawLine || columnProperty?.GetValue(mark) is not long rawColumn)
        {
            return false;
        }

        line = (int)Math.Max(1L, rawLine);
        column = (int)Math.Max(1L, rawColumn);
        return true;
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
