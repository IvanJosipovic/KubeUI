using System.Text;
using k8s.Models;
using KubeUI.Kubernetes;

namespace KubeUI.Features.Resources.Workloads.v1.Pod.ViewModels;

public sealed partial class PodLogsViewModel
{
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

    private void AppendOutputEntries(
        IReadOnlyList<PodLogOutputEntry> entries,
        PodLogStreamSession connectionSession,
        int outputGeneration)
    {
        if (!IsCurrentConnection(connectionSession) || outputGeneration != Volatile.Read(ref _outputGeneration))
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

    private void AppendOutputEntry(PodLogOutputEntry entry, PodLogStreamSession connectionSession, int outputGeneration)
    {
        if (!IsCurrentConnection(connectionSession) || outputGeneration != Volatile.Read(ref _outputGeneration))
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

    private bool IsCurrentConnection(PodLogStreamSession connectionSession)
    {
        return !_disposed
            && ReferenceEquals(_connectionSession, connectionSession)
            && !connectionSession.CancellationToken.IsCancellationRequested;
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

    bool IPodLogStreamSessionSink.IsCurrent(PodLogStreamSession session) => IsCurrentConnection(session);

    bool IPodLogStreamSessionSink.IsReconnectPending => _pendingReconnect;

    int IPodLogStreamSessionSink.OutputGeneration => Volatile.Read(ref _outputGeneration);

    bool IPodLogStreamSessionSink.HasExistingOutput(PodLogReadOptions option) => HasExistingOutput(option);

    List<string> IPodLogStreamSessionSink.GetRecentMessages(PodLogReadOptions option) => GetRecentMessages(option);

    async Task IPodLogStreamSessionSink.AppendOutputEntriesAsync(
        PodLogStreamSession session,
        IReadOnlyList<PodLogOutputEntry> entries,
        int outputGeneration)
    {
        await Dispatcher.UIThread.InvokeAsync(
            () => AppendOutputEntries(entries, session, outputGeneration),
            DispatcherPriority.Background);
    }

    void IPodLogStreamSessionSink.AppendStatusLine(
        PodLogStreamSession session,
        PodLogOutputEntry entry,
        int outputGeneration)
    {
        if (string.IsNullOrWhiteSpace(entry.Message) || !IsCurrentConnection(session))
        {
            return;
        }

        Dispatcher.UIThread.InvokeAsync(
            () => AppendOutputEntry(entry, session, outputGeneration),
            DispatcherPriority.Background);
    }

    void IPodLogStreamSessionSink.OpenFailed(
        PodLogStreamSession session,
        PodLogReadOptions option,
        Exception exception)
    {
        ((IPodLogStreamSessionSink)this).AppendStatusLine(
            session,
            new PodLogOutputEntry(option.PodName, option.ContainerName, exception.Message),
            Volatile.Read(ref _outputGeneration));
        ConnectionError = exception.Message;
    }

    void IPodLogStreamSessionSink.ReadFailed(PodLogStreamSession session, Exception exception)
    {
        if (!_pendingReconnect && IsCurrentConnection(session))
        {
            ConnectionError = exception.Message;
        }
    }

    void IPodLogStreamSessionSink.Disconnected(PodLogStreamSession session)
    {
        IsConnected = false;
    }

    void IPodLogStreamSessionSink.RequestReconnect(PodLogStreamSession session)
    {
        RequestReconnect(preserveOutput: true);
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

}
