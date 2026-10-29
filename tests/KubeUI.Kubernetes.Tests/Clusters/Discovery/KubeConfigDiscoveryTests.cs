using k8s.KubeConfigModels;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;

namespace KubeUI.Kubernetes.Tests.Clusters.Discovery;

public class KubeConfigDiscoveryTests
{
    private const string ValidKubeConfig = """
        apiVersion: v1
        kind: Config
        clusters:
        - name: dev-cluster
          cluster:
            server: https://dev.example.com
        users:
        - name: dev-user
          user: {}
        contexts:
        - name: dev
          context:
            cluster: dev-cluster
            user: dev-user
        """;

    [Fact]
    public void DiscoverFiles_returns_only_top_level_kubeconfigs_with_contexts()
    {
        using var directory = new TemporaryDirectory();
        var config = Write(directory.Path, "config", ValidKubeConfig);
        var extra = Write(directory.Path, "b-extra.yaml", ValidKubeConfig);
        Write(directory.Path, ".hidden", ValidKubeConfig);
        Write(directory.Path, "empty", string.Empty);
        Write(directory.Path, "garbage.yaml", "::: not yaml [");
        Write(directory.Path, "pod.yaml", "apiVersion: v1\nkind: Pod\nmetadata:\n  name: x\n");
        Write(directory.Path, "no-contexts", "apiVersion: v1\nkind: Config\nclusters: []\n");
        var cache = Directory.CreateDirectory(Path.Combine(directory.Path, "cache"));
        Write(cache.FullName, "nested", ValidKubeConfig);

        var discovery = new KubeConfigDiscovery(NullLogger<KubeConfigDiscovery>.Instance);

        discovery.DiscoverFiles(directory.Path).ShouldBe([extra, config]);
    }

    [Fact]
    public void DiscoverFiles_returns_empty_for_missing_directory()
    {
        var discovery = new KubeConfigDiscovery(NullLogger<KubeConfigDiscovery>.Instance);

        discovery.DiscoverFiles(Path.Combine(Path.GetTempPath(), $"kubeui-missing-{Guid.NewGuid():N}")).ShouldBeEmpty();
    }

    [Fact]
    public void IsKubeConfig_accepts_a_manually_selected_hidden_file()
    {
        using var directory = new TemporaryDirectory();
        var path = Write(directory.Path, ".kubeconfig", ValidKubeConfig);
        var discovery = new KubeConfigDiscovery(NullLogger<KubeConfigDiscovery>.Instance);

        discovery.IsKubeConfig(path).ShouldBeTrue();
        discovery.DiscoverFiles(directory.Path).ShouldBeEmpty();
    }

    [Fact]
    public void IsKubeConfig_returns_false_for_missing_file()
    {
        var discovery = new KubeConfigDiscovery(NullLogger<KubeConfigDiscovery>.Instance);

        discovery.IsKubeConfig(Path.Combine(Path.GetTempPath(), $"kubeui-missing-{Guid.NewGuid():N}")).ShouldBeFalse();
    }

    private static string Write(string directory, string name, string content)
    {
        var path = Path.Combine(directory, name);
        File.WriteAllText(path, content);
        return path;
    }
}
