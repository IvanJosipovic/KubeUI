using LiveChartsCore.SkiaSharpView.Extensions;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using LiveChartsCore.SkiaSharpView.VisualElements;
using k8s.Models;

namespace KubeUI.ViewModels;

public sealed partial class ClusterViewModel : ViewModelBase, IInitalizeCluster, IDisposable
{
    [ObservableProperty]
    private Client.Cluster _cluster;

    [ObservableProperty]
    private SolidColorPaint _color = new(SKColors.White);

    private DispatcherTimer _timer = new DispatcherTimer();

    public ClusterViewModel()
    {
        Title = Resources.ClusterViewModel_Title;
    }

    [ObservableProperty]
    public IEnumerable<ISeries> _cpu =
    GaugeGenerator.BuildSolidGauge(
        new GaugeItem(30, series => SetStyle("Usage", series)),
        new GaugeItem(50, series => SetStyle("Requests", series)),
        new GaugeItem(80, series => SetStyle("Limits", series)),
        new GaugeItem(20, series => SetStyle("Allocatable Capacity", series)),
        new GaugeItem(40, series => SetStyle("Capacity", series)),
        new GaugeItem(GaugeItem.Background, series =>
        {
            series.InnerRadius = 20;
        }));

    [ObservableProperty]
    private IEnumerable<ISeries> _memory=
    GaugeGenerator.BuildSolidGauge(
        new GaugeItem(30, series => SetStyle("Usage", series)),
        new GaugeItem(50, series => SetStyle("Requests", series)),
        new GaugeItem(80, series => SetStyle("Limits", series)),
        new GaugeItem(20, series => SetStyle("Allocatable Capacity", series)),
        new GaugeItem(40, series => SetStyle("Capacity", series)),
        new GaugeItem(GaugeItem.Background, series =>
        {
            series.InnerRadius = 20;
        }));

    [ObservableProperty]
    private IEnumerable<ISeries> _pods;

    [ObservableProperty]
    private int _maxPods;

    public static void SetStyle(string name, PieSeries<ObservableValue> series)
    {
        series.Name = name;
        series.DataLabelsPosition = PolarLabelsPosition.Start;
        series.DataLabelsFormatter = point => $"{point.Coordinate.PrimaryValue}";
        series.InnerRadius = 20;
        series.RelativeOuterRadius = 8;
        series.RelativeInnerRadius = 8;
    }

    public LabelVisual CPUTitle { get; set; } =
    new LabelVisual
    {
        Text = "CPU",
        TextSize = 25,
        Paint = new SolidColorPaint(SKColors.White)
    };

    public LabelVisual MemoryTitle { get; set; } =
    new LabelVisual
    {
        Text = "Memory",
        TextSize = 25,
        Paint = new SolidColorPaint(SKColors.White)
    };

    public LabelVisual PodsTitle { get; set; } =
    new LabelVisual
    {
        Text = "Pods",
        TextSize = 25,
        Paint = new SolidColorPaint(SKColors.White)
    };

    private void PopulateData()
    {
        var pods = Cluster.GetObjectDictionary<V1Pod>();
        var nodes = Cluster.GetObjectDictionary<V1Node>();

        var totalPods = pods.Count;
        MaxPods = nodes.Sum(x => x.Value.Status.Capacity["pods"].ToInt32());

        Pods = GaugeGenerator.BuildSolidGauge(
        new GaugeItem(totalPods, series => SetStyle("Usage", series)),
        new GaugeItem(GaugeItem.Background, series =>
        {
            series.InnerRadius = 20;
        }));
    }

    public void Initialize(Client.Cluster cluster)
    {
        Cluster = cluster;
        PopulateData();

        Title = Resources.ClusterViewModel_Title;
        Id = nameof(ClusterViewModel) + "-" + Cluster.Name + "-" + Title;

        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick += S_timer_Tick;
        _timer.Start();
    }

    private void S_timer_Tick(object? sender, EventArgs e)
    {
        PopulateData();
    }

    public void Dispose()
    {
        _timer.Tick -= S_timer_Tick;
        _timer.Stop();
    }
}
