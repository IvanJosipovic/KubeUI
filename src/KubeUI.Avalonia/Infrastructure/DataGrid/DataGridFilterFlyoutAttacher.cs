using Avalonia.Controls.DataGridFiltering;
using KubeUI.Avalonia.Controls.DataGridFilters;
using KubeUI.Avalonia.Resources;

namespace KubeUI.Avalonia.Infrastructure.DataGrid;

internal static class DataGridFilterFlyoutAttacher
{
    public static void Attach(
        IEnumerable<DataGridColumnDefinition> columns,
        DataGridColumnFilterFlyoutFactory factory,
        IFilteringModel filteringModel)
    {
        foreach (var column in columns)
        {
            if (column.Tag is IResourceListColumn resourceColumn)
            {
                column.FilterFlyout = factory.Create(resourceColumn, column, filteringModel);
            }
        }
    }
}
