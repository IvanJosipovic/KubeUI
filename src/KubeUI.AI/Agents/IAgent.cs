namespace KubeUI.AI.Agents;

public interface IAgent
{
    string Id { get; }
    string Name { get; }
    Task<IAgentSession> CreateSessionAsync(AgentSessionOptions options, CancellationToken cancellationToken = default);
}
