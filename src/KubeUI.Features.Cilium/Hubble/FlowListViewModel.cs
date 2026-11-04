using System;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Reactive.Concurrency;
using System.Reactive.Linq;
using Avalonia.Controls.Selection;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Declarative;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using k8s.Models;
using KubeUI.Controls.Presentation;
using KubeUI.Controls.Styles;
using KubeUI.Features.Cilium;
using KubeUI.Features.Cilium.Hubble;
using KubeUI.Features.Clusters.Workspace;
using KubeUI.Features.Resources;
using KubeUI.Kubernetes;
using Microsoft.Extensions.Logging;
using SvcSystems.Avalonia.DynamicTableView;
using Controls = KubeUI.Features.Cilium.Hubble.Controls;
using HubbleConnectionStatus = KubeUI.Features.Cilium.Hubble.ConnectionStatus;


namespace KubeUI.Features.Cilium.Hubble;

public sealed partial class FlowListViewModel : ViewModelBase, IInitializeCluster, IDisposable, IAsyncDisposable
{
    private const string FilterScopeKey = "hubble-flow-scope";
    private readonly ILogger<FlowListViewModel> _logger;
    private readonly IHubbleConnectionFactory _hubbleClientFactory;
    private readonly Lock _lifecycleGate = new();
    private readonly Lock _flowUpdateGate = new();
    private IHubbleConnection? _connection;
    private IDisposable? _flowSubscription;
    private CancellationTokenSource _lifetime = new();
    private int _disposeState;
    private Task? _initializationTask;
    private readonly SourceCache<FlowViewModel, FlowCacheKey> _flowCache = new(FlowCacheKey.From);
    private readonly Dictionary<FlowCacheKey, FlowOrderEntry> _flowOrderByKey = [];
    private readonly SortedSet<FlowOrderEntry> _flowOrder = new(FlowOrderEntryComparer.Instance);
    private readonly SortedSet<FlowOrderEntry> _nonMatchingFlowOrder = new(FlowOrderEntryComparer.Instance);
    private Func<FlowViewModel, bool> _matchesCurrentScope = static _ => true;
    private long _flowOrderSequence;
    private int _pausedState;
    private DynamicTableViewSource<FlowViewModel, FlowCacheKey>? _tableSource;
    private readonly ObservableCollection<V1Namespace> _localSelectedNamespaces = [];
    private ObservableCollection<V1Namespace>? _subscribedNamespaces;
    private FlowCacheKey[]? _selectionRuntimeState;

    [ObservableProperty] public partial ClusterWorkspace? Cluster { get; set; }
    [ObservableProperty] public partial bool IsConnected { get; set; }
    [ObservableProperty] public partial string ConnectionStatus { get; set; } = string.Empty;
    [ObservableProperty] public partial string SearchQuery { get; set; } = string.Empty;
    [ObservableProperty] public partial int FlowCount { get; set; }
    [ObservableProperty] public partial bool IsPaused { get; set; }
    [ObservableProperty] public partial bool IsNamespaceSelectionLinked { get; set; } = true;
    [ObservableProperty] public partial int MaxFlows { get; set; } = 10000;
    public FlowFilterViewModel Filter { get; } = new();

    [ObservableProperty] public partial DynamicTableViewState? TableViewRuntimeState { get; set; }

    public IDynamicTableViewSource TableSource => _tableSource ?? throw new InvalidOperationException("Not initialized");
    public ISelectionModel SelectionModel => TableSource.SelectionModel;
    public FlowViewModel? SelectedItem => SelectionModel.SelectedItem as FlowViewModel;

    public ObservableCollection<V1Namespace> SelectedNamespaces
        => IsNamespaceSelectionLinked && Cluster is not null ? Cluster.SelectedNamespaces : _localSelectedNamespaces;

    public IRelayCommand ClearCommand { get; }
    public IRelayCommand ResetFilterCommand { get; }

    public IEnumerable<MenuItemViewModel> GetContextMenuItems(IEnumerable? _)
    {
        return [];
    }

    public FlowListViewModel(
        ILogger<FlowListViewModel> logger,
        IHubbleConnectionFactory hubbleClientFactory)
    {
        _logger = logger;
        _hubbleClientFactory = hubbleClientFactory;

        ClearCommand = new RelayCommand(() =>
        {
            lock (_flowUpdateGate)
            {
                _flowCache.Clear();
                _flowOrderByKey.Clear();
                _flowOrder.Clear();
                _nonMatchingFlowOrder.Clear();
                FlowCount = 0;
            }
        });
        ResetFilterCommand = new RelayCommand(() => Filter.Reset());
        Filter.PropertyChanged += OnFilterPropertyChanged;

        _tableSource = new DynamicTableViewSource<FlowViewModel, FlowCacheKey>(
            _flowCache,
            FlowCacheKey.From,
            CreateColumns(),
            options: new DynamicTableViewSourceOptions
            {
                SelectionIdentityMode = DynamicTableViewSelectionIdentityMode.Key
            });
        _tableSource.SetSort([new DynamicTableViewSortDescriptor("Timestamp", ListSortDirection.Descending)]);
        _tableSource.SearchText = SearchQuery;
        SubscribeToSelectedNamespaces();
        ApplyFilter();

        Title = L10n.Instance.FlowListView_Title.CurrentValue()!;
    }

    public void Initialize(ClusterWorkspace cluster)
        => _ = InitializeAsync(cluster, _lifetime.Token);

    /// <summary>Starts Hubble flow collection for the specified cluster.</summary>
    /// <param name="cluster">Cluster workspace to read flows from.</param>
    /// <param name="ct">Cancellation token for initialization.</param>
    /// <returns>A task that completes when the stream is ready or initialization fails.</returns>
    public Task InitializeAsync(ClusterWorkspace cluster, CancellationToken ct = default)
    {
        lock (_lifecycleGate)
        {
            if (_disposeState != 0)
            {
                return Task.CompletedTask;
            }

            return _initializationTask ??= InitializeCoreAsync(cluster, ct);
        }
    }

    private async Task InitializeCoreAsync(ClusterWorkspace cluster, CancellationToken ct)
    {
        using var linkedCancellation = CancellationTokenSource.CreateLinkedTokenSource(ct, _lifetime.Token);
        Cluster = cluster;
        Id = $"FlowList-{cluster.Runtime.Name}";
        try
        {
            ConnectionStatus = L10n.Instance.FlowListView_StatusConnecting.CurrentValue()!;
            if (!await cluster.Runtime.IsCiliumInstalledAsync(linkedCancellation.Token).ConfigureAwait(true))
            {
                ConnectionStatus = L10n.Instance.FlowListView_StatusCiliumNotDetected.CurrentValue()!;
                return;
            }

            _connection = _hubbleClientFactory.CreateConnection(cluster.Runtime);
            _connection.StatusChanged += OnConnectionStatusChanged;
            ApplyFilter();
            await _connection.StartAsync(linkedCancellation.Token).ConfigureAwait(true);
            _flowSubscription = _connection.Flows
                .Buffer(TimeSpan.FromMilliseconds(50), 512, Scheduler.Default)
                .Subscribe(OnFlowsReceived, OnFlowStreamError);
        }
        catch (OperationCanceledException) when (linkedCancellation.IsCancellationRequested)
        {
            ConnectionStatus = L10n.Instance.FlowListView_StatusDisconnected.CurrentValue()!;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to initialize Hubble flow list");
            ConnectionStatus = $"{L10n.Instance.FlowListView_StatusError.CurrentValue()!}: {ex.Message}";
            IsConnected = false;
        }
    }

    internal static DynamicTableViewColumn<FlowViewModel>[] CreateColumns()
    {
        DynamicTableViewColumn<FlowViewModel>[] columns =
        [
            new DynamicTableViewColumn<FlowViewModel>(
                "Timestamp",
                L10n.Instance.FlowListView_ColumnTimestamp.CurrentValue()!,
                typeof(DateTime),
                flow => flow.Timestamp,
                flow => flow.Timestamp.ToString("HH:mm:ss.fff")),
            new DynamicTableViewColumn<FlowViewModel>(
                "Verdict",
                L10n.Instance.FlowListView_ColumnVerdict.CurrentValue()!,
                typeof(FlowVerdict),
                flow => flow.Verdict,
                flow => FormatVerdict(flow.Verdict)),
            new DynamicTableViewColumn<FlowViewModel>(
                "Direction",
                L10n.Instance.FlowListView_ColumnDirection.CurrentValue()!,
                typeof(KubeUI.Features.Cilium.Hubble.FlowDirection),
                flow => flow.Direction,
                flow => FormatDirection(flow.Direction)),
            new DynamicTableViewColumn<FlowViewModel>(
                "Source",
                L10n.Instance.FlowListView_ColumnSource.CurrentValue()!,
                typeof(string),
                flow => flow.SourcePod,
                null),
            new DynamicTableViewColumn<FlowViewModel>(
                "Destination",
                L10n.Instance.FlowListView_ColumnDestination.CurrentValue()!,
                typeof(string),
                flow => flow.DestinationPod,
                null),
            new DynamicTableViewColumn<FlowViewModel>(
                "Protocol",
                L10n.Instance.FlowListView_ColumnProtocol.CurrentValue()!,
                typeof(string),
                flow => flow.Protocol,
                null),
            new DynamicTableViewColumn<FlowViewModel>(
                "Port",
                L10n.Instance.FlowListView_ColumnPort.CurrentValue()!,
                typeof(int),
                flow => flow.DestinationPort,
                null),
            new DynamicTableViewColumn<FlowViewModel>(
                "Latency",
                L10n.Instance.FlowListView_ColumnLatency.CurrentValue()!,
                typeof(string),
                flow => flow.LatencyHumanReadable,
                null),
            new DynamicTableViewColumn<FlowViewModel>(
                "PolicyName",
                L10n.Instance.FlowListView_ColumnPolicy.CurrentValue()!,
                typeof(string),
                flow => flow.MatchedPolicyName ?? L10n.Instance.FlowListView_NoPolicy.CurrentValue()!,
                null,
                new FuncDataTemplate<FlowViewModel>((flow, _) => flow is null
                    ? new TextBlock()
                    : new TextBlock
                    {
                        Text = flow.MatchedPolicyName ?? L10n.Instance.FlowListView_NoPolicy.CurrentValue()!,
                        Foreground = flow.Verdict == FlowVerdict.Dropped
                            ? ApplicationBrushResources.GetBrush("ContainerStatusErrorBrush")
                            : ApplicationBrushResources.GetBrush("SubtleOutlineBrush")
                    }))
        ];

        var sourceColumn = columns.Single(static column => column.Key == "Source");
        sourceColumn.WidthMode = DynamicTableViewWidthMode.Star;
        sourceColumn.Width = 1.0;
        var destinationColumn = columns.Single(static column => column.Key == "Destination");
        destinationColumn.WidthMode = DynamicTableViewWidthMode.Star;
        destinationColumn.Width = 1.0;
        columns.Single(static column => column.Key == "Protocol").WidthMode = DynamicTableViewWidthMode.Auto;
        columns.Single(static column => column.Key == "Latency").WidthMode = DynamicTableViewWidthMode.Auto;
        columns.Single(static column => column.Key == "PolicyName").WidthMode = DynamicTableViewWidthMode.Auto;

        return columns;
    }

    internal static string FormatDirection(FlowDirection direction)
        => direction switch
        {
            KubeUI.Features.Cilium.Hubble.FlowDirection.Ingress => L10n.Instance.FlowDirection_Ingress.CurrentValue()!,
            KubeUI.Features.Cilium.Hubble.FlowDirection.Egress => L10n.Instance.FlowDirection_Egress.CurrentValue()!,
            _ => L10n.Instance.FlowDirection_Unknown.CurrentValue()!
        };

    internal static string FormatVerdict(FlowVerdict verdict)
        => verdict switch
        {
            FlowVerdict.Forwarded => L10n.Instance.FlowVerdict_Allowed.CurrentValue()!,
            FlowVerdict.Dropped => L10n.Instance.FlowVerdict_Dropped.CurrentValue()!,
            FlowVerdict.Error => L10n.Instance.FlowVerdict_Error.CurrentValue()!,
            _ => L10n.Instance.FlowVerdict_Unknown.CurrentValue()!
        };

    internal void OnFlowsReceived(IList<FlowViewModel> flows)
    {
        if (flows.Count == 0 || Volatile.Read(ref _disposeState) != 0 || Volatile.Read(ref _pausedState) != 0)
        {
            return;
        }

        lock (_flowUpdateGate)
        {
            if (Volatile.Read(ref _disposeState) != 0 || Volatile.Read(ref _pausedState) != 0)
            {
                return;
            }

            var matchesCurrentScope = _matchesCurrentScope;
            _flowCache.Edit(updater =>
            {
                foreach (var flow in flows)
                {
                    var key = FlowCacheKey.From(flow);
                    if (_flowOrderByKey.TryGetValue(key, out var previousOrder))
                    {
                        _flowOrder.Remove(previousOrder);
                        if (!previousOrder.MatchesCurrentScope)
                        {
                            _nonMatchingFlowOrder.Remove(previousOrder);
                        }
                    }

                    var matches = matchesCurrentScope(flow);
                    var order = new FlowOrderEntry(flow.Timestamp, ++_flowOrderSequence, key, matches);
                    _flowOrderByKey[key] = order;
                    _flowOrder.Add(order);
                    if (!matches)
                    {
                        _nonMatchingFlowOrder.Add(order);
                    }

                    updater.AddOrUpdate(flow);
                }

                while (_flowOrderByKey.Count > MaxFlows && _flowOrder.Count > 0)
                {
                    var oldest = _nonMatchingFlowOrder.Count > 0
                        ? _nonMatchingFlowOrder.Min
                        : _flowOrder.Min;
                    _flowOrder.Remove(oldest);
                    _nonMatchingFlowOrder.Remove(oldest);
                    _flowOrderByKey.Remove(oldest.Key);
                    updater.RemoveKey(oldest.Key);
                }
            });
        }

        UpdateFlowCount();
    }

    partial void OnIsPausedChanged(bool value)
        => Volatile.Write(ref _pausedState, value ? 1 : 0);

    private void UpdateFlowCount()
    {
        void Update()
        {
            if (Volatile.Read(ref _disposeState) == 0)
            {
                lock (_flowUpdateGate)
                {
                    FlowCount = _flowCache.Count;
                }
            }
        }

        if (Dispatcher.UIThread.CheckAccess())
        {
            Update();
        }
        else
        {
            Dispatcher.UIThread.Post(Update);
        }
    }

    private readonly record struct FlowOrderEntry(
        DateTime Timestamp,
        long Sequence,
        FlowCacheKey Key,
        bool MatchesCurrentScope);

    private sealed class FlowOrderEntryComparer : IComparer<FlowOrderEntry>
    {
        public static FlowOrderEntryComparer Instance { get; } = new();

        public int Compare(FlowOrderEntry left, FlowOrderEntry right)
        {
            var timestampComparison = left.Timestamp.CompareTo(right.Timestamp);
            return timestampComparison != 0
                ? timestampComparison
                : left.Sequence.CompareTo(right.Sequence);
        }
    }

    private void OnConnectionStatusChanged(HubbleConnectionStatus status)
    {
        void UpdateStatus()
        {
            if (Volatile.Read(ref _disposeState) != 0)
            {
                return;
            }

            ConnectionStatus = status switch
            {
                HubbleConnectionStatus.Disconnected => L10n.Instance.FlowListView_StatusDisconnected.CurrentValue()!,
                HubbleConnectionStatus.Connecting => L10n.Instance.FlowListView_StatusConnecting.CurrentValue()!,
                HubbleConnectionStatus.Connected => L10n.Instance.FlowListView_StatusConnected.CurrentValue()!,
                HubbleConnectionStatus.Error => L10n.Instance.FlowListView_StatusError.CurrentValue()!,
                _ => status.ToString()
            };
            IsConnected = status == HubbleConnectionStatus.Connected;
        }

        if (Dispatcher.UIThread.CheckAccess())
        {
            UpdateStatus();
        }
        else
        {
            Dispatcher.UIThread.Post(UpdateStatus);
        }
    }

    partial void OnSearchQueryChanged(string value)
    {
        if (_tableSource != null)
            _tableSource.SearchText = value;
    }

    private void OnFilterPropertyChanged(object? sender, PropertyChangedEventArgs e)
        => ApplyFilter();

    private void OnSelectedNamespacesChanged(object? sender, NotifyCollectionChangedEventArgs e)
        => ApplyFilter();

    partial void OnClusterChanged(ClusterWorkspace? value)
    {
        UnsubscribeFromSelectedNamespaces();
        OnPropertyChanged(nameof(SelectedNamespaces));
        SubscribeToSelectedNamespaces();
        ApplyFilter();
    }

    partial void OnIsNamespaceSelectionLinkedChanged(bool value)
    {
        if (Cluster is null)
        {
            OnPropertyChanged(nameof(SelectedNamespaces));
            return;
        }

        if (!value)
        {
            CopyNamespaces(Cluster.SelectedNamespaces, _localSelectedNamespaces);
        }

        SubscribeToSelectedNamespaces();
        OnPropertyChanged(nameof(SelectedNamespaces));
        ApplyFilter();
    }

    private void SubscribeToSelectedNamespaces()
    {
        UnsubscribeFromSelectedNamespaces();
        _subscribedNamespaces = SelectedNamespaces;
        _subscribedNamespaces.CollectionChanged += OnSelectedNamespacesChanged;
    }

    private void UnsubscribeFromSelectedNamespaces()
    {
        if (_subscribedNamespaces is not null)
        {
            _subscribedNamespaces.CollectionChanged -= OnSelectedNamespacesChanged;
            _subscribedNamespaces = null;
        }

        _localSelectedNamespaces.CollectionChanged -= OnSelectedNamespacesChanged;
    }

    private static void CopyNamespaces(IEnumerable<V1Namespace> source, ObservableCollection<V1Namespace> target)
    {
        target.Clear();
        foreach (var selectedNamespace in source)
        {
            target.Add(selectedNamespace);
        }
    }

    private void ApplyFilter()
    {
        if (_tableSource is null)
        {
            return;
        }

        var criteria = Filter.Capture();
        var selectedNamespaces = SelectedNamespaces
            .Select(static item => item.Name())
            .Where(static name => !string.IsNullOrWhiteSpace(name))
            .ToHashSet(StringComparer.Ordinal);

        bool MatchesCurrentScope(FlowViewModel flow)
        {
            if (!criteria.Matches(flow))
            {
                return false;
            }

            return selectedNamespaces.Count == 0
                || selectedNamespaces.Contains(flow.SourceNamespace)
                || selectedNamespaces.Contains(flow.DestinationNamespace);
        }

        lock (_flowUpdateGate)
        {
            foreach (var pair in _flowOrderByKey.ToArray())
            {
                var previousOrder = pair.Value;
                var cachedFlow = _flowCache.Lookup(pair.Key);
                if (!cachedFlow.HasValue)
                {
                    continue;
                }

                var flow = cachedFlow.Value;
                var matches = MatchesCurrentScope(flow);
                if (matches == previousOrder.MatchesCurrentScope)
                {
                    continue;
                }

                _flowOrder.Remove(previousOrder);
                _nonMatchingFlowOrder.Remove(previousOrder);
                var updatedOrder = previousOrder with { MatchesCurrentScope = matches };
                _flowOrderByKey[pair.Key] = updatedOrder;
                _flowOrder.Add(updatedOrder);
                if (!matches)
                {
                    _nonMatchingFlowOrder.Add(updatedOrder);
                }
            }

            _matchesCurrentScope = MatchesCurrentScope;
            _tableSource.SetScopeFilter(FilterScopeKey, item =>
                item is FlowViewModel flow && MatchesCurrentScope(flow));
        }
    }

    private void OnFlowStreamError(Exception exception)
    {
        _logger.LogError(exception, "Flow stream error");
        OnConnectionStatusChanged(HubbleConnectionStatus.Error);
    }

    public void CaptureSelectionState()
    {
        if (_tableSource is null)
            return;

        var selectedItems = SelectionModel.SelectedItems;
        var selectedKeys = new FlowCacheKey[selectedItems.Count];
        for (var index = 0; index < selectedItems.Count; index++)
            selectedKeys[index] = FlowCacheKey.From((FlowViewModel)selectedItems[index]!);
        _selectionRuntimeState = selectedKeys;
    }

    public void RestoreSelectionState()
    {
        if (_tableSource is null || _selectionRuntimeState is not { } savedKeys)
            return;

        var selectedKeys = new HashSet<FlowCacheKey>(savedKeys);
        var selection = _tableSource.SelectionModel;
        var desiredIndexes = new List<int>();
        var index = 0;
        foreach (var item in _tableSource.Items)
        {
            if (item is FlowViewModel resource && selectedKeys.Contains(FlowCacheKey.From(resource)))
                desiredIndexes.Add(index);
            index++;
        }

        if (selection.SelectedIndexes.Count == desiredIndexes.Count &&
            selection.SelectedIndexes.SequenceEqual(desiredIndexes))
            return;

        using (selection.BatchUpdate())
        {
            selection.Clear();
            foreach (var selectedIndex in desiredIndexes)
                selection.Select(selectedIndex);
        }
    }

    public void Dispose()
        => _ = DisposeAsync();

    /// <summary>Stops flow collection and releases the connection and table resources.</summary>
    public async ValueTask DisposeAsync()
    {
        Task? initializationTask;
        lock (_lifecycleGate)
        {
            if (Interlocked.Exchange(ref _disposeState, 1) != 0)
            {
                return;
            }

            _lifetime.Cancel();
            initializationTask = _initializationTask;
        }

        if (initializationTask is not null)
        {
            await initializationTask.ConfigureAwait(false);
        }

        _flowSubscription?.Dispose();
        Filter.PropertyChanged -= OnFilterPropertyChanged;
        UnsubscribeFromSelectedNamespaces();

        if (_connection is not null)
        {
            _connection.StatusChanged -= OnConnectionStatusChanged;
            await _connection.DisposeAsync().ConfigureAwait(false);
        }

        lock (_flowUpdateGate)
        {
            _tableSource?.Dispose();
            _flowCache.Dispose();
        }
        _lifetime.Dispose();
    }
}
