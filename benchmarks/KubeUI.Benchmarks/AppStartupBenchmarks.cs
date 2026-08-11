using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Engines;
using KubeUI.Kubernetes;

namespace KubeUI.Benchmarks;

[MemoryDiagnoser]
[SimpleJob(RunStrategy.Throughput, launchCount: 1, warmupCount: 0, iterationCount: 1, invocationCount: 1)]
[BenchmarkCategory("Startup")]
public class AppStartupBenchmarks
{
    [Benchmark]
    public async Task StartAndStopDesktopApp()
    {
        var host = Desktop.Program.CreateHostBuilder([]).Build();

        try
        {
            host.Services.ConfigureKubeUIKubernetesJsonLogging();
            await host.StartAsync();
            Desktop.Program.CreateAppBuilder(host.Services).SetupWithoutStarting();
        }
        finally
        {
            using var shutdownTimeout = new CancellationTokenSource(TimeSpan.FromSeconds(5));
            await host.StopAsync(shutdownTimeout.Token);
            await host.DisposeAsync();
        }
    }
}
