using System.IO;
using System.Text;
using Avalonia.Threading;
using AvaloniaEdit.Document;
using k8s.Models;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Pod.ViewModels;

public sealed partial class PodLogsViewModel
{
    private void AppendStatusLine(string podName, string containerName, string message, CancellationTokenSource connectionCts)
    {
        if (string.IsNullOrWhiteSpace(message) || !IsCurrentConnection(connectionCts))
        {
            return;
        }

        PodLogOutputEntry entry = new(podName, containerName, message);
        AddOutputEntry(entry);
        var outputGeneration = Volatile.Read(ref _outputGeneration);
        Dispatcher.UIThread.InvokeAsync(
            () => AppendOutputEntry(entry, connectionCts, outputGeneration),
            DispatcherPriority.Background);
    }

    private void DecrementActiveReaders(CancellationTokenSource connectionCts)
    {
        var isCurrentConnection = IsCurrentConnection(connectionCts);
        if (_readerCounts.TryGetValue(connectionCts, out var remainingReaders))
        {
            remainingReaders = _readerCounts.AddOrUpdate(connectionCts, 0, static (_, count) => count - 1);
            if (remainingReaders <= 0 && _readerCounts.TryRemove(connectionCts, out _))
            {
                if (isCurrentConnection)
                {
                    IsConnected = false;
                }
                else
                {
                    connectionCts.Dispose();
                }
            }
        }

        if (!isCurrentConnection)
        {
            return;
        }

        if (Interlocked.Decrement(ref _activeReaderCount) > 0)
        {
            return;
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
    }

    private string BuildSuggestedFileName()
    {
        var podName = Object?.Metadata?.Name ?? "pod";
        var containerName = string.IsNullOrWhiteSpace(ContainerName) ? "logs" : ContainerName;
        var namespaceName = Object?.Metadata?.NamespaceProperty;

        var fileName = namespaceName is { Length: > 0 }
            ? $"{namespaceName}-{podName}-{containerName}.log"
            : $"{podName}-{containerName}.log";

        return fileName.ReplaceInvalidFileNameChars();
    }

    private async Task ReadLogsAsync(
        StreamReader reader,
        PodLogReadOptions option,
        CancellationTokenSource connectionCts,
        PodLogSessionResolution connectionResolution)
    {
        var cancellationToken = connectionCts.Token;
        var streamEnded = false;
        var appendedOutput = false;
        List<PodLogOutputEntry> pendingOutput = [];
        List<string>? reconnectBuffer = HasExistingOutput(option) ? [] : null;
        var outputGeneration = Volatile.Read(ref _outputGeneration);
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var log = await reader.ReadLineAsync();
                if (log is null)
                {
                    streamEnded = true;
                    break;
                }

                if (reconnectBuffer is null)
                {
                    QueueOutputEntry(pendingOutput, option, log);
                    FlushOutputEntries(pendingOutput, connectionCts, outputGeneration);
                    pendingOutput.Clear();
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
                            FlushOutputEntries(pendingOutput, connectionCts, outputGeneration);
                            pendingOutput.Clear();
                        }

                        appendedOutput = lines.Count > 0;
                    }
                }
            }
        }
        catch (IOException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (ObjectDisposedException) when (cancellationToken.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to read pod log stream for {PodNamespace}/{PodName} container {ContainerName}.", option.PodNamespace, option.PodName, option.ContainerName);
            Dispatcher.UIThread.Post(
                () =>
                {
                    if (IsCurrentConnection(connectionCts))
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
                    FlushOutputEntries(pendingOutput, connectionCts, outputGeneration);
                    pendingOutput.Clear();
                }

                appendedOutput = lines.Count > 0;
            }

            FlushOutputEntries(pendingOutput, connectionCts, outputGeneration);

            if (streamEnded && appendedOutput && ShouldReconnectAfterStreamEnd(reader, option, connectionResolution.Pod, cancellationToken))
            {
                ScheduleReconnectAfterStreamEnd(connectionCts);
            }

            DecrementActiveReaders(connectionCts);
        }
    }

    private bool ShouldReconnectAfterStreamEnd(StreamReader reader, PodLogReadOptions option, V1Pod resolvedPod, CancellationToken cancellationToken)
    {
        return option.Follow
            && !option.Previous
            && !cancellationToken.IsCancellationRequested
            && !reader.BaseStream.CanSeek
            && !IsTerminalPod(resolvedPod)
            && Volatile.Read(ref _activeReaderCount) == 1;
    }

    private bool IsTerminalPod(V1Pod pod)
    {
        V1Pod? currentPod = Cluster.GetResource<V1Pod>(pod.Namespace(), pod.Name()) ?? pod;
        return string.Equals(currentPod.Status?.Phase, "Succeeded", StringComparison.Ordinal)
            || string.Equals(currentPod.Status?.Phase, "Failed", StringComparison.Ordinal);
    }

    private void ScheduleReconnectAfterStreamEnd(CancellationTokenSource connectionCts)
    {
        if (Interlocked.Increment(ref _streamEndedReconnectAttempts) > MaxAutomaticReconnectAttempts)
        {
            return;
        }

        if (Interlocked.Exchange(ref _streamEndedReconnectPending, 1) == 1)
        {
            return;
        }

        _ = Task.Run(async () =>
        {
            try
            {
                var delaySeconds = Math.Min(30, 1 << Math.Min(_streamEndedReconnectAttempts - 1, 4));
                await Task.Delay(TimeSpan.FromSeconds(delaySeconds), connectionCts.Token);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            if (!IsCurrentConnection(connectionCts))
            {
                return;
            }

            Dispatcher.UIThread.Post(() => RequestReconnect(preserveOutput: true), DispatcherPriority.Background);
        }, CancellationToken.None);
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

    private static void QueueOutputEntry(List<PodLogOutputEntry> pendingOutput, PodLogReadOptions option, string message)
    {
        pendingOutput.Add(new PodLogOutputEntry(option.PodName, option.ContainerName, message));
    }

    private void FlushOutputEntries(
        IReadOnlyList<PodLogOutputEntry> entries,
        CancellationTokenSource connectionCts,
        int outputGeneration)
    {
        if (entries.Count == 0)
        {
            return;
        }

        PodLogOutputEntry[] batch = entries.ToArray();
        for (var i = 0; i < batch.Length; i++)
        {
            AddOutputEntry(batch[i]);
        }

        Dispatcher.UIThread.InvokeAsync(
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

        Logs.Insert(Logs.TextLength, builder.ToString());
        TrimLogDocument();
    }

    private void AppendOutputEntry(PodLogOutputEntry entry, CancellationTokenSource connectionCts, int outputGeneration)
    {
        if (!IsCurrentConnection(connectionCts) || outputGeneration != Volatile.Read(ref _outputGeneration))
        {
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
        while (Logs.LineCount > MaxLogEntries)
        {
            var firstLine = Logs.GetLineByNumber(1);
            Logs.Remove(0, firstLine.TotalLength);
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
        PodLogOutputEntry[] entries;
        lock (_outputEntriesGate)
        {
            if (_outputEntries.Count == 0)
            {
                Logs.Text = string.Empty;
                return;
            }

            entries = _outputEntries.ToArray();
        }

        if (entries.Length == 0)
        {
            Logs.Text = string.Empty;
            return;
        }

        StringBuilder builder = new();
        var displayMode = GetCurrentDisplayMode();
        for (var i = 0; i < entries.Length; i++)
        {
            if (i > 0)
            {
                builder.AppendLine();
            }

            builder.Append(FormatOutputEntry(entries[i], ShowResourceNames, displayMode));
        }

        Logs.Text = builder.ToString();
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
        if (SelectedPodItems.Count > 1 || ContainsAllSelection(SelectedPodItems) && AvailablePods.Count > 1)
        {
            return PodLogDisplayMode.PodAndContainer;
        }

        if (SelectedContainerItems.Count > 1 || ContainsAllSelection(SelectedContainerItems) && AvailableContainers.Count > 1)
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
