using System.Text.Json;
using dotacp.protocol;
using KubeUI.AI.Agents;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KubeUI.AI.Acp;

// ACP permission policy. Runtime-specific metadata stays outside domain event mapping.
internal sealed class AcpPermissionMapper
{
    public AgentPermissionRequest Map(
        RequestPermissionRequest request,
        string? knownTitle,
        object? knownInput,
        ToolKind? knownKind,
        Dictionary<string, object>? knownMeta,
        IReadOnlySet<string> trustedMcpServers)
    {
        var title = request.ToolCall.Title ?? knownTitle;
        var input = request.ToolCall.RawInput ?? knownInput;
        var meta = request.ToolCall.Meta ?? knownMeta;
        var action = string.IsNullOrWhiteSpace(title)
            ? GetMcpToolAction(meta, input) ?? request.ToolCall.Kind.ToString()
            : title;
        var destructive = IsDestructive(knownKind ?? request.ToolCall.Kind, meta);
        var mcpServer = GetMcpServer(meta, input);
        var requiresApproval = destructive
            || (mcpServer is not null && !trustedMcpServers.Contains(mcpServer));
        return new AgentPermissionRequest(action, Serialize(input), destructive, requiresApproval);
    }

    private static string? GetMcpServer(Dictionary<string, object>? meta, object? input)
    {
        if (!IsMcpTool(meta))
            return null;
        return ParseObject(input)?["server"]?.Value<string>();
    }

    private static string? GetMcpToolAction(Dictionary<string, object>? meta, object? input)
    {
        if (!IsMcpTool(meta))
            return null;
        var inputJson = ParseObject(input);
        if (inputJson is null)
            return "MCP tool";
        var server = inputJson["server"]?.Value<string>();
        var tool = inputJson["tool"]?.Value<string>();
        return string.IsNullOrWhiteSpace(server) || string.IsNullOrWhiteSpace(tool)
            ? "MCP tool"
            : $"MCP {server}/{tool}";
    }

    private static JObject? ParseObject(object? input)
    {
        try
        {
            return JToken.Parse(JsonConvert.SerializeObject(input)) as JObject;
        }
        catch (Newtonsoft.Json.JsonException)
        {
            return null;
        }
    }

    private static bool IsTrue(object value) => value switch
    {
        bool boolean => boolean,
        JsonElement element when element.ValueKind == JsonValueKind.True => true,
        JValue token when token.Type == JTokenType.Boolean => token.Value<bool>(),
        _ => bool.TryParse(value.ToString(), out var parsed) && parsed
    };

    private static bool IsDestructive(ToolKind kind, Dictionary<string, object>? meta)
        => !IsMcpTool(meta)
            && kind is (ToolKind.Edit or ToolKind.Delete or ToolKind.Move or ToolKind.Execute);

    private static bool IsMcpTool(Dictionary<string, object>? meta)
        => meta?.TryGetValue("is_mcp_tool_call", out var marker) == true && IsTrue(marker);

    private static string? Serialize(object? value) => value is null ? null : JsonConvert.SerializeObject(value);
}
