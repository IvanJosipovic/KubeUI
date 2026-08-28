using System.Collections.ObjectModel;
using System.Collections.Specialized;
using Avalonia.Threading;
using k8s.Models;
using KubeUI.Kubernetes;

namespace KubeUI.Avalonia.Resources.Workloads.v1.Pod.ViewModels;

public sealed partial class PodLogsViewModel
{
    private List<PodLogReadOptions> BuildReadTargets(PodLogSessionState state, PodLogSessionResolution resolution)
    {
        List<PodLogReadOptions> targets = [];
        var pods = ResolveSelectedPods(resolution.RelatedPods, resolution.Pod);

        for (var i = 0; i < pods.Count; i++)
        {
            var pod = pods[i];
            var containers = ResolveSelectedContainers(pod);

            for (var j = 0; j < containers.Count; j++)
            {
                var container = containers[j];
                if (!IsContainerReadyForLogs(pod, container.Name, state.Previous))
                {
                    continue;
                }

                targets.Add(CreateReadOptionsForPod(state, pod, container.Name));
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

    private static IReadOnlyList<PodLogPodSelectionItem> BuildPodSelectionItems(IReadOnlyList<V1Pod> pods)
    {
        List<PodLogPodSelectionItem> items = [new PodLogPodSelectionItem(null, Assets.Resources.PodLogsView_AllPods, true)];

        for (var i = 0; i < pods.Count; i++)
        {
            var pod = pods[i];
            items.Add(new PodLogPodSelectionItem(pod, pod.Name(), false));
        }

        return items;
    }

    private static IReadOnlyList<PodLogContainerSelectionItem> BuildContainerSelectionItems(IReadOnlyList<PodLogContainerOption> containers)
    {
        List<PodLogContainerSelectionItem> items = [new PodLogContainerSelectionItem(string.Empty, Assets.Resources.PodLogsView_AllContainers, false, true)];

        for (var i = 0; i < containers.Count; i++)
        {
            var container = containers[i];
            items.Add(new PodLogContainerSelectionItem(container.Name, container.DisplayName, container.IsInitContainer, false, container.IsEphemeralContainer));
        }

        return items;
    }

    private ObservableCollection<PodLogPodSelectionItem> BuildSelectedPodItems(V1Pod currentPod, IReadOnlyList<PodLogPodSelectionItem> selectionItems)
    {
        return BuildSelectedItems(
            SelectedPodItems,
            selectionItems,
            item => item.IsAll,
            item => item.Pod?.Name() ?? string.Empty,
            key => FindPodSelectionItem(selectionItems, key),
            () => FindPodSelectionItem(selectionItems, currentPod.Name()));
    }

    private ObservableCollection<PodLogContainerSelectionItem> BuildSelectedContainerItems(string selectedContainerName, IReadOnlyList<PodLogContainerSelectionItem> selectionItems)
    {
        return BuildSelectedItems(
            SelectedContainerItems,
            selectionItems,
            item => item.IsAll,
            GetContainerSelectionKey,
            key => FindContainerSelectionItem(selectionItems, key.Name, key.IsInitContainer, key.IsEphemeralContainer),
            () => FindContainerSelectionItem(selectionItems, selectedContainerName));
    }

    private static ObservableCollection<TItem> BuildSelectedItems<TItem, TKey>(
        IReadOnlyCollection<TItem> currentSelection,
        IReadOnlyList<TItem> selectionItems,
        Func<TItem, bool> isAll,
        Func<TItem, TKey> getSelectionKey,
        Func<TKey, TItem?> findSelectionItem,
        Func<TItem?> findFallbackItem)
        where TItem : class
    {
        if (currentSelection.Count == 0)
        {
            return CreateSelection(findFallbackItem() ?? selectionItems[0]);
        }

        if (ContainsAllSelection(currentSelection, isAll))
        {
            return CreateSelection(selectionItems[0]);
        }

        List<TItem> selected = [];
        foreach (var selectedItem in currentSelection)
        {
            var item = findSelectionItem(getSelectionKey(selectedItem));
            if (item is not null)
            {
                selected.Add(item);
            }
        }

        return selected.Count > 0
            ? new ObservableCollection<TItem>(selected)
            : CreateSelection(findFallbackItem() ?? selectionItems[0]);
    }

    private IReadOnlyList<V1Pod> ResolveSelectedPods(IReadOnlyList<V1Pod> availablePods, V1Pod fallbackPod)
    {
        if (SelectedPodItems.Count == 0 || ContainsAllSelection(SelectedPodItems))
        {
            return availablePods;
        }

        HashSet<string> selectedNames = new(StringComparer.Ordinal);
        for (var i = 0; i < SelectedPodItems.Count; i++)
        {
            var item = SelectedPodItems[i];
            if (!item.IsAll && item.Pod is not null)
            {
                selectedNames.Add(item.Pod.Name());
            }
        }

        List<V1Pod> selectedPods = [];
        for (var i = 0; i < availablePods.Count; i++)
        {
            var pod = availablePods[i];
            if (selectedNames.Contains(pod.Name()))
            {
                selectedPods.Add(pod);
            }
        }

        return selectedPods.Count > 0 ? selectedPods : [fallbackPod];
    }

    private IReadOnlyList<PodLogContainerOption> ResolveSelectedContainers(V1Pod pod)
    {
        List<PodLogContainerOption> availableContainers = [.. BuildContainerOptions(pod)];
        if (SelectedContainerItems.Count == 0 || ContainsAllSelection(SelectedContainerItems))
        {
            return availableContainers;
        }

        HashSet<PodLogContainerSelectionKey> selectedKeys = [];
        for (var i = 0; i < SelectedContainerItems.Count; i++)
        {
            var item = SelectedContainerItems[i];
            if (!item.IsAll)
            {
                selectedKeys.Add(GetContainerSelectionKey(item));
            }
        }

        List<PodLogContainerOption> selectedContainers = [];
        for (var i = 0; i < availableContainers.Count; i++)
        {
            var container = availableContainers[i];
            if (selectedKeys.Contains(GetContainerSelectionKey(container)))
            {
                selectedContainers.Add(container);
            }
        }

        return selectedContainers;
    }

    private static PodLogPodSelectionItem? FindPodSelectionItem(IReadOnlyList<PodLogPodSelectionItem> items, string podName)
    {
        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];
            if (!item.IsAll && item.Pod is not null && string.Equals(item.Pod.Name(), podName, StringComparison.Ordinal))
            {
                return item;
            }
        }

        return null;
    }

    private static PodLogContainerSelectionItem? FindContainerSelectionItem(
        IReadOnlyList<PodLogContainerSelectionItem> items,
        string containerName,
        bool? isInitContainer = null,
        bool? isEphemeralContainer = null)
    {
        for (var i = 0; i < items.Count; i++)
        {
            var item = items[i];
            if (item.IsAll)
            {
                continue;
            }

            if (!string.Equals(item.Name, containerName, StringComparison.Ordinal))
            {
                continue;
            }

            if (isInitContainer.HasValue && item.IsInitContainer != isInitContainer.Value)
            {
                continue;
            }

            if (isEphemeralContainer.HasValue && item.IsEphemeralContainer != isEphemeralContainer.Value)
            {
                continue;
            }

            return item;
        }

        return null;
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

    private static PodLogContainerSelectionKey GetContainerSelectionKey(PodLogContainerSelectionItem item)
    {
        return new PodLogContainerSelectionKey(item.Name, item.IsInitContainer, item.IsEphemeralContainer);
    }

    private static PodLogContainerSelectionKey GetContainerSelectionKey(PodLogContainerOption item)
    {
        return new PodLogContainerSelectionKey(item.Name, item.IsInitContainer, item.IsEphemeralContainer);
    }

    private static ObservableCollection<TItem> CreateSelection<TItem>(TItem item)
    {
        return new ObservableCollection<TItem>([item]);
    }

    private static bool ContainsAllSelection<TSelectionItem>(IEnumerable<TSelectionItem> selectedItems, Func<TSelectionItem, bool> isAll)
    {
        foreach (var selectedItem in selectedItems)
        {
            if (isAll(selectedItem))
            {
                return true;
            }
        }

        return false;
    }

    private static bool ContainsAllSelection<TSelectionItem>(IReadOnlyCollection<TSelectionItem> selectedItems)
    {
        if (selectedItems.Count == 0)
        {
            return true;
        }

        foreach (var selectedItem in selectedItems)
        {
            switch (selectedItem)
            {
                case PodLogPodSelectionItem podSelectionItem when podSelectionItem.IsAll:
                    return true;
                case PodLogContainerSelectionItem containerSelectionItem when containerSelectionItem.IsAll:
                    return true;
            }
        }

        return false;
    }

    private void SelectedPodItemsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_isApplyingSession || _isNormalizingPodSelection)
        {
            return;
        }

        var normalization = GetPodSelectionNormalization(e);
        if (normalization != PodLogSelectionNormalization.None)
        {
            QueueNormalizeSelectedPodItems(normalization);
            UpdateResourceNameToggleState();
            return;
        }

        UpdateResourceNameToggleState();
        RequestReconnect();
    }

    private void SelectedContainerItemsOnCollectionChanged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        if (_isApplyingSession || _isNormalizingContainerSelection)
        {
            return;
        }

        var normalization = GetContainerSelectionNormalization(e);
        if (normalization != PodLogSelectionNormalization.None)
        {
            QueueNormalizeSelectedContainerItems(normalization);
            UpdateResourceNameToggleState();
            return;
        }

        UpdateResourceNameToggleState();
        RequestReconnect();
    }

    private PodLogSelectionNormalization GetPodSelectionNormalization(NotifyCollectionChangedEventArgs e)
    {
        if (SelectedPodItems.Count == 0)
        {
            return PodLogSelectionNormalization.SelectAll;
        }

        if (ContainsAllPodSelection(e.NewItems))
        {
            return PodLogSelectionNormalization.SelectAll;
        }

        if (ContainsAllSelection(SelectedPodItems))
        {
            return PodLogSelectionNormalization.RemoveAll;
        }

        if (SelectedPodItems.Count == PodSelectionItems.Count - 1 && SelectedPodItems.Count > 1)
        {
            return PodLogSelectionNormalization.SelectAll;
        }

        return PodLogSelectionNormalization.None;
    }

    private static bool ContainsAllPodSelection(System.Collections.IList? items)
    {
        if (items is null)
        {
            return false;
        }

        for (var i = 0; i < items.Count; i++)
        {
            if (items[i] is PodLogPodSelectionItem { IsAll: true })
            {
                return true;
            }
        }

        return false;
    }

    private PodLogSelectionNormalization GetContainerSelectionNormalization(NotifyCollectionChangedEventArgs e)
    {
        if (SelectedContainerItems.Count == 0)
        {
            return PodLogSelectionNormalization.SelectAll;
        }

        if (ContainsAllContainerSelection(e.NewItems))
        {
            return PodLogSelectionNormalization.SelectAll;
        }

        if (ContainsAllSelection(SelectedContainerItems))
        {
            return PodLogSelectionNormalization.RemoveAll;
        }

        if (SelectedContainerItems.Count == ContainerSelectionItems.Count - 1 && SelectedContainerItems.Count > 1)
        {
            return PodLogSelectionNormalization.SelectAll;
        }

        return PodLogSelectionNormalization.None;
    }

    private static bool ContainsAllContainerSelection(System.Collections.IList? items)
    {
        if (items is null)
        {
            return false;
        }

        for (var i = 0; i < items.Count; i++)
        {
            if (items[i] is PodLogContainerSelectionItem { IsAll: true })
            {
                return true;
            }
        }

        return false;
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

    private void QueueNormalizeSelectedPodItems(PodLogSelectionNormalization normalization)
    {
        if (_pendingNormalizePodSelection || PodSelectionItems.Count == 0)
        {
            _pendingPodSelectionNormalization = normalization;
            return;
        }

        _pendingPodSelectionNormalization = normalization;
        _pendingNormalizePodSelection = true;
        Dispatcher.UIThread.Post(NormalizeSelectedPodItems, DispatcherPriority.Background);
    }

    private void QueueNormalizeSelectedContainerItems(PodLogSelectionNormalization normalization)
    {
        if (_pendingNormalizeContainerSelection || ContainerSelectionItems.Count == 0)
        {
            _pendingContainerSelectionNormalization = normalization;
            return;
        }

        _pendingContainerSelectionNormalization = normalization;
        _pendingNormalizeContainerSelection = true;
        Dispatcher.UIThread.Post(NormalizeSelectedContainerItems, DispatcherPriority.Background);
    }

    private void NormalizeSelectedPodItems()
    {
        _pendingNormalizePodSelection = false;
        var normalization = _pendingPodSelectionNormalization;
        _pendingPodSelectionNormalization = PodLogSelectionNormalization.None;
        _isNormalizingPodSelection = true;
        try
        {
            if (normalization == PodLogSelectionNormalization.SelectAll && PodSelectionItems.Count > 0)
            {
                SelectAllPodItem();
            }
            else if (normalization == PodLogSelectionNormalization.RemoveAll)
            {
                RemoveAllPodItem();
            }
        }
        finally
        {
            _isNormalizingPodSelection = false;
        }

        UpdateResourceNameToggleState();
        RequestReconnect();
    }

    private void SelectAllPodItem()
    {
        SelectedPodItems.Clear();
        SelectedPodItems.Add(PodSelectionItems[0]);
    }

    private void RemoveAllPodItem()
    {
        for (var i = SelectedPodItems.Count - 1; i >= 0; i--)
        {
            if (SelectedPodItems[i].IsAll)
            {
                SelectedPodItems.RemoveAt(i);
            }
        }

        if (SelectedPodItems.Count == 0 && PodSelectionItems.Count > 0)
        {
            SelectedPodItems.Add(PodSelectionItems[0]);
        }

    }

    private void NormalizeSelectedContainerItems()
    {
        _pendingNormalizeContainerSelection = false;
        var normalization = _pendingContainerSelectionNormalization;
        _pendingContainerSelectionNormalization = PodLogSelectionNormalization.None;
        _isNormalizingContainerSelection = true;
        try
        {
            if (normalization == PodLogSelectionNormalization.SelectAll && ContainerSelectionItems.Count > 0)
            {
                SelectAllContainerItem();
            }
            else if (normalization == PodLogSelectionNormalization.RemoveAll)
            {
                RemoveAllContainerItem();
            }
        }
        finally
        {
            _isNormalizingContainerSelection = false;
        }

        UpdateResourceNameToggleState();
        RequestReconnect();
    }

    private void SelectAllContainerItem()
    {
        SelectedContainerItems.Clear();
        SelectedContainerItems.Add(ContainerSelectionItems[0]);
    }

    private void RemoveAllContainerItem()
    {
        for (var i = SelectedContainerItems.Count - 1; i >= 0; i--)
        {
            if (SelectedContainerItems[i].IsAll)
            {
                SelectedContainerItems.RemoveAt(i);
            }
        }

        if (SelectedContainerItems.Count == 0 && ContainerSelectionItems.Count > 0)
        {
            SelectedContainerItems.Add(ContainerSelectionItems[0]);
        }

    }

    private void ReplaceSelectedPodItems(IEnumerable<PodLogPodSelectionItem> items)
    {
        ReplaceSelectedItems(SelectedPodItems, items);
    }

    private void ReplaceSelectedContainerItems(IEnumerable<PodLogContainerSelectionItem> items)
    {
        ReplaceSelectedItems(SelectedContainerItems, items);
    }

    private void ReplaceSelectedItems<TItem>(ObservableCollection<TItem> selectedItems, IEnumerable<TItem> items)
    {
        var wasApplyingSession = _isApplyingSession;
        _isApplyingSession = true;
        try
        {
            selectedItems.Clear();
            foreach (var item in items)
            {
                selectedItems.Add(item);
            }
        }
        finally
        {
            _isApplyingSession = wasApplyingSession;
        }
    }

}
