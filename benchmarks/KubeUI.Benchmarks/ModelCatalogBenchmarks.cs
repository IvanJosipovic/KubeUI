#nullable enable

using BenchmarkDotNet.Attributes;
using KubernetesClient.Informer.Client;
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

    [GlobalSetup]
    public void Setup()
    {
        _sharedCatalog = new KubernetesModelCatalog();
        _clusterCatalog = new ClusterModelCatalog(_sharedCatalog);
        _podKey = new GroupApiVersionKind(string.Empty, "v1", "Pod", "pods");
        _missingKey = new GroupApiVersionKind("example.com", "v1", "Missing", "missings");
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
}
