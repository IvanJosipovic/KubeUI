using KubeUI.Kubernetes;
using KubeUI.Testing.Kubernetes.Bootstrap;

namespace KubeUI.Avalonia.Tests.Infra;

/// <summary>
/// Named generated fake clusters for tests that only need cluster names, created through
/// the production runtime so tests never construct <see cref="IClusterRuntime"/> doubles.
/// Clusters are not connected. Disposal disposes every generated cluster.
/// </summary>
internal sealed record NamedClusters(IReadOnlyList<IClusterRuntime> Runtimes, TestClusterGenerator Generator) : IAsyncDisposable
{
    public async ValueTask DisposeAsync() => await Generator.ResetAsync();
}

internal static class NamedClusterCatalogFactory
{
    public static async Task<NamedClusters> CreateAsync(params string[] names)
    {
        var generator = new TestClusterGenerator();
        try
        {
            var runtimes = new List<IClusterRuntime>(names.Length);
            foreach (var name in names)
            {
                var config = TestClusterConfig.Fake();
                config.Name = name;
                var cluster = await generator.CreateAsync(config, TestContext.Current.CancellationToken);
                runtimes.Add(cluster.Cluster);
            }

            return new NamedClusters(runtimes, generator);
        }
        catch
        {
            await generator.ResetAsync();
            throw;
        }
    }
}
