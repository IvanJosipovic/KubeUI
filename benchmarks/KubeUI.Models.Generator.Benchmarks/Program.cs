using BenchmarkDotNet.Running;

namespace KubeUI.Models.Generator.Benchmarks;

internal class Program
{
    static void Main(string[] args)
    {
        BenchmarkRunner.Run<Benchmark>();
    }
}
