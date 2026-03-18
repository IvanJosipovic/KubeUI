using AvaloniaEdit.Document;
using AvaloniaEdit.Folding;

namespace KubeUI.Avalonia.Views;

internal static class YamlFoldingStrategy
{
    // Plan (pseudocode):
    // 1) For each line, skip blank/comment-only lines as fold starts.
    // 2) Find the next non-blank/comment line.
    // 3) If the next meaningful line is not more indented, no fold.
    // 4) If it is more indented, create a fold that spans until the last line
    //    that is either blank/comment or has indent greater than the base indent.
    // 5) Ensure end offsets are safe even when trailing blank/comment lines exist.

    public static void UpdateFoldings(FoldingManager? manager, TextDocument? document)
    {
        if (manager == null || document == null) return;
        var newFoldings = CreateNewFoldings(document, out var firstErrorOffset);
        manager.UpdateFoldings(newFoldings, firstErrorOffset);
    }

    private static int CountLeadingSpaces(ReadOnlyMemory<char> memory, out bool isSequenceItem)
    {
        var span = memory.Span;
        var count = 0;
        while (count < span.Length && span[count] == ' ')
            count++;

        isSequenceItem = count < span.Length && span[count] == '-' &&
            (count + 1 == span.Length || span[count + 1] == ' ');

        return count;
    }

    private static bool IsMappingKey(ReadOnlyMemory<char> memory)
    {
        var span = memory.Span;
        var hashIndex = span.IndexOf('#');
        if (hashIndex >= 0)
            span = span[..hashIndex];

        span = span.TrimEnd();
        return span.Length > 0 && span[^1] == ':';
    }

    private static bool IsBlankLine(ReadOnlyMemory<char> memory) =>
        memory.Span.Trim().Length == 0;

    private static bool IsCommentLine(ReadOnlyMemory<char> memory) =>
        memory.Span.TrimStart().StartsWith("#");

    public static IEnumerable<NewFolding> CreateNewFoldings(TextDocument document, out int firstErrorOffset)
    {
        try
        {
            var foldMarkers = new List<NewFolding>();

            foreach (var line in document.Lines)
            {
                var text = document.GetTextAsMemory(line.Offset, line.Length);
                if (IsBlankLine(text) || IsCommentLine(text))
                    continue;

                var baseIndent = CountLeadingSpaces(text, out var baseIsSequenceItem);
                var baseIsMappingKey = IsMappingKey(text);
                var baseAllowsSameIndentSequence = baseIsMappingKey && !baseIsSequenceItem;
                var nextLine = line.NextLine;

                // Find the first meaningful next line.
                while (nextLine != null)
                {
                    var nextText = document.GetTextAsMemory(nextLine.Offset, nextLine.Length);
                    if (IsBlankLine(nextText) || IsCommentLine(nextText))
                    {
                        nextLine = nextLine.NextLine;
                        continue;
                    }
                    break;
                }

                if (nextLine == null)
                    continue;

                var nextLineText = document.GetTextAsMemory(nextLine.Offset, nextLine.Length);
                var nextIndent = CountLeadingSpaces(nextLineText, out var nextIsSequenceItem);

                var isFoldStart = nextIndent > baseIndent;
                if (!isFoldStart && baseAllowsSameIndentSequence &&
                    nextIndent == baseIndent && nextIsSequenceItem)
                {
                    isFoldStart = true;
                }

                if (!isFoldStart)
                    continue;

                var fold = new NewFolding
                {
                    StartOffset = line.Offset,
                    Name = text.ToString()
                };

                var endLine = nextLine;
                var scan = nextLine.NextLine;

                while (scan != null)
                {
                    var scanText = document.GetTextAsMemory(scan.Offset, scan.Length);
                    if (IsBlankLine(scanText) || IsCommentLine(scanText))
                    {
                        endLine = scan;
                        scan = scan.NextLine;
                        continue;
                    }

                    var scanIndent = CountLeadingSpaces(scanText, out var scanIsSequenceItem);
                    if (scanIndent < baseIndent)
                        break;
                    if (scanIndent == baseIndent)
                    {
                        if (!baseAllowsSameIndentSequence || !scanIsSequenceItem)
                            break;
                    }

                    endLine = scan;
                    scan = scan.NextLine;
                }

                fold.EndOffset = endLine.EndOffset;
                foldMarkers.Add(fold);
            }

            firstErrorOffset = -1;
            foldMarkers.Sort((a, b) => a.StartOffset.CompareTo(b.StartOffset));
            return foldMarkers;
        }
        catch
        {
            firstErrorOffset = 0;
            return [];
        }
    }
}
