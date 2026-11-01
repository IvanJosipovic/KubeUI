using System.Text.Json;
using KubeUI.AI.Diagnostics;
using KubeUI.AI.Permissions;
using SvcSystems.AI.AgentClientProtocol;
using SvcSystems.AI.AgentClientProtocol.Client.Models;
using SvcSystems.AI.AgentClientProtocol.Models;

namespace KubeUI.AI.Acp;

internal sealed class PermissionHandler(
    IAgentPermissionService permissionService,
    IReadOnlySet<string> trustedMcpServers) : IPermissionHandler
{
    private readonly PermissionMapper _mapper = new();

    internal static AcpPermissionDiagnostics GetDiagnostics(RequestPermissionRequest request)
    {
        var toolCall = request.ToolCall;
        return new AcpPermissionDiagnostics(
            !string.IsNullOrWhiteSpace(toolCall.ToolCallId),
            toolCall.Name,
            toolCall.Title,
            toolCall.Kind,
            HasValue(toolCall.RawInput),
            GetValueKind(toolCall.RawInput),
            GetRawInputLength(toolCall.RawInput),
            GetMetadataKeys(toolCall.Meta),
            GetMetadataKeys(request.Meta));
    }

    public async ValueTask<PermissionDecision> RequestPermissionAsync(
        RequestPermissionRequest request,
        CancellationToken cancellationToken = default)
    {
        using var activity = AgentActivitySource.Source.StartActivity("ai.permission.request");
        activity?.SetTag("agent.protocol", "acp");
        var diagnostics = GetDiagnostics(request);
        activity?.SetTag("acp.permission.tool_call.id_present", diagnostics.ToolCallIdPresent);
        activity?.SetTag("acp.permission.tool_call.name", diagnostics.Name);
        activity?.SetTag("acp.permission.tool_call.title", diagnostics.Title);
        activity?.SetTag("acp.permission.tool_call.kind", diagnostics.Kind);
        activity?.SetTag("acp.permission.tool_call.raw_input_present", diagnostics.RawInputPresent);
        activity?.SetTag("acp.permission.tool_call.raw_input_kind", diagnostics.RawInputKind);
        activity?.SetTag("acp.permission.tool_call.raw_input_length", diagnostics.RawInputLength);
        activity?.SetTag("acp.permission.tool_call.metadata_keys", diagnostics.ToolCallMetadataKeys);
        activity?.SetTag("acp.permission.request.metadata_keys", diagnostics.RequestMetadataKeys);
        var permissionRequest = _mapper.Map(request, trustedMcpServers);
        activity?.SetTag("permission.action", permissionRequest.Action);
        if (!permissionRequest.RequiresApproval)
        {
            activity?.SetTag("permission.result", "allowed");
            return SelectResponse(request, allowed: true);
        }

        try
        {
            var permission = await permissionService.RequestPermissionAsync(permissionRequest, cancellationToken).ConfigureAwait(false);
            var decision = SelectResponse(request, permission.Allowed);
            activity?.SetTag("permission.result", decision.Cancelled
                ? "cancelled"
                : permission.Allowed ? "allowed" : "denied");
            return decision;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
            activity?.SetTag("permission.result", "cancelled");
            return PermissionDecision.Cancel();
        }
    }

    private static bool HasValue(JsonElement? value)
        => value is { ValueKind: not JsonValueKind.Undefined and not JsonValueKind.Null };

    private static string? GetValueKind(JsonElement? value)
        => HasValue(value) ? value!.Value.ValueKind.ToString() : null;

    private static int? GetRawInputLength(JsonElement? value)
        => HasValue(value) ? value!.Value.GetRawText().Length : null;

    private static string? GetMetadataKeys(IReadOnlyDictionary<string, JsonElement>? metadata)
        => metadata is null || metadata.Count == 0
            ? null
            : string.Join(",", metadata.Keys.Order(StringComparer.Ordinal));

    private static PermissionDecision SelectResponse(RequestPermissionRequest request, bool allowed)
    {
        var option = request.Options.FirstOrDefault(item => allowed
                ? item.Kind is "allow_once" or "allow_always"
                : item.Kind is "reject_once" or "reject_always");
        return option is null ? PermissionDecision.Cancel() : PermissionDecision.Select(option.OptionId);
    }
}

internal sealed record AcpPermissionDiagnostics(
    bool ToolCallIdPresent,
    string? Name,
    string? Title,
    string? Kind,
    bool RawInputPresent,
    string? RawInputKind,
    int? RawInputLength,
    string? ToolCallMetadataKeys,
    string? RequestMetadataKeys);
