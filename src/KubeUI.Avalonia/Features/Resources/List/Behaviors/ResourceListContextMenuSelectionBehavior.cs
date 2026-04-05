using System.Linq;
using Avalonia.Controls;
using Avalonia.Controls.Selection;
using Avalonia.Input;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using KubeUI.Avalonia.Features.Resources.List.ViewModels;

namespace KubeUI.Avalonia.Features.Resources.List.Behaviors;

public sealed class ResourceListContextMenuSelectionBehavior : Behavior<DataGrid>
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

    internal static bool TrySelect(ISelectionModel selectionModel, int index)
    {
        if (index < 0)
        {
            return false;
        }

        if (selectionModel.IsSelected(index))
        {
            return true;
        }

        selectionModel.Clear();
        selectionModel.Select(index);
        return true;
    }

    private void OnPointerPressed(object? sender, PointerPressedEventArgs e)
    {
        if (AssociatedObject?.DataContext is not IResourceListViewModel viewModel)
        {
            return;
        }

        if (e.GetCurrentPoint(AssociatedObject).Properties.PointerUpdateKind != PointerUpdateKind.RightButtonPressed)
        {
            return;
        }

        if (e.Source is not Visual visual)
        {
            return;
        }

        DataGridRow? row = visual.GetSelfAndVisualAncestors().OfType<DataGridRow>().FirstOrDefault();
        if (row is null)
        {
            return;
        }

        if (!TrySelect(viewModel.SelectionModel, row.Index))
        {
            return;
        }
    }
}
