using k8s;
using k8s.Models;

namespace KubeUI.Kubernetes;

/// <summary>
/// Captures the source resource identity and log preferences that a pod logs view needs to restore or retarget a session.
/// </summary>
public sealed record PodLogSessionState(
    string ResourceNamespace,
    string ResourceName,
    string? ResourceUid,
    string ResourceKind,
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
    /// Captures the current pod or workload identity and log preferences into a reusable session state.
    /// </summary>
    PodLogSessionState CreateState(IKubernetesObject<V1ObjectMeta> resource, string containerName, bool previous, bool timestamps, int tailLines = 100);

    /// <summary>
    /// Resolves the session state against the current cluster contents.
    /// </summary>
    PodLogSessionResolution? TryResolve(IClusterRuntime cluster, PodLogSessionState state);

}

/// <inheritdoc />
public sealed class PodLogSessionResolver : IPodLogSessionResolver
{
    private const int DefaultTailLines = 100;

    /// <inheritdoc />
    public PodLogSessionState CreateState(IKubernetesObject<V1ObjectMeta> resource, string containerName, bool previous, bool timestamps, int tailLines = DefaultTailLines)
    {
        ArgumentNullException.ThrowIfNull(resource);

        var metadata = resource.Metadata ?? new V1ObjectMeta();
        var owner = resource is V1Pod
            ? GetPreferredOwnerReference(metadata.OwnerReferences)
            : null;

        return new PodLogSessionState(
            metadata.NamespaceProperty ?? string.Empty,
            metadata.Name ?? string.Empty,
            metadata.Uid,
            GetResourceKind(resource),
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

        var resources = GetResources(cluster);
        var indexes = BuildIndexes(resources);
        var pods = GetPods(resources);
        var currentPod = TryGetCurrentPod(pods, state);
        var relatedPods = state.ResourceKind == V1Pod.KubeKind
            ? GetRelatedPods(pods, state, currentPod)
            : GetDescendantPods(pods, state, indexes);

        if (currentPod is null)
        {
            if (relatedPods.Count == 0)
            {
                return null;
            }

            currentPod = relatedPods[0];
        }
        else if (!ContainsPod(relatedPods, currentPod))
        {
            relatedPods.Add(currentPod);
        }

        SortPodsByNewestFirst(relatedPods);

        var resolvedContainerName = ResolveContainerName(currentPod, state.ContainerName);
        var previousLogsAvailable = HasPreviousLogs(currentPod, resolvedContainerName);
        var podChanged = !string.Equals(state.ResourceUid, currentPod.Metadata?.Uid, StringComparison.Ordinal);

        return new PodLogSessionResolution(currentPod, resolvedContainerName, relatedPods, podChanged, previousLogsAvailable);
    }

    private static V1Pod? TryGetCurrentPod(IReadOnlyList<V1Pod> pods, PodLogSessionState state)
    {
        for (var i = 0; i < pods.Count; i++)
        {
            var pod = pods[i];
            if (string.Equals(pod.Namespace(), state.ResourceNamespace, StringComparison.Ordinal)
                && string.Equals(pod.Name(), state.ResourceName, StringComparison.Ordinal))
            {
                return pod;
            }
        }

        return null;
    }

    private static List<V1Pod> GetRelatedPods(IReadOnlyList<V1Pod> pods, PodLogSessionState state, V1Pod? currentPod)
    {
        List<V1Pod> relatedPods = [];

        for (var i = 0; i < pods.Count; i++)
        {
            var pod = pods[i];
            if (!string.Equals(pod.Namespace(), state.ResourceNamespace, StringComparison.Ordinal))
            {
                continue;
            }

            if (MatchesOwner(pod, state.OwnerUid, state.OwnerName, state.OwnerKind))
            {
                relatedPods.Add(pod);
            }
        }

        if (currentPod is not null && !ContainsPod(relatedPods, currentPod))
        {
            relatedPods.Add(currentPod);
        }

        SortPodsByNewestFirst(relatedPods);
        return relatedPods;
    }

    private static List<V1Pod> GetDescendantPods(
        IReadOnlyList<V1Pod> pods,
        PodLogSessionState state,
        ResourceIndexes indexes)
    {
        ResourceIdentity target = new(
            state.ResourceNamespace,
            state.ResourceName,
            state.ResourceUid,
            state.ResourceKind);
        List<V1Pod> relatedPods = [];

        for (var i = 0; i < pods.Count; i++)
        {
            var pod = pods[i];
            if (string.Equals(pod.Namespace(), state.ResourceNamespace, StringComparison.Ordinal)
                && IsDescendantOf(pod, target, indexes))
            {
                relatedPods.Add(pod);
            }
        }

        SortPodsByNewestFirst(relatedPods);
        return relatedPods;
    }

    private static bool IsDescendantOf(V1Pod pod, ResourceIdentity target, ResourceIndexes indexes)
    {
        var ownerReferences = pod.Metadata?.OwnerReferences;
        if (ownerReferences is null || ownerReferences.Count == 0)
        {
            return false;
        }

        List<V1OwnerReference> pending = [.. ownerReferences];
        HashSet<string> visited = new(StringComparer.Ordinal);

        for (var i = 0; i < pending.Count; i++)
        {
            var ownerReference = pending[i];
            if (MatchesOwner(ownerReference, target))
            {
                return true;
            }

            var owner = FindResource(indexes, pod.Namespace(), ownerReference);
            if (owner is null)
            {
                continue;
            }

            var ownerKey = GetResourceKey(owner.Resource);
            if (!visited.Add(ownerKey))
            {
                continue;
            }

            var ownerOwners = owner.Resource.Metadata?.OwnerReferences;
            if (ownerOwners is not null)
            {
                for (var j = 0; j < ownerOwners.Count; j++)
                {
                    pending.Add(ownerOwners[j]);
                }
            }
        }

        return false;
    }

    private static bool MatchesOwner(V1Pod pod, string? ownerUid, string? ownerName, string? ownerKind)
    {
        var ownerReferences = pod.Metadata?.OwnerReferences;
        if (ownerReferences is null)
        {
            return false;
        }

        for (var i = 0; i < ownerReferences.Count; i++)
        {
            var ownerReference = ownerReferences[i];
            if ((!string.IsNullOrWhiteSpace(ownerUid) && string.Equals(ownerReference.Uid, ownerUid, StringComparison.Ordinal))
                || (string.IsNullOrWhiteSpace(ownerUid)
                    && string.Equals(ownerReference.Name, ownerName, StringComparison.Ordinal)
                    && (string.IsNullOrWhiteSpace(ownerKind)
                        || string.Equals(ownerReference.Kind, ownerKind, StringComparison.Ordinal))))
            {
                return true;
            }
        }

        return false;
    }

    private static bool MatchesOwner(V1OwnerReference ownerReference, ResourceIdentity target)
    {
        if (!string.IsNullOrWhiteSpace(target.Uid))
        {
            return string.Equals(ownerReference.Uid, target.Uid, StringComparison.Ordinal);
        }

        return string.Equals(ownerReference.Name, target.Name, StringComparison.Ordinal)
            && (string.IsNullOrWhiteSpace(target.Kind)
                || string.Equals(ownerReference.Kind, target.Kind, StringComparison.Ordinal));
    }

    private static ResourceEntry? FindResource(
        ResourceIndexes indexes,
        string? resourceNamespace,
        V1OwnerReference ownerReference)
    {
        if (!string.IsNullOrWhiteSpace(ownerReference.Uid))
        {
            indexes.ByUid.TryGetValue(ownerReference.Uid, out var resourceByUid);
            if (resourceByUid is not null)
            {
                return resourceByUid;
            }
        }

        indexes.ByName.TryGetValue(
            (resourceNamespace ?? string.Empty, ownerReference.Kind ?? string.Empty, ownerReference.Name ?? string.Empty),
            out var resourceByName);
        return resourceByName;
    }

    private static List<ResourceEntry> GetResources(IClusterRuntime cluster)
    {
        List<ResourceEntry> resources = [];

        foreach (var pair in cluster.Objects)
        {
            if (pair.Value is not IResourceContainer container)
            {
                continue;
            }

            var kind = pair.Key.Kind;
            foreach (var resource in container.Snapshot())
            {
                resources.Add(new ResourceEntry(resource, GetResourceKind(resource, kind)));
            }
        }

        return resources;
    }

    private static List<V1Pod> GetPods(IReadOnlyList<ResourceEntry> resources)
    {
        List<V1Pod> pods = [];
        for (var i = 0; i < resources.Count; i++)
        {
            if (resources[i].Resource is V1Pod pod)
            {
                pods.Add(pod);
            }
        }

        return pods;
    }

    private static string GetResourceKind(IKubernetesObject<V1ObjectMeta> resource, string? fallbackKind = null)
    {
        return resource switch
        {
            V1Pod => V1Pod.KubeKind,
            V1Deployment => V1Deployment.KubeKind,
            V1ReplicaSet => V1ReplicaSet.KubeKind,
            V1DaemonSet => V1DaemonSet.KubeKind,
            V1StatefulSet => V1StatefulSet.KubeKind,
            V1Job => V1Job.KubeKind,
            V1CronJob => V1CronJob.KubeKind,
            _ => resource.Kind ?? fallbackKind ?? string.Empty,
        };
    }

    private static string GetResourceKey(IKubernetesObject<V1ObjectMeta> resource)
    {
        return resource.Metadata?.Uid
            ?? $"{resource.Namespace()}\u0000{resource.Kind}\u0000{resource.Name()}";
    }

    private sealed record ResourceEntry(IKubernetesObject<V1ObjectMeta> Resource, string Kind);

    private sealed record ResourceIndexes(
        IReadOnlyDictionary<string, ResourceEntry> ByUid,
        IReadOnlyDictionary<(string Namespace, string Kind, string Name), ResourceEntry> ByName);

    private static ResourceIndexes BuildIndexes(IReadOnlyList<ResourceEntry> resources)
    {
        Dictionary<string, ResourceEntry> byUid = new(StringComparer.Ordinal);
        Dictionary<(string Namespace, string Kind, string Name), ResourceEntry> byName = [];
        for (var i = 0; i < resources.Count; i++)
        {
            var resource = resources[i];
            var uid = resource.Resource.Metadata?.Uid;
            if (!string.IsNullOrWhiteSpace(uid))
            {
                byUid.TryAdd(uid, resource);
            }

            byName.TryAdd((resource.Resource.Namespace() ?? string.Empty, resource.Kind, resource.Resource.Name() ?? string.Empty), resource);
        }

        return new ResourceIndexes(byUid, byName);
    }

    private sealed record ResourceIdentity(string Namespace, string Name, string? Uid, string Kind);

    private static bool ContainsPod(IEnumerable<V1Pod> pods, V1Pod candidate)
    {
        var uid = candidate.Metadata?.Uid;
        foreach (var pod in pods)
        {
            if (!string.IsNullOrWhiteSpace(uid)
                ? string.Equals(pod.Metadata?.Uid, uid, StringComparison.Ordinal)
                : ReferenceEquals(pod, candidate)
                    || (string.Equals(pod.Namespace(), candidate.Namespace(), StringComparison.Ordinal)
                        && string.Equals(pod.Name(), candidate.Name(), StringComparison.Ordinal)))
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
            var leftTimestamp = left.Metadata?.CreationTimestamp ?? DateTime.MinValue;
            var rightTimestamp = right.Metadata?.CreationTimestamp ?? DateTime.MinValue;

            var comparison = rightTimestamp.CompareTo(leftTimestamp);
            if (comparison != 0)
            {
                return comparison;
            }

            return string.CompareOrdinal(left.Name(), right.Name());
        });
    }

    private static string ResolveContainerName(V1Pod pod, string requestedContainerName)
    {
        var containerName = FindContainerName(pod.Spec?.Containers, requestedContainerName);
        if (!string.IsNullOrWhiteSpace(containerName))
        {
            return containerName;
        }

        containerName = FindContainerName(pod.Spec?.InitContainers, requestedContainerName);
        if (!string.IsNullOrWhiteSpace(containerName))
        {
            return containerName;
        }

        containerName = FindEphemeralContainerName(pod.Spec?.EphemeralContainers, requestedContainerName);
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

        if (pod.Spec?.EphemeralContainers is { Count: > 0 })
        {
            return pod.Spec.EphemeralContainers[0].Name;
        }

        return requestedContainerName;
    }

    private static string? FindContainerName(IList<V1Container>? containers, string requestedContainerName)
    {
        if (containers is null || containers.Count == 0)
        {
            return null;
        }

        for (var i = 0; i < containers.Count; i++)
        {
            var container = containers[i];
            if (string.Equals(container.Name, requestedContainerName, StringComparison.Ordinal))
            {
                return container.Name;
            }
        }

        return null;
    }

    private static string? FindEphemeralContainerName(IList<V1EphemeralContainer>? containers, string requestedContainerName)
    {
        if (containers is null || containers.Count == 0)
        {
            return null;
        }

        for (var i = 0; i < containers.Count; i++)
        {
            var container = containers[i];
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
            || GetRestartCount(pod.Status?.InitContainerStatuses, containerName) > 0
            || GetRestartCount(pod.Status?.EphemeralContainerStatuses, containerName) > 0;
    }

    private static int GetRestartCount(IList<V1ContainerStatus>? containerStatuses, string containerName)
    {
        if (containerStatuses is null || containerStatuses.Count == 0)
        {
            return 0;
        }

        for (var i = 0; i < containerStatuses.Count; i++)
        {
            var containerStatus = containerStatuses[i];
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

        for (var i = 0; i < ownerReferences.Count; i++)
        {
            var ownerReference = ownerReferences[i];
            if (ownerReference.Controller == true)
            {
                return ownerReference;
            }
        }

        return ownerReferences[0];
    }
}
