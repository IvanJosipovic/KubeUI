using BenchmarkDotNet.Attributes;
using Microsoft.VSDiagnostics;

namespace KubeUI.Benchmarks;

[CPUUsageDiagnoser]
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
