using System.Linq;
using AvaloniaEdit.Document;
using BenchmarkDotNet.Attributes;
using KubeUI.Avalonia.Features.Resources.Yaml;
using KubeUI.Avalonia.Features.Resources.Yaml.Views;

namespace KubeUI.Benchmarks;

[MemoryDiagnoser]
public class YamlEditorBenchmarks
{
    private TextDocument _simpleDocument = null!;
    private TextDocument _mixedDocument = null!;

    [Params(50, 250, 1000)]
    public int ResourceCount { get; set; }

    [GlobalSetup]
    public void Setup()
    {
        _simpleDocument = new TextDocument(BuildSimpleYaml(ResourceCount));
        _mixedDocument = new TextDocument(BuildMixedYaml(ResourceCount));
    }

    [Benchmark(Baseline = true)]
    public int CreateFoldings_SimpleYaml()
    {
        return YamlFoldingStrategy.CreateNewFoldings(_simpleDocument, out _).Count();
    }

    [Benchmark]
    public int CreateFoldings_MixedYaml()
    {
        return YamlFoldingStrategy.CreateNewFoldings(_mixedDocument, out _).Count();
    }

    private static string BuildSimpleYaml(int resourceCount)
    {
                return string.Concat(Enumerable.Range(0, resourceCount).Select(static index => $$"""
                        apiVersion: v1
                        kind: ConfigMap
                        metadata:
                            name: resource-{{index}}
                            namespace: default
                        data:
                            appsettings.json: |-
                                {
                                    "enabled": true,
                                    "index": {{index}}
                                }
                        ---
                        """));
    }

    private static string BuildMixedYaml(int resourceCount)
    {
                return string.Concat(Enumerable.Range(0, resourceCount).Select(static index => $$"""
                        apiVersion: apps/v1
                        kind: Deployment
                        metadata:
                            name: app-{{index}}
                            namespace: default
                            labels:
                                app.kubernetes.io/name: kubeui
                                app.kubernetes.io/instance: app-{{index}}
                        spec:
                            replicas: 2
                            selector:
                                matchLabels:
                                    app: app-{{index}}
                            template:
                                metadata:
                                    annotations:
                                        checksum/config: abc123
                                spec:
                                    containers:
                                    - name: app
                                        image: nginx:latest
                                        env:
                                        - name: FEATURE_FLAG
                                            value: "true"
                                        - name: RESOURCE_INDEX
                                            value: "{{index}}"
                        # trailing comment to exercise comment skipping
                        ---
                        """));
    }
}
