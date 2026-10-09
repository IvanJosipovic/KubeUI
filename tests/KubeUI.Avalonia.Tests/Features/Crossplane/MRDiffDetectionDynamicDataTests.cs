using System.Collections.Specialized;
using System.ComponentModel;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using DynamicData;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.Crossplane;

public sealed class MRDiffDetectionDynamicDataTests
{
    [AvaloniaFact]
    public async Task Dynamic_table_applies_search_filter_and_sort_to_diff_rows()
    {
        using var rows = new SourceCache<CrossplaneDiffRow, string>(row => row.Key);
        using var table = CreateTable(rows);
        table.SetSort([new("name", ListSortDirection.Descending)]);
        rows.AddOrUpdate([
            CreateRow("uid-a", "api-a"),
            CreateRow("uid-b", "api-b"),
            CreateRow("uid-c", "worker")]);

        await WaitForAsync(() => Rows(table).Length == 3);
        table.SearchText = "api";
        await WaitForAsync(() => Rows(table).Length == 2);
        Rows(table).ShouldBe(["api-b", "api-a"]);

        table.SetFilter(new("name", DynamicTableViewFilterOperator.Contains, "api-a"));
        await WaitForAsync(() => Rows(table).Length == 1);
        Rows(table).ShouldBe(["api-a"]);

        table.ClearFilters();
        await WaitForAsync(() => Rows(table).Length == 2);
        table.SearchText = string.Empty;
        await WaitForAsync(() => Rows(table).Length == 3);
        Rows(table).ShouldBe(["worker", "api-b", "api-a"]);
    }

    [AvaloniaFact]
    public async Task Source_cache_updates_off_ui_thread_and_table_items_change_on_ui_thread()
    {
        using var rows = new SourceCache<CrossplaneDiffRow, string>(row => row.Key);
        using var table = CreateTable(rows);
        var collectionUpdate = new TaskCompletionSource<bool>(TaskCreationOptions.RunContinuationsAsynchronously);
        ((INotifyCollectionChanged)table.Items).CollectionChanged += (_, _) =>
            collectionUpdate.TrySetResult(Dispatcher.UIThread.CheckAccess());

        var cacheUpdateWasOnUiThread = await Task.Run(() =>
        {
            var isOnUiThread = Dispatcher.UIThread.CheckAccess();
            rows.AddOrUpdate(CreateRow("uid-a", "api-a"));
            return isOnUiThread;
        });
        var tableCollectionUpdateWasOnUiThread = await collectionUpdate.Task.WaitAsync(TimeSpan.FromSeconds(5));

        Assert.False(cacheUpdateWasOnUiThread);
        Assert.True(tableCollectionUpdateWasOnUiThread);
    }

    [AvaloniaFact]
    public async Task Selection_follows_a_diff_row_replaced_with_the_same_key()
    {
        using var rows = new SourceCache<CrossplaneDiffRow, string>(row => row.Key);
        using var table = CreateTable(rows);
        var original = CreateRow("uid-a", "api-a");
        rows.AddOrUpdate(original);
        await WaitForAsync(() => Rows(table).Length == 1);
        table.SelectionModel.Select(0);

        var replacement = CreateRow("uid-a", "api-a");
        rows.AddOrUpdate(replacement);
        await WaitForAsync(() => ReferenceEquals(table.SelectionModel.SelectedItem, replacement));

        Assert.Same(replacement, table.SelectionModel.SelectedItem);
    }

    private static DynamicTableViewSource<CrossplaneDiffRow, string> CreateTable(
        ISourceCache<CrossplaneDiffRow, string> rows)
    {
        return new DynamicTableViewSource<CrossplaneDiffRow, string>(
            rows,
            static row => row.Key,
            [
                CreateColumn("name", "Name", static row => row.Name),
                CreateColumn("kind", "Kind", static row => row.Kind)
            ],
            options: new DynamicTableViewSourceOptions
            {
                SelectionIdentityMode = DynamicTableViewSelectionIdentityMode.Key
            });
    }

    private static DynamicTableViewColumn<CrossplaneDiffRow> CreateColumn(
        string key,
        string header,
        Func<CrossplaneDiffRow, string> selector)
        => new(key, header, typeof(string), row => selector(row), selector, cellTemplate: null);

    private static CrossplaneDiffRow CreateRow(string uid, string name)
        => new(new CrossplaneDiffRecord(
            uid,
            name,
            "default",
            "example/v1",
            "Widget",
            "spec.value",
            "old",
            "new",
            false,
            false,
            false));

    private static string[] Rows(DynamicTableViewSource<CrossplaneDiffRow, string> table)
        => table.Items.Cast<CrossplaneDiffRow>().Select(static row => row.Name).ToArray();

    private static Task WaitForAsync(Func<bool> condition)
        => TestWait.UntilAsync(
            condition,
            5000,
            TestContext.Current.CancellationToken,
            () => Dispatcher.UIThread.RunJobs());
}
