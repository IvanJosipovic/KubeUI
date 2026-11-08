using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.InProcess.NoEmit;

namespace KubeUI.Benchmarks;

public sealed class YamlViewerResourceRefreshBenchmarkConfig : ManualConfig
{
    public YamlViewerResourceRefreshBenchmarkConfig()
    {
        AddJob(Job.Default
            .WithToolchain(InProcessNoEmitToolchain.Instance)
            .WithWarmupCount(6)
            .WithIterationCount(15));
    }
}
