using Avalonia.Controls;
using BenchmarkDotNet.Attributes;
using BenchmarkDotNet.Configs;
using BenchmarkDotNet.Jobs;
using BenchmarkDotNet.Toolchains.InProcess.NoEmit;
using KubeUI.Avalonia.Infrastructure.Presentation;
using KubeUI.Avalonia.Shell.Main;
using KubeUI.Avalonia.Tests.Infra;
using Microsoft.Extensions.DependencyInjection;

namespace KubeUI.Benchmarks;

[MemoryDiagnoser]
[Config(typeof(InProcessViewLocatorBenchmarkConfig))]
[BenchmarkCategory("ViewLocator")]
public class ViewLocatorBenchmarks : IDisposable
{
    private TestApp _app = null!;
    private ViewLocator _viewLocator = null!;
    private IViewFactory[] _viewFactories = null!;
    private HomeViewModel _viewModel = null!;

    [GlobalSetup]
    public void Setup()
    {
        _app = new TestApp();
        var services = _app.Services;
        _viewLocator = services.GetRequiredService<ViewLocator>();
        _viewFactories = services.GetRequiredService<IViewFactory[]>();
        _viewModel = new HomeViewModel();
    }

    [GlobalCleanup]
    public void Cleanup() => Dispose();

    public void Dispose() => _app?.Dispose();

    [Benchmark]
    public int ScanGeneratedFactories()
    {
        for (var index = 0; index < _viewFactories.Length; index++)
        {
            if (_viewFactories[index].CanCreate(_viewModel))
            {
                return index;
            }
        }

        return -1;
    }

    [Benchmark]
    public Control BuildHomeView() => _viewLocator.Build(_viewModel);
}

public sealed class InProcessViewLocatorBenchmarkConfig : ManualConfig
{
    public InProcessViewLocatorBenchmarkConfig()
    {
        AddJob(Job.MediumRun.WithToolchain(InProcessNoEmitToolchain.Default));
    }
}
