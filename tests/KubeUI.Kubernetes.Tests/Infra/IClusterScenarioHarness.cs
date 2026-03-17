using k8s;
using k8s.Models;
using KubeUI.Client;

namespace KubeUI.Kubernetes.Tests.Infra;

public interface IClusterScenarioHarness : IAsyncDisposable
{
    IClusterRuntime Cluster { get; }

    bool SupportsLimitedAccessScenarios { get; }

    Task InitializeAsync();

    Task<T> CreateDirectAsync<T>(T item) where T : class, IKubernetesObject<V1ObjectMeta>, new();

    Task CreateCustomResourceDefinitionAsync(V1CustomResourceDefinition crd);

    Task<IClusterRuntime> CreateLimitedAccessClusterAsync(bool includeNamespaceFallback);
}
