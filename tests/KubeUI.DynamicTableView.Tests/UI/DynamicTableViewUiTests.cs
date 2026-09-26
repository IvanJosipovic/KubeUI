using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using System.Reactive.Concurrency;
using DynamicData;
using KubeUI.DynamicTableView.Tests.Fixtures;

namespace KubeUI.DynamicTableView.Tests.UI;

public sealed class DynamicTableViewUiTests
{
    [AvaloniaFact]
    public void Source_columns_and_selection_are_attached_to_the_control()
    {
        using SourceCache<DynamicTableViewTestRow, string> cache = new(static row => row.Id);
        using var source = DynamicTableViewTestData.CreateSource(cache);
        DynamicTableView table = new() { Source = source };

        Assert.Equal(source.Columns.Count, table.Columns.Count);
        Assert.Same(source.SelectionModel, table.Selection);
        source.Columns.Add(DynamicTableViewColumn<DynamicTableViewTestRow>.Create("new", "New", static row => row.Id));
        Assert.Equal(source.Columns.Count, table.Columns.Count);
    }

    [AvaloniaFact]
    public void Default_ui_scheduler_publishes_bound_rows_on_the_avalonia_dispatcher()
    {
        using SourceCache<DynamicTableViewTestRow, string> cache = new(static row => row.Id);
        using var source = new DynamicTableViewSource<DynamicTableViewTestRow, string>(
            cache.Connect(),
            static row => row.Id,
            DynamicTableViewTestData.CreateColumns(),
            workerScheduler: ImmediateScheduler.Instance,
            searchDebounce: TimeSpan.Zero,
            searchScheduler: ImmediateScheduler.Instance);

        cache.AddOrUpdate(DynamicTableViewTestData.CreateRows()[0]);
        Assert.Empty(source.Items);

        Dispatcher.UIThread.RunJobs();

        Assert.Equal(["a"], source.Items.Cast<DynamicTableViewTestRow>().Select(static row => row.Id));
    }

    [AvaloniaFact]
    public void State_restore_reorders_columns_and_restores_width_sort_and_filter()
    {
        using SourceCache<DynamicTableViewTestRow, string> cache = new(static row => row.Id);
        using var source = DynamicTableViewTestData.CreateSource(cache);
        DynamicTableView table = new() { Source = source };
        DynamicTableViewState state = new(
        [
            new("age", 0, 123),
            new("name", 1, 180)
        ],
        [new("age", ListSortDirection.Descending)],
        [new("age", DynamicTableViewFilterOperator.GreaterThan, 15)]);

        table.RestoreState(state);
        var captured = table.CaptureState();

        Assert.Equal(["age", "name"], source.Columns.Take(2).Select(static column => column.Key));
        Assert.Equal(123, table.Columns[0].Width.Value);
        Assert.Equal(ListSortDirection.Descending, Assert.Single(captured.Sorts).Direction);
        Assert.Equal(DynamicTableViewFilterOperator.GreaterThan, Assert.Single(captured.Filters).Operator);
    }

    [AvaloniaFact]
    public void State_capture_and_restore_preserves_horizontal_and_vertical_scroll_offsets()
    {
        using SourceCache<DynamicTableViewTestRow, string> cache = new(static row => row.Id);
        using var source = DynamicTableViewTestData.CreateSource(cache);
        cache.AddOrUpdate(Enumerable.Range(0, 100).Select(index =>
            new DynamicTableViewTestRow($"row-{index:D3}", $"Row {index}", index, DateTimeOffset.UnixEpoch, true, DynamicTableViewTestState.Ready)));

        DynamicTableView firstTable = new() { Source = source };
        Window firstWindow = new() { Width = 420, Height = 240, Content = firstTable };
        try
        {
            firstWindow.Show();
            Dispatcher.UIThread.RunJobs();
            var firstScrollViewer = Assert.Single(firstTable.GetVisualDescendants().OfType<ScrollViewer>());
            Assert.True(firstScrollViewer.ScrollBarMaximum.Y > 0);
            Assert.InRange(firstTable.GetVisualDescendants().OfType<TableViewRow>().Count(), 1, 99);
            firstScrollViewer.Offset = new Vector(80, 200);
            Dispatcher.UIThread.RunJobs();
            var state = firstTable.CaptureState();

            Assert.Equal(firstScrollViewer.Offset, state.ScrollOffset);

            firstWindow.Close();
            Dispatcher.UIThread.RunJobs();
            DynamicTableView secondTable = new() { Source = source };
            Window secondWindow = new() { Width = 420, Height = 240, Content = secondTable };
            try
            {
                secondWindow.Show();
                Dispatcher.UIThread.RunJobs();
                secondTable.RestoreState(state);
                Dispatcher.UIThread.RunJobs();

                var secondScrollViewer = Assert.Single(secondTable.GetVisualDescendants().OfType<ScrollViewer>());
                Assert.Equal(state.ScrollOffset, secondScrollViewer.Offset);
            }
            finally
            {
                secondWindow.Close();
            }
        }
        finally
        {
            firstWindow.Close();
        }
    }

    [AvaloniaFact]
    public void State_restore_waits_for_virtualized_extent_to_grow()
    {
        using SourceCache<DynamicTableViewTestRow, string> cache = new(static row => row.Id);
        using var source = DynamicTableViewTestData.CreateSource(cache);
        cache.AddOrUpdate(DynamicTableViewTestData.CreateRows());
        DynamicTableView table = new() { Source = source };
        Window window = new() { Width = 420, Height = 240, Content = table };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var scrollViewer = Assert.Single(table.GetVisualDescendants().OfType<ScrollViewer>());
            table.RestoreState(new([], [], [], new Vector(0, 160)));
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(default, scrollViewer.Offset);

            cache.AddOrUpdate(Enumerable.Range(3, 100).Select(index =>
                new DynamicTableViewTestRow($"row-{index:D3}", $"Row {index}", index, DateTimeOffset.UnixEpoch, true, DynamicTableViewTestState.Ready)));
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(160, scrollViewer.Offset.Y);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Column_width_modes_preserve_pixel_and_star_settings()
    {
        using SourceCache<DynamicTableViewTestRow, string> cache = new(static row => row.Id);
        var columns = DynamicTableViewTestData.CreateColumns();
        columns[0].WidthMode = DynamicTableViewWidthMode.Pixel;
        columns[0].Width = 155;
        columns[1].WidthMode = DynamicTableViewWidthMode.Star;
        columns[1].Width = 2;
        using var source = DynamicTableViewTestData.CreateSource(cache, columns);
        DynamicTableView table = new() { Source = source };

        Assert.Equal(GridUnitType.Pixel, table.Columns[0].Width.GridUnitType);
        Assert.Equal(155, table.Columns[0].Width.Value);
        Assert.Equal(GridUnitType.Star, table.Columns[1].Width.GridUnitType);
        Assert.Equal(2, table.Columns[1].Width.Value);
    }

    [AvaloniaFact]
    public void Single_and_multiple_selection_follow_the_selection_model()
    {
        using SourceCache<DynamicTableViewTestRow, string> cache = new(static row => row.Id);
        using var source = DynamicTableViewTestData.CreateSource(cache);
        cache.AddOrUpdate(DynamicTableViewTestData.CreateRows());
        DynamicTableView table = new() { Source = source };

        source.SelectionModel.SingleSelect = true;
        source.SelectionModel.Select(0);
        source.SelectionModel.Select(1);
        Assert.Single(source.SelectionModel.SelectedItems);

        source.SelectionModel.SingleSelect = false;
        source.SelectionModel.Clear();
        source.SelectionModel.Select(0);
        source.SelectionModel.Select(2);
        Assert.Equal(2, source.SelectionModel.SelectedItems.Count);
    }

    [AvaloniaFact]
    public void Header_sort_button_cycles_ascending_descending_and_unsorted()
    {
        using SourceCache<DynamicTableViewTestRow, string> cache = new(static row => row.Id);
        using var source = DynamicTableViewTestData.CreateSource(cache);
        cache.AddOrUpdate([
            DynamicTableViewTestData.CreateRows()[0] with { Name = "Gamma" },
            DynamicTableViewTestData.CreateRows()[1] with { Name = "Alpha" },
            DynamicTableViewTestData.CreateRows()[2] with { Name = "Beta" }]);
        DynamicTableView table = new() { Source = source };
        Window window = new() { Width = 500, Height = 240, Content = table };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var nameHeader = Assert.IsAssignableFrom<Control>(table.Columns[0].Header);
            var sortButton = Assert.Single(nameHeader.GetVisualDescendants().OfType<Button>()
                .Where(static button => button.Classes.Contains("dynamic-table-view-sort-button")));

            sortButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Assert.Equal(["b", "c", "a"], source.Items.Cast<DynamicTableViewTestRow>().Select(static row => row.Id));

            sortButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Assert.Equal(["a", "c", "b"], source.Items.Cast<DynamicTableViewTestRow>().Select(static row => row.Id));

            sortButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
            Assert.Equal(["a", "b", "c"], source.Items.Cast<DynamicTableViewTestRow>().Select(static row => row.Id));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Custom_filter_flyout_factory_is_attached_and_opened_from_header()
    {
        using SourceCache<DynamicTableViewTestRow, string> cache = new(static row => row.Id);
        var column = DynamicTableViewColumn<DynamicTableViewTestRow>.Create("name", "Name", static row => row.Name);
        column.FilterFlyoutFactory = static _ => new Border();
        using var source = DynamicTableViewTestData.CreateSource(cache, [column]);
        DynamicTableView table = new() { Source = source };
        Window window = new() { Width = 420, Height = 240, Content = table };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var header = Assert.IsAssignableFrom<Control>(table.Columns[0].Header);
            var filterButton = Assert.Single(header.GetVisualDescendants().OfType<Button>()
                .Where(static button => button.Classes.Contains("dynamic-table-view-filter-button")));
            var attachedFlyout = Assert.IsType<Flyout>(FlyoutBase.GetAttachedFlyout(filterButton));

            FlyoutBase.ShowAttachedFlyout(filterButton);
            Dispatcher.UIThread.RunJobs();

            Assert.True(attachedFlyout.IsOpen);
            Assert.IsType<Border>(attachedFlyout.Content);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Standard_text_and_numeric_filter_flyouts_apply_typed_values()
    {
        using SourceCache<DynamicTableViewTestRow, string> cache = new(static row => row.Id);
        using var source = DynamicTableViewTestData.CreateSource(cache);
        cache.AddOrUpdate(DynamicTableViewTestData.CreateRows());
        DynamicTableView table = new() { Source = source };
        Window window = new() { Width = 520, Height = 260, Content = table };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            ApplyStandardFilter(table, 0, 0, "ph");
            Assert.Equal("ph", Assert.Single(source.FilterDescriptors).Value);
            Assert.Equal(["a"], source.Items.Cast<DynamicTableViewTestRow>().Select(static row => row.Id));

            source.ClearFilters();
            ApplyStandardFilter(table, 1, 3, "20");
            Assert.Equal(["b", "c"], source.Items.Cast<DynamicTableViewTestRow>().Select(static row => row.Id));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Right_click_targets_clicked_row_or_existing_multi_selection()
    {
        using SourceCache<DynamicTableViewTestRow, string> cache = new(static row => row.Id);
        using var source = DynamicTableViewTestData.CreateSource(cache);
        cache.AddOrUpdate(DynamicTableViewTestData.CreateRows());
        DynamicTableView table = new() { Source = source };
        DynamicTableViewContextMenuEventArgs? received = null;
        table.ContextMenuRequested += (_, args) => received = args;
        Window window = new() { Width = 420, Height = 240, Content = table };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            source.SelectionModel.Select(0);
            source.SelectionModel.Select(1);
            RightClickRow(window, table, "b");

            Assert.Equal("b", received?.ClickedItem is DynamicTableViewTestRow clicked ? clicked.Id : null);
            Assert.Equal(["a", "b"], received?.TargetItems.Cast<DynamicTableViewTestRow>().Select(static item => item.Id));

            RightClickRow(window, table, "c");

            Assert.Equal("c", received?.ClickedItem is DynamicTableViewTestRow unselected ? unselected.Id : null);
            Assert.Equal(["c"], received?.TargetItems.Cast<DynamicTableViewTestRow>().Select(static item => item.Id));
        }
        finally
        {
            window.Close();
        }
    }

    private static void RightClickRow(Window window, DynamicTableView table, string id)
    {
        var row = table.GetVisualDescendants().OfType<TableViewRow>()
            .Single(item => item.DataContext is DynamicTableViewTestRow rowData && rowData.Id == id);
        var position = row.TranslatePoint(new Point(5, 5), window) ?? throw new InvalidOperationException("Row has no window position.");
        window.MouseDown(position, MouseButton.Right);
        window.MouseUp(position, MouseButton.Right);
        Dispatcher.UIThread.RunJobs();
    }

    private static void ApplyStandardFilter(DynamicTableView table, int columnIndex, int operatorIndex, string firstValue)
    {
        var header = Assert.IsAssignableFrom<Control>(table.Columns[columnIndex].Header);
        var filterButton = Assert.Single(header.GetVisualDescendants().OfType<Button>()
            .Where(static button => button.Classes.Contains("dynamic-table-view-filter-button")));
        var flyout = Assert.IsType<Flyout>(FlyoutBase.GetAttachedFlyout(filterButton));
        FlyoutBase.ShowAttachedFlyout(filterButton);
        Dispatcher.UIThread.RunJobs();
        var content = Assert.IsAssignableFrom<Control>(flyout.Content);
        var comboBoxes = content.GetVisualDescendants().OfType<ComboBox>().ToArray();
        comboBoxes[0].SelectedIndex = operatorIndex;
        var valueBox = content.GetVisualDescendants().OfType<TextBox>()
            .Single(static textBox => textBox.Name == "PART_ValueBox");
        valueBox.Text = firstValue;
        Assert.Equal(firstValue, valueBox.Text);
        content.GetVisualDescendants().OfType<Button>()
            .Single(static button => button.Name == "PART_ApplyButton")
            .RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Dispatcher.UIThread.RunJobs();
        flyout.Hide();
    }
}
