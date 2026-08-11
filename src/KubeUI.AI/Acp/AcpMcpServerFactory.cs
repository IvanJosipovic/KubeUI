using dotacp.protocol;
using KubeUI.AI.Agents;

namespace KubeUI.AI.Acp;

internal static class AcpMcpServerFactory
{
    public static McpServer Create(IAgent agent, string endpoint)
    {
        return string.Equals(agent.Id, "copilot", StringComparison.Ordinal)
            ? new McpServerHttp { Name = "kubeui", Url = endpoint, Headers = [] }
            : new McpServerHttp { Name = "kubeui", Url = endpoint };
    }
}
