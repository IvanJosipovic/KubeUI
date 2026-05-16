using System.Collections.Specialized;
using System.ComponentModel;
using Avalonia.Styling;
using Avalonia.Threading;
using k8s.Models;
using KubeUI.Avalonia.Features.Clusters.Workspace.ViewModels;
using KubeUI.Avalonia.Features.Resources.List.ViewModels;
using KubeUI.Avalonia.Infrastructure;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Avalonia.Services.Settings;
using KubeUI.Kubernetes;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel;

namespace KubeUI.Avalonia.Features.Clusters.Overview.ViewModels;

public sealed partial class ClusterViewModel : ViewModelBase, IInitializeCluster, IDisposable
{
    private ClusterWorkspaceViewModel? _cluster;
    private bool _disposed;
    private Task? _overviewSeedTask;

    [ObservableProperty]
    public partial ISettingsService Settings { get; set; }

    [ObservableProperty]
    public partial ClusterWorkspaceViewModel? Cluster { get; set; }

    [ObservableProperty]
    public partial ClusterOverviewEventGridViewModel WarningsGrid { get; set; }

    [ObservableProperty]
    public partial ResourceListViewModel<Corev1Event> EventsVM { get; set; }

    [ObservableProperty]
    public partial CPUGaugeData CPUGaugeData { get; set; } = new();

    [ObservableProperty]
    public partial MemoryGaugeData MemoryGaugeData { get; set; } = new();

    [ObservableProperty]
    public partial PodGaugeData PodGaugeData { get; set; } = new();

    public ClusterViewModel(
        ResourceListViewModel<Corev1Event> eventsVm,
        ISettingsService settings)
    {
        Title = Assets.Resources.ClusterViewModel_Title;
        WarningsGrid = new ClusterOverviewEventGridViewModel("Warnings", "No warning events found.", static @event => string.Equals(@event.Type, "Warning", StringComparison.Ordinal), 15);
        EventsVM = eventsVm;
        Settings = settings;
    }

    public async Task RefreshData()
    {
        if (_cluster == null)
        {
            return;
        }

        await EnsureOverviewResourcesSeededAsync().ConfigureAwait(false);

        await Dispatcher.UIThread.InvokeAsync(() => WarningsGrid.Initialize(_cluster));

        await RefreshOverviewChartsAsync().ConfigureAwait(false);

        var pods = _cluster.GetResourceList<V1Pod>();
        var nodes = _cluster.GetResourceList<V1Node>();

        var allPodContainers = pods.SelectMany(p => p.Spec?.Containers ?? []).ToArray();
        var allMetricContainers = (_cluster.PodMetrics ?? []).SelectMany(m => m.Containers ?? []).ToArray();

        PodGaugeData.TotalPods.Value = pods.Count;
        PodGaugeData.MaxPods.Value = nodes.Sum(x => x.Status.Capacity?.TryGetValue("pods", out var value) == true ? value.ToDouble() : 0);

        CPUGaugeData.CpuAllocatable.Value = nodes.Sum(x => x.Status.Allocatable?.TryGetValue("cpu", out var value) == true ? value.ToDouble() : 0);
        CPUGaugeData.CpuCapacity.Value = nodes.Sum(x => x.Status.Capacity?.TryGetValue("cpu", out var value) == true ? value.ToDouble() : 0);

        CPUGaugeData.CpuRequests.Value = allPodContainers.Sum(c =>
            c.Resources?.Requests?.TryGetValue("cpu", out var q) == true
                ? q.ToDouble()
                : 0d);

        CPUGaugeData.CpuLimits.Value = allPodContainers.Sum(c =>
            c.Resources?.Limits?.TryGetValue("cpu", out var q) == true
                ? q.ToDouble()
                : 0d);

        CPUGaugeData.CpuUsage.Value = allMetricContainers.Sum(c =>
            c.Usage?.TryGetValue("cpu", out var q) == true
                ? q.ToDouble()
                : 0d);

        MemoryGaugeData.MemoryAllocatable.Value = nodes.Sum(x => x.Status.Allocatable?.TryGetValue("memory", out var value) == true ? value.ToDouble() : 0) / 1048576 / 1024;
        MemoryGaugeData.MemoryCapacity.Value = nodes.Sum(x => x.Status.Capacity?.TryGetValue("memory", out var value) == true ? value.ToDouble() : 0) / 1048576 / 1024;
        MemoryGaugeData.MemoryRequests.Value = allPodContainers.Sum(c =>
            c.Resources?.Requests?.TryGetValue("memory", out var q) == true
                ? q.ToDouble()
                : 0d) / 1048576 / 1024;

        MemoryGaugeData.MemoryLimits.Value = allPodContainers.Sum(c =>
            c.Resources?.Limits?.TryGetValue("memory", out var q) == true
                ? q.ToDouble()
                : 0d) / 1048576 / 1024;

        MemoryGaugeData.MemoryUsage.Value = allMetricContainers.Sum(c =>
            c.Usage?.TryGetValue("memory", out var q) == true
                ? q.ToDouble()
                : 0d) / 1048576 / 1024;
    }

    public void Initialize(ClusterWorkspaceViewModel cluster)
    {
        UnsubscribeCluster();

        _cluster = cluster;
        Cluster = cluster;
        _overviewSeedTask = null;
        Id = nameof(ClusterViewModel) + "-" + cluster.Name + "-" + Title;

        WarningsGrid.Initialize(cluster);

        if (EventsVM is IInitializeCluster init)
        {
            init.Initialize(cluster);
        }

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

    public static string TextColor
    {
        get
        {
            if (Application.Current.ActualThemeVariant == ThemeVariant.Dark)
            {
                return "#FFFFFF";
            }

            return "#000000";
        }
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

public partial class CPUGaugeData : ObservableObject
{
    [ObservableProperty]
    public partial ObservableValue CpuCapacity { get; set; } = new();

    [ObservableProperty]
    public partial ObservableValue CpuAllocatable { get; set; } = new();

    [ObservableProperty]
    public partial ObservableValue CpuLimits { get; set; } = new();

    [ObservableProperty]
    public partial ObservableValue CpuRequests { get; set; } = new();

    [ObservableProperty]
    public partial ObservableValue CpuUsage { get; set; } = new();

    public static Func<ChartPoint, string> DataLabelsFormatter { get; } = p => $"{p.Coordinate.PrimaryValue:F2}c";
}

public partial class MemoryGaugeData : ObservableObject
{
    [ObservableProperty]
    public partial ObservableValue MemoryCapacity { get; set; } = new();

    [ObservableProperty]
    public partial ObservableValue MemoryAllocatable { get; set; } = new();

    [ObservableProperty]
    public partial ObservableValue MemoryRequests { get; set; } = new();

    [ObservableProperty]
    public partial ObservableValue MemoryLimits { get; set; } = new();

    [ObservableProperty]
    public partial ObservableValue MemoryUsage { get; set; } = new();

    public static Func<ChartPoint, string> DataLabelsFormatter { get; } = p => $"{p.Coordinate.PrimaryValue:F2}Gi";
}

public partial class PodGaugeData : ObservableObject
{
    [ObservableProperty]
    public partial ObservableValue MaxPods { get; set; } = new();

    [ObservableProperty]
    public partial ObservableValue TotalPods { get; set; } = new();

    public static Func<ChartPoint, string> DataLabelsFormatter { get; } = p => $"{p.Coordinate.PrimaryValue:F0}";
}
