using k8s;
using k8s.Models;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Pod.ViewModels;

internal static class PodLogTopologyComparer
{
    internal static bool HasChanged(PodLogSessionResolution current, PodLogSessionResolution next)
    {
        if (!string.Equals(current.Pod.Metadata?.Uid, next.Pod.Metadata?.Uid, StringComparison.Ordinal)
            || current.RelatedPods.Count != next.RelatedPods.Count
            || !IsSameResource(current.ParentResource, next.ParentResource))
        {
            return true;
        }

        for (var i = 0; i < current.RelatedPods.Count; i++)
        {
            V1Pod currentPod = current.RelatedPods[i];
            V1Pod nextPod = next.RelatedPods[i];
            if (!string.Equals(currentPod.Metadata?.Uid, nextPod.Metadata?.Uid, StringComparison.Ordinal)
                || !GetContainerNames(currentPod).SequenceEqual(GetContainerNames(nextPod), StringComparer.Ordinal)
                || !HaveSameContainerLogStates(currentPod, nextPod))
            {
                return true;
            }
        }

        return !string.Equals(current.ContainerName, next.ContainerName, StringComparison.Ordinal);
    }

    private static bool HaveSameContainerLogStates(V1Pod current, V1Pod next)
    {
        return HaveSameContainerLogStates(current.Status?.ContainerStatuses, next.Status?.ContainerStatuses)
            && HaveSameContainerLogStates(current.Status?.InitContainerStatuses, next.Status?.InitContainerStatuses)
            && HaveSameContainerLogStates(current.Status?.EphemeralContainerStatuses, next.Status?.EphemeralContainerStatuses);
    }

    private static bool HaveSameContainerLogStates(
        IList<V1ContainerStatus>? current,
        IList<V1ContainerStatus>? next)
    {
        if (current is null || next is null)
        {
            return current is null && next is null;
        }

        if (current.Count != next.Count)
        {
            return false;
        }

        for (var i = 0; i < current.Count; i++)
        {
            V1ContainerStatus currentStatus = current[i];
            V1ContainerStatus? nextStatus = null;
            for (var j = 0; j < next.Count; j++)
            {
                if (string.Equals(currentStatus.Name, next[j].Name, StringComparison.Ordinal))
                {
                    nextStatus = next[j];
                    break;
                }
            }

            if (nextStatus is null
                || currentStatus.RestartCount != nextStatus.RestartCount
                || (currentStatus.State?.Running is not null) != (nextStatus.State?.Running is not null)
                || (currentStatus.State?.Waiting is not null) != (nextStatus.State?.Waiting is not null)
                || (currentStatus.State?.Terminated is not null) != (nextStatus.State?.Terminated is not null)
                || currentStatus.State?.Terminated?.ExitCode != nextStatus.State?.Terminated?.ExitCode
                || currentStatus.State?.Terminated?.FinishedAt != nextStatus.State?.Terminated?.FinishedAt
                || currentStatus.LastState?.Terminated?.FinishedAt != nextStatus.LastState?.Terminated?.FinishedAt)
            {
                return false;
            }
        }

        return true;
    }

    private static bool IsSameResource(
        IKubernetesObject<V1ObjectMeta>? current,
        IKubernetesObject<V1ObjectMeta>? next)
    {
        if (current is null || next is null)
        {
            return current is null && next is null;
        }

        var currentUid = current.Metadata?.Uid;
        var nextUid = next.Metadata?.Uid;
        if (!string.IsNullOrWhiteSpace(currentUid) || !string.IsNullOrWhiteSpace(nextUid))
        {
            return string.Equals(currentUid, nextUid, StringComparison.Ordinal);
        }

        return current.GetType() == next.GetType()
            && string.Equals(current.Namespace(), next.Namespace(), StringComparison.Ordinal)
            && string.Equals(current.Name(), next.Name(), StringComparison.Ordinal);
    }

    private static IEnumerable<string?> GetContainerNames(V1Pod pod)
    {
        return (pod.Spec?.Containers ?? []).Select(container => container.Name)
            .Concat((pod.Spec?.InitContainers ?? []).Select(container => container.Name))
            .Concat((pod.Spec?.EphemeralContainers ?? []).Select(container => container.Name));
    }
}
