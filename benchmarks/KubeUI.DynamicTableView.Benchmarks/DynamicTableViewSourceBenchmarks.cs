using System.ComponentModel;
using System.Reactive.Concurrency;
using BenchmarkDotNet.Attributes;
using DynamicData;
using KubeUI.DynamicTableView;

namespace KubeUI.DynamicTableView.Benchmarks;

[MemoryDiagnoser]
public class DynamicTableViewSourceBenchmarks : IDisposable
{
    private SourceCache<DynamicTableViewBenchmarkRow, int> _cache = null!;
    private DynamicTableViewSource<DynamicTableViewBenchmarkRow, int> _source = null!;
    private bool _searchFlip;

    [Params(100, 1_000, 10_000)]
    public int RowCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _cache = new(static row => row.Id);
        var columns = CreateColumns();
        _source = new(_cache.Connect(), static row => row.Id, columns, ImmediateScheduler.Instance, ImmediateScheduler.Instance, TimeSpan.Zero);
        var rows = new DynamicTableViewBenchmarkRow[RowCount];
        for (var i = 0; i < rows.Length; i++)
            rows[i] = new(i, $"Item {i:D5}", i % 101, i % 2 == 0);
        _cache.AddOrUpdate(rows);
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        Dispose();
    }

    public void Dispose()
    {
        _source?.Dispose();
        _cache?.Dispose();
    }

    [Benchmark]
    public int SourceUpdate()
    {
        var current = _cache.Lookup(RowCount / 2).Value;
        _cache.AddOrUpdate(current with { Score = current.Score + 1 });
        return _source.Items.Cast<DynamicTableViewBenchmarkRow>().Count();
    }

    [Benchmark]
    public int Sort()
    {
        _source.SetSort([new("score", ListSortDirection.Descending), new("name", ListSortDirection.Ascending)]);
        return _source.Items.Cast<DynamicTableViewBenchmarkRow>().First().Id;
    }

    [Benchmark]
    public int Filter()
    {
        _source.SetFilter(new("score", DynamicTableViewFilterOperator.GreaterThanOrEqual, 50));
        return _source.Items.Cast<DynamicTableViewBenchmarkRow>().Count();
    }

    [Benchmark]
    public int Search()
    {
        _searchFlip = !_searchFlip;
        _source.SearchText = _searchFlip ? "Item 000" : "Item 001";
        return _source.Items.Cast<DynamicTableViewBenchmarkRow>().Count();
    }

    [Benchmark]
    public int Selection()
    {
        if (_source.Items.Cast<DynamicTableViewBenchmarkRow>().Any())
            _source.SelectionModel.Select(0);
        var selectedCount = _source.SelectionModel.SelectedItems.Count;
        _source.SelectionModel.Clear();
        return selectedCount;
    }

    [Benchmark]
    public int CreateTwentyColumns()
        => CreateColumns().Length;

    [Benchmark]
    public int CombinedQuery()
    {
        _source.ClearFilters();
        _source.SearchText = "Item 00";
        _source.SetFilter(new("enabled", DynamicTableViewFilterOperator.Equals, true));
        _source.SetSort([new("score", ListSortDirection.Ascending)]);
        return _source.Items.Cast<DynamicTableViewBenchmarkRow>().Count();
    }

    private static DynamicTableViewColumn<DynamicTableViewBenchmarkRow>[] CreateColumns()
    {
        var columns = new DynamicTableViewColumn<DynamicTableViewBenchmarkRow>[20];
        columns[0] = DynamicTableViewColumn<DynamicTableViewBenchmarkRow>.Create("id", "Id", static row => row.Id);
        columns[1] = DynamicTableViewColumn<DynamicTableViewBenchmarkRow>.Create("name", "Name", static row => row.Name);
        columns[2] = DynamicTableViewColumn<DynamicTableViewBenchmarkRow>.Create("score", "Score", static row => row.Score);
        columns[3] = DynamicTableViewColumn<DynamicTableViewBenchmarkRow>.Create("enabled", "Enabled", static row => row.Enabled);
        for (var i = 4; i < columns.Length; i++)
        {
            var index = i;
            columns[i] = DynamicTableViewColumn<DynamicTableViewBenchmarkRow>.Create($"extra-{i}", $"Extra {i}", row => $"{row.Id}:{index}");
        }
        return columns;
    }
}
