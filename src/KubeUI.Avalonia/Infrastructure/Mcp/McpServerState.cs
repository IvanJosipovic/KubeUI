namespace KubeUI.Avalonia.Infrastructure.Mcp;

/// <summary>
/// Exposes the runtime state of the embedded MCP server.
/// </summary>
public interface IMcpServerState
{
    /// <summary>
    /// Gets the endpoint the embedded MCP server actually bound to once it has started;
    /// otherwise <c>null</c> and the configured endpoint should be assumed.
    /// </summary>
    string? Endpoint { get; }
}

/// <summary>
/// Tracks the runtime state of the embedded MCP server; updated by the host after a successful bind.
/// The live endpoint can differ from the configured one when the configured port was unavailable
/// and an ephemeral port was used instead.
/// </summary>
public sealed class McpServerState : IMcpServerState
{
    private volatile string? _endpoint;

    public string? Endpoint => _endpoint;

    /// <summary>
    /// Records the endpoint reported by the hosting layer after the server bound successfully.
    /// </summary>
    /// <param name="endpoint">The full MCP endpoint URL, including the server path.</param>
    public void SetEndpoint(string endpoint)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(endpoint);
        _endpoint = endpoint;
    }
}
