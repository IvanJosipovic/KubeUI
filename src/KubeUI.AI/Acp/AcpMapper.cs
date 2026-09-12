using dotacp.protocol;
using KubeUI.AI.Agents;
using Newtonsoft.Json;

namespace KubeUI.AI.Acp;

internal static class AcpMapper
{
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
