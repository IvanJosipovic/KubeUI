namespace KubeUI.AI.Agents;

public sealed record AgentSessionOptions
{
    public string? WorkingDirectory { get; init; }
    public IReadOnlyDictionary<string, string?> Environment { get; init; } = new Dictionary<string, string?>();
    public string? McpEndpoint { get; init; }
    public IReadOnlySet<string> TrustedMcpServers { get; init; } = new HashSet<string>(StringComparer.Ordinal);
    public AgentContext? Context { get; init; }
}
