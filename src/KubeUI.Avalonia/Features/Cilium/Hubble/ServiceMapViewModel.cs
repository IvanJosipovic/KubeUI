using System.Reactive.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using k8s.Models;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Avalonia.Infrastructure.Threading;
using KubeUI.Cilium;
using KubeUI.Cilium.Hubble;
using KubeUI.Kubernetes;
using Microsoft.Extensions.Logging;
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
    private int _disposeState;
    private Task? _initializationTask;

    [ObservableProperty] public partial ClusterWorkspace? Cluster { get; set; }
    [ObservableProperty] public partial bool IsConnected { get; set; }
    [ObservableProperty] public partial string ConnectionStatus { get; set; } = string.Empty;
    [ObservableProperty] public partial ServiceMapGraph? Graph { get; set; }
    [ObservableProperty] public partial TimeSpan TimeWindow { get; set; } = TimeSpan.FromMinutes(5);
    [ObservableProperty] public partial bool ShowOnlyErrors { get; set; }
    [ObservableProperty] public partial bool AggregateByNamespace { get; set; }
    [ObservableProperty] public partial ServiceMapNode? SelectedNode { get; set; }

    /// <summary>Gets the current namespace selection used to scope the graph.</summary>
    public ObservableCollection<V1Namespace> SelectedNamespaces
        => Cluster != null ? Cluster.SelectedNamespaces : _localSelectedNamespaces;

    /// <summary>Gets flows associated with the node selected for inspection.</summary>
    public ObservableCollection<FlowViewModel> SelectedNodeFlows => _selectedNodeFlows;

    /// <summary>Gets graph nodes for the selector.</summary>
    public IReadOnlyList<ServiceMapNode> Nodes => Graph?.Nodes ?? [];

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
        cluster.SelectedNamespaces.CollectionChanged += OnSelectedNamespacesChanged;
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
            _flowSubscription = _connection.Flows
                .ObserveOn(AvaloniaScheduler.Instance)
                .Subscribe(OnFlowReceived, OnFlowStreamError);
            _refreshSubscription = Observable.Interval(TimeSpan.FromSeconds(2), AvaloniaScheduler.Instance)
                .Subscribe(_ => RebuildGraph());
            RebuildGraph();
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
        => RebuildGraph();

    partial void OnGraphChanged(ServiceMapGraph? value)
    {
        OnPropertyChanged(nameof(Nodes));
        OnPropertyChanged(nameof(HasFlows));
    }

    partial void OnShowOnlyErrorsChanged(bool value)
        => RebuildGraph();

    partial void OnAggregateByNamespaceChanged(bool value)
    {
        SelectedNode = null;
        SelectedNodeFlows.Clear();
        RebuildGraph();
    }

    private void OnSelectedNamespacesChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        RebuildGraph();
        if (SelectedNode is { } selectedNode)
        {
            ShowFlowsForNode(selectedNode);
        }
    }

    private void OnFlowReceived(FlowViewModel flow)
    {
        if (Volatile.Read(ref _disposeState) != 0)
        {
            return;
        }

        _graphBuilder.AddFlow(flow);
        RebuildGraph();
        if (SelectedNode is { } selectedNode)
        {
            ShowFlowsForNode(selectedNode);
        }
    }

    private void RebuildGraph()
    {
        if (Volatile.Read(ref _disposeState) != 0)
        {
            return;
        }

        var selectedNamespaces = GetSelectedNamespaceNames();
        Graph = _graphBuilder.Build(TimeWindow, AggregateByNamespace, ShowOnlyErrors, selectedNamespaces);
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

    private void ShowFlowsForNode(ServiceMapNode? node)
    {
        SelectedNode = node;
        _selectedNodeFlows.Clear();
        if (node is null)
        {
            return;
        }

        var selectedNamespaces = GetSelectedNamespaceNames();
        foreach (var flow in _graphBuilder.GetFlows(TimeWindow, ShowOnlyErrors, selectedNamespaces))
        {
            var belongsToNode = AggregateByNamespace
                ? node.Id == NamespaceKey(flow.SourceNamespace) || node.Id == NamespaceKey(flow.DestinationNamespace)
                : node.Id == PodKey(flow.SourceNamespace, flow.SourcePod, flow.SourceIp, flow.SourceIdentity)
                    || node.Id == PodKey(flow.DestinationNamespace, flow.DestinationPod, flow.DestinationIp, flow.DestinationIdentity);
            if (belongsToNode)
            {
                _selectedNodeFlows.Add(flow);
            }
        }
    }

    private static string PodKey(string ns, string pod, string ip, uint identity)
    {
        if (!string.IsNullOrEmpty(pod))
            return $"{ns}/{pod}";
        if (!string.IsNullOrEmpty(ip))
            return $"ip/{ip}";
        return identity == 0 ? "world" : $"identity/{identity}";
    }

    private static string NamespaceKey(string ns)
        => string.IsNullOrEmpty(ns) ? "world" : $"namespace/{ns}";

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

        _refreshSubscription?.Dispose();
        _flowSubscription?.Dispose();
        if (Cluster is not null)
        {
            Cluster.SelectedNamespaces.CollectionChanged -= OnSelectedNamespacesChanged;
        }

        if (_connection is not null)
        {
            _connection.StatusChanged -= OnConnectionStatusChanged;
            await _connection.DisposeAsync().ConfigureAwait(false);
        }

        _lifetime.Dispose();
    }
}
