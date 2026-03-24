using Microsoft.Extensions.DependencyInjection;
using k8s.Models;
using KubeUI.Kubernetes;
using KubernetesCRDModelGen;
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
            new Generator(loggerFactory),
            new TestClusterSettingsStore(),
            new ServiceCollection().BuildServiceProvider());

        var exception = Should.Throw<Exception>(() => cluster.CanI(typeof(V1Pod), Verb.Create, subresource: "portforward"));
        exception.Message.ShouldContain("Missing V1SelfSubjectAccessReview");
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
