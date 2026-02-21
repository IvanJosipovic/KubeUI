using BenchmarkDotNet.Running;

namespace KubeUI.Benchmarks;

internal class Program
{
    static void Main(string[] args)
    {
        var _ = BenchmarkRunner.Run(typeof(Program).Assembly);
    }
}
