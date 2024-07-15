using Avalonia.Markup.Declarative;
using KubeUI.ViewModels;
using LiveChartsCore.Defaults;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Avalonia;
using LiveChartsCore.SkiaSharpView.Extensions;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.VisualElements;
using SkiaSharp;

namespace KubeUI.Views;

public class ClusterView : MyViewBase<ClusterViewModel>, IDisposable
{
    public ClusterView()
    {
        _timer.Interval = TimeSpan.FromSeconds(1);
        _timer.Tick += S_timer_Tick;
        _timer.Start();
    }

    private void S_timer_Tick(object? sender, EventArgs e)
    {
        if (ViewModel != null)
        {
            Dispatcher.UIThread.Invoke(ViewModel.RefreshData);
        }
    }

    private static SolidColorPaint _color = new(SKColors.White);

    private DispatcherTimer _timer = new();

    public static void SetSeries(string name, PieSeries<ObservableValue> series)
    {
        series.Name = name;
        series.DataLabelsPosition = PolarLabelsPosition.Start;
        series.DataLabelsFormatter = point => $"{point.Coordinate.PrimaryValue}";
        series.DataLabelsPaint = _color;
        series.InnerRadius = 20;
        series.RelativeOuterRadius = 8;
        series.RelativeInnerRadius = 8;
    }

    public static LabelVisual SetTitle(string name) => new LabelVisual()
    {
        Paint = _color,
        Text = name,
        TextSize = 25,
    };

    protected override StyleGroup? BuildStyles() =>
    [
        new Style<PieChart>()
            .Setter(PieChart.InitialRotationProperty, -90.0)
            .Setter(PieChart.LegendPositionProperty, LegendPosition.Bottom)
            .Setter(PieChart.LegendTextPaintProperty, _color)
            .Setter(PieChart.MaxAngleProperty, 270.0)
    ];

    protected override object Build(ClusterViewModel? vm) =>
        new Grid()
        .Cols("*,*,*").Rows("*,2*")
        .Children(
            new PieChart()
                .Row(0).Col(0)
                .SetProp(PieChart.TitleProperty, SetTitle("CPU"))
                .SetProp(PieChart.SeriesProperty, GaugeGenerator.BuildSolidGauge(
                    new GaugeItem(vm.CpuCapacity, series => {
                        SetSeries("Capacity", series);
                        series.DataLabelsFormatter = point => $"{point.Coordinate.PrimaryValue:F2}c";
                    }),
                    new GaugeItem(vm.CpuAllocatable, series => {
                        SetSeries("Allocatable", series);
                        series.DataLabelsFormatter = point => $"{point.Coordinate.PrimaryValue:F2}c";
                    })
                    )
                ),

            new PieChart()
                .Row(0).Col(1)
                .SetProp(PieChart.TitleProperty, SetTitle("Memory"))
                .SetProp(PieChart.SeriesProperty, GaugeGenerator.BuildSolidGauge(
                    new GaugeItem(vm.MemoryCapacity, series => {
                        SetSeries("Capacity", series);
                        series.DataLabelsFormatter = point => $"{point.Coordinate.PrimaryValue:F1}Gi";
                    }),
                    new GaugeItem(vm.MemoryAllocatable, series => {
                        SetSeries("Allocatable", series);
                        series.DataLabelsFormatter = point => $"{point.Coordinate.PrimaryValue:F1}Gi";
                    })
                    )
                ),

            new PieChart()
                .Row(0).Col(2)
                .SetProp(PieChart.TitleProperty, SetTitle("Pods"))
                .SetProp(PieChart.SeriesProperty, GaugeGenerator.BuildSolidGauge(
                    new GaugeItem(vm.MaxPods, series => SetSeries("Capacity", series)),
                    new GaugeItem(vm.TotalPods, series => SetSeries("Pods", series))
                    )
                ),

            new Grid()
                .Row(1).ColSpan(3)
                .Rows("Auto,*")
                .Children(
                    new TextBlock()
                        .Row(0)
                        .Text("Events")
                        .FontSize(25),
                    new ResourceListView()
                        .Row(1)
                        .SetProp(DataContextProperty, vm.EventsVM)
                )

        );

    public void Dispose()
    {
        _timer.Stop();
        _timer.Tick -= S_timer_Tick;
    }
}
