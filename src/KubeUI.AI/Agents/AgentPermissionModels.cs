namespace KubeUI.AI.Agents;

public sealed record AgentPermissionRequest(
    string Action,
    string? Resource = null,
    bool IsDestructive = false,
    bool RequiresApproval = false)
{
    /// <summary>Gets the authenticated client requesting permission, when available.</summary>
    public string? RequestingClient { get; init; }
}
public sealed record AgentPermissionResult(bool Allowed, string? Reason = null);
