using Avalonia.Controls;
using Avalonia.LogicalTree;
using AvaloniaEdit.Folding;
using BenchmarkDotNet.Attributes;
using KubeUI.Avalonia.Features.Resources.Yaml.Behaviors;

namespace KubeUI.Benchmarks;

[MemoryDiagnoser]
[BenchmarkCategory("YAML", "Leak")]
public class FoldingMarginLeakBenchmarks
{
    [Params(100, 1_000)]
    public int RefreshCount { get; set; }

    [Params(10)]
    public int MarkersPerRefresh { get; set; }

    [Benchmark(Baseline = true)]
    public int StockLifecycle_RetainedLogicalChildren()
    {
        var margin = new FoldingMargin();
        AddMarkers(margin, RefreshCount, MarkersPerRefresh, clearBeforeRefresh: false);
        return CountLogicalChildren(margin);
    }

    [Benchmark]
    public int LeakSafeLifecycle_RetainedLogicalChildren()
    {
        var margin = new FoldingMargin();
        AddMarkers(margin, RefreshCount, MarkersPerRefresh, clearBeforeRefresh: true);
        return CountLogicalChildren(margin);
    }

    private static void AddMarkers(
        FoldingMargin margin,
        int refreshCount,
        int markersPerRefresh,
        bool clearBeforeRefresh)
    {
        for (var refresh = 0; refresh < refreshCount; refresh++)
        {
            if (clearBeforeRefresh)
            {
                LeakSafeFoldingMargin.ClearLogicalChildren(margin);
            }

            for (var marker = 0; marker < markersPerRefresh; marker++)
            {
                ((ISetLogicalParent)new Control()).SetParent(margin);
            }
        }
    }

    private static int CountLogicalChildren(FoldingMargin margin)
    {
        return ((ILogical)margin).LogicalChildren.Count();
    }
}
