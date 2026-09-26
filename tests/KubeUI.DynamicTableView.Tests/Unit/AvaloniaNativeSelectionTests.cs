using System.ComponentModel;
using Avalonia.Controls.Selection;
using DynamicData;
using KubeUI.DynamicTableView.Tests.Fixtures;

namespace KubeUI.DynamicTableView.Tests.Unit;

public sealed class AvaloniaNativeSelectionTests
{
    [Fact]
    public void Native_selection_does_not_keep_identity_when_sorted_replacement_moves_selected_row()
    {
        using SourceCache<DynamicTableViewTestRow, string> cache = new(static row => row.Id);
        using var source = DynamicTableViewTestData.CreateSource(cache);
        cache.AddOrUpdate(DynamicTableViewTestData.CreateRows());
        var nativeSelection = new SelectionModel<object?> { Source = source.Items };
        nativeSelection.Select(1);

        source.SetSort([new("age", ListSortDirection.Ascending)]);
        cache.AddOrUpdate(DynamicTableViewTestData.CreateRows()[1] with { Age = 5 });

        Assert.Null(nativeSelection.SelectedItem);
    }

    [Fact]
    public void Native_selection_clears_a_row_removed_by_filtering()
    {
        using SourceCache<DynamicTableViewTestRow, string> cache = new(static row => row.Id);
        using var source = DynamicTableViewTestData.CreateSource(cache);
        cache.AddOrUpdate(DynamicTableViewTestData.CreateRows());
        var nativeSelection = new SelectionModel<object?> { Source = source.Items };
        nativeSelection.Select(1);

        source.SetFilter(new("name", DynamicTableViewFilterOperator.Equals, "Alpha"));

        Assert.Empty(nativeSelection.SelectedItems);
    }
}
