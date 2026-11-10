using BenchmarkDotNet.Attributes;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Features.Resources.Yaml;
using KubeUI.Kubernetes;
using KubeUI.Kubernetes.Serialization;
using Microsoft.Extensions.DependencyInjection;

namespace KubeUI.Benchmarks;

[MemoryDiagnoser]
[Config(typeof(KubeUIInProcessBenchmarkConfig))]
[BenchmarkCategory("YAML", "SerializationPolicy", "Validation")]
public class YamlSerializerPolicyBenchmarks : IDisposable
{
    private ServiceProvider _services = null!;
    private IKubernetesYamlSerializer _serializer = null!;
    private YamlSyntaxValidationService _validationService = null!;
    private V1Pod _resource = null!;
    private string _yaml = null!;

    [Params(100, 1000)]
    public int YamlLineCount { get; set; }

    [Params(true, false)]
    public bool JsonAllowReflection { get; set; }

    /// <summary>Creates a Pod fixture and serializer using the selected JSON metadata policy.</summary>
    [GlobalSetup]
    public void Setup()
    {
        _services = new ServiceCollection()
            .AddLogging()
            .AddKubeUIKubernetesServices(jsonAllowReflection: JsonAllowReflection)
            .BuildServiceProvider();
        _serializer = _services.GetRequiredService<IKubernetesYamlSerializer>();
        var modelCatalog = _services.GetRequiredService<KubernetesModelCatalog>();
        modelCatalog.Register(GroupApiVersionKind.From<V1Pod>(), typeof(V1Pod));
        _validationService = new YamlSyntaxValidationService(modelCatalog, _serializer);
        _yaml = YamlBenchmarkData.BuildPodYaml(YamlLineCount);
        _resource = _serializer.Deserialize<V1Pod>(_yaml);

        if (_yaml.Count(static character => character == '\n') + 1 != YamlLineCount
            || _validationService.Validate(_yaml).Count != 0)
        {
            throw new InvalidOperationException("YAML serializer policy fixture failed validation or line-count checks.");
        }
    }

    [GlobalCleanup]
    public void Cleanup() => Dispose();

    public void Dispose()
    {
        _services?.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>Serializes the Pod fixture with the selected JSON metadata policy.</summary>
    [Benchmark]
    public string SerializeResource() => _serializer.Serialize(_resource);

    /// <summary>Deserializes the Pod fixture with the selected JSON metadata policy.</summary>
    [Benchmark]
    public V1Pod DeserializeResource() => _serializer.Deserialize<V1Pod>(_yaml);

    /// <summary>Validates the Pod YAML fixture.</summary>
    [Benchmark]
    public int ValidateValidYaml() => _validationService.Validate(_yaml).Count;
}
