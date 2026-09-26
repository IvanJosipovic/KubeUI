using BenchmarkDotNet.Running;

namespace KubeUI.Benchmarks;

internal class Program
{
    static void Main(string[] args)
    {
        if (args.Length > 0 && args[0] == "--grid-baseline")
        {
            BenchmarkSwitcher.FromTypes(
            [
                typeof(IdentityPreservingSelectionModelBenchmarks),
                typeof(ResourceListPipelineBenchmarks),
                typeof(ResourceListViewModelBenchmarks)
            ]).Run(args[1..]);
            return;
        }

        BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly).Run(args);
    }
}
