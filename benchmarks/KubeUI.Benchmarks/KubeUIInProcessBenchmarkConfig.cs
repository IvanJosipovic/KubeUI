using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.InProcess.NoEmit;

namespace KubeUI.Benchmarks;

public sealed class KubeUIInProcessBenchmarkConfig : ManualConfig
{
    public KubeUIInProcessBenchmarkConfig()
    {
        AddJob(Job.Default.WithToolchain(InProcessNoEmitToolchain.Instance));
    }
}
