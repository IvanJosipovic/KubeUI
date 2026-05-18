using AvaloniaEdit.Document;
using AvaloniaEdit.Indentation;

namespace KubeUI.Avalonia.Features.Resources.Yaml;

internal sealed class YamlIndentationStrategy : IIndentationStrategy
{
    private const int IndentationSize = 2;

    public void IndentLine(TextDocument document, DocumentLine line)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentNullException.ThrowIfNull(line);

        var previousLine = FindPreviousContentLine(document, line);
        if (previousLine == null)
        {
            ReplaceIndentation(document, line, 0);
            return;
        }

        var previousText = document.GetText(previousLine);
        var indentation = GetNextLineIndentation(previousText);
        ReplaceIndentation(document, line, indentation);
    }

    public void IndentLines(TextDocument document, int beginLine, int endLine)
    {
        ArgumentNullException.ThrowIfNull(document);

        for (var lineNumber = beginLine; lineNumber <= endLine; lineNumber++)
        {
            IndentLine(document, document.GetLineByNumber(lineNumber));
        }
    }

    private static DocumentLine? FindPreviousContentLine(TextDocument document, DocumentLine line)
    {
        var current = line.PreviousLine;
        while (current != null)
        {
            var text = document.GetText(current);
            if (!string.IsNullOrWhiteSpace(text)
                && !IsCommentLine(text))
            {
                return current;
            }

            current = current.PreviousLine;
        }

        return null;
    }

    private static int GetNextLineIndentation(string previousLineText)
    {
        var leadingSpaces = CountLeadingSpaces(previousLineText);
        var content = StripInlineComment(previousLineText).Trim();
        if (string.IsNullOrWhiteSpace(content))
        {
            return leadingSpaces;
        }

        if (content == "-")
        {
            return leadingSpaces + IndentationSize;
        }

        if (content.StartsWith("- ", StringComparison.Ordinal))
        {
            var afterDash = content[2..].TrimStart();
            if (string.IsNullOrWhiteSpace(afterDash) || afterDash.Contains(':', StringComparison.Ordinal))
            {
                return leadingSpaces + IndentationSize;
            }

            return leadingSpaces;
        }

        return content.EndsWith(':') ? leadingSpaces + IndentationSize : leadingSpaces;
    }

    private static bool IsCommentLine(string text)
    {
        return text.TrimStart().StartsWith("#", StringComparison.Ordinal);
    }

    private static string StripInlineComment(string text)
    {
        var inSingleQuoted = false;
        var inDoubleQuoted = false;

        for (var i = 0; i < text.Length; i++)
        {
            var current = text[i];
            if (current == '\'' && !inDoubleQuoted)
            {
                inSingleQuoted = !inSingleQuoted;
                continue;
            }

            if (current == '"' && !inSingleQuoted)
            {
                inDoubleQuoted = !inDoubleQuoted;
                continue;
            }

            if (current == '#'
                && !inSingleQuoted
                && !inDoubleQuoted
                && (i == 0 || char.IsWhiteSpace(text[i - 1])))
            {
                return text[..i].TrimEnd();
            }
        }

        return text;
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

    private static void ReplaceIndentation(TextDocument document, DocumentLine line, int indentation)
    {
        var offset = line.Offset;
        var length = 0;
        while (length < line.Length && document.GetCharAt(offset + length) == ' ')
        {
            length++;
        }

        document.Replace(offset, length, new string(' ', indentation));
    }
}
