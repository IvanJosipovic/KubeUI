namespace KubeUI.AI.Agents;

public interface IAgentSession : IAsyncDisposable
{
    string Id { get; }
    IAsyncEnumerable<AgentEvent> Events { get; }
    Task PromptAsync(string prompt, CancellationToken cancellationToken = default);
    Task CancelAsync(CancellationToken cancellationToken = default);
}
