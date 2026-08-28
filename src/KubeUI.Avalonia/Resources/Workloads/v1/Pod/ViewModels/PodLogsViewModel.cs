using System.Collections.ObjectModel;
using System.Collections.Concurrent;
using System.IO;
using System.Reactive.Linq;
using AvaloniaEdit.Document;
using Dock.Model.Core;
using k8s;
using k8s.Models;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Avalonia.Resources.Workloads.v1.Pod.Services;
using KubeUI.Avalonia.Services.Settings;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Pod.ViewModels;

public sealed partial class PodLogsViewModel : ViewModelBase, IDisposable
{
    private const int DefaultTailLines = 100;
    private const int MaxLogEntries = 10_000;
    private const int MaxAutomaticReconnectAttempts = 5;

    private readonly ILogger<PodLogsViewModel> _logger;
    private readonly IPodLogExportService _exportService;
    private readonly IPodLogSessionResolver _sessionResolver;
    private readonly IPodLogStreamClient _streamClient;
    private readonly SemaphoreSlim _connectionGate = new(1, 1);
    private CancellationTokenSource? _connectionCts;
    private bool _disposed;
    private readonly object _outputEntriesGate = new();
    private readonly List<PodLogOutputEntry> _outputEntries = [];
    private readonly List<Stream> _streams = [];
    private readonly List<StreamReader> _streamReaders = [];
    private readonly ConcurrentDictionary<CancellationTokenSource, int> _readerCounts = new();
    private bool _hasLoadedSession;
    private bool _isApplyingSession;
    private bool _isNormalizingPodSelection;
    private bool _isNormalizingContainerSelection;
    private bool _pendingNormalizePodSelection;
    private bool _pendingNormalizeContainerSelection;
    private PodLogSelectionNormalization _pendingPodSelectionNormalization;
    private PodLogSelectionNormalization _pendingContainerSelectionNormalization;
    private bool _pendingReconnect;
    private bool _preserveOutputOnNextConnect;
    private int _streamEndedReconnectPending;
    private int _streamEndedReconnectAttempts;
    private int _activeReaderCount;
    private int _outputGeneration;
    private IDisposable? _resourceChangesSubscription;
    private IClusterRuntime? _subscribedCluster;

    public PodLogsViewModel(
        ILogger<PodLogsViewModel> logger,
        ISettingsService settingsService,
        IPodLogExportService exportService,
        IPodLogSessionResolver sessionResolver,
        IPodLogStreamClient streamClient)
    {
        _logger = logger;
        SettingsService = settingsService;
        _exportService = exportService;
        _sessionResolver = sessionResolver;
        _streamClient = streamClient;
        Title = Assets.Resources.PodLogsView_Title;
        SelectedPodItems.CollectionChanged += SelectedPodItemsOnCollectionChanged;
        SelectedContainerItems.CollectionChanged += SelectedContainerItemsOnCollectionChanged;
    }

    public ISettingsService SettingsService { get; }

    [ObservableProperty]
    public partial IClusterRuntime Cluster { get; set; }

    [ObservableProperty]
    public partial IKubernetesObject<V1ObjectMeta>? Object { get; set; }

    [ObservableProperty]
    public partial string ContainerName { get; set; }

    [ObservableProperty]
    public partial TextDocument Logs { get; set; } = CreateLogDocument();

    [ObservableProperty]
    public partial IReadOnlyList<V1Pod> AvailablePods { get; set; } = [];

    [ObservableProperty]
    public partial IReadOnlyList<PodLogContainerOption> AvailableContainers { get; set; } = [];

    [ObservableProperty]
    public partial IReadOnlyList<PodLogPodSelectionItem> PodSelectionItems { get; set; } = [];

    [ObservableProperty]
    public partial ObservableCollection<PodLogPodSelectionItem> SelectedPodItems { get; set; } = [];

    [ObservableProperty]
    public partial IReadOnlyList<PodLogContainerSelectionItem> ContainerSelectionItems { get; set; } = [];

    [ObservableProperty]
    public partial ObservableCollection<PodLogContainerSelectionItem> SelectedContainerItems { get; set; } = [];

    [ObservableProperty]
    public partial bool Previous { get; set; }

    [ObservableProperty]
    public partial bool Timestamps { get; set; }

    [ObservableProperty]
    public partial bool AutoScrollToBottom { get; set; } = true;

    [ObservableProperty]
    public partial bool WordWrap { get; set; }

    private bool _showResourceNames;

    public bool ShowResourceNames
    {
        get => _showResourceNames;
        set
        {
            if (_showResourceNames == value)
            {
                return;
            }

            _showResourceNames = value;
            OnPropertyChanged(nameof(ShowResourceNames));
            RenderOutputEntries();
        }
    }

    public bool CanShowResourceNames
    {
        get
        {
            return GetCurrentDisplayMode() != PodLogDisplayMode.None;
        }
    }

    [ObservableProperty]
    public partial bool JumpToPresentRequested { get; set; }

    [ObservableProperty]
    public partial Vector ScrollOffset { get; set; }

    [ObservableProperty]
    public partial PodLogSessionState? SessionState { get; set; }

    [ObservableProperty]
    public partial PodLogSessionResolution? SessionResolution { get; set; }

    [ObservableProperty]
    public partial bool PreviousLogsAvailable { get; set; }

    [ObservableProperty]
    public partial bool IsConnecting { get; set; }

    [ObservableProperty]
    public partial bool IsConnected { get; set; }

    [ObservableProperty]
    public partial string? ConnectionError { get; set; }

    public async Task Connect()
    {
        if (_disposed)
        {
            return;
        }

        await _connectionGate.WaitAsync();
        try
        {
            if (_disposed)
            {
                return;
            }

            var preserveOutput = _preserveOutputOnNextConnect;
            _preserveOutputOnNextConnect = false;
            if (!preserveOutput)
            {
                _streamEndedReconnectAttempts = 0;
            }
            ResetConnection();

            IsConnecting = true;
            ConnectionError = null;

            var state = _sessionResolver.CreateState(
                Object ?? throw new InvalidOperationException("The pod log view model is not initialized."),
                ContainerName,
                Previous,
                Timestamps,
                DefaultTailLines);

            var resolution = _sessionResolver.TryResolve(Cluster, state);
            if (resolution is null)
            {
                SessionState = state;
                SessionResolution = null;
                if (Object is V1Pod unresolvedPod)
                {
                    AvailablePods = [unresolvedPod];
                    AvailableContainers = BuildContainerOptions(unresolvedPod);
                    PodSelectionItems = BuildPodSelectionItems([unresolvedPod]);
                    ReplaceSelectedPodItems([PodSelectionItems[0]]);
                }
                else
                {
                    AvailablePods = [];
                    AvailableContainers = [];
                    PodSelectionItems = [];
                    ReplaceSelectedPodItems([]);
                }

                ContainerSelectionItems = BuildContainerSelectionItems(AvailableContainers);
                ReplaceSelectedContainerItems(ContainerSelectionItems.Count > 0 ? [ContainerSelectionItems[0]] : []);
                PreviousLogsAvailable = false;
                UpdateResourceNameToggleState();
                return;
            }

            PodLogSessionResolution? previousResolution = SessionResolution;
            SessionState = state;
            SessionResolution = resolution;
            PreviousLogsAvailable = resolution.PreviousLogsAvailable;

            _isApplyingSession = true;
            try
            {
                AvailablePods = resolution.RelatedPods;
                AvailableContainers = BuildContainerOptions(resolution.RelatedPods);
                PodSelectionItems = BuildPodSelectionItems(resolution.RelatedPods);
                ObservableCollection<PodLogPodSelectionItem> selectedPodItems = BuildSelectedPodItems(resolution.Pod, PodSelectionItems);
                if (previousResolution is not null
                    && Object is not V1Pod
                    && !ContainsAllSelection(SelectedPodItems)
                    && !string.Equals(previousResolution.Pod.Metadata?.Uid, resolution.Pod.Metadata?.Uid, StringComparison.Ordinal))
                {
                    PodLogPodSelectionItem? currentPodItem = FindPodSelectionItem(PodSelectionItems, resolution.Pod.Name());
                    if (currentPodItem is not null)
                    {
                        selectedPodItems = new ObservableCollection<PodLogPodSelectionItem>([currentPodItem]);
                    }
                }

                ReplaceSelectedPodItems(selectedPodItems);
                ContainerSelectionItems = BuildContainerSelectionItems(AvailableContainers);
                ReplaceSelectedContainerItems(BuildSelectedContainerItems(resolution.ContainerName, ContainerSelectionItems));
                if (Object is V1Pod)
                {
                    Object = resolution.Pod;
                }
                ContainerName = resolution.ContainerName;
            }
            finally
            {
                _isApplyingSession = false;
            }

            UpdateResourceNameToggleState();

            var options = BuildReadTargets(state, resolution);
            if (options.Count == 0)
            {
                return;
            }

            if (!preserveOutput)
            {
                ClearOutput();
            }

            CancellationTokenSource connectionCts = new();
            _connectionCts = connectionCts;

            IsConnected = true;
            _hasLoadedSession = true;
            _activeReaderCount = options.Count;
            _readerCounts[connectionCts] = options.Count;
            EnsureResourceChangeSubscription();

            for (var i = 0; i < options.Count; i++)
            {
                var option = options[i];
                try
                {
                    var stream = await _streamClient.OpenAsync(Cluster, option, connectionCts.Token);
                    if (_disposed || connectionCts.IsCancellationRequested)
                    {
                        stream.Dispose();
                        break;
                    }

                    StreamReader reader = new(stream);
                    _streams.Add(stream);
                    _streamReaders.Add(reader);
                    _ = Task.Run(() => ReadLogsAsync(reader, option, connectionCts, resolution));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unable to open pod log stream for {PodNamespace}/{PodName} container {ContainerName}.", option.PodNamespace, option.PodName, option.ContainerName);
                    var isLastActiveReader = DecrementActiveReaders(connectionCts);
                    AppendStatusLine(option.PodName, option.ContainerName, ex.Message, connectionCts);
                    ConnectionError = ex.Message;
                    if (ex is IOException or HttpRequestException
                        && option.Follow
                        && !option.Previous
                        && !connectionCts.IsCancellationRequested
                        && !IsTerminalPod(resolution.Pod)
                        && isLastActiveReader)
                    {
                        ScheduleReconnectAfterStreamEnd(connectionCts);
                    }
                }
            }

            if (_activeReaderCount == 0)
            {
                IsConnected = false;
                return;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to view pod logs.");
            ConnectionError = ex.Message;
        }
        finally
        {
            IsConnecting = false;
            _connectionGate.Release();
            if (_pendingReconnect)
            {
                _pendingReconnect = false;
                RequestReconnect();
            }
        }
    }

    [RelayCommand]
    public void Clear()
    {
        ClearOutput();
    }

    [RelayCommand]
    public Task Refresh()
    {
        return Connect();
    }

    [RelayCommand]
    public void JumpToPresent()
    {
        AutoScrollToBottom = true;
        JumpToPresentRequested = true;
    }

    [RelayCommand]
    public Task DownloadLogs()
    {
        var suggestedFileName = BuildSuggestedFileName();
        return DownloadLogsAsync(suggestedFileName);
    }

    private async Task DownloadLogsAsync(string suggestedFileName)
    {
        try
        {
            await _exportService.ExportAsync(suggestedFileName, Logs.Text);
        }
        catch (IOException ex)
        {
            _logger.LogError(ex, "Unable to export pod logs.");
            ConnectionError = ex.Message;
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger.LogError(ex, "Unable to export pod logs.");
            ConnectionError = ex.Message;
        }
    }

    [RelayCommand]
    public Task JumpToControlledByLogs()
    {
        if (SessionResolution?.RelatedPods.Count is not > 1 || Object is not V1Pod)
        {
            return Task.CompletedTask;
        }

        _isApplyingSession = true;
        try
        {
            SelectAllPods();
        }
        finally
        {
            _isApplyingSession = false;
        }

        return Connect();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        SelectedPodItems.CollectionChanged -= SelectedPodItemsOnCollectionChanged;
        SelectedContainerItems.CollectionChanged -= SelectedContainerItemsOnCollectionChanged;
        _resourceChangesSubscription?.Dispose();
        _resourceChangesSubscription = null;
        _subscribedCluster = null;

        ResetConnection();

        TextDocument logs = Logs;
        logs.Text = string.Empty;
        Logs = CreateLogDocument();

        // Connect may still be awaiting OpenAsync and will release the gate in its finally block.
        // Keep the gate and CTS alive until those asynchronous operations have drained.
    }

    partial void OnLogsChanged(TextDocument value)
    {
        value.UndoStack.SizeLimit = 0;
    }

    partial void OnPreviousChanged(bool value)
    {
        RequestReconnect();
    }

    partial void OnSelectedPodItemsChanged(ObservableCollection<PodLogPodSelectionItem> value)
    {
        value.CollectionChanged += SelectedPodItemsOnCollectionChanged;
        if (_isApplyingSession)
        {
            return;
        }

        if (SelectedPodItems.Count == 0)
        {
            QueueNormalizeSelectedPodItems(PodLogSelectionNormalization.SelectAll);
        }

        UpdateResourceNameToggleState();
        RequestReconnect();
    }

    partial void OnSelectedContainerItemsChanged(ObservableCollection<PodLogContainerSelectionItem> value)
    {
        value.CollectionChanged += SelectedContainerItemsOnCollectionChanged;
        if (_isApplyingSession)
        {
            return;
        }

        if (SelectedContainerItems.Count == 0)
        {
            QueueNormalizeSelectedContainerItems(PodLogSelectionNormalization.SelectAll);
        }

        UpdateResourceNameToggleState();
        RequestReconnect();
    }

    partial void OnObjectChanged(IKubernetesObject<V1ObjectMeta>? value)
    {
        if (_isApplyingSession)
        {
            return;
        }

        UpdateResourceNameToggleState();
        RequestReconnect();
    }

    partial void OnContainerNameChanged(string value)
    {
        if (_isApplyingSession)
        {
            return;
        }

        UpdateResourceNameToggleState();
        RequestReconnect();
    }

    partial void OnTimestampsChanged(bool value)
    {
        RequestReconnect();
    }

    private void RequestReconnect(bool preserveOutput = false)
    {
        if (preserveOutput)
        {
            _preserveOutputOnNextConnect = true;
        }

        if (!_hasLoadedSession || IsConnecting)
        {
            if (_hasLoadedSession)
            {
                _pendingReconnect = true;
            }

            return;
        }

        _ = Connect();
    }

    private void EnsureResourceChangeSubscription()
    {
        if (ReferenceEquals(_subscribedCluster, Cluster))
        {
            return;
        }

        _resourceChangesSubscription?.Dispose();
        _resourceChangesSubscription = Cluster.ConnectResources()
            .Throttle(TimeSpan.FromMilliseconds(50))
            .Subscribe(_ => QueueResourceReevaluation());
        _subscribedCluster = Cluster;
    }

    private void QueueResourceReevaluation()
    {
        if (_disposed || SessionState is null || SessionResolution is null)
        {
            return;
        }

        Dispatcher.UIThread.Post(
            () =>
            {
                if (_disposed || SessionState is null || SessionResolution is null)
                {
                    return;
                }

                PodLogSessionResolution? resolution = _sessionResolver.TryResolve(Cluster, SessionState);
                if (resolution is not null && HasLogTopologyChanged(SessionResolution, resolution))
                {
                    RequestReconnect(preserveOutput: true);
                }
            },
            DispatcherPriority.ApplicationIdle);
    }

    private static bool HasLogTopologyChanged(PodLogSessionResolution current, PodLogSessionResolution next)
    {
        if (!string.Equals(current.Pod.Metadata?.Uid, next.Pod.Metadata?.Uid, StringComparison.Ordinal)
            || current.RelatedPods.Count != next.RelatedPods.Count)
        {
            return true;
        }

        for (var i = 0; i < current.RelatedPods.Count; i++)
        {
            V1Pod currentPod = current.RelatedPods[i];
            V1Pod nextPod = next.RelatedPods[i];
            if (!string.Equals(currentPod.Metadata?.Uid, nextPod.Metadata?.Uid, StringComparison.Ordinal)
                || !GetContainerNames(currentPod).SequenceEqual(GetContainerNames(nextPod), StringComparer.Ordinal))
            {
                return true;
            }
        }

        return !string.Equals(current.ContainerName, next.ContainerName, StringComparison.Ordinal);
    }

    private static IEnumerable<string?> GetContainerNames(V1Pod pod)
    {
        return (pod.Spec?.Containers ?? []).Select(container => container.Name)
            .Concat((pod.Spec?.InitContainers ?? []).Select(container => container.Name))
            .Concat((pod.Spec?.EphemeralContainers ?? []).Select(container => container.Name));
    }

    private void ResetConnection()
    {
        CancellationTokenSource? previousConnectionCts = _connectionCts;
        try
        {
            previousConnectionCts?.Cancel();
        }
        catch (ObjectDisposedException)
        {
        }

        for (var i = 0; i < _streamReaders.Count; i++)
        {
            _streamReaders[i].Dispose();
        }

        _streamReaders.Clear();

        for (var i = 0; i < _streams.Count; i++)
        {
            _streams[i].Dispose();
        }

        _streams.Clear();

        _connectionCts = null;
        if (previousConnectionCts is not null && !_readerCounts.ContainsKey(previousConnectionCts))
        {
            previousConnectionCts.Dispose();
        }

        _activeReaderCount = 0;
        _streamEndedReconnectPending = 0;
        IsConnected = false;
    }

    private void ClearOutput()
    {
        Interlocked.Increment(ref _outputGeneration);
        Logs.Text = string.Empty;
        lock (_outputEntriesGate)
        {
            _outputEntries.Clear();
        }
    }

    private static TextDocument CreateLogDocument()
    {
        TextDocument document = new();
        document.UndoStack.SizeLimit = 0;
        return document;
    }

}
