using System.Collections.Concurrent;
using System.Diagnostics;
using System.Threading.Channels;
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
    private readonly AcpPermissionMapper _permissionMapper = new();
    private readonly ConcurrentDictionary<string, ToolCallContext> _toolCalls = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, byte> _startedToolCalls = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, byte> _completedToolCalls = new(StringComparer.Ordinal);

    public bool TrackToolCall(SessionUpdate update)
    {
        switch (update)
        {
            case ToolCall toolCall:
                return TrackToolCall(
                    toolCall.ToolCallId.ToString(),
                    toolCall.Status,
                    new(toolCall.Title, toolCall.RawInput, toolCall.Kind, toolCall.Meta));
            case SessionUpdateToolCallUpdate toolUpdate:
                var key = toolUpdate.ToolCallId.ToString();
                _toolCalls.AddOrUpdate(
                    key,
                    _ => new(toolUpdate.Title, toolUpdate.RawInput, toolUpdate.Kind, toolUpdate.Meta),
                    (_, previous) => new(
                        toolUpdate.Title ?? previous.Title,
                        toolUpdate.RawInput ?? previous.Input,
                        toolUpdate.Kind,
                        toolUpdate.Meta ?? previous.Meta));
                return TrackToolCall(key, toolUpdate.Status, _toolCalls[key]);
            default:
                return true;
        }
    }

    private bool TrackToolCall(string key, ToolCallStatus status, ToolCallContext context)
    {
        if (status is ToolCallStatus.Completed or ToolCallStatus.Failed)
        {
            _toolCalls.TryRemove(key, out _);
            _startedToolCalls.TryRemove(key, out _);
            return _completedToolCalls.TryAdd(key, 0);
        }

        _toolCalls[key] = context;
        return _startedToolCalls.TryAdd(key, 0);
    }

    public async Task<RequestPermissionResponse> RequestAsync(
        RequestPermissionRequest request,
        CancellationToken cancellationToken = default)
    {
        using var activity = AgentActivitySource.Source.StartActivity("ai.permission.request");
        activity?.SetTag("agent.protocol", "acp");
        activity?.SetTag("permission.action", request.ToolCall.Title);
        _toolCalls.TryGetValue(request.ToolCall.ToolCallId.ToString(), out var knownTool);
        var permissionRequest = _permissionMapper.Map(
            request,
            knownTool?.Title,
            knownTool?.Input,
            knownTool?.Kind,
            knownTool?.Meta,
            _trustedMcpServers);
        if (!permissionRequest.RequiresApproval)
            return SelectResponse(request, allowed: true, activity);

        events.TryWrite(new AgentPermissionRequestedEvent(permissionRequest));
        try
        {
            var permission = await permissionService.RequestPermissionAsync(permissionRequest, cancellationToken).ConfigureAwait(false);
            return SelectResponse(request, permission.Allowed, activity);
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            activity?.SetTag("permission.result", "cancelled");
            return new RequestPermissionResponse { Outcome = new RequestPermissionOutcomeCancelled() };
        }
    }

    public void Clear()
    {
        _toolCalls.Clear();
        _startedToolCalls.Clear();
        _completedToolCalls.Clear();
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
