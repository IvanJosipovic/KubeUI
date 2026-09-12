namespace KubeUI.AI.Configuration;

public sealed record AcpAgentDefinition
{
    public required string Id { get; init; }
    public required string Name { get; init; }
    public required string Executable { get; init; }
    public IReadOnlyList<string> Arguments { get; init; } = [];
    public string? AuthenticationMethodId { get; init; }
    public IReadOnlyDictionary<string, string?> Environment { get; init; } = new Dictionary<string, string?>();
    public IReadOnlyList<string> EnvironmentVariableNames { get; init; } = [];
}
