using System.IO;
using System.Text;
using Avalonia.Threading;
using AvaloniaEdit.Document;
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
        Dispatcher.UIThread.InvokeAsync(() => AppendOutputEntry(entry, connectionCts), DispatcherPriority.Background);
    }

    private void DecrementActiveReaders(CancellationTokenSource connectionCts)
    {
        if (!IsCurrentConnection(connectionCts))
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
        string podName = Object?.Metadata?.Name ?? "pod";
        string containerName = string.IsNullOrWhiteSpace(ContainerName) ? "logs" : ContainerName;
        string? namespaceName = Object?.Metadata?.NamespaceProperty;

        string fileName = namespaceName is { Length: > 0 }
            ? $"{namespaceName}-{podName}-{containerName}.log"
            : $"{podName}-{containerName}.log";

        return fileName.ReplaceInvalidFileNameChars();
    }

    private async Task ReadLogsAsync(StreamReader reader, PodLogReadOptions option, CancellationTokenSource connectionCts)
    {
        CancellationToken cancellationToken = connectionCts.Token;
        bool streamEnded = false;
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                string? log = await reader.ReadLineAsync();
                if (log is null)
                {
                    streamEnded = true;
                    break;
                }

                PodLogOutputEntry entry = new(option.PodName, option.ContainerName, log);
                AddOutputEntry(entry);
                await Dispatcher.UIThread.InvokeAsync(
                    () => AppendOutputEntry(entry, connectionCts),
                    DispatcherPriority.Background);
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
        }
        finally
        {
            if (streamEnded && ShouldReconnectAfterStreamEnd(reader, option, cancellationToken))
            {
                ScheduleReconnectAfterStreamEnd(option, connectionCts);
            }

            DecrementActiveReaders(connectionCts);
        }
    }

    private static bool ShouldReconnectAfterStreamEnd(StreamReader reader, PodLogReadOptions option, CancellationToken cancellationToken)
    {
        return option.Follow
            && !option.Previous
            && !cancellationToken.IsCancellationRequested
            && !reader.BaseStream.CanSeek;
    }

    private void ScheduleReconnectAfterStreamEnd(PodLogReadOptions option, CancellationTokenSource connectionCts)
    {
        if (Interlocked.Exchange(ref _streamEndedReconnectPending, 1) == 1)
        {
            return;
        }

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(1), connectionCts.Token);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            if (!IsCurrentConnection(connectionCts))
            {
                return;
            }

            Dispatcher.UIThread.Post(RequestReconnect, DispatcherPriority.Background);
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

    private void AppendOutputEntry(PodLogOutputEntry entry, CancellationTokenSource connectionCts)
    {
        if (!IsCurrentConnection(connectionCts))
        {
            return;
        }

        string line = FormatOutputEntry(entry, ShowResourceNames, GetCurrentDisplayMode());
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
            DocumentLine firstLine = Logs.GetLineByNumber(1);
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
        for (int i = 0; i < entries.Length; i++)
        {
            if (i > 0)
            {
                builder.AppendLine();
            }

            builder.Append(FormatOutputEntry(entries[i], ShowResourceNames, GetCurrentDisplayMode()));
        }

        Logs.Text = builder.ToString();
    }

    private static string FormatOutputEntry(PodLogOutputEntry entry, bool showResourceNames, PodLogDisplayMode displayMode)
    {
        if (!showResourceNames)
        {
            return entry.Message;
        }

        string prefix = BuildDisplayPrefix(entry.PodName, entry.ContainerName, displayMode);
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
}
