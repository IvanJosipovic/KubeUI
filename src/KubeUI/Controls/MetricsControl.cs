using Avalonia.Controls.Primitives;
using FluentAvalonia.Core;
using k8s;
using k8s.Models;
using KubeUI.Client;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Avalonia;

namespace KubeUI.Controls;

public sealed class MetricsControl : ViewBase
{
    public static readonly DirectProperty<MetricsControl, ICluster> ClusterProperty =
        AvaloniaProperty.RegisterDirect<MetricsControl, ICluster>(
        nameof(Cluster),
        o => o.Cluster,
        (o, v) => o.Cluster = v);

    private ICluster _cluster;

    public ICluster Cluster
    {
        get { return _cluster; }
        set { SetAndRaise(ClusterProperty, ref _cluster, value); }
    }

    public static readonly DirectProperty<MetricsControl, IKubernetesObject<V1ObjectMeta>> ResourceProperty =
        AvaloniaProperty.RegisterDirect<MetricsControl, IKubernetesObject<V1ObjectMeta>>(
    nameof(Resource),
    o => o.Resource,
    (o, v) => o.Resource = v);

    private IKubernetesObject<V1ObjectMeta> _resource;

    public IKubernetesObject<V1ObjectMeta> Resource
    {
        get { return _resource; }
        set { SetAndRaise(ResourceProperty, ref _resource, value); }
    }

    public static readonly DirectProperty<MetricsControl, ISeries[]> SeriesProperty =
        AvaloniaProperty.RegisterDirect<MetricsControl, ISeries[]>(
        nameof(Series),
        o => o.Series,
        (o, v) => o.Series = v);

    private ISeries[] _series = [];

    public ISeries[] Series
    {
        get { return _series; }
        set { SetAndRaise(SeriesProperty, ref _series, value); }
    }

    private static readonly DispatcherTimer s_timer = new(DispatcherPriority.Default);

    public MetricsControl()
    {
        s_timer.Tick += Timer_Tick;
        s_timer.Interval = TimeSpan.FromSeconds(30);
        s_timer.Start();
    }

    public ICartesianAxis[] XAxes { get; set; } = [
        new DateTimeAxis(TimeSpan.FromMinutes(1), date => date.ToString("hh:mm")),
    ];

    private async void Timer_Tick(object? sender, EventArgs e)
    {
        await GenerateSeries();
    }

    protected override void OnLoaded(RoutedEventArgs e)
    {
        base.OnLoaded(e);

        Dispatcher.UIThread.Post(async () => await GenerateSeries());
    }

    private async Task GenerateSeries()
    {
        var rate = "5m";
        var from = DateTimeOffset.Now.Add(-TimeSpan.FromDays(1));
        var to = DateTimeOffset.Now;

        switch (_cluster.MetricsService.MetricsServiceType)
        {
            case Client.Metrics.MetricsServiceType.None:
                break;
            case Client.Metrics.MetricsServiceType.KubernetesMetricsServer:
                break;
            case Client.Metrics.MetricsServiceType.Prometheus:
                if (Resource is V1Pod pod)
                {
                    var podCpuUsage = $$$"""sum(rate(container_cpu_usage_seconds_total{container != "POD",container != "",pod=~"{{{pod.Name()}}}",namespace="{{{pod.Namespace()}}}"}[1m]))""";
                    var podCpuRequests = $$$"""sum(kube_pod_container_resource_requests{pod=~"{{{pod.Name()}}}",resource="cpu",namespace="{{{pod.Namespace()}}}"})""";
                    var podCpuLimits = $$$"""sum(kube_pod_container_resource_limits{pod=~"{{{pod.Name()}}}",resource="cpu",namespace="{{{pod.Namespace()}}}"})""";

                    var podMemoryUsage = $$$"""sum(container_memory_working_set_bytes{container!="POD",container!="",pod=~"{{{pod.Name()}}}",namespace="{{{pod.Namespace()}}}"})""";
                    var podMemoryRequests = $$$"""sum(kube_pod_container_resource_requests{pod=~"{{{pod.Name()}}}",resource="memory",namespace="{{{pod.Namespace()}}}"})""";
                    var podMemoryLimits = $$$"""sum(kube_pod_container_resource_limits{pod=~"{{{pod.Name()}}}",resource="memory",namespace="{{{pod.Namespace()}}}"})""";

                    MergeSeries(await Task.WhenAll(
                        GeneratePromethes("Usage", podCpuUsage, from, to, rate),
                        GeneratePromethes("Requests", podCpuRequests, from, to, rate),
                        GeneratePromethes("Limits", podCpuLimits, from, to, rate)
                    ));

                    //MergeSeries(await Task.WhenAll(
                    //    GeneratePromethes("Usage", podMemoryUsage, from, to, rate),
                    //    GeneratePromethes("Requests", podMemoryRequests, from, to, rate),
                    //    GeneratePromethes("Limits", podMemoryLimits, from, to, rate)
                    //));
                }
                break;
            case Client.Metrics.MetricsServiceType.AzureManagedPrometheus:
                break;
            default:
                break;
        }
    }

    private void MergeSeries(ISeries[] series)
    {
        foreach (var item in series)
        {
            if (item.Values.Count() == 0)
            {
                continue;
            }

            var existing = Series?.FirstOrDefault(x => x.Name == item.Name);

            if (existing != null)
            {
                existing.Values = item.Values;
            }
            else
            {
                Series = [.. Series, item];
            }
        }
    }

    private async Task<ISeries> GeneratePromethes(string name, string query, DateTimeOffset from, DateTimeOffset to, string rate)
    {
        var data1 = await _cluster.MetricsService.GetPrometheusMetics(query, from, to, rate);

        if (data1.Status != "success")
        {
            throw new Exception(data1.Error);
        }

        var data = new StackedStepAreaSeries<DateTimePoint>
        {
            Name = name,
            Values = data1.Data.Result.FirstOrDefault()?.Values.Select(x => new DateTimePoint(x.Item1.DateTime, x.Item2)).ToList() ?? [],
        };

        return data;
    }

    protected override StyleGroup? BuildStyles() => [];

    protected override object Build() =>
        new Grid()
            .HorizontalAlignment(HorizontalAlignment.Stretch)
            .VerticalAlignment(VerticalAlignment.Stretch)
            .Rows("Auto,*")
            .Children([
                new StackPanel()
                    .Row(0)
                    .Height(30)
                    .Orientation(Orientation.Horizontal)
                    .Children([
                        new ToggleButton()
                            .IsChecked(true)
                            .ToolTip("CPU")
                            .Content(new PathIcon() { Data = (Geometry)Application.Current.FindResource("arrow_undo_regular") }),
                        new ToggleButton()
                            .IsChecked(true)
                            .ToolTip("Memory")
                            .Content(new PathIcon() { Data = (Geometry)Application.Current.FindResource("arrow_undo_regular") }),
                        new ToggleButton()
                            .IsChecked(true)
                            .ToolTip("FileSystem")
                            .Content(new PathIcon() { Data = (Geometry)Application.Current.FindResource("arrow_undo_regular") }),
                        new ToggleButton()
                            .IsChecked(true)
                            .ToolTip("Network")
                            .Content(new PathIcon() { Data = (Geometry)Application.Current.FindResource("arrow_undo_regular") }),
                    ]),
                new CartesianChart()
                    .Row(1)
                    .Height(200)
                    .Series(@SeriesProperty)
                    .XAxes(@XAxes)
                ]);

    protected override void OnUnloaded(RoutedEventArgs e)
    {
        base.OnUnloaded(e);

        s_timer.Tick -= Timer_Tick;
    }
}
