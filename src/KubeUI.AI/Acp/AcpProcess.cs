using System.Diagnostics;
using KubeUI.AI.Agents;
using KubeUI.AI.Configuration;

namespace KubeUI.AI.Acp;

internal sealed class AcpProcess(AcpAgentDefinition definition, AgentSessionOptions options) : IAcpProcess
{
    private Process? _process;

    public event Action<string>? ErrorReceived;
    public event Action? Exited;

    public Stream Input => _process?.StandardInput.BaseStream ?? throw new InvalidOperationException("ACP process is not running.");
    public Stream Output => _process?.StandardOutput.BaseStream ?? throw new InvalidOperationException("ACP process is not running.");

    public Task StartAsync(CancellationToken cancellationToken)
    {
        var startInfo = new ProcessStartInfo
        {
            FileName = ExecutableLocator.Find(definition.Executable) ?? definition.Executable,
            WorkingDirectory = options.WorkingDirectory ?? Environment.CurrentDirectory,
            RedirectStandardInput = true,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
            CreateNoWindow = true
        };
        foreach (var argument in definition.Arguments)
            startInfo.ArgumentList.Add(argument);
        foreach (var item in definition.Environment)
            startInfo.Environment[item.Key] = item.Value;
        foreach (var item in options.Environment)
            startInfo.Environment[item.Key] = item.Value;
        foreach (var name in definition.EnvironmentVariableNames)
        {
            var value = Environment.GetEnvironmentVariable(name);
            if (value is not null)
                startInfo.Environment[name] = value;
        }

        _process = new Process { StartInfo = startInfo, EnableRaisingEvents = true };
        _process.Exited += ProcessOnExited;
        if (!_process.Start())
            throw new InvalidOperationException($"Unable to start ACP agent '{definition.Id}'.");
        _ = DrainErrorAsync(_process.StandardError);
        return Task.CompletedTask;
    }

    private void ProcessOnExited(object? sender, EventArgs args) => Exited?.Invoke();

    private async Task DrainErrorAsync(StreamReader reader)
    {
        try
        {
            while (await reader.ReadLineAsync().ConfigureAwait(false) is { } line)
                ErrorReceived?.Invoke(line);
        }
        catch (ObjectDisposedException) { }
    }

    public async Task StopAsync(CancellationToken cancellationToken)
    {
        if (_process is null)
            return;
        if (!_process.HasExited)
            _process.Kill(entireProcessTree: true);
        await _process.WaitForExitAsync(cancellationToken).ConfigureAwait(false);
    }

    public async ValueTask DisposeAsync()
    {
        if (_process is not null)
        {
            try { await StopAsync(CancellationToken.None).ConfigureAwait(false); } catch { }
            _process.Dispose();
            _process = null;
        }
    }
}
