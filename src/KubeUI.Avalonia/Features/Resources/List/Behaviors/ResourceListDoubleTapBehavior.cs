using System.Collections;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Input;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;

namespace KubeUI.Avalonia.Features.Resources.List.Behaviors;

public sealed class ResourceListDoubleTapBehavior : Behavior<DataGrid>
{
    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject != null)
        {
            AssociatedObject.DoubleTapped += OnDoubleTapped;
        }
    }

    protected override void OnDetaching()
    {
        if (AssociatedObject != null)
        {
            AssociatedObject.DoubleTapped -= OnDoubleTapped;
        }

        base.OnDetaching();
    }

    private void OnDoubleTapped(object? sender, TappedEventArgs e)
    {
        if (AssociatedObject?.DataContext is not IResourceListViewModel viewModel)
        {
            return;
        }

        Execute(viewModel, e.Source);
    }

    internal static bool Execute(IResourceListViewModel viewModel, object? source)
    {
        if (!ShouldHandleSource(source))
        {
            return false;
        }

        var selectedItems = new ArrayList(viewModel.SelectionModel.SelectedItems
            .Where(static item => item != null)
            .Cast<object>()
            .ToArray());

        if (selectedItems.Count == 0 || !viewModel.ResourceConfig.ViewCommand.CanExecute(selectedItems))
        {
            return false;
        }

        viewModel.ResourceConfig.ViewCommand.Execute(selectedItems);
        return true;
    }

    internal static bool ShouldHandleSource(object? source)
    {
        if (source is not Visual visual)
        {
            return false;
        }

        var ancestors = visual.GetSelfAndVisualAncestors().ToArray();
        if (ancestors.Any(static x => x is DataGridColumnHeader or ScrollBar))
        {
            return false;
        }

        return ancestors.Any(static x => x is DataGridRow or DataGridCell);
    }
}
