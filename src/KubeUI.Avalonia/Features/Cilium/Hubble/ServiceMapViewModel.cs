using System.Collections.Specialized;
using System.Reactive.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using k8s.Models;
using KubeUI.Avalonia.Features.Cilium.Hubble.Controls;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Avalonia.Infrastructure.Threading;
using KubeUI.Cilium;
using KubeUI.Cilium.Hubble;
using KubeUI.Kubernetes;
using Microsoft.Extensions.Logging;
using Westermo.GraphX.Common.Enums;
using HubbleConnectionStatus = KubeUI.Cilium.Hubble.ConnectionStatus;

namespace KubeUI.Avalonia.Features.Cilium.Hubble;

public sealed partial class ServiceMapViewModel : ViewModelBase, IInitializeCluster, IDisposable, IAsyncDisposable
{
    private readonly ILogger<ServiceMapViewModel> _logger;
    private readonly IHubbleConnectionFactory _hubbleClientFactory;
    private readonly object _lifecycleGate = new();
    private readonly ServiceMapGraphBuilder _graphBuilder = new();
    private readonly ObservableCollection<V1Namespace> _localSelectedNamespaces = [];
    private readonly ObservableCollection<FlowViewModel> _selectedNodeFlows = [];
    private CancellationTokenSource _lifetime = new();
    private IHubbleConnection? _connection;
    private IDisposable? _flowSubscription;
    private IDisposable? _refreshSubscription;
    private IDisposable? _namespaceScopeUpdateSubscription;
    private HashSet<string>? _activeNamespaceNames;
    private CancellationTokenSource? _graphBuildCancellation;
    private CancellationTokenSource? _selectedFlowsCancellation;
    private Task? _graphBuildTask;
    private Task? _selectedFlowsTask;
    private long _graphBuildVersion;
    private long _flowSubscriptionVersion;
    private long _selectedFlowsVersion;
    private int _disposeState;
    private Task? _initializationTask;

    [ObservableProperty] public partial ClusterWorkspace? Cluster { get; set; }
    [ObservableProperty] public partial bool IsConnected { get; set; }
    [ObservableProperty] public partial string ConnectionStatus { get; set; } = string.Empty;
    [ObservableProperty] public partial ServiceMapGraph? Graph { get; set; }
    [ObservableProperty] public partial TimeSpan TimeWindow { get; set; } = TimeSpan.FromMinutes(5);
    [ObservableProperty] public partial bool ShowOnlyErrors { get; set; }
    [ObservableProperty] public partial bool AggregateByNamespace { get; set; }
    /// <summary>Gets or sets the GraphX layout algorithm used by the service map.</summary>
    [ObservableProperty] public partial LayoutAlgorithmTypeEnum LayoutAlgorithm { get; set; } = LayoutAlgorithmTypeEnum.CompoundFDP;
    /// <summary>Gets or sets the GraphX edge-routing algorithm used by the service map.</summary>
    [ObservableProperty] public partial EdgeRoutingAlgorithmTypeEnum EdgeRoutingAlgorithm { get; set; } = EdgeRoutingAlgorithmTypeEnum.SimpleER;
    [ObservableProperty] public partial ServiceMapNode? SelectedNode { get; set; }
    [ObservableProperty] public partial bool IsNamespaceSelectionLinked { get; set; } = true;

    /// <summary>Gets the current namespace selection used to scope the graph.</summary>
    public ObservableCollection<V1Namespace> SelectedNamespaces
        => IsNamespaceSelectionLinked && Cluster is not null ? Cluster.SelectedNamespaces : _localSelectedNamespaces;

    /// <summary>Gets whether at least one namespace is selected for the service map.</summary>
    public bool HasSelectedNamespaces => SelectedNamespaces.Count > 0;

    /// <summary>Gets the empty-state message for the current namespace and flow selection.</summary>
    public string EmptyStateMessage => HasSelectedNamespaces
        ? Assets.Resources.ServiceMapView_NoFlows
        : Assets.Resources.ServiceMapView_SelectNamespacePrompt;

    /// <summary>Gets flows associated with the node selected for inspection.</summary>
    public ObservableCollection<FlowViewModel> SelectedNodeFlows => _selectedNodeFlows;

    /// <summary>Gets graph nodes for the selector.</summary>
    public IReadOnlyList<ServiceMapNode> Nodes => Graph?.Nodes ?? [];

    /// <summary>Gets the localized node and edge counts displayed above the graph.</summary>
    public string GraphSummary => string.Format(
        System.Globalization.CultureInfo.CurrentCulture,
        Assets.Resources.ServiceMapView_GraphCountsFormat,
        Graph?.NodeCount ?? 0,
        Graph?.EdgeCount ?? 0);

    /// <summary>Gets whether the current graph contains any nodes.</summary>
    public bool HasFlows => Graph?.NodeCount > 0;

    public IRelayCommand<ServiceMapNode> ShowFlowsForNodeCommand { get; }

    public ServiceMapViewModel(ILogger<ServiceMapViewModel> logger, IHubbleConnectionFactory hubbleClientFactory)
    {
        _logger = logger;
        _hubbleClientFactory = hubbleClientFactory;

        ShowFlowsForNodeCommand = new RelayCommand<ServiceMapNode>(ShowFlowsForNode);
        Title = Assets.Resources.ServiceMapView_Title;
        Graph = new ServiceMapGraph();
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
        Id = $"ServiceMap-{cluster.Runtime.Name}";
        ConnectionStatus = Assets.Resources.ServiceMapView_StatusConnecting;

        try
        {
            if (!await cluster.Runtime.IsCiliumInstalledAsync(linkedCancellation.Token).ConfigureAwait(true))
            {
                ConnectionStatus = Assets.Resources.ServiceMapView_StatusCiliumNotDetected;
                return;
            }

            _connection = _hubbleClientFactory.CreateConnection(cluster.Runtime);
            _connection.StatusChanged += OnConnectionStatusChanged;
            await _connection.StartAsync(linkedCancellation.Token).ConfigureAwait(true);
            UpdateNamespaceScope();
        }
        catch (OperationCanceledException) when (linkedCancellation.IsCancellationRequested)
        {
            ConnectionStatus = Assets.Resources.ServiceMapView_StatusDisconnected;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to initialize Hubble service map");
            ConnectionStatus = string.Format(Assets.Resources.ServiceMapView_StatusErrorFormat, ex.Message);
            IsConnected = false;
        }
    }

    partial void OnTimeWindowChanged(TimeSpan value)
    {
        RebuildGraph();
        LoadSelectedNodeFlows(SelectedNode);
    }

    partial void OnGraphChanged(ServiceMapGraph? value)
    {
        OnPropertyChanged(nameof(Nodes));
        OnPropertyChanged(nameof(HasFlows));
        OnPropertyChanged(nameof(GraphSummary));
        OnPropertyChanged(nameof(EmptyStateMessage));

        if (SelectedNode is { } selectedNode)
        {
            if (value is null || !value.Nodes.Any(node => node.Id == selectedNode.Id))
            {
                SelectedNode = null;
            }
        }
    }

    partial void OnShowOnlyErrorsChanged(bool value)
    {
        RebuildGraph();
        LoadSelectedNodeFlows(SelectedNode);
    }

    partial void OnAggregateByNamespaceChanged(bool value)
    {
        SelectedNode = null;
        SelectedNodeFlows.Clear();
        RebuildGraph();
    }

    private INotifyCollectionChanged? _subscribedNamespaces;

    partial void OnClusterChanged(ClusterWorkspace? value)
    {
        _namespaceScopeUpdateSubscription?.Dispose();
        _namespaceScopeUpdateSubscription = null;
        UnsubscribeFromSelectedNamespaces();
        OnPropertyChanged(nameof(SelectedNamespaces));
        SubscribeToSelectedNamespaces();
        UpdateNamespaceScope();
    }

    partial void OnIsNamespaceSelectionLinkedChanged(bool value)
    {
        if (Cluster is null)
        {
            OnPropertyChanged(nameof(SelectedNamespaces));
            SubscribeToSelectedNamespaces();
            UpdateNamespaceScope();
            return;
        }

        if (!value)
        {
            CopyNamespaces(Cluster.SelectedNamespaces, _localSelectedNamespaces);
        }

        UnsubscribeFromSelectedNamespaces();
        OnPropertyChanged(nameof(SelectedNamespaces));
        SubscribeToSelectedNamespaces();
        UpdateNamespaceScope();
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

    private void OnSelectedNamespacesChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (!HasSelectedNamespaces)
        {
            _namespaceScopeUpdateSubscription?.Dispose();
            _namespaceScopeUpdateSubscription = null;
            UpdateNamespaceScope();
            return;
        }

        _namespaceScopeUpdateSubscription?.Dispose();
        _namespaceScopeUpdateSubscription = Observable.Timer(TimeSpan.FromMilliseconds(150), AvaloniaScheduler.Instance)
            .Subscribe(_ =>
            {
                _namespaceScopeUpdateSubscription?.Dispose();
                _namespaceScopeUpdateSubscription = null;
                UpdateNamespaceScope();
            });
    }

    private static void CopyNamespaces(IEnumerable<V1Namespace> source, ObservableCollection<V1Namespace> target)
    {
        target.Clear();
        foreach (var selectedNamespace in source)
        {
            target.Add(selectedNamespace);
        }
    }

    private void OnFlowReceived(FlowViewModel flow, long subscriptionVersion)
    {
        if (Volatile.Read(ref _disposeState) != 0
            || subscriptionVersion != Volatile.Read(ref _flowSubscriptionVersion))
        {
            return;
        }

        _graphBuilder.AddFlow(flow);
    }

    private void UpdateNamespaceScope()
    {
        if (Volatile.Read(ref _disposeState) != 0)
        {
            return;
        }

        var selectedNamespaces = GetSelectedNamespaceNames();
        OnPropertyChanged(nameof(HasSelectedNamespaces));
        OnPropertyChanged(nameof(EmptyStateMessage));

        if (selectedNamespaces.Count == 0)
        {
            _activeNamespaceNames = selectedNamespaces;
            Interlocked.Increment(ref _flowSubscriptionVersion);
            _flowSubscription?.Dispose();
            _flowSubscription = null;
            _refreshSubscription?.Dispose();
            _refreshSubscription = null;
            CancelGraphBuild();
            CancelSelectedFlowsLoad();
            ConnectionStatus = Assets.Resources.ServiceMapView_StatusSelectNamespace;
            IsConnected = false;
            SelectedNode = null;
            SelectedNodeFlows.Clear();
            Graph = new ServiceMapGraph();
            return;
        }

        var sameScope = _activeNamespaceNames?.SetEquals(selectedNamespaces) == true;
        _activeNamespaceNames = selectedNamespaces;
        if (_connection is not null && (!sameScope || _flowSubscription is null))
        {
            Interlocked.Increment(ref _flowSubscriptionVersion);
            _flowSubscription?.Dispose();
            var subscriptionVersion = Volatile.Read(ref _flowSubscriptionVersion);
            ConnectionStatus = Assets.Resources.ServiceMapView_StatusConnecting;
            IsConnected = false;
            _flowSubscription = _connection.ObserveFlows(selectedNamespaces)
                .Subscribe(flow => OnFlowReceived(flow, subscriptionVersion), OnFlowStreamError);
        }

        _refreshSubscription ??= Observable.Interval(TimeSpan.FromSeconds(2), AvaloniaScheduler.Instance)
            .Subscribe(_ => RebuildGraph());
        RebuildGraph(selectedNamespaces);
        LoadSelectedNodeFlows(SelectedNode);
    }

    private HashSet<string> GetSelectedNamespaceNames()
    {
        var names = new HashSet<string>(StringComparer.Ordinal);
        foreach (var selectedNamespace in SelectedNamespaces)
        {
            var name = selectedNamespace.Name();
            if (!string.IsNullOrWhiteSpace(name))
            {
                names.Add(name);
            }
        }

        return names;
    }

    private void RebuildGraph(HashSet<string>? selectedNamespaces = null)
    {
        if (Volatile.Read(ref _disposeState) != 0)
        {
            return;
        }

        selectedNamespaces ??= GetSelectedNamespaceNames();
        if (selectedNamespaces.Count == 0)
        {
            Graph = new ServiceMapGraph();
            return;
        }

        var cancellation = new CancellationTokenSource();
        Cancel(Interlocked.Exchange(ref _graphBuildCancellation, cancellation));
        var version = Interlocked.Increment(ref _graphBuildVersion);
        var timeWindow = TimeWindow;
        var aggregateByNamespace = AggregateByNamespace;
        var showOnlyErrors = ShowOnlyErrors;
        _graphBuildTask = BuildGraphAsync(
            version,
            timeWindow,
            aggregateByNamespace,
            showOnlyErrors,
            selectedNamespaces,
            cancellation);
    }

    private async Task BuildGraphAsync(
        long version,
        TimeSpan timeWindow,
        bool aggregateByNamespace,
        bool showOnlyErrors,
        HashSet<string> selectedNamespaces,
        CancellationTokenSource cancellation)
    {
        try
        {
            var graph = await Task.Run(
                () => _graphBuilder.Build(timeWindow, aggregateByNamespace, showOnlyErrors, selectedNamespaces, cancellation.Token),
                cancellation.Token).ConfigureAwait(false);
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (Volatile.Read(ref _disposeState) == 0
                    && !cancellation.IsCancellationRequested
                    && version == Volatile.Read(ref _graphBuildVersion))
                {
                    Graph = graph;
                }
            });
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to build Hubble service map graph");
        }
        finally
        {
            Interlocked.CompareExchange(ref _graphBuildCancellation, null, cancellation);
            cancellation.Dispose();
        }
    }

    private void CancelGraphBuild()
    {
        Interlocked.Increment(ref _graphBuildVersion);
        Cancel(Interlocked.Exchange(ref _graphBuildCancellation, null));
    }

    private void ShowFlowsForNode(ServiceMapNode? node)
    {
        SelectedNode = node;
    }

    partial void OnSelectedNodeChanged(ServiceMapNode? value)
        => LoadSelectedNodeFlows(value);

    private void LoadSelectedNodeFlows(ServiceMapNode? node)
    {
        CancelSelectedFlowsLoad();
        _selectedNodeFlows.Clear();
        if (node is null || !HasSelectedNamespaces)
        {
            return;
        }

        var selectedNamespaces = GetSelectedNamespaceNames();
        var cancellation = new CancellationTokenSource();
        _selectedFlowsCancellation = cancellation;
        var version = Interlocked.Increment(ref _selectedFlowsVersion);
        var timeWindow = TimeWindow;
        var aggregateByNamespace = AggregateByNamespace;
        var showOnlyErrors = ShowOnlyErrors;
        _selectedFlowsTask = LoadSelectedNodeFlowsAsync(
            node,
            version,
            timeWindow,
            aggregateByNamespace,
            showOnlyErrors,
            selectedNamespaces,
            cancellation);
    }

    private async Task LoadSelectedNodeFlowsAsync(
        ServiceMapNode node,
        long version,
        TimeSpan timeWindow,
        bool aggregateByNamespace,
        bool showOnlyErrors,
        HashSet<string> selectedNamespaces,
        CancellationTokenSource cancellation)
    {
        try
        {
            var flows = await Task.Run(
                () => _graphBuilder.GetFlowsForNode(node, timeWindow, aggregateByNamespace, showOnlyErrors, selectedNamespaces, cancellation.Token),
                cancellation.Token).ConfigureAwait(false);
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                if (Volatile.Read(ref _disposeState) != 0
                    || cancellation.IsCancellationRequested
                    || version != Volatile.Read(ref _selectedFlowsVersion)
                    || SelectedNode?.Id != node.Id)
                {
                    return;
                }

                foreach (var flow in flows)
                {
                    _selectedNodeFlows.Add(flow);
                }
            });
        }
        catch (OperationCanceledException) when (cancellation.IsCancellationRequested)
        {
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unable to load Hubble flows for the selected service-map node");
        }
        finally
        {
            Interlocked.CompareExchange(ref _selectedFlowsCancellation, null, cancellation);
            cancellation.Dispose();
        }
    }

    private void CancelSelectedFlowsLoad()
    {
        Interlocked.Increment(ref _selectedFlowsVersion);
        Cancel(Interlocked.Exchange(ref _selectedFlowsCancellation, null));
    }

    private static void Cancel(CancellationTokenSource? cancellation)
    {
        if (cancellation is null)
        {
            return;
        }

        try
        {
            cancellation.Cancel();
        }
        catch (ObjectDisposedException)
        {
            // The worker completed and disposed its source while cancellation raced.
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

            if (!HasSelectedNamespaces)
            {
                ConnectionStatus = Assets.Resources.ServiceMapView_StatusSelectNamespace;
                IsConnected = false;
                return;
            }

            ConnectionStatus = status switch
            {
                HubbleConnectionStatus.Disconnected => Assets.Resources.ServiceMapView_StatusDisconnected,
                HubbleConnectionStatus.Connecting => Assets.Resources.ServiceMapView_StatusConnecting,
                HubbleConnectionStatus.Connected => Assets.Resources.ServiceMapView_StatusConnected,
                HubbleConnectionStatus.Error => Assets.Resources.ServiceMapView_StatusError,
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

    private void OnFlowStreamError(Exception exception)
    {
        _logger.LogError(exception, "Flow stream error");
        OnConnectionStatusChanged(HubbleConnectionStatus.Error);
    }

    public void Dispose()
        => _ = DisposeAsync();

    /// <summary>Stops graph refresh and releases the Hubble connection.</summary>
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

        CancelGraphBuild();
        CancelSelectedFlowsLoad();
        if (_graphBuildTask is { } graphBuildTask)
        {
            await graphBuildTask.ConfigureAwait(false);
        }

        if (_selectedFlowsTask is { } selectedFlowsTask)
        {
            await selectedFlowsTask.ConfigureAwait(false);
        }

        _refreshSubscription?.Dispose();
        _namespaceScopeUpdateSubscription?.Dispose();
        _flowSubscription?.Dispose();
        UnsubscribeFromSelectedNamespaces();

        if (_connection is not null)
        {
            _connection.StatusChanged -= OnConnectionStatusChanged;
            await _connection.DisposeAsync().ConfigureAwait(false);
        }

        _lifetime.Dispose();
    }
}
