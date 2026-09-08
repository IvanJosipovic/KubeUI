using System.Collections.ObjectModel;
using System.Collections.Concurrent;
using System.Collections.Specialized;
using System.Globalization;
using System.IO;
using System.Reactive.Linq;
using Avalonia.Threading;
using AvaloniaEdit.Document;
using Dock.Model.Core;
using Humanizer;
using k8s;
using k8s.Autorest;
using k8s.Models;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Avalonia.Resources.Workloads.v1.Pod.Services;
using KubeUI.Avalonia.Services.Settings;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Pod.ViewModels;

/// <summary>Coordinates pod-log selection, streaming, reconnects, and display state.</summary>
public sealed partial class PodLogsViewModel : ViewModelBase, IDisposable
{
    private const int DefaultTailLines = 500;
    private const int MaxLogEntries = 10_000;
    private const int MaxAutomaticReconnectAttempts = 5;
    internal const int StreamWarningThreshold = 25;
    internal const int MaxStreamCount = 100;
    internal const int MaxScopeCount = 20;

    private readonly ILogger<PodLogsViewModel> _logger;
    private readonly IPodLogExportService _exportService;
    private readonly IPodLogSessionResolver _sessionResolver;
    private readonly IPodLogStreamClient _streamClient;
    private readonly Func<int, CancellationToken, Task> _automaticReconnectDelay;
    private readonly SemaphoreSlim _connectionGate = new(1, 1);
    private CancellationTokenSource? _connectionCts;
    private bool _disposed;
    private readonly object _outputEntriesGate = new();
    private readonly List<PodLogOutputEntry> _outputEntries = [];
    private readonly object _streamsGate = new();
    private readonly List<Stream> _streams = [];
    private readonly List<StreamReader> _streamReaders = [];
    private readonly ConcurrentDictionary<CancellationTokenSource, int> _readerCounts = new();
    private bool _hasLoadedSession;
    private bool _isApplyingSession;
    private bool _pendingSelectionReconnect;
    private bool _pendingReconnect;
    private bool _preserveOutputOnNextConnect;
    private bool _clearOutputBeforeNextConnect;
    private int _streamEndedReconnectAttempts;
    private int _streamEndedReconnectPending;
    private int _activeReaderCount;
    private int _outputGeneration;
    private PodLogDisplayMode _resourceNameDisplayMode;
    private bool _awaitingReadableTargets;
    private IDisposable? _resourceChangesSubscription;
    private IClusterRuntime? _subscribedCluster;
    private bool _isSettingScope;
    private string? _scopeResourceKind;
    private readonly ObservableCollection<PodLogScopeSelectionItem> _scopeItems = [];
    private bool _scopesExplicitlyCleared;
    private bool _isNormalizingScopeSelection;
    private readonly HashSet<string> _resourceKeysToSelectOnResolve = new(StringComparer.Ordinal);

    /// <summary>Initializes a pod-log view model with its streaming and export services.</summary>
    public PodLogsViewModel(
        ILogger<PodLogsViewModel> logger,
        ISettingsService settingsService,
        IPodLogExportService exportService,
        IPodLogSessionResolver sessionResolver,
        IPodLogStreamClient streamClient)
        : this(logger, settingsService, exportService, sessionResolver, streamClient, DelayAutomaticReconnectAsync)
    {
    }

    internal PodLogsViewModel(
        ILogger<PodLogsViewModel> logger,
        ISettingsService settingsService,
        IPodLogExportService exportService,
        IPodLogSessionResolver sessionResolver,
        IPodLogStreamClient streamClient,
        Func<int, CancellationToken, Task> automaticReconnectDelay)
    {
        _logger = logger;
        SettingsService = settingsService;
        _exportService = exportService;
        _sessionResolver = sessionResolver;
        _streamClient = streamClient;
        _automaticReconnectDelay = automaticReconnectDelay;
        Title = Assets.Resources.PodLogsView_Title;
        SelectedScopeItems.CollectionChanged += SelectedScopeItemsOnCollectionChanged;
    }

    /// <summary>Gets the settings service used by the log view.</summary>
    public ISettingsService SettingsService { get; }

    /// <summary>Gets the selected resource name.</summary>
    public string ScopeResourceName => Object?.Name() ?? string.Empty;

    /// <summary>Gets the selected resource namespace.</summary>
    public string ScopeNamespace => Object?.Namespace() ?? string.Empty;

    /// <summary>Gets whether the selected resource has a namespace.</summary>
    public bool HasScopeNamespace => !string.IsNullOrWhiteSpace(ScopeNamespace);

    /// <summary>Gets whether the compact single-resource identity should be shown.</summary>
    public bool ShowSingleScopeIdentity => _scopeItems.Count <= 1;

    /// <summary>Gets whether the namespace should be shown in the single-resource identity.</summary>
    public bool ShowSingleScopeNamespace => ShowSingleScopeIdentity && HasScopeNamespace;

    /// <summary>Gets the resources that contribute Pods to this log session.</summary>
    public IReadOnlyList<PodLogScopeSelectionItem> ScopeItems => _scopeItems;

    /// <summary>Gets selected resources in the multi-resource selector.</summary>
    public ObservableCollection<PodLogScopeSelectionItem> SelectedScopeItems { get; } = [];

    internal ObservableCollection<PodLogSourceTreeNode> SourceTreeItems { get; } = [];

    /// <summary>Gets whether more than one resource contributes to this session.</summary>
    public bool IsMultiScope => _scopeItems.Count > 1;

    /// <summary>Gets a compact summary of all resources contributing to this log session.</summary>
    public string ScopeSummary
    {
        get
        {
            if (!IsMultiScope)
            {
                return ScopeResourceName;
            }

            var distinctKinds = _scopeItems
                .Select(static item => item.ResourceKind)
                .Distinct(StringComparer.Ordinal)
                .ToArray();
            var distinctNamespaces = _scopeItems
                .Select(static item => item.Resource.Namespace())
                .Where(static value => !string.IsNullOrWhiteSpace(value))
                .Distinct(StringComparer.Ordinal)
                .ToArray();

            if (distinctKinds.Length == 1 && distinctNamespaces.Length == 1)
            {
                return string.Format(
                    CultureInfo.CurrentCulture,
                    Assets.Resources.PodLogsView_MultiScopeKindNamespace,
                    _scopeItems.Count,
                    PluralizeKind(distinctKinds[0]),
                    distinctNamespaces[0]);
            }

            if (distinctKinds.Length == 1)
            {
                return string.Format(
                    CultureInfo.CurrentCulture,
                    Assets.Resources.PodLogsView_MultiScopeKindNamespaces,
                    _scopeItems.Count,
                    PluralizeKind(distinctKinds[0]),
                    distinctNamespaces.Length);
            }

            return string.Format(
                CultureInfo.CurrentCulture,
                Assets.Resources.PodLogsView_MultiScopeSummary,
                _scopeItems.Count,
                distinctKinds.Length,
                distinctNamespaces.Length);
        }
    }

    /// <summary>Gets the number of selected resources that currently resolve readable Pods.</summary>
    public string ScopeStatusSummary
    {
        get
        {
            if (!IsMultiScope || MultiSessionResolution is null)
            {
                return string.Empty;
            }

            var activeCount = MultiSessionResolution.Scopes.Count(static scope => scope.Error is null);
            return activeCount == MultiSessionResolution.Scopes.Count
                ? string.Empty
                : $"{activeCount}/{MultiSessionResolution.Scopes.Count} active";
        }
    }

    /// <summary>Gets the resource kind used to resolve the selected scopes.</summary>
    public string ScopeResourceKind => GetScopeResourceKind();

    /// <summary>Gets whether all selected scopes are Pods.</summary>
    public bool IsPodScope => _scopeItems.Count > 0
        ? _scopeItems.All(scope => string.Equals(scope.ResourceKind, V1Pod.KubeKind, StringComparison.Ordinal))
        : string.Equals(ScopeResourceKind, V1Pod.KubeKind, StringComparison.Ordinal);

    /// <summary>Gets whether the primary scope is a workload controller.</summary>
    public bool IsControllerScope => Object is not null && !IsPodScope;

    /// <summary>Gets whether the selected Pod can navigate to its controller logs.</summary>
    public bool CanJumpToController => !IsMultiScope && SessionResolution?.ParentResource is not null;

    [ObservableProperty]
    /// <summary>Gets or sets the cluster that supplies the logs.</summary>
    public partial IClusterRuntime Cluster { get; set; }

    [ObservableProperty]
    /// <summary>Gets or sets the primary selected resource.</summary>
    public partial IKubernetesObject<V1ObjectMeta>? Object { get; set; }

    [ObservableProperty]
    /// <summary>Gets or sets the container name, or an empty value for all containers.</summary>
    public partial string ContainerName { get; set; }

    [ObservableProperty]
    /// <summary>Gets the document containing the displayed log output.</summary>
    public partial TextDocument Logs { get; set; } = CreateLogDocument();

    [ObservableProperty]
    /// <summary>Gets the Pods currently resolved from the selected scopes.</summary>
    public partial IReadOnlyList<V1Pod> AvailablePods { get; set; } = [];

    [ObservableProperty]
    /// <summary>Gets the containers available in the resolved Pods.</summary>
    public partial IReadOnlyList<PodLogContainerOption> AvailableContainers { get; set; } = [];

    [ObservableProperty]
    /// <summary>Gets or sets whether previous container logs are requested.</summary>
    public partial bool Previous { get; set; }

    [ObservableProperty]
    /// <summary>Gets or sets whether timestamps are shown in the log output.</summary>
    public partial bool Timestamps { get; set; }

    [ObservableProperty]
    /// <summary>Gets or sets whether output keeps the editor at the bottom.</summary>
    public partial bool AutoScrollToBottom { get; set; } = true;

    [ObservableProperty]
    /// <summary>Gets or sets whether long log lines wrap in the editor.</summary>
    public partial bool WordWrap { get; set; }

    private bool _showResourceNames;

    /// <summary>Gets or sets whether output lines include resource-name prefixes.</summary>
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

    /// <summary>Gets whether resource-name prefixes are meaningful for the current output.</summary>
    public bool CanShowResourceNames
    {
        get
        {
            return GetCurrentDisplayMode() != PodLogDisplayMode.None;
        }
    }

    [ObservableProperty]
    /// <summary>Gets or sets the request to resume following the newest logs.</summary>
    public partial bool FollowLogsRequested { get; set; }

    [ObservableProperty]
    /// <summary>Gets or sets the persisted editor scroll offset.</summary>
    public partial Vector ScrollOffset { get; set; }

    [ObservableProperty]
    /// <summary>Gets or sets the resolved single-scope session state.</summary>
    public partial PodLogSessionState? SessionState { get; set; }

    [ObservableProperty]
    /// <summary>Gets or sets the resolved single-scope log target.</summary>
    public partial PodLogSessionResolution? SessionResolution { get; set; }

    [ObservableProperty]
    /// <summary>Gets or sets the resolved multi-scope session state.</summary>
    public partial PodLogMultiSessionState? MultiSessionState { get; set; }

    [ObservableProperty]
    /// <summary>Gets or sets the resolved multi-scope log targets.</summary>
    public partial PodLogMultiSessionResolution? MultiSessionResolution { get; set; }

    [ObservableProperty]
    /// <summary>Gets whether previous logs are available for the current target.</summary>
    public partial bool PreviousLogsAvailable { get; set; }

    [ObservableProperty]
    /// <summary>Gets whether the view is resolving or opening log streams.</summary>
    public partial bool IsConnecting { get; set; }

    [ObservableProperty]
    /// <summary>Gets whether at least one log stream is active.</summary>
    public partial bool IsConnected { get; set; }

    [ObservableProperty]
    /// <summary>Gets the number of streams planned for the current selection.</summary>
    public partial int PlannedStreamCount { get; set; }

    /// <summary>Gets whether the selected targets exceed the recommended stream count.</summary>
    public bool HasStreamLimitWarning => PlannedStreamCount > StreamWarningThreshold;

    /// <summary>Gets a warning describing the current stream fan-out.</summary>
    public string StreamLimitWarning => HasStreamLimitWarning && PlannedStreamCount <= MaxStreamCount
        ? string.Format(
            CultureInfo.CurrentCulture,
            Assets.Resources.PodLogsView_StreamLimitWarning,
            PlannedStreamCount,
            StreamWarningThreshold)
        : string.Empty;

    [ObservableProperty]
    /// <summary>Gets the latest user-visible connection error.</summary>
    public partial string? ConnectionError { get; set; }

    /// <summary>Resolves the selected resources and opens their log streams.</summary>
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
            var clearOutputBeforeResolve = _clearOutputBeforeNextConnect;
            _clearOutputBeforeNextConnect = false;
            if (!preserveOutput)
            {
                _streamEndedReconnectAttempts = 0;
            }
            ResetConnection();
            if (clearOutputBeforeResolve)
            {
                ClearOutput();
            }

            IsConnecting = true;
            ConnectionError = null;

            IReadOnlyList<PodLogScopeSelectionItem> scopeItems = GetScopeItems();
            if (scopeItems.Count == 0)
            {
                if (!_scopesExplicitlyCleared)
                {
                    throw new InvalidOperationException("The pod log view model is not initialized.");
                }

                SessionState = null;
                SessionResolution = null;
                MultiSessionState = null;
                MultiSessionResolution = null;
                AvailablePods = [];
                AvailableContainers = [];
                SourceTreeItems.Clear();
                UpdatePlannedStreamCount(0);
                ConnectionError = Assets.Resources.PodLogsView_NoResources;
                return;
            }

            IKubernetesObject<V1ObjectMeta> scopeResource = scopeItems[0].Resource;
            var loadTasks = new Task[scopeItems.Count];
            for (var scopeIndex = 0; scopeIndex < scopeItems.Count; scopeIndex++)
            {
                loadTasks[scopeIndex] = LoadScopeResourcesAsync(scopeItems[scopeIndex].Resource);
            }

            await Task.WhenAll(loadTasks);

            if (_disposed)
            {
                return;
            }

            PodLogMultiSessionState multiState = _sessionResolver.CreateMultiState(
                scopeItems.Select(static item => item.Resource).ToArray(),
                ContainerName,
                Previous,
                Timestamps,
                DefaultTailLines);
            MultiSessionState = multiState;
            PodLogSessionState state = _sessionResolver.CreateState(
                scopeResource,
                ContainerName,
                Previous,
                Timestamps,
                DefaultTailLines);
            SessionState = state;
            _hasLoadedSession = true;
            EnsureResourceChangeSubscription();

            PodLogMultiSessionResolution multiResolution = _sessionResolver.TryResolve(Cluster, multiState);
            MultiSessionResolution = multiResolution;
            UpdateScopeResolutionPresentation(multiResolution);
            OnPropertyChanged(nameof(ScopeStatusSummary));
            PodLogSessionResolution? resolution = multiResolution.PrimaryPod is null
                ? null
                : new PodLogSessionResolution(
                    multiResolution.PrimaryPod,
                    multiResolution.ContainerName,
                    multiResolution.RelatedPods,
                    !string.Equals(state.ResourceUid, multiResolution.PrimaryPod.Metadata?.Uid, StringComparison.Ordinal),
                    multiResolution.PreviousLogsAvailable,
                    multiResolution.Scopes.Count == 1 ? multiResolution.Scopes[0].ParentResource : null);
            if (resolution is null)
            {
                _awaitingReadableTargets = true;
                SessionResolution = null;
                if (Object is V1Pod unresolvedPod)
                {
                    AvailablePods = [unresolvedPod];
                    AvailableContainers = BuildContainerOptions(unresolvedPod);
                }
                else
                {
                    AvailablePods = [];
                    AvailableContainers = [];
                }

                ReconcileSourceTree();
                PreviousLogsAvailable = false;
                UpdatePlannedStreamCount(0);
                UpdateResourceNameToggleState();
                return;
            }

            SessionState = state;
            SessionResolution = resolution;
            PreviousLogsAvailable = resolution.PreviousLogsAvailable;

            _isApplyingSession = true;
            try
            {
                AvailablePods = resolution.RelatedPods;
                AvailableContainers = BuildContainerOptions(resolution.RelatedPods);
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

            ReconcileSourceTree();
            UpdateResourceNameToggleState();

            var options = BuildReadTargets(state);
            UpdatePlannedStreamCount(options.Count);
            if (options.Count > MaxStreamCount)
            {
                _awaitingReadableTargets = true;
                ConnectionError = string.Format(
                    CultureInfo.CurrentCulture,
                    Assets.Resources.PodLogsView_StreamLimitExceeded,
                    options.Count,
                    MaxStreamCount);
                return;
            }

            if (options.Count == 0)
            {
                _awaitingReadableTargets = true;
                return;
            }

            _awaitingReadableTargets = false;
            if (!preserveOutput && !clearOutputBeforeResolve)
            {
                ClearOutput();
            }

            CancellationTokenSource connectionCts = new();
            _connectionCts = connectionCts;

            IsConnected = true;
            _activeReaderCount = options.Count;
            _readerCounts[connectionCts] = options.Count;

            var openTasks = new Task[options.Count];
            for (var i = 0; i < options.Count; i++)
            {
                openTasks[i] = OpenAndReadLogsAsync(options[i], connectionCts, resolution);
            }

            await Task.WhenAll(openTasks);

            if (Volatile.Read(ref _activeReaderCount) == 0)
            {
                IsConnected = false;
                return;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to view pod logs.");
            if (!_pendingReconnect)
            {
                ConnectionError = ex.Message;
            }
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

    private async Task LoadScopeResourcesAsync(IKubernetesObject<V1ObjectMeta> selectedResource)
    {
        await PodLogResourceLoader.EnsureScopeResourcesAsync(Cluster, selectedResource);
        try
        {
            await PodLogResourceLoader
                .EnsureParentResourceAsync(Cluster, selectedResource)
                .WaitAsync(TimeSpan.FromSeconds(2));
        }
        catch (Exception ex) when (ex is HttpOperationException
            or HttpRequestException
            or TaskCanceledException
            or TimeoutException)
        {
            LogUnableToLoadParentResource(ex, selectedResource.Namespace(), selectedResource.Name());
        }
    }

    private async Task OpenAndReadLogsAsync(
        PodLogReadOptions option,
        CancellationTokenSource connectionCts,
        PodLogSessionResolution resolution)
    {
        Stream? stream = null;
        StreamReader? reader = null;
        try
        {
            stream = await _streamClient.OpenAsync(Cluster, option, connectionCts.Token);
            reader = new StreamReader(stream);
            stream = null;
            var registered = false;
            lock (_streamsGate)
            {
                if (!_disposed
                    && !connectionCts.IsCancellationRequested
                    && ReferenceEquals(_connectionCts, connectionCts))
                {
                    _streams.Add(reader.BaseStream);
                    _streamReaders.Add(reader);
                    registered = true;
                }
            }

            if (!registered)
            {
                DecrementActiveReaders(connectionCts);
                return;
            }

            StreamReader registeredReader = reader;
            reader = null;
            _ = Task.Run(() => ReadLogsAsync(registeredReader, option, connectionCts, resolution));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to open pod log stream for {PodNamespace}/{PodName} container {ContainerName}.", option.PodNamespace, option.PodName, option.ContainerName);
            var isLastActiveReader = DecrementActiveReaders(connectionCts);
            if (!_pendingReconnect && IsCurrentConnection(connectionCts))
            {
                AppendStatusLine(option.PodName, option.ContainerName, ex.Message, connectionCts);
                ConnectionError = ex.Message;
            }

            if (!_pendingReconnect
                && ex is IOException or HttpRequestException
                && option.Follow
                && !option.Previous
                && !connectionCts.IsCancellationRequested
                && !IsTerminalPod(resolution.Pod)
                && isLastActiveReader)
            {
                ScheduleReconnectAfterStreamEnd(connectionCts);
            }
        }
        finally
        {
            reader?.Dispose();
            stream?.Dispose();
        }
    }

    [RelayCommand]
    private void ClearScopes()
    {
        ClearScopes(updateSelection: true);
    }

    private void ClearScopes(bool updateSelection)
    {
        if (_scopeItems.Count == 0)
        {
            return;
        }

        _isApplyingSession = true;
        try
        {
            _scopeItems.Clear();
            _resourceKeysToSelectOnResolve.Clear();
            _scopesExplicitlyCleared = true;
            _scopeResourceKind = null;
            Object = null;
            SessionState = null;
            SessionResolution = null;
            MultiSessionState = null;
            MultiSessionResolution = null;
            AvailablePods = [];
            AvailableContainers = [];
            SourceTreeItems.Clear();
            UpdatePlannedStreamCount(0);
            ResetConnection();
        }
        finally
        {
            _isApplyingSession = false;
        }

        ConnectionError = Assets.Resources.PodLogsView_NoResources;
        UpdateScopePresentation(updateSelection);
    }

    [RelayCommand]
    /// <summary>Clears the displayed and buffered log output.</summary>
    public void Clear()
    {
        ClearOutput();
    }

    [RelayCommand]
    /// <summary>Reconnects and reloads the current log session.</summary>
    public Task Refresh()
    {
        return Connect();
    }

    [RelayCommand]
    /// <summary>Requests that the editor follow the newest output.</summary>
    public void FollowLogs()
    {
        AutoScrollToBottom = true;
        FollowLogsRequested = true;
    }

    [RelayCommand]
    /// <summary>Exports the current log output through the platform picker.</summary>
    public Task DownloadLogs()
    {
        var suggestedFileName = BuildSuggestedFileName();
        return DownloadLogsAsync(suggestedFileName);
    }

    private async Task DownloadLogsAsync(string suggestedFileName)
    {
        try
        {
            await _exportService.ExportAsync(suggestedFileName, BuildExportContent());
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
    /// <summary>Opens the logs for the selected resource's controller.</summary>
    public Task JumpToControlledByLogs()
    {
        IKubernetesObject<V1ObjectMeta>? parentResource = SessionResolution?.ParentResource;
        if (parentResource is null)
        {
            return Task.CompletedTask;
        }

        _isApplyingSession = true;
        try
        {
            SetScope(parentResource, parentResource.Kind);
            ContainerName = string.Empty;
            SourceTreeItems.Clear();
            _preserveOutputOnNextConnect = false;
            _pendingReconnect = false;
            _clearOutputBeforeNextConnect = true;
        }
        finally
        {
            _isApplyingSession = false;
        }

        return Connect();
    }

    /// <summary>Stops active streams and releases the view's subscriptions.</summary>
    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        _resourceChangesSubscription?.Dispose();
        _resourceChangesSubscription = null;
        _subscribedCluster = null;

        ResetConnection(updateConnectionState: false);

        void ClearLogDocument()
        {
            IsConnected = false;
            TextDocument logs = Logs;
            logs.Text = string.Empty;
            Logs = CreateLogDocument();
        }

        if (Dispatcher.UIThread.CheckAccess())
        {
            ClearLogDocument();
        }
        else
        {
            Dispatcher.UIThread.Invoke(ClearLogDocument);
        }

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

    partial void OnAutoScrollToBottomChanged(bool value)
    {
        if (value)
        {
            FollowLogsRequested = true;
        }
    }

    partial void OnObjectChanged(IKubernetesObject<V1ObjectMeta>? value)
    {
        if (!_isSettingScope && !_isApplyingSession)
        {
            _scopeResourceKind = null;
            _scopeItems.Clear();
            _resourceKeysToSelectOnResolve.Clear();
            _scopesExplicitlyCleared = false;
            if (value is not null)
            {
                var kind = GetKnownResourceKind(value) ?? value.Kind;
                _scopeItems.Add(new PodLogScopeSelectionItem(value, kind, BuildScopeDisplayName(value, kind)));
            }
        }

        UpdateScopePresentation();
        if (_isApplyingSession)
        {
            return;
        }

        UpdateResourceNameToggleState();
        RequestReconnect();
    }

    partial void OnSessionResolutionChanged(PodLogSessionResolution? value)
    {
        OnPropertyChanged(nameof(CanJumpToController));
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

    private void UpdateScopePresentation(bool updateSelection = true)
    {
        OnPropertyChanged(nameof(ScopeResourceName));
        OnPropertyChanged(nameof(ScopeNamespace));
        OnPropertyChanged(nameof(HasScopeNamespace));
        OnPropertyChanged(nameof(ShowSingleScopeIdentity));
        OnPropertyChanged(nameof(ShowSingleScopeNamespace));
        OnPropertyChanged(nameof(ScopeResourceKind));
        if (updateSelection)
        {
            ReplaceSelectedScopeItems(_scopeItems);
        }
        OnPropertyChanged(nameof(IsMultiScope));
        OnPropertyChanged(nameof(ScopeSummary));
        OnPropertyChanged(nameof(ScopeStatusSummary));
        OnPropertyChanged(nameof(IsPodScope));
        OnPropertyChanged(nameof(IsControllerScope));
        var resourceKind = ScopeResourceKind;
        Title = Object is null
            ? Assets.Resources.PodLogsView_Title
            : IsMultiScope
            ? BuildMultiScopeTitle()
            : Object is null || IsPodScope
                ? Assets.Resources.PodLogsView_Title
                : string.Format(
                CultureInfo.CurrentCulture,
                Assets.Resources.PodLogsView_ResourceTitleFormat,
                resourceKind);
        ReconcileSourceTree();
        UpdateResourceNameToggleState();
    }

    private void UpdatePlannedStreamCount(int count)
    {
        PlannedStreamCount = count;
        OnPropertyChanged(nameof(HasStreamLimitWarning));
        OnPropertyChanged(nameof(StreamLimitWarning));
    }

    private string BuildMultiScopeTitle()
    {
        var distinctKinds = _scopeItems
            .Select(static item => item.ResourceKind)
            .Distinct(StringComparer.Ordinal)
            .ToArray();
        return distinctKinds.Length == 1
            ? string.Format(
                CultureInfo.CurrentCulture,
                Assets.Resources.PodLogsView_MultiScopeTitleKind,
                _scopeItems.Count,
                PluralizeKind(distinctKinds[0]))
            : string.Format(
                CultureInfo.CurrentCulture,
                Assets.Resources.PodLogsView_MultiScopeTitle,
                _scopeItems.Count);
    }

    private static string PluralizeKind(string kind)
    {
        return kind.Humanize(LetterCasing.Title).Pluralize();
    }

    private void UpdateScopeResolutionPresentation(PodLogMultiSessionResolution resolution)
    {
        for (var itemIndex = 0; itemIndex < _scopeItems.Count; itemIndex++)
        {
            PodLogScopeSelectionItem item = _scopeItems[itemIndex];
            PodLogScopeResolution? scopeResolution = resolution.Scopes.FirstOrDefault(scope =>
                string.Equals(scope.Scope.ResourceKind, item.ResourceKind, StringComparison.Ordinal)
                && string.Equals(scope.Scope.ResourceNamespace, item.Resource.Namespace(), StringComparison.Ordinal)
                && string.Equals(scope.Scope.ResourceName, item.Resource.Name(), StringComparison.Ordinal)
                && (string.IsNullOrWhiteSpace(scope.Scope.ResourceUid)
                    || string.Equals(scope.Scope.ResourceUid, item.Resource.Metadata?.Uid, StringComparison.Ordinal)));
            if (scopeResolution is null)
            {
                item.ResolutionStatus = Assets.Resources.PodLogsView_Resolving;
                item.ResolvedPodCount = 0;
                continue;
            }

            item.ResolutionStatus = scopeResolution.Error ?? Assets.Resources.PodLogsView_Active;
            item.ResolvedPodCount = scopeResolution.Pods.Count;
        }
    }

    private void SelectedScopeItemsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_isNormalizingScopeSelection)
        {
            return;
        }

        if (SelectedScopeItems.Count == 0)
        {
            Dispatcher.UIThread.Post(
                () => ClearScopes(updateSelection: true),
                DispatcherPriority.Background);
            return;
        }

        HashSet<string> selectedIdentities = new(
            SelectedScopeItems.Select(static scope => BuildScopeIdentity(scope.Resource, scope.ResourceKind)),
            StringComparer.Ordinal);
        var removed = 0;
        for (var scopeIndex = _scopeItems.Count - 1; scopeIndex >= 0; scopeIndex--)
        {
            PodLogScopeSelectionItem scope = _scopeItems[scopeIndex];
            if (!selectedIdentities.Contains(BuildScopeIdentity(scope.Resource, scope.ResourceKind)))
            {
                _scopeItems.RemoveAt(scopeIndex);
                removed++;
            }
        }
        if (removed == 0)
        {
            return;
        }

        _resourceKeysToSelectOnResolve.IntersectWith(selectedIdentities);
        if (Object is null || !_scopeItems.Any(scope => ReferenceEquals(scope.Resource, Object)))
        {
            _scopeResourceKind = _scopeItems[0].ResourceKind;
            _isSettingScope = true;
            try
            {
                Object = _scopeItems[0].Resource;
            }
            finally
            {
                _isSettingScope = false;
            }
        }

        UpdateScopePresentation(updateSelection: false);
        Dispatcher.UIThread.Post(
            () => RequestReconnect(preserveOutput: true),
            DispatcherPriority.Background);
    }

    private void ReplaceSelectedScopeItems(IEnumerable<PodLogScopeSelectionItem> items)
    {
        _isNormalizingScopeSelection = true;
        try
        {
            SelectedScopeItems.Clear();
            foreach (PodLogScopeSelectionItem item in items)
            {
                SelectedScopeItems.Add(item);
            }
        }
        finally
        {
            _isNormalizingScopeSelection = false;
        }
    }

    internal void SetScope(IKubernetesObject<V1ObjectMeta> resource, string? resourceKind)
    {
        _scopeItems.Clear();
        _resourceKeysToSelectOnResolve.Clear();
        _scopesExplicitlyCleared = false;
        _scopeResourceKind = GetKnownResourceKind(resource)
            ?? (string.IsNullOrWhiteSpace(resourceKind) ? resource.Kind : resourceKind);
        _scopeItems.Add(new PodLogScopeSelectionItem(
            resource,
            _scopeResourceKind,
            BuildScopeDisplayName(resource, _scopeResourceKind)));
        _isSettingScope = true;
        try
        {
            Object = resource;
        }
        finally
        {
            _isSettingScope = false;
        }

        UpdateScopePresentation();
    }

    internal void SetScopes(
        IReadOnlyList<IKubernetesObject<V1ObjectMeta>> resources,
        string? resourceKind)
    {
        ArgumentNullException.ThrowIfNull(resources);
        if (resources.Count == 0)
        {
            throw new ArgumentException("At least one resource is required.", nameof(resources));
        }
        if (resources.Count > MaxScopeCount)
        {
            throw new ArgumentOutOfRangeException(
                nameof(resources),
                resources.Count,
                $"A logs view can include up to {MaxScopeCount} resources.");
        }

        _scopeItems.Clear();
        _resourceKeysToSelectOnResolve.Clear();
        _scopesExplicitlyCleared = false;
        HashSet<string> identities = new(StringComparer.Ordinal);
        for (var i = 0; i < resources.Count; i++)
        {
            IKubernetesObject<V1ObjectMeta> resource = resources[i];
            var kind = GetKnownResourceKind(resource)
                ?? (string.IsNullOrWhiteSpace(resourceKind) ? resource.Kind : resourceKind);
            var identity = BuildScopeIdentity(resource, kind);
            if (identities.Add(identity))
            {
                _scopeItems.Add(new PodLogScopeSelectionItem(resource, kind, BuildScopeDisplayName(resource, kind)));
            }
        }
        _scopeResourceKind = _scopeItems[0].ResourceKind;
        _isSettingScope = true;
        try
        {
            Object = _scopeItems[0].Resource;
        }
        finally
        {
            _isSettingScope = false;
        }

        UpdateScopePresentation();
    }

    [RelayCommand]
    private void RemoveScope(PodLogScopeSelectionItem? scope)
    {
        if (scope is null || _scopeItems.Count <= 1 || !_scopeItems.Remove(scope))
        {
            return;
        }

        _resourceKeysToSelectOnResolve.Remove(BuildScopeIdentity(scope.Resource, scope.ResourceKind));
        if (ReferenceEquals(Object, scope.Resource))
        {
            _isSettingScope = true;
            try
            {
                _scopeResourceKind = _scopeItems[0].ResourceKind;
                Object = _scopeItems[0].Resource;
            }
            finally
            {
                _isSettingScope = false;
            }
        }

        UpdateScopePresentation();
        RequestReconnect(preserveOutput: true);
    }

    internal Task AddScopesAsync(
        IReadOnlyList<IKubernetesObject<V1ObjectMeta>> resources,
        string? resourceKind)
    {
        ArgumentNullException.ThrowIfNull(resources);
        if (resources.Count == 0)
        {
            return Task.CompletedTask;
        }

        HashSet<string> identities = new(
            _scopeItems.Select(static scope => BuildScopeIdentity(scope.Resource, scope.ResourceKind)),
            StringComparer.Ordinal);
        var added = false;
        for (var i = 0; i < resources.Count; i++)
        {
            IKubernetesObject<V1ObjectMeta> resource = resources[i];
            var kind = GetKnownResourceKind(resource)
                ?? (string.IsNullOrWhiteSpace(resourceKind) ? resource.Kind : resourceKind);
            var identity = BuildScopeIdentity(resource, kind);
            if (!identities.Add(identity))
            {
                continue;
            }

            if (_scopeItems.Count >= MaxScopeCount)
            {
                ConnectionError = string.Format(
                    CultureInfo.CurrentCulture,
                    Assets.Resources.PodLogsView_ScopeLimitExceeded,
                    MaxScopeCount);
                break;
            }

            _scopeItems.Add(new PodLogScopeSelectionItem(resource, kind, BuildScopeDisplayName(resource, kind)));
            _resourceKeysToSelectOnResolve.Add(identity);
            added = true;
        }

        if (!added)
        {
            return Task.CompletedTask;
        }

        _scopesExplicitlyCleared = false;
        if (Object is null)
        {
            _scopeResourceKind = _scopeItems[0].ResourceKind;
            _isSettingScope = true;
            try
            {
                Object = _scopeItems[0].Resource;
            }
            finally
            {
                _isSettingScope = false;
            }
        }

        UpdateScopePresentation();
        _clearOutputBeforeNextConnect = true;
        return ConnectAndRestoreFollowLogsAsync(AutoScrollToBottom);
    }

    private async Task ConnectAndRestoreFollowLogsAsync(bool followLogs)
    {
        await Connect();
        AutoScrollToBottom = followLogs;
    }

    private IReadOnlyList<PodLogScopeSelectionItem> GetScopeItems()
    {
        if (_scopeItems.Count > 0)
        {
            return _scopeItems;
        }

        if (Object is null)
        {
            return [];
        }

        IKubernetesObject<V1ObjectMeta> resource = Object;
        var kind = GetScopeResourceKind();
        _scopeItems.Add(new PodLogScopeSelectionItem(resource, kind, BuildScopeDisplayName(resource, kind)));
        return _scopeItems;
    }

    private static string BuildScopeDisplayName(IKubernetesObject<V1ObjectMeta> resource, string kind)
    {
        var resourceNamespace = resource.Namespace();
        return string.IsNullOrWhiteSpace(resourceNamespace)
            ? $"{kind}/{resource.Name()}"
            : $"{kind}/{resourceNamespace}/{resource.Name()}";
    }

    private static string BuildScopeIdentity(IKubernetesObject<V1ObjectMeta> resource, string kind)
    {
        return $"{kind}\n{resource.Namespace()}\n{resource.Metadata?.Uid ?? resource.Name()}";
    }

    private string GetScopeResourceKind()
    {
        if (!string.IsNullOrWhiteSpace(_scopeResourceKind))
        {
            return _scopeResourceKind;
        }

        return GetKnownResourceKind(Object)
            ?? Object?.Kind
            ?? string.Empty;
    }

    private static string? GetKnownResourceKind(IKubernetesObject<V1ObjectMeta>? resource)
    {
        return resource switch
        {
            V1Pod => V1Pod.KubeKind,
            V1Deployment => V1Deployment.KubeKind,
            V1ReplicaSet => V1ReplicaSet.KubeKind,
            V1DaemonSet => V1DaemonSet.KubeKind,
            V1StatefulSet => V1StatefulSet.KubeKind,
            V1Job => V1Job.KubeKind,
            V1CronJob => V1CronJob.KubeKind,
            _ => null,
        };
    }

    [LoggerMessage(
        Level = LogLevel.Warning,
        Message = "Unable to load the parent resource for {ResourceNamespace}/{ResourceName}; pod logs remain available.")]
    private partial void LogUnableToLoadParentResource(
        Exception exception,
        string? resourceNamespace,
        string? resourceName);

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
        if (_disposed || IsConnecting || MultiSessionState is null)
        {
            return;
        }

        Dispatcher.UIThread.Post(
            () =>
            {
                if (_disposed || MultiSessionState is null)
                {
                    return;
                }

                PodLogMultiSessionResolution resolution = _sessionResolver.TryResolve(Cluster, MultiSessionState);
                if (_awaitingReadableTargets && resolution.PrimaryPod is not null && !IsTerminalPod(resolution.PrimaryPod)
                    || HasMultiScopeTopologyChanged(MultiSessionResolution, resolution))
                {
                    RequestReconnect(preserveOutput: MultiSessionResolution is not null);
                }
            },
            DispatcherPriority.ApplicationIdle);
    }

    private static bool HasMultiScopeTopologyChanged(
        PodLogMultiSessionResolution? current,
        PodLogMultiSessionResolution next)
    {
        if (current is null
            || current.Scopes.Count != next.Scopes.Count
            || current.RelatedPods.Count != next.RelatedPods.Count)
        {
            return true;
        }

        for (var i = 0; i < current.Scopes.Count; i++)
        {
            PodLogScopeResolution currentScope = current.Scopes[i];
            PodLogScopeResolution nextScope = next.Scopes[i];
            if (!Equals(currentScope.Scope, nextScope.Scope)
                || !string.Equals(currentScope.Error, nextScope.Error, StringComparison.Ordinal)
                || !PodLogTopologyComparer.IsSameResource(
                    currentScope.ParentResource,
                    nextScope.ParentResource)
                || currentScope.Pods.Count != nextScope.Pods.Count)
            {
                return true;
            }
        }

        return PodLogTopologyComparer.HavePodsChanged(current.RelatedPods, next.RelatedPods)
            || !string.Equals(current.ContainerName, next.ContainerName, StringComparison.Ordinal);
    }

    private void ResetConnection(bool updateConnectionState = true)
    {
        CancellationTokenSource? previousConnectionCts = _connectionCts;
        try
        {
            previousConnectionCts?.Cancel();
        }
        catch (ObjectDisposedException)
        {
        }

        StreamReader[] streamReaders;
        Stream[] streams;
        lock (_streamsGate)
        {
            streamReaders = _streamReaders.ToArray();
            streams = _streams.ToArray();
            _streamReaders.Clear();
            _streams.Clear();
        }

        for (var i = 0; i < streamReaders.Length; i++)
        {
            streamReaders[i].Dispose();
        }

        for (var i = 0; i < streams.Length; i++)
        {
            streams[i].Dispose();
        }

        _connectionCts = null;
        if (previousConnectionCts is not null && !_readerCounts.ContainsKey(previousConnectionCts))
        {
            previousConnectionCts.Dispose();
        }

        _activeReaderCount = 0;
        if (updateConnectionState)
        {
            IsConnected = false;
        }
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
