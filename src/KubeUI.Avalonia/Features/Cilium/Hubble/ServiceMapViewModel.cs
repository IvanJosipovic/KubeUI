using System.Reactive.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Avalonia.Infrastructure.Threading;
using KubeUI.Cilium;
using KubeUI.Cilium.Hubble;
using KubeUI.Kubernetes;
using k8s.Models;
using Microsoft.Extensions.Logging;


namespace KubeUI.Avalonia.Features.Cilium.Hubble;

public sealed partial class ServiceMapViewModel : ViewModelBase, IInitializeCluster, IAsyncDisposable
{
    private readonly ILogger<ServiceMapViewModel> _logger;
    private readonly HubbleClientFactory _hubbleClientFactory;
    private HubbleConnection? _connection;
    private IDisposable? _flowSubscription;
    private readonly ServiceMapGraphBuilder _graphBuilder = new();
    private readonly ObservableCollection<V1Namespace> _localSelectedNamespaces = [];

    [ObservableProperty] public partial ClusterWorkspace? Cluster { get; set; }
    [ObservableProperty] public partial bool IsConnected { get; set; }
    [ObservableProperty] public partial ServiceMapGraph? Graph { get; set; }
    [ObservableProperty] public partial TimeSpan TimeWindow { get; set; } = TimeSpan.FromMinutes(5);
    [ObservableProperty] public partial bool ShowOnlyErrors { get; set; }
    [ObservableProperty] public partial bool AggregateByNamespace { get; set; } = false;

    public ObservableCollection<V1Namespace> SelectedNamespaces
        => Cluster != null ? Cluster.SelectedNamespaces : _localSelectedNamespaces;

    public IRelayCommand ResetLayoutCommand { get; }
    public IRelayCommand<ServiceMapNode> ShowFlowsForNodeCommand { get; }

    public ServiceMapViewModel(ILogger<ServiceMapViewModel> logger, HubbleClientFactory hubbleClientFactory)
    {
        _logger = logger;
        _hubbleClientFactory = hubbleClientFactory;

        ResetLayoutCommand = new RelayCommand(() => { /* reset graph layout */ });
        ShowFlowsForNodeCommand = new RelayCommand<ServiceMapNode>(node =>
        {
            // Navigate to FlowListView filtered by this node
        });

        Title = Assets.Resources.ServiceMapView_Title;
    }

    public void Initialize(ClusterWorkspace cluster)
    {
        _ = InitializeAsync(cluster, CancellationToken.None);
    }

    public async Task InitializeAsync(ClusterWorkspace cluster, CancellationToken ct = default)
    {
        Cluster = cluster;
        Id = $"ServiceMap-{cluster.Runtime.Name}";

        if (!await cluster.Runtime.IsCiliumInstalledAsync(ct))
            return;

        _connection = _hubbleClientFactory.CreateConnection(cluster.Runtime);
        await _connection.StartAsync(ct);

        _flowSubscription = _connection.Flows
            .Where(f => SelectedNamespaces.Count == 0 ||
                       SelectedNamespaces.Any(ns => ns.Name() == f.SourceNamespace || ns.Name() == f.DestinationNamespace))
            .ObserveOn(AvaloniaScheduler.Instance)
            .Subscribe(OnFlowReceived, ex => _logger.LogError(ex, "Flow stream error"));

        _ = Task.Run(async () =>
        {
            while (!ct.IsCancellationRequested)
            {
                await Task.Delay(TimeSpan.FromSeconds(2), ct);
                RebuildGraph();
            }
        }, ct);

        IsConnected = true;
    }

    private void OnFlowReceived(FlowViewModel flow)
    {
        if (ShowOnlyErrors && flow.Verdict != FlowVerdict.Dropped) return;
        _graphBuilder.AddFlow(flow, TimeWindow);
    }

    private void RebuildGraph()
    {
        var graph = _graphBuilder.Build(AggregateByNamespace);
        Dispatcher.UIThread.Post(() => Graph = graph);
    }

    public async ValueTask DisposeAsync()
    {
        _flowSubscription?.Dispose();
        if (_connection != null)
        {
            await _connection.DisposeAsync();
        }
    }
}