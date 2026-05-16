using Avalonia.Styling;
using KubeUI.Avalonia.Infrastructure;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView;
using LiveChartsCore.SkiaSharpView.Avalonia;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.VisualElements;
using LiveChartsCore.Themes;
using SkiaSharp;

namespace KubeUI.Avalonia.Features.Clusters.Overview.ViewModels;

public sealed partial class ClusterOverviewChartPanelViewModel : ObservableObject, IDisposable
{
    private static readonly TimeSpan s_defaultTimeWindow = TimeSpan.FromHours(1);
    private const float s_lineStrokeThickness = 2f;
    private readonly Axis _yAxis = new()
    {
        LabelsPaint = CreateChartTextPaint(),
        TextSize = 11,
    };
    private readonly Axis _xAxis;

    public ClusterOverviewChartPanelViewModel()
    {
        ChartTitle = new XamlDrawnLabelVisual
        {
            TextSize = 11,
            Paint = CreateChartTextPaint(),
        };

        _xAxis = new DateTimeAxis(TimeSpan.FromMinutes(10), value => value.ToString("HH:mm"))
        {
            LabelsPaint = CreateChartTextPaint(),
            TextSize = 11,
        };

        XAxes = [_xAxis];
        YAxes = [_yAxis];
    }

    [ObservableProperty]
    public partial string Title { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string EmptyText { get; set; } = "No chart data available.";

    [ObservableProperty]
    public partial bool HasData { get; set; }

    public bool HasNoData => !HasData;

#pragma warning disable CS0618
    public XamlDrawnLabelVisual ChartTitle { get; }
#pragma warning restore CS0618

    public ObservableCollection<ISeries> Series { get; } = [];

    public ICartesianAxis[] XAxes { get; }

    public ICartesianAxis[] YAxes { get; }

    public void SetSeries(IReadOnlyList<ClusterOverviewMetricSeries> snapshots)
    {
        ChartTitle.Text = Title;

        for (var i = Series.Count - 1; i >= 0; i--)
        {
            if (Series[i] is not LineSeries<DateTimePoint> line
                || !snapshots.Any(x => string.Equals(x.Name, line.Name, StringComparison.Ordinal)))
            {
                (Series[i] as IDisposable)?.Dispose();
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
                ApplyLineSeriesStyle(existing, index);
                Series.Insert(index, existing);
            }
            else
            {
                existing.Values = new ObservableCollection<DateTimePoint>(snapshot.Points);
                ApplyLineSeriesStyle(existing, index);
                var currentIndex = Series.IndexOf(existing);
                if (currentIndex != index)
                {
                    Series.Move(currentIndex, index);
                }
            }
        }

        HasData = snapshots.Count > 0;
        OnPropertyChanged(nameof(HasNoData));
        UpdateYAxisLimits();
        UpdateXAxisLimits();
    }

    public void Dispose()
    {
        DisposePaint(_xAxis.LabelsPaint);
        DisposePaint(_yAxis.LabelsPaint);

        foreach (var series in Series.OfType<IDisposable>().ToArray())
        {
            series.Dispose();
        }

        Series.Clear();
    }

    private void UpdateYAxisLimits()
    {
        var values = Series
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

    private void UpdateXAxisLimits()
    {
        var timestamps = Series
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

    private static void ApplyLineSeriesStyle(LineSeries<DateTimePoint> series, int index)
    {
        var palette = Application.Current?.ActualThemeVariant == ThemeVariant.Light
            ? ColorPalletes.MaterialDesign500
            : ColorPalletes.MaterialDesign200;
        var color = palette[index % palette.Length].AsSKColor();

        series.Stroke = new SolidColorPaint(color, s_lineStrokeThickness);
        series.Fill = null;
    }

    private static void DisposePaint(object? paint)
    {
        if (paint is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
