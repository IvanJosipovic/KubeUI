using System;
using System.ComponentModel;
using System.Collections.Specialized;
using System.Reactive.Linq;
using Avalonia.Controls.Selection;
using Avalonia.Controls.Templates;
using Avalonia.Markup.Declarative;
using Avalonia.Media;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using DynamicData;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Features.Resources.Common;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Avalonia.Infrastructure.Threading;
using KubeUI.Avalonia.Resources;
using KubeUI.Cilium;
using KubeUI.Cilium.Hubble;
using SvcSystems.Avalonia.DynamicTableView;
using Controls = KubeUI.Avalonia.Features.Cilium.Hubble.Controls;
using k8s.Models;
using KubeUI.Kubernetes;
using Microsoft.Extensions.Logging;
using HubbleConnectionStatus = KubeUI.Cilium.Hubble.ConnectionStatus;


namespace KubeUI.Avalonia.Features.Cilium.Hubble;

public sealed partial class FlowListViewModel : ViewModelBase, IInitializeCluster, IDisposable, IAsyncDisposable
{
    private const string FilterScopeKey = "hubble-flow-scope";
    private readonly ILogger<FlowListViewModel> _logger;
    private readonly IHubbleConnectionFactory _hubbleClientFactory;
    private readonly object _lifecycleGate = new();
    private IHubbleConnection? _connection;
    private IDisposable? _flowSubscription;
    private CancellationTokenSource _lifetime = new();
    private int _disposeState;
    private Task? _initializationTask;
    private readonly SourceCache<FlowViewModel, FlowCacheKey> _flowCache = new(FlowCacheKey.From);
    private DynamicTableViewSource<FlowViewModel, FlowCacheKey>? _tableSource;
    private readonly ObservableCollection<V1Namespace> _localSelectedNamespaces = [];
    private FlowCacheKey[]? _selectionRuntimeState;

    [ObservableProperty] public partial ClusterWorkspace? Cluster { get; set; }
    [ObservableProperty] public partial bool IsConnected { get; set; }
    [ObservableProperty] public partial string ConnectionStatus { get; set; } = string.Empty;
    [ObservableProperty] public partial string SearchQuery { get; set; } = string.Empty;
    [ObservableProperty] public partial int FlowCount { get; set; }
    [ObservableProperty] public partial bool IsPaused { get; set; }
    [ObservableProperty] public partial int MaxFlows { get; set; } = 10000;
    public FlowFilterViewModel Filter { get; } = new();

    [ObservableProperty] public partial DynamicTableViewState? TableViewRuntimeState { get; set; }

    public IDynamicTableViewSource TableSource => _tableSource ?? throw new InvalidOperationException("Not initialized");
    public ISelectionModel SelectionModel => TableSource.SelectionModel;
    public FlowViewModel? SelectedItem => SelectionModel.SelectedItem as FlowViewModel;

    public ObservableCollection<V1Namespace> SelectedNamespaces
        => Cluster != null ? Cluster.SelectedNamespaces : _localSelectedNamespaces;

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
            _flowCache.Clear();
            FlowCount = 0;
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
        _tableSource.SearchText = SearchQuery;
        ApplyFilter();

        Title = Assets.Resources.FlowListView_Title;
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
        cluster.SelectedNamespaces.CollectionChanged += OnSelectedNamespacesChanged;
        try
        {
            ConnectionStatus = Assets.Resources.FlowListView_StatusConnecting;
            if (!await cluster.Runtime.IsCiliumInstalledAsync(linkedCancellation.Token).ConfigureAwait(true))
            {
                ConnectionStatus = Assets.Resources.FlowListView_StatusCiliumNotDetected;
                return;
            }

            _connection = _hubbleClientFactory.CreateConnection(cluster.Runtime);
            _connection.StatusChanged += OnConnectionStatusChanged;
            ApplyFilter();
            await _connection.StartAsync(linkedCancellation.Token).ConfigureAwait(true);
            _flowSubscription = _connection.Flows
                .ObserveOn(AvaloniaScheduler.Instance)
                .Subscribe(OnFlowReceived, OnFlowStreamError);
        }
        catch (OperationCanceledException) when (linkedCancellation.IsCancellationRequested)
        {
            ConnectionStatus = Assets.Resources.FlowListView_StatusDisconnected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to initialize Hubble flow list");
            ConnectionStatus = $"{Assets.Resources.FlowListView_StatusError}: {ex.Message}";
            IsConnected = false;
        }
    }

    private static DynamicTableViewColumn<FlowViewModel>[] CreateColumns()
    {
        return
        [
            new DynamicTableViewColumn<FlowViewModel>(
                "Timestamp",
                Assets.Resources.FlowListView_ColumnTimestamp,
                typeof(string),
                flow => flow.Timestamp.ToString("HH:mm:ss.fff"),
                null),
            new DynamicTableViewColumn<FlowViewModel>(
                "Verdict",
                Assets.Resources.FlowListView_ColumnVerdict,
                typeof(FlowVerdict),
                flow => flow.Verdict,
                null,
                new FuncDataTemplate<FlowViewModel>((flow, _) => new Controls.VerdictBadge { Verdict = flow.Verdict })),
            new DynamicTableViewColumn<FlowViewModel>(
                "Direction",
                Assets.Resources.FlowListView_ColumnDirection,
                typeof(KubeUI.Cilium.Hubble.FlowDirection),
                flow => flow.Direction,
                null,
                new FuncDataTemplate<FlowViewModel>((flow, _) => new Controls.DirectionIcon { Direction = flow.Direction })),
            new DynamicTableViewColumn<FlowViewModel>(
                "Source",
                Assets.Resources.FlowListView_ColumnSource,
                typeof(FlowEndpoint),
                flow => flow.Source,
                null,
                new FuncDataTemplate<FlowViewModel>((flow, _) => new Controls.FlowEndpointView { Endpoint = flow.Source, IsSource = true })),
            new DynamicTableViewColumn<FlowViewModel>(
                "Destination",
                Assets.Resources.FlowListView_ColumnDestination,
                typeof(FlowEndpoint),
                flow => flow.Destination,
                null,
                new FuncDataTemplate<FlowViewModel>((flow, _) => new Controls.FlowEndpointView { Endpoint = flow.Destination, IsSource = false })),
            new DynamicTableViewColumn<FlowViewModel>(
                "Protocol",
                Assets.Resources.FlowListView_ColumnProtocol,
                typeof(string),
                flow => flow.Protocol,
                null),
            new DynamicTableViewColumn<FlowViewModel>(
                "Port",
                Assets.Resources.FlowListView_ColumnPort,
                typeof(int),
                flow => flow.DestinationPort,
                null),
            new DynamicTableViewColumn<FlowViewModel>(
                "Latency",
                Assets.Resources.FlowListView_ColumnLatency,
                typeof(string),
                flow => flow.LatencyHumanReadable,
                null),
            new DynamicTableViewColumn<FlowViewModel>(
                "PolicyName",
                Assets.Resources.FlowListView_ColumnPolicy,
                typeof(string),
                flow => flow.MatchedPolicyName ?? Assets.Resources.FlowListView_NoPolicy,
                null,
                new FuncDataTemplate<FlowViewModel>((flow, _) => new TextBlock
                {
                    Text = flow.MatchedPolicyName ?? Assets.Resources.FlowListView_NoPolicy,
                    Foreground = flow.Verdict == FlowVerdict.Dropped
                        ? Application.Current?.TryGetResource("ContainerStatusErrorBrush", Application.Current.ActualThemeVariant, out var r) == true && r is IBrush b ? b : Brushes.Red
                        : Application.Current?.TryGetResource("SystemBaseMediumBrush", Application.Current.ActualThemeVariant, out var r2) == true && r2 is IBrush b2 ? b2 : Brushes.Gray
                }))
        ];
    }

    private void OnFlowReceived(FlowViewModel flow)
    {
        if (Volatile.Read(ref _disposeState) != 0 || IsPaused) return;

        _flowCache.AddOrUpdate(flow);

        if (_flowCache.Count > MaxFlows)
        {
            FlowViewModel? oldest = null;
            foreach (var cachedFlow in _flowCache.Items)
            {
                if (oldest is null || cachedFlow.Timestamp < oldest.Timestamp)
                {
                    oldest = cachedFlow;
                }
            }

            if (oldest is null)
            {
                return;
            }

            _flowCache.Remove(oldest);
        }

        FlowCount = _flowCache.Count;
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
                HubbleConnectionStatus.Disconnected => Assets.Resources.FlowListView_StatusDisconnected,
                HubbleConnectionStatus.Connecting => Assets.Resources.FlowListView_StatusConnecting,
                HubbleConnectionStatus.Connected => Assets.Resources.FlowListView_StatusConnected,
                HubbleConnectionStatus.Error => Assets.Resources.FlowListView_StatusError,
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
        if (_tableSource != null) _tableSource.SearchText = value;
    }

    private void OnFilterPropertyChanged(object? sender, PropertyChangedEventArgs e)
        => ApplyFilter();

    private void OnSelectedNamespacesChanged(object? sender, NotifyCollectionChangedEventArgs e)
        => ApplyFilter();

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
        _tableSource.SetScopeFilter(FilterScopeKey, item =>
        {
            if (item is not FlowViewModel flow || !criteria.Matches(flow))
            {
                return false;
            }

            return selectedNamespaces.Count == 0
                || selectedNamespaces.Contains(flow.SourceNamespace)
                || selectedNamespaces.Contains(flow.DestinationNamespace);
        });
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
        if (Cluster is not null)
        {
            Cluster.SelectedNamespaces.CollectionChanged -= OnSelectedNamespacesChanged;
        }

        if (_connection is not null)
        {
            _connection.StatusChanged -= OnConnectionStatusChanged;
            await _connection.DisposeAsync().ConfigureAwait(false);
        }

        _tableSource?.Dispose();
        _flowCache.Dispose();
        _lifetime.Dispose();
    }
}
