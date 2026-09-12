using KubeUI.AI.Agents;

namespace KubeUI.AI.Permissions;

public interface IAgentPermissionService
{
    Task<AgentPermissionResult> RequestPermissionAsync(AgentPermissionRequest request, CancellationToken cancellationToken = default);
}

public sealed class DenyByDefaultAgentPermissionService : IAgentPermissionService
{
    public Task<AgentPermissionResult> RequestPermissionAsync(AgentPermissionRequest request, CancellationToken cancellationToken = default)
    {
        ArgumentNullException.ThrowIfNull(request);
        return Task.FromResult(new AgentPermissionResult(false, "No interactive permission service is configured."));
    }
}
