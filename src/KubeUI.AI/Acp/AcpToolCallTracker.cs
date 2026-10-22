using SvcSystems.AI.AgentClientProtocol;
using SvcSystems.AI.AgentClientProtocol.Models;

namespace KubeUI.AI.Acp;

/// <summary>
/// Tracks tool-call identity across ACP <see cref="ToolCall"/> and <see cref="ToolCallUpdate"/>
/// updates so a repeated start or completion for the same tool call is forwarded only once.
/// </summary>
internal sealed class AcpToolCallTracker
{
    private readonly HashSet<string> _startedToolCalls = new(StringComparer.Ordinal);
    private readonly HashSet<string> _completedToolCalls = new(StringComparer.Ordinal);

    /// <summary>
    /// Determines whether an ACP session event should be forwarded to the host.
    /// </summary>
    /// <param name="update">Event received from the agent runtime.</param>
    /// <returns><see langword="true"/> when the event is new and should be forwarded.</returns>
    public bool ShouldForward(SessionEvent update)
    {
        ArgumentNullException.ThrowIfNull(update);

        if (update is SessionUpdateEvent { Update.Value: ToolCall tool })
        {
            return _startedToolCalls.Add(tool.ToolCallId);
        }

        if (update is SessionUpdateEvent { Update.Value: ToolCallUpdate toolUpdate }
            && toolUpdate.Status is "completed" or "failed")
        {
            if (!_completedToolCalls.Add(toolUpdate.ToolCallId))
                return false;
            _startedToolCalls.Remove(toolUpdate.ToolCallId);
        }

        return true;
    }
}
