namespace KubeUI.AI.Agents;

public interface IAgentRegistry
{
    IReadOnlyList<IAgent> Agents { get; }
}
