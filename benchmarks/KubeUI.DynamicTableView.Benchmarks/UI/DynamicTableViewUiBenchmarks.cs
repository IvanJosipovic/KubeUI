using System.Reactive.Concurrency;
using Avalonia;
using Avalonia.Controls;
using Avalonia.Controls.Primitives;
using Avalonia.Headless;
using Avalonia.Input;
using Avalonia.Interactivity;
using Avalonia.Threading;
using Avalonia.VisualTree;
using BenchmarkDotNet.Attributes;
using DynamicData;
using KubeUI.DynamicTableView;

namespace KubeUI.DynamicTableView.Benchmarks.UI;

[MemoryDiagnoser]
public class DynamicTableViewUiBenchmarks : IDisposable
{
    private SourceCache<DynamicTableViewBenchmarkRow, int> _cache = null!;
    private DynamicTableViewSource<DynamicTableViewBenchmarkRow, int> _source = null!;
    private DynamicTableView _table = null!;
    private Window _window = null!;
    private ScrollViewer _scrollViewer = null!;
    private Button _filterButton = null!;
    private int _contextTargetCount;
    private bool _longName;

    [Params(100, 1_000, 10_000)]
    public int RowCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        AppBuilder.Configure<DynamicTableViewBenchmarkApp>()
            .UseHeadless(new AvaloniaHeadlessPlatformOptions())
            .SetupWithoutStarting();

        _cache = new(static row => row.Id);
        _source = new(_cache.Connect(), static row => row.Id, CreateColumns(),
            ImmediateScheduler.Instance, ImmediateScheduler.Instance, TimeSpan.Zero);
        var rows = new DynamicTableViewBenchmarkRow[RowCount];
        for (var index = 0; index < rows.Length; index++)
            rows[index] = new(index, $"Item {index:D5}", index % 101, index % 2 == 0);
        _cache.AddOrUpdate(rows);

        _table = new() { Source = _source };
        _table.ContextMenuRequested += (_, args) => _contextTargetCount = args.TargetItems.Count;
        _window = new() { Width = 1000, Height = 700, Content = _table };
        _window.Show();
        Dispatcher.UIThread.RunJobs();
        _scrollViewer = _table.GetVisualDescendants().OfType<ScrollViewer>().First();
        var header = _table.GetVisualDescendants().OfType<Control>()
            .Single(static control => control.Classes.Contains("dynamic-table-view-header") &&
                control.DataContext is DynamicTableViewColumn column && column.Key == "name");
        _filterButton = header.GetVisualDescendants().OfType<Button>()
            .Single(static button => button.Classes.Contains("dynamic-table-view-filter-button"));
    }

    [GlobalCleanup]
    public void Cleanup()
        => Dispose();

    public void Dispose()
    {
        if (_window is not null)
        {
            _window.Close();
            Dispatcher.UIThread.RunJobs();
        }

        _source?.Dispose();
        _cache?.Dispose();
    }

    [Benchmark]
    public int ScrollAndRecycle()
    {
        var maximum = Math.Max(0, _scrollViewer.Extent.Height - _scrollViewer.Viewport.Height);
        _scrollViewer.Offset = new Vector(0, _scrollViewer.Offset.Y == 0 ? maximum : 0);
        Dispatcher.UIThread.RunJobs();
        return _table.GetVisualDescendants().OfType<TableViewRow>().Count();
    }

    [Benchmark]
    public double AutoWidthAfterSourceUpdate()
    {
        _longName = !_longName;
        var visibleRow = _table.GetVisualDescendants().OfType<TableViewRow>().First();
        var current = (DynamicTableViewBenchmarkRow)visibleRow.DataContext!;
        _cache.AddOrUpdate(current with { Name = _longName ? new string('W', 200) : $"Item {current.Id:D5}" });
        Dispatcher.UIThread.RunJobs();
        return _table.Columns[1].ActualWidth;
    }

    [Benchmark]
    public int OpenApplyAndClearFilter()
    {
        _source.ClearFilters();
        Dispatcher.UIThread.RunJobs();
        FlyoutBase.ShowAttachedFlyout(_filterButton);
        Dispatcher.UIThread.RunJobs();
        var flyout = (Flyout)FlyoutBase.GetAttachedFlyout(_filterButton)!;
        if (flyout.Content is not Control content)
            throw new InvalidOperationException("Filter flyout content is not available.");
        var valueBox = content.GetVisualDescendants().OfType<TextBox>()
            .Single(static textBox => textBox.Name == "PART_ValueBox");
        var applyButton = content.GetVisualDescendants().OfType<Button>().Single(static button => button.Name == "PART_ApplyButton");
        valueBox.Text = "Item 000";
        applyButton.RaiseEvent(new RoutedEventArgs(Button.ClickEvent));
        Dispatcher.UIThread.RunJobs();
        flyout.Hide();
        return _source.Items.Cast<DynamicTableViewBenchmarkRow>().Count();
    }

    [Benchmark]
    public double RestoreState()
    {
        var state = _table.CaptureState();
        _table.RestoreState(state);
        Dispatcher.UIThread.RunJobs();
        return _scrollViewer.Offset.Y;
    }

    [Benchmark]
    public int RightClickSelectionTargeting()
    {
        _source.SelectionModel.Clear();
        _source.SelectionModel.Select(0);
        _source.SelectionModel.Select(1);
        var selectedRow = _table.GetVisualDescendants().OfType<TableViewRow>()
            .First(row => row.DataContext is DynamicTableViewBenchmarkRow item && item.Id == 0);
        var position = selectedRow.TranslatePoint(new Point(5, 5), _window)
            ?? throw new InvalidOperationException("The selected row has no window position.");
        _window.MouseDown(position, MouseButton.Right);
        _window.MouseUp(position, MouseButton.Right);
        Dispatcher.UIThread.RunJobs();
        return _contextTargetCount;
    }

    private static DynamicTableViewColumn<DynamicTableViewBenchmarkRow>[] CreateColumns()
    {
        var columns = new DynamicTableViewColumn<DynamicTableViewBenchmarkRow>[20];
        columns[0] = DynamicTableViewColumn<DynamicTableViewBenchmarkRow>.Create("id", "Id", static row => row.Id);
        columns[1] = DynamicTableViewColumn<DynamicTableViewBenchmarkRow>.Create("name", "Name", static row => row.Name);
        columns[2] = DynamicTableViewColumn<DynamicTableViewBenchmarkRow>.Create("score", "Score", static row => row.Score);
        columns[3] = DynamicTableViewColumn<DynamicTableViewBenchmarkRow>.Create("enabled", "Enabled", static row => row.Enabled);
        for (var index = 4; index < columns.Length; index++)
        {
            var columnIndex = index;
            columns[index] = DynamicTableViewColumn<DynamicTableViewBenchmarkRow>.Create(
                $"extra-{index}", $"Extra {index}", row => $"{row.Id}:{columnIndex}");
        }

        return columns;
    }
}
