using KubeUI.Avalonia.Features.Clusters.Workspace.ViewModels;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Kubernetes;
using Avalonia.Threading;
using k8s;
using k8s.Models;
using System.Collections.Specialized;
using System.ComponentModel;

namespace KubeUI.Avalonia.Features.Clusters.Overview.ViewModels;

public sealed partial class ClusterViewModel : ViewModelBase, IInitializeCluster, IDisposable
{
    private ClusterWorkspaceViewModel? _cluster;
    private bool _disposed;

    public ClusterViewModel()
    {
        Title = Assets.Resources.ClusterViewModel_Title;
        WarningsGrid = new ClusterOverviewEventGridViewModel("Warnings", "No warning events found.", static @event => string.Equals(@event.Type, "Warning", StringComparison.Ordinal), 15);
    }

    [ObservableProperty]
    public partial ClusterWorkspaceViewModel? Cluster { get; set; }

    [ObservableProperty]
    public partial ClusterOverviewEventGridViewModel WarningsGrid { get; set; }

    [ObservableProperty]
    public partial string BackendText { get; set; } = "Metrics backend unavailable";

    [ObservableProperty]
    public partial string LastUpdatedText { get; set; } = "Not loaded";

    [ObservableProperty]
    public partial string StatusText { get; set; } = "Cluster overview metrics are loading.";

    [ObservableProperty]
    public partial bool ShowStatus { get; set; } = true;

    public async Task RefreshData()
    {
        if (_cluster == null)
        {
            return;
        }

        await _cluster.SeedResource<V1Pod>(true).ConfigureAwait(false);
        await _cluster.SeedResource<V1Node>(true).ConfigureAwait(false);
        await _cluster.SeedResource<V1Namespace>(true).ConfigureAwait(false);
        await _cluster.SeedResource<V1Deployment>().ConfigureAwait(false);
        await _cluster.SeedResource<V1StatefulSet>().ConfigureAwait(false);
        await _cluster.SeedResource<V1DaemonSet>().ConfigureAwait(false);
        await _cluster.SeedResource<V1Job>().ConfigureAwait(false);

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            BackendText = FormatBackendText(_cluster);
            LastUpdatedText = DateTime.Now.ToString("g");
            ShowStatus = true;
            StatusText = _cluster.MetricsServiceType switch
            {
                MetricsServiceType.Prometheus => "Cluster overview metrics and live event grids are active.",
                MetricsServiceType.KubernetesMetricsServer => "Cluster overview is using live resource summaries and event grids. Historical charting is still limited on Metrics Server.",
                _ => "Metrics are not available for this cluster yet.",
            };
        });
    }

    public void Initialize(ClusterWorkspaceViewModel cluster)
    {
        UnsubscribeCluster();

        _cluster = cluster;
        Cluster = cluster;
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

    private static string FormatBackendText(ClusterWorkspaceViewModel cluster)
    {
        return cluster.MetricsServiceType switch
        {
            MetricsServiceType.Prometheus => cluster.ActivePrometheusProviderKind is { } provider
                ? $"Prometheus ({provider})"
                : "Prometheus",
            MetricsServiceType.KubernetesMetricsServer => "Kubernetes Metrics Server",
            _ => "No metrics backend",
        };
    }
}
