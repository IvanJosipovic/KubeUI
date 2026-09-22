using BenchmarkDotNet.Attributes;
using KubeUI.Avalonia.Infrastructure.DataGrid;

namespace KubeUI.Benchmarks;

[MemoryDiagnoser]
[ShortRunJob]
[BenchmarkCategory("DataGrid", "Selection")]
public class IdentityPreservingSelectionModelBenchmarks : IDisposable
{
    private IdentityPreservingSelectionModel<BenchmarkItem, string> _model = null!;
    private List<BenchmarkItem> _source = null!;
    private BenchmarkItem[] _orderA = null!;
    private BenchmarkItem[] _orderB = null!;
    private bool _useSourceB;

    [Params(100, 1_000, 10_000)]
    public int ItemCount { get; set; }

    [Params(1, 10, 100)]
    public int SelectedCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        var items = new BenchmarkItem[ItemCount];
        for (var index = 0; index < items.Length; index++)
        {
            items[index] = new BenchmarkItem($"item-{index:D5}");
        }

        var selectionCount = Math.Min(SelectedCount, ItemCount);
        _orderA = items;
        _orderB = new BenchmarkItem[ItemCount];
        Array.Copy(items, selectionCount, _orderB, 0, ItemCount - selectionCount);
        Array.Copy(items, 0, _orderB, ItemCount - selectionCount, selectionCount);
        _source = new List<BenchmarkItem>(items);
        _model = new IdentityPreservingSelectionModel<BenchmarkItem, string>(static item => item.Id)
        {
            Source = _source
        };
        _model.SetIdentitySource(_source);

        for (var index = 0; index < selectionCount; index++)
        {
            _model.Select(index);
        }
    }

    [IterationSetup]
    public void Reset()
    {
        _useSourceB = !_useSourceB;
        _source.Clear();
        _source.AddRange(_useSourceB ? _orderB : _orderA);
    }

    [Benchmark]
    public int RestoreSelectionByIdentity()
    {
        _model.SetIdentitySource(_source);
        return _model.SelectedIndexes.Count;
    }

    public void Dispose()
    {
        _model.Dispose();
    }

    private sealed class BenchmarkItem
    {
        public BenchmarkItem(string id)
        {
            Id = id;
        }

        public string Id { get; }
    }
}
