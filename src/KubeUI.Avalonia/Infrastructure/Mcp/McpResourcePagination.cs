using System.Text.Json;
using System.Text.Json.Serialization;

namespace KubeUI.Avalonia.Infrastructure.Mcp;

/// <summary>One page of resource summaries and the cursor for the next page, when present.</summary>
/// <param name="Resources">Resources in stable namespace, name, and UID order.</param>
/// <param name="NextCursor">Opaque cursor for the next page, or <see langword="null"/> when complete.</param>
public sealed record McpResourcePage(
    IReadOnlyList<McpResourceInfo> Resources,
    string? NextCursor);

internal sealed record McpResourceCursor(
    int Version,
    string Cluster,
    string ApiVersion,
    string Kind,
    string? Namespace,
    string? LastNamespace,
    string LastName,
    string? LastUid);

internal static class McpResourceCursorCodec
{
    private const int CurrentVersion = 1;
    private const int MaximumEncodedLength = 4096;

    public static string Encode(
        string cluster,
        string apiVersion,
        string kind,
        string? @namespace,
        string? lastNamespace,
        string lastName,
        string? lastUid)
    {
        var cursor = new McpResourceCursor(
            CurrentVersion, cluster, apiVersion, kind, @namespace,
            lastNamespace, lastName, lastUid);
        var bytes = JsonSerializer.SerializeToUtf8Bytes(
            cursor, McpResourceCursorJsonSerializationContext.Default.McpResourceCursor);
        return Convert.ToBase64String(bytes)
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
    }

    public static McpResourceCursor Decode(string cursor)
    {
        ArgumentNullException.ThrowIfNull(cursor);
        if (cursor.Length is 0 or > MaximumEncodedLength)
            throw InvalidCursor();

        try
        {
            var base64 = cursor.Replace('-', '+').Replace('_', '/');
            base64 += new string('=', (4 - base64.Length % 4) % 4);
            var bytes = Convert.FromBase64String(base64);
            var decoded = JsonSerializer.Deserialize(
                bytes, McpResourceCursorJsonSerializationContext.Default.McpResourceCursor);
            if (decoded is null
                || decoded.Version != CurrentVersion
                || string.IsNullOrWhiteSpace(decoded.Cluster)
                || string.IsNullOrWhiteSpace(decoded.ApiVersion)
                || string.IsNullOrWhiteSpace(decoded.Kind)
                || decoded.LastName is null)
            {
                throw InvalidCursor();
            }

            return decoded;
        }
        catch (FormatException)
        {
            throw InvalidCursor();
        }
        catch (JsonException)
        {
            throw InvalidCursor();
        }
    }

    public static void EnsureMatches(
        McpResourceCursor cursor,
        string cluster,
        string apiVersion,
        string kind,
        string? @namespace)
    {
        if (!string.Equals(cursor.Cluster, cluster, StringComparison.Ordinal)
            || !string.Equals(cursor.ApiVersion, apiVersion, StringComparison.Ordinal)
            || !string.Equals(cursor.Kind, kind, StringComparison.Ordinal)
            || !string.Equals(cursor.Namespace, @namespace, StringComparison.Ordinal))
        {
            throw new ArgumentException("The cursor does not match this resource query.", "cursor");
        }
    }

    private static ArgumentException InvalidCursor()
        => new("The resource cursor is invalid. Start the listing again.", "cursor");
}

[JsonSourceGenerationOptions]
[JsonSerializable(typeof(McpResourceCursor))]
internal partial class McpResourceCursorJsonSerializationContext : JsonSerializerContext;
