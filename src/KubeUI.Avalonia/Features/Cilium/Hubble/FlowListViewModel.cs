using System;
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
using Controls = KubeUI.Avalonia.Features.Cilium.Hubble.Controls;
using k8s.Models;
using KubeUI.Kubernetes;
using Microsoft.Extensions.Logging;


namespace KubeUI.Avalonia.Features.Cilium.Hubble;

public sealed partial class FlowListViewModel : ViewModelBase, IInitializeCluster, IDisposable
{
    private readonly ILogger<FlowListViewModel> _logger;
    private readonly HubbleClientFactory _hubbleClientFactory;
    private HubbleConnection? _connection;
    private IDisposable? _flowSubscription;
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
    [ObservableProperty] public partial FlowFilterViewModel Filter { get; set; } = new();

    [ObservableProperty] public partial DynamicTableViewState? TableViewRuntimeState { get; set; }

    public IDynamicTableViewSource TableSource => _tableSource ?? throw new InvalidOperationException("Not initialized");
    public ISelectionModel SelectionModel => TableSource.SelectionModel;
    public FlowViewModel? SelectedItem => SelectionModel.SelectedItem as FlowViewModel;

    public ObservableCollection<V1Namespace> SelectedNamespaces
        => Cluster != null ? Cluster.SelectedNamespaces : _localSelectedNamespaces;

    public IRelayCommand ClearCommand { get; }
    public IRelayCommand<FlowViewModel[]> ViewFlowCommand { get; }
    public IRelayCommand ResetFilterCommand { get; }

    public IEnumerable<MenuItemViewModel> GetContextMenuItems(IEnumerable? _)
    {
        return [];
    }

    public FlowListViewModel(
        ILogger<FlowListViewModel> logger,
        HubbleClientFactory hubbleClientFactory)
    {
        _logger = logger;
        _hubbleClientFactory = hubbleClientFactory;

        ClearCommand = new RelayCommand(() => _flowCache.Clear());
        ViewFlowCommand = new RelayCommand<FlowViewModel[]>(flows =>
        {
            // Open flow detail view - TODO
        });
        ResetFilterCommand = new RelayCommand(() => Filter.Reset());

        Title = Assets.Resources.FlowListView_Title;
    }

    public void Initialize(ClusterWorkspace cluster)
    {
        _ = InitializeAsync(cluster, CancellationToken.None);
    }

    public async Task InitializeAsync(ClusterWorkspace cluster, CancellationToken ct = default)
    {
        Cluster = cluster;
        Id = $"FlowList-{cluster.Runtime.Name}";

        if (!await cluster.Runtime.IsCiliumInstalledAsync(ct))
        {
            ConnectionStatus = Assets.Resources.FlowListView_StatusCiliumNotDetected;
            return;
        }

        _connection = _hubbleClientFactory.CreateConnection(cluster.Runtime);
        _connection.StatusChanged += OnConnectionStatusChanged;
        await _connection.StartAsync(ct);

        var columns = CreateColumns();

        _tableSource = new DynamicTableViewSource<FlowViewModel, FlowCacheKey>(
            _flowCache,
            FlowCacheKey.From,
            columns,
            options: new DynamicTableViewSourceOptions
            {
                SelectionIdentityMode = DynamicTableViewSelectionIdentityMode.Key
            });
        _tableSource.SearchText = SearchQuery;

        _flowSubscription = _connection.Flows
            .Where(f => Filter.Matches(f))
            .ObserveOn(AvaloniaScheduler.Instance)
            .Subscribe(OnFlowReceived, ex => _logger.LogError(ex, "Flow stream error"));

        IsConnected = true;
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
                "Bytes",
                Assets.Resources.FlowListView_ColumnBytes,
                typeof(string),
                flow => flow.BytesHumanReadable,
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
        if (IsPaused) return;

        _flowCache.AddOrUpdate(flow);

        if (_flowCache.Count > MaxFlows)
        {
            var oldest = _flowCache.Items.OrderBy(f => f.Timestamp).First();
            _flowCache.Remove(oldest);
        }

        FlowCount = _flowCache.Count;
    }

    private void OnConnectionStatusChanged(KubeUI.Cilium.Hubble.ConnectionStatus status)
    {
        ConnectionStatus = status switch
        {
            KubeUI.Cilium.Hubble.ConnectionStatus.Disconnected => Assets.Resources.FlowListView_StatusDisconnected,
            KubeUI.Cilium.Hubble.ConnectionStatus.Connecting => Assets.Resources.FlowListView_StatusConnecting,
            KubeUI.Cilium.Hubble.ConnectionStatus.Connected => Assets.Resources.FlowListView_StatusConnected,
            KubeUI.Cilium.Hubble.ConnectionStatus.Error => Assets.Resources.FlowListView_StatusError,
            _ => status.ToString()
        };
        IsConnected = status == KubeUI.Cilium.Hubble.ConnectionStatus.Connected;
    }

    partial void OnSearchQueryChanged(string value)
    {
        if (_tableSource != null) _tableSource.SearchText = value;
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
    {
        _flowSubscription?.Dispose();
        _ = _connection?.DisposeAsync();
        _tableSource?.Dispose();
        _flowCache.Dispose();
    }
}