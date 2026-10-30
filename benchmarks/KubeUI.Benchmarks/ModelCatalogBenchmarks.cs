#nullable enable

using BenchmarkDotNet.Attributes;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Features.Resources.Yaml;
using KubeUI.Kubernetes;
using KubeUI.Kubernetes.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace KubeUI.Benchmarks;

[MemoryDiagnoser]
[ShortRunJob]
public class ModelCatalogBenchmarks
{
    private ServiceProvider _services = null!;
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
        _services = new ServiceCollection().AddKubeUIKubernetesServices(jsonAllowReflection: false, yamlAllowReflection: false).BuildServiceProvider();
        _sharedCatalog = new KubernetesModelCatalog();
        _clusterCatalog = new ClusterModelCatalog(_sharedCatalog);
        _podKey = new GroupApiVersionKind(string.Empty, "v1", "Pod", "pods");
        _missingKey = new GroupApiVersionKind("example.com", "v1", "Missing", "missings");
        _yamlValidationService = new YamlSyntaxValidationService(_sharedCatalog, _services.GetRequiredService<IKubernetesYamlSerializer>());
    }

    [GlobalCleanup]
    public void Cleanup() => _services.Dispose();

    [Benchmark]
    public void BuiltInTypeLookup()
    {
        _sharedCatalog.TryGetResourceType(_podKey, out var resourceType);
        GC.KeepAlive(resourceType);
    }

    [Benchmark]
    public void ClusterTypeLookupHit()
    {
        _clusterCatalog.TryGetResourceType(_podKey, out var resourceType);
        GC.KeepAlive(resourceType);
    }

    [Benchmark]
    public void ClusterTypeLookupMiss()
    {
        _clusterCatalog.TryGetResourceType(_missingKey, out var resourceType);
        GC.KeepAlive(resourceType);
    }

    [Benchmark]
    public void BuiltInTypeMapSnapshot()
    {
        GC.KeepAlive(_sharedCatalog.GetYamlTypeMap());
    }

    [Benchmark]
    public void LazyYamlTypeMapLookup()
    {
        GC.KeepAlive(_sharedCatalog.GetYamlTypeMap());
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
