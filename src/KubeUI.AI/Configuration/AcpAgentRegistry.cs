using System.Collections.ObjectModel;
using KubeUI.AI.Acp;
using KubeUI.AI.Agents;
using KubeUI.AI.Permissions;

namespace KubeUI.AI.Configuration;

public sealed class AcpAgentRegistry : IAgentRegistry
{
    private readonly AcpAgentDefinition[] _definitions;
    private readonly Lazy<ReadOnlyCollection<IAgent>> _agents;
    private readonly IAgentPermissionService? _permissionService;

    public AcpAgentRegistry(IEnumerable<AcpAgentDefinition> definitions, IAgentPermissionService? permissionService = null)
    {
        ArgumentNullException.ThrowIfNull(definitions);
        _definitions = [.. definitions];
        _permissionService = permissionService;
        _agents = new Lazy<ReadOnlyCollection<IAgent>>(CreateAvailableAgents);
    }

    public IReadOnlyList<IAgent> Agents => _agents.Value;

    private ReadOnlyCollection<IAgent> CreateAvailableAgents()
    {
        List<IAgent> agents = [];
        foreach (var definition in _definitions)
        {
            if (IsExecutableAvailable(definition.Executable))
                agents.Add(new AcpAgent(definition, _permissionService));
        }

        return agents.AsReadOnly();
    }

    private static bool IsExecutableAvailable(string executable)
        => ExecutableLocator.Find(executable) is not null;
}
