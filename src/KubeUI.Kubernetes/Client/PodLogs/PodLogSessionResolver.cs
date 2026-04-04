using k8s.Models;

namespace KubeUI.Kubernetes;

/// <summary>
/// Captures the pod identity and log preferences that a pod logs view needs to restore or retarget a session.
/// </summary>
public sealed record PodLogSessionState(
    string PodNamespace,
    string PodName,
    string? PodUid,
    string? OwnerUid,
    string? OwnerName,
    string? OwnerKind,
    string ContainerName,
    bool Previous,
    bool Timestamps,
    int TailLines);

/// <summary>
/// Describes the pod that should currently be queried for logs.
/// </summary>
public sealed record PodLogSessionResolution(
    V1Pod Pod,
    string ContainerName,
    IReadOnlyList<V1Pod> RelatedPods,
    bool PodChanged,
    bool PreviousLogsAvailable);

/// <summary>
/// Describes the concrete log request that should be sent to the Kubernetes API.
/// </summary>
public sealed record PodLogReadOptions(
    string PodNamespace,
    string PodName,
    string ContainerName,
    bool Previous,
    bool Timestamps,
    bool Follow,
    int TailLines);

/// <summary>
/// Resolves pod log session state against the current cluster contents.
/// </summary>
public interface IPodLogSessionResolver
{
    /// <summary>
    /// Captures the current pod identity and log preferences into a reusable session state.
    /// </summary>
    PodLogSessionState CreateState(V1Pod pod, string containerName, bool previous, bool timestamps, int tailLines = 100);

    /// <summary>
    /// Resolves the session state against the current cluster contents.
    /// </summary>
    PodLogSessionResolution? TryResolve(IClusterRuntime cluster, PodLogSessionState state);

    /// <summary>
    /// Builds the log request parameters for the resolved pod.
    /// </summary>
    PodLogReadOptions CreateReadOptions(PodLogSessionState state, PodLogSessionResolution resolution, bool follow = true);
}

/// <inheritdoc />
public sealed class PodLogSessionResolver : IPodLogSessionResolver
{
    private const int DefaultTailLines = 100;

    /// <inheritdoc />
    public PodLogSessionState CreateState(V1Pod pod, string containerName, bool previous, bool timestamps, int tailLines = DefaultTailLines)
    {
        ArgumentNullException.ThrowIfNull(pod);

        V1ObjectMeta metadata = pod.Metadata ?? new V1ObjectMeta();
        V1OwnerReference? owner = GetPreferredOwnerReference(metadata.OwnerReferences);

        return new PodLogSessionState(
            metadata.NamespaceProperty ?? string.Empty,
            metadata.Name ?? string.Empty,
            metadata.Uid,
            owner?.Uid,
            owner?.Name,
            owner?.Kind,
            containerName,
            previous,
            timestamps,
            tailLines > 0 ? tailLines : DefaultTailLines);
    }

    /// <inheritdoc />
    public PodLogSessionResolution? TryResolve(IClusterRuntime cluster, PodLogSessionState state)
    {
        ArgumentNullException.ThrowIfNull(cluster);
        ArgumentNullException.ThrowIfNull(state);

        V1Pod? currentPod = TryGetCurrentPod(cluster, state);
        List<V1Pod> relatedPods = GetRelatedPods(cluster, state);

        if (currentPod is null)
        {
            if (relatedPods.Count == 0)
            {
                return null;
            }

            currentPod = relatedPods[0];
        }
        else if (!ContainsPodWithUid(relatedPods, currentPod.Metadata?.Uid))
        {
            relatedPods.Add(currentPod);
        }

        SortPodsByNewestFirst(relatedPods);

        string resolvedContainerName = ResolveContainerName(currentPod, state.ContainerName);
        bool previousLogsAvailable = HasPreviousLogs(currentPod, resolvedContainerName);
        bool podChanged = !string.Equals(state.PodUid, currentPod.Metadata?.Uid, StringComparison.Ordinal);

        return new PodLogSessionResolution(currentPod, resolvedContainerName, relatedPods, podChanged, previousLogsAvailable);
    }

    /// <inheritdoc />
    public PodLogReadOptions CreateReadOptions(PodLogSessionState state, PodLogSessionResolution resolution, bool follow = true)
    {
        ArgumentNullException.ThrowIfNull(state);
        ArgumentNullException.ThrowIfNull(resolution);

        return new PodLogReadOptions(
            resolution.Pod.Namespace(),
            resolution.Pod.Name(),
            resolution.ContainerName,
            state.Previous && resolution.PreviousLogsAvailable,
            state.Timestamps,
            follow,
            state.TailLines > 0 ? state.TailLines : DefaultTailLines);
    }

    private static V1Pod? TryGetCurrentPod(IClusterRuntime cluster, PodLogSessionState state)
    {
        V1Pod? pod = cluster.GetResource<V1Pod>(state.PodNamespace, state.PodName);

        if (pod is not null)
        {
            return pod;
        }

        if (string.IsNullOrWhiteSpace(state.OwnerUid))
        {
            return null;
        }

        List<V1Pod> pods = GetRelatedPods(cluster, state);
        return pods.Count > 0 ? pods[0] : null;
    }

    private static List<V1Pod> GetRelatedPods(IClusterRuntime cluster, PodLogSessionState state)
    {
        List<V1Pod> relatedPods = [];

        IReadOnlyList<V1Pod> pods = cluster.GetResourceList<V1Pod>();
        for (int i = 0; i < pods.Count; i++)
        {
            V1Pod pod = pods[i];
            if (!string.Equals(pod.Namespace(), state.PodNamespace, StringComparison.Ordinal))
            {
                continue;
            }

            if (string.IsNullOrWhiteSpace(state.OwnerUid))
            {
                continue;
            }

            if (MatchesOwnerUid(pod, state.OwnerUid))
            {
                relatedPods.Add(pod);
            }
        }

        V1Pod? currentPod = cluster.GetResource<V1Pod>(state.PodNamespace, state.PodName);
        if (currentPod is not null && !ContainsPodWithUid(relatedPods, currentPod.Metadata?.Uid))
        {
            relatedPods.Add(currentPod);
        }

        if (relatedPods.Count == 0 && currentPod is not null)
        {
            relatedPods.Add(currentPod);
        }

        SortPodsByNewestFirst(relatedPods);
        return relatedPods;
    }

    private static bool MatchesOwnerUid(V1Pod pod, string ownerUid)
    {
        IList<V1OwnerReference>? ownerReferences = pod.Metadata?.OwnerReferences;
        if (ownerReferences is null || ownerReferences.Count == 0)
        {
            return false;
        }

        for (int i = 0; i < ownerReferences.Count; i++)
        {
            V1OwnerReference ownerReference = ownerReferences[i];
            if (string.Equals(ownerReference.Uid, ownerUid, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static bool ContainsPodWithUid(IEnumerable<V1Pod> pods, string? uid)
    {
        if (string.IsNullOrWhiteSpace(uid))
        {
            return false;
        }

        foreach (V1Pod pod in pods)
        {
            if (string.Equals(pod.Metadata?.Uid, uid, StringComparison.Ordinal))
            {
                return true;
            }
        }

        return false;
    }

    private static void SortPodsByNewestFirst(List<V1Pod> pods)
    {
        pods.Sort(static (left, right) =>
        {
            DateTime leftTimestamp = left.Metadata?.CreationTimestamp ?? DateTime.MinValue;
            DateTime rightTimestamp = right.Metadata?.CreationTimestamp ?? DateTime.MinValue;

            int comparison = rightTimestamp.CompareTo(leftTimestamp);
            if (comparison != 0)
            {
                return comparison;
            }

            return string.CompareOrdinal(left.Name(), right.Name());
        });
    }

    private static string ResolveContainerName(V1Pod pod, string requestedContainerName)
    {
        string? containerName = FindContainerName(pod.Spec?.Containers, requestedContainerName);
        if (!string.IsNullOrWhiteSpace(containerName))
        {
            return containerName;
        }

        containerName = FindContainerName(pod.Spec?.InitContainers, requestedContainerName);
        if (!string.IsNullOrWhiteSpace(containerName))
        {
            return containerName;
        }

        if (pod.Spec?.Containers is { Count: > 0 })
        {
            return pod.Spec.Containers[0].Name;
        }

        if (pod.Spec?.InitContainers is { Count: > 0 })
        {
            return pod.Spec.InitContainers[0].Name;
        }

        return requestedContainerName;
    }

    private static string? FindContainerName(IList<V1Container>? containers, string requestedContainerName)
    {
        if (containers is null || containers.Count == 0)
        {
            return null;
        }

        for (int i = 0; i < containers.Count; i++)
        {
            V1Container container = containers[i];
            if (string.Equals(container.Name, requestedContainerName, StringComparison.Ordinal))
            {
                return container.Name;
            }
        }

        return null;
    }

    private static bool HasPreviousLogs(V1Pod pod, string containerName)
    {
        return GetRestartCount(pod.Status?.ContainerStatuses, containerName) > 0
            || GetRestartCount(pod.Status?.InitContainerStatuses, containerName) > 0;
    }

    private static int GetRestartCount(IList<V1ContainerStatus>? containerStatuses, string containerName)
    {
        if (containerStatuses is null || containerStatuses.Count == 0)
        {
            return 0;
        }

        for (int i = 0; i < containerStatuses.Count; i++)
        {
            V1ContainerStatus containerStatus = containerStatuses[i];
            if (string.Equals(containerStatus.Name, containerName, StringComparison.Ordinal))
            {
                return containerStatus.RestartCount;
            }
        }

        return 0;
    }

    private static V1OwnerReference? GetPreferredOwnerReference(IList<V1OwnerReference>? ownerReferences)
    {
        if (ownerReferences is null || ownerReferences.Count == 0)
        {
            return null;
        }

        for (int i = 0; i < ownerReferences.Count; i++)
        {
            V1OwnerReference ownerReference = ownerReferences[i];
            if (ownerReference.Controller == true)
            {
                return ownerReference;
            }
        }

        return ownerReferences[0];
    }
}
