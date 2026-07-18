using Avalonia.Controls;
using Avalonia.VisualTree;
using Avalonia.Xaml.Interactivity;
using KubeUI.Avalonia.Features.Resources.List.ViewModels;
using ProDataGrid;

namespace KubeUI.Avalonia.Features.Resources.List.Behaviors;

public sealed class ResourceListPersistenceBehavior : Behavior<DataGrid>
{
    private IResourceListViewModel? ViewModel => AssociatedObject?.DataContext as IResourceListViewModel;
    private bool _loaded;

    protected override void OnAttached()
    {
        base.OnAttached();

        if (AssociatedObject is not DataGrid grid)
        {
            return;
        }

        grid.AttachedToVisualTree += GridAttachedToVisualTree;
        grid.DetachedFromVisualTree += GridDetachedFromVisualTree;
        grid.LayoutUpdated += GridLayoutUpdated;
    }

    protected override void OnDetaching()
    {
        if (AssociatedObject is DataGrid grid)
        {
            grid.AttachedToVisualTree -= GridAttachedToVisualTree;
            grid.DetachedFromVisualTree -= GridDetachedFromVisualTree;
            grid.LayoutUpdated -= GridLayoutUpdated;
        }

        base.OnDetaching();
    }

    private void GridAttachedToVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        _loaded = false;

        if (sender is DataGrid grid && ViewModel?.DataGridRuntimeState is { } state)
        {
            grid.RestoreState(state, DataGridStateSections.All, CreateStateOptions(grid));
        }

        _loaded = true;
    }

    private void GridDetachedFromVisualTree(object? sender, VisualTreeAttachmentEventArgs e)
    {
        SaveState(sender as DataGrid);
    }

    private void GridLayoutUpdated(object? sender, EventArgs e)
    {
        if (_loaded)
        {
            SaveState(sender as DataGrid);
        }
    }

    private void SaveState(DataGrid? grid)
    {
        if (grid is null || ViewModel is not { } viewModel)
        {
            return;
        }

        var state = grid.CaptureState(DataGridStateSections.All, CreateStateOptions(grid));
        if (state.Scroll is not null || viewModel.DataGridRuntimeState?.Scroll is null)
        {
            viewModel.DataGridRuntimeState = state;
        }
    }

    private static DataGridStateOptions CreateStateOptions(DataGrid grid) => new()
    {
        ColumnKeySelector = column => column.ColumnKey?.ToString(),
        ColumnKeyResolver = key => grid.Columns.FirstOrDefault(column =>
            string.Equals(column.ColumnKey?.ToString(), key?.ToString(), StringComparison.Ordinal))
    };
}
