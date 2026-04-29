using System.Collections.ObjectModel;
using System.IO;
using AvaloniaEdit.Document;
using Dock.Model.Core;
using k8s.Models;
using KubeUI.Avalonia.Features.Clusters.Workspace.ViewModels;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Avalonia.Resources.Workloads.v1.Pod.Services;
using KubeUI.Avalonia.Services.Settings;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Pod.ViewModels;

public sealed partial class PodLogsViewModel : ViewModelBase, IDisposable
{
    private const int DefaultTailLines = 100;

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
    private bool _hasLoadedSession;
    private bool _isApplyingSession;
    private bool _isNormalizingPodSelection;
    private bool _isNormalizingContainerSelection;
    private bool _pendingNormalizePodSelection;
    private bool _pendingNormalizeContainerSelection;
    private PodLogSelectionNormalization _pendingPodSelectionNormalization;
    private PodLogSelectionNormalization _pendingContainerSelectionNormalization;
    private bool _pendingReconnect;
    private int _streamEndedReconnectPending;
    private int _activeReaderCount;

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
        Title = Assets.Resources.PodLogsViewModel_Title;
        SelectedPodItems.CollectionChanged += SelectedPodItemsOnCollectionChanged;
        SelectedContainerItems.CollectionChanged += SelectedContainerItemsOnCollectionChanged;
    }

    public ISettingsService SettingsService { get; }

    [ObservableProperty]
    public partial ClusterWorkspaceViewModel Cluster { get; set; }

    [ObservableProperty]
    public partial V1Pod Object { get; set; }

    [ObservableProperty]
    public partial string ContainerName { get; set; }

    [ObservableProperty]
    public partial TextDocument Logs { get; set; } = new();

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
    public partial string? StatusMessage { get; set; }

    public async Task Connect()
    {
        await _connectionGate.WaitAsync();
        try
        {
            ResetConnection();

            IsConnecting = true;
            StatusMessage = null;

            PodLogSessionState state = _sessionResolver.CreateState(
                Object ?? throw new InvalidOperationException("The pod log view model is not initialized."),
                ContainerName,
                Previous,
                Timestamps,
                DefaultTailLines);

            PodLogSessionResolution? resolution = _sessionResolver.TryResolve(Cluster, state);
            if (resolution is null)
            {
                SessionState = state;
                SessionResolution = null;
                AvailablePods = [Object];
                AvailableContainers = BuildContainerOptions(Object);
                PodSelectionItems = BuildPodSelectionItems([Object]);
                ReplaceSelectedPodItems([PodSelectionItems[0]]);
                ContainerSelectionItems = BuildContainerSelectionItems(AvailableContainers);
                ReplaceSelectedContainerItems([ContainerSelectionItems[0]]);
                PreviousLogsAvailable = false;
                UpdateResourceNameToggleState();
                StatusMessage = "Unable to resolve a pod log session.";
                return;
            }

            SessionState = state;
            SessionResolution = resolution;
            PreviousLogsAvailable = resolution.PreviousLogsAvailable;

            _isApplyingSession = true;
            try
            {
                AvailablePods = resolution.RelatedPods;
                AvailableContainers = BuildContainerOptions(resolution.Pod);
                PodSelectionItems = BuildPodSelectionItems(resolution.RelatedPods);
                ReplaceSelectedPodItems(BuildSelectedPodItems(resolution.Pod, PodSelectionItems));
                ContainerSelectionItems = BuildContainerSelectionItems(AvailableContainers);
                ReplaceSelectedContainerItems(BuildSelectedContainerItems(resolution.ContainerName, ContainerSelectionItems));
                Object = resolution.Pod;
                ContainerName = resolution.ContainerName;
            }
            finally
            {
                _isApplyingSession = false;
            }

            UpdateResourceNameToggleState();

            List<PodLogReadOptions> options = BuildReadTargets(state, resolution);
            if (options.Count == 0)
            {
                StatusMessage = "Unable to resolve a pod log target.";
                return;
            }

            Logs.Text = string.Empty;
            lock (_outputEntriesGate)
            {
                _outputEntries.Clear();
            }

            CancellationTokenSource connectionCts = new();
            _connectionCts = connectionCts;

            IsConnected = true;
            _hasLoadedSession = true;
            _activeReaderCount = options.Count;

            for (int i = 0; i < options.Count; i++)
            {
                PodLogReadOptions option = options[i];
                try
                {
                    Stream stream = await _streamClient.OpenAsync(Cluster, option, connectionCts.Token);
                    StreamReader reader = new(stream);
                    _streams.Add(stream);
                    _streamReaders.Add(reader);
                    _ = Task.Run(() => ReadLogsAsync(reader, option, connectionCts.Token));
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Unable to open pod log stream for {PodNamespace}/{PodName} container {ContainerName}.", option.PodNamespace, option.PodName, option.ContainerName);
                    DecrementActiveReaders();
                    AppendStatusLine(option.PodName, option.ContainerName, ex.Message);
                }
            }

            if (_activeReaderCount == 0)
            {
                StatusMessage = "Unable to open any pod log streams.";
                IsConnected = false;
                return;
            }
        }
        catch (Exception ex)
        {
            StatusMessage = ex.Message;
            _logger.LogError(ex, "Unable to view pod logs.");
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
        Logs.Text = string.Empty;
        lock (_outputEntriesGate)
        {
            _outputEntries.Clear();
        }
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
        string suggestedFileName = BuildSuggestedFileName();
        return _exportService.ExportAsync(suggestedFileName, Logs.Text);
    }

    [RelayCommand]
    public Task JumpToControlledByLogs()
    {
        V1Pod pod = SessionResolution?.Pod ?? Object ?? throw new InvalidOperationException("The pod log view model is not initialized.");

        V1OwnerReference? controller = PodLogFileNameExtensions.GetControllerReference(pod);
        if (controller is null)
        {
            StatusMessage = "This pod does not have a controlling workload.";
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

        CancellationTokenSource? connectionCts = _connectionCts;
        _connectionCts = null;
        connectionCts?.Cancel();
        for (int i = 0; i < _streamReaders.Count; i++)
        {
            _streamReaders[i].Dispose();
        }

        for (int i = 0; i < _streams.Count; i++)
        {
            _streams[i].Dispose();
        }

        connectionCts?.Dispose();
        _connectionGate.Dispose();
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

    partial void OnObjectChanged(V1Pod value)
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

    private void RequestReconnect()
    {
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

    private void ResetConnection()
    {
        _connectionCts?.Cancel();

        for (int i = 0; i < _streamReaders.Count; i++)
        {
            _streamReaders[i].Dispose();
        }

        _streamReaders.Clear();

        for (int i = 0; i < _streams.Count; i++)
        {
            _streams[i].Dispose();
        }

        _streams.Clear();

        _connectionCts?.Dispose();
        _connectionCts = null;

        _activeReaderCount = 0;
        _streamEndedReconnectPending = 0;
        IsConnected = false;
    }

}
