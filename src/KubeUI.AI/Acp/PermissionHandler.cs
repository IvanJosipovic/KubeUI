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

    public async ValueTask<PermissionDecision> RequestPermissionAsync(
        RequestPermissionRequest request,
        CancellationToken cancellationToken = default)
    {
        using var activity = AgentActivitySource.Source.StartActivity("ai.permission.request");
        activity?.SetTag("agent.protocol", "acp");
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

    private static PermissionDecision SelectResponse(RequestPermissionRequest request, bool allowed)
    {
        var option = request.Options.FirstOrDefault(item => allowed
                ? item.Kind is "allow_once" or "allow_always"
                : item.Kind is "reject_once" or "reject_always");
        return option is null ? PermissionDecision.Cancel() : PermissionDecision.Select(option.OptionId);
    }
}
