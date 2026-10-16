namespace KubeUI.AI.Mcp;

/// <summary>Authenticated local MCP client and its cluster access grant. An empty external cluster set allows all clusters.</summary>
public sealed record McpClientIdentity(
    string Id,
    string Name,
    bool IsInternal,
    IReadOnlySet<string> AllowedClusters);

/// <summary>Ambient identity for the current stateless MCP request.</summary>
public sealed class McpRequestIdentityAccessor
{
    private readonly AsyncLocal<McpClientIdentity?> _current = new();

    /// <summary>Creates an empty request identity scope accessor for dependency injection.</summary>
    public McpRequestIdentityAccessor()
    {
    }

    internal McpRequestIdentityAccessor(McpClientIdentity initialIdentity)
    {
        _current.Value = initialIdentity;
    }

    /// <summary>Gets the current authenticated MCP caller.</summary>
    public McpClientIdentity? Current => _current.Value;

    /// <summary>Sets the authenticated identity for the current asynchronous request flow.</summary>
    /// <param name="identity">Identity authenticated by the MCP transport boundary.</param>
    /// <returns>Scope that restores the previous request identity when disposed.</returns>
    public IDisposable Enter(McpClientIdentity identity)
    {
        ArgumentNullException.ThrowIfNull(identity);
        var prior = _current.Value;
        _current.Value = identity;
        return new RestoreIdentity(_current, prior);
    }

    private sealed class RestoreIdentity(AsyncLocal<McpClientIdentity?> current, McpClientIdentity? prior) : IDisposable
    {
        private bool _disposed;

        public void Dispose()
        {
            if (_disposed)
                return;
            current.Value = prior;
            _disposed = true;
        }
    }
}
