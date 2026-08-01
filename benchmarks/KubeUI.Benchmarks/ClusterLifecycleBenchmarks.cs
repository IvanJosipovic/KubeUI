using BenchmarkDotNet.Attributes;
using KubeUI.Testing.Kubernetes.Bootstrap;

namespace KubeUI.Benchmarks;

[MemoryDiagnoser]
[ShortRunJob]
[BenchmarkCategory("Lifecycle")]
public class ClusterLifecycleBenchmarks
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
    public async Task CreateAndDisposeDisconnectedCluster()
    {
        await using var cluster = await new TestClusterGenerator().CreateAsync(
        new TestClusterConfig(),
            CancellationToken.None);
    }

    [Benchmark]
    public async Task<long> LiveBytesAfterEightLifecycles()
    {
        for (var i = 0; i < 8; i++)
        {
            await using var cluster = await new TestClusterGenerator().CreateAsync(
                new TestClusterConfig(),
                CancellationToken.None);
        }

        GC.Collect();
        GC.WaitForPendingFinalizers();
        GC.Collect();
        var liveBytes = GC.GetTotalMemory(forceFullCollection: false) - _baselineBytes;
        Console.WriteLine($"Live managed bytes after eight lifecycles: {liveBytes:N0}");
        return liveBytes;
    }
}
