using KubeUI.Avalonia.Features.Clusters.Workspace.ViewModels;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Kubernetes;
using k8s;
using k8s.Models;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;

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
        _cluster = cluster;
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

    private async Task RefreshAsync()
    {
        if (_cluster == null || DataContext is not IKubernetesObject<V1ObjectMeta> resource)
        {
            IsVisible = false;
            Tabs = [];
            return;
        }

        IsVisible = true;

        if (_cluster.MetricsServiceType == MetricsServiceType.KubernetesMetricsServer)
        {
            Tabs = [];
            ShowTabs = false;
            ShowStatus = true;
            StatusText = "Historical metrics are unavailable while the cluster is using Kubernetes Metrics Server.";
            return;
        }

        if (_cluster.MetricsServiceType != MetricsServiceType.Prometheus)
        {
            Tabs = [];
            ShowTabs = false;
            ShowStatus = true;
            StatusText = "Metrics are not available for this cluster.";
            return;
        }

        var descriptor = await ResourceMetricsCatalog.CreateAsync(_cluster, resource).ConfigureAwait(false);
        if (descriptor == null)
        {
            IsVisible = false;
            Tabs = [];
            return;
        }

        var tabs = new List<MetricTabViewModel>();

        foreach (var tab in descriptor.Tabs)
        {
            var panels = new List<MetricPanelViewModel>();

            foreach (var panel in tab.Panels)
            {
                var result = await _cluster.RequestMetricsAsync(panel.Request).ConfigureAwait(false);
                var series = CreateSeries(panel, result);
                if (series.Length == 0)
                {
                    continue;
                }

                panels.Add(new MetricPanelViewModel
                {
                    Title = panel.Title,
                    Series = series,
                });
            }

            if (panels.Count > 0)
            {
                tabs.Add(new MetricTabViewModel
                {
                    Title = tab.Title,
                    Panels = panels,
                });
            }
        }

        await Dispatcher.UIThread.InvokeAsync(() =>
        {
            Tabs = [.. tabs];
            ShowTabs = Tabs.Count > 0;
            ShowStatus = !ShowTabs;
            StatusText = ShowTabs ? null : descriptor.EmptyState ?? "No Prometheus metrics are available for this resource.";
        });
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

public sealed class MetricTabViewModel
{
    public required string Title { get; init; }

    public required IReadOnlyList<MetricPanelViewModel> Panels { get; init; }
}

public sealed class MetricPanelViewModel
{
    public required string Title { get; init; }

    public required ISeries[] Series { get; init; }

    public ICartesianAxis[] XAxes { get; } =
    [
        new DateTimeAxis(TimeSpan.FromMinutes(10), value => value.ToString("HH:mm")),
    ];
}
