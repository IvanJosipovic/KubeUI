using Avalonia.Threading;
using k8s.Models;
using KubeUI.Kubernetes;

namespace KubeUI.Features.Resources.Workloads.v1.Pod.ViewModels;

internal interface IPodLogStreamSessionSink
{
    bool IsCurrent(PodLogStreamSession session);

    bool IsReconnectPending { get; }

    int OutputGeneration { get; }

    bool HasExistingOutput(PodLogReadOptions option);

    List<string> GetRecentMessages(PodLogReadOptions option);

    Task AppendOutputEntriesAsync(
        PodLogStreamSession session,
        IReadOnlyList<PodLogOutputEntry> entries,
        int outputGeneration);

    void AppendStatusLine(PodLogStreamSession session, PodLogOutputEntry entry, int outputGeneration);

    void OpenFailed(PodLogStreamSession session, PodLogReadOptions option, Exception exception);

    void ReadFailed(PodLogStreamSession session, Exception exception);

    void Disconnected(PodLogStreamSession session);

    void RequestReconnect(PodLogStreamSession session);
}

/// <summary>Owns opening, reading, reconnecting, cancellation, and reader lifetime for one pod-log session.</summary>
internal sealed class PodLogStreamSession : IDisposable
{
    private const int MaxOutputBatchSize = 128;
    private const int MaxAutomaticReconnectAttempts = 5;

    private readonly Lock _readersGate = new();
    private readonly List<StreamReader> _readers = [];
    private readonly CancellationTokenSource _cancellation = new();
    private readonly CancellationToken _cancellationToken;
    private readonly IClusterRuntime _cluster;
    private readonly IPodLogStreamClient _streamClient;
    private readonly ILogger<PodLogsViewModel> _logger;
    private readonly IPodLogStreamSessionSink _sink;
    private readonly Func<int, CancellationToken, Task> _automaticReconnectDelay;
    private int _remainingReaders;
    private int _disposed;
    private int _reconnectAttempts;
    private int _reconnectPending;

    public PodLogStreamSession(
        IClusterRuntime cluster,
        IPodLogStreamClient streamClient,
        ILogger<PodLogsViewModel> logger,
        IPodLogStreamSessionSink sink,
        Func<int, CancellationToken, Task> automaticReconnectDelay,
        int reconnectAttempts = 0)
    {
        _cluster = cluster;
        _streamClient = streamClient;
        _logger = logger;
        _sink = sink;
        _automaticReconnectDelay = automaticReconnectDelay;
        _reconnectAttempts = reconnectAttempts;
        _cancellationToken = _cancellation.Token;
    }

    public CancellationToken CancellationToken => _cancellationToken;

    public int RemainingReaders => Volatile.Read(ref _remainingReaders);

    public int ReconnectAttempts => Volatile.Read(ref _reconnectAttempts);

    public static Task DelayAutomaticReconnectAsync(int reconnectAttempt, CancellationToken cancellationToken)
    {
        var delaySeconds = Math.Min(30, 1 << Math.Min(reconnectAttempt - 1, 4));
        return Task.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken);
    }

    public void SetReaderCount(int count)
    {
        ArgumentOutOfRangeException.ThrowIfNegative(count);
        Volatile.Write(ref _remainingReaders, count);
    }

    public Task StartAsync(IReadOnlyList<PodLogReadOptions> options, PodLogSessionResolution resolution)
    {
        var openTasks = new Task[options.Count];
        for (var i = 0; i < options.Count; i++)
        {
            openTasks[i] = OpenAndReadAsync(options[i], resolution);
        }

        return Task.WhenAll(openTasks);
    }

    public bool TryRegister(StreamReader reader)
    {
        lock (_readersGate)
        {
            if (Volatile.Read(ref _disposed) != 0
                || _cancellation.IsCancellationRequested
                || !_sink.IsCurrent(this))
            {
                return false;
            }

            _readers.Add(reader);
            return true;
        }
    }

    public bool CompleteReader()
    {
        var remaining = Interlocked.Decrement(ref _remainingReaders);
        var isLastReader = remaining <= 0;
        if (isLastReader && !_sink.IsCurrent(this) && Volatile.Read(ref _disposed) != 0)
        {
            lock (_readersGate)
            {
                _cancellation.Dispose();
            }
        }

        if (isLastReader && Volatile.Read(ref _disposed) == 0 && _sink.IsCurrent(this))
        {
            Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (_sink.IsCurrent(this))
                {
                    _sink.Disconnected(this);
                }
            }, DispatcherPriority.Background);
        }

        return isLastReader;
    }

    public void Release(StreamReader reader)
    {
        bool ownsReader;
        lock (_readersGate)
        {
            ownsReader = _readers.Remove(reader);
        }

        if (ownsReader)
        {
            reader.Dispose();
        }
    }

    public void Dispose()
    {
        StreamReader[] readers;
        lock (_readersGate)
        {
            if (Interlocked.Exchange(ref _disposed, 1) != 0)
            {
                return;
            }

            try
            {
                _cancellation.Cancel();
            }
            catch (ObjectDisposedException)
            {
            }

            readers = _readers.ToArray();
            _readers.Clear();
        }

        for (var i = 0; i < readers.Length; i++)
        {
            readers[i].Dispose();
        }

        if (RemainingReaders <= 0)
        {
            lock (_readersGate)
            {
                _cancellation.Dispose();
            }
        }
    }

    private async Task OpenAndReadAsync(PodLogReadOptions option, PodLogSessionResolution resolution)
    {
        Stream? stream = null;
        StreamReader? reader = null;
        try
        {
            stream = await _streamClient.OpenAsync(_cluster, option, CancellationToken);
            reader = new StreamReader(stream);
            stream = null;
            if (!TryRegister(reader))
            {
                CompleteReader();
                return;
            }

            var registeredReader = reader;
            reader = null;
            _ = Task.Run(() => ReadLogsAsync(registeredReader, option, resolution));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to open pod log stream for {PodNamespace}/{PodName} container {ContainerName}.", option.PodNamespace, option.PodName, option.ContainerName);
            var isLastReader = CompleteReader();
            if (!_sink.IsReconnectPending && _sink.IsCurrent(this))
            {
                _sink.OpenFailed(this, option, ex);
            }

            if (!_sink.IsReconnectPending
                && ex is IOException or HttpRequestException
                && option.Follow
                && !option.Previous
                && !CancellationToken.IsCancellationRequested
                && !IsTerminalPod(resolution.Pod)
                && isLastReader)
            {
                ScheduleReconnect();
            }
        }
        finally
        {
            reader?.Dispose();
            stream?.Dispose();
        }
    }

    private async Task ReadLogsAsync(
        StreamReader reader,
        PodLogReadOptions option,
        PodLogSessionResolution resolution)
    {
        var cancellationToken = CancellationToken;
        var streamEnded = false;
        var transientReadFailure = false;
        var appendedOutput = false;
        List<PodLogOutputEntry> pendingOutput = [];
        List<string>? reconnectBuffer = _sink.HasExistingOutput(option) ? [] : null;
        var outputGeneration = _sink.OutputGeneration;
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var read = reader.ReadLineAsync(cancellationToken);
                if (!read.IsCompletedSuccessfully || pendingOutput.Count >= MaxOutputBatchSize)
                {
                    await FlushOutputEntriesAsync(pendingOutput, outputGeneration);
                    pendingOutput.Clear();
                }

                var log = await read;
                if (log is null)
                {
                    streamEnded = true;
                    break;
                }

                if (reconnectBuffer is null)
                {
                    QueueOutputEntry(pendingOutput, option, log);
                    appendedOutput = true;
                }
                else
                {
                    reconnectBuffer.Add(log);
                    if (TryFlushReconnectBuffer(reconnectBuffer, option, atEnd: false, out var lines))
                    {
                        reconnectBuffer = null;
                        for (var i = 0; i < lines.Count; i++)
                        {
                            QueueOutputEntry(pendingOutput, option, lines[i]);
                            if (pendingOutput.Count >= MaxOutputBatchSize)
                            {
                                await FlushOutputEntriesAsync(pendingOutput, outputGeneration);
                                pendingOutput.Clear();
                            }
                        }

                        appendedOutput |= lines.Count > 0;
                    }
                }
            }
        }
        catch (Exception ex) when (cancellationToken.IsCancellationRequested
            && ex is OperationCanceledException or IOException or ObjectDisposedException)
        {
        }
        catch (Exception ex) when (ex is IOException or HttpRequestException)
        {
            transientReadFailure = true;
            _logger.LogWarning(ex, "Pod log stream disconnected for {PodNamespace}/{PodName} container {ContainerName}.", option.PodNamespace, option.PodName, option.ContainerName);
            if (_sink.IsCurrent(this))
            {
                Dispatcher.UIThread.Post(() =>
                {
                    if (_sink.IsCurrent(this))
                    {
                        _sink.ReadFailed(this, ex);
                    }
                }, DispatcherPriority.Background);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to read pod log stream for {PodNamespace}/{PodName} container {ContainerName}.", option.PodNamespace, option.PodName, option.ContainerName);
            if (_sink.IsCurrent(this))
            {
                Dispatcher.UIThread.Post(() =>
                {
                    if (_sink.IsCurrent(this))
                    {
                        _sink.ReadFailed(this, ex);
                    }
                }, DispatcherPriority.Background);
            }
        }
        finally
        {
            try
            {
                if (reconnectBuffer is not null
                    && TryFlushReconnectBuffer(reconnectBuffer, option, atEnd: true, out var lines))
                {
                    for (var i = 0; i < lines.Count; i++)
                    {
                        QueueOutputEntry(pendingOutput, option, lines[i]);
                        if (pendingOutput.Count >= MaxOutputBatchSize)
                        {
                            await FlushOutputEntriesAsync(pendingOutput, outputGeneration);
                            pendingOutput.Clear();
                        }
                    }

                    appendedOutput |= lines.Count > 0;
                }

                await FlushOutputEntriesAsync(pendingOutput, outputGeneration);

                var isLastReader = CompleteReader();
                if (((streamEnded && appendedOutput) || transientReadFailure)
                    && ShouldReconnectAfterStreamEnd(reader, option, resolution.Pod, cancellationToken, isLastReader))
                {
                    ScheduleReconnect();
                }
            }
            finally
            {
                Release(reader);
            }
        }
    }

    private async Task FlushOutputEntriesAsync(IReadOnlyList<PodLogOutputEntry> entries, int outputGeneration)
    {
        if (entries.Count > 0)
        {
            await _sink.AppendOutputEntriesAsync(this, entries.ToArray(), outputGeneration);
        }
    }

    private bool ShouldReconnectAfterStreamEnd(
        StreamReader reader,
        PodLogReadOptions option,
        V1Pod resolvedPod,
        CancellationToken cancellationToken,
        bool isLastReader)
    {
        return option.Follow
            && !option.Previous
            && !cancellationToken.IsCancellationRequested
            && !reader.BaseStream.CanSeek
            && !IsTerminalPod(resolvedPod)
            && isLastReader;
    }

    private void ScheduleReconnect()
    {
        if (Interlocked.Exchange(ref _reconnectPending, 1) != 0)
        {
            return;
        }

        var reconnectAttempt = Interlocked.Increment(ref _reconnectAttempts);
        if (reconnectAttempt > MaxAutomaticReconnectAttempts)
        {
            Interlocked.Exchange(ref _reconnectPending, 0);
            return;
        }

        _ = Task.Run(async () =>
        {
            try
            {
                await _automaticReconnectDelay(reconnectAttempt, CancellationToken);
            }
            catch (OperationCanceledException)
            {
                Interlocked.Exchange(ref _reconnectPending, 0);
                return;
            }

            Dispatcher.UIThread.Post(() =>
            {
                Interlocked.Exchange(ref _reconnectPending, 0);
                if (_sink.IsCurrent(this))
                {
                    _sink.RequestReconnect(this);
                }
            }, DispatcherPriority.Background);
        }, CancellationToken.None);
    }

    private static bool IsTerminalPod(V1Pod pod) => pod.Status?.Phase is "Succeeded" or "Failed";

    private bool TryFlushReconnectBuffer(
        List<string> pending,
        PodLogReadOptions option,
        bool atEnd,
        out List<string> lines)
    {
        var history = _sink.GetRecentMessages(option);
        var overlap = GetSuffixPrefixOverlap(history, pending);
        if (!atEnd && overlap == pending.Count)
        {
            lines = [];
            return false;
        }

        lines = pending.Skip(overlap).ToList();
        return true;
    }

    private static int GetSuffixPrefixOverlap(IReadOnlyList<string> history, IReadOnlyList<string> pending)
    {
        var maximum = Math.Min(history.Count, pending.Count);
        for (var length = maximum; length > 0; length--)
        {
            var matches = true;
            for (var i = 0; i < length; i++)
            {
                if (!string.Equals(history[history.Count - length + i], pending[i], StringComparison.Ordinal))
                {
                    matches = false;
                    break;
                }
            }

            if (matches)
            {
                return length;
            }
        }

        return 0;
    }

    private static void QueueOutputEntry(List<PodLogOutputEntry> pendingOutput, PodLogReadOptions option, string message)
    {
        pendingOutput.Add(new PodLogOutputEntry(option.PodName, option.ContainerName, message));
    }
}
