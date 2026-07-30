using k8s;
using k8s.Models;

namespace KubeUI.Testing;

public interface IClusterScenarioHarness : IAsyncDisposable
{
    IClusterRuntime Cluster { get; }

    bool SupportsLimitedAccessScenarios { get; }

    Task InitializeAsync(CancellationToken cancellationToken = default);

    Task<T> CreateDirectAsync<T>(T item, CancellationToken cancellationToken = default) where T : class, IKubernetesObject<V1ObjectMeta>, new();

    Task<T> ReplaceDirectAsync<T>(T item, CancellationToken cancellationToken = default) where T : class, IKubernetesObject<V1ObjectMeta>, new();

    Task DeleteDirectAsync<T>(T item, CancellationToken cancellationToken = default) where T : class, IKubernetesObject<V1ObjectMeta>, new();

    Task CreateCustomResourceDefinitionAsync(V1CustomResourceDefinition crd, CancellationToken cancellationToken = default);

    Task<IClusterRuntime> CreateLimitedAccessClusterAsync(bool includeNamespaceFallback, CancellationToken cancellationToken = default);
}

