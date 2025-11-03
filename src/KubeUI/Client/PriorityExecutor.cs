using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Text;
using System.Threading.Channels;

namespace KubeUI.Client;

public enum WorkPriority { High, Normal, Low }

public sealed class PriorityExecutor : IAsyncDisposable, IPriorityExecutor
{
    private readonly ILogger<PriorityExecutor> _logger;

    private readonly Channel<Func<CancellationToken, Task>> _high = Channel.CreateUnbounded<Func<CancellationToken, Task>>();

    private readonly Channel<Func<CancellationToken, Task>> _normal = Channel.CreateUnbounded<Func<CancellationToken, Task>>();

    private readonly Channel<Func<CancellationToken, Task>> _low = Channel.CreateUnbounded<Func<CancellationToken, Task>>();

    private readonly CancellationTokenSource _cts = new();
    private readonly List<Task> _workers = [];

    public PriorityExecutor(ILogger<PriorityExecutor> logger, int workers)
    {
        _logger = logger;

        for (int i = 0; i < workers; i++)
            _workers.Add(Task.Run(() => Worker(_cts.Token)));
    }

    public ValueTask Enqueue(Func<CancellationToken, Task> work, WorkPriority priority = WorkPriority.Normal)
        => priority switch
        {
            WorkPriority.High => _high.Writer.WriteAsync(work),
            WorkPriority.Low => _low.Writer.WriteAsync(work),
            _ => _normal.Writer.WriteAsync(work)
        };

    private async Task Worker(CancellationToken ct)
    {
        while (!ct.IsCancellationRequested)
        {
            // Phase 1. Try to read in priority order
            if (_high.Reader.TryRead(out var job) ||
                _normal.Reader.TryRead(out job) ||
                _low.Reader.TryRead(out job))
            {
                try
                {
                    await job(ct);
                }
                catch (OperationCanceledException ex)
                {
                    _logger.LogInformation("Task was cancelled {ex}", ex);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Task threw an error");
                }

                continue;
            }

            // Phase 2. Nothing available. Wait for any to become readable
            // Exit if all channels completed
            if (_high.Reader.Completion.IsCompleted &&
                _normal.Reader.Completion.IsCompleted &&
                _low.Reader.Completion.IsCompleted)
            {
                break;
            }

            var tHigh = _high.Reader.WaitToReadAsync(ct).AsTask();
            var tNorm = _normal.Reader.WaitToReadAsync(ct).AsTask();
            var tLow = _low.Reader.WaitToReadAsync(ct).AsTask();

            await Task.WhenAny(tHigh, tNorm, tLow);
        }
    }

    public async ValueTask DisposeAsync()
    {
        _cts.Cancel();
        _high.Writer.TryComplete();
        _normal.Writer.TryComplete();
        _low.Writer.TryComplete();
        try
        { await Task.WhenAll(_workers); }
        catch { }
        _cts.Dispose();
    }
}
