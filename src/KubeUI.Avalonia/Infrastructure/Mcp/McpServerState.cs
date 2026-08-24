namespace KubeUI.Avalonia.Infrastructure.Mcp;

/// <summary>
/// Holds the endpoint the embedded MCP server actually bound to at runtime, which can differ from the
/// configured endpoint when the configured port was unavailable and an ephemeral port was used instead.
/// </summary>
public sealed class McpServerState
{
    private volatile string? _endpoint;

    /// <summary>
    /// Gets the live MCP endpoint once the embedded server has started; otherwise <c>null</c>.
    /// </summary>
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
