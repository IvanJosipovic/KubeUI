using AvaloniaEdit.Document;
using AvaloniaEdit.Folding;

namespace KubeUI.Avalonia.Features.Resources.Yaml;

internal static class YamlFoldingStrategy
{
    private const string LastAppliedConfiguration = "kubectl.kubernetes.io/last-applied-configuration";

    public static void UpdateFoldings(FoldingManager? manager, TextDocument? document)
    {
        if (manager == null || document == null)
            return;
        var newFoldings = CreateNewFoldings(document, out var firstErrorOffset);
        manager.UpdateFoldings(newFoldings, firstErrorOffset);
    }

    public static IEnumerable<NewFolding> CreateNewFoldings(TextDocument document, out int firstErrorOffset)
    {
        var lineCount = document.LineCount;
        var foldMarkers = new List<NewFolding>(lineCount / 4);
        var lineInfos = new LineInfo[lineCount];
        var lineOffsets = new int[lineCount];
        var lineLengths = new int[lineCount];
        var lineEndOffsets = new int[lineCount];

        var index = 0;
        foreach (var line in document.Lines)
        {
            lineOffsets[index] = line.Offset;
            lineLengths[index] = line.Length;
            lineEndOffsets[index] = line.EndOffset;
            lineInfos[index] = AnalyzeLine(document.GetTextAsMemory(line.Offset, line.Length));
            index++;
        }

        for (var lineIndex = 0; lineIndex < index; lineIndex++)
        {
            var lineInfo = lineInfos[lineIndex];
            if (lineInfo.IsBlankOrComment)
                continue;

            var nextLineIndex = lineIndex + 1;
            LineInfo nextLineInfo = default;
            while (nextLineIndex < index)
            {
                nextLineInfo = lineInfos[nextLineIndex];
                if (nextLineInfo.IsBlankOrComment)
                {
                    nextLineIndex++;
                    continue;
                }

                break;
            }

            if (nextLineIndex >= index)
                continue;

            var baseAllowsSameIndentSequence = lineInfo.IsMappingKey && !lineInfo.IsSequenceItem;
            var isFoldStart = nextLineInfo.Indent > lineInfo.Indent;
            if (!isFoldStart && baseAllowsSameIndentSequence &&
                nextLineInfo.Indent == lineInfo.Indent && nextLineInfo.IsSequenceItem)
            {
                isFoldStart = true;
            }

            if (!isFoldStart)
                continue;

            var fold = new NewFolding
            {
                StartOffset = lineOffsets[lineIndex],
                Name = document.GetText(lineOffsets[lineIndex], lineLengths[lineIndex])
            };

            var endLineIndex = nextLineIndex;
            var scanIndex = nextLineIndex + 1;

            while (scanIndex < index)
            {
                var scanInfo = lineInfos[scanIndex];
                if (scanInfo.IsBlankOrComment)
                {
                    endLineIndex = scanIndex;
                    scanIndex++;
                    continue;
                }

                if (scanInfo.Indent < lineInfo.Indent)
                    break;
                if (scanInfo.Indent == lineInfo.Indent)
                {
                    if (!baseAllowsSameIndentSequence || !scanInfo.IsSequenceItem)
                        break;
                }

                endLineIndex = scanIndex;
                scanIndex++;
            }

            fold.EndOffset = lineEndOffsets[endLineIndex];
            foldMarkers.Add(fold);
        }

        firstErrorOffset = -1;
        return foldMarkers;
    }

    public static bool IsNoisyFieldFolding(TextDocument document, NewFolding folding)
    {
        return IsNoisyFieldFolding(document, folding.StartOffset, folding.EndOffset);
    }

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

    private static LineInfo AnalyzeLine(ReadOnlyMemory<char> text)
    {
        var span = text.Span;
        var indent = 0;

        while (indent < span.Length && span[indent] == ' ')
        {
            indent++;
        }

        if (indent == span.Length)
        {
            return LineInfo.BlankOrComment;
        }

        if (span[indent] == '#')
        {
            return LineInfo.BlankOrComment;
        }

        var end = span.Length;
        while (end > indent && char.IsWhiteSpace(span[end - 1]))
        {
            end--;
        }

        for (var i = indent; i < end; i++)
        {
            if (span[i] != '#')
            {
                continue;
            }

            end = i;
            while (end > indent && char.IsWhiteSpace(span[end - 1]))
            {
                end--;
            }
            break;
        }

        if (end <= indent)
        {
            return LineInfo.BlankOrComment;
        }

        var isSequenceItem = span[indent] == '-' && (indent + 1 == end || span[indent + 1] == ' ');
        var isMappingKey = span[end - 1] == ':';

        return new LineInfo(indent, isSequenceItem, isMappingKey);
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
            if (indent >= childIndent)
            {
                continue;
            }

            if (!string.Equals(key, expectedPath[expectedIndex], StringComparison.Ordinal))
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
        if (colon <= 0)
        {
            return null;
        }

        return trimmed[..colon].Trim().Trim('"', '\'');
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

    private readonly record struct LineInfo(int Indent, bool IsSequenceItem, bool IsMappingKey)
    {
        public bool IsBlankOrComment => Indent < 0;

        public static LineInfo BlankOrComment => new(-1, false, false);
    }

    private readonly record struct OpenFold(NewFolding Fold, int Indent, bool AllowsSameIndentSequence);
}
