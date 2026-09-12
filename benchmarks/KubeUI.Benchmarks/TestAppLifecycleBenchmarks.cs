using BenchmarkDotNet.Attributes;
using KubeUI.Avalonia.Tests.Infra;

namespace KubeUI.Benchmarks;

[MemoryDiagnoser]
[ShortRunJob]
[BenchmarkCategory("Lifecycle")]
public class TestAppLifecycleBenchmarks
{
    private long _baselineBytes;

    [IterationSetup]
    public void CollectBaseline()
    {
        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        _baselineBytes = GC.GetTotalMemory(forceFullCollection: false);
    }

    [Benchmark]
    public void CreateAndDisposeTestApp()
    {
        using var app = new TestApp();
    }

    [Benchmark]
    public long LiveBytesAfterEightLifecycles()
    {
        for (var i = 0; i < 8; i++)
        {
            using var app = new TestApp();
        }

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        var liveBytes = GC.GetTotalMemory(forceFullCollection: false) - _baselineBytes;
        Console.WriteLine($"Live managed bytes after eight TestApp lifecycles: {liveBytes:N0}");
        return liveBytes;
    }
}
