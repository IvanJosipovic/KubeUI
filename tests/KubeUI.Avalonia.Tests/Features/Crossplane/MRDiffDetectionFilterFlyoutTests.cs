using Avalonia.Controls.DataGridFiltering;
using Avalonia.Controls;
using Avalonia.Controls.Templates;
using KubeUI.Avalonia.Controls.DataGridFilters;
using KubeUI.Avalonia.Infrastructure.DataGrid;
using KubeUI.Avalonia.Resources;

namespace KubeUI.Avalonia.Tests.Features.Crossplane;

public sealed class MRDiffDetectionFilterFlyoutTests
{
    [Fact]
    public void Attaching_mr_diff_filter_flyouts_assigns_a_flyout_to_the_column()
    {
        var valueColumn = new DataGridValueColumn<CrossplaneDiffRow, string>
        {
            Key = "name",
            Name = "Name",
            Field = row => row.Name
        };
        var binding = DataGridBindingDefinition.Create<CrossplaneDiffRow, CrossplaneDiffRow>(row => row);
        var column = new DataGridTextColumnDefinition
        {
            ColumnKey = valueColumn.Key,
            Tag = valueColumn,
            Binding = binding,
            ValueAccessor = valueColumn.ValueAccessor,
            ValueType = valueColumn.ValueType,
            ShowFilterButton = true
        };
        var filteringModel = new FilteringModel();
        var factory = new DataGridColumnFilterFlyoutFactory(new DataGridColumnFilterService(TimeProvider.System));

        DataGridFilterFlyoutAttacher.Attach([column], factory, filteringModel);

        Assert.NotNull(column.FilterFlyout);
    }
}
