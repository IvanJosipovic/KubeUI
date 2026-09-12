using KubeUI.AI.Agents;

namespace KubeUI.Avalonia.Features.AI;

public interface IAgentContextService
{
    AgentContext? Context { get; }
    event EventHandler? ContextChanged;
    void SetContext(object owner, AgentContext? context);
    void ClearContext(object owner);
}

public sealed class AgentContextService : IAgentContextService
{
    private object? _owner;
    private AgentContext? _context;

    public AgentContext? Context => _context;
    public event EventHandler? ContextChanged;

    public void SetContext(object owner, AgentContext? context)
    {
        ArgumentNullException.ThrowIfNull(owner);
        _owner = owner;
        _context = context;
        ContextChanged?.Invoke(this, EventArgs.Empty);
    }

    public void ClearContext(object owner)
    {
        ArgumentNullException.ThrowIfNull(owner);
        if (!ReferenceEquals(_owner, owner))
            return;
        _owner = null;
        _context = null;
        ContextChanged?.Invoke(this, EventArgs.Empty);
    }
}
