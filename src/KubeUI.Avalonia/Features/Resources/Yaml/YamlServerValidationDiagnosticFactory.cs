using k8s;
using k8s.Autorest;
using k8s.Models;
using KubeUI.Avalonia.Infrastructure;
using YamlDotNet.RepresentationModel;

namespace KubeUI.Avalonia.Features.Resources.Yaml;

internal static class YamlServerValidationDiagnosticFactory
{
    public static IReadOnlyList<YamlDiagnostic> Create(string yaml, Exception exception)
    {
        if (Utilities.GetMeaningfulException(exception) is not HttpOperationException operationException)
        {
            return [];
        }

        try
        {
            var status = KubernetesJson.Deserialize<V1Status>(operationException.Response.Content);
            if (status?.Details?.Causes is not { Count: > 0 } causes)
            {
                return [];
            }

            var stream = new YamlStream();
            stream.Load(new StringReader(yaml));
            if (stream.Documents.Count != 1)
            {
                return [];
            }

            var diagnostics = new List<YamlDiagnostic>(causes.Count);
            foreach (var cause in causes)
            {
                if (string.IsNullOrWhiteSpace(cause.Field)
                    || !TryFindNode(stream.Documents[0].RootNode, cause.Field, out var node))
                {
                    continue;
                }

                var message = string.IsNullOrWhiteSpace(cause.Message)
                    ? cause.Field
                    : $"{cause.Field}: {cause.Message}";
                diagnostics.Add(new YamlDiagnostic(
                    (int)Math.Max(1L, node.Start.Line),
                    (int)Math.Max(1L, node.Start.Column),
                    (int)Math.Max(1L, node.End.Line),
                    (int)Math.Max(1L, node.End.Column),
                    message,
                    YamlDiagnosticSeverity.Error));
            }

            return diagnostics;
        }
        catch (Exception)
        {
            return [];
        }
    }

    private static bool TryFindNode(YamlNode root, string field, out YamlNode node)
    {
        node = root;
        var position = 0;
        while (position < field.Length)
        {
            var separator = field.IndexOfAny(['.', '['], position);
            if (separator < 0)
            {
                separator = field.Length;
            }

            if (separator > position)
            {
                if (node is not YamlMappingNode mapping
                    || !mapping.Children.TryGetValue(new YamlScalarNode(field[position..separator]), out var child))
                {
                    return false;
                }

                node = child;
                position = separator;
            }

            if (position < field.Length && field[position] == '[')
            {
                var end = field.IndexOf(']', position + 1);
                if (end < 0
                    || !int.TryParse(field.AsSpan(position + 1, end - position - 1), out var index)
                    || node is not YamlSequenceNode sequence
                    || index < 0
                    || index >= sequence.Children.Count)
                {
                    return false;
                }

                node = sequence.Children[index];
                position = end + 1;
            }

            if (position < field.Length && field[position] == '.')
            {
                position++;
            }
        }

        return true;
    }
}
