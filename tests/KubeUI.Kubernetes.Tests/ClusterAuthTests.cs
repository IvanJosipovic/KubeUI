using k8s.Models;
using KubernetesCRDModelGen;
using KubeUI.Kubernetes;
using KubeUI.Testing;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging.Abstractions;
using Shouldly;

namespace KubeUI.Kubernetes.Tests;

public sealed class ClusterAuthTests
{
    [Fact]
    public void cani_throws_when_permission_review_has_not_been_cached_yet()
    {
        using var loggerFactory = NullLoggerFactory.Instance;
        var cluster = new Cluster(
            NullLogger<Cluster>.Instance,
            loggerFactory,
            new ModelCache(),
            new Generator(),
            new TestClusterSettingsStore(),
            new ServiceCollection().BuildServiceProvider());

        var exception = Should.Throw<Exception>(() => cluster.CanI(typeof(V1Pod), Verb.Create, subresource: "portforward"));
        exception.Message.ShouldContain("Missing V1SelfSubjectAccessReview");
    }

    [Fact]
    public async Task cani_any_namespace_uses_namespace_scoped_permission_when_cluster_scope_is_denied()
    {
        var cluster = new TestClusterRuntime();

        cluster.SetPermission<V1Pod>(Verb.Create, false, subresource: "portforward");
        cluster.SetPermission<V1Pod>(Verb.Create, true, "my-app", "portforward");

        await cluster.AddOrUpdateResource(new V1Namespace
        {
            Metadata = new V1ObjectMeta { Name = "my-app" }
        });

        cluster.CanIAnyNamespace<V1Pod>(Verb.Create, "portforward").ShouldBeTrue();
    }

    private sealed class TestClusterSettingsStore : IClusterSettingsStore
    {
        public IReadOnlyCollection<string> KubeConfigPaths => [];

        public void AddKubeConfigPath(string path)
        {
        }

        public IReadOnlyCollection<string> GetClusterNamespaces(IClusterRuntime cluster)
        {
            return [];
        }
    }
}
