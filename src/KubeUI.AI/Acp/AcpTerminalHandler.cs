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
            new AgentPermissionRequest("run_process", FormatInvocation(request), IsDestructive: true), cancellationToken).ConfigureAwait(false);
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
            await Task.WhenAll(
                CaptureStreamAsync(terminal.Process.StandardOutput, terminal),
                CaptureStreamAsync(terminal.Process.StandardError, terminal)).ConfigureAwait(false);
            await terminal.Process.WaitForExitAsync().ConfigureAwait(false);
            terminal.ExitCode = terminal.Process.ExitCode >= 0 ? (uint)terminal.Process.ExitCode : null;
        }
        finally
        {
            terminal.Exited.TrySetResult();
        }
    }

    private static async Task CaptureStreamAsync(StreamReader reader, TerminalState terminal)
    {
        var buffer = new char[4096];
        var count = await reader.ReadAsync(buffer).ConfigureAwait(false);
        while (count > 0)
        {
            terminal.Append(buffer.AsSpan(0, count));
            count = await reader.ReadAsync(buffer).ConfigureAwait(false);
        }
    }

    private static string FormatInvocation(CreateTerminalRequest request)
    {
        var arguments = request.Args is { Length: > 0 }
            ? $" {string.Join(' ', request.Args)}"
            : string.Empty;
        var workingDirectory = string.IsNullOrWhiteSpace(request.Cwd)
            ? Environment.CurrentDirectory
            : request.Cwd;
        return $"{request.Command}{arguments} (cwd: {workingDirectory})";
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

        public void Append(ReadOnlySpan<char> value)
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
                    var length = 0;
                    while (length < value.Length)
                    {
                        var nextLength = char.IsHighSurrogate(value[length]) && length + 1 < value.Length
                            && char.IsLowSurrogate(value[length + 1]) ? 2 : 1;
                        if (Encoding.UTF8.GetByteCount(value.Slice(length, nextLength)) > remaining)
                            break;
                        length += nextLength;
                        remaining -= Encoding.UTF8.GetByteCount(value.Slice(length - nextLength, nextLength));
                    }
                    value = value[..length];
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
            try
            { if (!Process.HasExited) Process.Kill(entireProcessTree: true); }
            catch { }
            Process.Dispose();
        }
    }
}
