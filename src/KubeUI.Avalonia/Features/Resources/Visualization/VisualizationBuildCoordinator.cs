namespace KubeUI.Avalonia.Features.Resources.Visualization;

/// <summary>Coalesces visualization builds and cancels superseded work.</summary>
internal sealed class VisualizationBuildCoordinator<TRequest> : IDisposable
{
    private readonly Func<TRequest, int, CancellationToken, Task> _buildAndPublish;
    private TRequest? _pending;
    private int _pendingVersion;
    private bool _hasPending;
    private bool _running;
    private int _version;
    private CancellationTokenSource? _cancellation;
    private bool _disposed;

    public VisualizationBuildCoordinator(Func<TRequest, int, CancellationToken, Task> buildAndPublish)
    {
        _buildAndPublish = buildAndPublish ?? throw new ArgumentNullException(nameof(buildAndPublish));
    }

    public bool IsPendingOrRunning => _running || _hasPending;

    public int CurrentVersion => _version;

    public int Enqueue(TRequest request)
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        var version = ++_version;
        _pending = request;
        _pendingVersion = version;
        _hasPending = true;
        _cancellation?.Cancel();
        if (!_running)
        {
            _running = true;
            _ = ProcessAsync();
        }

        return version;
    }

    public int Invalidate()
    {
        ObjectDisposedException.ThrowIf(_disposed, this);
        _cancellation?.Cancel();
        return ++_version;
    }

    public void Clear()
    {
        if (_disposed)
        {
            return;
        }

        _hasPending = false;
        _pending = default;
        _cancellation?.Cancel();
        ++_version;
    }

    public bool IsCurrent(int version) => !_disposed && version == _version;

    private async Task ProcessAsync()
    {
        while (!_disposed && _hasPending)
        {
            var request = _pending!;
            var requestVersion = _pendingVersion;
            _pending = default;
            _hasPending = false;
            using CancellationTokenSource cancellation = new();
            _cancellation = cancellation;
            try
            {
                await _buildAndPublish(request, requestVersion, cancellation.Token).ConfigureAwait(true);
            }
            catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
            {
            }
            finally
            {
                if (ReferenceEquals(_cancellation, cancellation))
                {
                    _cancellation = null;
                }
            }
        }

        _running = false;
    }

    public void Dispose()
    {
        _disposed = true;
        _hasPending = false;
        _cancellation?.Cancel();
        _cancellation = null;
    }
}
