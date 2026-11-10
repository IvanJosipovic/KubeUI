using KubeUI.Features.AI.Acp;
using KubeUI.Features.AI.Agents;
using KubeUI.Features.AI.Permissions;
using Shouldly;
using SvcSystems.AI.AgentClientProtocol.Client.Models;

namespace KubeUI.Features.AI.Tests.Acp;

internal static class TerminalRequests
{
    public static CreateTerminalRequest Create(string sessionId, string command, List<string>? args = null, ulong? outputByteLimit = null)
        => new()
        {
            SessionId = sessionId,
            Command = command,
            Args = args,
            OutputByteLimit = outputByteLimit,
        };

    public static WaitForTerminalExitRequest WaitForExit(string sessionId, string terminalId)
        => new() { SessionId = sessionId, TerminalId = terminalId };

    public static TerminalOutputRequest Output(string sessionId, string terminalId)
        => new() { SessionId = sessionId, TerminalId = terminalId };

    public static ReleaseTerminalRequest Release(string sessionId, string terminalId)
        => new() { SessionId = sessionId, TerminalId = terminalId };
}

public sealed class TerminalHandlerTests
{
    [Fact]
    public async Task CreateAsync_denied_by_permission_service_throws_and_starts_no_terminal()
    {
        var handler = new TerminalHandler(new ScriptedPermissionService(allowed: false, reason: "process denied"));
        await using var _ = handler;

        await Should.ThrowAsync<UnauthorizedAccessException>(async () => await handler.CreateAsync(
            TerminalRequests.Create("session", "dotnet", ["--version"]),
            TestContext.Current.CancellationToken));
    }

    [Fact]
    public async Task Terminal_capture_returns_output_and_exit_status_for_a_completed_process()
    {
        using var handler = new TerminalHandler(new ScriptedPermissionService(allowed: true));
        var terminalId = await handler.CreateAsync(
            TerminalRequests.Create("session", "dotnet", ["--version"]),
            TestContext.Current.CancellationToken);

        var exitStatus = await handler.WaitForExitAsync(
            TerminalRequests.WaitForExit("session", terminalId),
            TestContext.Current.CancellationToken);

        exitStatus.ExitCode.ShouldBe(0u);
        var output = await handler.GetOutputAsync(TerminalRequests.Output("session", terminalId));
        output.Truncated.ShouldBeFalse();
        output.Output.ShouldNotBeNullOrWhiteSpace();
        output.ExitStatus.ShouldNotBeNull().ExitCode.ShouldBe(0u);
    }

    [Fact]
    public async Task Output_exceeding_the_limit_is_truncated()
    {
        using var handler = new TerminalHandler(new ScriptedPermissionService(allowed: true));
        // dotnet --info emits several kilobytes; a 16-byte limit forces the truncation path.
        var terminalId = await handler.CreateAsync(
            TerminalRequests.Create("session", "dotnet", ["--info"], outputByteLimit: 16),
            TestContext.Current.CancellationToken);

        await handler.WaitForExitAsync(
            TerminalRequests.WaitForExit("session", terminalId),
            TestContext.Current.CancellationToken);

        var output = await handler.GetOutputAsync(TerminalRequests.Output("session", terminalId));
        output.Truncated.ShouldBeTrue();
        output.Output.Length.ShouldBeLessThanOrEqualTo(16);
    }

    [Fact]
    public async Task Nonzero_exit_code_is_reported()
    {
        using var handler = new TerminalHandler(new ScriptedPermissionService(allowed: true));
        var terminalId = await handler.CreateAsync(
            TerminalRequests.Create("session", "dotnet", ["build", "--tl:off", "-clp:ErrorsOnly", "this-path-does-not-exist.proj"]),
            TestContext.Current.CancellationToken);

        var exitStatus = await handler.WaitForExitAsync(
            TerminalRequests.WaitForExit("session", terminalId),
            TestContext.Current.CancellationToken);

        exitStatus.ExitCode.ShouldNotBeNull().ShouldNotBe(0u);
    }

    [Fact]
    public async Task Unknown_terminal_id_is_rejected_for_output_and_exit_wait()
    {
        using var handler = new TerminalHandler(new ScriptedPermissionService(allowed: true));

        await Should.ThrowAsync<InvalidOperationException>(() => handler.GetOutputAsync(
            TerminalRequests.Output("session", "missing")).AsTask());
        await Should.ThrowAsync<InvalidOperationException>(() => handler.WaitForExitAsync(
            TerminalRequests.WaitForExit("session", "missing"),
            TestContext.Current.CancellationToken).AsTask());
    }

    [Fact]
    public async Task Release_disposes_the_terminal_and_rejects_further_output_reads()
    {
        using var handler = new TerminalHandler(new ScriptedPermissionService(allowed: true));
        var terminalId = await handler.CreateAsync(
            TerminalRequests.Create("session", "dotnet", ["--version"]),
            TestContext.Current.CancellationToken);
        await handler.WaitForExitAsync(
            TerminalRequests.WaitForExit("session", terminalId),
            TestContext.Current.CancellationToken);

        await handler.ReleaseAsync(TerminalRequests.Release("session", terminalId));

        await Should.ThrowAsync<InvalidOperationException>(() => handler.GetOutputAsync(
            TerminalRequests.Output("session", terminalId)).AsTask());
    }

    private sealed class ScriptedPermissionService(bool allowed, string? reason = null) : IAgentPermissionService
    {
        public Task<AgentPermissionResult> RequestPermissionAsync(AgentPermissionRequest request, CancellationToken cancellationToken = default)
            => Task.FromResult(new AgentPermissionResult(allowed, reason));
    }
}
