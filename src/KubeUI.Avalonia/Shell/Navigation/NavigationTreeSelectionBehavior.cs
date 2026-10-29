using System.Windows.Input;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;

namespace KubeUI.Avalonia.Shell.Navigation;

internal sealed class NavigationTreeSelectionBehavior : Behavior<TreeView>
{
    private readonly ICommand _itemInvokedCommand;

    public NavigationTreeSelectionBehavior(ICommand itemInvokedCommand)
    {
        ArgumentNullException.ThrowIfNull(itemInvokedCommand);
        _itemInvokedCommand = itemInvokedCommand;
    }

    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject is { } tree)
        {
            tree.AddHandler(InputElement.PointerReleasedEvent, OnPointerReleased, RoutingStrategies.Tunnel);
            tree.AddHandler(InputElement.KeyDownEvent, OnKeyDown, RoutingStrategies.Tunnel);
        }
    }

    protected override void OnDetaching()
    {
        if (AssociatedObject is { } tree)
        {
            tree.RemoveHandler(InputElement.PointerReleasedEvent, OnPointerReleased);
            tree.RemoveHandler(InputElement.KeyDownEvent, OnKeyDown);
        }

        base.OnDetaching();
    }

    private void OnPointerReleased(object? sender, PointerReleasedEventArgs args)
    {
        var tree = AssociatedObject;
        if (tree is null
            || args.GetCurrentPoint(tree).Properties.PointerUpdateKind != PointerUpdateKind.LeftButtonReleased
            || args.Source is not Visual source
            || FindItem(source) is not { DataContext: { } item })
        {
            return;
        }

        Invoke(item);
    }

    private void OnKeyDown(object? sender, KeyEventArgs args)
    {
        if (args.Key is not (Key.Enter or Key.Space)
            || AssociatedObject?.SelectedItem is not { } item)
        {
            return;
        }

        Invoke(item);
        args.Handled = true;
    }

    private void Invoke(object item)
    {
        if (_itemInvokedCommand.CanExecute(item))
        {
            _itemInvokedCommand.Execute(item);
        }
    }

    private static TreeViewItem? FindItem(Visual source)
    {
        TreeViewItem? item = null;
        foreach (var ancestor in source.GetSelfAndVisualAncestors())
        {
            if (ancestor is ToggleButton)
            {
                return null;
            }

            if (ancestor is TreeViewItem treeViewItem)
            {
                item = treeViewItem;
                break;
            }
        }

        return item;
    }
}
