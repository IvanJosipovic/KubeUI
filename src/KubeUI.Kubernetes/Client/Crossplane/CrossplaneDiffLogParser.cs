using System.Text;
using System.Text.Json;

namespace KubeUI.Kubernetes;

/// <summary>Parses Crossplane provider log lines containing Terraform instance diffs.</summary>
public sealed class CrossplaneDiffLogParser
{
    public IReadOnlyList<CrossplaneDiffRecord> Parse(string line)
    {
        if (string.IsNullOrWhiteSpace(line))
        {
            return [];
        }

        var messageIndex = line.IndexOf("Diff detected", StringComparison.Ordinal);
        if (messageIndex < 0)
        {
            return [];
        }

        var jsonStart = line.IndexOf('{', messageIndex);
        if (jsonStart < 0)
        {
            return [];
        }

        try
        {
            using var document = JsonDocument.Parse(line[jsonStart..]);
            var root = document.RootElement;
            if (!TryGetString(root, "uid", out var uid)
                || !TryGetString(root, "name", out var name)
                || !TryGetString(root, "namespace", out var @namespace)
                || !TryGetString(root, "gvk", out var gvk)
                || !TryGetString(root, "instanceDiff", out var instanceDiff))
            {
                return [];
            }

            ParseGvk(gvk, out var apiVersion, out var kind);
            return ParseAttributes(uid, name, @namespace, apiVersion, kind, instanceDiff);
        }
        catch (JsonException)
        {
            return [];
        }
    }

    private static IReadOnlyList<CrossplaneDiffRecord> ParseAttributes(
        string uid,
        string name,
        string @namespace,
        string apiVersion,
        string kind,
        string instanceDiff)
    {
        var attributesIndex = instanceDiff.IndexOf("Attributes:map[", StringComparison.Ordinal);
        if (attributesIndex < 0)
        {
            return [];
        }

        var bodyStart = instanceDiff.IndexOf('{', attributesIndex);
        if (bodyStart < 0 || !TryFindMatching(instanceDiff, bodyStart, '{', '}', out var bodyEnd))
        {
            return [];
        }

        var records = new List<CrossplaneDiffRecord>();
        var index = bodyStart + 1;
        while (index < bodyEnd)
        {
            SkipWhitespace(instanceDiff, ref index, bodyEnd);
            if (index >= bodyEnd || instanceDiff[index] != '"')
            {
                index++;
                continue;
            }

            if (!TryReadQuoted(instanceDiff, ref index, bodyEnd, out var field))
            {
                break;
            }

            var valueStart = instanceDiff.IndexOf("ResourceAttrDiff{", index, bodyEnd - index, StringComparison.Ordinal);
            if (valueStart < 0)
            {
                break;
            }

            var attrStart = valueStart + "ResourceAttrDiff".Length;
            if (!TryFindMatching(instanceDiff, attrStart, '{', '}', out var attrEnd))
            {
                break;
            }

            var body = instanceDiff.AsSpan(attrStart + 1, attrEnd - attrStart - 1);
            var oldValue = ReadGoProperty(body, "Old") ?? string.Empty;
            var newValue = ReadGoProperty(body, "New") ?? string.Empty;
            records.Add(new CrossplaneDiffRecord(
                uid,
                name,
                @namespace,
                apiVersion,
                kind,
                field,
                oldValue,
                newValue,
                ReadGoBoolean(body, "NewComputed"),
                ReadGoBoolean(body, "NewRemoved"),
                ReadGoBoolean(body, "RequiresNew"),
                ReadGoBoolean(body, "Sensitive")));

            index = attrEnd + 1;
        }

        return records;
    }

    private static string? ReadGoProperty(ReadOnlySpan<char> body, string property)
    {
        var marker = property + ":\"";
        var start = body.IndexOf(marker, StringComparison.Ordinal);
        if (start < 0)
        {
            return null;
        }

        start += marker.Length;
        var builder = new StringBuilder();
        for (var i = start; i < body.Length; i++)
        {
            var character = body[i];
            if (character == '"')
            {
                return builder.ToString();
            }

            if (character == '\\' && i + 1 < body.Length)
            {
                builder.Append(body[++i] switch
                {
                    'n' => '\n',
                    'r' => '\r',
                    't' => '\t',
                    '\\' => '\\',
                    '"' => '"',
                    _ => body[i]
                });
            }
            else
            {
                builder.Append(character);
            }
        }

        return null;
    }

    private static bool ReadGoBoolean(ReadOnlySpan<char> body, string property)
    {
        var marker = property + ":true";
        return body.IndexOf(marker, StringComparison.Ordinal) >= 0;
    }

    private static bool TryGetString(JsonElement root, string property, out string value)
    {
        if (root.TryGetProperty(property, out var element) && element.ValueKind == JsonValueKind.String)
        {
            value = element.GetString() ?? string.Empty;
            return true;
        }

        value = string.Empty;
        return false;
    }

    private static void ParseGvk(string gvk, out string apiVersion, out string kind)
    {
        var separator = gvk.IndexOf(", Kind=", StringComparison.Ordinal);
        if (separator < 0)
        {
            apiVersion = gvk;
            kind = string.Empty;
            return;
        }

        apiVersion = gvk[..separator];
        kind = gvk[(separator + ", Kind=".Length)..];
    }

    private static bool TryReadQuoted(string value, ref int index, int end, out string result)
    {
        result = string.Empty;
        if (index >= end || value[index] != '"')
        {
            return false;
        }

        index++;
        var builder = new StringBuilder();
        while (index < end)
        {
            var character = value[index++];
            if (character == '"')
            {
                result = builder.ToString();
                return true;
            }

            if (character == '\\' && index < end)
            {
                builder.Append(value[index++]);
            }
            else
            {
                builder.Append(character);
            }
        }

        return false;
    }

    private static bool TryFindMatching(string value, int start, char opening, char closing, out int end)
    {
        var depth = 0;
        var quoted = false;
        var escaped = false;
        for (var index = start; index < value.Length; index++)
        {
            var character = value[index];
            if (quoted)
            {
                if (escaped)
                {
                    escaped = false;
                }
                else if (character == '\\')
                {
                    escaped = true;
                }
                else if (character == '"')
                {
                    quoted = false;
                }

                continue;
            }

            if (character == '"')
            {
                quoted = true;
            }
            else if (character == opening)
            {
                depth++;
            }
            else if (character == closing && --depth == 0)
            {
                end = index;
                return true;
            }
        }

        end = -1;
        return false;
    }

    private static void SkipWhitespace(string value, ref int index, int end)
    {
        while (index < end && char.IsWhiteSpace(value[index]))
        {
            index++;
        }
    }
}
