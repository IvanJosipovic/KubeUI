using k8s;
using k8s.Models;

namespace KubeUI.Testing.Kubernetes.Scenarios;

public interface IClusterScenarioHarness : IAsyncDisposable
{
    IClusterRuntime Cluster { get; }

    Task InitializeAsync(CancellationToken cancellationToken = default);

    Task<T> CreateDirectAsync<T>(T item, CancellationToken cancellationToken = default) where T : class, IKubernetesObject<V1ObjectMeta>, new();

    Task<T> ReplaceDirectAsync<T>(T item, CancellationToken cancellationToken = default) where T : class, IKubernetesObject<V1ObjectMeta>, new();

    Task DeleteDirectAsync<T>(T item, CancellationToken cancellationToken = default) where T : class, IKubernetesObject<V1ObjectMeta>, new();

    Task<IClusterRuntime> CreateLimitedAccessClusterAsync(LimitedAccessScenario scenario, CancellationToken cancellationToken = default);
}

public sealed record LimitedAccessScenario(string Yaml, IReadOnlyCollection<string>? FallbackNamespaces = null);

