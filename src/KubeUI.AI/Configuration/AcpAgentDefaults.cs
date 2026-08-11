namespace KubeUI.AI.Configuration;

public static class AcpAgentDefaults
{
    public static IReadOnlyList<AcpAgentDefinition> Definitions { get; } =
    [
        new AcpAgentDefinition
        {
            Id = "codex",
            Name = "Codex",
            Executable = "npx",
            Arguments = ["-y", "@agentclientprotocol/codex-acp"],
            AuthenticationMethodId = "chat-gpt"
        },
        new AcpAgentDefinition
        {
            Id = "copilot",
            Name = "GitHub Copilot",
            Executable = "copilot",
            Arguments = ["--acp"]
        },
        new AcpAgentDefinition
        {
            Id = "claude",
            Name = "Claude Code",
            Executable = "claude",
            Arguments = ["--acp"]
        },
        new AcpAgentDefinition
        {
            Id = "gemini",
            Name = "Gemini",
            Executable = "gemini",
            Arguments = ["--acp"]
        }
    ];
}
