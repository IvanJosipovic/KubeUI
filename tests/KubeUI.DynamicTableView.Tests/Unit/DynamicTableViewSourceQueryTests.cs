using System.ComponentModel;
using System.Reactive.Concurrency;
using DynamicData;
using KubeUI.DynamicTableView.Tests.Fixtures;
using Microsoft.Reactive.Testing;

namespace KubeUI.DynamicTableView.Tests.Unit;

public sealed class DynamicTableViewSourceQueryTests
{
    [Fact]
    public void Multiple_sorts_filters_and_search_apply_in_sequence()
    {
        using SourceCache<DynamicTableViewTestRow, string> cache = new(static row => row.Id);
        using var source = DynamicTableViewTestData.CreateSource(cache);
        DynamicTableViewTestRow[] rows =
        [
            new("a", "shared", 2, DateTimeOffset.UnixEpoch, true, DynamicTableViewTestState.Ready),
            new("b", "shared", 1, DateTimeOffset.UnixEpoch, true, DynamicTableViewTestState.Ready),
            new("c", "shared", 3, DateTimeOffset.UnixEpoch, true, DynamicTableViewTestState.Ready),
            new("d", "other", 0, DateTimeOffset.UnixEpoch, true, DynamicTableViewTestState.Ready)
        ];
        cache.AddOrUpdate(rows);
        source.SetFilter(new("name", DynamicTableViewFilterOperator.Contains, "shared"));
        source.SearchText = "shared";
        source.SetSort(
        [
            new("name", ListSortDirection.Ascending),
            new("age", ListSortDirection.Descending)
        ]);

        Assert.Equal(["c", "a", "b"], source.Items.Cast<DynamicTableViewTestRow>().Select(static row => row.Id));
    }

    [Fact]
    public void Search_matches_every_searchable_column_and_skips_disabled_columns()
    {
        using SourceCache<DynamicTableViewTestRow, string> cache = new(static row => row.Id);
        var columns = DynamicTableViewTestData.CreateColumns();
        columns[1].IsSearchable = false;
        using var source = DynamicTableViewTestData.CreateSource(cache, columns);
        cache.AddOrUpdate(DynamicTableViewTestData.CreateRows());

        source.SearchText = "failed";

        Assert.Equal(["c"], source.Items.Cast<DynamicTableViewTestRow>().Select(static row => row.Id));
    }

    [Fact]
    public void Sorting_without_active_filters_restores_all_rows()
    {
        using SourceCache<DynamicTableViewTestRow, string> cache = new(static row => row.Id);
        using var source = DynamicTableViewTestData.CreateSource(cache);
        cache.AddOrUpdate(DynamicTableViewTestData.CreateRows());

        source.SetSort([new("age", ListSortDirection.Descending)]);

        Assert.Equal(["c", "b", "a"], source.Items.Cast<DynamicTableViewTestRow>().Select(static row => row.Id));
    }

    [Fact]
    public void Search_uses_the_configured_debounce_scheduler()
    {
        using SourceCache<DynamicTableViewTestRow, string> cache = new(static row => row.Id);
        TestScheduler searchScheduler = new();
        using var source = DynamicTableViewTestData.CreateSource(
            cache,
            workerScheduler: ImmediateScheduler.Instance,
            uiScheduler: ImmediateScheduler.Instance,
            searchDebounce: TimeSpan.FromMilliseconds(250),
            searchScheduler: searchScheduler);
        cache.AddOrUpdate(DynamicTableViewTestData.CreateRows());

        source.SearchText = "beta";
        Assert.Equal(3, source.Items.Cast<DynamicTableViewTestRow>().Count());
        searchScheduler.AdvanceBy(TimeSpan.FromMilliseconds(249).Ticks);
        Assert.Equal(3, source.Items.Cast<DynamicTableViewTestRow>().Count());
        searchScheduler.AdvanceBy(TimeSpan.FromMilliseconds(1).Ticks);

        Assert.Equal(["b"], source.Items.Cast<DynamicTableViewTestRow>().Select(static row => row.Id));
    }

    [Fact]
    public void A_new_search_text_cancels_the_previous_debounce()
    {
        using SourceCache<DynamicTableViewTestRow, string> cache = new(static row => row.Id);
        TestScheduler searchScheduler = new();
        using var source = DynamicTableViewTestData.CreateSource(
            cache,
            workerScheduler: ImmediateScheduler.Instance,
            uiScheduler: ImmediateScheduler.Instance,
            searchDebounce: TimeSpan.FromMilliseconds(250),
            searchScheduler: searchScheduler);
        cache.AddOrUpdate(DynamicTableViewTestData.CreateRows());

        source.SearchText = "Alpha";
        searchScheduler.AdvanceBy(TimeSpan.FromMilliseconds(200).Ticks);
        source.SearchText = "Beta";
        searchScheduler.AdvanceBy(TimeSpan.FromMilliseconds(249).Ticks);
        Assert.Equal(3, source.Items.Cast<DynamicTableViewTestRow>().Count());
        searchScheduler.AdvanceBy(TimeSpan.FromMilliseconds(1).Ticks);

        Assert.Equal(["b"], source.Items.Cast<DynamicTableViewTestRow>().Select(static row => row.Id));

        source.SearchText = string.Empty;
        Assert.Equal(3, source.Items.Cast<DynamicTableViewTestRow>().Count());
    }
}
