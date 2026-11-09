using AvaloniaEdit.Document;
using BenchmarkDotNet.Attributes;
using KubeUI.Controls.Editors;

namespace KubeUI.Benchmarks;

[MemoryDiagnoser]
[Config(typeof(KubeUIInProcessBenchmarkConfig))]
[BenchmarkCategory("YAML", "Folding")]
public class YamlEditorBenchmarks
{
    private const int OperationsPerInvocation = 16;
    private string _yaml = null!;
    private TextDocument _document = null!;

    [Params(10, 100, 1000)]
    public int YamlLineCount { get; set; }

    /// <summary>Builds the nested YAML document on Avalonia's UI thread.</summary>
    [GlobalSetup]
    public void Setup()
    {
        _yaml = YamlBenchmarkData.BuildNestedYaml(YamlLineCount);
        YamlBenchmarkUiThread.Instance.Invoke(() =>
        {
            _document = new TextDocument(_yaml);
            return 0;
        });
    }

    /// <summary>Measures repeated folding generation for the nested YAML fixture.</summary>
    [Benchmark(OperationsPerInvoke = OperationsPerInvocation)]
    public int CreateFoldings_DeepYaml()
    {
        return YamlBenchmarkUiThread.Instance.Invoke(() =>
        {
            var totalFoldings = 0;
            for (var index = 0; index < OperationsPerInvocation; index++)
            {
                totalFoldings += YamlIndentationFoldingStrategy.CreateNewFoldings(_document, out _).Count();
            }

            return totalFoldings;
        });
    }
}
