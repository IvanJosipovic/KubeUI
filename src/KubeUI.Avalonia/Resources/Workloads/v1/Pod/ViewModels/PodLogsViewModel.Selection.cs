using Avalonia.Threading;
using k8s.Models;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Pod.ViewModels;

public sealed partial class PodLogsViewModel
{
    private List<PodLogReadOptions> BuildReadTargets(PodLogSessionState state)
    {
        List<PodLogReadOptions> targets = [];
        HashSet<string> seenTargets = new(StringComparer.Ordinal);
        foreach (PodLogSourceTreeNode resourceNode in SourceTreeItems)
        {
            foreach (PodLogSourceTreeNode podNode in resourceNode.Children)
            {
                if (podNode.Value is not V1Pod pod)
                {
                    continue;
                }

                foreach (PodLogSourceTreeNode containerNode in podNode.Children)
                {
                    if (containerNode.IsChecked != true
                        || containerNode.Value is not PodLogContainerOption container
                        || !IsContainerReadyForLogs(pod, container.Name, state.Previous))
                    {
                        continue;
                    }

                    var targetKey = $"{BuildPodSourceKey(pod)}\n{BuildContainerSourceKey(container)}";
                    if (seenTargets.Add(targetKey))
                    {
                        targets.Add(CreateReadOptionsForPod(state, pod, container.Name));
                    }
                }
            }
        }

        return targets;
    }

    private PodLogReadOptions CreateReadOptionsForPod(PodLogSessionState state, V1Pod pod, string containerName)
    {
        var previousLogsAvailable = HasPreviousLogs(pod, containerName);
        return new PodLogReadOptions(
            pod.Namespace(),
            pod.Name(),
            containerName,
            state.Previous && previousLogsAvailable,
            state.Timestamps,
            true,
            state.TailLines);
    }

    private static IReadOnlyList<PodLogContainerOption> BuildContainerOptions(V1Pod pod)
    {
        List<PodLogContainerOption> containers = [];

        var initContainers = pod.Spec?.InitContainers;
        if (initContainers is not null)
        {
            for (var i = 0; i < initContainers.Count; i++)
            {
                var container = initContainers[i];
                containers.Add(new PodLogContainerOption(container.Name, $"{container.Name} (init)", IsInitContainer: true));
            }
        }

        var appContainers = pod.Spec?.Containers;
        if (appContainers is not null)
        {
            for (var i = 0; i < appContainers.Count; i++)
            {
                var container = appContainers[i];
                containers.Add(new PodLogContainerOption(container.Name, container.Name, IsInitContainer: false));
            }
        }

        var ephemeralContainers = pod.Spec?.EphemeralContainers;
        if (ephemeralContainers is not null)
        {
            for (var i = 0; i < ephemeralContainers.Count; i++)
            {
                var container = ephemeralContainers[i];
                containers.Add(new PodLogContainerOption(container.Name, $"{container.Name} (ephemeral)", IsInitContainer: false, IsEphemeralContainer: true));
            }
        }

        return containers;
    }

    private static IReadOnlyList<PodLogContainerOption> BuildContainerOptions(IReadOnlyList<V1Pod> pods)
    {
        List<PodLogContainerOption> containers = [];
        HashSet<PodLogContainerSelectionKey> seen = [];

        for (var i = 0; i < pods.Count; i++)
        {
            var podContainers = BuildContainerOptions(pods[i]);
            for (var j = 0; j < podContainers.Count; j++)
            {
                var container = podContainers[j];
                var key = GetContainerSelectionKey(container);
                if (seen.Add(key))
                {
                    containers.Add(container);
                }
            }
        }

        return containers;
    }

    private void ReconcileSourceTree()
    {
        if (MultiSessionResolution is null)
        {
            SourceTreeItems.Clear();
            return;
        }

        SourceTreeSelectionSnapshot selection = CaptureSourceTreeSelection();
        HashSet<string> assignedPods = new(StringComparer.Ordinal);
        HashSet<string> desiredResourceKeys = new(StringComparer.Ordinal);
        for (var scopeIndex = 0; scopeIndex < _scopeItems.Count; scopeIndex++)
        {
            PodLogScopeSelectionItem scopeItem = _scopeItems[scopeIndex];
            var resourceKey = BuildScopeIdentity(scopeItem.Resource, scopeItem.ResourceKind);
            desiredResourceKeys.Add(resourceKey);
            PodLogSourceTreeNode? resourceNode = FindSourceNode(SourceTreeItems, resourceKey);
            var resourceIsNew = resourceNode is null;
            resourceNode ??= new PodLogSourceTreeNode(
                PodLogSourceNodeKind.Resource,
                resourceKey,
                BuildSourceResourceDisplayName(scopeItem),
                scopeItem,
                true,
                SourceTreeSelectionChanged);
            resourceNode.Update(BuildSourceResourceDisplayName(scopeItem), scopeItem);
            if (resourceIsNew)
            {
                SourceTreeItems.Add(resourceNode);
            }

            PodLogScopeResolution? scopeResolution = FindScopeResolution(scopeItem);
            var selectResolvedResource = _resourceKeysToSelectOnResolve.Contains(resourceKey);
            ReconcilePodNodes(
                resourceNode,
                scopeResolution?.Pods ?? [],
                assignedPods,
                selectResolvedResource,
                selection);
            if (selectResolvedResource && resourceNode.Children.Count > 0)
            {
                _resourceKeysToSelectOnResolve.Remove(resourceKey);
            }
        }

        RemoveMissingNodes(SourceTreeItems, desiredResourceKeys);
        UpdateSourceTreeParentStates();
    }

    private void ReconcilePodNodes(
        PodLogSourceTreeNode resourceNode,
        IReadOnlyList<V1Pod> pods,
        HashSet<string> assignedPods,
        bool selectAllNewNodes,
        SourceTreeSelectionSnapshot selection)
    {
        HashSet<string> desiredPodKeys = new(StringComparer.Ordinal);
        for (var podIndex = 0; podIndex < pods.Count; podIndex++)
        {
            V1Pod pod = pods[podIndex];
            var podKey = BuildPodSourceKey(pod);
            if (!assignedPods.Add(podKey))
            {
                continue;
            }

            desiredPodKeys.Add(podKey);
            PodLogSourceTreeNode? podNode = FindSourceNode(resourceNode.Children, podKey);
            var podIsNew = podNode is null;
            podNode ??= new PodLogSourceTreeNode(
                PodLogSourceNodeKind.Pod,
                podKey,
                pod.Name(),
                pod,
                false,
                SourceTreeSelectionChanged);
            podNode.Update(pod.Name(), pod);
            if (podIsNew)
            {
                resourceNode.Children.Add(podNode);
            }

            ReconcileContainerNodes(
                podNode,
                BuildContainerOptions(pod),
                podIsNew,
                selectAllNewNodes,
                selection);
        }

        RemoveMissingNodes(resourceNode.Children, desiredPodKeys);
    }

    private void ReconcileContainerNodes(
        PodLogSourceTreeNode podNode,
        IReadOnlyList<PodLogContainerOption> containers,
        bool podIsNew,
        bool selectAllNewNodes,
        SourceTreeSelectionSnapshot selection)
    {
        HashSet<string> desiredContainerKeys = new(StringComparer.Ordinal);
        var hadContainers = podNode.Children.Count > 0;
        var allExistingContainersSelected = AreAllChildrenSelected(podNode.Children);
        var initiallyRequestedContainer = SessionState?.ContainerName ?? ContainerName;
        for (var containerIndex = 0; containerIndex < containers.Count; containerIndex++)
        {
            PodLogContainerOption container = containers[containerIndex];
            var containerKey = BuildContainerSourceKey(container);
            desiredContainerKeys.Add(containerKey);
            PodLogSourceTreeNode? containerNode = FindSourceNode(podNode.Children, containerKey);
            var containerIsNew = containerNode is null;
            var selectNewContainer = selectAllNewNodes
                || podIsNew && selection.HasPods
                    && selection.SelectNewPods
                    && (selection.AllContainersSelected || selection.CommonContainerKeys.Contains(containerKey))
                || podIsNew && !selection.HasPods
                    && (string.IsNullOrWhiteSpace(initiallyRequestedContainer)
                        || string.Equals(container.Name, initiallyRequestedContainer, StringComparison.Ordinal))
                || !podIsNew && hadContainers && allExistingContainersSelected;
            containerNode ??= new PodLogSourceTreeNode(
                PodLogSourceNodeKind.Container,
                containerKey,
                container.DisplayName,
                container,
                selectNewContainer,
                SourceTreeSelectionChanged);
            containerNode.Update(container.DisplayName, container);
            if (containerIsNew)
            {
                podNode.Children.Add(containerNode);
            }
        }

        RemoveMissingNodes(podNode.Children, desiredContainerKeys);
    }

    private SourceTreeSelectionSnapshot CaptureSourceTreeSelection()
    {
        var hasPods = false;
        var selectNewPods = true;
        var allContainersSelected = true;
        HashSet<string>? commonContainerKeys = null;
        foreach (PodLogSourceTreeNode resourceNode in SourceTreeItems)
        {
            foreach (PodLogSourceTreeNode podNode in resourceNode.Children)
            {
                hasPods = true;
                var selectedContainerKeys = podNode.Children
                    .Where(static node => node.IsChecked == true)
                    .Select(static node => node.Key)
                    .ToHashSet(StringComparer.Ordinal);
                if (selectedContainerKeys.Count == 0)
                {
                    selectNewPods = false;
                }

                if (podNode.Children.Count == 0 || selectedContainerKeys.Count != podNode.Children.Count)
                {
                    allContainersSelected = false;
                }

                if (commonContainerKeys is null)
                {
                    commonContainerKeys = selectedContainerKeys;
                }
                else
                {
                    commonContainerKeys.IntersectWith(selectedContainerKeys);
                }
            }
        }

        return new SourceTreeSelectionSnapshot(
            hasPods,
            selectNewPods,
            allContainersSelected,
            commonContainerKeys ?? []);
    }

    private sealed record SourceTreeSelectionSnapshot(
        bool HasPods,
        bool SelectNewPods,
        bool AllContainersSelected,
        IReadOnlySet<string> CommonContainerKeys);

    private static PodLogSourceTreeNode? FindSourceNode(
        IEnumerable<PodLogSourceTreeNode> nodes,
        string key)
    {
        foreach (PodLogSourceTreeNode node in nodes)
        {
            if (string.Equals(node.Key, key, StringComparison.Ordinal))
            {
                return node;
            }
        }

        return null;
    }

    private static void RemoveMissingNodes(
        IList<PodLogSourceTreeNode> nodes,
        IReadOnlySet<string> desiredKeys)
    {
        for (var i = nodes.Count - 1; i >= 0; i--)
        {
            if (!desiredKeys.Contains(nodes[i].Key))
            {
                nodes.RemoveAt(i);
            }
        }
    }

    private static bool AreAllChildrenSelected(IReadOnlyCollection<PodLogSourceTreeNode> nodes)
    {
        return nodes.Count == 0 || nodes.All(static node => node.IsChecked == true);
    }

    private static string BuildPodSourceKey(V1Pod pod)
    {
        return $"{pod.Namespace()}\n{pod.Metadata?.Uid ?? pod.Name()}";
    }

    private static string BuildContainerSourceKey(PodLogContainerOption container)
    {
        return $"{container.Name}\n{container.IsInitContainer}\n{container.IsEphemeralContainer}";
    }

    private static string BuildSourceResourceDisplayName(PodLogScopeSelectionItem scopeItem)
    {
        if (string.IsNullOrWhiteSpace(scopeItem.ResolutionStatus))
        {
            return scopeItem.DisplayName;
        }

        return $"{scopeItem.DisplayName} ({scopeItem.ResolutionStatus}, {scopeItem.ResolvedPodCount} Pods)";
    }

    private PodLogScopeResolution? FindScopeResolution(PodLogScopeSelectionItem scopeItem)
    {
        if (MultiSessionResolution is null)
        {
            return null;
        }

        for (var i = 0; i < MultiSessionResolution.Scopes.Count; i++)
        {
            PodLogScopeResolution resolution = MultiSessionResolution.Scopes[i];
            if (!string.IsNullOrWhiteSpace(resolution.Scope.ResourceUid)
                && string.Equals(resolution.Scope.ResourceUid, scopeItem.Resource.Metadata?.Uid, StringComparison.Ordinal)
                && string.Equals(resolution.Scope.ResourceKind, scopeItem.ResourceKind, StringComparison.Ordinal))
            {
                return resolution;
            }

            if (string.Equals(resolution.Scope.ResourceName, scopeItem.Resource.Name(), StringComparison.Ordinal)
                && string.Equals(resolution.Scope.ResourceNamespace, scopeItem.Resource.Namespace(), StringComparison.Ordinal)
                && string.Equals(resolution.Scope.ResourceKind, scopeItem.ResourceKind, StringComparison.Ordinal))
            {
                return resolution;
            }
        }

        return null;
    }

    private static bool? GetAggregateSelection(IReadOnlyCollection<PodLogSourceTreeNode> children, bool emptyValue)
    {
        if (children.Count == 0)
        {
            return emptyValue;
        }

        bool? aggregate = null;
        foreach (PodLogSourceTreeNode child in children)
        {
            if (!child.IsChecked.HasValue)
            {
                return null;
            }

            aggregate ??= child.IsChecked;
            if (aggregate != child.IsChecked)
            {
                return null;
            }
        }

        return aggregate;
    }

    private void SourceTreeSelectionChanged(PodLogSourceTreeNode node, bool isSelected)
    {
        switch (node.Kind)
        {
            case PodLogSourceNodeKind.Resource:
                if (!isSelected)
                {
                    SelectedScopeItems.Remove((PodLogScopeSelectionItem)node.Value);
                    return;
                }

                SetDescendantsSelected(node, true);
                break;
            case PodLogSourceNodeKind.Pod:
                SetDescendantsSelected(node, isSelected);
                break;
        }

        UpdateSourceTreeParentStates();
        UpdateResourceNameToggleState();
        QueueSelectionReconnect();
    }

    private static void SetDescendantsSelected(PodLogSourceTreeNode node, bool isSelected)
    {
        foreach (PodLogSourceTreeNode child in node.Children)
        {
            child.UpdateIsChecked(isSelected);
            SetDescendantsSelected(child, isSelected);
        }
    }

    private void UpdateSourceTreeParentStates()
    {
        foreach (PodLogSourceTreeNode resourceNode in SourceTreeItems)
        {
            foreach (PodLogSourceTreeNode podNode in resourceNode.Children)
            {
                podNode.UpdateIsChecked(GetAggregateSelection(podNode.Children, false));
            }

            resourceNode.UpdateIsChecked(GetAggregateSelection(resourceNode.Children, true));
        }
    }

    private (int PodCount, int TargetCount) GetSelectedSourceCounts()
    {
        HashSet<string> selectedPods = new(StringComparer.Ordinal);
        HashSet<string> selectedTargets = new(StringComparer.Ordinal);
        foreach (PodLogSourceTreeNode resourceNode in SourceTreeItems)
        {
            foreach (PodLogSourceTreeNode podNode in resourceNode.Children)
            {
                foreach (PodLogSourceTreeNode containerNode in podNode.Children)
                {
                    if (containerNode.IsChecked == true)
                    {
                        selectedPods.Add(podNode.Key);
                        selectedTargets.Add($"{podNode.Key}\n{containerNode.Key}");
                    }
                }
            }
        }

        return (selectedPods.Count, selectedTargets.Count);
    }

    private static bool HasPreviousLogs(V1Pod pod, string containerName)
    {
        return GetRestartCount(pod.Status?.ContainerStatuses, containerName) > 0
            || GetRestartCount(pod.Status?.InitContainerStatuses, containerName) > 0
            || GetRestartCount(pod.Status?.EphemeralContainerStatuses, containerName) > 0;
    }

    private static bool IsContainerReadyForLogs(V1Pod pod, string containerName, bool previous)
    {
        if (previous && HasPreviousLogs(pod, containerName))
        {
            return true;
        }

        V1ContainerStatus? status = FindContainerStatus(pod.Status?.ContainerStatuses, containerName)
            ?? FindContainerStatus(pod.Status?.InitContainerStatuses, containerName)
            ?? FindContainerStatus(pod.Status?.EphemeralContainerStatuses, containerName);

        return status is null || status.State?.Waiting is null;
    }

    private static V1ContainerStatus? FindContainerStatus(IList<V1ContainerStatus>? statuses, string containerName)
    {
        if (statuses is null)
        {
            return null;
        }

        for (var i = 0; i < statuses.Count; i++)
        {
            if (string.Equals(statuses[i].Name, containerName, StringComparison.Ordinal))
            {
                return statuses[i];
            }
        }

        return null;
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

    private static PodLogContainerSelectionKey GetContainerSelectionKey(PodLogContainerOption item)
    {
        return new PodLogContainerSelectionKey(item.Name, item.IsInitContainer, item.IsEphemeralContainer);
    }

    private void UpdateResourceNameToggleState()
    {
        var displayMode = GetCurrentDisplayMode();
        OnPropertyChanged(nameof(CanShowResourceNames));
        if (displayMode != PodLogDisplayMode.None
            && displayMode != _resourceNameDisplayMode
            && !ShowResourceNames)
        {
            _resourceNameDisplayMode = displayMode;
            ShowResourceNames = true;
            return;
        }

        _resourceNameDisplayMode = displayMode;
        if (ShowResourceNames)
        {
            RenderOutputEntries();
        }
    }

    private void QueueSelectionReconnect()
    {
        ConnectionError = null;
        if (!_hasLoadedSession)
        {
            return;
        }

        if (IsConnecting)
        {
            _pendingReconnect = true;
            return;
        }

        if (_pendingSelectionReconnect)
        {
            return;
        }

        _pendingSelectionReconnect = true;
        Dispatcher.UIThread.Post(
            () =>
            {
                _pendingSelectionReconnect = false;
                if (!_disposed)
                {
                    RequestReconnect();
                }
            },
            DispatcherPriority.Background);
    }

}
