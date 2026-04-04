using k8s.Models;
using KubeUI.Testing;
using Shouldly;

namespace KubeUI.Kubernetes.Tests.PodLogs;

public sealed class PodLogSessionResolverTests
{
    private readonly PodLogSessionResolver _resolver = new();

    [Fact]
    public void CreateState_should_capture_pod_identity_and_preferred_owner()
    {
        V1Pod pod = CreatePod(
            name: "app-7c9dd9f4f4-abcde",
            namespaceName: "default",
            uid: "pod-uid",
            ownerUid: "replicaset-uid",
            ownerName: "app-7c9dd9f4f4",
            ownerKind: "ReplicaSet",
            containers: ["app"],
            creationTimestamp: new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc));

        PodLogSessionState state = _resolver.CreateState(pod, "app", previous: true, timestamps: false, tailLines: 250);

        state.PodNamespace.ShouldBe("default");
        state.PodName.ShouldBe("app-7c9dd9f4f4-abcde");
        state.PodUid.ShouldBe("pod-uid");
        state.OwnerUid.ShouldBe("replicaset-uid");
        state.OwnerName.ShouldBe("app-7c9dd9f4f4");
        state.OwnerKind.ShouldBe("ReplicaSet");
        state.ContainerName.ShouldBe("app");
        state.Previous.ShouldBeTrue();
        state.Timestamps.ShouldBeFalse();
        state.TailLines.ShouldBe(250);
    }

    [Fact]
    public async Task TryResolve_should_keep_the_current_pod_and_enable_previous_logs_when_container_restarted()
    {
        TestClusterRuntime cluster = new();
        V1Pod pod = CreatePod(
            name: "app-7c9dd9f4f4-abcde",
            namespaceName: "default",
            uid: "pod-uid",
            ownerUid: "replicaset-uid",
            ownerName: "app-7c9dd9f4f4",
            ownerKind: "ReplicaSet",
            containers: ["app", "sidecar"],
            restartCount: 2,
            creationTimestamp: new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc));

        await cluster.AddOrUpdateResource(pod);

        PodLogSessionState state = _resolver.CreateState(pod, "app", previous: true, timestamps: true);
        PodLogSessionResolution? resolution = _resolver.TryResolve(cluster, state);

        resolution.ShouldNotBeNull();
        resolution!.Pod.Name().ShouldBe("app-7c9dd9f4f4-abcde");
        resolution.ContainerName.ShouldBe("app");
        resolution.PodChanged.ShouldBeFalse();
        resolution.PreviousLogsAvailable.ShouldBeTrue();
        resolution.RelatedPods.Count.ShouldBe(1);

        PodLogReadOptions options = _resolver.CreateReadOptions(state, resolution);

        options.PodNamespace.ShouldBe("default");
        options.PodName.ShouldBe("app-7c9dd9f4f4-abcde");
        options.ContainerName.ShouldBe("app");
        options.Previous.ShouldBeTrue();
        options.Timestamps.ShouldBeTrue();
        options.Follow.ShouldBeTrue();
        options.TailLines.ShouldBe(100);
    }

    [Fact]
    public async Task TryResolve_should_retarget_to_newest_sibling_pod_when_the_original_pod_has_been_replaced()
    {
        TestClusterRuntime cluster = new();
        V1Pod oldPod = CreatePod(
            name: "app-7c9dd9f4f4-abcde",
            namespaceName: "default",
            uid: "old-pod-uid",
            ownerUid: "replicaset-uid",
            ownerName: "app-7c9dd9f4f4",
            ownerKind: "ReplicaSet",
            containers: ["app"],
            restartCount: 0,
            creationTimestamp: new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc));

        V1Pod newPod = CreatePod(
            name: "app-7c9dd9f4f4-fghij",
            namespaceName: "default",
            uid: "new-pod-uid",
            ownerUid: "replicaset-uid",
            ownerName: "app-7c9dd9f4f4",
            ownerKind: "ReplicaSet",
            containers: ["app"],
            restartCount: 1,
            creationTimestamp: new DateTime(2026, 4, 1, 12, 5, 0, DateTimeKind.Utc));

        await cluster.AddOrUpdateResource(newPod);

        PodLogSessionState state = _resolver.CreateState(oldPod, "app", previous: true, timestamps: false);
        PodLogSessionResolution? resolution = _resolver.TryResolve(cluster, state);

        resolution.ShouldNotBeNull();
        resolution!.Pod.Name().ShouldBe("app-7c9dd9f4f4-fghij");
        resolution.PodChanged.ShouldBeTrue();
        resolution.PreviousLogsAvailable.ShouldBeTrue();
        resolution.RelatedPods.Count.ShouldBe(1);
        resolution.RelatedPods[0].Name().ShouldBe("app-7c9dd9f4f4-fghij");

        PodLogReadOptions options = _resolver.CreateReadOptions(state, resolution);

        options.PodName.ShouldBe("app-7c9dd9f4f4-fghij");
        options.Previous.ShouldBeTrue();
        options.Timestamps.ShouldBeFalse();
    }

    [Fact]
    public async Task TryResolve_should_fallback_to_the_first_available_container_and_disable_previous_logs_when_restart_count_is_zero()
    {
        TestClusterRuntime cluster = new();
        V1Pod pod = CreatePod(
            name: "app-7c9dd9f4f4-abcde",
            namespaceName: "default",
            uid: "pod-uid",
            ownerUid: "replicaset-uid",
            ownerName: "app-7c9dd9f4f4",
            ownerKind: "ReplicaSet",
            containers: ["app", "sidecar"],
            restartCount: 0,
            creationTimestamp: new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc));

        await cluster.AddOrUpdateResource(pod);

        PodLogSessionState state = _resolver.CreateState(pod, "missing-container", previous: true, timestamps: true, tailLines: -1);
        PodLogSessionResolution? resolution = _resolver.TryResolve(cluster, state);

        resolution.ShouldNotBeNull();
        resolution!.ContainerName.ShouldBe("app");
        resolution.PreviousLogsAvailable.ShouldBeFalse();

        PodLogReadOptions options = _resolver.CreateReadOptions(state, resolution);

        options.ContainerName.ShouldBe("app");
        options.Previous.ShouldBeFalse();
        options.TailLines.ShouldBe(100);
    }

    private static V1Pod CreatePod(
        string name,
        string namespaceName,
        string uid,
        string? ownerUid = null,
        string? ownerName = null,
        string? ownerKind = null,
        string[]? containers = null,
        int restartCount = 0,
        DateTime? creationTimestamp = null)
    {
        List<V1Container> containerList = [];
        if (containers is not null)
        {
            for (int i = 0; i < containers.Length; i++)
            {
                containerList.Add(new V1Container { Name = containers[i] });
            }
        }

        List<V1OwnerReference>? ownerReferences = null;
        if (!string.IsNullOrWhiteSpace(ownerUid))
        {
            ownerReferences = [
                new V1OwnerReference
                {
                    Uid = ownerUid,
                    Name = ownerName,
                    Kind = ownerKind,
                    Controller = true,
                },
            ];
        }

        List<V1ContainerStatus>? containerStatuses = null;
        if (containerList.Count > 0)
        {
            containerStatuses = [
                new V1ContainerStatus
                {
                    Name = containerList[0].Name,
                    RestartCount = restartCount,
                },
            ];
        }

        return new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = name,
                NamespaceProperty = namespaceName,
                Uid = uid,
                OwnerReferences = ownerReferences,
                CreationTimestamp = creationTimestamp,
            },
            Spec = new V1PodSpec
            {
                Containers = containerList,
            },
            Status = new V1PodStatus
            {
                ContainerStatuses = containerStatuses,
            },
        };
    }
}
