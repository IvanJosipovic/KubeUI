using DynamicData;
using k8s.Models;
using KubernetesClient.Informer.Client;
using Shouldly;

namespace KubeUI.Kubernetes.Tests.PodLogs;

public sealed class PodLogSessionResolverTests
{
    [Fact]
    public async Task Deployment_state_resolves_pods_through_replica_set()
    {
        await using var harness = await new TestClusterGenerator().CreateAsync(
            new TestClusterConfig { Type = KubernetesBackend.Fake },
            TestContext.Current.CancellationToken);

        var deployment = new V1Deployment
        {
            Metadata = Metadata("api", "deployment-uid"),
        };
        var replicaSet = new V1ReplicaSet
        {
            Metadata = Metadata(
                "api-7c9dd9f4f4",
                "replicaset-uid",
                new V1OwnerReference
                {
                    Kind = V1Deployment.KubeKind,
                    Name = "api",
                    Uid = "deployment-uid",
                    Controller = true,
                }),
        };
        var pod = CreatePod(
            "api-7c9dd9f4f4-abcde",
            "pod-uid",
            new V1OwnerReference
            {
                Kind = V1ReplicaSet.KubeKind,
                Name = "api-7c9dd9f4f4",
                Uid = "replicaset-uid",
                Controller = true,
            });
        AddResources(harness.Cluster, deployment, replicaSet, pod);

        var resolver = new PodLogSessionResolver();
        var state = resolver.CreateState(deployment, string.Empty, false, false);

        var resolution = resolver.TryResolve(harness.Cluster, state);

        resolution.ShouldNotBeNull();
        resolution!.Pod.Name().ShouldBe("api-7c9dd9f4f4-abcde");
        resolution.RelatedPods.Select(x => x.Name()).ShouldBe(["api-7c9dd9f4f4-abcde"]);
    }

    [Fact]
    public async Task Cron_job_state_resolves_pods_through_job()
    {
        await using var harness = await new TestClusterGenerator().CreateAsync(
            new TestClusterConfig { Type = KubernetesBackend.Fake },
            TestContext.Current.CancellationToken);

        var cronJob = new V1CronJob
        {
            Metadata = Metadata("backup", "cronjob-uid"),
        };
        var job = new V1Job
        {
            Metadata = Metadata(
                "backup-123",
                "job-uid",
                new V1OwnerReference
                {
                    Kind = V1CronJob.KubeKind,
                    Name = "backup",
                    Uid = "cronjob-uid",
                    Controller = true,
                }),
        };
        var pod = CreatePod(
            "backup-123-pod",
            "pod-uid",
            new V1OwnerReference
            {
                Kind = V1Job.KubeKind,
                Name = "backup-123",
                Uid = "job-uid",
                Controller = true,
            });
        AddResources(harness.Cluster, cronJob, job, pod);

        var resolver = new PodLogSessionResolver();
        var state = resolver.CreateState(cronJob, string.Empty, false, false);

        var resolution = resolver.TryResolve(harness.Cluster, state);

        resolution.ShouldNotBeNull();
        resolution!.Pod.Name().ShouldBe("backup-123-pod");
    }

    private static V1ObjectMeta Metadata(string name, string uid, V1OwnerReference? owner = null)
    {
        return new V1ObjectMeta
        {
            Name = name,
            NamespaceProperty = "default",
            Uid = uid,
            OwnerReferences = owner is null ? null : [owner],
        };
    }

    private static V1Pod CreatePod(string name, string uid, V1OwnerReference owner)
    {
        return new V1Pod
        {
            Metadata = Metadata(name, uid, owner),
            Spec = new V1PodSpec
            {
                Containers = [new V1Container { Name = "app" }],
            },
        };
    }

    private static void AddResources(
        Cluster cluster,
        V1Deployment deployment,
        V1ReplicaSet replicaSet,
        V1Pod pod)
    {
        AddResource(cluster, GroupApiVersionKind.From<V1Deployment>(), deployment);
        AddResource(cluster, GroupApiVersionKind.From<V1ReplicaSet>(), replicaSet);
        AddResource(cluster, GroupApiVersionKind.From<V1Pod>(), pod);
    }

    private static void AddResources(Cluster cluster, V1CronJob cronJob, V1Job job, V1Pod pod)
    {
        AddResource(cluster, GroupApiVersionKind.From<V1CronJob>(), cronJob);
        AddResource(cluster, GroupApiVersionKind.From<V1Job>(), job);
        AddResource(cluster, GroupApiVersionKind.From<V1Pod>(), pod);
    }

    private static void AddResource<T>(Cluster cluster, GroupApiVersionKind kind, T resource)
        where T : class, k8s.IKubernetesObject<V1ObjectMeta>, new()
    {
        var container = new ContainerClass<T>();
        container.Items.AddOrUpdate(resource);
        cluster.Objects[kind] = container;
    }
}
