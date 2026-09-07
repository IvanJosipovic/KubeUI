namespace KubeUI.Avalonia.Infrastructure.Mcp;

/// <summary>
/// Tracks the port used by the embedded MCP server for the current process.
/// </summary>
public sealed class McpServerState
{
    /// <summary>
    /// Gets the bound port, or null before the host starts.
    /// </summary>
    public int? BoundPort { get; set; }
}
