using System.Text;
using System.Text.Json;
using KubeUI.Avalonia.Infrastructure.Mcp;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Infrastructure.Mcp;

public sealed class McpResourcePaginationTests
{
    [Fact]
    public void cursor_round_trips_and_is_bound_to_the_resource_query()
    {
        var encoded = McpResourceCursorCodec.Encode(
            "dev", "v1", "Pod", "default", "default", "api", "uid-api");

        var decoded = McpResourceCursorCodec.Decode(encoded);

        decoded.Cluster.ShouldBe("dev");
        decoded.ApiVersion.ShouldBe("v1");
        decoded.Kind.ShouldBe("Pod");
        decoded.Namespace.ShouldBe("default");
        decoded.LastNamespace.ShouldBe("default");
        decoded.LastName.ShouldBe("api");
        decoded.LastUid.ShouldBe("uid-api");
        Should.NotThrow(() => McpResourceCursorCodec.EnsureMatches(
            decoded, "dev", "v1", "Pod", "default"));
        Should.Throw<ArgumentException>(() => McpResourceCursorCodec.EnsureMatches(
            decoded, "prod", "v1", "Pod", "default"));
        Should.Throw<ArgumentException>(() => McpResourceCursorCodec.EnsureMatches(
            decoded, "dev", "apps/v1", "Pod", "default"));
        Should.Throw<ArgumentException>(() => McpResourceCursorCodec.EnsureMatches(
            decoded, "dev", "v1", "Deployment", "default"));
        Should.Throw<ArgumentException>(() => McpResourceCursorCodec.EnsureMatches(
            decoded, "dev", "v1", "Pod", "other"));
    }

    [Fact]
    public void cursor_decoder_rejects_malformed_and_unsupported_cursors()
    {
        var unsupportedVersion = EncodeJson(new
        {
            Version = 2,
            Cluster = "dev",
            ApiVersion = "v1",
            Kind = "Pod",
            Namespace = (string?)null,
            LastNamespace = "default",
            LastName = "api",
            LastUid = "uid-api",
        });
        var missingLastName = EncodeJson(new
        {
            Version = 1,
            Cluster = "dev",
            ApiVersion = "v1",
            Kind = "Pod",
            Namespace = (string?)null,
            LastNamespace = "default",
            LastName = (string?)null,
            LastUid = "uid-api",
        });
        var missingCluster = EncodeJson(new
        {
            Version = 1,
            Cluster = string.Empty,
            ApiVersion = "v1",
            Kind = "Pod",
            Namespace = (string?)null,
            LastNamespace = "default",
            LastName = "api",
            LastUid = "uid-api",
        });
        var missingApiVersion = EncodeJson(new
        {
            Version = 1,
            Cluster = "dev",
            ApiVersion = string.Empty,
            Kind = "Pod",
            Namespace = (string?)null,
            LastNamespace = "default",
            LastName = "api",
            LastUid = "uid-api",
        });
        var missingKind = EncodeJson(new
        {
            Version = 1,
            Cluster = "dev",
            ApiVersion = "v1",
            Kind = string.Empty,
            Namespace = (string?)null,
            LastNamespace = "default",
            LastName = "api",
            LastUid = "uid-api",
        });
        var malformedJson = Convert.ToBase64String(Encoding.UTF8.GetBytes("{"))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

        foreach (var cursor in new[]
        {
            "", "!", malformedJson, EncodeJson<object?>(null), unsupportedVersion,
            missingLastName, missingCluster, missingApiVersion, missingKind
        })
            Should.Throw<ArgumentException>(() => McpResourceCursorCodec.Decode(cursor));
    }

    private static string EncodeJson<T>(T value)
        => Convert.ToBase64String(JsonSerializer.SerializeToUtf8Bytes(value))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
}
