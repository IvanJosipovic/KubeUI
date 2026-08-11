using System.Collections.ObjectModel;
using KubeUI.AI.Acp;
using KubeUI.AI.Agents;
using KubeUI.AI.Permissions;

namespace KubeUI.AI.Configuration;

public sealed class AcpAgentRegistry : IAgentRegistry
{
    private readonly AcpAgentDefinition[] _definitions;
    private readonly List<IAgent> _agents = [];
    private readonly ReadOnlyCollection<IAgent> _readOnlyAgents;
    private readonly IAgentPermissionService? _permissionService;

    public AcpAgentRegistry(IEnumerable<AcpAgentDefinition> definitions, IAgentPermissionService? permissionService = null)
    {
        ArgumentNullException.ThrowIfNull(definitions);
        _definitions = [.. definitions];
        _permissionService = permissionService;
        _readOnlyAgents = _agents.AsReadOnly();
        RebuildAvailableAgents();
    }

    public IReadOnlyList<IAgent> Agents => _readOnlyAgents;

    private void RebuildAvailableAgents()
    {
        _agents.Clear();
        foreach (var definition in _definitions)
        {
            if (IsExecutableAvailable(definition.Executable))
                _agents.Add(new AcpAgent(definition, _permissionService));
        }
    }

    private static bool IsExecutableAvailable(string executable)
        => ExecutableLocator.Find(executable) is not null;
}
