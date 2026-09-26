using System.Reactive.Concurrency;
using DynamicData;

namespace KubeUI.DynamicTableView.Tests.Fixtures;

public static class DynamicTableViewTestData
{
    public static DynamicTableViewColumn<DynamicTableViewTestRow>[] CreateColumns()
    {
        var name = DynamicTableViewColumn<DynamicTableViewTestRow>.Create(
            "name", "Name", static row => row.Name);
        var age = DynamicTableViewColumn<DynamicTableViewTestRow>.Create(
            "age", "Age", static row => row.Age);
        var created = DynamicTableViewColumn<DynamicTableViewTestRow>.Create(
            "created", "Created", static row => row.CreatedAt);
        var enabled = DynamicTableViewColumn<DynamicTableViewTestRow>.Create(
            "enabled", "Enabled", static row => row.Enabled);
        enabled.FilterChoices = [new("False", false), new("True", true)];
        var state = DynamicTableViewColumn<DynamicTableViewTestRow>.CreateEnum(
            "state", "State", static row => row.State);
        return [name, age, created, enabled, state];
    }

    public static DynamicTableViewSource<DynamicTableViewTestRow, string> CreateSource(
        SourceCache<DynamicTableViewTestRow, string> cache,
        DynamicTableViewColumn<DynamicTableViewTestRow>[]? columns = null,
        IScheduler? workerScheduler = null,
        IScheduler? uiScheduler = null,
        TimeSpan? searchDebounce = null,
        IScheduler? searchScheduler = null,
        DynamicTableViewSourceOptions? options = null)
        => new(cache.Connect(), static row => row.Id, columns ?? CreateColumns(),
            workerScheduler ?? ImmediateScheduler.Instance, uiScheduler ?? ImmediateScheduler.Instance,
            searchDebounce ?? TimeSpan.Zero, searchScheduler ?? ImmediateScheduler.Instance, options);

    public static DynamicTableViewTestRow[] CreateRows()
        =>
        [
        new("a", "Alpha", 10, new DateTimeOffset(2024, 1, 1, 0, 0, 0, TimeSpan.Zero), true, DynamicTableViewTestState.Ready),
        new("b", "Beta", 20, new DateTimeOffset(2024, 1, 2, 0, 0, 0, TimeSpan.Zero), false, DynamicTableViewTestState.Pending),
        new("c", "Gamma", 30, new DateTimeOffset(2024, 1, 3, 0, 0, 0, TimeSpan.Zero), true, DynamicTableViewTestState.Failed)
        ];
}
