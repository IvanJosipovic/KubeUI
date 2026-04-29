using System.IO;
using System.Text;
using Avalonia.Threading;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Pod.ViewModels;

public sealed partial class PodLogsViewModel
{
    private void AppendStatusLine(string podName, string containerName, string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            return;
        }

        PodLogOutputEntry entry = new(podName, containerName, message);
        lock (_outputEntriesGate)
        {
            _outputEntries.Add(entry);
        }
        Dispatcher.UIThread.InvokeAsync(() => AppendOutputEntry(entry), DispatcherPriority.Background);
    }

    private void DecrementActiveReaders()
    {
        if (Interlocked.Decrement(ref _activeReaderCount) > 0)
        {
            return;
        }

        Dispatcher.UIThread.InvokeAsync(() => IsConnected = false, DispatcherPriority.Background);
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

    private async Task ReadLogsAsync(StreamReader reader, PodLogReadOptions option, CancellationToken cancellationToken)
    {
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
                lock (_outputEntriesGate)
                {
                    _outputEntries.Add(entry);
                }
                await Dispatcher.UIThread.InvokeAsync(() => AppendOutputEntry(entry), DispatcherPriority.Background);
            }
        }
        catch (IOException ex) when (cancellationToken.IsCancellationRequested || ex.Message.Equals("The request was aborted.", StringComparison.Ordinal))
        {
        }
        catch (ObjectDisposedException)
        {
        }
        finally
        {
            if (streamEnded && ShouldReconnectAfterStreamEnd(reader, option, cancellationToken))
            {
                ScheduleReconnectAfterStreamEnd(option, cancellationToken);
            }

            DecrementActiveReaders();
        }
    }

    private static bool ShouldReconnectAfterStreamEnd(StreamReader reader, PodLogReadOptions option, CancellationToken cancellationToken)
    {
        return option.Follow
            && !option.Previous
            && !cancellationToken.IsCancellationRequested
            && !reader.BaseStream.CanSeek;
    }

    private void ScheduleReconnectAfterStreamEnd(PodLogReadOptions option, CancellationToken cancellationToken)
    {
        if (Interlocked.Exchange(ref _streamEndedReconnectPending, 1) == 1)
        {
            return;
        }

        Dispatcher.UIThread.Post(() =>
        {
            StatusMessage = $"Log stream ended for {option.PodName}/{option.ContainerName}. Reconnecting...";
        }, DispatcherPriority.Background);

        _ = Task.Run(async () =>
        {
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(1), cancellationToken);
            }
            catch (OperationCanceledException)
            {
                return;
            }

            if (cancellationToken.IsCancellationRequested)
            {
                return;
            }

            Dispatcher.UIThread.Post(RequestReconnect, DispatcherPriority.Background);
        }, CancellationToken.None);
    }

    private void AppendOutputEntry(PodLogOutputEntry entry)
    {
        string line = FormatOutputEntry(entry, ShowResourceNames, GetCurrentDisplayMode());
        if (Logs.TextLength > 0)
        {
            Logs.Insert(Logs.TextLength, Environment.NewLine);
        }

        Logs.Insert(Logs.TextLength, line);
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
