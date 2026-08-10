#nullable enable

using BenchmarkDotNet.Attributes;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Features.Resources.Yaml;
using KubeUI.Kubernetes;

namespace KubeUI.Benchmarks;

[MemoryDiagnoser]
[ShortRunJob]
public class ModelCatalogBenchmarks
{
    private KubernetesModelCatalog _sharedCatalog = null!;
    private ClusterModelCatalog _clusterCatalog = null!;
    private GroupApiVersionKind _podKey;
    private GroupApiVersionKind _missingKey;
    private YamlSyntaxValidationService _yamlValidationService = null!;
    private const string ValidPodYaml = """
        apiVersion: v1
        kind: Pod
        metadata:
          name: benchmark-pod
        """;

    [GlobalSetup]
    public void Setup()
    {
        _sharedCatalog = new KubernetesModelCatalog();
        _clusterCatalog = new ClusterModelCatalog(_sharedCatalog);
        _podKey = new GroupApiVersionKind(string.Empty, "v1", "Pod", "pods");
        _missingKey = new GroupApiVersionKind("example.com", "v1", "Missing", "missings");
        _yamlValidationService = new YamlSyntaxValidationService();
    }

    [Benchmark]
    public void BuiltInTypeLookup()
    {
        GC.KeepAlive(_sharedCatalog.GetResourceType(_podKey));
    }

    [Benchmark]
    public void ClusterTypeLookupHit()
    {
        GC.KeepAlive(_clusterCatalog.GetResourceType(_podKey));
    }

    [Benchmark]
    public void ClusterTypeLookupMiss()
    {
        GC.KeepAlive(_clusterCatalog.GetResourceType(_missingKey));
    }

    [Benchmark]
    public void BuiltInDocumentationLookup()
    {
        GC.KeepAlive(_sharedCatalog.GetDocumentation(typeof(k8s.Models.V1Pod)));
    }

    [Benchmark]
    public void LazyYamlTypeMapLookup()
    {
        GC.KeepAlive(_clusterCatalog.GetYamlTypeMap());
    }

    /// <summary>
    /// Repeated validation exercises the cached cluster YAML map; the map should not be rebuilt per validation.
    /// </summary>
    [Benchmark]
    public int RepeatedYamlValidation()
    {
        var diagnostics = 0;
        for (var i = 0; i < 100; i++)
        {
            diagnostics += _yamlValidationService.Validate(ValidPodYaml, _clusterCatalog).Count;
        }

        return diagnostics;
    }
}
