namespace KubeUI.Features.AI.Mcp;

/// <summary>
/// Tracks the runtime state of the embedded MCP server; updated by the host after a successful bind.
/// </summary>
public sealed partial class McpServerState : ObservableObject, IMcpServerState
{
    [ObservableProperty]
    public partial int? BoundPort { get; private set; }

    /// <summary>
    /// Records the port the embedded MCP server bound to.
    /// </summary>
    public void SetBoundPort(int? port) => BoundPort = port;
}
