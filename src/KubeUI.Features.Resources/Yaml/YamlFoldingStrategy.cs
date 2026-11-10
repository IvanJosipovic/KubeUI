using AvaloniaEdit.Document;
using AvaloniaEdit.Folding;

namespace KubeUI.Features.Resources.Yaml;

internal static class YamlFoldingStrategy
{
    private const string LastAppliedConfiguration = "kubectl.kubernetes.io/last-applied-configuration";

    public static bool IsNoisyFieldFolding(TextDocument document, NewFolding folding)
        => IsNoisyFieldFolding(document, folding.StartOffset, folding.EndOffset);

    public static bool IsNoisyFieldFolding(TextDocument document, int startOffset, int endOffset)
    {
        var line = document.GetLineByOffset(startOffset);
        var key = GetMappingKey(document.GetText(line));
        if (key == "managedFields")
        {
            return HasParentPath(document, startOffset, "metadata");
        }

        return key == LastAppliedConfiguration
            && endOffset > line.EndOffset
            && HasParentPath(document, startOffset, "metadata", "annotations");
    }

    private static bool HasParentPath(TextDocument document, int targetOffset, params string[] expectedPath)
    {
        var targetLine = document.GetLineByOffset(targetOffset);
        var childIndent = CountLeadingSpaces(document.GetText(targetLine));
        var expectedIndex = expectedPath.Length - 1;
        for (var line = targetLine.PreviousLine; line != null; line = line.PreviousLine)
        {
            var key = GetMappingKey(document.GetText(line));
            if (key == null)
            {
                continue;
            }

            var indent = CountLeadingSpaces(document.GetText(line));
            if (indent >= childIndent || !string.Equals(key, expectedPath[expectedIndex], StringComparison.Ordinal))
            {
                continue;
            }

            expectedIndex--;
            if (expectedIndex < 0)
            {
                return true;
            }

            childIndent = indent;
        }

        return false;
    }

    private static string? GetMappingKey(string lineText)
    {
        var trimmed = lineText.TrimStart();
        if (trimmed.Length == 0 || trimmed[0] == '#' || trimmed[0] == '-')
        {
            return null;
        }

        var colon = trimmed.IndexOf(':');
        return colon <= 0 ? null : trimmed[..colon].Trim().Trim('"', '\'');
    }

    private static int CountLeadingSpaces(string text)
    {
        var count = 0;
        while (count < text.Length && text[count] == ' ')
        {
            count++;
        }

        return count;
    }
}
