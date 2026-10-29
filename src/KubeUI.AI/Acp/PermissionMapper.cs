using System.Text.Json;
using KubeUI.AI.Agents;
using SvcSystems.AI.AgentClientProtocol.Client.Models;
using SvcSystems.AI.AgentClientProtocol.Models;

namespace KubeUI.AI.Acp;

internal sealed class PermissionMapper
{
    public AgentPermissionRequest Map(RequestPermissionRequest request, IReadOnlySet<string> trustedMcpServers)
    {
        var input = request.ToolCall.RawInput;
        var metadata = request.ToolCall.Meta;
        var isMcpTool = IsMcpTool(metadata) || IsMcpTool(request.Meta);
        var server = GetStringProperty(input, "server");
        var tool = GetStringProperty(input, "tool");
        var mcpAction = isMcpTool && !string.IsNullOrWhiteSpace(server) && !string.IsNullOrWhiteSpace(tool)
            ? $"MCP {server}/{tool}"
            : null;
        var action = !string.IsNullOrWhiteSpace(request.ToolCall.Title)
            ? request.ToolCall.Title
            : mcpAction ?? request.ToolCall.Name ?? "ACP tool";

        // An MCP tool is only trusted when both the server and the tool identity are known.
        // Incomplete tool identity stays approval gated so a trusted server cannot be used to
        // reach an unidentified tool.
        var hasCompleteMcpIdentity = !string.IsNullOrWhiteSpace(server) && !string.IsNullOrWhiteSpace(tool);
        var trustedMcp = isMcpTool
            && hasCompleteMcpIdentity
            && trustedMcpServers.Contains(server!);

        return new AgentPermissionRequest(
            action,
            input?.GetRawText(),
            IsDestructive: IsDestructive(request.ToolCall.Kind),
            RequiresApproval: !trustedMcp);
    }

    private static bool IsDestructive(string? kind) => kind?.ToLowerInvariant() switch
    {
        "read" or "search" or "fetch" or "think" => false,
        "edit" or "delete" or "move" or "execute" => true,
        _ => true
    };

    private static bool IsMcpTool(IReadOnlyDictionary<string, JsonElement>? metadata)
        => metadata is not null
            && metadata.TryGetValue("is_mcp_tool_call", out var value)
            && (value.ValueKind == JsonValueKind.True
                || value.ValueKind == JsonValueKind.String
                    && bool.TryParse(value.GetString(), out var marker)
                    && marker);

    private static string? GetStringProperty(JsonElement? value, string propertyName)
        => value is { ValueKind: JsonValueKind.Object } json
            && json.TryGetProperty(propertyName, out var property)
            && property.ValueKind == JsonValueKind.String
                ? property.GetString()
                : null;
}
