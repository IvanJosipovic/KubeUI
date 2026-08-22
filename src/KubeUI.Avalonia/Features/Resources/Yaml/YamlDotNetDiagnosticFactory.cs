using k8s;
using KubeUI.Avalonia.Infrastructure;
using KubernetesYamlSerializer = KubeUI.Kubernetes.Serialization.KubernetesYaml;
using YamlDotNet.Core;

namespace KubeUI.Avalonia.Features.Resources.Yaml;

internal static class YamlDotNetDiagnosticFactory
{
    public static bool IsUnknownTypeException(Exception exception)
    {
        return Utilities.GetMeaningfulException(exception) is KeyNotFoundException;
    }

    public static IReadOnlyList<YamlDiagnostic> Create(string yaml, Exception sourceException)
    {
        var meaningfulException = Utilities.GetMeaningfulException(sourceException);
        if (meaningfulException is YamlException yamlException)
        {
            return CreateYamlExceptionDiagnostic(yaml, sourceException, yamlException);
        }

        if (TryGetExceptionLocation(sourceException, out var location))
        {
            return [CreateDiagnostic(location, Utilities.GetMeaningfulExceptionMessage(sourceException))];
        }

        return [CreateDiagnostic(new YamlDiagnosticLocation(1, 1, 1, 1), Utilities.GetMeaningfulExceptionMessage(sourceException))];
    }

    public static IReadOnlyList<YamlDiagnostic> CreateUnknownTypeDiagnostic(string yaml)
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
            return Create(yaml, ex);
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
            return [CreateDiagnostic(duplicateLocation, message)];
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
        if (firstIndex < 0 || firstIndex >= lines.Length)
        {
            location = default!;
            return false;
        }

        for (var i = firstIndex; i < lines.Length; i++)
        {
            if (!TryGetMappingKey(lines[i], out var key, out var indent, out _))
            {
                continue;
            }

            for (var j = i + 1; j < lines.Length; j++)
            {
                if (!TryGetMappingKey(lines[j], out var candidate, out var candidateIndent, out var candidateColumn))
                {
                    continue;
                }

                if (candidateIndent < indent)
                {
                    break;
                }

                if (candidateIndent == indent && string.Equals(candidate, key, StringComparison.Ordinal))
                {
                    location = new YamlDiagnosticLocation(j + 1, candidateColumn, j + 1, candidateColumn + candidate.Length);
                    return true;
                }
            }
        }

        location = default!;
        return false;
    }

    private static bool TryGetMappingKey(string line, out string key, out int indent, out int keyColumn)
    {
        var trimmed = line.TrimStart();
        var lineIndent = line.Length - trimmed.Length;
        if (trimmed.StartsWith('-')
            && (trimmed.Length == 1 || char.IsWhiteSpace(trimmed[1])))
        {
            trimmed = trimmed[1..].TrimStart();
            keyColumn = lineIndent + line[lineIndent..].IndexOf(trimmed, StringComparison.Ordinal) + 1;
            indent = keyColumn - 1;
        }
        else
        {
            keyColumn = lineIndent + 1;
            indent = lineIndent;
        }

        var separator = trimmed.IndexOf(':');
        if (separator <= 0
            || trimmed.StartsWith('#')
            || (separator + 1 < trimmed.Length && !char.IsWhiteSpace(trimmed[separator + 1])))
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
            if (current is YamlException yamlException)
            {
                location = new YamlDiagnosticLocation(
                    (int)Math.Max(1L, yamlException.Start.Line),
                    (int)Math.Max(1L, yamlException.Start.Column),
                    (int)Math.Max(1L, yamlException.End.Line),
                    (int)Math.Max(1L, yamlException.End.Column));
                return true;
            }
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
            var trimmed = line.TrimStart();
            var indent = line.Length - trimmed.Length;
            if (!string.IsNullOrWhiteSpace(line)
                && !trimmed.StartsWith('#')
                && indent == 0
                && trimmed.StartsWith(key + ":", StringComparison.Ordinal))
            {
                return new YamlDiagnosticLocation(i + 1, 1, i + 1, key.Length + 1);
            }
        }

        return null;
    }

    private static YamlDiagnostic CreateDiagnostic(YamlDiagnosticLocation location, string message)
    {
        return new YamlDiagnostic(
            location.StartLine,
            location.StartColumn,
            location.EndLine,
            location.EndColumn,
            message,
            YamlDiagnosticSeverity.Error);
    }

    private sealed record YamlDiagnosticLocation(int StartLine, int StartColumn, int EndLine, int EndColumn);
}
