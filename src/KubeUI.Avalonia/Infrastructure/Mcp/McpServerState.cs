namespace KubeUI.Avalonia.Infrastructure.Mcp;

/// <summary>
/// Exposes the runtime state of the embedded MCP server.
/// </summary>
public interface IMcpServerState
{
    /// <summary>
    /// Gets the port the embedded MCP server actually bound to, or null when the server has not started
    /// and the configured port should be assumed.
    /// </summary>
    int? BoundPort { get; }
}

/// <summary>
/// Tracks the runtime state of the embedded MCP server; updated by the host after a successful bind.
/// </summary>
public sealed class McpServerState : IMcpServerState
{
    public int? BoundPort { get; private set; }

    /// <summary>
    /// Records the port the embedded MCP server bound to.
    /// </summary>
    public void SetBoundPort(int port) => BoundPort = port;
}
