using System;
using Avalonia.Controls;
using Avalonia.Input;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using KubeUI.Avalonia.Features.Resources.List.ViewModels;

namespace KubeUI.Avalonia.Features.Resources.List.Behaviors;

public sealed class ResourceListContextMenuBehavior : Behavior<DataGrid>
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
        if (AssociatedObject is not DataGrid grid)
        {
            return;
        }

        if (e.GetCurrentPoint(grid).Properties.PointerUpdateKind != PointerUpdateKind.RightButtonPressed)
        {
            return;
        }

        if (e.Source is not Visual source)
        {
            return;
        }

        DataGridRow? row = source.GetSelfAndVisualAncestors().OfType<DataGridRow>().FirstOrDefault();
        if (row is null)
        {
            return;
        }

        if (grid.ContextMenu is not ContextMenu contextMenu)
        {
            return;
        }

        if (grid.DataContext is not IResourceListViewModel viewModel)
        {
            contextMenu.ItemsSource = null;
            return;
        }

        IEnumerable? selectedItems = ResolveContextMenuItemsSource(viewModel, row.Index, row.DataContext);

        contextMenu.ItemsSource = viewModel.GetContextMenuItems(selectedItems);
    }

    internal static IEnumerable? ResolveContextMenuItemsSource(IResourceListViewModel viewModel, int rowIndex, object? rowDataContext)
    {
        if (viewModel.SelectionModel.SelectedIndexes.Contains(rowIndex))
        {
            return viewModel.SelectionModel.SelectedItems;
        }

        return rowDataContext is null ? [] : new[] { rowDataContext };
    }
}
