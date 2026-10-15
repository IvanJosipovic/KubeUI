using System.Text;
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
    public void event_cursor_uses_the_shared_resource_key_format()
    {
        var encoded = McpResourceCursorCodec.Encode(
            "dev", "v1", "Event", "default", "default", "event", "uid-event");

        var decoded = McpResourceCursorCodec.Decode(encoded);

        decoded.Cluster.ShouldBe("dev");
        decoded.Kind.ShouldBe("Event");
        decoded.LastName.ShouldBe("event");
        Should.NotThrow(() => McpResourceCursorCodec.EnsureMatches(
            decoded, "dev", "v1", "Event", "default"));
    }

    [Fact]
    public void cursor_decoder_rejects_malformed_and_unsupported_cursors()
    {
        var unsupportedVersion = EncodeJson(
            """{"Version":2,"Cluster":"dev","ApiVersion":"v1","Kind":"Pod","Namespace":null,"LastNamespace":"default","LastName":"api","LastUid":"uid-api"}""");
        var missingLastName = EncodeJson(
            """{"Version":1,"Cluster":"dev","ApiVersion":"v1","Kind":"Pod","Namespace":null,"LastNamespace":"default","LastName":null,"LastUid":"uid-api"}""");
        var missingCluster = EncodeJson(
            """{"Version":1,"Cluster":"","ApiVersion":"v1","Kind":"Pod","Namespace":null,"LastNamespace":"default","LastName":"api","LastUid":"uid-api"}""");
        var missingApiVersion = EncodeJson(
            """{"Version":1,"Cluster":"dev","ApiVersion":"","Kind":"Pod","Namespace":null,"LastNamespace":"default","LastName":"api","LastUid":"uid-api"}""");
        var missingKind = EncodeJson(
            """{"Version":1,"Cluster":"dev","ApiVersion":"v1","Kind":"","Namespace":null,"LastNamespace":"default","LastName":"api","LastUid":"uid-api"}""");
        var malformedJson = EncodeJson("{");
        var nullJson = EncodeJson("null");

        foreach (var cursor in new[]
        {
            "", "!", malformedJson, nullJson, unsupportedVersion,
            missingLastName, missingCluster, missingApiVersion, missingKind
        })
            Should.Throw<ArgumentException>(() => McpResourceCursorCodec.Decode(cursor));
    }

    private static string EncodeJson(string json)
        => Convert.ToBase64String(Encoding.UTF8.GetBytes(json))
            .TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');
}
