using Avalonia.Input;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using LiveChartsCore.SkiaSharpView.Avalonia;

namespace KubeUI.Avalonia.Features.Resources.Properties.Behaviors;

public sealed class ChartWheelScrollBehavior : Behavior<CartesianChart>
{
    private const double ScrollStep = 48;
    private const RoutingStrategies s_routingStrategies = RoutingStrategies.Tunnel;
    private bool _handlerAttached;

    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject != null && !_handlerAttached)
        {
            AssociatedObject.AddHandler(InputElement.PointerWheelChangedEvent, OnPointerWheelChanged, s_routingStrategies, handledEventsToo: true);
            _handlerAttached = true;
        }
    }

    protected override void OnDetaching()
    {
        if (AssociatedObject != null && _handlerAttached)
        {
            AssociatedObject.RemoveHandler(InputElement.PointerWheelChangedEvent, OnPointerWheelChanged);
            _handlerAttached = false;
        }

        base.OnDetaching();
    }

    private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
        // A nested scrollable that already handled the event keeps it, so scrolling inside the chart
        // content is not stolen by the outer scroll viewer.
        if (e.Handled)
        {
            return;
        }

        var scrollViewer = AssociatedObject?.FindAncestorOfType<ScrollViewer>();
        if (scrollViewer == null)
        {
            return;
        }

        var maxOffset = Math.Max(0, scrollViewer.Extent.Height - scrollViewer.Viewport.Height);
        var nextOffset = Math.Clamp(scrollViewer.Offset.Y - (e.Delta.Y * ScrollStep), 0, maxOffset);
        scrollViewer.Offset = new Vector(scrollViewer.Offset.X, nextOffset);
        e.Handled = true;
    }
}
