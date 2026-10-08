using BenchmarkDotNet.Attributes;

namespace KubeUI.Benchmarks;

[MemoryDiagnoser]
public class SampleBenchmarks
{
    [GlobalSetup]
    public void Setup()
    {
    }

    [Benchmark]
    public bool FindReviewIndexed_Benchmark()
    {
        return true;
    }
}
