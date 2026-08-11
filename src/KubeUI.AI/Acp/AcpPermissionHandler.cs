using System.Collections.Concurrent;
using System.Threading.Channels;
using System.Diagnostics;
using dotacp.protocol;
using KubeUI.AI.Agents;
using KubeUI.AI.Diagnostics;
using KubeUI.AI.Permissions;

namespace KubeUI.AI.Acp;

internal sealed class AcpPermissionHandler(
    ChannelWriter<AgentEvent> events,
    IAgentPermissionService permissionService,
    IReadOnlySet<string>? trustedMcpServers = null)
{
    private readonly IReadOnlySet<string> _trustedMcpServers = trustedMcpServers ?? new HashSet<string>(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, ToolCallContext> _toolCalls = new(StringComparer.Ordinal);

    public void TrackToolCall(SessionUpdate update)
    {
        switch (update)
        {
            case ToolCall toolCall:
                _toolCalls[toolCall.ToolCallId.ToString()] = new(toolCall.Title, toolCall.RawInput, toolCall.Kind, toolCall.Meta);
                break;
            case SessionUpdateToolCallUpdate toolUpdate when _toolCalls.TryGetValue(toolUpdate.ToolCallId.ToString(), out var previous):
                _toolCalls[toolUpdate.ToolCallId.ToString()] = new(
                    toolUpdate.Title ?? previous.Title,
                    toolUpdate.RawInput ?? previous.Input,
                    toolUpdate.Kind,
                    toolUpdate.Meta ?? previous.Meta);
                break;
        }
    }

    public async Task<RequestPermissionResponse> RequestAsync(
        RequestPermissionRequest request,
        CancellationToken cancellationToken = default)
    {
        using var activity = AgentActivitySource.Source.StartActivity("ai.permission.request");
        activity?.SetTag("agent.protocol", "acp");
        activity?.SetTag("permission.action", request.ToolCall.Title);
        _toolCalls.TryGetValue(request.ToolCall.ToolCallId.ToString(), out var knownTool);
        var permissionRequest = AcpMapper.ToPermissionRequest(
            request,
            knownTool?.Title,
            knownTool?.Input,
            knownTool?.Kind,
            knownTool?.Meta,
            _trustedMcpServers);
        if (!permissionRequest.RequiresApproval)
            return SelectResponse(request, allowed: true, activity);

        events.TryWrite(new AgentPermissionRequestedEvent(permissionRequest));
        var permission = await permissionService.RequestPermissionAsync(permissionRequest, cancellationToken).ConfigureAwait(false);
        return SelectResponse(request, permission.Allowed, activity);
    }

    private static RequestPermissionResponse SelectResponse(
        RequestPermissionRequest request,
        bool allowed,
        Activity? activity)
    {
        var option = request.Options.FirstOrDefault(item => allowed
            ? item.Kind is PermissionOptionKind.AllowOnce or PermissionOptionKind.AllowAlways
            : item.Kind is PermissionOptionKind.RejectOnce or PermissionOptionKind.RejectAlways);
        activity?.SetTag("permission.result", option is null ? "cancelled" : allowed ? "allowed" : "denied");
        return option is null
            ? new RequestPermissionResponse { Outcome = new RequestPermissionOutcomeCancelled() }
            : new RequestPermissionResponse { Outcome = new SelectedPermissionOutcome { OptionId = option.OptionId } };
    }

    private sealed record ToolCallContext(
        string? Title,
        object? Input,
        ToolKind Kind,
        Dictionary<string, object>? Meta);
}
