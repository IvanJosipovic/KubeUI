using k8s;
using k8s.Models;
using KubeUI.Avalonia.Resources.Workloads.v1.Pod.ViewModels;
using KubeUI.Kubernetes;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Resources.Workloads.v1.Pod;

public sealed class PodLogTopologyComparerTests
{
    [Fact]
    public void Identical_topology_is_unchanged()
    {
        PodLogSessionResolution resolution = CreateResolution(CreatePod());

        PodLogTopologyComparer.HasChanged(resolution, resolution).ShouldBeFalse();
    }

    [Theory]
    [InlineData("pod-uid")]
    [InlineData("related-count")]
    [InlineData("related-uid")]
    [InlineData("app-container")]
    [InlineData("init-container")]
    [InlineData("ephemeral-container")]
    [InlineData("selected-container")]
    public void Pod_or_container_identity_changes_are_detected(string change)
    {
        PodLogSessionResolution current = CreateResolution(CreatePod());
        V1Pod nextPod = Clone(current.Pod);
        List<V1Pod> relatedPods = [nextPod];
        var containerName = "app";
        switch (change)
        {
            case "pod-uid":
                nextPod.Metadata!.Uid = "next-pod-uid";
                break;
            case "related-count":
                relatedPods.Add(Clone(nextPod));
                break;
            case "related-uid":
                nextPod.Metadata!.Uid = "next-related-uid";
                break;
            case "app-container":
                nextPod.Spec!.Containers[0].Name = "next-app";
                break;
            case "init-container":
                nextPod.Spec!.InitContainers![0].Name = "next-init";
                break;
            case "ephemeral-container":
                nextPod.Spec!.EphemeralContainers![0].Name = "next-debugger";
                break;
            case "selected-container":
                containerName = "sidecar";
                break;
            default:
                throw new InvalidOperationException(change);
        }

        PodLogSessionResolution next = current with
        {
            Pod = nextPod,
            RelatedPods = relatedPods,
            ContainerName = containerName,
        };

        PodLogTopologyComparer.HasChanged(current, next).ShouldBeTrue();
    }

    [Theory]
    [InlineData("both-null", false)]
    [InlineData("current-null", true)]
    [InlineData("next-null", true)]
    [InlineData("same-uid", false)]
    [InlineData("different-uid", true)]
    [InlineData("current-uid-only", true)]
    [InlineData("next-uid-only", true)]
    [InlineData("uidless-same", false)]
    [InlineData("uidless-type", true)]
    [InlineData("uidless-namespace", true)]
    [InlineData("uidless-name", true)]
    public void Parent_identity_uses_uid_then_resource_coordinates(string change, bool expectedChanged)
    {
        V1ReplicaSet currentParent = CreateReplicaSet("parent", "default", null);
        IKubernetesObject<V1ObjectMeta>? nextParent = CreateReplicaSet("parent", "default", null);
        IKubernetesObject<V1ObjectMeta>? currentResource = currentParent;
        switch (change)
        {
            case "both-null":
                currentResource = null;
                nextParent = null;
                break;
            case "current-null":
                currentResource = null;
                break;
            case "next-null":
                nextParent = null;
                break;
            case "same-uid":
                currentParent.Metadata!.Uid = "uid";
                nextParent!.Metadata!.Uid = "uid";
                break;
            case "different-uid":
                currentParent.Metadata!.Uid = "uid-1";
                nextParent!.Metadata!.Uid = "uid-2";
                break;
            case "current-uid-only":
                currentParent.Metadata!.Uid = "uid";
                break;
            case "next-uid-only":
                nextParent!.Metadata!.Uid = "uid";
                break;
            case "uidless-type":
                nextParent = new V1Deployment
                {
                    Metadata = new V1ObjectMeta { Name = "parent", NamespaceProperty = "default" },
                };
                break;
            case "uidless-namespace":
                nextParent!.Metadata!.NamespaceProperty = "other";
                break;
            case "uidless-name":
                nextParent!.Metadata!.Name = "other";
                break;
            case "uidless-same":
                break;
            default:
                throw new InvalidOperationException(change);
        }

        PodLogSessionResolution current = CreateResolution(CreatePod()) with { ParentResource = currentResource };
        PodLogSessionResolution next = current with { ParentResource = nextParent };

        PodLogTopologyComparer.HasChanged(current, next).ShouldBe(expectedChanged);
    }

    [Theory]
    [InlineData("current-status-null")]
    [InlineData("next-status-null")]
    [InlineData("count")]
    [InlineData("name")]
    [InlineData("restart")]
    [InlineData("running")]
    [InlineData("waiting")]
    [InlineData("terminated")]
    [InlineData("exit-code")]
    [InlineData("finished-at")]
    [InlineData("last-finished-at")]
    [InlineData("init")]
    [InlineData("ephemeral")]
    public void Container_log_state_changes_are_detected(string change)
    {
        V1Pod currentPod = CreatePod();
        V1Pod nextPod = Clone(currentPod);
        switch (change)
        {
            case "current-status-null":
                currentPod.Status!.ContainerStatuses = null;
                break;
            case "next-status-null":
                nextPod.Status!.ContainerStatuses = null;
                break;
            case "count":
                nextPod.Status!.ContainerStatuses!.Add(CreateStatus("other"));
                break;
            case "name":
                nextPod.Status!.ContainerStatuses![0].Name = "other";
                break;
            case "restart":
                nextPod.Status!.ContainerStatuses![0].RestartCount = 1;
                break;
            case "running":
                nextPod.Status!.ContainerStatuses![0].State = new V1ContainerState();
                break;
            case "waiting":
                nextPod.Status!.ContainerStatuses![0].State!.Waiting = new V1ContainerStateWaiting();
                break;
            case "terminated":
                nextPod.Status!.ContainerStatuses![0].State!.Terminated =
                    new V1ContainerStateTerminated { ExitCode = 0 };
                break;
            case "exit-code":
                SetTerminatedStates(currentPod, nextPod);
                nextPod.Status!.ContainerStatuses![0].State!.Terminated!.ExitCode = 1;
                break;
            case "finished-at":
                SetTerminatedStates(currentPod, nextPod);
                nextPod.Status!.ContainerStatuses![0].State!.Terminated!.FinishedAt =
                    new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc);
                break;
            case "last-finished-at":
                SetTerminatedStates(currentPod, nextPod);
                nextPod.Status!.ContainerStatuses![0].LastState!.Terminated!.FinishedAt =
                    new DateTime(2026, 1, 2, 0, 0, 0, DateTimeKind.Utc);
                break;
            case "init":
                nextPod.Status!.InitContainerStatuses![0].RestartCount = 1;
                break;
            case "ephemeral":
                nextPod.Status!.EphemeralContainerStatuses![0].RestartCount = 1;
                break;
            default:
                throw new InvalidOperationException(change);
        }

        PodLogTopologyComparer.HasChanged(
            CreateResolution(currentPod),
            CreateResolution(nextPod)).ShouldBeTrue();
    }

    private static PodLogSessionResolution CreateResolution(V1Pod pod)
    {
        return new PodLogSessionResolution(
            pod,
            "app",
            [pod],
            false,
            false,
            CreateReplicaSet("parent", "default", "parent-uid"));
    }

    private static V1Pod CreatePod()
    {
        return new V1Pod
        {
            Metadata = new V1ObjectMeta { Name = "pod", NamespaceProperty = "default", Uid = "pod-uid" },
            Spec = new V1PodSpec
            {
                Containers = [new V1Container { Name = "app" }],
                InitContainers = [new V1Container { Name = "init" }],
                EphemeralContainers = [new V1EphemeralContainer { Name = "debugger" }],
            },
            Status = new V1PodStatus
            {
                ContainerStatuses = [CreateStatus("app")],
                InitContainerStatuses = [CreateStatus("init")],
                EphemeralContainerStatuses = [CreateStatus("debugger")],
            },
        };
    }

    private static V1ContainerStatus CreateStatus(string name)
    {
        return new V1ContainerStatus
        {
            Name = name,
            State = new V1ContainerState { Running = new V1ContainerStateRunning() },
            LastState = new V1ContainerState
            {
                Terminated = new V1ContainerStateTerminated
                {
                    ExitCode = 0,
                    FinishedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                },
            },
        };
    }

    private static V1ReplicaSet CreateReplicaSet(string name, string namespaceName, string? uid)
    {
        return new V1ReplicaSet
        {
            Metadata = new V1ObjectMeta { Name = name, NamespaceProperty = namespaceName, Uid = uid },
        };
    }

    private static void SetTerminatedStates(V1Pod current, V1Pod next)
    {
        foreach (V1Pod pod in new[] { current, next })
        {
            pod.Status!.ContainerStatuses![0].State = new V1ContainerState
            {
                Terminated = new V1ContainerStateTerminated
                {
                    ExitCode = 0,
                    FinishedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
                },
            };
        }
    }

    private static V1Pod Clone(V1Pod pod)
    {
        return KubernetesJson.Deserialize<V1Pod>(KubernetesJson.Serialize(pod));
    }
}
