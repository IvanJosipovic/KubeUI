using KubeUI.Avalonia.Features.Clusters.Workspace.ViewModels;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Kubernetes;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.Styling;
using Avalonia.VisualTree;
using FluentIcons.Common;
using Humanizer;
using k8s;
using k8s.Models;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Threading;

namespace KubeUI.Avalonia.Features.Resources.Properties.Controls;

public sealed partial class MetricsControl : UserControl, IInitializeCluster
{
    private static readonly TimeSpan s_metricsServerSampleInterval = TimeSpan.FromSeconds(30);
    private static readonly DispatcherTimer s_timer = new(DispatcherPriority.Default);
    private ClusterWorkspaceViewModel? _cluster;
    private MetricsServerHistoryState? _metricsServerHistory;
    private readonly SemaphoreSlim _refreshGate = new(1, 1);
    private int _refreshPending;

    [GeneratedDirectProperty]
    public partial ObservableCollection<MetricTabViewModel> Tabs { get; set; } = [];

    [GeneratedDirectProperty]
    public partial string? StatusText { get; set; }

    [GeneratedDirectProperty]
    public partial bool ShowStatus { get; set; }

    [GeneratedDirectProperty]
    public partial bool ShowTabs { get; set; }

    [GeneratedDirectProperty]
    public partial ObservableCollection<MetricSummaryItemViewModel> CurrentMetrics { get; set; } = [];

    [GeneratedDirectProperty]
    public partial bool ShowCurrentMetrics { get; set; }

    [GeneratedDirectProperty]
    public partial MetricTabViewModel? SelectedTab { get; set; }

    public MetricsControl()
    {
        InitializeComponent();
        AddHandler(InputElement.PointerWheelChangedEvent, OnPointerWheelChanged, RoutingStrategies.Tunnel, handledEventsToo: true);

        if (!s_timer.IsEnabled)
        {
            s_timer.Interval = TimeSpan.FromSeconds(30);
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

    protected override void OnDataContextChanged(EventArgs e)
    {
        base.OnDataContextChanged(e);
        QueueRefresh();
    }

    private async void Timer_Tick(object? sender, EventArgs e)
    {
        QueueRefresh();
    }

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

        if (_cluster?.NodeMetrics is INotifyCollectionChanged nodeMetrics)
        {
            nodeMetrics.CollectionChanged += OnMetricsCollectionChanged;
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

        if (_cluster?.NodeMetrics is INotifyCollectionChanged nodeMetrics)
        {
            nodeMetrics.CollectionChanged -= OnMetricsCollectionChanged;
        }
    }

    private void OnMetricsCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        QueueRefresh();
    }

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
                catch (Exception)
                {
                    await Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        Tabs.Clear();
                        CurrentMetrics.Clear();
                        SelectedTab = null;
                        ShowTabs = false;
                        ShowCurrentMetrics = false;
                        ShowStatus = true;
                        StatusText = "Unable to load metrics.";
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
        if (!Dispatcher.UIThread.CheckAccess())
        {
            throw new InvalidOperationException("MetricsControl refresh must run on the UI thread.");
        }

        if (_cluster == null || DataContext is not IKubernetesObject<V1ObjectMeta> resource)
        {
            IsVisible = false;
            Tabs.Clear();
            CurrentMetrics.Clear();
            SelectedTab = null;
            _metricsServerHistory = null;
            return;
        }

        IsVisible = true;

        if (_cluster.MetricsServiceType == MetricsServiceType.KubernetesMetricsServer)
        {
            var historyTabs = TryCaptureMetricsServerCharts(resource);
            MergeTabs(historyTabs);
            ShowTabs = Tabs.Count > 0;
            ShowCurrentMetrics = false;
            CurrentMetrics.Clear();
            ShowStatus = !ShowTabs;
            StatusText = ShowTabs
                ? null
                : "Current metrics are unavailable for this resource while the cluster is using Kubernetes Metrics Server.";

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
            CurrentMetrics.Clear();
            SelectedTab = null;
            ShowTabs = false;
            ShowCurrentMetrics = false;
            ShowStatus = true;
            StatusText = "Metrics are not available for this cluster.";
            return;
        }

        var descriptor = await ResourceMetricsCatalog.CreateAsync(_cluster, resource).ConfigureAwait(false);
        if (descriptor == null)
        {
            IsVisible = false;
            Tabs.Clear();
            CurrentMetrics.Clear();
            SelectedTab = null;
            return;
        }

        var tabs = new List<MetricTabSnapshot>();

        foreach (var tab in descriptor.Tabs)
        {
            var panels = new List<MetricPanelSnapshot>();

            foreach (var panel in tab.Panels)
            {
                var result = await _cluster.RequestMetricsAsync(panel.Request).ConfigureAwait(false);
                var series = CreateSeries(panel, result);
                if (series.Count == 0)
                {
                    continue;
                }

                panels.Add(new MetricPanelSnapshot(panel.Title, series));
            }

            if (panels.Count > 0)
            {
                tabs.Add(new MetricTabSnapshot(tab.Title, tab.Icon, panels));
            }
        }

        MergeTabs(tabs);
        CurrentMetrics.Clear();
        ShowTabs = Tabs.Count > 0;
        ShowCurrentMetrics = false;
        ShowStatus = !ShowTabs;
        if (SelectedTab == null || !Tabs.Contains(SelectedTab))
        {
            SelectedTab = Tabs.FirstOrDefault();
        }
        StatusText = ShowTabs ? null : descriptor.EmptyState ?? "No Prometheus metrics are available for this resource.";
    }

    private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        if (e.Source is not Visual source
            || source.FindAncestorOfType<LiveChartsCore.SkiaSharpView.Avalonia.CartesianChart>() == null)
        {
            return;
        }

        var scrollViewer = this.FindAncestorOfType<ScrollViewer>();
        if (scrollViewer == null)
        {
            return;
        }

        const double scrollStep = 48;
        var maxOffset = Math.Max(0, scrollViewer.Extent.Height - scrollViewer.Viewport.Height);
        var nextOffset = Math.Clamp(scrollViewer.Offset.Y - (e.Delta.Y * scrollStep), 0, maxOffset);
        scrollViewer.Offset = new Vector(scrollViewer.Offset.X, nextOffset);
        e.Handled = true;
    }

    private void MergeCurrentMetrics(IReadOnlyList<MetricSummaryItemViewModel> snapshots)
    {
        for (var i = CurrentMetrics.Count - 1; i >= 0; i--)
        {
            if (!snapshots.Any(x => string.Equals(x.Key, CurrentMetrics[i].Key, StringComparison.Ordinal)))
            {
                CurrentMetrics.RemoveAt(i);
            }
        }

        for (var index = 0; index < snapshots.Count; index++)
        {
            var snapshot = snapshots[index];
            var existing = CurrentMetrics.FirstOrDefault(x => string.Equals(x.Key, snapshot.Key, StringComparison.Ordinal));

            if (existing == null)
            {
                existing = new MetricSummaryItemViewModel
                {
                    Key = snapshot.Key,
                    Value = snapshot.Value,
                };
                CurrentMetrics.Insert(index, existing);
            }
            else
            {
                existing.Value = snapshot.Value;
                var currentIndex = CurrentMetrics.IndexOf(existing);
                if (currentIndex != index)
                {
                    CurrentMetrics.Move(currentIndex, index);
                }
            }
        }
    }

    private IReadOnlyList<MetricTabSnapshot> TryCaptureMetricsServerCharts(IKubernetesObject<V1ObjectMeta> resource)
    {
        var resourceKey = GetMetricsServerResourceKey(resource);
        if (resourceKey == null)
        {
            _metricsServerHistory = null;
            return [];
        }

        if (_metricsServerHistory == null
            || !string.Equals(_metricsServerHistory.ResourceKey, resourceKey, StringComparison.Ordinal))
        {
            _metricsServerHistory = new MetricsServerHistoryState(resourceKey);
        }

        var sample = resource switch
        {
            V1Pod pod => TryCreatePodMetricsServerSample(pod),
            V1Node node => TryCreateNodeMetricsServerSample(node),
            _ => null,
        };

        if (sample == null)
        {
            return CreateMetricsServerTabs(_metricsServerHistory);
        }

        var timestamp = NormalizeMetricsServerTimestamp(DateTimeOffset.UtcNow);
        _metricsServerHistory.Upsert(timestamp, sample.Value);
        return CreateMetricsServerTabs(_metricsServerHistory);
    }

    private static string? GetMetricsServerResourceKey(IKubernetesObject<V1ObjectMeta> resource)
    {
        return resource switch
        {
            V1Pod pod => $"pod:{pod.Namespace()}:{pod.Name()}",
            V1Node node => $"node::{node.Name()}",
            _ => null,
        };
    }

    private MetricsServerSample? TryCreatePodMetricsServerSample(V1Pod pod)
    {
        var metric = _cluster?.PodMetrics.FirstOrDefault(x =>
            string.Equals(x.Name(), pod.Name(), StringComparison.Ordinal)
            && string.Equals(x.Namespace(), pod.Namespace(), StringComparison.Ordinal));

        if (metric == null || metric.Containers == null || metric.Containers.Count == 0)
        {
            return null;
        }

        var cpu = metric.Containers.Sum(static x => x.Usage["cpu"].ToDecimal());
        var memory = metric.Containers.Sum(static x => (double)x.Usage["memory"].ToInt64());
        return new MetricsServerSample(cpu, memory);
    }

    private MetricsServerSample? TryCreateNodeMetricsServerSample(V1Node node)
    {
        var metric = _cluster?.NodeMetrics.FirstOrDefault(x => string.Equals(x.Name(), node.Name(), StringComparison.Ordinal));
        if (metric == null)
        {
            return null;
        }

        var cpu = metric.Usage["cpu"].ToDecimal();
        var memory = (double)metric.Usage["memory"].ToInt64();
        return new MetricsServerSample(cpu, memory);
    }

    private static DateTimeOffset NormalizeMetricsServerTimestamp(DateTimeOffset timestamp)
    {
        var seconds = (long)(timestamp.ToUnixTimeSeconds() / s_metricsServerSampleInterval.TotalSeconds * s_metricsServerSampleInterval.TotalSeconds);
        return DateTimeOffset.FromUnixTimeSeconds(seconds);
    }

    private static IReadOnlyList<MetricTabSnapshot> CreateMetricsServerTabs(MetricsServerHistoryState history)
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
                    new MetricSeriesSnapshot(
                        "Usage",
                        cpuPoints),
                ]),
            ]),
            new MetricTabSnapshot("Memory", Icon.Code,
            [
                new MetricPanelSnapshot("Memory",
                [
                    new MetricSeriesSnapshot(
                        "Usage",
                        memoryPoints),
                ]),
            ]),
        ];
    }

    private static IReadOnlyList<DateTimePoint> CreateMetricsServerChartPoints(
        IReadOnlyList<MetricsServerSamplePoint> samples,
        Func<MetricsServerSamplePoint, double> selector)
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

        return samples
            .Select(sample => new DateTimePoint(sample.Timestamp.LocalDateTime, selector(sample)))
            .ToArray();
    }

    private IReadOnlyList<MetricSummaryItemViewModel> CreateCurrentMetrics(IKubernetesObject<V1ObjectMeta> resource)
    {
        return resource switch
        {
            V1Pod pod => CreateCurrentMetricsForPods([pod.Name()], pod.Namespace()),
            V1Node node => CreateCurrentMetricsForNode(node.Name()),
            V1Namespace ns => CreateCurrentMetricsForPods(null, ns.Name()),
            V1Deployment deployment => CreateCurrentMetricsForSelector(deployment.Spec?.Selector?.MatchLabels, deployment.Namespace()),
            V1StatefulSet statefulSet => CreateCurrentMetricsForSelector(statefulSet.Spec?.Selector?.MatchLabels, statefulSet.Namespace()),
            V1DaemonSet daemonSet => CreateCurrentMetricsForSelector(daemonSet.Spec?.Selector?.MatchLabels, daemonSet.Namespace()),
            V1ReplicaSet replicaSet => CreateCurrentMetricsForSelector(replicaSet.Spec?.Selector?.MatchLabels, replicaSet.Namespace()),
            V1Job job => CreateCurrentMetricsForSelector(job.Spec?.Selector?.MatchLabels, job.Namespace()),
            _ => [],
        };
    }

    private IReadOnlyList<MetricSummaryItemViewModel> CreateCurrentMetricsForNode(string? nodeName)
    {
        if (string.IsNullOrWhiteSpace(nodeName))
        {
            return [];
        }

        var metric = _cluster?.NodeMetrics.FirstOrDefault(x => x.Name() == nodeName);
        if (metric == null)
        {
            return [];
        }

        return
        [
            new MetricSummaryItemViewModel { Key = "CPU", Value = $"{metric.Usage["cpu"].ToDecimal():F3}c" },
            new MetricSummaryItemViewModel { Key = "Memory", Value = metric.Usage["memory"].ToInt64().Bytes().Humanize() },
        ];
    }

    private IReadOnlyList<MetricSummaryItemViewModel> CreateCurrentMetricsForSelector(IDictionary<string, string>? selector, string? @namespace)
    {
        if (selector == null || selector.Count == 0 || string.IsNullOrWhiteSpace(@namespace))
        {
            return [];
        }

        var podNames = _cluster?.GetResourceList<V1Pod>()
            .Where(x => x.Namespace() == @namespace && MatchesSelector(x.Metadata?.Labels, selector))
            .Select(x => x.Name())
            .Where(x => !string.IsNullOrWhiteSpace(x))
            .Cast<string>()
            .Distinct(StringComparer.Ordinal)
            .ToArray();

        if (podNames == null || podNames.Length == 0)
        {
            return [];
        }

        return CreateCurrentMetricsForPods(podNames, @namespace);
    }

    private IReadOnlyList<MetricSummaryItemViewModel> CreateCurrentMetricsForPods(IReadOnlyCollection<string>? podNames, string? @namespace)
    {
        if (string.IsNullOrWhiteSpace(@namespace))
        {
            return [];
        }

        var podMetrics = (_cluster?.PodMetrics ?? [])
            .Where(x => x.Namespace() == @namespace
                && (podNames == null || podNames.Count == 0 || podNames.Contains(x.Name(), StringComparer.Ordinal)))
            .ToList();

        if (podMetrics.Count == 0)
        {
            return [];
        }

        decimal cpu = 0;
        long memoryBytes = 0;
        var containerCount = 0;

        foreach (var metric in podMetrics)
        {
            foreach (var container in metric.Containers)
            {
                containerCount++;
                cpu += container.Usage["cpu"].ToDecimal();
                memoryBytes += container.Usage["memory"].ToInt64();
            }
        }

        return
        [
            new MetricSummaryItemViewModel { Key = "Pods", Value = podMetrics.Count.ToString() },
            new MetricSummaryItemViewModel { Key = "Containers", Value = containerCount.ToString() },
            new MetricSummaryItemViewModel { Key = "CPU", Value = $"{cpu:F3}c" },
            new MetricSummaryItemViewModel { Key = "Memory", Value = memoryBytes.Bytes().Humanize() },
        ];
    }

    private static bool MatchesSelector(IDictionary<string, string>? labels, IDictionary<string, string> selector)
    {
        if (labels == null || selector.Count == 0)
        {
            return false;
        }

        foreach (var pair in selector)
        {
            if (!labels.TryGetValue(pair.Key, out var value) || !string.Equals(value, pair.Value, StringComparison.Ordinal))
            {
                return false;
            }
        }

        return true;
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
                var legend = panel.LegendLabels.TryGetValue(query.Name, out var label)
                    ? label
                    : query.Name;

                if (item.Labels.Count > 0 && metricSeries.Count > 1)
                {
                    legend += $" ({item.Labels.Values.FirstOrDefault()})";
                }

                series.Add(new MetricSeriesSnapshot(
                    legend,
                    item.Points.Select(static value => new DateTimePoint(value.Timestamp.LocalDateTime, value.Value)).ToArray()));
            }
        }

        return series;
    }
}

public sealed partial class MetricTabViewModel : ObservableObject
{
    public required string Title { get; init; }

    [ObservableProperty]
    public partial Icon Icon { get; set; }

    [ObservableProperty]
    public partial bool ShowTitle { get; set; } = true;

    public ObservableCollection<MetricPanelViewModel> Panels { get; } = [];

    internal void MergePanels(IReadOnlyList<MetricPanelSnapshot> snapshots)
    {
        for (var i = Panels.Count - 1; i >= 0; i--)
        {
            if (!snapshots.Any(x => string.Equals(x.Title, Panels[i].Title, StringComparison.Ordinal)))
            {
                Panels.RemoveAt(i);
            }
        }

        for (var index = 0; index < snapshots.Count; index++)
        {
            var snapshot = snapshots[index];
            var existing = Panels.FirstOrDefault(x => string.Equals(x.Title, snapshot.Title, StringComparison.Ordinal));

            if (existing == null)
            {
                existing = new MetricPanelViewModel
                {
                    Title = snapshot.Title,
                };
                Panels.Insert(index, existing);
            }

            existing.MergeSeries(snapshot.Series);

            if (existing != null)
            {
                var currentIndex = Panels.IndexOf(existing);
                if (currentIndex != index)
                {
                    Panels.Move(currentIndex, index);
                }
            }
        }

        ShowTitle = snapshots.Count != 1
            || !string.Equals(Title, snapshots[0].Title, StringComparison.OrdinalIgnoreCase);
    }
}

public sealed partial class MetricPanelViewModel : ObservableObject
{
    private static readonly TimeSpan s_defaultTimeWindow = TimeSpan.FromHours(1);
    private readonly Axis _yAxis = new()
    {
        LabelsPaint = CreateChartTextPaint(),
        TextSize = 11,
    };
    private readonly Axis _xAxis;

    public required string Title { get; init; }

    public ObservableCollection<ISeries> Series { get; } = [];

    public ICartesianAxis[] XAxes { get; }

    public ICartesianAxis[] YAxes { get; } =
    [];

    public MetricPanelViewModel()
    {
        _xAxis = new DateTimeAxis(TimeSpan.FromMinutes(10), value => value.ToString("HH:mm"))
        {
            LabelsPaint = CreateChartTextPaint(),
            TextSize = 11,
        };
        XAxes = [_xAxis];
        YAxes = [_yAxis];
    }

    internal void MergeSeries(IReadOnlyList<MetricSeriesSnapshot> snapshots)
    {
        for (var i = Series.Count - 1; i >= 0; i--)
        {
            if (Series[i] is not LineSeries<DateTimePoint> line
                || !snapshots.Any(x => string.Equals(x.Name, line.Name, StringComparison.Ordinal)))
            {
                Series.RemoveAt(i);
            }
        }

        for (var index = 0; index < snapshots.Count; index++)
        {
            var snapshot = snapshots[index];
            var existing = Series
                .OfType<LineSeries<DateTimePoint>>()
                .FirstOrDefault(x => string.Equals(x.Name, snapshot.Name, StringComparison.Ordinal));

            if (existing == null)
            {
                existing = new LineSeries<DateTimePoint>
                {
                    Name = snapshot.Name,
                    GeometrySize = 0,
                    Values = new ObservableCollection<DateTimePoint>(snapshot.Points),
                };
                Series.Insert(index, existing);
            }
            else
            {
                var values = existing.Values as ObservableCollection<DateTimePoint>;
                if (values == null)
                {
                    values = new ObservableCollection<DateTimePoint>();
                    existing.Values = values;
                }

                SyncPoints(values, snapshot.Points);
                var currentIndex = Series.IndexOf(existing);
                if (currentIndex != index)
                {
                    Series.Move(currentIndex, index);
                }
            }
        }

        UpdateYAxisLimits(Series);
        UpdateXAxisLimits(Series);
    }

    private static void SyncPoints(ObservableCollection<DateTimePoint> target, IReadOnlyList<DateTimePoint> source)
    {
        while (target.Count > source.Count)
        {
            target.RemoveAt(target.Count - 1);
        }

        for (var i = 0; i < source.Count; i++)
        {
            if (i < target.Count)
            {
                target[i] = source[i];
            }
            else
            {
                target.Add(source[i]);
            }
        }
    }

    private void UpdateYAxisLimits(IEnumerable<ISeries> series)
    {
        var values = series
            .OfType<LineSeries<DateTimePoint>>()
            .SelectMany(x => x.Values?.OfType<DateTimePoint>() ?? [])
            .Where(x => x.Value.HasValue)
            .Select(x => x.Value!.Value)
            .Where(x => !double.IsNaN(x) && !double.IsInfinity(x))
            .ToArray();

        if (values.Length == 0)
        {
            _yAxis.MinLimit = null;
            _yAxis.MaxLimit = null;
            return;
        }

        var min = values.Min();
        var max = values.Max();

        if (Math.Abs(max - min) < double.Epsilon)
        {
            var delta = Math.Max(Math.Abs(max) * 0.1, 0.01);
            _yAxis.MinLimit = min - delta;
            _yAxis.MaxLimit = max + delta;
            return;
        }

        var padding = Math.Max((max - min) * 0.15, Math.Abs(max) * 0.02);
        _yAxis.MinLimit = min - padding;
        _yAxis.MaxLimit = max + padding;
    }

    private void UpdateXAxisLimits(IEnumerable<ISeries> series)
    {
        var timestamps = series
            .OfType<LineSeries<DateTimePoint>>()
            .SelectMany(x => x.Values?.OfType<DateTimePoint>() ?? [])
            .Select(x => x.DateTime)
            .Where(x => x != default)
            .Order()
            .ToArray();

        if (timestamps.Length == 0)
        {
            _xAxis.MinLimit = null;
            _xAxis.MaxLimit = null;
            return;
        }

        var min = timestamps[0];
        var max = timestamps[^1];
        var span = max - min;

        if (span < s_defaultTimeWindow)
        {
            min = max - s_defaultTimeWindow;
        }

        _xAxis.MinLimit = min.Ticks;
        _xAxis.MaxLimit = max.Ticks;
    }

    private static SolidColorPaint CreateChartTextPaint()
    {
        var color = Application.Current?.ActualThemeVariant == ThemeVariant.Light
            ? SKColors.Black
            : SKColors.White;

        return new SolidColorPaint(color)
        {
            SKTypeface = SKTypeface.FromFamilyName("Inter"),
        };
    }
}

public sealed partial class MetricSummaryItemViewModel : ObservableObject
{
    public required string Key { get; init; }

    [ObservableProperty]
    public partial string Value { get; set; } = string.Empty;
}

internal sealed record MetricTabSnapshot(string Title, Icon Icon, IReadOnlyList<MetricPanelSnapshot> Panels);

internal sealed record MetricPanelSnapshot(string Title, IReadOnlyList<MetricSeriesSnapshot> Series);

internal sealed record MetricSeriesSnapshot(string Name, IReadOnlyList<DateTimePoint> Points);

internal sealed class MetricsServerHistoryState(string resourceKey)
{
    private static readonly TimeSpan s_historyDuration = TimeSpan.FromHours(1);

    public string ResourceKey { get; } = resourceKey;

    public List<MetricsServerSamplePoint> Samples { get; } = [];

    public void Upsert(DateTimeOffset timestamp, MetricsServerSample sample)
    {
        var cutoff = timestamp - s_historyDuration;
        Samples.RemoveAll(x => x.Timestamp < cutoff);

        var existing = Samples.FindIndex(x => x.Timestamp == timestamp);
        var point = new MetricsServerSamplePoint(timestamp, sample.Cpu, sample.Memory);

        if (existing >= 0)
        {
            Samples[existing] = point;
            return;
        }

        Samples.Add(point);
        Samples.Sort(static (a, b) => a.Timestamp.CompareTo(b.Timestamp));
    }
}

internal readonly record struct MetricsServerSample(decimal Cpu, double Memory);

internal readonly record struct MetricsServerSamplePoint(DateTimeOffset Timestamp, decimal Cpu, double Memory);
