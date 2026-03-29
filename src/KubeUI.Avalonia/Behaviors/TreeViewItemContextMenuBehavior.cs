using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;

namespace KubeUI.Avalonia.Behaviors;

public sealed class TreeViewItemContextMenuBehavior : Behavior<TreeView>
{
    private ContextMenu? _openContextMenu;

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
        CloseOpenContextMenu();

        if (AssociatedObject != null)
        {
            AssociatedObject.RemoveHandler(InputElement.PointerPressedEvent, OnPointerPressed);
        }

        base.OnDetaching();
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (e.GetCurrentPoint(AssociatedObject).Properties.PointerUpdateKind != PointerUpdateKind.RightButtonPressed)
        {
            return;
        }

        if (e.Source is not Visual source)
        {
            return;
        }

        var item = source.GetSelfAndVisualAncestors().OfType<TreeViewItem>().FirstOrDefault();
        if (item == null)
        {
            return;
        }

        // Right-click on any navigation item should never flow into the tree's
        // default selection behavior. Only items with an actual context menu
        // get a popup; all other items remain inert.
        e.Handled = true;

        var host = FindContextMenuHost(item);

        if (host?.ContextMenu == null)
        {
            return;
        }

        if (ReferenceEquals(_openContextMenu, host.ContextMenu) && _openContextMenu.IsOpen)
        {
            CloseOpenContextMenu();
            return;
        }

        CloseOpenContextMenu();
        _openContextMenu = host.ContextMenu;
        _openContextMenu.Closed += OnContextMenuClosed;
        host.ContextMenu.Open(host);
    }

    private static Control? FindContextMenuHost(TreeViewItem item)
    {
        return item.GetVisualDescendants()
            .OfType<Control>()
            .FirstOrDefault(control =>
                control.ContextMenu != null
                && ReferenceEquals(control.DataContext, item.DataContext));
    }

    private void OnContextMenuClosed(object? sender, EventArgs e)
    {
        if (ReferenceEquals(sender, _openContextMenu))
        {
            ResetOpenContextMenu();
        }
    }

    private void CloseOpenContextMenu()
    {
        if (_openContextMenu == null)
        {
            return;
        }

        var menu = _openContextMenu;
        ResetOpenContextMenu();

        if (menu.IsOpen)
        {
            menu.Close();
        }
    }

    private void ResetOpenContextMenu()
    {
        if (_openContextMenu != null)
        {
            _openContextMenu.Closed -= OnContextMenuClosed;
        }

        _openContextMenu = null;
    }
}
