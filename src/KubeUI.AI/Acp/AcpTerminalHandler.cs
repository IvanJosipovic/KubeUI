using System.Diagnostics;
using System.Text;
using dotacp.protocol;
using KubeUI.AI.Agents;
using KubeUI.AI.Diagnostics;
using KubeUI.AI.Permissions;

namespace KubeUI.AI.Acp;

internal sealed class AcpTerminalHandler(IAgentPermissionService permissionService) : IDisposable
{
    private readonly System.Collections.Concurrent.ConcurrentDictionary<string, TerminalState> _terminals = new(StringComparer.Ordinal);

    public async Task<CreateTerminalResponse> CreateAsync(CreateTerminalRequest request, CancellationToken cancellationToken = default)
    {
        using var activity = AgentActivitySource.Source.StartActivity("ai.tool.execute");
        activity?.SetTag("agent.protocol", "acp");
        activity?.SetTag("tool.name", request.Command);
        var permission = await permissionService.RequestPermissionAsync(
            new AgentPermissionRequest("run_process", request.Command, IsDestructive: true), cancellationToken).ConfigureAwait(false);
        if (!permission.Allowed)
        {
            activity?.SetTag("permission.result", "denied");
            throw new UnauthorizedAccessException(permission.Reason ?? $"Running '{request.Command}' was denied.");
        }
        activity?.SetTag("permission.result", "allowed");

        var startInfo = new ProcessStartInfo
        {
            FileName = request.Command,
            WorkingDirectory = string.IsNullOrWhiteSpace(request.Cwd) ? Environment.CurrentDirectory : request.Cwd,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        foreach (var argument in request.Args ?? [])
            startInfo.ArgumentList.Add(argument);
        foreach (var variable in request.Env ?? [])
            startInfo.Environment[variable.Name] = variable.Value;
#pragma warning disable CA2000 // TerminalState owns the process after successful startup.
        var process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
#pragma warning restore CA2000
        try
        {
            if (!process.Start())
                throw new InvalidOperationException($"Unable to start terminal command '{request.Command}'.");
        }
        catch
        {
            process.Dispose();
            throw;
        }

        var terminalId = Guid.NewGuid().ToString("N");
        var terminal = new TerminalState(process, request.OutputByteLimit);
        _terminals[terminalId] = terminal;
        _ = CaptureOutputAsync(terminal);
        return new CreateTerminalResponse { TerminalId = terminalId };
    }

    public Task<KillTerminalResponse> KillAsync(KillTerminalRequest request, CancellationToken _ = default)
    {
        if (_terminals.TryGetValue(request.TerminalId, out var terminal) && !terminal.Process.HasExited)
            terminal.Process.Kill(entireProcessTree: true);
        return Task.FromResult(new KillTerminalResponse());
    }

    public Task<TerminalOutputResponse> OutputAsync(TerminalOutputRequest request, CancellationToken _ = default)
    {
        if (!_terminals.TryGetValue(request.TerminalId, out var terminal))
            throw new InvalidOperationException($"Terminal '{request.TerminalId}' was not found.");
        return Task.FromResult(terminal.ToResponse());
    }

    public Task<ReleaseTerminalResponse> ReleaseAsync(ReleaseTerminalRequest request, CancellationToken _ = default)
    {
        if (_terminals.TryRemove(request.TerminalId, out var terminal))
            terminal.Dispose();
        return Task.FromResult(new ReleaseTerminalResponse());
    }

    public async Task<WaitForTerminalExitResponse> WaitAsync(WaitForTerminalExitRequest request, CancellationToken cancellationToken = default)
    {
        if (!_terminals.TryGetValue(request.TerminalId, out var terminal))
            throw new InvalidOperationException($"Terminal '{request.TerminalId}' was not found.");
        await terminal.Exited.Task.WaitAsync(cancellationToken).ConfigureAwait(false);
        return new WaitForTerminalExitResponse { ExitCode = terminal.ExitCode };
    }

    public void Dispose()
    {
        foreach (var item in _terminals)
            item.Value.Dispose();
        _terminals.Clear();
    }

    private static async Task CaptureOutputAsync(TerminalState terminal)
    {
        try
        {
            var stdout = terminal.Process.StandardOutput.ReadToEndAsync();
            var stderr = terminal.Process.StandardError.ReadToEndAsync();
            await Task.WhenAll(stdout, stderr).ConfigureAwait(false);
            terminal.Append(stdout.Result);
            terminal.Append(stderr.Result);
            await terminal.Process.WaitForExitAsync().ConfigureAwait(false);
            terminal.ExitCode = terminal.Process.ExitCode >= 0 ? (uint)terminal.Process.ExitCode : null;
        }
        finally
        {
            terminal.Exited.TrySetResult();
        }
    }

    private sealed class TerminalState(Process process, ulong? outputByteLimit) : IDisposable
    {
        private readonly object _gate = new();
        private readonly StringBuilder _output = new();
        private readonly ulong? _outputByteLimit = outputByteLimit;

        public Process Process { get; } = process;
        public TaskCompletionSource Exited { get; } = new(TaskCreationOptions.RunContinuationsAsynchronously);
        public uint? ExitCode { get; set; }
        public bool Truncated { get; private set; }

        public void Append(string value)
        {
            lock (_gate)
            {
                var remaining = _outputByteLimit is null
                    ? int.MaxValue
                    : (long)_outputByteLimit.Value - Encoding.UTF8.GetByteCount(_output.ToString());
                if (remaining <= 0)
                {
                    Truncated = true;
                    return;
                }
                if (Encoding.UTF8.GetByteCount(value) > remaining)
                {
                    value = value[..Math.Min(value.Length, (int)remaining)];
                    Truncated = true;
                }
                _output.Append(value);
            }
        }

        public TerminalOutputResponse ToResponse()
        {
            lock (_gate)
            {
                return new TerminalOutputResponse
                {
                    Output = _output.ToString(),
                    Truncated = Truncated,
                    ExitStatus = new TerminalExitStatus { ExitCode = ExitCode }
                };
            }
        }

        public void Dispose()
        {
            try { if (!Process.HasExited) Process.Kill(entireProcessTree: true); } catch { }
            Process.Dispose();
        }
    }
}
