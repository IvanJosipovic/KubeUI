using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Running;

namespace KubeUI.Benchmarks;

internal class Program
{
    static void Main(string[] args)
    {
        var switcher = BenchmarkSwitcher.FromAssembly(typeof(Program).Assembly);
        if (args.Contains("--inProcess", StringComparer.OrdinalIgnoreCase)
            || args.Contains("-i", StringComparer.Ordinal))
        {
            switcher.Run(args, new DebugInProcessConfig());
            return;
        }

        switcher.Run(args);
    }
}
