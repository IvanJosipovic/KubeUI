using k8s.Models;

namespace KubeUI.Documentation.Tests.Infra;

public sealed class WalkthroughDemoResourcesTests
{
    [Fact]
    public void CreateDemoResources_builds_ten_deployment_replica_chains_with_varied_pod_counts()
    {
        var resources = KubeUIWalkthroughRecorder.CreateDemoResources().Resources;
        var deployments = resources.OfType<V1Deployment>().ToArray();
        var replicaSets = resources.OfType<V1ReplicaSet>().ToArray();
        var pods = resources.OfType<V1Pod>().ToArray();
        List<int> replicaCounts = new(deployments.Length);

        Assert.Equal(10, deployments.Length);
        Assert.Equal(deployments.Length, replicaSets.Length);
        Assert.Equal(10, resources.OfType<V1Service>().Count());
        Assert.Equal(10, resources.OfType<V1Ingress>().Count());

        foreach (var deployment in deployments)
        {
            var deploymentUid = deployment.Metadata?.Uid;
            Assert.False(string.IsNullOrWhiteSpace(deploymentUid));

            var replicaSet = Assert.Single(replicaSets.Where(candidate =>
                candidate.Metadata?.OwnerReferences?.Any(owner =>
                    owner.Kind == V1Deployment.KubeKind && owner.Uid == deploymentUid) == true));
            var ownedPods = pods.Where(pod =>
                pod.Metadata?.OwnerReferences?.Any(owner =>
                    owner.Kind == V1ReplicaSet.KubeKind
                    && owner.Uid == replicaSet.Metadata?.Uid
                    && owner.Name == replicaSet.Metadata?.Name) == true).ToArray();

            Assert.InRange(ownedPods.Length, 2, 5);
            Assert.Equal(deployment.Spec?.Replicas, replicaSet.Spec?.Replicas);
            Assert.Equal(replicaSet.Spec?.Replicas, ownedPods.Length);
            Assert.All(ownedPods, pod =>
                Assert.Equal(replicaSet.Metadata?.Name, pod.Metadata?.OwnerReferences?.Single().Name));
            replicaCounts.Add(ownedPods.Length);
        }

        Assert.True(replicaCounts.Distinct().Count() > 1);
        Assert.Equal(
            deployments.Select(deployment => deployment.Spec?.Replicas),
            KubeUIWalkthroughRecorder.CreateDemoResources().Resources
                .OfType<V1Deployment>()
                .Select(deployment => deployment.Spec?.Replicas));
    }
}
