using System.IO;
using System.Text;
using Avalonia.Threading;
using AvaloniaEdit.Document;
using k8s.Models;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Pod.ViewModels;

public sealed partial class PodLogsViewModel
{
    private const int MaxOutputBatchSize = 128;

    private void AppendStatusLine(string podName, string containerName, string message, CancellationTokenSource connectionCts)
    {
        if (string.IsNullOrWhiteSpace(message) || !IsCurrentConnection(connectionCts))
        {
            return;
        }

        PodLogOutputEntry entry = new(podName, containerName, message);
        var outputGeneration = Volatile.Read(ref _outputGeneration);
        Dispatcher.UIThread.InvokeAsync(
            () => AppendOutputEntry(entry, connectionCts, outputGeneration),
            DispatcherPriority.Background);
    }

    private bool DecrementActiveReaders(CancellationTokenSource connectionCts)
    {
        var isCurrentConnection = IsCurrentConnection(connectionCts);
        var isLastReaderForConnection = false;
        if (_readerCounts.TryGetValue(connectionCts, out var remainingReaders))
        {
            remainingReaders = _readerCounts.AddOrUpdate(connectionCts, 0, static (_, count) => count - 1);
            if (remainingReaders <= 0 && _readerCounts.TryRemove(connectionCts, out _))
            {
                isLastReaderForConnection = true;
                if (!isCurrentConnection)
                {
                    connectionCts.Dispose();
                }
            }
        }

        if (!isCurrentConnection)
        {
            return false;
        }

        Interlocked.Decrement(ref _activeReaderCount);
        if (!isLastReaderForConnection)
        {
            return false;
        }

        Dispatcher.UIThread.InvokeAsync(
            () =>
            {
                if (IsCurrentConnection(connectionCts))
                {
                    IsConnected = false;
                }
            },
            DispatcherPriority.Background);

        return true;
    }

    private string BuildSuggestedFileName()
    {
        if (IsMultiScope)
        {
            return $"kubeui-logs-{_scopeItems.Count}-resources-{DateTime.UtcNow:yyyyMMdd-HHmmss}.log";
        }

        var podName = Object?.Metadata?.Name ?? "pod";
        var containerName = string.IsNullOrWhiteSpace(ContainerName) ? "logs" : ContainerName;
        var namespaceName = Object?.Metadata?.NamespaceProperty;

        var fileName = namespaceName is { Length: > 0 }
            ? $"{namespaceName}-{podName}-{containerName}.log"
            : $"{podName}-{containerName}.log";

        return fileName.ReplaceInvalidFileNameChars();
    }

    private string BuildExportContent()
    {
        if (!IsMultiScope)
        {
            PodLogOutputEntry[] singleScopeEntries;
            lock (_outputEntriesGate)
            {
                singleScopeEntries = _outputEntries.ToArray();
            }

            return singleScopeEntries.Length == 0 ? Logs.Text : BuildOutputText(singleScopeEntries);
        }

        StringBuilder builder = new();
        builder.AppendLine("# KubeUI multi-resource log export");
        builder.AppendLine($"# Exported: {DateTimeOffset.UtcNow:O}");
        builder.AppendLine($"# Resources: {_scopeItems.Count}");
        builder.AppendLine($"# Resolved Pods: {AvailablePods.Count}");
        builder.AppendLine($"# Streams: {PlannedStreamCount}");
        builder.AppendLine("# Cross-stream lines are shown in arrival order.");
        for (var i = 0; i < _scopeItems.Count; i++)
        {
            builder.Append("# - ");
            builder.AppendLine(_scopeItems[i].DisplayName);
        }

        builder.AppendLine();
        PodLogOutputEntry[] exportEntries;
        lock (_outputEntriesGate)
        {
            exportEntries = _outputEntries.ToArray();
        }

        builder.Append(exportEntries.Length == 0 ? Logs.Text : BuildOutputText(exportEntries));
        return builder.ToString();
    }

    private async Task ReadLogsAsync(
        StreamReader reader,
        PodLogReadOptions option,
        CancellationTokenSource connectionCts,
        PodLogSessionResolution connectionResolution)
    {
        var cancellationToken = connectionCts.Token;
        var streamEnded = false;
        var transientReadFailure = false;
        var appendedOutput = false;
        List<PodLogOutputEntry> pendingOutput = [];
        List<string>? reconnectBuffer = HasExistingOutput(option) ? [] : null;
        var outputGeneration = Volatile.Read(ref _outputGeneration);
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var read = reader.ReadLineAsync(cancellationToken);
                // Publish quiet streams promptly, but combine already-buffered lines.
                if (!read.IsCompletedSuccessfully || pendingOutput.Count >= MaxOutputBatchSize)
                {
                    await FlushOutputEntriesAsync(pendingOutput, connectionCts, outputGeneration);
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
                                await FlushOutputEntriesAsync(pendingOutput, connectionCts, outputGeneration);
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
            Dispatcher.UIThread.Post(
                () =>
                {
                    if (!_pendingReconnect && IsCurrentConnection(connectionCts))
                    {
                        ConnectionError = ex.Message;
                    }
                },
                DispatcherPriority.Background);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to read pod log stream for {PodNamespace}/{PodName} container {ContainerName}.", option.PodNamespace, option.PodName, option.ContainerName);
            Dispatcher.UIThread.Post(
                () =>
                {
                    if (!_pendingReconnect && IsCurrentConnection(connectionCts))
                    {
                        ConnectionError = ex.Message;
                    }
                },
                DispatcherPriority.Background);
        }
        finally
        {
            if (reconnectBuffer is not null
                && TryFlushReconnectBuffer(reconnectBuffer, option, atEnd: true, out var lines))
            {
                for (var i = 0; i < lines.Count; i++)
                {
                    QueueOutputEntry(pendingOutput, option, lines[i]);
                    if (pendingOutput.Count >= MaxOutputBatchSize)
                    {
                        await FlushOutputEntriesAsync(pendingOutput, connectionCts, outputGeneration);
                        pendingOutput.Clear();
                    }
                }

                appendedOutput |= lines.Count > 0;
            }

            await FlushOutputEntriesAsync(pendingOutput, connectionCts, outputGeneration);

            var isLastActiveReader = DecrementActiveReaders(connectionCts);
            if (((streamEnded && appendedOutput) || transientReadFailure)
                && ShouldReconnectAfterStreamEnd(reader, option, connectionResolution.Pod, cancellationToken, isLastActiveReader))
            {
                ScheduleReconnectAfterStreamEnd(connectionCts);
            }
        }
    }

    private bool ShouldReconnectAfterStreamEnd(
        StreamReader reader,
        PodLogReadOptions option,
        V1Pod resolvedPod,
        CancellationToken cancellationToken,
        bool isLastActiveReader)
    {
        return option.Follow
            && !option.Previous
            && !cancellationToken.IsCancellationRequested
            && !reader.BaseStream.CanSeek
            && !IsTerminalPod(resolvedPod)
            && isLastActiveReader;
    }

    private static bool IsTerminalPod(V1Pod pod)
    {
        return pod.Status?.Phase is "Succeeded" or "Failed";
    }

    private void ScheduleReconnectAfterStreamEnd(CancellationTokenSource connectionCts)
    {
        if (Interlocked.Exchange(ref _streamEndedReconnectPending, 1) != 0)
        {
            return;
        }

        var reconnectAttempt = Interlocked.Increment(ref _streamEndedReconnectAttempts);
        if (reconnectAttempt > MaxAutomaticReconnectAttempts)
        {
            Interlocked.Exchange(ref _streamEndedReconnectPending, 0);
            return;
        }

        _ = Task.Run(async () =>
        {
            try
            {
                await _automaticReconnectDelay(reconnectAttempt, connectionCts.Token);
            }
            catch (OperationCanceledException)
            {
                Interlocked.Exchange(ref _streamEndedReconnectPending, 0);
                return;
            }

            Dispatcher.UIThread.Post(
                () =>
                {
                    Interlocked.Exchange(ref _streamEndedReconnectPending, 0);
                    RequestReconnect(preserveOutput: true);
                },
                DispatcherPriority.Background);
        }, CancellationToken.None);
    }

    private static Task DelayAutomaticReconnectAsync(int reconnectAttempt, CancellationToken cancellationToken)
    {
        var delaySeconds = Math.Min(30, 1 << Math.Min(reconnectAttempt - 1, 4));
        return Task.Delay(TimeSpan.FromSeconds(delaySeconds), cancellationToken);
    }

    private void AddOutputEntry(PodLogOutputEntry entry)
    {
        lock (_outputEntriesGate)
        {
            _outputEntries.Add(entry);
            if (_outputEntries.Count > MaxLogEntries)
            {
                _outputEntries.RemoveRange(0, _outputEntries.Count - MaxLogEntries);
            }
        }
    }

    private void AddOutputEntries(IReadOnlyList<PodLogOutputEntry> entries)
    {
        lock (_outputEntriesGate)
        {
            for (var i = 0; i < entries.Count; i++)
            {
                _outputEntries.Add(entries[i]);
            }

            if (_outputEntries.Count > MaxLogEntries)
            {
                _outputEntries.RemoveRange(0, _outputEntries.Count - MaxLogEntries);
            }
        }
    }

    private static void QueueOutputEntry(List<PodLogOutputEntry> pendingOutput, PodLogReadOptions option, string message)
    {
        pendingOutput.Add(new PodLogOutputEntry(option.PodName, option.ContainerName, message));
    }

    private async Task FlushOutputEntriesAsync(
        IReadOnlyList<PodLogOutputEntry> entries,
        CancellationTokenSource connectionCts,
        int outputGeneration)
    {
        if (entries.Count == 0)
        {
            return;
        }

        PodLogOutputEntry[] batch = entries.ToArray();

        // Backpressure bounds outstanding dispatcher work to one batch per reader.
        await Dispatcher.UIThread.InvokeAsync(
            () => AppendOutputEntries(batch, connectionCts, outputGeneration),
            DispatcherPriority.Background);
    }

    private void AppendOutputEntries(
        IReadOnlyList<PodLogOutputEntry> entries,
        CancellationTokenSource connectionCts,
        int outputGeneration)
    {
        if (!IsCurrentConnection(connectionCts) || outputGeneration != Volatile.Read(ref _outputGeneration))
        {
            return;
        }

        AddOutputEntries(entries);

        if (_displayPaused)
        {
            PendingLogCount += entries.Count;
            return;
        }

        var displayMode = GetCurrentDisplayMode();
        StringBuilder builder = new();
        for (var i = 0; i < entries.Count; i++)
        {
            if (i > 0 || Logs.TextLength > 0)
            {
                builder.AppendLine();
            }

            builder.Append(FormatOutputEntry(entries[i], ShowResourceNames, displayMode));
        }

        using (Logs.RunUpdate())
        {
            Logs.Insert(Logs.TextLength, builder.ToString());
            TrimLogDocument();
        }
    }

    private void AppendOutputEntry(PodLogOutputEntry entry, CancellationTokenSource connectionCts, int outputGeneration)
    {
        if (!IsCurrentConnection(connectionCts) || outputGeneration != Volatile.Read(ref _outputGeneration))
        {
            return;
        }

        AddOutputEntry(entry);

        if (_displayPaused)
        {
            PendingLogCount++;
            return;
        }

        var line = FormatOutputEntry(entry, ShowResourceNames, GetCurrentDisplayMode());
        if (Logs.TextLength > 0)
        {
            Logs.Insert(Logs.TextLength, Environment.NewLine);
        }

        Logs.Insert(Logs.TextLength, line);
        TrimLogDocument();
    }

    private void TrimLogDocument()
    {
        if (Logs.LineCount > MaxLogEntries)
        {
            var firstRetainedLine = Logs.GetLineByNumber(Logs.LineCount - MaxLogEntries + 1);
            Logs.Remove(0, firstRetainedLine.Offset);
        }
    }

    private bool IsCurrentConnection(CancellationTokenSource connectionCts)
    {
        return !_disposed
            && ReferenceEquals(_connectionCts, connectionCts)
            && !connectionCts.IsCancellationRequested;
    }

    private void RenderOutputEntries()
    {
        if (_displayPaused)
        {
            return;
        }

        PodLogOutputEntry[] entries;
        lock (_outputEntriesGate)
        {
            if (_outputEntries.Count == 0)
            {
                return;
            }

            entries = _outputEntries.ToArray();
        }

        Logs.Text = BuildOutputText(entries);
    }

    private string BuildOutputText(IReadOnlyList<PodLogOutputEntry> entries)
    {
        StringBuilder builder = new();
        var displayMode = GetCurrentDisplayMode();
        for (var i = 0; i < entries.Count; i++)
        {
            if (i > 0)
            {
                builder.AppendLine();
            }

            builder.Append(FormatOutputEntry(entries[i], ShowResourceNames, displayMode));
        }

        return builder.ToString();
    }

    private static string FormatOutputEntry(PodLogOutputEntry entry, bool showResourceNames, PodLogDisplayMode displayMode)
    {
        if (!showResourceNames)
        {
            return entry.Message;
        }

        var prefix = BuildDisplayPrefix(entry.PodName, entry.ContainerName, displayMode);
        if (!string.IsNullOrWhiteSpace(prefix))
        {
            return $"[{prefix}] {entry.Message}";
        }

        return entry.Message;
    }

    private PodLogDisplayMode GetCurrentDisplayMode()
    {
        var selectedSources = GetSelectedSourceCounts();
        if (selectedSources.PodCount > 1)
        {
            return PodLogDisplayMode.PodAndContainer;
        }

        if (selectedSources.TargetCount > 1)
        {
            return PodLogDisplayMode.Container;
        }

        return PodLogDisplayMode.None;
    }

    private static string BuildDisplayPrefix(string podName, string containerName, PodLogDisplayMode displayMode)
    {
        return displayMode switch
        {
            PodLogDisplayMode.PodAndContainer => $"{podName}/{containerName}",
            PodLogDisplayMode.Container => containerName,
            _ => string.Empty,
        };
    }

    private bool HasExistingOutput(PodLogReadOptions option)
    {
        lock (_outputEntriesGate)
        {
            for (var i = _outputEntries.Count - 1; i >= 0; i--)
            {
                var entry = _outputEntries[i];
                if (string.Equals(entry.PodName, option.PodName, StringComparison.Ordinal)
                    && string.Equals(entry.ContainerName, option.ContainerName, StringComparison.Ordinal))
                {
                    return true;
                }
            }
        }

        return false;
    }

    private bool TryFlushReconnectBuffer(
        List<string> pending,
        PodLogReadOptions option,
        bool atEnd,
        out List<string> lines)
    {
        var history = GetRecentMessages(option);
        var overlap = GetSuffixPrefixOverlap(history, pending);

        if (!atEnd && overlap == pending.Count)
        {
            lines = [];
            return false;
        }

        lines = pending.Skip(overlap).ToList();
        return true;
    }

    private List<string> GetRecentMessages(PodLogReadOptions option)
    {
        List<string> history = [];
        lock (_outputEntriesGate)
        {
            for (var i = _outputEntries.Count - 1; i >= 0 && history.Count < option.TailLines; i--)
            {
                var entry = _outputEntries[i];
                if (string.Equals(entry.PodName, option.PodName, StringComparison.Ordinal)
                    && string.Equals(entry.ContainerName, option.ContainerName, StringComparison.Ordinal))
                {
                    history.Add(entry.Message);
                }
            }
        }

        history.Reverse();
        return history;
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

}
