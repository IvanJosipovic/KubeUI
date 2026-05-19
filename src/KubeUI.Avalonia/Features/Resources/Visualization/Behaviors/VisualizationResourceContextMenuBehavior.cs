using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using KubeUI.Avalonia.Features.Resources.Visualization.ViewModels;

namespace KubeUI.Avalonia.Features.Resources.Visualization.Behaviors;

public sealed class VisualizationResourceContextMenuBehavior : Behavior<Control>
{
    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject != null)
        {
            AssociatedObject.AddHandler(InputElement.PointerPressedEvent, OnPointerPressed, RoutingStrategies.Tunnel);
        }
    }

    protected override void OnDetaching()
    {
        if (AssociatedObject != null)
        {
            AssociatedObject.RemoveHandler(InputElement.PointerPressedEvent, OnPointerPressed);
        }

        base.OnDetaching();
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (AssociatedObject is not Control control)
        {
            return;
        }

        if (e.GetCurrentPoint(control).Properties.PointerUpdateKind != PointerUpdateKind.RightButtonPressed)
        {
            return;
        }

        if (control.ContextMenu is not ContextMenu contextMenu)
        {
            return;
        }

        if (control.DataContext is not VisualizationViewModel.ResourceNodeViewModel node)
        {
            contextMenu.ItemsSource = null;
            return;
        }

        contextMenu.ItemsSource = node.ContextMenuItems;
    }
}
