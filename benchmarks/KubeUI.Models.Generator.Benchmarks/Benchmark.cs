using BenchmarkDotNet.Attributes;
using k8s;
using k8s.Models;

namespace KubeUI.Models.Generator.Benchmarks;

[MemoryDiagnoser]
public class Benchmark
{
    IGenerator generator;

    V1CustomResourceDefinition crd;

    [GlobalSetup]
    public void GlobalSetup()
    {
        generator = new Generator();

        crd = KubernetesYaml.LoadFromFileAsync<V1CustomResourceDefinition>("managedclusters.containerservice.azure.com.yaml").Result;
    }

    [Benchmark]
    public void Test1()
    {
        var ass = generator.GenerateAssembly(crd);
    }
}
