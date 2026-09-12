using BenchmarkDotNet.Attributes;
using k8s.Models;
using KubeUI.Avalonia.Features.Resources.Visualization;
using KubeUI.Kubernetes.Resources.Relationships;

namespace KubeUI.Benchmarks;

[MemoryDiagnoser]
[BenchmarkCategory("Visualization")]
public class VisualizationPipelineBenchmarks
{
    [Params(250, 1_000)]
    public int ResourceCount { get; set; }

    private ResourceRelationshipGraph _graph = ResourceRelationshipGraph.Empty;
    private IReadOnlySet<string> _selectedNamespaces = new HashSet<string>(StringComparer.Ordinal);
    private IReadOnlySet<string> _selectedTypes = new HashSet<string>(StringComparer.Ordinal);
    private V1Pod _root = new();

    [GlobalSetup]
    public void Setup()
    {
        List<V1Pod> resources = new(ResourceCount);
        for (var index = 0; index < ResourceCount; index++)
        {
            resources.Add(new V1Pod
            {
                ApiVersion = "v1",
                Kind = "Pod",
                Metadata = new V1ObjectMeta
                {
                    NamespaceProperty = index % 2 == 0 ? "namespace-a" : "namespace-b",
                    Name = $"pod-{index}",
                },
            });
        }

        _root = resources[0];
        _selectedNamespaces = new HashSet<string>(["namespace-a"], StringComparer.Ordinal);
        _selectedTypes = new HashSet<string>(["Pod"], StringComparer.Ordinal);
        _graph = new ResourceRelationshipBuilder().Build(resources, new HashSet<string>(StringComparer.Ordinal), hideNoise: false);
    }

    [Benchmark]
    public int ProjectSelectedNamespace()
        => ResourceGraphProjection.ToSelectedNamespaces(_graph, _selectedNamespaces).Resources.Count;

    [Benchmark]
    public int ProjectRoot()
        => ResourceGraphProjection.ToRootResource(_graph, _root).Resources.Count;

    [Benchmark]
    public int ApplyDisplayFilter()
        => ResourceGraphDisplayFilter.Apply(_graph, _selectedTypes, showNotReadyOnly: false).Resources.Count;
}
