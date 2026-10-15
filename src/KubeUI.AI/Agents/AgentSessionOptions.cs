namespace KubeUI.AI.Agents;

public sealed record AgentSessionOptions
{
    public string? WorkingDirectory { get; init; }
    public IReadOnlyDictionary<string, string?> Environment { get; init; } = new Dictionary<string, string?>();
    public string? McpEndpoint { get; init; }
    /// <summary>Gets HTTP headers forwarded only to the configured MCP endpoint.</summary>
    public IReadOnlyDictionary<string, string> McpHeaders { get; init; } = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
    /// <summary>Credential lease transferred to the session and released when setup fails or the session ends.</summary>
    public IDisposable? McpCredentialLease { get; init; }
    public IReadOnlySet<string> TrustedMcpServers { get; init; } = new HashSet<string>(StringComparer.Ordinal);
    /// <summary>
    /// Gets the filesystem roots allowed for ACP file operations. The default set is empty and uses case-insensitive string comparison.
    /// </summary>
    public IReadOnlySet<string> FileSystemRoots { get; init; } = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
    public AgentContext? Context { get; init; }

    /// <inheritdoc />
    public override string ToString() => "AgentSessionOptions { credentials redacted }";
}
