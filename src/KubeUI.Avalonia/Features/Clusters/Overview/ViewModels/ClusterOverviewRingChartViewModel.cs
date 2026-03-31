using Avalonia;
using Avalonia.Styling;
using KubeUI.Avalonia.Infrastructure;
using LiveChartsCore;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.SkiaSharpView.Avalonia;
using LiveChartsCore.SkiaSharpView.Painting;
using SkiaSharp;
using System.Collections.ObjectModel;

namespace KubeUI.Avalonia.Features.Clusters.Overview.ViewModels;

public sealed partial class ClusterOverviewRingChartViewModel : ObservableObject, IDisposable
{
    private static readonly string[] s_defaultColors = ["#C94DD1", "#50BF59", "#49A6E9", "#3A4047"];

    [ObservableProperty]
    public partial string Title { get; set; } = string.Empty;

    [ObservableProperty]
    public partial string EmptyText { get; set; } = "No chart data available.";

    [ObservableProperty]
    public partial bool HasData { get; set; }

    public bool HasNoData => !HasData;

    [ObservableProperty]
    public partial double MaxValue { get; set; } = 1;

[ObservableProperty]
public partial ObservableCollection<ISeries> Series { get; set; } = [];

public ObservableCollection<ClusterOverviewRingChartLegendItemViewModel> LegendItems { get; } = [];

    public void SetSeries(
        IReadOnlyList<ClusterOverviewMetricSeries> snapshots,
        Func<double, string>? valueFormatter = null,
        IReadOnlyList<ClusterOverviewMetricSeries>? legendSnapshots = null,
        int innerRadius = 20,
        double maxRadialColumnWidth = 24)
    {
        ObservableCollection<ISeries> series = [];
        LegendItems.Clear();
        double maxValue = 0;
        double capacityValue = 0;
        IReadOnlyList<ClusterOverviewMetricSeries> legends = legendSnapshots ?? snapshots;

        foreach (var snapshot in snapshots)
        {
            double value = 0;
            if (snapshot.Points.Count > 0)
            {
                value = snapshot.Points[^1].Value ?? 0;
            }

            if (value > maxValue)
            {
                maxValue = value;
            }

            if (string.Equals(snapshot.Name, "Capacity", StringComparison.Ordinal)
                || string.Equals(snapshot.Name, "Allocatable Capacity", StringComparison.Ordinal))
            {
                capacityValue = Math.Max(capacityValue, value);
            }

            string formattedValue = valueFormatter?.Invoke(value) ?? value.ToString("F0");
            XamlGaugeSeries slice = new()
            {
                SeriesName = snapshot.Name,
                GaugeValue = Math.Max(0, value),
                InnerRadius = innerRadius,
                MaxRadialColumnWidth = maxRadialColumnWidth,
                IsHoverable = true,
                HoverPushout = 10,
                ShowDataLabels = false,
                DataLabelsPaint = new SolidColorPaint(SKColors.Transparent),
                ToolTipLabelFormatter = _ => $"{snapshot.Name}: {formattedValue}",
                Fill = new SolidColorPaint(SKColor.Parse(s_defaultColors[series.Count % s_defaultColors.Length])),
            };
            series.Add(slice);
        }

        for (int i = 0; i < legends.Count; i++)
        {
            ClusterOverviewMetricSeries snapshot = legends[i];
            double value = 0;
            if (snapshot.Points.Count > 0)
            {
                value = snapshot.Points[^1].Value ?? 0;
            }

            if (string.Equals(snapshot.Name, "Capacity", StringComparison.Ordinal)
                || string.Equals(snapshot.Name, "Allocatable Capacity", StringComparison.Ordinal))
            {
                capacityValue = Math.Max(capacityValue, value);
            }

            string formattedValue = valueFormatter?.Invoke(value) ?? value.ToString("F0");
            LegendItems.Add(new ClusterOverviewRingChartLegendItemViewModel(snapshot.Name, formattedValue, s_defaultColors[i % s_defaultColors.Length]));
        }

        if (series.Count > 0)
        {
            series.Add(new XamlGaugeBackgroundSeries
            {
                InnerRadius = innerRadius,
                MaxRadialColumnWidth = maxRadialColumnWidth,
                IsHoverable = false,
            });
        }

        foreach (var item in Series)
        {
            (item as IDisposable)?.Dispose();
        }

        Series = series;
        MaxValue = capacityValue > 0 ? capacityValue : maxValue > 0 ? maxValue : 1;
        HasData = snapshots.Count > 0;
        OnPropertyChanged(nameof(HasNoData));
    }

    public static double ClampToFrame(double value, double frame)
    {
        if (frame <= 0)
        {
            return Math.Max(0, value);
        }

        return Math.Max(0, Math.Min(value, frame));
    }

    public void Dispose()
    {
        foreach (var series in Series)
        {
            (series as IDisposable)?.Dispose();
        }

        Series.Clear();
    }

}

public sealed partial class ClusterOverviewRingChartLegendItemViewModel : ObservableObject
{
    public ClusterOverviewRingChartLegendItemViewModel(string name, string value, string color)
    {
        Name = name;
        Value = value;
        Color = color;
    }

    [ObservableProperty]
    public partial string Name { get; set; }

    [ObservableProperty]
    public partial string Value { get; set; }

    [ObservableProperty]
    public partial string Color { get; set; }
}
