namespace KubeUI.AI.Agents;

public abstract record AgentEvent;

public sealed record AgentMessageEvent(AgentMessage Message) : AgentEvent;
public sealed record AgentToolStartedEvent(AgentToolCall Tool) : AgentEvent;
public sealed record AgentToolCompletedEvent(AgentToolResult Result) : AgentEvent;
public sealed record AgentPlanChangedEvent(AgentPlan Plan) : AgentEvent;
public sealed record AgentPermissionRequestedEvent(AgentPermissionRequest Request) : AgentEvent;
public sealed record AgentUsageChangedEvent(AgentUsage Usage) : AgentEvent;
public sealed record AgentStatusEvent(string Text) : AgentEvent;
public sealed record AgentDiagnosticEvent(string Text) : AgentEvent;
public sealed record AgentTurnCompletedEvent : AgentEvent;
