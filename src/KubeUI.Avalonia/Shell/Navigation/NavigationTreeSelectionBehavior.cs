using System.Windows.Input;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;

namespace KubeUI.Avalonia.Shell.Navigation;

internal sealed class NavigationTreeSelectionBehavior : Behavior<TreeView>
{
    private readonly ICommand _selectionChangedCommand;
    private NavigationItem? _expandableItemSelectionToIgnore;

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
            var shouldIgnore = ReferenceEquals(args.AddedItems[0], _expandableItemSelectionToIgnore);
            _expandableItemSelectionToIgnore = null;
            if (shouldIgnore)
            {
                return;
            }

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
        if (item is not null && ReferenceEquals(tree.SelectedItem, item.DataContext))
        {
            if (item.DataContext is NavigationItem navigationItem
                && navigationItem is not ClusterNavigationNode
                && navigationItem.NavigationItems.Count > 0
                && !source.GetSelfAndVisualAncestors().OfType<ToggleButton>()
                    .Any(toggle => ReferenceEquals(toggle.DataContext, navigationItem)))
            {
                _expandableItemSelectionToIgnore = navigationItem;
            }

            tree.SelectedItem = null;
        }
    }
}
