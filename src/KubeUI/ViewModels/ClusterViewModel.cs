using LiveChartsCore.SkiaSharpView.Extensions;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;

namespace KubeUI.ViewModels;

public sealed partial class ClusterViewModel : ViewModelBase
{
    [ObservableProperty]
    private Client.Cluster _cluster;

    public ClusterViewModel()
    {
        Title = Resources.ClusterViewModel_Title;
    }

    public IEnumerable<ISeries> CPU { get; set; } =
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

    public IEnumerable<ISeries> Memory { get; set; } =
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

    public IEnumerable<ISeries> Pods { get; set; } =
    GaugeGenerator.BuildSolidGauge(
        new GaugeItem(30, series => SetStyle("Usage", series)),
        new GaugeItem(GaugeItem.Background, series => SetStyle("Capacity", series)),
        new GaugeItem(GaugeItem.Background, series =>
        {
            series.InnerRadius = 20;
        }));

    public static void SetStyle(string name, PieSeries<ObservableValue> series)
    {
        series.Name = name;
        series.DataLabelsPosition = PolarLabelsPosition.Start;
        series.DataLabelsFormatter = point => $"{point.Coordinate.PrimaryValue}";
        series.InnerRadius = 20;
        series.RelativeOuterRadius = 8;
        series.RelativeInnerRadius = 8;
    }
}
