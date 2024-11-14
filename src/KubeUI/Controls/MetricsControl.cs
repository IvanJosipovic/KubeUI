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

    private ISeries[] _series;

    public ISeries[] Series
    {
        get { return _series; }
        set { SetAndRaise(SeriesProperty, ref _series, value); }
    }

    private static readonly DispatcherTimer s_timer = new(DispatcherPriority.Default);

    public MetricsControl()
    {
        s_timer.Tick += Timer_Tick;
        s_timer.Interval = TimeSpan.FromSeconds(1);
        s_timer.Start();

        Series =
        [
            //new StackedStepAreaSeries<double>([3, 2, 3, 5, 3, 4, 6]){
            // Name = "CPU",

            //}
        new StackedStepAreaSeries<DateTimePoint>
        {
            Values = [
                new() { DateTime = new(2021, 1, 1), Value = 3 },
                new() { DateTime = new(2021, 1, 2), Value = 6 },
                new() { DateTime = new(2021, 1, 3), Value = 5 },
                new() { DateTime = new(2021, 1, 4), Value = 3 },
                new() { DateTime = new(2021, 1, 5), Value = 5 },
                new() { DateTime = new(2021, 1, 6), Value = 8 },
                new() { DateTime = new(2021, 1, 7), Value = 6 }
            ]
        }
        ];
    }

    public ICartesianAxis[] XAxes { get; set; } = [
        new DateTimeAxis(TimeSpan.FromMinutes(1), date => date.ToString("mm"))
    ];

    private async void Timer_Tick(object? sender, EventArgs e)
    {
        switch (_cluster.MetricsService.MetricsServiceType)
        {
            case Client.Metrics.MetricsServiceType.None:
                break;
            case Client.Metrics.MetricsServiceType.KubernetesMetricsServer:
                break;
            case Client.Metrics.MetricsServiceType.Prometheus:
                if (Resource is V1Pod)
                {
                    var data1 =  await _cluster.MetricsService.GetPrometheusMetics("sum(rate(node_cpu_seconds_total{mode=~\"user|system\"}[5m])) by(node)", DateTimeOffset.Now.Add(-TimeSpan.FromHours(1)), DateTimeOffset.Now);

                    if (data1.Status != "success")
                    {
                        throw new Exception(data1.Error);
                    }

                    var data = new StackedStepAreaSeries<DateTimePoint>
                    {
                        Name = "CPU",
                        Values = data1.Data.Result.First().Values.Select(x => new DateTimePoint(x.Item1.DateTime, x.Item2)).ToList()
                    };

                    Series = [data];
                }

                break;
            case Client.Metrics.MetricsServiceType.AzureManagedPrometheus:
                break;
            default:
                break;
        }
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
                    .Orientation(Orientation.Horizontal)
                    .Children([
                        new Label()
                            .Margin(0,0,4,0)
                            .Content("CPU: "),
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
