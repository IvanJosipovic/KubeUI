namespace KubeUI.Features.AI.Agents;

public interface IAgentRegistry
{
    IReadOnlyList<IAgent> Agents { get; }
}
