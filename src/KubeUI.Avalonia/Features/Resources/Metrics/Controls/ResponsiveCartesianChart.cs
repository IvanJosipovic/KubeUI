using Avalonia.Input;
using LiveChartsCore.Drawing;
using LiveChartsCore.SkiaSharpView.Avalonia;

namespace KubeUI.Avalonia.Features.Resources.Metrics.Controls;

public sealed class ResponsiveCartesianChart : CartesianChart
{
    private Rect _lastEffectiveViewport;

    internal LvcPoint PointerPosition { get; private set; }

    public ResponsiveCartesianChart()
    {
        EffectiveViewportChanged += OnEffectiveViewportChanged;
    }

    protected override void OnPointerMoved(PointerEventArgs e)
    {
        var position = e.GetPosition(this);
        PointerPosition = new LvcPoint((float)position.X, (float)position.Y);
        base.OnPointerMoved(e);
        if (Tooltip is NearestSeriesTooltip tooltip)
        {
            tooltip.UpdateAnchor(PointerPosition);
        }
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

        CoreChart.Update(null);
        InvalidateMeasure();
        InvalidateArrange();
        InvalidateVisual();
    }
}
