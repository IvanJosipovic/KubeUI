using BenchmarkDotNet.Running;

namespace KubeUI.Benchmarks;

internal class Program
{
    static void Main(string[] args)
    {
        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }
}
