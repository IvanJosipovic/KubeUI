namespace KubeUI.AI.Agents;

[Flags]
public enum AgentCapabilities
{
    None = 0,
    FileSystem = 1 << 0,
    Terminal = 1 << 1,
    Permissions = 1 << 2,
    Mcp = 1 << 3,
    Plans = 1 << 4,
    Usage = 1 << 5
}
