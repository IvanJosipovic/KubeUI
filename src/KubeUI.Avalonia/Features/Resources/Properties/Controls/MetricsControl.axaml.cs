using KubeUI.Avalonia.Features.Clusters.Workspace.ViewModels;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Kubernetes;
using k8s;
using k8s.Models;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using System.Text.RegularExpressions;

namespace KubeUI.Avalonia.Features.Resources.Properties.Controls;

public sealed partial class MetricsControl : UserControl, IInitializeCluster
{
    private static readonly DispatcherTimer s_timer = new(DispatcherPriority.Default);
    private ClusterWorkspaceViewModel? _cluster;

    [GeneratedDirectProperty]
    public partial ISeries[] Series { get; set; } = [];

    [GeneratedDirectProperty]
    public partial ICartesianAxis[] XAxes { get; set; } =
    [
        new DateTimeAxis(TimeSpan.FromMinutes(10), value => value.ToString("HH:mm")),
    ];

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
            Series = [];
            return;
        }

        if (_cluster.MetricsServiceType is not (MetricsServiceType.Prometheus or MetricsServiceType.PrometheusExternal))
        {
            IsVisible = false;
            Series = [];
            return;
        }

        if (resource is not V1Pod pod)
        {
            IsVisible = false;
            Series = [];
            return;
        }

        IsVisible = true;

        var from = DateTimeOffset.UtcNow.AddHours(-1);
        var to = DateTimeOffset.UtcNow;
        const string step = "5m";

        try
        {
            var podName = Regex.Escape(pod.Name());
            var podNamespace = pod.Namespace();

            var usageQuery = $$$"""sum(rate(container_cpu_usage_seconds_total{container!="POD",container!="",pod=~"{{{podName}}}",namespace="{{{podNamespace}}}"}[5m]))""";
            var requestsQuery = $$$"""sum(kube_pod_container_resource_requests{pod=~"{{{podName}}}",resource="cpu",namespace="{{{podNamespace}}}"})""";
            var limitsQuery = $$$"""sum(kube_pod_container_resource_limits{pod=~"{{{podName}}}",resource="cpu",namespace="{{{podNamespace}}}"})""";

            var series = await Task.WhenAll(
                CreateSeriesAsync("Usage", usageQuery, from, to, step),
                CreateSeriesAsync("Requests", requestsQuery, from, to, step),
                CreateSeriesAsync("Limits", limitsQuery, from, to, step));

            Series = series.Where(static s => s.Values?.Cast<object>().Any() == true).ToArray();
            IsVisible = Series.Length > 0;
        }
        catch
        {
            IsVisible = false;
            Series = [];
        }
    }

    private async Task<ISeries> CreateSeriesAsync(string name, string query, DateTimeOffset from, DateTimeOffset to, string step)
    {
        var result = await _cluster!.GetPrometheusMetrics(query, from, to, step);
        if (result == null || !string.Equals(result.Status, "success", StringComparison.Ordinal))
        {
            return new StackedStepAreaSeries<DateTimePoint>
            {
                Name = name,
                Values = [],
            };
        }

        return new StackedStepAreaSeries<DateTimePoint>
        {
            Name = name,
            Values = result.Data.Result.FirstOrDefault()?.Values
                .Select(value => new DateTimePoint(value.Timestamp.LocalDateTime, value.Value))
                .ToList() ?? [],
        };
    }
}
