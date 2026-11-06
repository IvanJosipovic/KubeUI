namespace KubeUI.Features.Clusters.Workspace;

/// <summary>Identifies which cached authorization decisions a refresh pass replaces.</summary>
internal enum PermissionRefreshScope
{
    /// <summary>Discard cluster-wide and namespace-scoped decisions.</summary>
    All = 1,

    /// <summary>Discard namespace-scoped decisions only.</summary>
    Namespaces = 2,
}

/// <summary>Coalesces permission-refresh triggers and runs them through a single debounced worker.</summary>
internal sealed class PermissionRefreshCoordinator : IDisposable
{
    private readonly Func<PermissionRefreshScope, CancellationToken, Task> _refreshAsync;
    private readonly Func<bool> _canRefresh;
    private readonly Func<CancellationToken, Task> _debounce;
    private readonly CancellationToken _disposalToken;
    private readonly Action<Exception>? _reportError;
    private readonly SemaphoreSlim _refreshGate = new(1, 1);
    private int _requestedScope;
    private int _workerRunning;
    private int _refreshInProgress;
    private int _disposed;

    /// <summary>Creates a coordinator that runs refresh passes supplied by its owner.</summary>
    /// <param name="refreshAsync">Performs one refresh pass for the requested scope.</param>
    /// <param name="canRefresh">Reports whether a refresh may start now.</param>
    /// <param name="debounce">Waits before a queued pass runs; tests supply an immediate gate.</param>
    /// <param name="disposalToken">Cancels queued and running passes when the owner is disposed.</param>
    /// <param name="reportError">Receives unexpected worker failures.</param>
    public PermissionRefreshCoordinator(
        Func<PermissionRefreshScope, CancellationToken, Task> refreshAsync,
        Func<bool> canRefresh,
        Func<CancellationToken, Task> debounce,
        CancellationToken disposalToken,
        Action<Exception>? reportError = null)
    {
        _refreshAsync = refreshAsync;
        _canRefresh = canRefresh;
        _debounce = debounce;
        _disposalToken = disposalToken;
        _reportError = reportError;
    }

    /// <summary>Gets whether a full refresh pass is currently replacing cached decisions.</summary>
    public bool IsRefreshInProgress => Volatile.Read(ref _refreshInProgress) != 0;

    /// <summary>Requests a refresh after an invalidation event, unless a full pass is already running.</summary>
    public void Invalidate()
    {
        if (IsRefreshInProgress || IsDisposed() || !_canRefresh())
        {
            return;
        }

        Request(PermissionRefreshScope.All);
    }

    /// <summary>Queues a refresh pass, combining its scope with any pass already queued.</summary>
    /// <param name="scope">Decisions the queued pass should replace.</param>
    public void Request(PermissionRefreshScope scope)
    {
        if (IsDisposed() || !_canRefresh())
        {
            return;
        }

        Interlocked.Or(ref _requestedScope, (int)scope);
        ScheduleWorker();
    }

    /// <summary>Runs one full refresh pass immediately, without debouncing.</summary>
    /// <param name="cancellationToken">Cancels the pass.</param>
    public async Task RefreshAsync(CancellationToken cancellationToken = default)
    {
        if (IsDisposed() || !_canRefresh())
        {
            return;
        }

        await RefreshCoreAsync(PermissionRefreshScope.All, cancellationToken).ConfigureAwait(false);
    }

    private bool IsDisposed() => Volatile.Read(ref _disposed) != 0;

    private void ScheduleWorker()
    {
        if (Interlocked.CompareExchange(ref _workerRunning, 1, 0) != 0)
        {
            return;
        }

        _ = Task.Run(RunWorkerAsync);
    }

    private async Task RunWorkerAsync()
    {
        var cancellationToken = _disposalToken;
        try
        {
            while (true)
            {
                var requested = Interlocked.Exchange(ref _requestedScope, 0);
                if (requested == 0)
                {
                    break;
                }

                await _debounce(cancellationToken).ConfigureAwait(false);
                if (!_canRefresh())
                {
                    continue;
                }

                var scope = (requested & (int)PermissionRefreshScope.All) != 0
                    ? PermissionRefreshScope.All
                    : PermissionRefreshScope.Namespaces;
                await RefreshCoreAsync(scope, cancellationToken).ConfigureAwait(false);
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            _reportError?.Invoke(ex);
        }
        finally
        {
            Interlocked.Exchange(ref _workerRunning, 0);
            if (!IsDisposed() && Volatile.Read(ref _requestedScope) != 0)
            {
                ScheduleWorker();
            }
        }
    }

    private async Task RefreshCoreAsync(PermissionRefreshScope scope, CancellationToken cancellationToken)
    {
        var isFullRefresh = scope == PermissionRefreshScope.All;
        await _refreshGate.WaitAsync(cancellationToken).ConfigureAwait(false);
        if (isFullRefresh)
        {
            Interlocked.Exchange(ref _refreshInProgress, 1);
        }

        try
        {
            await _refreshAsync(scope, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            if (isFullRefresh)
            {
                Interlocked.Exchange(ref _refreshInProgress, 0);
            }

            _refreshGate.Release();
        }
    }

    /// <summary>Stops scheduling new passes and prevents the worker from rescheduling itself.</summary>
    /// <remarks>
    /// <see cref="_refreshGate"/> is intentionally not disposed: an in-flight pass may still release it while
    /// disposal completes, and disposing it would turn that release into an <see cref="ObjectDisposedException"/>
    /// reported as a worker failure. The semaphore allocates no OS resources unless its wait handle is accessed,
    /// which never happens here.
    /// </remarks>
    public void Dispose()
    {
        Interlocked.Exchange(ref _disposed, 1);
        Interlocked.Exchange(ref _requestedScope, 0);
    }
}
