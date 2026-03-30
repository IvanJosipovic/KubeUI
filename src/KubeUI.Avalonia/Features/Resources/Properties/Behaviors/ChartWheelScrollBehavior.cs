using Avalonia;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using LiveChartsCore.SkiaSharpView.Avalonia;

namespace KubeUI.Avalonia.Features.Resources.Properties.Behaviors;

public sealed class ChartWheelScrollBehavior : Behavior<CartesianChart>
{
    private const double ScrollStep = 48;

    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject != null)
        {
            AssociatedObject.AddHandler(InputElement.PointerWheelChangedEvent, OnPointerWheelChanged, RoutingStrategies.Tunnel, handledEventsToo: true);
        }
    }

    protected override void OnDetaching()
    {
        if (AssociatedObject != null)
        {
            AssociatedObject.RemoveHandler(InputElement.PointerWheelChangedEvent, OnPointerWheelChanged);
        }

        base.OnDetaching();
    }

    private void OnPointerWheelChanged(object? sender, PointerWheelEventArgs e)
    {
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
