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
[BenchmarkCategory("YAML", "Serialization", "Validation")]
public class YamlCodecBenchmarks : IDisposable
{
    private ServiceProvider _services = null!;
    private IKubernetesYamlSerializer _serializer = null!;
    private YamlSyntaxValidationService _validationService = null!;
    private object _resource = null!;
    private string _yaml = null!;
    private string _invalidYaml = null!;
    private bool _useConfigMap;

    [Params(10, 100, 1000)]
    public int YamlLineCount { get; set; }

    /// <summary>Creates serializer fixtures and verifies valid and invalid YAML inputs.</summary>
    [GlobalSetup]
    public void Setup()
    {
        _services = new ServiceCollection()
            .AddLogging()
            .AddKubeUIKubernetesServices(jsonAllowReflection: false)
            .BuildServiceProvider();
        _serializer = _services.GetRequiredService<IKubernetesYamlSerializer>();
        var modelCatalog = _services.GetRequiredService<KubernetesModelCatalog>();
        modelCatalog.Register(GroupApiVersionKind.From<V1Pod>(), typeof(V1Pod));
        modelCatalog.Register(GroupApiVersionKind.From<V1ConfigMap>(), typeof(V1ConfigMap));
        _validationService = new YamlSyntaxValidationService(modelCatalog, _serializer);
        _useConfigMap = YamlLineCount == 10;
        _resource = _useConfigMap ? CreateConfigMapFixture() : CreatePodFixture();
        _invalidYaml = BuildInvalidYaml(_yaml);

        if (_validationService.Validate(_yaml).Count != 0 || _validationService.Validate(_invalidYaml).Count == 0)
        {
            throw new InvalidOperationException("YAML benchmark fixtures did not produce expected validation results.");
        }
    }

    private static int CountLines(string yaml) => yaml.Count(static character => character == '\n') + 1;

    [GlobalCleanup]
    public void Cleanup() => Dispose();

    public void Dispose()
    {
        _services?.Dispose();
        GC.SuppressFinalize(this);
    }

    /// <summary>Serializes the configured resource fixture.</summary>
    [Benchmark]
    public string SerializeResource() => _serializer.Serialize(_resource);

    /// <summary>Deserializes the configured resource fixture.</summary>
    [Benchmark]
    public object DeserializeResource() => _useConfigMap
        ? _serializer.Deserialize<V1ConfigMap>(_yaml)
        : _serializer.Deserialize<V1Pod>(_yaml);

    /// <summary>Validates the valid YAML fixture.</summary>
    [Benchmark]
    public int ValidateValidYaml() => _validationService.Validate(_yaml).Count;

    /// <summary>Validates the malformed YAML fixture.</summary>
    [Benchmark]
    public int ValidateInvalidYaml() => _validationService.Validate(_invalidYaml).Count;

    /// <summary>Creates and sizes a ConfigMap fixture to the requested YAML line count.</summary>
    private object CreateConfigMapFixture()
    {
        var configMap = new V1ConfigMap
        {
            ApiVersion = V1ConfigMap.KubeApiVersion,
            Kind = V1ConfigMap.KubeKind,
            Metadata = new V1ObjectMeta { Name = "yaml-benchmark", NamespaceProperty = "default" },
            Data = new Dictionary<string, string>
            {
                ["benchmark-revision"] = "1",
                ["config"] = "nested-map-value",
            },
        };

        var fillerIndex = 0;
        _yaml = _serializer.Serialize(configMap);
        while (CountLines(_yaml) < YamlLineCount)
        {
            configMap.Data[$"value-{fillerIndex.ToString(System.Globalization.CultureInfo.InvariantCulture)}"] = "nested-map-value";
            fillerIndex++;
            _yaml = _serializer.Serialize(configMap);
        }

        EnsureExactLineCount(_yaml);
        return configMap;
    }

    /// <summary>Creates and sizes a Pod fixture to the requested YAML line count.</summary>
    private object CreatePodFixture()
    {
        var pod = _serializer.Deserialize<V1Pod>(YamlBenchmarkData.BuildPodYaml(10));
        var serializedYaml = _serializer.Serialize(pod);
        var containerIndex = pod.Spec!.Containers!.Count;
        while (true)
        {
            pod.Spec.Containers.Add(CreateContainer(containerIndex));
            var candidateYaml = _serializer.Serialize(pod);
            if (CountLines(candidateYaml) > YamlLineCount)
            {
                pod.Spec.Containers.RemoveAt(pod.Spec.Containers.Count - 1);
                break;
            }

            serializedYaml = candidateYaml;
            containerIndex++;
        }

        var annotations = pod.Metadata!.Annotations!;
        var annotationIndex = 0;
        while (CountLines(serializedYaml) < YamlLineCount)
        {
            annotations[$"benchmark.kubeui.com/value-{annotationIndex.ToString(System.Globalization.CultureInfo.InvariantCulture)}"] = "nested-map-value";
            annotationIndex++;
            serializedYaml = _serializer.Serialize(pod);
        }

        EnsureExactLineCount(serializedYaml);
        _yaml = serializedYaml;
        return pod;
    }

    private static V1Container CreateContainer(int index)
    {
        return new V1Container
        {
            Name = $"app-{index.ToString(System.Globalization.CultureInfo.InvariantCulture)}",
            Image = "nginx:latest",
            Env =
            [
                new V1EnvVar { Name = "FEATURE_FLAG", Value = "true" },
                new V1EnvVar
                {
                    Name = "RESOURCE_INDEX",
                    Value = index.ToString(System.Globalization.CultureInfo.InvariantCulture),
                },
            ],
            Resources = new V1ResourceRequirements
            {
                Limits = new Dictionary<string, ResourceQuantity>
                {
                    ["cpu"] = new ResourceQuantity("100m"),
                    ["memory"] = new ResourceQuantity("64Mi"),
                },
            },
        };
    }

    /// <summary>Throws when serialized fixture line count differs from the benchmark parameter.</summary>
    private void EnsureExactLineCount(string yaml)
    {
        var actualLineCount = CountLines(yaml);
        if (actualLineCount != YamlLineCount)
        {
            throw new InvalidOperationException(
                $"Expected {YamlLineCount} YAML lines; serializer produced {actualLineCount}.");
        }
    }

    private static string BuildInvalidYaml(string validYaml)
    {
        var lines = validYaml.Split('\n');
        lines[^1] = "invalid: [";
        return string.Join('\n', lines);
    }
}
