using System.Globalization;
using System.Text;
using System.Text.Json;

namespace KubeUI.Kubernetes;

/// <summary>Parses Crossplane provider log lines containing Terraform instance diffs.</summary>
public sealed class CrossplaneDiffLogParser
{
    private const string ResourceAttrDiffTypeMarker = "ResourceAttrDiff{";

    /// <summary>Parses a provider log line, returning only complete attributes from truncated payloads.</summary>
    /// <remarks>Use the overload with a truncation output flag when that distinction matters.</remarks>
    public IReadOnlyList<CrossplaneDiffRecord> Parse(string line)
        => Parse(line, out _);

    /// <summary>Parses a provider log line and reports when only its complete prefix could be recovered.</summary>
    /// <param name="line">The provider log line.</param>
    /// <param name="wasTruncated">
    /// Set to <see langword="true"/> when the structured diff payload was clipped and only complete attributes
    /// were returned.
    /// </param>
    public IReadOnlyList<CrossplaneDiffRecord> Parse(string line, out bool wasTruncated)
    {
        wasTruncated = false;
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

            if (!TryParseGvk(gvk, out var apiVersion, out var kind))
            {
                return [];
            }

            var records = ParseAttributes(uid, name, @namespace, apiVersion, kind, instanceDiff);
            if (!IsIncompleteInstanceDiff(instanceDiff)
                || !instanceDiff.Contains("InstanceDiff{", StringComparison.Ordinal)
                || !instanceDiff.Contains("Attributes:map[", StringComparison.Ordinal))
            {
                return records;
            }

            wasTruncated = true;
            return ParseAttributes(uid, name, @namespace, apiVersion, kind, instanceDiff, allowIncomplete: true);
        }
        catch (JsonException)
        {
            return ParseTruncatedPayload(line[jsonStart..], out wasTruncated);
        }
    }

    private static IReadOnlyList<CrossplaneDiffRecord> ParseTruncatedPayload(string json, out bool wasTruncated)
    {
        wasTruncated = false;
        JsonDocument document;
        try
        {
            document = JsonDocument.Parse(string.Concat(json, "\"}"));
        }
        catch (JsonException)
        {
            return [];
        }

        using (document)
        {
            var root = document.RootElement;
            if (!TryGetString(root, "uid", out var uid)
                || !TryGetString(root, "name", out var name)
                || !TryGetString(root, "namespace", out var @namespace)
                || !TryGetString(root, "gvk", out var gvk)
                || !TryGetString(root, "instanceDiff", out var instanceDiff)
                || !instanceDiff.Contains("InstanceDiff{", StringComparison.Ordinal)
                || !instanceDiff.Contains("Attributes:map[", StringComparison.Ordinal)
                || !TryParseGvk(gvk, out var apiVersion, out var kind))
            {
                return [];
            }

            var records = ParseAttributes(uid, name, @namespace, apiVersion, kind, instanceDiff, allowIncomplete: true);
            wasTruncated = true;
            return records;
        }
    }

    private static bool IsIncompleteInstanceDiff(string instanceDiff)
    {
        var instanceStart = instanceDiff.IndexOf('{');
        return instanceStart >= 0
            && !TryFindMatching(instanceDiff, instanceStart, '{', '}', out _);
    }

    private static IReadOnlyList<CrossplaneDiffRecord> ParseAttributes(
        string uid,
        string name,
        string @namespace,
        string apiVersion,
        string kind,
        string instanceDiff,
        bool allowIncomplete = false)
    {
        var instanceStart = instanceDiff.IndexOf('{');
        if (instanceStart < 0)
        {
            return [];
        }

        var instanceComplete = TryFindMatching(instanceDiff, instanceStart, '{', '}', out var instanceEnd);
        if ((!instanceComplete && !allowIncomplete)
            || (instanceComplete && !instanceDiff.AsSpan(instanceEnd + 1).Trim().IsEmpty))
        {
            return [];
        }

        var attributesIndex = instanceDiff.IndexOf("Attributes:map[", StringComparison.Ordinal);
        if (attributesIndex < 0)
        {
            return [];
        }

        var bodyStart = instanceDiff.IndexOf('{', attributesIndex);
        if (bodyStart < 0)
        {
            return [];
        }

        if (!TryFindMatching(instanceDiff, bodyStart, '{', '}', out var bodyEnd))
        {
            if (!allowIncomplete)
            {
                return [];
            }

            bodyEnd = instanceDiff.Length;
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

            SkipWhitespace(instanceDiff, ref index, bodyEnd);
            if (index >= bodyEnd || instanceDiff[index] != ':')
            {
                break;
            }

            index++;
            SkipWhitespace(instanceDiff, ref index, bodyEnd);
            if (instanceDiff.AsSpan(index).StartsWith("nil", StringComparison.Ordinal)
                && (index + 3 >= bodyEnd || char.IsWhiteSpace(instanceDiff[index + 3]) || instanceDiff[index + 3] == ','))
            {
                index += 3;
                var nextEntry = instanceDiff.IndexOf(',', index, bodyEnd - index);
                index = nextEntry < 0 ? bodyEnd : nextEntry + 1;
                continue;
            }

            var valueStart = instanceDiff.IndexOf(ResourceAttrDiffTypeMarker, index, bodyEnd - index, StringComparison.Ordinal);
            if (valueStart < 0)
            {
                break;
            }

            var typePrefix = instanceDiff.AsSpan(index, valueStart - index).Trim();
            if (!typePrefix.IsEmpty
                && !typePrefix.SequenceEqual("*".AsSpan())
                && !typePrefix.SequenceEqual("*terraform.".AsSpan())
                && !typePrefix.SequenceEqual("terraform.".AsSpan()))
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
            var sensitive = ReadGoBoolean(body, "Sensitive");
            records.Add(new CrossplaneDiffRecord(
                uid,
                name,
                @namespace,
                apiVersion,
                kind,
                field,
                sensitive ? string.Empty : oldValue,
                sensitive ? string.Empty : newValue,
                ReadGoBoolean(body, "NewComputed"),
                ReadGoBoolean(body, "NewRemoved"),
                ReadGoBoolean(body, "RequiresNew"),
                sensitive));

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
        var index = start;
        while (index < body.Length)
        {
            var character = body[index++];
            if (character == '"')
            {
                return builder.ToString();
            }

            if (character == '\\')
            {
                if (!TryAppendGoEscape(body, ref index, builder))
                {
                    return null;
                }
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

    private static bool TryParseGvk(string gvk, out string apiVersion, out string kind)
    {
        var separator = gvk.IndexOf(", Kind=", StringComparison.Ordinal);
        if (separator <= 0 || separator + ", Kind=".Length >= gvk.Length)
        {
            apiVersion = string.Empty;
            kind = string.Empty;
            return false;
        }

        apiVersion = gvk[..separator];
        kind = gvk[(separator + ", Kind=".Length)..];
        return !string.IsNullOrWhiteSpace(apiVersion) && !string.IsNullOrWhiteSpace(kind);
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

            if (character == '\\')
            {
                if (!TryAppendGoEscape(value.AsSpan(0, end), ref index, builder))
                {
                    return false;
                }
            }
            else
            {
                builder.Append(character);
            }
        }

        return false;
    }

    private static bool TryAppendGoEscape(ReadOnlySpan<char> value, ref int index, StringBuilder builder)
    {
        if (index >= value.Length)
        {
            return false;
        }

        var escape = value[index++];
        switch (escape)
        {
            case 'a':
                builder.Append('\a');
                return true;
            case 'b':
                builder.Append('\b');
                return true;
            case 'f':
                builder.Append('\f');
                return true;
            case 'n':
                builder.Append('\n');
                return true;
            case 'r':
                builder.Append('\r');
                return true;
            case 't':
                builder.Append('\t');
                return true;
            case 'v':
                builder.Append('\v');
                return true;
            case '\\':
            case '"':
                builder.Append(escape);
                return true;
            case 'x':
                return TryAppendHexEscape(value, ref index, 2, builder);
            case 'u':
                return TryAppendHexEscape(value, ref index, 4, builder);
            case 'U':
                return TryAppendHexEscape(value, ref index, 8, builder);
            default:
                if (escape is < '0' or > '7' || index + 2 > value.Length)
                {
                    return false;
                }

                var octal = escape - '0';
                for (var digit = 0; digit < 2; digit++)
                {
                    var character = value[index++];
                    if (character is < '0' or > '7')
                    {
                        return false;
                    }

                    octal = (octal * 8) + character - '0';
                }

                if (octal > byte.MaxValue)
                {
                    return false;
                }

                builder.Append((char)octal);
                return true;
        }
    }

    private static bool TryAppendHexEscape(ReadOnlySpan<char> value, ref int index, int digitCount, StringBuilder builder)
    {
        if (index + digitCount > value.Length
            || !int.TryParse(value.Slice(index, digitCount), NumberStyles.AllowHexSpecifier, CultureInfo.InvariantCulture, out var codePoint)
            || !Rune.TryCreate(codePoint, out var rune))
        {
            return false;
        }

        index += digitCount;
        builder.Append(rune.ToString());
        return true;
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
