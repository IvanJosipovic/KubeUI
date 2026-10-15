using System.Windows.Input;
using Avalonia.Input;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;

namespace KubeUI.Avalonia.Shell.Navigation;

internal sealed class NavigationTreeSelectionBehavior : Behavior<TreeView>
{
    private readonly ICommand _selectionChangedCommand;

    public NavigationTreeSelectionBehavior(ICommand selectionChangedCommand)
    {
        ArgumentNullException.ThrowIfNull(selectionChangedCommand);
        _selectionChangedCommand = selectionChangedCommand;
    }

    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject is { } tree)
        {
            tree.SelectionChanged += OnSelectionChanged;
            tree.AddHandler(InputElement.PointerPressedEvent, OnPointerPressed, RoutingStrategies.Tunnel);
        }
    }

    protected override void OnDetaching()
    {
        if (AssociatedObject is { } tree)
        {
            tree.SelectionChanged -= OnSelectionChanged;
            tree.RemoveHandler(InputElement.PointerPressedEvent, OnPointerPressed);
        }

        base.OnDetaching();
    }

    private void OnSelectionChanged(object? sender, SelectionChangedEventArgs args)
    {
        if (args.AddedItems.Count > 0)
        {
            _selectionChangedCommand.Execute(args);
        }
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs args)
    {
        var tree = AssociatedObject;
        if (tree is null
            || args.GetCurrentPoint(tree).Properties.PointerUpdateKind != PointerUpdateKind.LeftButtonPressed
            || args.Source is not Visual source)
        {
            return;
        }

        var item = source.GetSelfAndVisualAncestors().OfType<TreeViewItem>().FirstOrDefault();
        if (item is null)
        {
            return;
        }

        if (ReferenceEquals(tree.SelectedItem, item.DataContext))
        {
            tree.SelectedItem = null;
        }
    }
}
