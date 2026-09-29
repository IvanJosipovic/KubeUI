using System.Text.Json;
using dotacp.protocol;
using KubeUI.AI.Agents;

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
        var isMcpTool = IsMcpTool(meta);
        var mcpServer = GetMcpServer(meta, input);
        var requiresApproval = destructive
            || (isMcpTool && (mcpServer is null || !trustedMcpServers.Contains(mcpServer)));
        return new AgentPermissionRequest(action, Serialize(input), destructive, requiresApproval);
    }

    private static string? GetMcpServer(Dictionary<string, object>? meta, object? input)
    {
        if (!IsMcpTool(meta))
            return null;
        return GetStringProperty(ParseObject(input), "server");
    }

    private static string? GetMcpToolAction(Dictionary<string, object>? meta, object? input)
    {
        if (!IsMcpTool(meta))
            return null;
        var inputJson = ParseObject(input);
        if (inputJson is null)
            return "MCP tool";
        var server = GetStringProperty(inputJson, "server");
        var tool = GetStringProperty(inputJson, "tool");
        return string.IsNullOrWhiteSpace(server) || string.IsNullOrWhiteSpace(tool)
            ? "MCP tool"
            : $"MCP {server}/{tool}";
    }

    private static JsonElement? ParseObject(object? input) => AcpJsonValue.ParseObject(input);

    private static string? GetStringProperty(JsonElement? element, string propertyName) =>
        element is { } json && json.TryGetProperty(propertyName, out var property) && property.ValueKind == JsonValueKind.String
            ? property.GetString()
            : null;

    private static bool IsTrue(object value) => value switch
    {
        bool boolean => boolean,
        JsonElement element when element.ValueKind == JsonValueKind.True => true,
        _ => bool.TryParse(value.ToString(), out var parsed) && parsed
    };

    private static bool IsDestructive(ToolKind kind, Dictionary<string, object>? meta)
        => !IsMcpTool(meta)
            && kind is (ToolKind.Edit or ToolKind.Delete or ToolKind.Move or ToolKind.Execute);

    private static bool IsMcpTool(Dictionary<string, object>? meta)
        => meta?.TryGetValue("is_mcp_tool_call", out var marker) == true && IsTrue(marker);

    private static string? Serialize(object? value) => AcpJsonValue.Serialize(value);
}
