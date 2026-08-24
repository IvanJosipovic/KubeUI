using System.Text.Json;
using dotacp.protocol;
using KubeUI.AI.Agents;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace KubeUI.AI.Acp;

internal static class AcpMapper
{
    public static AgentPermissionRequest ToPermissionRequest(
        RequestPermissionRequest request,
        string? knownTitle = null,
        object? knownInput = null,
        ToolKind? knownKind = null,
        Dictionary<string, object>? knownMeta = null,
        IReadOnlySet<string>? trustedMcpServers = null)
    {
        var title = request.ToolCall.Title ?? knownTitle;
        var input = request.ToolCall.RawInput ?? knownInput;
        var meta = request.ToolCall.Meta ?? knownMeta;
        var action = string.IsNullOrWhiteSpace(title)
            ? GetMcpToolAction(meta, input) ?? request.ToolCall.Kind.ToString()
            : title;
        var resource = Serialize(input);
        var destructive = IsDestructive(knownKind ?? request.ToolCall.Kind, meta);
        var mcpServer = GetMcpServer(meta, input);
        var requiresApproval = destructive
            || (mcpServer is not null && !(trustedMcpServers?.Contains(mcpServer) ?? false));
        return new AgentPermissionRequest(action, resource, destructive, requiresApproval);
    }

    private static string? GetMcpServer(Dictionary<string, object>? meta, object? input)
    {
        if (!IsMcpTool(meta))
            return null;

        var inputObject = JsonConvert.SerializeObject(input);
        var inputJson = JObject.Parse(inputObject);
        return inputJson["server"]?.Value<string>();
    }

    private static string? GetMcpToolAction(Dictionary<string, object>? meta, object? input)
    {
        if (meta?.TryGetValue("is_mcp_tool_call", out var marker) != true || !IsTrue(marker))
            return null;

        var inputObject = JsonConvert.SerializeObject(input);
        var inputJson = JObject.Parse(inputObject);
        var server = inputJson["server"]?.Value<string>();
        var tool = inputJson["tool"]?.Value<string>();
        if (string.IsNullOrWhiteSpace(server) || string.IsNullOrWhiteSpace(tool))
            return "MCP tool";

        return $"MCP {server}/{tool}";
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

    public static AgentEvent? ToAgentEvent(SessionUpdate update)
    {
        switch (update)
        {
            case SessionUpdateAgentMessageChunk message when GetText(message.Content) is { Length: > 0 } text:
                return new AgentMessageEvent(new AgentMessage("assistant", text));
            case SessionUpdateAgentThoughtChunk thought when GetText(thought.Content) is { Length: > 0 } text:
                return new AgentStatusEvent(text);
            case SessionUpdateUserMessageChunk user when GetText(user.Content) is { Length: > 0 } text:
                return new AgentMessageEvent(new AgentMessage("user", text));
            case ToolCall toolCall:
            {
                var name = toolCall.Title ?? toolCall.Kind.ToString();
                return toolCall.Status is ToolCallStatus.Completed or ToolCallStatus.Failed
                    ? new AgentToolCompletedEvent(new AgentToolResult(name, toolCall.Status == ToolCallStatus.Completed, Serialize(toolCall.RawOutput)))
                    : new AgentToolStartedEvent(new AgentToolCall(name, Serialize(toolCall.RawInput)));
            }
            case SessionUpdateToolCallUpdate tool:
            {
                var name = tool.Title ?? tool.Kind.ToString();
                return tool.Status is ToolCallStatus.Completed or ToolCallStatus.Failed
                    ? new AgentToolCompletedEvent(new AgentToolResult(name, tool.Status == ToolCallStatus.Completed, Serialize(tool.RawOutput)))
                    : new AgentToolStartedEvent(new AgentToolCall(name, Serialize(tool.RawInput)));
            }
            case Plan plan:
                return new AgentPlanChangedEvent(new AgentPlan(
                    plan.Entries?.Select(static entry => entry.Content).Where(static content => content is not null).Cast<string>().ToArray()
                    ?? []));
            case UsageUpdate usage:
                return new AgentUsageChangedEvent(new AgentUsage(0, 0, checked((long)usage.Used)));
            case AvailableCommandsUpdate commands:
                return new AgentStatusEvent($"Available commands: {string.Join(", ", commands.AvailableCommands?.Select(command => command.Name) ?? [])}");
            case ConfigOptionUpdate options:
                return new AgentStatusEvent($"Configuration options: {string.Join(", ", options.ConfigOptions?.Select(option => option.Name) ?? [])}");
            case CurrentModeUpdate mode:
                return new AgentStatusEvent($"Current mode: {mode.CurrentModeId}");
            case SessionInfoUpdate info:
                return new AgentStatusEvent(string.IsNullOrWhiteSpace(info.Title) ? "Session information updated" : info.Title);
            default:
                return null;
        }
    }

    private static string? GetText(ContentBlock? content) => content switch
    {
        TextContent text => text.Text,
        _ => null
    };

    private static string? Serialize(object? value) => value is null ? null : JsonConvert.SerializeObject(value);
}
