using System.Collections.ObjectModel;
using KubeUI.Features.AI.Acp;
using KubeUI.Features.AI.Agents;
using KubeUI.Features.AI.Permissions;

namespace KubeUI.Features.AI.Configuration;

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
                _agents.Add(new Agent(definition, _permissionService));
        }
    }

    private static bool IsExecutableAvailable(string executable)
        => ExecutableLocator.Find(executable) is not null;
}
