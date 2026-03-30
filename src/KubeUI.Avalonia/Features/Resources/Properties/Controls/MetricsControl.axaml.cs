using KubeUI.Avalonia.Features.Clusters.Workspace.ViewModels;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Kubernetes;
using Avalonia.Styling;
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

namespace KubeUI.Avalonia.Features.Resources.Properties.Controls;

public sealed partial class MetricsControl : UserControl, IInitializeCluster
{
    private static readonly DispatcherTimer s_timer = new(DispatcherPriority.Default);
    private ClusterWorkspaceViewModel? _cluster;

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
        _ = RefreshAsync();
    }

    protected override void OnAttachedToVisualTree(VisualTreeAttachmentEventArgs e)
    {
        base.OnAttachedToVisualTree(e);
        s_timer.Tick += Timer_Tick;
        _ = RefreshAsync();
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
        _ = RefreshAsync();
    }

    private async void Timer_Tick(object? sender, EventArgs e)
    {
        await RefreshAsync();
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
        _ = RefreshAsync();
    }

    private void OnClusterPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName is nameof(ClusterWorkspaceViewModel.ActiveMetricsBackend)
            or nameof(ClusterWorkspaceViewModel.MetricsServiceType)
            or nameof(ClusterWorkspaceViewModel.ActivePrometheusProviderKind)
            or nameof(ClusterWorkspaceViewModel.IsMetricsAvailable))
        {
            _ = RefreshAsync();
        }
    }

    private async Task RefreshAsync()
    {
        if (_cluster == null || DataContext is not IKubernetesObject<V1ObjectMeta> resource)
        {
            IsVisible = false;
            Tabs.Clear();
            CurrentMetrics.Clear();
            SelectedTab = null;
            return;
        }

        IsVisible = true;

        if (_cluster.MetricsServiceType == MetricsServiceType.KubernetesMetricsServer)
        {
            await Dispatcher.UIThread.InvokeAsync(() =>
            {
                Tabs.Clear();
                SelectedTab = null;
                MergeCurrentMetrics(CreateCurrentMetrics(resource));
                ShowTabs = false;
                ShowCurrentMetrics = CurrentMetrics.Count > 0;
                ShowStatus = !ShowCurrentMetrics;
                StatusText = ShowCurrentMetrics
                    ? "Current metrics from Kubernetes Metrics Server."
                    : "Current metrics are unavailable for this resource while the cluster is using Kubernetes Metrics Server.";
            });
            return;
        }

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
                if (series.Length == 0)
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

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
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
        });
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

    private static ISeries[] CreateSeries(MetricPanelDefinition panel, MetricResultSet result)
    {
        if (result.IsEmpty)
        {
            return [];
        }

        var series = new List<ISeries>();

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

                series.Add(new LineSeries<DateTimePoint>
                {
                    Name = legend,
                    GeometrySize = 0,
                    Values = item.Points.Select(static value => new DateTimePoint(value.Timestamp.LocalDateTime, value.Value)).ToList(),
                });
            }
        }

        return [.. series];
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
                    Series = snapshot.Series,
                };
                Panels.Insert(index, existing);
            }
            else
            {
                existing.Series = snapshot.Series;
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
    public required string Title { get; init; }

    [ObservableProperty]
    public partial ISeries[] Series { get; set; }

    public ICartesianAxis[] XAxes { get; } =
    [
        new DateTimeAxis(TimeSpan.FromMinutes(10), value => value.ToString("HH:mm"))
        {
            LabelsPaint = CreateChartTextPaint(),
            TextSize = 11,
        },
    ];

    public ICartesianAxis[] YAxes { get; } =
    [
        new Axis
        {
            LabelsPaint = CreateChartTextPaint(),
            TextSize = 11,
        },
    ];

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

internal sealed record MetricPanelSnapshot(string Title, ISeries[] Series);
