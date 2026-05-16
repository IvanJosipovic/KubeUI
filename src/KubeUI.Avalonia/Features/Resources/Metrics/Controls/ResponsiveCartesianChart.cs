using Avalonia.Layout;
using LiveChartsCore.Kernel;
using LiveChartsCore.Measure;
using LiveChartsCore.SkiaSharpView.Avalonia;

namespace KubeUI.Avalonia.Features.Resources.Metrics.Controls;

public sealed class ResponsiveCartesianChart : CartesianChart
{
    private Rect _lastEffectiveViewport;

    public ResponsiveCartesianChart()
    {
        EffectiveViewportChanged += OnEffectiveViewportChanged;
    }

    private void OnEffectiveViewportChanged(object? sender, EffectiveViewportChangedEventArgs e)
    {
        if (!IsEffectivelyVisible || Bounds.Width <= 0 || Bounds.Height <= 0)
        {
            _lastEffectiveViewport = e.EffectiveViewport;
            return;
        }

        var viewport = e.EffectiveViewport;
        var enteredViewport = _lastEffectiveViewport.Width <= 0 || _lastEffectiveViewport.Height <= 0;
        var widthChanged = Math.Abs(viewport.Width - _lastEffectiveViewport.Width) > 1;
        var heightChanged = Math.Abs(viewport.Height - _lastEffectiveViewport.Height) > 1;

        _lastEffectiveViewport = viewport;

        if (!enteredViewport && !widthChanged && !heightChanged)
        {
            return;
        }

        CoreChart.Update((ChartUpdateParams?)null);
        InvalidateMeasure();
        InvalidateArrange();
        InvalidateVisual();
    }
}
