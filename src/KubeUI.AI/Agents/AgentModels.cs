namespace KubeUI.AI.Agents;

public sealed record AgentToolCall(string Name, string? Input = null);
public sealed record AgentToolResult(string Name, bool Succeeded, string? Output = null);
public sealed record AgentPermissionRequest(
    string Action,
    string? Resource = null,
    bool IsDestructive = false,
    bool RequiresApproval = false);
public sealed record AgentPermissionResult(bool Allowed, string? Reason = null);
public sealed record AgentPlan(IReadOnlyList<string> Steps);
public sealed record AgentUsage(long InputTokens, long OutputTokens, long TotalTokens = 0);
