using System.Collections.Specialized;
using DynamicData;
using KubeUI.DynamicTableView.Tests.Fixtures;
using Microsoft.Reactive.Testing;

namespace KubeUI.DynamicTableView.Tests.Unit;

public sealed class DynamicTableViewSourceOptionsTests
{
    [Theory]
    [InlineData(true, NotifyCollectionChangedAction.Replace)]
    [InlineData(false, NotifyCollectionChangedAction.Remove)]
    public void Update_notification_mode_is_configurable(bool useReplaceForUpdates, NotifyCollectionChangedAction firstExpectedAction)
    {
        using SourceCache<DynamicTableViewTestRow, string> cache = new(static row => row.Id);
        using var source = DynamicTableViewTestData.CreateSource(
            cache,
            options: new() { UseReplaceForUpdates = useReplaceForUpdates });
        var original = DynamicTableViewTestData.CreateRows()[0];
        cache.AddOrUpdate(original);
        List<NotifyCollectionChangedAction> actions = [];
        ((INotifyCollectionChanged)source.Items).CollectionChanged += (_, args) => actions.Add(args.Action);

        cache.AddOrUpdate(original with { Name = "Updated" });

        Assert.NotEmpty(actions);
        Assert.Equal(firstExpectedAction, actions[0]);
        if (!useReplaceForUpdates)
            Assert.Contains(NotifyCollectionChangedAction.Add, actions);
    }

    [Fact]
    public void Reference_identity_drops_selection_when_the_source_replaces_the_row_object()
    {
        using SourceCache<DynamicTableViewTestRow, string> cache = new(static row => row.Id);
        using var source = DynamicTableViewTestData.CreateSource(
            cache,
            options: new() { SelectionIdentityMode = DynamicTableViewSelectionIdentityMode.Reference });
        cache.AddOrUpdate(DynamicTableViewTestData.CreateRows());
        source.SelectionModel.Select(1);

        cache.AddOrUpdate(DynamicTableViewTestData.CreateRows()[1] with { Name = "New instance" });

        Assert.Empty(source.SelectionModel.SelectedItems);
    }

    [Fact]
    public void Reference_identity_keeps_selection_when_the_source_updates_the_same_object_instance()
    {
        using SourceCache<DynamicTableViewTestRow, string> cache = new(static row => row.Id);
        TestScheduler uiScheduler = new();
        using var source = DynamicTableViewTestData.CreateSource(
            cache,
            uiScheduler: uiScheduler,
            options: new() { SelectionIdentityMode = DynamicTableViewSelectionIdentityMode.Reference });
        var rows = DynamicTableViewTestData.CreateRows();
        var selected = rows[1];
        cache.AddOrUpdate(rows);
        uiScheduler.AdvanceBy(100);
        source.SelectionModel.Select(1);
        Assert.Same(selected, source.SelectionModel.SelectedItem);

        cache.AddOrUpdate(selected);
        uiScheduler.AdvanceBy(100);

        Assert.Same(selected, source.SelectionModel.SelectedItem);
    }

    [Fact]
    public void Key_identity_keeps_selection_when_the_source_replaces_the_row_object()
    {
        using SourceCache<DynamicTableViewTestRow, string> cache = new(static row => row.Id);
        TestScheduler uiScheduler = new();
        using var source = DynamicTableViewTestData.CreateSource(
            cache,
            uiScheduler: uiScheduler,
            options: new() { SelectionIdentityMode = DynamicTableViewSelectionIdentityMode.Key });
        cache.AddOrUpdate(DynamicTableViewTestData.CreateRows());
        uiScheduler.AdvanceBy(100);
        source.SelectionModel.Select(1);

        cache.AddOrUpdate(DynamicTableViewTestData.CreateRows()[1] with { Name = "New instance" });
        uiScheduler.AdvanceBy(100);

        Assert.Equal("b", Assert.IsType<DynamicTableViewTestRow>(source.SelectionModel.SelectedItem).Id);
    }

    [Fact]
    public void Row_matching_uses_the_configured_identity_comparer()
    {
        using SourceCache<DynamicTableViewTestRow, string> cache = new(static row => row.Id);
        var first = DynamicTableViewTestData.CreateRows()[0];
        var equalButDistinct = first with { };
        using var keySource = DynamicTableViewTestData.CreateSource(cache);
        using var referenceSource = DynamicTableViewTestData.CreateSource(
            cache,
            options: new() { SelectionIdentityMode = DynamicTableViewSelectionIdentityMode.Reference });

        Assert.True(keySource.AreSameRows(first, equalButDistinct));
        Assert.False(referenceSource.AreSameRows(first, equalButDistinct));
        Assert.False(referenceSource.AreSameRows(first, null));
    }

    [Fact]
    public void Key_identity_keeps_selection_when_update_notifications_use_remove_and_add()
    {
        using SourceCache<DynamicTableViewTestRow, string> cache = new(static row => row.Id);
        TestScheduler uiScheduler = new();
        using var source = DynamicTableViewTestData.CreateSource(
            cache,
            uiScheduler: uiScheduler,
            options: new()
            {
                UseReplaceForUpdates = false,
                SelectionIdentityMode = DynamicTableViewSelectionIdentityMode.Key
            });
        cache.AddOrUpdate(DynamicTableViewTestData.CreateRows());
        uiScheduler.AdvanceBy(100);
        source.SelectionModel.Select(1);

        cache.AddOrUpdate(DynamicTableViewTestData.CreateRows()[1] with { Name = "Updated" });
        uiScheduler.AdvanceBy(100);

        Assert.Equal("b", Assert.IsType<DynamicTableViewTestRow>(source.SelectionModel.SelectedItem).Id);
        Assert.Equal("Updated", Assert.IsType<DynamicTableViewTestRow>(source.SelectionModel.SelectedItem).Name);
    }
}
