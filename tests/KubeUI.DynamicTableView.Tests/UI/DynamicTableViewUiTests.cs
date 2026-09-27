using System.ComponentModel;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Controls.Presenters;
using Avalonia.Headless;
using Avalonia.Headless.XUnit;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Media;
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
    public void Clicking_blank_header_area_sorts_and_keeps_indicator_next_to_text()
    {
        using SourceCache<DynamicTableViewTestRow, string> cache = new(static row => row.Id);
        var nameColumn = DynamicTableViewColumn<DynamicTableViewTestRow>.Create("name", "Name", static row => row.Name);
        nameColumn.WidthMode = DynamicTableViewWidthMode.Pixel;
        nameColumn.Width = 220;
        using var source = DynamicTableViewTestData.CreateSource(cache, [nameColumn]);
        cache.AddOrUpdate([
            DynamicTableViewTestData.CreateRows()[0] with { Name = "Gamma" },
            DynamicTableViewTestData.CreateRows()[1] with { Name = "Alpha" },
            DynamicTableViewTestData.CreateRows()[2] with { Name = "Beta" }]);
        DynamicTableView table = new() { Source = source };
        Window window = new() { Width = 640, Height = 320, Content = table };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var header = GetHeaderControl(table, 0);
            var filterButton = Assert.Single(header.GetVisualDescendants().OfType<Button>()
                .Where(static button => button.Classes.Contains("dynamic-table-view-filter-button")));
            var filterOrigin = filterButton.TranslatePoint(default, header)
                ?? throw new InvalidOperationException("Filter button has no header position.");
            var blankHeaderPoint = header.TranslatePoint(
                new Point(filterOrigin.X - 8, header.Bounds.Height / 2), window)
                ?? throw new InvalidOperationException("Header has no window position.");

            window.MouseDown(blankHeaderPoint, MouseButton.Left);
            window.MouseUp(blankHeaderPoint, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();

            Assert.Single(source.SortDescriptors);
            Assert.Equal(["b", "c", "a"], source.Items.Cast<DynamicTableViewTestRow>().Select(static row => row.Id));
            Assert.Single(header.GetVisualDescendants().OfType<Button>());
            var label = Assert.Single(header.GetVisualDescendants().OfType<TextBlock>()
                .Where(static textBlock => textBlock.Text == "Name"));
            var sortIcon = Assert.Single(header.GetVisualDescendants().OfType<PathIcon>()
                .Where(icon => icon.IsVisible && !icon.GetSelfAndVisualAncestors().OfType<Button>().Contains(filterButton)));
            var labelOrigin = label.TranslatePoint(default, header)!.Value;
            var sortIconOrigin = sortIcon.TranslatePoint(default, header)!.Value;
            Assert.True(sortIconOrigin.X >= labelOrigin.X + label.DesiredSize.Width);
            Assert.True(sortIconOrigin.X < filterOrigin.X);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Column_header_strip_is_taller_than_data_rows()
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

            var headerPresenter = Assert.IsType<TableViewColumnHeadersPresenter>(table.GetVisualDescendants().OfType<TableViewColumnHeadersPresenter>().First());
            var headerStrip = Assert.IsType<Border>(headerPresenter.GetSelfAndVisualAncestors().OfType<Border>().First());
            var row = Assert.IsType<TableViewRow>(table.GetVisualDescendants().OfType<TableViewRow>().First());

            Assert.True(headerStrip.Bounds.Height >= row.Bounds.Height + 6,
                $"Expected header strip ({headerStrip.Bounds.Height}) to be at least 6 pixels taller than row ({row.Bounds.Height}).");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void All_column_headers_are_templated_with_many_columns()
    {
        using SourceCache<DynamicTableViewTestRow, string> cache = new(static row => row.Id);
        using var source = DynamicTableViewTestData.CreateSource(cache);
        for (var index = source.Columns.Count; index < 11; index++)
            source.Columns.Add(DynamicTableViewColumn<DynamicTableViewTestRow>.Create($"extra-{index}", $"Extra {index}", static row => row.Id));
        DynamicTableView table = new() { Source = source };
        Window window = new() { Width = 1200, Height = 320, Content = table };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            AssertVisibleHeaders(table, source.Columns.Select(static column => column.Header.ToString() ?? string.Empty));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Restoring_reordered_state_keeps_all_column_headers_visible()
    {
        using SourceCache<DynamicTableViewTestRow, string> cache = new(static row => row.Id);
        using var source = DynamicTableViewTestData.CreateSource(cache);
        for (var index = source.Columns.Count; index < 11; index++)
            source.Columns.Add(DynamicTableViewColumn<DynamicTableViewTestRow>.Create($"extra-{index}", $"Extra {index}", static row => row.Id));
        DynamicTableView table = new() { Source = source };
        Window window = new() { Width = 1200, Height = 320, Content = table };
        var expectedHeaders = source.Columns.Select(static column => column.Header.ToString() ?? string.Empty).ToArray();
        var savedColumns = source.Columns
            .Select((column, index) => new DynamicTableViewColumnState(column.Key, index, 140))
            .ToArray();
        savedColumns[0] = savedColumns[0] with { Order = 1 };
        savedColumns[1] = savedColumns[1] with { Order = 0 };
        (expectedHeaders[0], expectedHeaders[1]) = (expectedHeaders[1], expectedHeaders[0]);

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            AssertVisibleHeaders(table, source.Columns.Select(static column => column.Header.ToString() ?? string.Empty));

            table.RestoreState(new(savedColumns, [], [], default));
            Dispatcher.UIThread.RunJobs();

            AssertVisibleHeaders(table, expectedHeaders);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Grid_line_visibility_styles_rows_cells_and_headers()
    {
        using SourceCache<DynamicTableViewTestRow, string> cache = new(static row => row.Id);
        using var source = DynamicTableViewTestData.CreateSource(cache);
        cache.AddOrUpdate(DynamicTableViewTestData.CreateRows());
        DynamicTableView table = new() { Source = source };
        var lineBrush = new SolidColorBrush(Colors.Orange);
        table.Resources["TableViewColumnHeaderSeparatorBackground"] = lineBrush;
        Window window = new() { Width = 420, Height = 240, Content = table };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var cells = table.GetVisualDescendants().OfType<TableViewCell>().ToArray();
            var rows = table.GetVisualDescendants().OfType<TableViewRow>().ToArray();
            var headers = table.GetVisualDescendants().OfType<TableViewColumnHeader>().ToArray();

            Assert.NotEmpty(cells);
            Assert.NotEmpty(rows);
            Assert.NotEmpty(headers);
            Assert.Equal(DynamicTableViewGridLinesVisibility.None, table.GridLinesVisibility);

            (DynamicTableViewGridLinesVisibility Visibility, Thickness RowThickness, Thickness CellThickness, Thickness HeaderThickness)[] modes =
            [
                (DynamicTableViewGridLinesVisibility.None, new Thickness(0), new Thickness(0), new Thickness(0)),
                (DynamicTableViewGridLinesVisibility.Horizontal, new Thickness(0, 0, 0, 1), new Thickness(0), new Thickness(0, 0, 0, 1)),
                (DynamicTableViewGridLinesVisibility.Vertical, new Thickness(0), new Thickness(0, 0, 1, 0), new Thickness(0, 0, 1, 0)),
                (DynamicTableViewGridLinesVisibility.All, new Thickness(0, 0, 0, 1), new Thickness(0, 0, 1, 0), new Thickness(0, 0, 1, 1))
            ];

            foreach (var mode in modes)
            {
                table.GridLinesVisibility = mode.Visibility;
                Dispatcher.UIThread.RunJobs();

                Assert.All(rows, row => Assert.Equal(mode.RowThickness, row.BorderThickness));
                Assert.All(cells, cell => Assert.Equal(mode.CellThickness, cell.BorderThickness));
                Assert.All(headers, header => Assert.Equal(mode.HeaderThickness, header.BorderThickness));
                if (mode.Visibility != DynamicTableViewGridLinesVisibility.None)
                {
                    if (mode.RowThickness.Bottom > 0)
                    {
                        Assert.All(rows, row =>
                        {
                            Assert.Same(lineBrush, row.BorderBrush);
                            var rowBorder = Assert.Single(row.GetVisualDescendants().OfType<Border>());
                            Assert.Equal(row.Bounds.Height, rowBorder.Bounds.Height);
                        });
                    }
                    if (mode.CellThickness.Right > 0)
                        Assert.All(cells, cell => Assert.Same(lineBrush, cell.BorderBrush));
                    if (mode.HeaderThickness.Right > 0 || mode.HeaderThickness.Bottom > 0)
                        Assert.All(headers, header => Assert.Same(lineBrush, header.BorderBrush));
                }
            }
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Caller_added_key_binding_executes_for_a_custom_gesture()
    {
        DynamicTableView table = new() { Focusable = true };
        CallbackCommand command = new();
        table.KeyBindings.Add(new KeyBinding
        {
            Gesture = new KeyGesture(Key.K, KeyModifiers.Control),
            Command = command
        });
        Window window = new() { Width = 300, Height = 160, Content = table };

        try
        {
            window.Show();
            table.Focus();
            window.KeyPress(Key.K, RawInputModifiers.Control, PhysicalKey.K, null);

            Assert.Equal(1, command.ExecutionCount);
        }
        finally
        {
            window.Close();
        }
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
    public void Restored_table_headers_remain_visible_after_switching_tabs()
    {
        using SourceCache<DynamicTableViewTestRow, string> cache = new(static row => row.Id);
        using var source = DynamicTableViewTestData.CreateSource(cache);
        cache.AddOrUpdate(DynamicTableViewTestData.CreateRows());
        DynamicTableView table = new() { Source = source };
        TabControl tabs = new()
        {
            Items =
            {
                new TabItem { Header = "Resources", Content = table },
                new TabItem { Header = "Other", Content = new Border() }
            }
        };
        Window window = new() { Width = 640, Height = 360, Content = tabs };
        string[] initialHeaders = ["Name", "Age", "Created", "Enabled", "State"];
        string[] restoredHeaders = ["Age", "Name", "Created", "Enabled", "State"];
        var attachCount = 0;
        var detachCount = 0;
        table.AttachedToVisualTree += (_, _) => attachCount++;
        table.DetachedFromVisualTree += (_, _) => detachCount++;

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            AssertVisibleHeaders(table, initialHeaders);

            table.RestoreState(new(
                [new("age", 0, 120), new("name", 1, 180)],
                [],
                [],
                default));
            Dispatcher.UIThread.RunJobs();

            tabs.SelectedIndex = 1;
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(1, detachCount);
            tabs.SelectedIndex = 0;
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(2, attachCount);
            AssertVisibleHeaders(table, restoredHeaders);
            Assert.Equal(["age", "name"], source.Columns.Take(2).Select(static column => column.Key));
            Assert.Equal(120, table.Columns[0].Width.Value);
            Assert.Equal(180, table.Columns[1].Width.Value);
        }
        finally
        {
            window.Close();
        }
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
    public void Auto_width_grows_from_realized_cells_and_honors_header_and_pixel_modes()
    {
        using SourceCache<DynamicTableViewTestRow, string> cache = new(static row => row.Id);
        var automatic = DynamicTableViewColumn<DynamicTableViewTestRow>.Create(
            "name", "Name", static row => row.Name);
        automatic.WidthMode = DynamicTableViewWidthMode.Auto;
        automatic.MinWidth = 30;
        var headerOnly = DynamicTableViewColumn<DynamicTableViewTestRow>.Create(
            "id", "Id", static row => row.Id);
        headerOnly.WidthMode = DynamicTableViewWidthMode.Header;
        headerOnly.MinWidth = 30;
        var fixedWidth = DynamicTableViewColumn<DynamicTableViewTestRow>.Create(
            "age", "Age", static row => row.Age);
        fixedWidth.WidthMode = DynamicTableViewWidthMode.Pixel;
        fixedWidth.Width = 123;
        fixedWidth.MinWidth = 30;
        using var source = DynamicTableViewTestData.CreateSource(cache, [automatic, headerOnly, fixedWidth]);
        var item = DynamicTableViewTestData.CreateRows()[0] with { Id = new string('I', 80), Name = "Short" };
        cache.AddOrUpdate(item);
        DynamicTableView table = new() { Source = source };
        Window window = new() { Width = 700, Height = 260, Content = table };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var initialAutoWidth = table.Columns[0].ActualWidth;

            cache.AddOrUpdate(item with { Name = new string('W', 80) });
            Dispatcher.UIThread.RunJobs();
            Dispatcher.UIThread.RunJobs();

            Assert.True(table.Columns[0].ActualWidth > initialAutoWidth);
            Assert.True(table.Columns[0].ActualWidth > table.Columns[1].ActualWidth);
            Assert.Equal(123, table.Columns[2].ActualWidth);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Dragging_a_header_reorders_the_source_and_native_columns()
    {
        using SourceCache<DynamicTableViewTestRow, string> cache = new(static row => row.Id);
        using var source = DynamicTableViewTestData.CreateSource(cache);
        source.Columns[0].WidthMode = DynamicTableViewWidthMode.Pixel;
        source.Columns[0].Width = 150;
        source.Columns[1].WidthMode = DynamicTableViewWidthMode.Pixel;
        source.Columns[1].Width = 230;
        DynamicTableView table = new() { Source = source };
        Window window = new() { Width = 800, Height = 260, Content = table };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var firstHeader = GetHeaderControl(table, 0);
            var secondHeader = GetHeaderControl(table, 1);
            var start = firstHeader.TranslatePoint(new Point(firstHeader.Bounds.Width / 2, firstHeader.Bounds.Height / 2), window);
            var target = secondHeader.TranslatePoint(new Point(secondHeader.Bounds.Width / 2, secondHeader.Bounds.Height / 2), window);
            Assert.NotNull(start);
            Assert.NotNull(target);

            window.MouseDown(start!.Value, MouseButton.Left);
            window.MouseMove(target!.Value);
            window.MouseUp(target.Value, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();

            Assert.Equal("age", source.Columns[1].Key);
            Assert.Equal(230, table.Columns[1].Width.Value);
            Assert.Equal(source.Columns.Count, table.Columns.Count);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Virtualized_cells_rebind_to_current_rows_after_scrolling()
    {
        using SourceCache<DynamicTableViewTestRow, string> cache = new(static row => row.Id);
        var name = DynamicTableViewColumn<DynamicTableViewTestRow>.Create("name", "Name", static row => row.Name);
        using var source = DynamicTableViewTestData.CreateSource(cache, [name]);
        cache.AddOrUpdate(Enumerable.Range(0, 1_000).Select(index =>
            new DynamicTableViewTestRow($"row-{index:D4}", $"Item {index:D4}", index, DateTimeOffset.UnixEpoch, true, DynamicTableViewTestState.Ready)));
        DynamicTableView table = new() { Source = source };
        Window window = new() { Width = 420, Height = 240, Content = table };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var scrollViewer = Assert.Single(table.GetVisualDescendants().OfType<ScrollViewer>());
            var firstRows = table.GetVisualDescendants().OfType<TableViewRow>().ToArray();
            Assert.InRange(firstRows.Length, 1, 99);
            scrollViewer.Offset = new Vector(0, 8_000);
            Dispatcher.UIThread.RunJobs();

            var visibleRows = table.GetVisualDescendants().OfType<TableViewRow>().ToArray();
            Assert.InRange(visibleRows.Length, 1, 99);
            Assert.All(visibleRows, row =>
            {
                var item = Assert.IsType<DynamicTableViewTestRow>(row.DataContext);
                Assert.Contains(row.GetVisualDescendants().OfType<TextBlock>(), textBlock => textBlock.Text == item.Name);
            });
            Assert.Contains(visibleRows, row => ((DynamicTableViewTestRow)row.DataContext!).Id.CompareTo("row-0100") > 0);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Updating_rows_and_changing_selection_do_not_scroll_offscreen_items_into_view()
    {
        using SourceCache<DynamicTableViewTestRow, string> cache = new(static row => row.Id);
        using var source = new DynamicTableViewSource<DynamicTableViewTestRow, string>(
            cache.Connect(),
            static row => row.Id,
            DynamicTableViewTestData.CreateColumns(),
            workerScheduler: ImmediateScheduler.Instance,
            searchDebounce: TimeSpan.Zero,
            searchScheduler: ImmediateScheduler.Instance);
        var rows = Enumerable.Range(0, 1_000)
            .Select(index => new DynamicTableViewTestRow(
                $"row-{index:D4}", $"Item {index:D4}", index, DateTimeOffset.UnixEpoch, true, DynamicTableViewTestState.Ready))
            .ToArray();
        cache.AddOrUpdate(rows);
        source.SetSort([new("name", ListSortDirection.Ascending)]);
        DynamicTableView table = new() { Source = source };
        Window window = new() { Width = 420, Height = 240, Content = table };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            var scrollViewer = Assert.Single(table.GetVisualDescendants().OfType<ScrollViewer>());
            Assert.Equal(1_000, source.Items.Cast<DynamicTableViewTestRow>().Count());
            Assert.True(scrollViewer.ScrollBarMaximum.Y > 0);
            Assert.False(table.AutoScrollToSelectedItem);
            source.SelectionModel.Select(500);
            Dispatcher.UIThread.RunJobs();
            var scrolledOffset = scrollViewer.Offset.Y;

            Assert.Equal(0, scrolledOffset);

            cache.AddOrUpdate(rows[0] with { Name = "Zzz updated first row" });
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(scrolledOffset, scrollViewer.Offset.Y);
            Assert.Equal("row-0500", Assert.IsType<DynamicTableViewTestRow>(source.SelectionModel.SelectedItem).Id);

            source.SelectionModel.SelectedIndex = 900;
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(scrolledOffset, scrollViewer.Offset.Y);
            Assert.Equal("row-0901", Assert.IsType<DynamicTableViewTestRow>(source.SelectionModel.SelectedItem).Id);
        }
        finally
        {
            window.Close();
        }
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
    public void Dragging_across_rows_selects_and_deselects_range_while_left_button_is_held()
    {
        using SourceCache<DynamicTableViewTestRow, string> cache = new(static row => row.Id);
        using var source = DynamicTableViewTestData.CreateSource(cache);
        cache.AddOrUpdate(Enumerable.Range(0, 12).Select(index =>
            new DynamicTableViewTestRow($"row-{index:D2}", $"Item {index:D2}", index, DateTimeOffset.UnixEpoch, true, DynamicTableViewTestState.Ready)));
        DynamicTableView table = new() { Source = source };
        Window window = new() { Width = 700, Height = 420, Content = table };
        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var first = GetRowCenter(window, table, "row-00");
            var tenth = GetRowCenter(window, table, "row-09");
            var hit = table.GetVisualAt(window.TranslatePoint(tenth, table) ?? tenth);
            var hitRow = hit!.GetSelfAndVisualAncestors().OfType<TableViewRow>().Single(row =>
                row.DataContext is DynamicTableViewTestRow data && data.Id == "row-09");
            Assert.Equal(9, table.IndexFromContainer(hitRow));
            window.MouseDown(first, MouseButton.Left);
            window.MouseMove(tenth, RawInputModifiers.LeftMouseButton);
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(10, source.SelectionModel.SelectedItems.Count);
            Assert.Equal(Enumerable.Range(0, 10), source.SelectionModel.SelectedIndexes.Order());

            var fifth = GetRowCenter(window, table, "row-04");
            window.MouseMove(fifth, RawInputModifiers.LeftMouseButton);
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(Enumerable.Range(0, 5), source.SelectionModel.SelectedIndexes.Order());

            window.MouseMove(tenth, RawInputModifiers.LeftMouseButton);
            Dispatcher.UIThread.RunJobs();
            Assert.Equal(Enumerable.Range(0, 10), source.SelectionModel.SelectedIndexes.Order());

            window.MouseUp(tenth, MouseButton.Left);
            Dispatcher.UIThread.RunJobs();

            var selectedStart = GetRowCenter(window, table, "row-00");
            var selectedEnd = GetRowCenter(window, table, "row-09");
            window.MouseDown(selectedStart, MouseButton.Left);
            window.MouseMove(selectedEnd, RawInputModifiers.LeftMouseButton);
            Dispatcher.UIThread.RunJobs();

            Assert.Empty(source.SelectionModel.SelectedItems);
            window.MouseUp(selectedEnd, MouseButton.Left);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Header_click_cycles_ascending_descending_and_unsorted()
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
            var nameHeader = GetHeaderControl(table, 0);
            ClickHeader(window, nameHeader, 12);
            Assert.Equal(["b", "c", "a"], source.Items.Cast<DynamicTableViewTestRow>().Select(static row => row.Id));

            ClickHeader(window, nameHeader, 12);
            Assert.Equal(["a", "c", "b"], source.Items.Cast<DynamicTableViewTestRow>().Select(static row => row.Id));

            ClickHeader(window, nameHeader, 12);
            Assert.Equal(["a", "b", "c"], source.Items.Cast<DynamicTableViewTestRow>().Select(static row => row.Id));
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Multiple_sorts_are_disabled_by_default_and_the_clicked_column_replaces_the_previous_sort()
    {
        using SourceCache<DynamicTableViewTestRow, string> cache = new(static row => row.Id);
        using var source = DynamicTableViewTestData.CreateSource(cache);
        cache.AddOrUpdate([
            new DynamicTableViewTestRow("a", "Zeta", 30, DateTimeOffset.UnixEpoch, true, DynamicTableViewTestState.Ready),
            new DynamicTableViewTestRow("b", "Alpha", 10, DateTimeOffset.UnixEpoch, true, DynamicTableViewTestState.Ready),
            new DynamicTableViewTestRow("c", "Beta", 20, DateTimeOffset.UnixEpoch, true, DynamicTableViewTestState.Ready)]);
        DynamicTableView table = new() { Source = source };
        Window window = new() { Width = 640, Height = 320, Content = table };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            Assert.False(table.AllowMultipleSorts);
            ClickHeader(window, GetHeaderControl(table, 0), 12);
            ClickHeader(window, GetHeaderControl(table, 1), 12);

            var activeSort = Assert.Single(source.SortDescriptors);
            Assert.Equal("age", activeSort.ColumnKey);
            Assert.Equal(ListSortDirection.Ascending, activeSort.Direction);

            ClickHeader(window, GetHeaderControl(table, 1), 12);
            activeSort = Assert.Single(source.SortDescriptors);
            Assert.Equal(ListSortDirection.Descending, activeSort.Direction);

            ClickHeader(window, GetHeaderControl(table, 1), 12);
            Assert.Empty(source.SortDescriptors);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public void Enabling_multiple_sorts_allows_two_sort_descriptors()
    {
        using SourceCache<DynamicTableViewTestRow, string> cache = new(static row => row.Id);
        using var source = DynamicTableViewTestData.CreateSource(cache);
        DynamicTableView table = new() { Source = source, AllowMultipleSorts = true };
        Window window = new() { Width = 640, Height = 320, Content = table };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();
            ClickHeader(window, GetHeaderControl(table, 0), 12);
            ClickHeader(window, GetHeaderControl(table, 1), 12);

            Assert.Equal(["name", "age"], source.SortDescriptors.Select(static descriptor => descriptor.ColumnKey));
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
            var header = GetHeaderControl(table, 0);
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
    public void Standard_filter_flyout_uses_default_width_and_allows_host_override()
    {
        using SourceCache<DynamicTableViewTestRow, string> cache = new(static row => row.Id);
        using var source = DynamicTableViewTestData.CreateSource(cache);
        DynamicTableView table = new() { Source = source };
        table.Resources["DynamicTableView.FilterFlyoutWidth"] = 360d;
        Window window = new() { Width = 520, Height = 260, Content = table };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var header = GetHeaderControl(table, 0);
            var filterButton = Assert.Single(header.GetVisualDescendants().OfType<Button>()
                .Where(static button => button.Classes.Contains("dynamic-table-view-filter-button")));
            var flyout = Assert.IsType<Flyout>(FlyoutBase.GetAttachedFlyout(filterButton));
            FlyoutBase.ShowAttachedFlyout(filterButton);
            Dispatcher.UIThread.RunJobs();

            var flyoutContent = Assert.IsAssignableFrom<Control>(flyout.Content);
            var panel = Assert.Single(flyoutContent.GetVisualDescendants().OfType<StackPanel>()
                .Where(static panel => panel.Width == 360));
            Assert.Equal(360, panel.Width);
            Assert.Equal(360, panel.Bounds.Width);

            flyout.Hide();
            table.Resources.Remove("DynamicTableView.FilterFlyoutWidth");
            FlyoutBase.ShowAttachedFlyout(filterButton);
            Dispatcher.UIThread.RunJobs();

            Assert.Equal(280, panel.Width);
            Assert.Equal(280, panel.Bounds.Width);
            flyout.Hide();
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
            AssertStandardFilterState(table, 0, 0, "ph");

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
    public void Filter_icon_uses_accent_foreground_while_column_filter_is_active()
    {
        using SourceCache<DynamicTableViewTestRow, string> cache = new(static row => row.Id);
        using var source = DynamicTableViewTestData.CreateSource(cache);
        cache.AddOrUpdate(DynamicTableViewTestData.CreateRows());
        DynamicTableView table = new() { Source = source };
        SolidColorBrush accentBrush = new(Colors.Orange);
        table.Resources["SystemControlForegroundAccentBrush"] = accentBrush;
        Window window = new() { Width = 520, Height = 260, Content = table };

        try
        {
            window.Show();
            Dispatcher.UIThread.RunJobs();

            var header = GetHeaderControl(table, 0);
            var filterButton = Assert.Single(header.GetVisualDescendants().OfType<Button>()
                .Where(static button => button.Classes.Contains("dynamic-table-view-filter-button")));
            var filterIcon = Assert.Single(filterButton.GetVisualDescendants().OfType<PathIcon>());
            Assert.NotSame(accentBrush, filterIcon.Foreground);

            ApplyStandardFilter(table, 0, 0, "ph");

            Assert.Contains("filtered", header.Classes);
            Assert.Same(accentBrush, filterIcon.Foreground);

            source.ClearFilters();
            Dispatcher.UIThread.RunJobs();

            Assert.DoesNotContain("filtered", header.Classes);
            Assert.NotSame(accentBrush, filterIcon.Foreground);
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

    private static Point GetRowCenter(Window window, DynamicTableView table, string id)
    {
        var row = table.GetVisualDescendants().OfType<TableViewRow>()
            .Single(item => item.DataContext is DynamicTableViewTestRow rowData && rowData.Id == id);
        return row.TranslatePoint(new Point(row.Bounds.Width / 2, row.Bounds.Height / 2), window)
            ?? throw new InvalidOperationException("Row has no window position.");
    }

    private static void ClickHeader(Window window, Control header, double x)
    {
        var position = header.TranslatePoint(new Point(x, header.Bounds.Height / 2), window)
            ?? throw new InvalidOperationException("Header has no window position.");
        window.MouseDown(position, MouseButton.Left);
        window.MouseUp(position, MouseButton.Left);
        Dispatcher.UIThread.RunJobs();
    }

    private static void AssertVisibleHeaders(DynamicTableView table, IEnumerable<string> expectedHeaders)
    {
        var visibleLabels = table.GetVisualDescendants().OfType<TextBlock>()
            .Where(static textBlock => textBlock.IsVisible)
            .Select(static textBlock => textBlock.Text)
            .ToHashSet(StringComparer.Ordinal);
        var headerDetails = table.GetVisualDescendants().OfType<TableViewColumnHeader>()
            .Select(header => $"{header.Column?.Header} visible={header.IsVisible} bounds={header.Bounds} text=[{string.Join(",", header.GetVisualDescendants().OfType<TextBlock>().Select(static textBlock => textBlock.Text))}]");
        foreach (var header in expectedHeaders)
            Assert.True(visibleLabels.Contains(header), $"Missing visible header '{header}'. Visible labels: {string.Join(",", visibleLabels)}. Headers: {string.Join("; ", headerDetails)}");
    }

    private static Control GetHeaderControl(DynamicTableView table, int columnIndex)
        => table.GetVisualDescendants().OfType<TableViewColumnHeader>()
            .Single(header => ReferenceEquals(header.Column, table.Columns[columnIndex]))
            .GetVisualDescendants().OfType<TemplatedControl>().First();

    private static void ApplyStandardFilter(DynamicTableView table, int columnIndex, int operatorIndex, string firstValue)
    {
        var header = GetHeaderControl(table, columnIndex);
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

    private static void AssertStandardFilterState(DynamicTableView table, int columnIndex, int operatorIndex, string firstValue)
    {
        var header = GetHeaderControl(table, columnIndex);
        var filterButton = Assert.Single(header.GetVisualDescendants().OfType<Button>()
            .Where(static button => button.Classes.Contains("dynamic-table-view-filter-button")));
        var flyout = Assert.IsType<Flyout>(FlyoutBase.GetAttachedFlyout(filterButton));
        FlyoutBase.ShowAttachedFlyout(filterButton);
        Dispatcher.UIThread.RunJobs();

        var content = Assert.IsAssignableFrom<Control>(flyout.Content);
        Assert.Equal(operatorIndex, content.GetVisualDescendants().OfType<ComboBox>().First().SelectedIndex);
        Assert.Equal(firstValue, content.GetVisualDescendants().OfType<TextBox>()
            .Single(static textBox => textBox.Name == "PART_ValueBox").Text);
        flyout.Hide();
    }
}
