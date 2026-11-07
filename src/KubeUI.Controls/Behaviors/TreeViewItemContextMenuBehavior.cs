using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;

namespace KubeUI.Controls.Behaviors;

/// <summary>Opens row-owned context menus on right-click without selecting TreeView rows.</summary>
/// <remarks>
/// Right-clicks on rows without a context menu remain inert. Detaching closes an open popup and
/// releases its close-event subscription.
/// </remarks>
public sealed class TreeViewItemContextMenuBehavior : Behavior<TreeView>
{
    private ContextMenu? _openContextMenu;

    /// <inheritdoc />
    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject is { } tree)
        {
            tree.AddHandler(InputElement.PointerPressedEvent, OnPointerPressed, RoutingStrategies.Tunnel);
        }
    }

    /// <inheritdoc />
    protected override void OnDetaching()
    {
        CloseOpenContextMenu();

        if (AssociatedObject is { } tree)
        {
            tree.RemoveHandler(InputElement.PointerPressedEvent, OnPointerPressed);
        }

        base.OnDetaching();
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        var tree = AssociatedObject;
        if (tree is null || e.GetCurrentPoint(tree).Properties.PointerUpdateKind != PointerUpdateKind.RightButtonPressed)
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

        // Right-click on any row must not trigger TreeView selection. Only rows with an actual
        // context menu get a popup; rows without one remain inert.
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
        _openContextMenu.Open(host);
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
