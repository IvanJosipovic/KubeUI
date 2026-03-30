using Avalonia.Controls;
using Avalonia.Threading;
using FluentIcons.Common;
using KubeUI.Avalonia.Features.Clusters.Workspace.ViewModels;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Kubernetes;
using k8s;
using k8s.Models;
using LiveChartsCore.Defaults;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Text.RegularExpressions;
using System.Threading;

namespace KubeUI.Avalonia.Features.Resources.Properties.Controls;

public sealed partial class PodContainerMetricsControl : UserControl, IInitializeCluster
{
    private static readonly TimeSpan s_metricsServerSampleInterval = TimeSpan.FromSeconds(30);
    private static readonly DispatcherTimer s_timer = new(DispatcherPriority.Default);
    private ClusterWorkspaceViewModel? _cluster;
    private ContainerMetricsServerHistoryState? _metricsServerHistory;
    private readonly SemaphoreSlim _refreshGate = new(1, 1);
    private int _refreshPending;

    public static readonly StyledProperty<V1Pod?> PodProperty =
        AvaloniaProperty.Register<PodContainerMetricsControl, V1Pod?>(nameof(Pod));

    public static readonly StyledProperty<V1Container?> ContainerProperty =
        AvaloniaProperty.Register<PodContainerMetricsControl, V1Container?>(nameof(Container));

    static PodContainerMetricsControl()
    {
        PodProperty.Changed.AddClassHandler<PodContainerMetricsControl>((control, _) => control.QueueRefresh());
        ContainerProperty.Changed.AddClassHandler<PodContainerMetricsControl>((control, _) => control.QueueRefresh());
    }

    [GeneratedDirectProperty]
    public partial ObservableCollection<MetricTabViewModel> Tabs { get; set; } = [];

    [GeneratedDirectProperty]
    public partial string? StatusText { get; set; }

    [GeneratedDirectProperty]
    public partial bool ShowStatus { get; set; }

    [GeneratedDirectProperty]
    public partial bool ShowTabs { get; set; }

    [GeneratedDirectProperty]
    public partial MetricTabViewModel? SelectedTab { get; set; }

    public V1Pod? Pod
    {
        get => GetValue(PodProperty);
        set => SetValue(PodProperty, value);
    }

    public V1Container? Container
    {
        get => GetValue(ContainerProperty);
        set => SetValue(ContainerProperty, value);
    }

    public PodContainerMetricsControl()
    {
        InitializeComponent();

        if (!s_timer.IsEnabled)
        {
            s_timer.Interval = s_metricsServerSampleInterval;
            s_timer.Start();
        }
    }

    public void Initialize(ClusterWorkspaceViewModel cluster)
    {
        UnsubscribeCluster();
        _cluster = cluster;
        SubscribeCluster();
        QueueRefresh();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        s_timer.Tick += Timer_Tick;
        QueueRefresh();
    }

    protected override void OnDetachedFromVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnDetachedFromVisualTree(e);
        s_timer.Tick -= Timer_Tick;
        UnsubscribeCluster();
    }

    private void Timer_Tick(object? sender, EventArgs e) => QueueRefresh();

    private void SubscribeCluster()
    {
        if (_cluster is INotifyPropertyChanged propertyChanged)
        {
            propertyChanged.PropertyChanged += OnClusterPropertyChanged;
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

        if (_cluster?.PodMetrics is INotifyCollectionChanged podMetrics)
        {
            podMetrics.CollectionChanged -= OnMetricsCollectionChanged;
        }
    }

    private void OnMetricsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e) => QueueRefresh();

    private void OnClusterPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(ClusterWorkspaceViewModel.ActiveMetricsBackend)
            or nameof(ClusterWorkspaceViewModel.MetricsServiceType)
            or nameof(ClusterWorkspaceViewModel.ActivePrometheusProviderKind)
            or nameof(ClusterWorkspaceViewModel.IsMetricsAvailable))
        {
            QueueRefresh();
        }
    }

    private void QueueRefresh()
    {
        Interlocked.Exchange(ref _refreshPending, 1);
        _ = RefreshLoopAsync();
    }

    private async Task RefreshLoopAsync()
    {
        if (!await _refreshGate.WaitAsync(0).ConfigureAwait(false))
        {
            return;
        }

        try
        {
            while (Interlocked.Exchange(ref _refreshPending, 0) == 1)
            {
                try
                {
                    await Dispatcher.UIThread.InvokeAsync(RefreshCoreAsync);
                }
                catch
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        Tabs.Clear();
                        SelectedTab = null;
                        ShowTabs = false;
                        ShowStatus = true;
                        StatusText = "Unable to load container metrics.";
                    });
                }
            }
        }
        finally
        {
            _refreshGate.Release();
        }
    }

    private async Task RefreshCoreAsync()
    {
        if (_cluster == null || Pod == null || Container == null || string.IsNullOrWhiteSpace(Container.Name))
        {
            IsVisible = false;
            Tabs.Clear();
            SelectedTab = null;
            _metricsServerHistory = null;
            return;
        }

        IsVisible = true;

        if (_cluster.MetricsServiceType == MetricsServiceType.KubernetesMetricsServer)
        {
            var tabs = CaptureMetricsServerCharts(Pod, Container.Name);
            MergeTabs(tabs);
            ShowTabs = Tabs.Count > 0;
            ShowStatus = !ShowTabs;
            StatusText = ShowTabs ? null : "Current container metrics are unavailable while the cluster is using Kubernetes Metrics Server.";

            if (SelectedTab == null || !Tabs.Contains(SelectedTab))
            {
                SelectedTab = Tabs.FirstOrDefault();
            }

            return;
        }

        _metricsServerHistory = null;

        if (_cluster.MetricsServiceType != MetricsServiceType.Prometheus)
        {
            Tabs.Clear();
            SelectedTab = null;
            ShowTabs = false;
            ShowStatus = true;
            StatusText = "Metrics are not available for this cluster.";
            return;
        }

        var request = CreatePrometheusRequest(Pod, Container.Name);
        var panels = new List<MetricPanelSnapshot>();

        foreach (var panel in request)
        {
            var result = await _cluster.RequestMetricsAsync(panel.Request).ConfigureAwait(false);
            var series = CreateSeries(panel, result);
            if (series.Count > 0)
            {
                panels.Add(new MetricPanelSnapshot(panel.Title, series));
            }
        }

        var snapshots = panels.Count == 0
            ? []
            : new[]
            {
                new MetricTabSnapshot("CPU", Icon.ArrowSync, panels.Where(x => x.Title.StartsWith("CPU", StringComparison.Ordinal)).ToArray()),
                new MetricTabSnapshot("Memory", Icon.Code, panels.Where(x => x.Title.StartsWith("Memory", StringComparison.Ordinal)).ToArray()),
                new MetricTabSnapshot("Filesystem", Icon.Save, panels.Where(x => x.Title.StartsWith("Filesystem", StringComparison.Ordinal)).ToArray()),
            }.Where(x => x.Panels.Count > 0).ToArray();

        MergeTabs(snapshots);
        ShowTabs = Tabs.Count > 0;
        ShowStatus = !ShowTabs;
        StatusText = ShowTabs ? null : "No Prometheus metrics are available for this container.";

        if (SelectedTab == null || !Tabs.Contains(SelectedTab))
        {
            SelectedTab = Tabs.FirstOrDefault();
        }
    }

    private IReadOnlyList<MetricTabSnapshot> CaptureMetricsServerCharts(V1Pod pod, string containerName)
    {
        var resourceKey = $"pod:{pod.Namespace()}:{pod.Name()}:{containerName}";

        if (_metricsServerHistory == null || !string.Equals(_metricsServerHistory.ResourceKey, resourceKey, StringComparison.Ordinal))
        {
            _metricsServerHistory = new ContainerMetricsServerHistoryState(resourceKey);
        }

        var metric = _cluster?.PodMetrics.FirstOrDefault(x =>
            string.Equals(x.Name(), pod.Name(), StringComparison.Ordinal)
            && string.Equals(x.Namespace(), pod.Namespace(), StringComparison.Ordinal));
        var containerMetric = metric?.Containers?.FirstOrDefault(x => string.Equals(x.Name, containerName, StringComparison.Ordinal));

        if (containerMetric == null)
        {
            return CreateMetricsServerTabs(_metricsServerHistory);
        }

        var timestamp = NormalizeMetricsServerTimestamp(DateTimeOffset.UtcNow);
        _metricsServerHistory.Upsert(timestamp, new ContainerMetricsServerSample(containerMetric.Usage["cpu"].ToDecimal(), containerMetric.Usage["memory"].ToInt64()));
        return CreateMetricsServerTabs(_metricsServerHistory);
    }

    private static MetricPanelDefinition[] CreatePrometheusRequest(V1Pod pod, string containerName)
    {
        var options = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["namespace"] = pod.Namespace(),
            ["pods"] = Regex.Escape(pod.Name()),
            ["selector"] = "container, pod, namespace",
            ["container"] = containerName,
        };

        return
        [
            CreatePanel("CPU Usage", [("cpuUsage", "Usage")]),
            CreatePanel("CPU Requests & Limits", [("cpuRequests", "Requests"), ("cpuLimits", "Limits")]),
            CreatePanel("Memory Usage", [("memoryUsage", "Usage")]),
            CreatePanel("Memory Requests & Limits", [("memoryRequests", "Requests"), ("memoryLimits", "Limits")]),
            CreatePanel("Filesystem", [("fsUsage", "Usage"), ("fsReads", "Reads"), ("fsWrites", "Writes")]),
        ];

        MetricPanelDefinition CreatePanel(string title, IReadOnlyList<(string QueryName, string Label)> queries)
        {
            return new MetricPanelDefinition
            {
                Title = title,
                Filter = static _ => true,
                LegendLabels = queries.ToDictionary(static x => x.QueryName, static x => x.Label, StringComparer.Ordinal),
                Request = new MetricRequest
                {
                    Category = MetricCategory.Pods,
                    StepSeconds = 60,
                    RangeSeconds = 3600,
                    Queries = queries.Select(query => new MetricQueryDefinition
                    {
                        Name = query.QueryName,
                        Options = new Dictionary<string, string>(options, StringComparer.Ordinal),
                    }).ToArray(),
                },
            };
        }
    }

    private void MergeTabs(IReadOnlyList<MetricTabSnapshot> snapshots)
    {
        var selectedTitle = SelectedTab?.Title;

        for (var i = Tabs.Count - 1; i >= 0; i--)
        {
            if (!snapshots.Any(x => string.Equals(x.Title, Tabs[i].Title, StringComparison.Ordinal)))
            {
                Tabs.RemoveAt(i);
            }
        }

        for (var index = 0; index < snapshots.Count; index++)
        {
            var snapshot = snapshots[index];
            var existing = Tabs.FirstOrDefault(x => string.Equals(x.Title, snapshot.Title, StringComparison.Ordinal));

            if (existing == null)
            {
                existing = new MetricTabViewModel
                {
                    Title = snapshot.Title,
                    Icon = snapshot.Icon,
                };
                Tabs.Insert(index, existing);
            }
            else
            {
                existing.Icon = snapshot.Icon;
                var currentIndex = Tabs.IndexOf(existing);
                if (currentIndex != index)
                {
                    Tabs.Move(currentIndex, index);
                }
            }

            existing.MergePanels(snapshot.Panels);
        }

        SelectedTab = selectedTitle == null
            ? Tabs.FirstOrDefault()
            : Tabs.FirstOrDefault(x => string.Equals(x.Title, selectedTitle, StringComparison.Ordinal)) ?? Tabs.FirstOrDefault();
    }

    private static IReadOnlyList<MetricSeriesSnapshot> CreateSeries(MetricPanelDefinition panel, MetricResultSet result)
    {
        if (result.IsEmpty)
        {
            return [];
        }

        var series = new List<MetricSeriesSnapshot>();

        foreach (var query in panel.Request.Queries)
        {
            if (!result.Metrics.TryGetValue(query.Name, out var metricSeries))
            {
                continue;
            }

            foreach (var item in metricSeries.Where(panel.Filter))
            {
                var legend = panel.LegendLabels.TryGetValue(query.Name, out var label) ? label : query.Name;
                series.Add(new MetricSeriesSnapshot(
                    legend,
                    item.Points.Select(static point => new DateTimePoint(point.Timestamp.LocalDateTime, point.Value)).ToArray()));
            }
        }

        return series;
    }

    private static DateTimeOffset NormalizeMetricsServerTimestamp(DateTimeOffset timestamp)
    {
        var seconds = (long)(timestamp.ToUnixTimeSeconds() / s_metricsServerSampleInterval.TotalSeconds * s_metricsServerSampleInterval.TotalSeconds);
        return DateTimeOffset.FromUnixTimeSeconds(seconds);
    }

    private static IReadOnlyList<MetricTabSnapshot> CreateMetricsServerTabs(ContainerMetricsServerHistoryState history)
    {
        if (history.Samples.Count == 0)
        {
            return [];
        }

        var cpuPoints = CreateMetricsServerChartPoints(history.Samples, static x => (double)x.Cpu);
        var memoryPoints = CreateMetricsServerChartPoints(history.Samples, static x => x.Memory);

        return
        [
            new MetricTabSnapshot("CPU", Icon.ArrowSync,
            [
                new MetricPanelSnapshot("CPU",
                [
                    new MetricSeriesSnapshot("Usage", cpuPoints),
                ]),
            ]),
            new MetricTabSnapshot("Memory", Icon.Code,
            [
                new MetricPanelSnapshot("Memory",
                [
                    new MetricSeriesSnapshot("Usage", memoryPoints),
                ]),
            ]),
        ];
    }

    private static IReadOnlyList<DateTimePoint> CreateMetricsServerChartPoints(
        IReadOnlyList<ContainerMetricsServerSamplePoint> samples,
        Func<ContainerMetricsServerSamplePoint, double> selector)
    {
        if (samples.Count == 1)
        {
            var sample = samples[0];
            var value = selector(sample);
            return
            [
                new DateTimePoint(sample.Timestamp.Subtract(s_metricsServerSampleInterval).LocalDateTime, value),
                new DateTimePoint(sample.Timestamp.LocalDateTime, value),
            ];
        }

        return samples.Select(sample => new DateTimePoint(sample.Timestamp.LocalDateTime, selector(sample))).ToArray();
    }
}

internal sealed class ContainerMetricsServerHistoryState(string resourceKey)
{
    private static readonly TimeSpan s_historyDuration = TimeSpan.FromHours(1);

    public string ResourceKey { get; } = resourceKey;

    public List<ContainerMetricsServerSamplePoint> Samples { get; } = [];

    public void Upsert(DateTimeOffset timestamp, ContainerMetricsServerSample sample)
    {
        var existing = Samples.FindIndex(x => x.Timestamp == timestamp);
        var point = new ContainerMetricsServerSamplePoint(timestamp, sample.Cpu, sample.Memory);

        if (existing >= 0)
        {
            Samples[existing] = point;
        }
        else
        {
            Samples.Add(point);
        }

        var cutoff = timestamp - s_historyDuration;
        Samples.RemoveAll(x => x.Timestamp < cutoff);
        Samples.Sort(static (a, b) => a.Timestamp.CompareTo(b.Timestamp));
    }
}

internal readonly record struct ContainerMetricsServerSample(decimal Cpu, long Memory);

internal readonly record struct ContainerMetricsServerSamplePoint(DateTimeOffset Timestamp, decimal Cpu, long Memory);
