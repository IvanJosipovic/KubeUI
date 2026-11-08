using Avalonia;
using Avalonia.Controls;
using Avalonia.Headless;
using Avalonia.Threading;
using AvaloniaEdit;
using AvaloniaEdit.Folding;
using BenchmarkDotNet.Attributes;
using KubeUI.Avalonia.Tests.Infra;
using KubeUI.Features.Resources.Yaml;
using Microsoft.Extensions.DependencyInjection;

namespace KubeUI.Benchmarks;

[MemoryDiagnoser]
[Config(typeof(KubeUIInProcessBenchmarkConfig))]
[BenchmarkCategory("YAML", "Rendering", "Headless")]
public class YamlViewerRenderingBenchmarks
{
    private YamlBenchmarkUiThread _uiThread = null!;
    private ResourceYamlViewModel _viewModel = null!;
    private ResourceYamlView _view = null!;
    private Window _window = null!;

    [Params(10, 100, 1000)]
    public int YamlLineCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _uiThread = YamlBenchmarkUiThread.Instance;
        _uiThread.Invoke(() =>
        {
            _viewModel = _uiThread.App.Services.GetRequiredService<ResourceYamlViewModel>();
            _viewModel.InitializeReadOnly(
                "YAML rendering benchmark",
                "yaml-rendering-benchmark",
                YamlBenchmarkData.BuildNestedYaml(YamlLineCount));
            _view = new ResourceYamlView { DataContext = _viewModel };
            _window = new Window
            {
                Content = _view,
                Width = 1280,
                Height = 900,
            };
            _window.Show();
            return RenderFrameAndInspect();
        });
    }

    [GlobalCleanup]
    public void Cleanup()
    {
        if (_uiThread is null || _window is null)
        {
            return;
        }

        _uiThread.Invoke(() =>
        {
            _window.Close();
            _viewModel.Dispose();
            return 0;
        });
    }

    [Benchmark]
    public int RenderYamlView() => _uiThread.Invoke(RenderFrameAndInspect);

    private int RenderFrameAndInspect()
    {
        _window.Measure(new Size(_window.Width, _window.Height));
        _window.Arrange(new Rect(0, 0, _window.Width, _window.Height));
        Dispatcher.UIThread.RunJobs();

        using var frame = _window.CaptureRenderedFrame()
            ?? throw new InvalidOperationException("Headless YAML view rendered no frame.");

        var editor = _view.FindControl<TextEditor>("Editor")
            ?? throw new InvalidOperationException("Resource YAML editor was not created.");
        var margin = editor.TextArea.LeftMargins.OfType<FoldingMargin>().Single();
        var foldingCount = margin.FoldingManager.AllFoldings.Count();
        var visualLineCount = editor.TextArea.TextView.VisualLines.Count;
        if (foldingCount == 0 || visualLineCount == 0)
        {
            throw new InvalidOperationException(
                $"Expected rendered YAML foldings and lines; got {foldingCount} foldings and {visualLineCount} visual lines.");
        }

        return visualLineCount;
    }
}
