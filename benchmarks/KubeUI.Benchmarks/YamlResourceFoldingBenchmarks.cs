using AvaloniaEdit.Document;
using BenchmarkDotNet.Attributes;
using KubeUI.Controls.Editors;
using KubeUI.Features.Resources.Yaml;

namespace KubeUI.Benchmarks;

[MemoryDiagnoser]
[Config(typeof(KubeUIInProcessBenchmarkConfig))]
[BenchmarkCategory("YAML", "Folding", "Resource")]
public class YamlResourceFoldingBenchmarks
{
    private const int OperationsPerInvocation = 16;
    private string _yaml = null!;
    private TextDocument _document = null!;

    [Params(10, 100, 1000)]
    public int YamlLineCount { get; set; }

    /// <summary>Creates and verifies a Pod YAML document for folding measurements.</summary>
    [GlobalSetup]
    public void Setup()
    {
        _yaml = YamlBenchmarkData.BuildPodYaml(YamlLineCount);
        if (_yaml.Count(static character => character == '\n') + 1 != YamlLineCount)
        {
            throw new InvalidOperationException("Kubernetes YAML fixture line count does not match benchmark parameter.");
        }
        _document = YamlBenchmarkUiThread.Instance.Invoke(() => new TextDocument(_yaml));
    }

    /// <summary>Measures repeated folding generation for a Kubernetes Pod.</summary>
    [Benchmark(OperationsPerInvoke = OperationsPerInvocation)]
    public int CreateFoldingsForPod() => YamlBenchmarkUiThread.Instance.Invoke(() =>
    {
        var totalFoldings = 0;
        for (var index = 0; index < OperationsPerInvocation; index++)
        {
            totalFoldings += YamlIndentationFoldingStrategy.CreateNewFoldings(_document, out _).Count();
        }

        return totalFoldings;
    });
}
