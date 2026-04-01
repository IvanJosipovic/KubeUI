using Avalonia;
using Avalonia.Styling;
using KubeUI.Avalonia.Infrastructure;
using LiveChartsCore;
using LiveChartsCore.Defaults;
using LiveChartsCore.Kernel;
using LiveChartsCore.Kernel.Sketches;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView.Avalonia;
using LiveChartsCore.SkiaSharpView.Painting;
using LiveChartsCore.SkiaSharpView.VisualElements;
using SkiaSharp;
using System.Collections.ObjectModel;

namespace KubeUI.Avalonia.Features.Clusters.Overview.ViewModels;

public sealed partial class ClusterOverviewRingChartViewModel : ObservableObject, IDisposable
{
    private static readonly string[] s_defaultColors = ["#C94DD1", "#50BF59", "#49A6E9", "#3A4047"];

    [ObservableProperty]
    public partial string Title { get; set; } = string.Empty;

    [ObservableProperty]
    public partial double MaxValue { get; set; } = 1;

    [ObservableProperty]
    public partial ObservableCollection<ISeries> Series { get; set; } = [];

    [ObservableProperty]
    public partial bool HasData { get; set; }

    public bool HasNoData => !HasData;

    public ObservableCollection<IChartElement> VisualElements { get; } = [];

    public ObservableCollection<ClusterOverviewRingChartLegendItemViewModel> LegendItems { get; } = [];

    public void SetSeries(
        IReadOnlyList<ClusterOverviewMetricSeries> snapshots,
        Func<double, string>? valueFormatter = null,
        IReadOnlyList<ClusterOverviewMetricSeries>? legendSnapshots = null,
        int innerRadius = 20,
        double maxRadialColumnWidth = 24,
        bool useMax = false,
        string? title = null)
    {
        if (title != null)
        {
            Title = title;
            SyncTitleElement();
        }

        IReadOnlyList<ClusterOverviewMetricSeries> displayedSnapshots = snapshots.Count == 0
            ? [new ClusterOverviewMetricSeries(string.IsNullOrWhiteSpace(Title) ? "Usage" : Title, [new DateTimePoint(DateTime.Now, 0)])]
            : snapshots;
        IReadOnlyList<ClusterOverviewMetricSeries> legends = legendSnapshots ?? displayedSnapshots;
        ObservableCollection<ISeries> series = BuildSeries(displayedSnapshots, innerRadius, maxRadialColumnWidth, valueFormatter, out double maxValue, out double useMaxValue, out bool hasUseMaxSeries);
        if (legendSnapshots != null)
        {
            for (int i = 0; i < legendSnapshots.Count; i++)
            {
                ClusterOverviewMetricSeries snapshot = legendSnapshots[i];
                if (!snapshot.UseMax)
                {
                    continue;
                }

                hasUseMaxSeries = true;
                useMaxValue = Math.Max(useMaxValue, GetLatestValue(snapshot));
            }
        }

        SyncLegendItems(legends, valueFormatter);
        SyncSeries(series);
        HasData = snapshots.Count > 0;
        OnPropertyChanged(nameof(HasNoData));

        MaxValue = useMax || hasUseMaxSeries
            ? useMaxValue > 0 ? useMaxValue : maxValue > 0 ? maxValue : 1
            : maxValue > 0 ? maxValue : 1;
    }

    public static double ClampToFrame(double value, double frame)
    {
        if (frame <= 0)
        {
            return Math.Max(0, value);
        }

        return Math.Max(0, Math.Min(value, frame));
    }

    private static ObservableCollection<ISeries> BuildSeries(
        IReadOnlyList<ClusterOverviewMetricSeries> snapshots,
        int innerRadius,
        double maxRadialColumnWidth,
        Func<double, string>? valueFormatter,
        out double maxValue,
        out double useMaxValue,
        out bool hasUseMaxSeries)
    {
        ObservableCollection<ISeries> series = [];
        maxValue = 0;
        useMaxValue = 0;
        hasUseMaxSeries = false;

        foreach (var snapshot in snapshots)
        {
            double value = GetLatestValue(snapshot);

            maxValue = Math.Max(maxValue, value);
            if (snapshot.UseMax)
            {
                hasUseMaxSeries = true;
                useMaxValue = Math.Max(useMaxValue, value);
            }

            string formattedValue = valueFormatter?.Invoke(value) ?? value.ToString("F0");
            series.Add(new XamlGaugeSeries
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
            });
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

        return series;
    }

    private void SyncLegendItems(IReadOnlyList<ClusterOverviewMetricSeries> legends, Func<double, string>? valueFormatter)
    {
        if (legends.Count == 0)
        {
            LegendItems.Clear();
            return;
        }

        for (int i = 0; i < legends.Count; i++)
        {
            ClusterOverviewMetricSeries snapshot = legends[i];
            double value = GetLatestValue(snapshot);
            string formattedValue = valueFormatter?.Invoke(value) ?? value.ToString("F0");

            if (i < LegendItems.Count)
            {
                ClusterOverviewRingChartLegendItemViewModel item = LegendItems[i];
                item.Name = snapshot.Name;
                item.Value = formattedValue;
                item.Color = s_defaultColors[i % s_defaultColors.Length];
            }
            else
            {
                LegendItems.Add(new ClusterOverviewRingChartLegendItemViewModel(snapshot.Name, formattedValue, s_defaultColors[i % s_defaultColors.Length]));
            }
        }

        for (int i = LegendItems.Count - 1; i >= legends.Count; i--)
        {
            LegendItems.RemoveAt(i);
        }
    }

    private void SyncSeries(IReadOnlyList<ISeries> series)
    {
        for (int i = 0; i < series.Count; i++)
        {
            if (i < Series.Count)
            {
                if (Series[i] is XamlGaugeSeries currentGauge && series[i] is XamlGaugeSeries targetGauge)
                {
                    SyncGaugeSeries(currentGauge, targetGauge);
                    continue;
                }

                if (Series[i] is XamlGaugeBackgroundSeries currentBackground && series[i] is XamlGaugeBackgroundSeries targetBackground)
                {
                    SyncGaugeBackgroundSeries(currentBackground, targetBackground);
                    continue;
                }

                (Series[i] as IDisposable)?.Dispose();
                Series[i] = series[i];
                continue;
            }

            Series.Add(series[i]);
        }

        for (int i = Series.Count - 1; i >= series.Count; i--)
        {
            (Series[i] as IDisposable)?.Dispose();
            Series.RemoveAt(i);
        }
    }

    private static void SyncGaugeSeries(XamlGaugeSeries current, XamlGaugeSeries target)
    {
        current.SeriesName = target.SeriesName;
        current.GaugeValue = target.GaugeValue;
        current.InnerRadius = target.InnerRadius;
        current.MaxRadialColumnWidth = target.MaxRadialColumnWidth;
        current.IsHoverable = target.IsHoverable;
        current.HoverPushout = target.HoverPushout;
        current.ShowDataLabels = target.ShowDataLabels;
        current.DataLabelsPaint = target.DataLabelsPaint;
        current.ToolTipLabelFormatter = target.ToolTipLabelFormatter;
        current.Fill = target.Fill;
    }

    private static void SyncGaugeBackgroundSeries(XamlGaugeBackgroundSeries current, XamlGaugeBackgroundSeries target)
    {
        current.InnerRadius = target.InnerRadius;
        current.MaxRadialColumnWidth = target.MaxRadialColumnWidth;
        current.IsHoverable = target.IsHoverable;
    }

    private static double GetLatestValue(ClusterOverviewMetricSeries series)
    {
        if (series.Points.Count == 0)
        {
            return 0;
        }

        return series.Points[^1].Value ?? 0;
    }

    public void Dispose()
    {
        for (int i = Series.Count - 1; i >= 0; i--)
        {
            if (Series[i] is IDisposable disposable)
            {
                disposable.Dispose();
            }
            Series.RemoveAt(i);
        }

        LegendItems.Clear();
        VisualElements.Clear();
    }

    private void SyncTitleElement()
    {
        if (VisualElements.Count == 0)
        {
            VisualElements.Add(new XamlDrawnLabelVisual
            {
                Text = Title,
                TextSize = 11,
                Padding = new(0, 0, 0, 8),
                Paint = new SolidColorPaint(SKColors.White),
            });
            return;
        }

        if (VisualElements[0] is XamlDrawnLabelVisual label)
        {
            label.Text = Title;
        }
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
