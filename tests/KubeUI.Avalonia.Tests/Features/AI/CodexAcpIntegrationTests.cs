using KubeUI.AI.Acp;
using KubeUI.AI.Agents;
using KubeUI.AI.Configuration;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.AI;

public sealed class CodexAcpIntegrationTests
{
    [Fact]
    public async Task codex_acp_can_initialize_create_session_prompt_stream_and_shutdown()
    {
        if (!string.Equals(Environment.GetEnvironmentVariable("KUBEUI_RUN_REAL_ACP"), "1", StringComparison.Ordinal))
            throw Xunit.Sdk.SkipException.ForSkip("Set KUBEUI_RUN_REAL_ACP=1 to run the external Codex ACP integration test.");

        var agent = new AcpAgent(new AcpAgentDefinition
        {
            Id = "codex",
            Name = "Codex",
            Executable = "npx",
            Arguments = ["-y", "@agentclientprotocol/codex-acp"],
            AuthenticationMethodId = "chat-gpt"
        });

        using var timeout = new CancellationTokenSource(TimeSpan.FromMinutes(2));
        await using var session = await agent.CreateSessionAsync(
            new AgentSessionOptions { WorkingDirectory = Directory.GetCurrentDirectory() }, timeout.Token);
        await session.PromptAsync("Reply with exactly: ACP integration ok", timeout.Token);

        var completed = false;
        await foreach (var item in session.Events.WithCancellation(timeout.Token))
        {
            if (item is AgentTurnCompletedEvent)
            {
                completed = true;
                break;
            }
        }

        completed.ShouldBeTrue();
    }
}
