using System.ComponentModel;
using DynamicData;
using KubeUI.DynamicTableView.Tests.Fixtures;
using Microsoft.Reactive.Testing;

namespace KubeUI.DynamicTableView.Tests.Unit;

public sealed class DynamicTableViewSourceChangeTests
{
    [Fact]
    public void Connect_updates_bound_rows_on_add_update_and_remove()
    {
        using SourceCache<DynamicTableViewTestRow, string> cache = new(static row => row.Id);
        using var source = DynamicTableViewTestData.CreateSource(cache);
        var original = DynamicTableViewTestData.CreateRows()[0];
        cache.AddOrUpdate(original);
        Assert.Equal(["a"], source.Items.Cast<DynamicTableViewTestRow>().Select(static row => row.Id));

        var updated = original with { Name = "Updated" };
        cache.AddOrUpdate(updated);
        Assert.Equal("Updated", Assert.Single(source.Items.Cast<DynamicTableViewTestRow>()).Name);

        cache.RemoveKey(updated.Id);
        Assert.Empty(source.Items);
    }

    [Fact]
    public void Dynamic_columns_raise_changed_and_update_search_and_filter_behavior()
    {
        using SourceCache<DynamicTableViewTestRow, string> cache = new(static row => row.Id);
        using var source = DynamicTableViewTestData.CreateSource(cache);
        cache.AddOrUpdate(DynamicTableViewTestData.CreateRows());
        var changes = 0;
        source.Changed += (_, _) => changes++;
        var idColumn = DynamicTableViewColumn<DynamicTableViewTestRow>.Create(
            "id", "Identifier", static row => row.Id);

        source.Columns.Add(idColumn);
        source.SearchText = "c";

        Assert.True(changes >= 2);
        Assert.Equal(["c"], source.Items.Cast<DynamicTableViewTestRow>().Select(static row => row.Id));
    }

    [Fact]
    public void Filtering_out_selected_row_clears_selection_and_clearing_filter_does_not_restore_it()
    {
        using SourceCache<DynamicTableViewTestRow, string> cache = new(static row => row.Id);
        TestScheduler uiScheduler = new();
        using var source = DynamicTableViewTestData.CreateSource(cache, uiScheduler: uiScheduler);
        cache.AddOrUpdate(DynamicTableViewTestData.CreateRows());
        uiScheduler.AdvanceBy(100);
        source.SelectionModel.Select(1);
        Assert.Equal("b", (source.SelectionModel.SelectedItem as DynamicTableViewTestRow)?.Id);
        source.SetFilter(new("name", DynamicTableViewFilterOperator.Equals, "Alpha"));
        uiScheduler.AdvanceBy(100);
        Assert.Empty(source.SelectionModel.SelectedItems.Where(static item => item is not null));

        source.ClearFilters();
        uiScheduler.AdvanceBy(100);

        Assert.Equal(["a", "b", "c"], source.Items.Cast<DynamicTableViewTestRow>().Select(static row => row.Id));
        Assert.Empty(source.SelectionModel.SelectedItems);
    }

    [Fact]
    public void Explicit_clear_discards_selection_hidden_by_filter()
    {
        using SourceCache<DynamicTableViewTestRow, string> cache = new(static row => row.Id);
        TestScheduler uiScheduler = new();
        using var source = DynamicTableViewTestData.CreateSource(cache, uiScheduler: uiScheduler);
        cache.AddOrUpdate(DynamicTableViewTestData.CreateRows());
        uiScheduler.AdvanceBy(100);
        source.SelectionModel.Select(1);

        source.SetFilter(new("name", DynamicTableViewFilterOperator.Equals, "Alpha"));
        uiScheduler.AdvanceBy(1);
        source.SelectionModel.Clear();

        source.ClearFilters();
        uiScheduler.AdvanceBy(1);

        Assert.Empty(source.SelectionModel.SelectedItems);
    }

    [Fact]
    public void Filtering_some_multi_selected_rows_keeps_only_visible_selection()
    {
        using SourceCache<DynamicTableViewTestRow, string> cache = new(static row => row.Id);
        TestScheduler uiScheduler = new();
        using var source = DynamicTableViewTestData.CreateSource(cache, uiScheduler: uiScheduler);
        cache.AddOrUpdate(DynamicTableViewTestData.CreateRows());
        uiScheduler.AdvanceBy(100);
        source.SelectionModel.Select(1);
        source.SelectionModel.Select(2);

        source.SetFilter(new("name", DynamicTableViewFilterOperator.Equals, "Gamma"));
        uiScheduler.AdvanceBy(100);

        Assert.Equal("c", Assert.Single(source.SelectionModel.SelectedItems) is DynamicTableViewTestRow selected ? selected.Id : null);

        source.ClearFilters();
        uiScheduler.AdvanceBy(100);

        Assert.Equal("c", Assert.Single(source.SelectionModel.SelectedItems) is DynamicTableViewTestRow restored ? restored.Id : null);
    }

    [Fact]
    public void Selection_identity_survives_source_replacement_and_sort_movement()
    {
        using SourceCache<DynamicTableViewTestRow, string> cache = new(static row => row.Id);
        TestScheduler uiScheduler = new();
        using var source = DynamicTableViewTestData.CreateSource(cache, uiScheduler: uiScheduler);
        cache.AddOrUpdate(DynamicTableViewTestData.CreateRows());
        uiScheduler.AdvanceBy(100);
        source.SelectionModel.Select(1);

        source.SetSort([new("age", ListSortDirection.Ascending)]);
        cache.AddOrUpdate(DynamicTableViewTestData.CreateRows()[1] with { Age = 5 });
        uiScheduler.AdvanceBy(100);

        Assert.Equal(["b", "a", "c"], source.Items.Cast<DynamicTableViewTestRow>().Select(static row => row.Id));
        Assert.Equal("b", Assert.Single(source.SelectionModel.SelectedItems) is DynamicTableViewTestRow selected ? selected.Id : null);
    }

    [Fact]
    public void Multi_selection_survives_large_incremental_sorted_update()
    {
        using SourceCache<DynamicTableViewTestRow, string> cache = new(static row => row.Id);
        TestScheduler uiScheduler = new();
        using var source = DynamicTableViewTestData.CreateSource(cache, uiScheduler: uiScheduler);
        var rows = Enumerable.Range(0, 400)
            .Select(index => new DynamicTableViewTestRow($"row-{index:D3}", $"Row {index}", index, DateTimeOffset.UnixEpoch, true, DynamicTableViewTestState.Ready))
            .ToArray();
        cache.AddOrUpdate(rows);
        uiScheduler.AdvanceBy(100);
        source.SetSort([new("age", ListSortDirection.Ascending)]);
        source.SelectionModel.SelectAll();

        cache.AddOrUpdate(rows[200] with { Age = -1, Name = "Moved to start" });
        uiScheduler.AdvanceBy(100);

        Assert.Equal("row-200", ((DynamicTableViewTestRow)source.Items.Cast<object>().First()).Id);
        Assert.Equal(400, source.SelectionModel.SelectedItems.Count);
        Assert.Equal(400, source.SelectionModel.SelectedIndexes.Count);
    }

    [Fact]
    public void Disposing_source_stops_consuming_changes_and_disposes_selection()
    {
        using SourceCache<DynamicTableViewTestRow, string> cache = new(static row => row.Id);
        var source = DynamicTableViewTestData.CreateSource(cache);
        source.Dispose();

        Assert.Throws<ObjectDisposedException>(() => source.SetSort([]));
    }
}
