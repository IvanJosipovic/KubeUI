using System.Collections.Specialized;
using System.ComponentModel;
using Avalonia.Threading;
using k8s;
using k8s.Models;
using KubeUI.Avalonia.Features.Clusters.Workspace.ViewModels;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Features.Clusters.Overview.ViewModels;

public sealed partial class ClusterViewModel : ViewModelBase, IInitializeCluster, IDisposable
{
    private ClusterWorkspaceViewModel? _cluster;
    private bool _disposed;
    private Task? _overviewSeedTask;

    public ClusterViewModel()
    {
        Title = Assets.Resources.ClusterViewModel_Title;
        WarningsGrid = new ClusterOverviewEventGridViewModel("Warnings", "No warning events found.", static @event => string.Equals(@event.Type, "Warning", StringComparison.Ordinal), 15);
    }

    [ObservableProperty]
    public partial ClusterWorkspaceViewModel? Cluster { get; set; }

    [ObservableProperty]
    public partial ClusterOverviewEventGridViewModel WarningsGrid { get; set; }

    public async Task RefreshData()
    {
        if (_cluster == null)
        {
            return;
        }

        await EnsureOverviewResourcesSeededAsync().ConfigureAwait(false);

        await Dispatcher.UIThread.InvokeAsync(() => WarningsGrid.Initialize(_cluster));

        await RefreshOverviewChartsAsync().ConfigureAwait(false);
    }

    public void Initialize(ClusterWorkspaceViewModel cluster)
    {
        UnsubscribeCluster();

        _cluster = cluster;
        Cluster = cluster;
        _overviewSeedTask = null;
        Id = nameof(ClusterViewModel) + "-" + cluster.Name + "-" + Title;

        WarningsGrid.Initialize(cluster);

        SubscribeCluster();
        _ = RefreshData();
    }

    public void Dispose()
    {
        if (_disposed)
        {
            return;
        }

        _disposed = true;
        UnsubscribeCluster();
        WarningsGrid.Dispose();
        DisposeOverviewCharts();
    }

    private void SubscribeCluster()
    {
        if (_cluster is INotifyPropertyChanged propertyChanged)
        {
            propertyChanged.PropertyChanged += OnClusterPropertyChanged;
        }

        if (_cluster?.NodeMetrics is INotifyCollectionChanged nodeMetrics)
        {
            nodeMetrics.CollectionChanged += OnMetricsCollectionChanged;
        }

        if (_cluster?.PodMetrics is INotifyCollectionChanged podMetrics)
        {
            podMetrics.CollectionChanged += OnMetricsCollectionChanged;
        }
    }

    private void UnsubscribeCluster()
    {
        if (_cluster is INotifyPropertyChanged propertyChanged)
        {
            propertyChanged.PropertyChanged -= OnClusterPropertyChanged;
        }

        if (_cluster?.NodeMetrics is INotifyCollectionChanged nodeMetrics)
        {
            nodeMetrics.CollectionChanged -= OnMetricsCollectionChanged;
        }

        if (_cluster?.PodMetrics is INotifyCollectionChanged podMetrics)
        {
            podMetrics.CollectionChanged -= OnMetricsCollectionChanged;
        }
    }

    private void OnClusterPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(ClusterWorkspaceViewModel.ActiveMetricsBackend)
            or nameof(ClusterWorkspaceViewModel.MetricsServiceType)
            or nameof(ClusterWorkspaceViewModel.ActivePrometheusProviderKind)
            or nameof(ClusterWorkspaceViewModel.IsMetricsAvailable))
        {
            _ = RefreshData();
        }
    }

    private void OnMetricsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        _ = RefreshData();
    }

    private Task EnsureOverviewResourcesSeededAsync()
    {
        var seedTask = _overviewSeedTask;
        if (seedTask != null)
        {
            return seedTask;
        }

        var cluster = _cluster;
        if (cluster == null)
        {
            return Task.CompletedTask;
        }

        seedTask = SeedOverviewResourcesAsync(cluster);
        return Interlocked.CompareExchange(ref _overviewSeedTask, seedTask, null) ?? seedTask;
    }

    private static async Task SeedOverviewResourcesAsync(ClusterWorkspaceViewModel cluster)
    {
        Task podSeedTask = cluster.SeedResource<V1Pod>();
        Task nodeSeedTask = cluster.SeedResource<V1Node>();
        Task eventSeedTask = cluster.SeedResource<Corev1Event>();

        await Task.WhenAll(podSeedTask, nodeSeedTask, eventSeedTask).ConfigureAwait(false);
    }
}
