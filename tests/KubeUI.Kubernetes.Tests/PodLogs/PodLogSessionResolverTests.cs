using DynamicData;
using k8s;
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
        resolution.ParentResource.ShouldBeNull();
    }

    [Fact]
    public async Task Resolution_exposes_the_immediate_parent_one_level_at_a_time()
    {
        await using var harness = await new TestClusterGenerator().CreateAsync(
            new TestClusterConfig { Type = KubernetesBackend.Fake },
            TestContext.Current.CancellationToken);
        V1Deployment deployment = new()
        {
            Metadata = Metadata("api", "deployment-uid"),
        };
        V1ReplicaSet replicaSet = new()
        {
            Metadata = Metadata(
                "api-rs",
                "replicaset-uid",
                new V1OwnerReference
                {
                    Kind = V1Deployment.KubeKind,
                    Name = deployment.Name(),
                    Uid = deployment.Uid(),
                    Controller = true,
                }),
        };
        V1Pod pod = CreatePod(
            "api-pod",
            "pod-uid",
            new V1OwnerReference
            {
                Kind = V1ReplicaSet.KubeKind,
                Name = replicaSet.Name(),
                Uid = replicaSet.Uid(),
                Controller = true,
            });
        AddResources(harness.Cluster, deployment, replicaSet, pod);
        PodLogSessionResolver resolver = new();

        PodLogSessionResolution? podResolution = resolver.TryResolve(
            harness.Cluster,
            resolver.CreateState(pod, "app", false, false));
        PodLogSessionResolution? replicaSetResolution = resolver.TryResolve(
            harness.Cluster,
            resolver.CreateState(replicaSet, "app", false, false));

        podResolution.ShouldNotBeNull();
        podResolution.ParentResource.ShouldBeSameAs(replicaSet);
        replicaSetResolution.ShouldNotBeNull();
        replicaSetResolution.ParentResource.ShouldBeSameAs(deployment);
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
        PodLogSessionResolution? jobResolution = resolver.TryResolve(
            harness.Cluster,
            resolver.CreateState(job, string.Empty, false, false));

        resolution.ShouldNotBeNull();
        resolution!.Pod.Name().ShouldBe("backup-123-pod");
        resolution.ParentResource.ShouldBeNull();
        jobResolution.ShouldNotBeNull();
        jobResolution.ParentResource.ShouldBeSameAs(cronJob);
    }

    [Fact]
    public void CreateState_captures_resource_identity_preferences_and_controller_owner()
    {
        V1Pod pod = new()
        {
            Metadata = new V1ObjectMeta
            {
                Name = "api",
                NamespaceProperty = "production",
                Uid = "pod-uid",
                OwnerReferences =
                [
                    new V1OwnerReference { Kind = V1Job.KubeKind, Name = "non-controller", Uid = "job-uid" },
                    new V1OwnerReference
                    {
                        Kind = V1ReplicaSet.KubeKind,
                        Name = "api-rs",
                        Uid = "replicaset-uid",
                        Controller = true,
                    },
                ],
            },
        };
        PodLogSessionResolver resolver = new();

        PodLogSessionState state = resolver.CreateState(pod, "app", previous: true, timestamps: true, tailLines: 0);

        state.ResourceNamespace.ShouldBe("production");
        state.ResourceName.ShouldBe("api");
        state.ResourceUid.ShouldBe("pod-uid");
        state.ResourceKind.ShouldBe(V1Pod.KubeKind);
        state.OwnerKind.ShouldBe(V1ReplicaSet.KubeKind);
        state.OwnerName.ShouldBe("api-rs");
        state.OwnerUid.ShouldBe("replicaset-uid");
        state.ContainerName.ShouldBe("app");
        state.Previous.ShouldBeTrue();
        state.Timestamps.ShouldBeTrue();
        state.TailLines.ShouldBe(100);
        Should.Throw<ArgumentNullException>(() => resolver.CreateState(null!, string.Empty, false, false));
    }

    [Fact]
    public async Task Direct_pod_resolution_keeps_the_current_pod_and_sorts_matching_siblings()
    {
        await using var harness = await new TestClusterGenerator().CreateAsync(
            new TestClusterConfig { Type = KubernetesBackend.Fake },
            TestContext.Current.CancellationToken);
        V1OwnerReference owner = new()
        {
            Kind = V1ReplicaSet.KubeKind,
            Name = "api-rs",
            Uid = "replicaset-uid",
            Controller = true,
        };
        V1Pod current = CreatePod("current", "current-uid", owner);
        current.Metadata!.CreationTimestamp = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        V1Pod newest = CreatePod("newest", "newest-uid", owner);
        newest.Metadata!.CreationTimestamp = new DateTime(2026, 1, 1, 12, 2, 0, DateTimeKind.Utc);
        V1Pod alpha = CreatePod("alpha", "alpha-uid", owner);
        alpha.Metadata!.CreationTimestamp = new DateTime(2026, 1, 1, 12, 1, 0, DateTimeKind.Utc);
        V1Pod beta = CreatePod("beta", "beta-uid", owner);
        beta.Metadata!.CreationTimestamp = alpha.Metadata.CreationTimestamp;
        V1Pod unrelated = CreatePod(
            "unrelated",
            "unrelated-uid",
            new V1OwnerReference { Kind = V1ReplicaSet.KubeKind, Name = "other", Uid = "other-uid", Controller = true });
        V1Pod otherNamespace = CreatePod("other-namespace", "other-namespace-uid", owner);
        otherNamespace.Metadata!.NamespaceProperty = "other";
        AddResource(harness.Cluster, GroupApiVersionKind.From<V1Pod>(), current);
        AddResource(harness.Cluster, GroupApiVersionKind.From<V1Pod>(), newest);
        AddResource(harness.Cluster, GroupApiVersionKind.From<V1Pod>(), beta);
        AddResource(harness.Cluster, GroupApiVersionKind.From<V1Pod>(), alpha);
        AddResource(harness.Cluster, GroupApiVersionKind.From<V1Pod>(), unrelated);
        AddResource(harness.Cluster, GroupApiVersionKind.From<V1Pod>(), otherNamespace);
        PodLogSessionResolver resolver = new();

        PodLogSessionResolution? resolution = resolver.TryResolve(
            harness.Cluster,
            resolver.CreateState(current, "app", false, false));

        resolution.ShouldNotBeNull();
        resolution!.Pod.ShouldBeSameAs(current);
        resolution.PodChanged.ShouldBeFalse();
        resolution.RelatedPods.Select(pod => pod.Name()).ShouldBe(["newest", "alpha", "beta", "current"]);
    }

    [Fact]
    public async Task Removed_pod_resolves_to_the_newest_matching_owner_and_missing_owner_returns_null()
    {
        await using var harness = await new TestClusterGenerator().CreateAsync(
            new TestClusterConfig { Type = KubernetesBackend.Fake },
            TestContext.Current.CancellationToken);
        V1OwnerReference owner = new()
        {
            Kind = V1ReplicaSet.KubeKind,
            Name = "api-rs",
            Uid = "replicaset-uid",
            Controller = true,
        };
        V1Pod removed = CreatePod("removed", "removed-uid", owner);
        V1Pod older = CreatePod("older", "older-uid", owner);
        older.Metadata!.CreationTimestamp = new DateTime(2026, 1, 1, 12, 0, 0, DateTimeKind.Utc);
        V1Pod newer = CreatePod("newer", "newer-uid", owner);
        newer.Metadata!.CreationTimestamp = new DateTime(2026, 1, 1, 12, 1, 0, DateTimeKind.Utc);
        AddResource(harness.Cluster, GroupApiVersionKind.From<V1Pod>(), older);
        AddResource(harness.Cluster, GroupApiVersionKind.From<V1Pod>(), newer);
        PodLogSessionResolver resolver = new();

        PodLogSessionResolution? resolution = resolver.TryResolve(
            harness.Cluster,
            resolver.CreateState(removed, "app", false, false));
        PodLogSessionState missingState = resolver.CreateState(
            new V1Pod { Metadata = Metadata("missing", "missing-uid") },
            "app",
            false,
            false);

        resolution.ShouldNotBeNull();
        resolution!.Pod.ShouldBeSameAs(newer);
        resolution.PodChanged.ShouldBeTrue();
        resolver.TryResolve(harness.Cluster, missingState).ShouldBeNull();
        Should.Throw<ArgumentNullException>(() => resolver.TryResolve(null!, missingState));
        Should.Throw<ArgumentNullException>(() => resolver.TryResolve(harness.Cluster, null!));
    }

    [Fact]
    public async Task Resolution_supports_init_ephemeral_and_missing_container_fallbacks()
    {
        await using var harness = await new TestClusterGenerator().CreateAsync(
            new TestClusterConfig { Type = KubernetesBackend.Fake },
            TestContext.Current.CancellationToken);
        V1Pod pod = new()
        {
            Metadata = Metadata("api", "pod-uid"),
            Spec = new V1PodSpec
            {
                Containers = [new V1Container { Name = "app" }],
                InitContainers = [new V1Container { Name = "setup" }],
                EphemeralContainers = [new V1EphemeralContainer { Name = "debug" }],
            },
            Status = new V1PodStatus
            {
                ContainerStatuses = [new V1ContainerStatus { Name = "app", RestartCount = 0 }],
                InitContainerStatuses = [new V1ContainerStatus { Name = "setup", RestartCount = 1 }],
                EphemeralContainerStatuses = [new V1ContainerStatus { Name = "debug", RestartCount = 2 }],
            },
        };
        AddResource(harness.Cluster, GroupApiVersionKind.From<V1Pod>(), pod);
        PodLogSessionResolver resolver = new();

        PodLogSessionResolution init = resolver.TryResolve(
            harness.Cluster,
            resolver.CreateState(pod, "setup", true, false)).ShouldNotBeNull();
        PodLogSessionResolution ephemeral = resolver.TryResolve(
            harness.Cluster,
            resolver.CreateState(pod, "debug", true, false)).ShouldNotBeNull();
        PodLogSessionResolution fallback = resolver.TryResolve(
            harness.Cluster,
            resolver.CreateState(pod, "missing", false, false)).ShouldNotBeNull();

        init.ContainerName.ShouldBe("setup");
        init.PreviousLogsAvailable.ShouldBeTrue();
        ephemeral.ContainerName.ShouldBe("debug");
        ephemeral.PreviousLogsAvailable.ShouldBeTrue();
        fallback.ContainerName.ShouldBe("app");
        fallback.PreviousLogsAvailable.ShouldBeFalse();
    }

    [Fact]
    public async Task Unknown_workload_kind_resolves_a_direct_owned_pod_by_name_when_uids_are_missing()
    {
        await using var harness = await new TestClusterGenerator().CreateAsync(
            new TestClusterConfig { Type = KubernetesBackend.Fake },
            TestContext.Current.CancellationToken);
        V1ConfigMap workload = new()
        {
            Kind = "Widget",
            Metadata = new V1ObjectMeta { Name = "custom", NamespaceProperty = "default" },
        };
        V1Pod pod = CreatePod(
            "custom-pod",
            "pod-uid",
            new V1OwnerReference { Kind = "Widget", Name = "custom", Controller = true });
        AddResource(harness.Cluster, GroupApiVersionKind.From<V1ConfigMap>(), workload);
        AddResource(harness.Cluster, GroupApiVersionKind.From<V1Pod>(), pod);
        PodLogSessionResolver resolver = new();

        PodLogSessionState state = resolver.CreateState(workload, "app", false, false, tailLines: 25);
        PodLogSessionResolution? resolution = resolver.TryResolve(harness.Cluster, state);

        state.ResourceKind.ShouldBe("Widget");
        state.TailLines.ShouldBe(25);
        resolution.ShouldNotBeNull();
        resolution!.Pod.ShouldBeSameAs(pod);
    }

    [Fact]
    public async Task Workload_resolution_uses_name_indexes_and_ignores_broken_owner_chains()
    {
        await using var harness = await new TestClusterGenerator().CreateAsync(
            new TestClusterConfig { Type = KubernetesBackend.Fake },
            TestContext.Current.CancellationToken);
        V1Deployment deployment = new()
        {
            Metadata = new V1ObjectMeta { Name = "api", NamespaceProperty = "default" },
        };
        V1ReplicaSet replicaSet = new()
        {
            Metadata = Metadata(
                "api-rs",
                "replicaset-uid",
                new V1OwnerReference { Kind = V1Deployment.KubeKind, Name = "api", Controller = true }),
        };
        V1Pod matching = CreatePod(
            "matching",
            "matching-uid",
            new V1OwnerReference { Kind = V1ReplicaSet.KubeKind, Name = "api-rs", Uid = "stale-replicaset-uid", Controller = true });
        V1Pod missingOwner = CreatePod(
            "missing-owner",
            "missing-owner-uid",
            new V1OwnerReference { Kind = V1ReplicaSet.KubeKind, Name = "absent", Controller = true });
        V1Pod ownerless = new()
        {
            Metadata = Metadata("ownerless", "ownerless-uid"),
            Spec = new V1PodSpec { Containers = [new V1Container { Name = "app" }] },
        };
        V1ReplicaSet cycleA = new()
        {
            Metadata = Metadata(
                "cycle-a",
                "cycle-a-uid",
                new V1OwnerReference { Kind = V1ReplicaSet.KubeKind, Name = "cycle-b", Uid = "cycle-b-uid" }),
        };
        V1ReplicaSet cycleB = new()
        {
            Metadata = Metadata(
                "cycle-b",
                "cycle-b-uid",
                new V1OwnerReference { Kind = V1ReplicaSet.KubeKind, Name = "cycle-a", Uid = "cycle-a-uid" }),
        };
        V1Pod cyclic = CreatePod(
            "cyclic",
            "cyclic-uid",
            new V1OwnerReference { Kind = V1ReplicaSet.KubeKind, Name = "cycle-a", Uid = "cycle-a-uid" });
        AddResource(harness.Cluster, GroupApiVersionKind.From<V1Deployment>(), deployment);
        AddResource(harness.Cluster, GroupApiVersionKind.From<V1ReplicaSet>(), replicaSet);
        AddResource(harness.Cluster, GroupApiVersionKind.From<V1ReplicaSet>(), cycleA);
        AddResource(harness.Cluster, GroupApiVersionKind.From<V1ReplicaSet>(), cycleB);
        AddResource(harness.Cluster, GroupApiVersionKind.From<V1Pod>(), matching);
        AddResource(harness.Cluster, GroupApiVersionKind.From<V1Pod>(), missingOwner);
        AddResource(harness.Cluster, GroupApiVersionKind.From<V1Pod>(), ownerless);
        AddResource(harness.Cluster, GroupApiVersionKind.From<V1Pod>(), cyclic);
        harness.Cluster.Objects[new GroupApiVersionKind("example.dev", "v1", "Ignored", "ignored")] = new object();
        PodLogSessionResolver resolver = new();

        PodLogSessionResolution? resolution = resolver.TryResolve(
            harness.Cluster,
            resolver.CreateState(deployment, "app", false, false));

        resolution.ShouldNotBeNull();
        resolution!.RelatedPods.ShouldBe([matching]);
    }

    [Fact]
    public async Task Container_fallback_prefers_init_then_ephemeral_and_preserves_requested_name_when_empty()
    {
        await using var harness = await new TestClusterGenerator().CreateAsync(
            new TestClusterConfig { Type = KubernetesBackend.Fake },
            TestContext.Current.CancellationToken);
        PodLogSessionResolver resolver = new();
        V1Pod initOnly = new()
        {
            Metadata = Metadata("init-only", "init-uid"),
            Spec = new V1PodSpec
            {
                Containers = [],
                InitContainers = [new V1Container { Name = "setup" }],
            },
        };
        V1Pod ephemeralOnly = new()
        {
            Metadata = Metadata("ephemeral-only", "ephemeral-uid"),
            Spec = new V1PodSpec
            {
                Containers = [],
                EphemeralContainers = [new V1EphemeralContainer { Name = "debug" }],
            },
        };
        V1Pod empty = new()
        {
            Metadata = Metadata("empty", "empty-uid"),
            Spec = new V1PodSpec { Containers = [] },
        };
        AddResource(harness.Cluster, GroupApiVersionKind.From<V1Pod>(), initOnly);
        AddResource(harness.Cluster, GroupApiVersionKind.From<V1Pod>(), ephemeralOnly);
        AddResource(harness.Cluster, GroupApiVersionKind.From<V1Pod>(), empty);

        PodLogSessionResolution initResolution = resolver.TryResolve(
            harness.Cluster,
            resolver.CreateState(initOnly, "missing", false, false)).ShouldNotBeNull();
        PodLogSessionResolution ephemeralResolution = resolver.TryResolve(
            harness.Cluster,
            resolver.CreateState(ephemeralOnly, "missing", false, false)).ShouldNotBeNull();
        PodLogSessionResolution emptyResolution = resolver.TryResolve(
            harness.Cluster,
            resolver.CreateState(empty, "requested", false, false)).ShouldNotBeNull();

        initResolution.ContainerName.ShouldBe("setup");
        ephemeralResolution.ContainerName.ShouldBe("debug");
        emptyResolution.ContainerName.ShouldBe("requested");
    }

    [Fact]
    public void CreateState_supports_all_workload_kinds_and_first_owner_fallback()
    {
        PodLogSessionResolver resolver = new();
        IKubernetesObject<V1ObjectMeta>[] workloads =
        [
            new V1Deployment { Metadata = Metadata("deployment", "1") },
            new V1ReplicaSet { Metadata = Metadata("replicaset", "2") },
            new V1DaemonSet { Metadata = Metadata("daemonset", "3") },
            new V1StatefulSet { Metadata = Metadata("statefulset", "4") },
            new V1Job { Metadata = Metadata("job", "5") },
            new V1CronJob { Metadata = Metadata("cronjob", "6") },
        ];

        workloads.Select(resource => resolver.CreateState(resource, string.Empty, false, false).ResourceKind)
            .ShouldBe([
                V1Deployment.KubeKind,
                V1ReplicaSet.KubeKind,
                V1DaemonSet.KubeKind,
                V1StatefulSet.KubeKind,
                V1Job.KubeKind,
                V1CronJob.KubeKind,
            ]);

        V1Pod pod = CreatePod(
            "pod",
            "pod-uid",
            new V1OwnerReference { Kind = V1Job.KubeKind, Name = "job", Uid = "job-uid" });
        PodLogSessionState state = resolver.CreateState(pod, "app", false, false);
        state.OwnerUid.ShouldBe("job-uid");
    }

    [Fact]
    public async Task Direct_pod_without_uid_is_not_duplicated_in_related_pods()
    {
        await using var harness = await new TestClusterGenerator().CreateAsync(
            new TestClusterConfig { Type = KubernetesBackend.Fake },
            TestContext.Current.CancellationToken);
        V1Pod pod = new()
        {
            Metadata = new V1ObjectMeta { Name = "pod", NamespaceProperty = "default" },
            Spec = new V1PodSpec { Containers = [new V1Container { Name = "app" }] },
        };
        AddResource(harness.Cluster, GroupApiVersionKind.From<V1Pod>(), pod);
        PodLogSessionResolver resolver = new();

        PodLogSessionResolution? resolution = resolver.TryResolve(
            harness.Cluster,
            resolver.CreateState(pod, "app", false, false));

        resolution.ShouldNotBeNull();
        resolution!.RelatedPods.ShouldBe([pod]);
        resolution.PodChanged.ShouldBeFalse();
    }

    [Fact]
    public async Task Deployment_state_ignores_an_unrelated_same_named_pod()
    {
        await using var harness = await new TestClusterGenerator().CreateAsync(
            new TestClusterConfig { Type = KubernetesBackend.Fake },
            TestContext.Current.CancellationToken);
        V1Deployment deployment = new()
        {
            Metadata = Metadata("api", "deployment-uid"),
        };
        V1ReplicaSet replicaSet = new()
        {
            Metadata = Metadata(
                "api-rs",
                "replicaset-uid",
                new V1OwnerReference
                {
                    Kind = V1Deployment.KubeKind,
                    Name = deployment.Name(),
                    Uid = deployment.Uid(),
                    Controller = true,
                }),
        };
        V1Pod descendantPod = CreatePod(
            "api-rs-pod",
            "descendant-pod-uid",
            new V1OwnerReference
            {
                Kind = V1ReplicaSet.KubeKind,
                Name = replicaSet.Name(),
                Uid = replicaSet.Uid(),
                Controller = true,
            });
        V1Pod unrelatedPod = CreatePod(
            "api",
            "unrelated-pod-uid",
            new V1OwnerReference { Kind = V1ReplicaSet.KubeKind, Name = "unrelated", Uid = "unrelated-uid" });
        AddResources(harness.Cluster, deployment, replicaSet, descendantPod);
        AddResource(harness.Cluster, GroupApiVersionKind.From<V1Pod>(), unrelatedPod);
        PodLogSessionResolver resolver = new();
        PodLogSessionState state = resolver.CreateState(deployment, "app", false, false);

        PodLogSessionResolution? resolution = resolver.TryResolve(harness.Cluster, state);

        resolution.ShouldNotBeNull();
        resolution!.Pod.ShouldBeSameAs(descendantPod);
        resolution.RelatedPods.ShouldBe([descendantPod]);
    }

    [Fact]
    public void CreateState_handles_missing_metadata_and_owner_variants()
    {
        PodLogSessionResolver resolver = new();
        V1Pod noMetadata = new();
        V1Pod noOwners = new() { Metadata = new V1ObjectMeta() };
        V1Pod emptyOwners = new() { Metadata = new V1ObjectMeta { OwnerReferences = [] } };
        V1Pod firstOwnerFallback = new()
        {
            Metadata = new V1ObjectMeta
            {
                OwnerReferences =
                [
                    new V1OwnerReference { Kind = V1Job.KubeKind, Name = "first", Uid = "first-uid" },
                    new V1OwnerReference { Kind = V1Job.KubeKind, Name = "second", Uid = "second-uid" },
                ],
            },
        };

        PodLogSessionState missingMetadataState = resolver.CreateState(noMetadata, string.Empty, false, false);
        PodLogSessionState noOwnersState = resolver.CreateState(noOwners, string.Empty, false, false);
        PodLogSessionState emptyOwnersState = resolver.CreateState(emptyOwners, string.Empty, false, false);
        PodLogSessionState fallbackState = resolver.CreateState(firstOwnerFallback, string.Empty, false, false);

        missingMetadataState.ResourceName.ShouldBeEmpty();
        missingMetadataState.ResourceNamespace.ShouldBeEmpty();
        noOwnersState.OwnerUid.ShouldBeNull();
        emptyOwnersState.OwnerUid.ShouldBeNull();
        fallbackState.OwnerName.ShouldBe("first");
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
        ContainerClass<T> container = cluster.Objects.TryGetValue(kind, out var existing)
            ? existing.ShouldBeOfType<ContainerClass<T>>()
            : new ContainerClass<T>();
        container.Items.AddOrUpdate(resource);
        cluster.Objects[kind] = container;
    }
}
