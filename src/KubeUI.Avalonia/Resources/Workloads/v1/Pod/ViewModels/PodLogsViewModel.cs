using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.IO;
using System.Text;
using Avalonia.Threading;
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
    private readonly object _outputEntriesGate = new();
    private readonly List<PodLogOutputEntry> _outputEntries = [];
    private readonly List<Stream> _streams = [];
    private readonly List<StreamReader> _streamReaders = [];
    private bool _hasLoadedSession;
    private bool _isApplyingSession;
    private bool _isNormalizingPodSelection;
    private bool _isNormalizingContainerSelection;
    private bool _pendingReconnect;
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
            await ResetConnectionAsync();

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
                    _ = Task.Run(() => ReadLogsAsync(reader, option.PodName, option.ContainerName, connectionCts.Token));
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
        _connectionCts?.Cancel();
        for (int i = 0; i < _streamReaders.Count; i++)
        {
            _streamReaders[i].Dispose();
        }

        for (int i = 0; i < _streams.Count; i++)
        {
            _streams[i].Dispose();
        }

        _connectionCts?.Dispose();
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

    private async Task ResetConnectionAsync()
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
        IsConnected = false;
    }

    private List<PodLogReadOptions> BuildReadTargets(PodLogSessionState state, PodLogSessionResolution resolution)
    {
        List<PodLogReadOptions> targets = [];
        IReadOnlyList<V1Pod> pods = ResolveSelectedPods(resolution.RelatedPods, resolution.Pod);
        IReadOnlyList<PodLogContainerOption> containers = ResolveSelectedContainers(resolution.Pod, state.ContainerName);

        for (int i = 0; i < pods.Count; i++)
        {
            V1Pod pod = pods[i];

            for (int j = 0; j < containers.Count; j++)
            {
                PodLogContainerOption container = containers[j];
                targets.Add(CreateReadOptionsForPod(state, pod, container.Name));
            }
        }

        return targets;
    }

    private PodLogReadOptions CreateReadOptionsForPod(PodLogSessionState state, V1Pod pod, string containerName)
    {
        bool previousLogsAvailable = HasPreviousLogs(pod, containerName);
        return new PodLogReadOptions(
            pod.Namespace(),
            pod.Name(),
            containerName,
            state.Previous && previousLogsAvailable,
            state.Timestamps,
            true,
            state.TailLines);
    }

    private static IReadOnlyList<PodLogContainerOption> BuildContainerOptions(V1Pod pod)
    {
        List<PodLogContainerOption> containers = [];

        IList<V1Container>? initContainers = pod.Spec?.InitContainers;
        if (initContainers is not null)
        {
            for (int i = 0; i < initContainers.Count; i++)
            {
                V1Container container = initContainers[i];
                containers.Add(new PodLogContainerOption(container.Name, $"{container.Name} (init)", IsInitContainer: true));
            }
        }

        IList<V1Container>? appContainers = pod.Spec?.Containers;
        if (appContainers is not null)
        {
            for (int i = 0; i < appContainers.Count; i++)
            {
                V1Container container = appContainers[i];
                containers.Add(new PodLogContainerOption(container.Name, container.Name, IsInitContainer: false));
            }
        }

        return containers;
    }

    private static IReadOnlyList<PodLogPodSelectionItem> BuildPodSelectionItems(IReadOnlyList<V1Pod> pods)
    {
        List<PodLogPodSelectionItem> items = [new PodLogPodSelectionItem(null, Assets.Resources.PodLogsView_AllPods, true)];

        for (int i = 0; i < pods.Count; i++)
        {
            V1Pod pod = pods[i];
            items.Add(new PodLogPodSelectionItem(pod, pod.Name(), false));
        }

        return items;
    }

    private static IReadOnlyList<PodLogContainerSelectionItem> BuildContainerSelectionItems(IReadOnlyList<PodLogContainerOption> containers)
    {
        List<PodLogContainerSelectionItem> items = [new PodLogContainerSelectionItem(string.Empty, Assets.Resources.PodLogsView_AllContainers, false, true)];

        for (int i = 0; i < containers.Count; i++)
        {
            PodLogContainerOption container = containers[i];
            items.Add(new PodLogContainerSelectionItem(container.Name, container.DisplayName, container.IsInitContainer, false));
        }

        return items;
    }

    private ObservableCollection<PodLogPodSelectionItem> BuildSelectedPodItems(V1Pod currentPod, IReadOnlyList<PodLogPodSelectionItem> selectionItems)
    {
        if (SelectedPodItems.Count == 0)
        {
            PodLogPodSelectionItem? currentItem = FindPodSelectionItem(selectionItems, currentPod.Name());
            return currentItem is null ? new ObservableCollection<PodLogPodSelectionItem>([selectionItems[0]]) : new ObservableCollection<PodLogPodSelectionItem>([currentItem]);
        }

        if (ContainsAllSelection(SelectedPodItems))
        {
            return new ObservableCollection<PodLogPodSelectionItem>([selectionItems[0]]);
        }

        List<PodLogPodSelectionItem> selected = [];
        for (int i = 0; i < SelectedPodItems.Count; i++)
        {
            PodLogPodSelectionItem selectedItem = SelectedPodItems[i];
            if (selectedItem.IsAll)
            {
                return new ObservableCollection<PodLogPodSelectionItem>([selectionItems[0]]);
            }

            PodLogPodSelectionItem? item = FindPodSelectionItem(selectionItems, selectedItem.Pod?.Name() ?? string.Empty);
            if (item is not null)
            {
                selected.Add(item);
            }
        }

        if (selected.Count == 0)
        {
            PodLogPodSelectionItem? currentItem = FindPodSelectionItem(selectionItems, currentPod.Name());
            return currentItem is null ? new ObservableCollection<PodLogPodSelectionItem>([selectionItems[0]]) : new ObservableCollection<PodLogPodSelectionItem>([currentItem]);
        }

        return new ObservableCollection<PodLogPodSelectionItem>(selected);
    }

    private ObservableCollection<PodLogContainerSelectionItem> BuildSelectedContainerItems(string selectedContainerName, IReadOnlyList<PodLogContainerSelectionItem> selectionItems)
    {
        if (SelectedContainerItems.Count == 0)
        {
            PodLogContainerSelectionItem? currentItem = FindContainerSelectionItem(selectionItems, selectedContainerName);
            return currentItem is null ? new ObservableCollection<PodLogContainerSelectionItem>([selectionItems[0]]) : new ObservableCollection<PodLogContainerSelectionItem>([currentItem]);
        }

        if (ContainsAllSelection(SelectedContainerItems))
        {
            return new ObservableCollection<PodLogContainerSelectionItem>([selectionItems[0]]);
        }

        List<PodLogContainerSelectionItem> selected = [];
        for (int i = 0; i < SelectedContainerItems.Count; i++)
        {
            PodLogContainerSelectionItem selectedItem = SelectedContainerItems[i];
            if (selectedItem.IsAll)
            {
                return new ObservableCollection<PodLogContainerSelectionItem>([selectionItems[0]]);
            }

            PodLogContainerSelectionItem? item = FindContainerSelectionItem(selectionItems, selectedItem.Name, selectedItem.IsInitContainer);
            if (item is not null)
            {
                selected.Add(item);
            }
        }

        if (selected.Count == 0)
        {
            PodLogContainerSelectionItem? currentItem = FindContainerSelectionItem(selectionItems, selectedContainerName);
            return currentItem is null ? new ObservableCollection<PodLogContainerSelectionItem>([selectionItems[0]]) : new ObservableCollection<PodLogContainerSelectionItem>([currentItem]);
        }

        return new ObservableCollection<PodLogContainerSelectionItem>(selected);
    }

    private IReadOnlyList<V1Pod> ResolveSelectedPods(IReadOnlyList<V1Pod> availablePods, V1Pod fallbackPod)
    {
        if (SelectedPodItems.Count == 0 || ContainsAllSelection(SelectedPodItems))
        {
            return availablePods;
        }

        HashSet<string> selectedNames = new(StringComparer.Ordinal);
        for (int i = 0; i < SelectedPodItems.Count; i++)
        {
            PodLogPodSelectionItem item = SelectedPodItems[i];
            if (!item.IsAll && item.Pod is not null)
            {
                selectedNames.Add(item.Pod.Name());
            }
        }

        if (selectedNames.Count == 0)
        {
            return [fallbackPod];
        }

        List<V1Pod> selectedPods = [];
        for (int i = 0; i < availablePods.Count; i++)
        {
            V1Pod pod = availablePods[i];
            if (selectedNames.Contains(pod.Name()))
            {
                selectedPods.Add(pod);
            }
        }

        return selectedPods.Count > 0 ? selectedPods : [fallbackPod];
    }

    private IReadOnlyList<PodLogContainerOption> ResolveSelectedContainers(V1Pod pod, string fallbackContainerName)
    {
        List<PodLogContainerOption> availableContainers = [.. BuildContainerOptions(pod)];
        if (SelectedContainerItems.Count == 0 || ContainsAllSelection(SelectedContainerItems))
        {
            return availableContainers;
        }

        HashSet<string> selectedKeys = new(StringComparer.Ordinal);
        for (int i = 0; i < SelectedContainerItems.Count; i++)
        {
            PodLogContainerSelectionItem item = SelectedContainerItems[i];
            if (!item.IsAll)
            {
                selectedKeys.Add(GetContainerSelectionKey(item));
            }
        }

        if (selectedKeys.Count == 0)
        {
            return ResolveFallbackContainers(pod, fallbackContainerName);
        }

        List<PodLogContainerOption> selectedContainers = [];
        for (int i = 0; i < availableContainers.Count; i++)
        {
            PodLogContainerOption container = availableContainers[i];
            if (selectedKeys.Contains(GetContainerSelectionKey(container)))
            {
                selectedContainers.Add(container);
            }
        }

        return selectedContainers.Count > 0 ? selectedContainers : ResolveFallbackContainers(pod, fallbackContainerName);
    }

    private static IReadOnlyList<PodLogContainerOption> ResolveFallbackContainers(V1Pod pod, string fallbackContainerName)
    {
        string resolvedContainerName = ResolveContainerName(pod, fallbackContainerName);
        List<PodLogContainerOption> containers = [.. BuildContainerOptions(pod)];
        for (int i = 0; i < containers.Count; i++)
        {
            if (string.Equals(containers[i].Name, resolvedContainerName, StringComparison.Ordinal))
            {
                return [containers[i]];
            }
        }

        return containers.Count > 0 ? [containers[0]] : [];
    }

    private static PodLogPodSelectionItem? FindPodSelectionItem(IReadOnlyList<PodLogPodSelectionItem> items, string podName)
    {
        for (int i = 0; i < items.Count; i++)
        {
            PodLogPodSelectionItem item = items[i];
            if (!item.IsAll && item.Pod is not null && string.Equals(item.Pod.Name(), podName, StringComparison.Ordinal))
            {
                return item;
            }
        }

        return null;
    }

    private static PodLogContainerSelectionItem? FindContainerSelectionItem(IReadOnlyList<PodLogContainerSelectionItem> items, string containerName, bool? isInitContainer = null)
    {
        for (int i = 0; i < items.Count; i++)
        {
            PodLogContainerSelectionItem item = items[i];
            if (item.IsAll)
            {
                continue;
            }

            if (!string.Equals(item.Name, containerName, StringComparison.Ordinal))
            {
                continue;
            }

            if (isInitContainer.HasValue && item.IsInitContainer != isInitContainer.Value)
            {
                continue;
            }

            return item;
        }

        return null;
    }

    private static string ResolveContainerName(V1Pod pod, string requestedContainerName)
    {
        string? containerName = FindContainerName(pod.Spec?.Containers, requestedContainerName);
        if (!string.IsNullOrWhiteSpace(containerName))
        {
            return containerName;
        }

        containerName = FindContainerName(pod.Spec?.InitContainers, requestedContainerName);
        if (!string.IsNullOrWhiteSpace(containerName))
        {
            return containerName;
        }

        if (pod.Spec?.Containers is { Count: > 0 })
        {
            return pod.Spec.Containers[0].Name;
        }

        if (pod.Spec?.InitContainers is { Count: > 0 })
        {
            return pod.Spec.InitContainers[0].Name;
        }

        return requestedContainerName;
    }

    private static string? FindContainerName(IList<V1Container>? containers, string requestedContainerName)
    {
        if (containers is null || containers.Count == 0)
        {
            return null;
        }

        for (int i = 0; i < containers.Count; i++)
        {
            V1Container container = containers[i];
            if (string.Equals(container.Name, requestedContainerName, StringComparison.Ordinal))
            {
                return container.Name;
            }
        }

        return null;
    }

    private static bool HasPreviousLogs(V1Pod pod, string containerName)
    {
        return GetRestartCount(pod.Status?.ContainerStatuses, containerName) > 0
            || GetRestartCount(pod.Status?.InitContainerStatuses, containerName) > 0;
    }

    private static int GetRestartCount(IList<V1ContainerStatus>? containerStatuses, string containerName)
    {
        if (containerStatuses is null || containerStatuses.Count == 0)
        {
            return 0;
        }

        for (int i = 0; i < containerStatuses.Count; i++)
        {
            V1ContainerStatus containerStatus = containerStatuses[i];
            if (string.Equals(containerStatus.Name, containerName, StringComparison.Ordinal))
            {
                return containerStatus.RestartCount;
            }
        }

        return 0;
    }

    private static string GetContainerSelectionKey(PodLogContainerSelectionItem item)
    {
        return $"{item.Name}|{item.IsInitContainer}";
    }

    private static string GetContainerSelectionKey(PodLogContainerOption item)
    {
        return $"{item.Name}|{item.IsInitContainer}";
    }

    private static bool ContainsAllSelection<TSelectionItem>(IReadOnlyCollection<TSelectionItem> selectedItems)
    {
        if (selectedItems.Count == 0)
        {
            return true;
        }

        foreach (TSelectionItem selectedItem in selectedItems)
        {
            switch (selectedItem)
            {
                case PodLogPodSelectionItem podSelectionItem when podSelectionItem.IsAll:
                    return true;
                case PodLogContainerSelectionItem containerSelectionItem when containerSelectionItem.IsAll:
                    return true;
            }
        }

        return false;
    }

    private void SelectedPodItemsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_isApplyingSession || _isNormalizingPodSelection)
        {
            return;
        }

        if (SelectedPodItems.Count == 0 || SelectedPodItems.Count == PodSelectionItems.Count - 1 || ContainsAllSelection(SelectedPodItems))
        {
            _isNormalizingPodSelection = true;
            try
            {
                if (PodSelectionItems.Count > 0)
                {
                    SelectedPodItems.Clear();
                    SelectedPodItems.Add(PodSelectionItems[0]);
                }
            }
            finally
            {
                _isNormalizingPodSelection = false;
            }
        }

        UpdateResourceNameToggleState();
        RequestReconnect();
    }

    private void SelectedContainerItemsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_isApplyingSession || _isNormalizingContainerSelection)
        {
            return;
        }

        if (SelectedContainerItems.Count == 0 || SelectedContainerItems.Count == ContainerSelectionItems.Count - 1 || ContainsAllSelection(SelectedContainerItems))
        {
            _isNormalizingContainerSelection = true;
            try
            {
                if (ContainerSelectionItems.Count > 0)
                {
                    SelectedContainerItems.Clear();
                    SelectedContainerItems.Add(ContainerSelectionItems[0]);
                }
            }
            finally
            {
                _isNormalizingContainerSelection = false;
            }
        }

        UpdateResourceNameToggleState();
        RequestReconnect();
    }

    private void SelectAllPods()
    {
        if (PodSelectionItems.Count == 0)
        {
            return;
        }

        _isNormalizingPodSelection = true;
        try
        {
            SelectedPodItems.Clear();
            SelectedPodItems.Add(PodSelectionItems[0]);
        }
        finally
        {
            _isNormalizingPodSelection = false;
        }

        UpdateResourceNameToggleState();
    }

    private void UpdateResourceNameToggleState()
    {
        OnPropertyChanged(nameof(CanShowResourceNames));
        if (ShowResourceNames)
        {
            RenderOutputEntries();
        }
    }

    private void ReplaceSelectedPodItems(IEnumerable<PodLogPodSelectionItem> items)
    {
        bool wasApplyingSession = _isApplyingSession;
        _isApplyingSession = true;
        try
        {
            SelectedPodItems.Clear();
            foreach (PodLogPodSelectionItem item in items)
            {
                SelectedPodItems.Add(item);
            }
        }
        finally
        {
            _isApplyingSession = wasApplyingSession;
        }
    }

    private void ReplaceSelectedContainerItems(IEnumerable<PodLogContainerSelectionItem> items)
    {
        bool wasApplyingSession = _isApplyingSession;
        _isApplyingSession = true;
        try
        {
            SelectedContainerItems.Clear();
            foreach (PodLogContainerSelectionItem item in items)
            {
                SelectedContainerItems.Add(item);
            }
        }
        finally
        {
            _isApplyingSession = wasApplyingSession;
        }
    }

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
        string namespaceName = Object?.Metadata?.NamespaceProperty;

        string fileName = namespaceName is { Length: > 0 }
            ? $"{namespaceName}-{podName}-{containerName}.log"
            : $"{podName}-{containerName}.log";

        return fileName.ReplaceInvalidFileNameChars();
    }

    private async Task ReadLogsAsync(StreamReader reader, string podName, string containerName, CancellationToken cancellationToken)
    {
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                string? log = await reader.ReadLineAsync();
                if (log is null)
                {
                    break;
                }

                PodLogOutputEntry entry = new(podName, containerName, log);
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
            DecrementActiveReaders();
        }
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

public sealed record PodLogContainerOption(string Name, string DisplayName, bool IsInitContainer);

public sealed record PodLogPodSelectionItem(V1Pod? Pod, string DisplayName, bool IsAll);

public sealed record PodLogContainerSelectionItem(string Name, string DisplayName, bool IsInitContainer, bool IsAll);

internal readonly record struct PodLogOutputEntry(string PodName, string ContainerName, string Message);

internal enum PodLogDisplayMode
{
    None,
    Container,
    PodAndContainer,
}

internal static class PodLogFileNameExtensions
{
    public static string ReplaceInvalidFileNameChars(this string value)
    {
        char[] invalidChars = Path.GetInvalidFileNameChars();
        StringBuilder builder = new(value.Length);
        for (int i = 0; i < value.Length; i++)
        {
            char character = value[i];
            builder.Append(Array.IndexOf(invalidChars, character) >= 0 ? '_' : character);
        }

        return builder.ToString();
    }

    internal static V1OwnerReference? GetControllerReference(V1Pod pod)
    {
        IList<V1OwnerReference>? ownerReferences = pod.Metadata?.OwnerReferences;
        if (ownerReferences is null)
        {
            return null;
        }

        for (int i = 0; i < ownerReferences.Count; i++)
        {
            V1OwnerReference ownerReference = ownerReferences[i];
            if (ownerReference.Controller == true)
            {
                return ownerReference;
            }
        }

        return ownerReferences.Count > 0 ? ownerReferences[0] : null;
    }
}
