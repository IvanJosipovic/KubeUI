using LiveChartsCore;
using LiveChartsCore.Drawing;
using LiveChartsCore.Drawing.Layouts;
using LiveChartsCore.Kernel;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView.Drawing;
using LiveChartsCore.SkiaSharpView.SKCharts;

namespace KubeUI.Avalonia.Features.Resources.Metrics.Controls;

internal sealed class NearestSeriesTooltip : SKDefaultTooltip
{
    private ChartPoint[] _foundPoints = [];
    private Chart? _chart;

    public override void Show(IEnumerable<ChartPoint> foundPoints, Chart chart)
    {
        _foundPoints = foundPoints.ToArray();
        _chart = chart;
        base.Show(_foundPoints, chart);
        UpdateAnchor(((ResponsiveCartesianChart)chart.View).PointerPosition);
    }

    public override void Hide(Chart chart)
    {
        _foundPoints = [];
        _chart = null;
        base.Hide(chart);
    }

    internal void UpdateAnchor(LvcPoint pointerPosition)
    {
        if (_chart is not Chart chart
            || _foundPoints.Length == 0
            || Content is not Layout<SkiaSharpDrawingContext> layout)
        {
            return;
        }

        ChartPoint? nearestPoint = null;
        var nearestDistance = double.PositiveInfinity;
        foreach (var point in _foundPoints)
        {
            var distance = point.DistanceTo(pointerPosition, FindingStrategy.CompareAll);
            if (distance < nearestDistance)
            {
                nearestPoint = point;
                nearestDistance = distance;
            }
        }

        if (nearestPoint is null)
        {
            return;
        }

        ChartPoint[] anchorPoints = [nearestPoint];
        _ = anchorPoints.GetTooltipLocation(Measure(), chart);
        var placement = chart.AutoToolTipsInfo.ToolTipPlacement;
        AlignWedge(placement, layout);

        var tooltipSize = Measure();
        var location = anchorPoints.GetTooltipLocation(tooltipSize, chart);
        if (placement != chart.AutoToolTipsInfo.ToolTipPlacement)
        {
            AlignWedge(chart.AutoToolTipsInfo.ToolTipPlacement, layout);
            tooltipSize = Measure();
            location = anchorPoints.GetTooltipLocation(tooltipSize, chart);
        }

        X = location.X;
        Y = location.Y;
    }

    private void AlignWedge(PopUpPlacement placement, Layout<SkiaSharpDrawingContext> layout)
    {
        Geometry.Placement = placement;
        switch (placement)
        {
            case PopUpPlacement.Top:
                layout.Padding = new Padding(8, 4, 8, 4 + Wedge);
                break;
            case PopUpPlacement.Bottom:
                layout.Padding = new Padding(8, 4 + Wedge, 8, 4);
                break;
            case PopUpPlacement.Left:
                layout.Padding = new Padding(8, 4, 8 + Wedge, 4);
                break;
            case PopUpPlacement.Right:
                layout.Padding = new Padding(8 + Wedge, 4, 8, 4);
                break;
        }
    }
}
