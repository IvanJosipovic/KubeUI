using System.Collections.ObjectModel;
using System.Text;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Dock.Model.Core;
using k8s.Models;
using KubeUI.Avalonia.Resources.Workloads.v1.Pod.Services;
using KubeUI.Avalonia.Resources.Workloads.v1.Pod.ViewModels;
using KubeUI.Avalonia.Services.Settings;
using KubeUI.Avalonia.Tests.Infra;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Resources.Workloads.v1.Pod;

public sealed class PodLogsViewModelTests
{
    [AvaloniaFact]
    public async Task Deployment_rollout_should_switch_logs_to_the_new_pod_without_refresh()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        V1Deployment deployment = new()
        {
            Metadata = new V1ObjectMeta
            {
                Name = "api",
                NamespaceProperty = "default",
                Uid = "deployment-uid",
            },
        };
        V1ReplicaSet oldReplicaSet = CreateOwnedReplicaSet("api-old", "old-replicaset-uid", deployment);
        V1Pod oldPod = CreatePod(
            "api-old-pod", "default", "old-pod-uid", "old-replicaset-uid", "api-old", "ReplicaSet", ["app"],
            creationTimestamp: new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc));

        await workspace.Runtime.AddOrUpdateResource(deployment);
        await workspace.Runtime.AddOrUpdateResource(oldReplicaSet);
        await workspace.Runtime.AddOrUpdateResource(oldPod);
        await workspace.Runtime.SeedResource<V1Deployment>(true);
        await workspace.Runtime.SeedResource<V1ReplicaSet>(true);
        await workspace.Runtime.SeedResource<V1Pod>(true);

        using StatusChangingPodLogStreamClient streamClient = new("old pod line\n", "new pod line\n");
        using PodLogsViewModel viewModel = CreateViewModel(workspace.Runtime, streamClient);
        viewModel.Object = deployment;
        viewModel.ContainerName = "app";

        await viewModel.Connect();
        await WaitForAsync(() => streamClient.Requests.Count == 1 && viewModel.Logs.Text.Contains("old pod line", StringComparison.Ordinal));

        V1ReplicaSet newReplicaSet = CreateOwnedReplicaSet("api-new", "new-replicaset-uid", deployment);
        V1Pod newPod = CreatePod(
            "api-new-pod", "default", "new-pod-uid", "new-replicaset-uid", "api-new", "ReplicaSet", ["app"],
            creationTimestamp: new DateTime(2026, 4, 1, 12, 5, 0, DateTimeKind.Utc));

        await workspace.Runtime.AddOrUpdateResource(newReplicaSet);
        await workspace.Runtime.AddOrUpdateResource(newPod);
        await WaitForAsync(() => workspace.Runtime.GetResource<V1Pod>("default", newPod.Name()) is not null);

        await WaitForAsync(() => streamClient.Requests.Any(request => request.PodName == newPod.Name()));
        await WaitForAsync(() => viewModel.Logs.Text.Contains("new pod line", StringComparison.Ordinal));

        streamClient.Requests.Any(request => request.PodName == newPod.Name()).ShouldBeTrue();
        viewModel.AvailablePods.Select(pod => pod.Name()).ShouldContain(newPod.Name());
    }

    [AvaloniaFact]
    public async Task Connect_should_retarget_to_the_newest_matching_pod_and_stream_logs()
    {
        using var workspace = await Application.Current.CreateClusterAsync();

        V1Pod originalPod = CreatePod(
            name: "app-7c9dd9f4f4-abcde",
            namespaceName: "default",
            uid: "old-pod-uid",
            ownerUid: "replicaset-uid",
            ownerName: "app-7c9dd9f4f4",
            ownerKind: "ReplicaSet",
            containers: ["app"],
            creationTimestamp: new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc));

        V1Pod replacementPod = CreatePod(
            name: "app-7c9dd9f4f4-fghij",
            namespaceName: "default",
            uid: "new-pod-uid",
            ownerUid: "replicaset-uid",
            ownerName: "app-7c9dd9f4f4",
            ownerKind: "ReplicaSet",
            containers: ["app"],
            creationTimestamp: new DateTime(2026, 4, 1, 12, 5, 0, DateTimeKind.Utc));

        await workspace.Runtime.AddOrUpdateResource(replacementPod);
        await workspace.Runtime.SeedResource<V1Pod>(true);

        RecordingPodLogStreamClient streamClient = new(["first line\nsecond line\n"]);
        using PodLogsViewModel viewModel = CreateViewModel(workspace.Runtime, streamClient);
        viewModel.Object = originalPod;
        viewModel.ContainerName = "app";

        await viewModel.Connect();

        await WaitForAsync(() => streamClient.Requests.Count == 1 && viewModel.Logs.Text.Contains("second line", StringComparison.Ordinal));

        streamClient.Requests.Count.ShouldBe(1);
        streamClient.Requests[0].PodName.ShouldBe("app-7c9dd9f4f4-fghij");
        streamClient.Requests[0].ContainerName.ShouldBe("app");
        streamClient.Requests[0].Previous.ShouldBeFalse();
        viewModel.Object.Name().ShouldBe("app-7c9dd9f4f4-fghij");
        viewModel.SessionResolution.ShouldNotBeNull();
        viewModel.SessionResolution!.PodChanged.ShouldBeTrue();
        viewModel.Logs.Text.ShouldContain("first line");
        viewModel.Logs.Text.ShouldContain("second line");
    }

    [AvaloniaFact]
    public async Task Connect_should_expose_related_pods_and_all_container_options()
    {
        using var workspace = await Application.Current.CreateClusterAsync();

        V1Pod olderPod = CreatePod(
            name: "app-7c9dd9f4f4-abcde",
            namespaceName: "default",
            uid: "old-pod-uid",
            ownerUid: "replicaset-uid",
            ownerName: "app-7c9dd9f4f4",
            ownerKind: "ReplicaSet",
            containers: ["app", "sidecar"],
            initContainers: ["init-db"],
            creationTimestamp: new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc));

        V1Pod newerPod = CreatePod(
            name: "app-7c9dd9f4f4-fghij",
            namespaceName: "default",
            uid: "new-pod-uid",
            ownerUid: "replicaset-uid",
            ownerName: "app-7c9dd9f4f4",
            ownerKind: "ReplicaSet",
            containers: ["app", "sidecar"],
            initContainers: ["init-db"],
            creationTimestamp: new DateTime(2026, 4, 1, 12, 5, 0, DateTimeKind.Utc));

        await workspace.Runtime.AddOrUpdateResource(olderPod);
        await workspace.Runtime.AddOrUpdateResource(newerPod);
        await workspace.Runtime.SeedResource<V1Pod>(true);

        RecordingPodLogStreamClient streamClient = new(["newer line\n"]);
        using PodLogsViewModel viewModel = CreateViewModel(workspace.Runtime, streamClient);
        viewModel.Object = olderPod;
        viewModel.ContainerName = "app";

        await viewModel.Connect();

        await WaitForAsync(() => streamClient.Requests.Count == 1);

        viewModel.AvailablePods.Count.ShouldBe(2);
        viewModel.AvailablePods[0].Name().ShouldBe("app-7c9dd9f4f4-fghij");
        viewModel.AvailablePods[1].Name().ShouldBe("app-7c9dd9f4f4-abcde");
        viewModel.AvailableContainers.Count.ShouldBe(3);
        viewModel.AvailableContainers.Select(x => x.Name).ShouldBe(["init-db", "app", "sidecar"]);
        viewModel.AvailableContainers[0].DisplayName.ShouldBe("init-db (init)");
        viewModel.AvailableContainers[1].DisplayName.ShouldBe("app");
        viewModel.PodSelectionItems.Count.ShouldBe(3);
        viewModel.PodSelectionItems[0].IsAll.ShouldBeTrue();
        viewModel.PodSelectionItems[0].DisplayName.ShouldBe(KubeUI.Avalonia.Assets.Resources.PodLogsView_AllPods);
        viewModel.SelectedPodItems.Count.ShouldBe(1);
        viewModel.SelectedPodItems[0].Pod!.Name().ShouldBe("app-7c9dd9f4f4-abcde");
        viewModel.ContainerSelectionItems.Count.ShouldBe(4);
        viewModel.ContainerSelectionItems[0].IsAll.ShouldBeTrue();
        viewModel.ContainerSelectionItems[0].DisplayName.ShouldBe(KubeUI.Avalonia.Assets.Resources.PodLogsView_AllContainers);
        viewModel.SelectedContainerItems.Count.ShouldBe(1);
        viewModel.SelectedContainerItems[0].Name.ShouldBe("app");
    }

    [AvaloniaFact]
    public async Task Connect_should_stream_multiple_selected_pods_and_containers()
    {
        using var workspace = await Application.Current.CreateClusterAsync();

        V1Pod olderPod = CreatePod(
            name: "app-7c9dd9f4f4-abcde",
            namespaceName: "default",
            uid: "old-pod-uid",
            ownerUid: "replicaset-uid",
            ownerName: "app-7c9dd9f4f4",
            ownerKind: "ReplicaSet",
            containers: ["app", "sidecar"],
            creationTimestamp: new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc));

        V1Pod newerPod = CreatePod(
            name: "app-7c9dd9f4f4-fghij",
            namespaceName: "default",
            uid: "new-pod-uid",
            ownerUid: "replicaset-uid",
            ownerName: "app-7c9dd9f4f4",
            ownerKind: "ReplicaSet",
            containers: ["app", "sidecar"],
            creationTimestamp: new DateTime(2026, 4, 1, 12, 5, 0, DateTimeKind.Utc));

        V1Pod newestPod = CreatePod(
            name: "app-7c9dd9f4f4-klmno",
            namespaceName: "default",
            uid: "newest-pod-uid",
            ownerUid: "replicaset-uid",
            ownerName: "app-7c9dd9f4f4",
            ownerKind: "ReplicaSet",
            containers: ["app", "sidecar", "metrics"],
            creationTimestamp: new DateTime(2026, 4, 1, 12, 10, 0, DateTimeKind.Utc));

        await workspace.Runtime.AddOrUpdateResource(olderPod);
        await workspace.Runtime.AddOrUpdateResource(newerPod);
        await workspace.Runtime.AddOrUpdateResource(newestPod);
        await workspace.Runtime.SeedResource<V1Pod>(true);

        RecordingPodLogStreamClient streamClient = new(["newer app line\n", "newer sidecar line\n", "older app line\n", "older sidecar line\n"]);
        using PodLogsViewModel viewModel = CreateViewModel(workspace.Runtime, streamClient);
        viewModel.Object = olderPod;
        viewModel.ContainerName = "app";
        viewModel.ShowResourceNames = true;
        viewModel.SelectedPodItems = new ObservableCollection<PodLogPodSelectionItem>(
            [
                new PodLogPodSelectionItem(olderPod, olderPod.Name(), false),
                new PodLogPodSelectionItem(newerPod, newerPod.Name(), false),
            ]);
        viewModel.SelectedContainerItems = new ObservableCollection<PodLogContainerSelectionItem>(
            [
                new PodLogContainerSelectionItem("app", "app", false, false),
                new PodLogContainerSelectionItem("sidecar", "sidecar", false, false),
            ]);

        await viewModel.Connect();

        await WaitForAsync(() => streamClient.Requests.Count == 4 && viewModel.Logs.Text.Contains("older sidecar line", StringComparison.Ordinal));

        streamClient.Requests.Count.ShouldBe(4);
        streamClient.Requests.Select(x => (x.PodName, x.ContainerName)).ShouldBe(
            [
                ("app-7c9dd9f4f4-fghij", "app"),
                ("app-7c9dd9f4f4-fghij", "sidecar"),
                ("app-7c9dd9f4f4-abcde", "app"),
                ("app-7c9dd9f4f4-abcde", "sidecar"),
            ]);
        viewModel.SelectedPodItems.Count.ShouldBe(2);
        viewModel.SelectedContainerItems.Count.ShouldBe(2);
        viewModel.Logs.Text.ShouldContain("[app-7c9dd9f4f4-fghij/app] newer app line");
        viewModel.Logs.Text.ShouldContain("[app-7c9dd9f4f4-abcde/sidecar] older sidecar line");
    }

    [AvaloniaFact]
    public async Task Connect_should_not_open_logs_for_a_container_that_is_waiting_to_start()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        V1Pod pod = CreatePod(
            name: "app-7c9dd9f4f4-abcde",
            namespaceName: "default",
            uid: "pod-uid",
            containers: ["app", "sidecar"]);
        pod.Status!.Phase = "Running";
        pod.Status.ContainerStatuses =
        [
            new V1ContainerStatus
            {
                Name = "app",
                State = new V1ContainerState { Waiting = new V1ContainerStateWaiting { Reason = "ContainerCreating" } },
            },
            new V1ContainerStatus
            {
                Name = "sidecar",
                State = new V1ContainerState { Running = new V1ContainerStateRunning() },
            },
        ];

        await workspace.Runtime.AddOrUpdateResource(pod);
        await workspace.Runtime.SeedResource<V1Pod>(true);
        await WaitForAsync(() => workspace.Runtime.GetResource<V1Pod>("default", pod.Name())?.Status?.ContainerStatuses?.Count == 2);

        RecordingPodLogStreamClient streamClient = new(["sidecar line\n"]);
        using PodLogsViewModel viewModel = CreateViewModel(workspace.Runtime, streamClient);
        viewModel.Object = pod;
        viewModel.ContainerName = "app";
        viewModel.SelectedContainerItems = new ObservableCollection<PodLogContainerSelectionItem>(
            [new PodLogContainerSelectionItem(string.Empty, "all", false, true)]);

        await viewModel.Connect();
        await WaitForAsync(() => streamClient.Requests.Count == 1 && viewModel.Logs.Text.Contains("sidecar line", StringComparison.Ordinal));

        streamClient.Requests.Select(request => request.ContainerName).ShouldBe(["sidecar"]);
        viewModel.Logs.Text.ShouldNotContain("app line");
    }

    [AvaloniaFact]
    public async Task Refresh_should_open_a_container_after_it_changes_from_waiting_to_running()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        V1Pod waitingPod = CreatePod(
            name: "app-7c9dd9f4f4-abcde",
            namespaceName: "default",
            uid: "pod-uid",
            containers: ["app", "sidecar"]);
        waitingPod.Status!.Phase = "Running";
        waitingPod.Status.ContainerStatuses =
        [
            new V1ContainerStatus
            {
                Name = "app",
                State = new V1ContainerState { Waiting = new V1ContainerStateWaiting { Reason = "ContainerCreating" } },
            },
            new V1ContainerStatus
            {
                Name = "sidecar",
                State = new V1ContainerState { Running = new V1ContainerStateRunning() },
            },
        ];
        V1Pod runningPod = CreatePod(
            name: waitingPod.Name(),
            namespaceName: "default",
            uid: "pod-uid",
            containers: ["app", "sidecar"]);
        runningPod.Status!.Phase = "Running";
        runningPod.Status.ContainerStatuses =
        [
            new V1ContainerStatus
            {
                Name = "app",
                State = new V1ContainerState { Running = new V1ContainerStateRunning() },
            },
            new V1ContainerStatus
            {
                Name = "sidecar",
                State = new V1ContainerState { Running = new V1ContainerStateRunning() },
            },
        ];

        await workspace.Runtime.AddOrUpdateResource(waitingPod);
        await workspace.Runtime.SeedResource<V1Pod>(true);
        await WaitForAsync(() => workspace.Runtime.GetResource<V1Pod>("default", waitingPod.Name())?.Status?.ContainerStatuses?.Count == 2);

        RecordingPodLogStreamClient streamClient = new(["sidecar line\n", "app line\n", "refreshed sidecar line\n"]);
        using PodLogsViewModel viewModel = CreateViewModel(workspace.Runtime, streamClient);
        viewModel.Object = waitingPod;
        viewModel.ContainerName = "app";
        viewModel.SelectedContainerItems = new ObservableCollection<PodLogContainerSelectionItem>(
            [new PodLogContainerSelectionItem(string.Empty, "all", false, true)]);

        await viewModel.Connect();
        await WaitForAsync(() => streamClient.Requests.Count == 1 && viewModel.Logs.Text.Contains("sidecar line", StringComparison.Ordinal));

        await workspace.Runtime.AddOrUpdateResource(runningPod);
        await WaitForAsync(() => workspace.Runtime.GetResource<V1Pod>("default", runningPod.Name())?.Status?.ContainerStatuses?.FirstOrDefault(status => status.Name == "app")?.State?.Running is not null);
        await viewModel.Refresh();

        await WaitForAsync(() => streamClient.Requests.Count == 3 && streamClient.Requests.Any(request => request.ContainerName == "app") && viewModel.Logs.Text.Contains("app line", StringComparison.Ordinal));

        streamClient.Requests.Skip(1).Select(request => request.ContainerName).ShouldBe(["app", "sidecar"]);
    }

    [AvaloniaFact]
    public async Task Show_resource_names_should_toggle_rendered_prefixes()
    {
        using var workspace = await Application.Current.CreateClusterAsync();

        V1Pod olderPod = CreatePod(
            name: "app-7c9dd9f4f4-abcde",
            namespaceName: "default",
            uid: "old-pod-uid",
            ownerUid: "replicaset-uid",
            ownerName: "app-7c9dd9f4f4",
            ownerKind: "ReplicaSet",
            containers: ["app", "sidecar"],
            creationTimestamp: new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc));

        V1Pod newerPod = CreatePod(
            name: "app-7c9dd9f4f4-fghij",
            namespaceName: "default",
            uid: "new-pod-uid",
            ownerUid: "replicaset-uid",
            ownerName: "app-7c9dd9f4f4",
            ownerKind: "ReplicaSet",
            containers: ["app", "sidecar"],
            creationTimestamp: new DateTime(2026, 4, 1, 12, 5, 0, DateTimeKind.Utc));

        await workspace.Runtime.AddOrUpdateResource(olderPod);
        await workspace.Runtime.AddOrUpdateResource(newerPod);
        await workspace.Runtime.SeedResource<V1Pod>(true);

        RecordingPodLogStreamClient streamClient = new(["newer app line\n", "older app line\n"]);
        using PodLogsViewModel viewModel = CreateViewModel(workspace.Runtime, streamClient);
        viewModel.Object = olderPod;
        viewModel.ContainerName = "app";
        viewModel.SelectedPodItems = new ObservableCollection<PodLogPodSelectionItem>(
            [
                new PodLogPodSelectionItem(olderPod, olderPod.Name(), false),
                new PodLogPodSelectionItem(newerPod, newerPod.Name(), false),
            ]);
        viewModel.SelectedContainerItems = new ObservableCollection<PodLogContainerSelectionItem>(
            [
                new PodLogContainerSelectionItem("app", "app", false, false),
                new PodLogContainerSelectionItem("sidecar", "sidecar", false, false),
            ]);

        await viewModel.Connect();

        await WaitForAsync(() => viewModel.Logs.Text.Contains("newer app line", StringComparison.Ordinal));
        viewModel.ShowResourceNames.ShouldBeFalse();
        viewModel.Logs.Text.ShouldContain("newer app line");
        viewModel.Logs.Text.ShouldNotContain("[app-7c9dd9f4f4-fghij/app] newer app line");

        viewModel.ShowResourceNames = true;
        await WaitForAsync(() => viewModel.Logs.Text.Contains("[app-7c9dd9f4f4-fghij/app] newer app line", StringComparison.Ordinal));
        viewModel.Logs.Text.ShouldContain("[app-7c9dd9f4f4-fghij/app] newer app line");
        viewModel.Logs.Text.ShouldContain("[app-7c9dd9f4f4-fghij/sidecar] older app line");

        viewModel.ShowResourceNames = false;
        await WaitForAsync(() => !viewModel.Logs.Text.Contains("[app-7c9dd9f4f4-fghij/app]", StringComparison.Ordinal));
        viewModel.Logs.Text.ShouldContain("newer app line");
        viewModel.Logs.Text.ShouldNotContain("[app-7c9dd9f4f4-fghij/app] newer app line");
    }

    [AvaloniaFact]
    public async Task Changing_previous_should_restart_the_session_with_updated_log_options()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        V1Pod pod = CreatePod(
            name: "app-7c9dd9f4f4-abcde",
            namespaceName: "default",
            uid: "pod-uid",
            ownerUid: "replicaset-uid",
            ownerName: "app-7c9dd9f4f4",
            ownerKind: "ReplicaSet",
            containers: ["app"],
            restartCount: 1,
            creationTimestamp: new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc));

        await workspace.Runtime.AddOrUpdateResource(pod);
        await workspace.Runtime.SeedResource<V1Pod>(true);

        RecordingPodLogStreamClient streamClient = new(["current line\n", "previous line\n"]);
        using PodLogsViewModel viewModel = CreateViewModel(workspace.Runtime, streamClient);
        viewModel.Object = pod;
        viewModel.ContainerName = "app";

        await viewModel.Connect();
        await WaitForAsync(() => streamClient.Requests.Count == 1);

        viewModel.Previous = true;

        await WaitForAsync(() => streamClient.Requests.Count == 2 && viewModel.Logs.Text.Contains("previous line", StringComparison.Ordinal));

        streamClient.Requests.Count.ShouldBe(2);
        streamClient.Requests[0].Previous.ShouldBeFalse();
        streamClient.Requests[1].Previous.ShouldBeTrue();
        viewModel.PreviousLogsAvailable.ShouldBeTrue();
        viewModel.SessionState.ShouldNotBeNull();
        viewModel.SessionState!.Previous.ShouldBeTrue();
        viewModel.Logs.Text.ShouldContain("previous line");
    }

    [AvaloniaFact]
    public async Task Refresh_should_restart_the_session_with_the_current_selection()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        V1Pod pod = CreatePod(
            name: "app-7c9dd9f4f4-abcde",
            namespaceName: "default",
            uid: "pod-uid",
            ownerUid: "replicaset-uid",
            ownerName: "app-7c9dd9f4f4",
            ownerKind: "ReplicaSet",
            containers: ["app"],
            restartCount: 1,
            creationTimestamp: new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc));

        await workspace.Runtime.AddOrUpdateResource(pod);
        await workspace.Runtime.SeedResource<V1Pod>(true);

        RecordingPodLogStreamClient streamClient = new(["initial line\n", "refreshed line\n"]);
        using PodLogsViewModel viewModel = CreateViewModel(workspace.Runtime, streamClient);
        viewModel.Object = pod;
        viewModel.ContainerName = "app";

        await viewModel.Connect();
        await WaitForAsync(() => streamClient.Requests.Count == 1);

        await viewModel.Refresh();

        await WaitForAsync(() => streamClient.Requests.Count == 2 && viewModel.Logs.Text.Contains("refreshed line", StringComparison.Ordinal));

        streamClient.Requests.Count.ShouldBe(2);
        streamClient.Requests[1].PodName.ShouldBe("app-7c9dd9f4f4-abcde");
        streamClient.Requests[1].ContainerName.ShouldBe("app");
        viewModel.Logs.Text.ShouldContain("refreshed line");
    }

    [AvaloniaFact]
    public async Task Refresh_should_include_a_new_related_pod_added_after_connect()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        V1Pod originalPod = CreatePod(
            name: "app-7c9dd9f4f4-abcde",
            namespaceName: "default",
            uid: "old-pod-uid",
            ownerUid: "replicaset-uid",
            ownerName: "app-7c9dd9f4f4",
            ownerKind: "ReplicaSet",
            containers: ["app"],
            creationTimestamp: new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc));
        V1Pod addedPod = CreatePod(
            name: "app-7c9dd9f4f4-fghij",
            namespaceName: "default",
            uid: "new-pod-uid",
            ownerUid: "replicaset-uid",
            ownerName: "app-7c9dd9f4f4",
            ownerKind: "ReplicaSet",
            containers: ["app"],
            creationTimestamp: new DateTime(2026, 4, 1, 12, 5, 0, DateTimeKind.Utc));

        await workspace.Runtime.AddOrUpdateResource(originalPod);
        await workspace.Runtime.SeedResource<V1Pod>(true);

        RecordingPodLogStreamClient streamClient = new(["original line\n", "refreshed line\n", "added line\n"]);
        using PodLogsViewModel viewModel = CreateViewModel(workspace.Runtime, streamClient);
        viewModel.Object = originalPod;
        viewModel.ContainerName = "app";

        await viewModel.Connect();
        await WaitForAsync(() => streamClient.Requests.Count == 1);

        await workspace.Runtime.AddOrUpdateResource(addedPod);
        await WaitForAsync(() => workspace.Runtime.GetResource<V1Pod>("default", addedPod.Name()) is not null);
        await viewModel.Refresh();

        await WaitForAsync(() => streamClient.Requests.Count == 2 && viewModel.Logs.Text.Contains("refreshed line", StringComparison.Ordinal));

        streamClient.Requests[1].PodName.ShouldBe(originalPod.Name());
        viewModel.Object.Name().ShouldBe(originalPod.Name());
        viewModel.AvailablePods.Select(pod => pod.Name()).ShouldBe([addedPod.Name(), originalPod.Name()]);
        viewModel.SessionResolution!.PodChanged.ShouldBeFalse();

        viewModel.SelectedPodItems = new ObservableCollection<PodLogPodSelectionItem>([viewModel.PodSelectionItems[1]]);
        await WaitForAsync(() => streamClient.Requests.Count >= 3
            && streamClient.Requests[^1].PodName == addedPod.Name()
            && viewModel.Logs.Text.Contains("added line", StringComparison.Ordinal));

        viewModel.SelectedPodItems.Count.ShouldBe(1);
        viewModel.SelectedPodItems[0].Pod!.Name().ShouldBe(addedPod.Name());
        streamClient.Requests[^1].PodName.ShouldBe(addedPod.Name());
        viewModel.Logs.Text.ShouldContain("added line");
    }

    [AvaloniaFact]
    public async Task Refresh_should_fall_back_when_the_current_newest_pod_is_removed()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        V1Pod remainingPod = CreatePod(
            name: "app-7c9dd9f4f4-abcde",
            namespaceName: "default",
            uid: "old-pod-uid",
            ownerUid: "replicaset-uid",
            ownerName: "app-7c9dd9f4f4",
            ownerKind: "ReplicaSet",
            containers: ["app"],
            creationTimestamp: new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc));
        V1Pod removedPod = CreatePod(
            name: "app-7c9dd9f4f4-fghij",
            namespaceName: "default",
            uid: "new-pod-uid",
            ownerUid: "replicaset-uid",
            ownerName: "app-7c9dd9f4f4",
            ownerKind: "ReplicaSet",
            containers: ["app"],
            creationTimestamp: new DateTime(2026, 4, 1, 12, 5, 0, DateTimeKind.Utc));

        await workspace.Runtime.AddOrUpdateResource(remainingPod);
        await workspace.Runtime.AddOrUpdateResource(removedPod);
        await workspace.Runtime.SeedResource<V1Pod>(true);

        RecordingPodLogStreamClient streamClient = new(["newest line\n", "fallback line\n"]);
        using PodLogsViewModel viewModel = CreateViewModel(workspace.Runtime, streamClient);
        viewModel.Object = removedPod;
        viewModel.ContainerName = "app";

        await viewModel.Connect();
        await WaitForAsync(() => streamClient.Requests.Count == 1);
        streamClient.Requests[0].PodName.ShouldBe(removedPod.Name());

        await workspace.Runtime.DeleteResource(removedPod);
        await WaitForAsync(() => workspace.Runtime.GetResource<V1Pod>("default", removedPod.Name()) is null);
        await viewModel.Refresh();

        await WaitForAsync(() => streamClient.Requests.Count == 2 && viewModel.Logs.Text.Contains("fallback line", StringComparison.Ordinal));

        streamClient.Requests[1].PodName.ShouldBe(remainingPod.Name());
        viewModel.Object.Name().ShouldBe(remainingPod.Name());
        viewModel.AvailablePods.Count.ShouldBe(1);
        viewModel.AvailablePods[0].Name().ShouldBe(remainingPod.Name());
        viewModel.SessionResolution!.PodChanged.ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task Refresh_should_expose_a_new_container_added_to_an_existing_pod()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        V1Pod originalPod = CreatePod(
            name: "app-7c9dd9f4f4-abcde",
            namespaceName: "default",
            uid: "pod-uid",
            containers: ["app"]);
        V1Pod updatedPod = CreatePod(
            name: originalPod.Name(),
            namespaceName: "default",
            uid: "pod-uid",
            containers: ["app", "sidecar"]);

        await workspace.Runtime.AddOrUpdateResource(originalPod);
        await workspace.Runtime.SeedResource<V1Pod>(true);

        RecordingPodLogStreamClient streamClient = new(["app line\n", "refreshed app line\n", "sidecar line\n"]);
        using PodLogsViewModel viewModel = CreateViewModel(workspace.Runtime, streamClient);
        viewModel.Object = originalPod;
        viewModel.ContainerName = "app";

        await viewModel.Connect();
        await WaitForAsync(() => streamClient.Requests.Count == 1);

        await workspace.Runtime.AddOrUpdateResource(updatedPod);
        await WaitForAsync(() => workspace.Runtime.GetResource<V1Pod>("default", updatedPod.Name())?.Spec?.Containers?.Count == 2);
        await viewModel.Refresh();

        await WaitForAsync(() => streamClient.Requests.Count == 2 && viewModel.ContainerSelectionItems.Count == 3);

        viewModel.AvailableContainers.Select(container => container.Name).ShouldBe(["app", "sidecar"]);
        viewModel.ContainerSelectionItems.Select(container => container.Name).ShouldBe([string.Empty, "app", "sidecar"]);

        viewModel.SelectedContainerItems = new ObservableCollection<PodLogContainerSelectionItem>([viewModel.ContainerSelectionItems[2]]);
        await WaitForAsync(() => streamClient.Requests.Count >= 3
            && streamClient.Requests[^1].ContainerName == "sidecar"
            && viewModel.Logs.Text.Contains("sidecar line", StringComparison.Ordinal));

        viewModel.SelectedContainerItems.Count.ShouldBe(1);
        viewModel.SelectedContainerItems[0].Name.ShouldBe("sidecar");
        streamClient.Requests[^1].ContainerName.ShouldBe("sidecar");
        viewModel.Logs.Text.ShouldContain("sidecar line");
    }

    [AvaloniaFact]
    public async Task Current_follow_stream_ending_should_reconnect_to_continue_after_container_restart()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        V1Pod pod = CreatePod(
            name: "app-7c9dd9f4f4-abcde",
            namespaceName: "default",
            uid: "pod-uid",
            containers: ["app"],
            creationTimestamp: new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc));
        pod.Status!.Phase = "Running";

        await workspace.Runtime.AddOrUpdateResource(pod);
        await workspace.Runtime.SeedResource<V1Pod>(true);

        RestartingPodLogStreamClient streamClient = new("before restart\n", "after restart\n");
        using PodLogsViewModel viewModel = CreateViewModel(workspace.Runtime, streamClient);
        viewModel.Object = pod;
        viewModel.ContainerName = "app";

        await viewModel.Connect();

        await WaitForAsync(() => streamClient.Requests.Count == 2 && viewModel.Logs.Text.Contains("after restart", StringComparison.Ordinal), timeoutMs: 5000);

        streamClient.Requests.Count.ShouldBe(2);
        streamClient.Requests[0].Follow.ShouldBeTrue();
        streamClient.Requests[0].Previous.ShouldBeFalse();
        streamClient.Requests[1].PodName.ShouldBe("app-7c9dd9f4f4-abcde");
        viewModel.Logs.Text.ShouldContain("after restart");
    }

    [AvaloniaFact]
    public async Task Terminal_pod_should_not_reconnect_after_follow_stream_ends()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        V1Pod pod = CreatePod(
            name: "job-completed",
            namespaceName: "default",
            uid: "completed-pod-uid",
            containers: ["job"]);
        pod.Status!.Phase = "Succeeded";

        await workspace.Runtime.AddOrUpdateResource(pod);
        await workspace.Runtime.SeedResource<V1Pod>(true);

        RestartingPodLogStreamClient streamClient = new("completed line\n", "unexpected reconnect\n");
        using PodLogsViewModel viewModel = CreateViewModel(workspace.Runtime, streamClient);
        viewModel.Object = pod;
        viewModel.ContainerName = "job";

        await viewModel.Connect();
        await WaitForAsync(() => streamClient.Requests.Count == 1 && viewModel.Logs.Text.Contains("completed line", StringComparison.Ordinal));
        await Should.ThrowAsync<TimeoutException>(() => TestWait.UntilAsync(
            () => streamClient.Requests.Count > 1,
            TimeSpan.FromSeconds(2),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs()));

        streamClient.Requests.Count.ShouldBe(1);
        viewModel.IsConnected.ShouldBeFalse();
        viewModel.Logs.Text.ShouldContain("completed line");
        viewModel.Logs.Text.ShouldNotContain("unexpected reconnect");
    }

    [AvaloniaFact]
    public async Task Running_pod_becoming_succeeded_should_stop_follow_reconnects()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        V1Pod runningPod = CreatePod(
            name: "job-transition",
            namespaceName: "default",
            uid: "transition-pod-uid",
            containers: ["job"]);
        runningPod.Status!.Phase = "Running";
        V1Pod completedPod = CreatePod(
            name: runningPod.Name(),
            namespaceName: "default",
            uid: "transition-pod-uid",
            containers: ["job"]);
        completedPod.Status!.Phase = "Succeeded";

        await workspace.Runtime.AddOrUpdateResource(runningPod);
        await workspace.Runtime.SeedResource<V1Pod>(true);

        using StatusChangingPodLogStreamClient streamClient = new("running line\n");
        using PodLogsViewModel viewModel = CreateViewModel(workspace.Runtime, streamClient);
        viewModel.Object = runningPod;
        viewModel.ContainerName = "job";

        await viewModel.Connect();
        await WaitForAsync(() => viewModel.Logs.Text.Contains("running line", StringComparison.Ordinal));

        await workspace.Runtime.AddOrUpdateResource(completedPod);
        await WaitForAsync(() => workspace.Runtime.GetResource<V1Pod>("default", completedPod.Name())?.Status?.Phase == "Succeeded");
        streamClient.Release();

        await WaitForAsync(() => viewModel.IsConnected == false);
        await Should.ThrowAsync<TimeoutException>(() => TestWait.UntilAsync(
            () => streamClient.Requests.Count > 1,
            TimeSpan.FromSeconds(2),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs()));

        streamClient.Requests.Count.ShouldBe(1);
        viewModel.Logs.Text.ShouldContain("running line");
    }

    [AvaloniaFact]
    public async Task Ending_one_multi_container_stream_should_not_reconnect_all_streams()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        V1Pod pod = CreatePod(
            name: "app-7c9dd9f4f4-abcde",
            namespaceName: "default",
            uid: "pod-uid",
            containers: ["app", "sidecar"],
            initContainers: ["init-db"]);
        pod.Status!.Phase = "Running";

        await workspace.Runtime.AddOrUpdateResource(pod);
        await workspace.Runtime.SeedResource<V1Pod>(true);

        using MultiContainerPodLogStreamClient streamClient = new();
        using PodLogsViewModel viewModel = CreateViewModel(workspace.Runtime, streamClient);
        viewModel.Object = pod;
        viewModel.ContainerName = "app";
        viewModel.SelectedContainerItems = new ObservableCollection<PodLogContainerSelectionItem>(
            [new PodLogContainerSelectionItem(string.Empty, "all", false, true)]);

        await viewModel.Connect();

        await WaitForAsync(() => streamClient.Requests.Count == 3 && viewModel.Logs.Text.Contains("init line", StringComparison.Ordinal));
        await Should.ThrowAsync<TimeoutException>(() => TestWait.UntilAsync(
            () => streamClient.Requests.Count > 3,
            TimeSpan.FromSeconds(2),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs()));

        streamClient.Requests.Count.ShouldBe(3);
        viewModel.Logs.Text.ShouldContain("app line");
        viewModel.Logs.Text.ShouldContain("sidecar line");
    }

    [AvaloniaFact]
    public async Task Reconnecting_multiple_container_streams_should_stop_when_replayed_tail_has_no_new_lines()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        V1Pod pod = CreatePod(
            name: "app-7c9dd9f4f4-abcde",
            namespaceName: "default",
            uid: "pod-uid",
            containers: ["app", "sidecar"]);
        pod.Status!.Phase = "Running";

        await workspace.Runtime.AddOrUpdateResource(pod);
        await workspace.Runtime.SeedResource<V1Pod>(true);

        workspace.Runtime.GetResource<V1Pod>("default", pod.Name()).ShouldNotBeNull().Status!.Phase.ShouldBe("Running");
        ReplayingPodLogStreamClient streamClient = new();
        using PodLogsViewModel viewModel = CreateViewModel(workspace.Runtime, streamClient);
        viewModel.Object = pod;
        viewModel.ContainerName = "app";
        viewModel.SelectedContainerItems = new ObservableCollection<PodLogContainerSelectionItem>(
            [new PodLogContainerSelectionItem(string.Empty, "all", false, true)]);

        await viewModel.Connect();

        await WaitForAsync(() => streamClient.RequestCount == 4, timeoutMs: 5000);
        await Should.ThrowAsync<TimeoutException>(() => TestWait.UntilAsync(
            () => streamClient.RequestCount > 4,
            TimeSpan.FromSeconds(2),
            cancellationToken: TestContext.Current.CancellationToken,
            beforePoll: () => Dispatcher.UIThread.RunJobs()));

        viewModel.Logs.LineCount.ShouldBe(2);
        streamClient.RequestCount.ShouldBe(4);
    }

    [AvaloniaFact]
    public async Task Connect_should_retain_only_the_newest_log_entries()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        V1Pod pod = CreatePod(
            name: "app",
            namespaceName: "default",
            uid: "pod-uid",
            containers: ["app"]);
        await workspace.Runtime.AddOrUpdateResource(pod);
        await workspace.Runtime.SeedResource<V1Pod>(true);
        var payload = string.Join(
            Environment.NewLine,
            Enumerable.Range(1, 10_001).Select(index => $"line-{index}"));
        RecordingPodLogStreamClient streamClient = new([payload]);
        using PodLogsViewModel viewModel = CreateViewModel(workspace.Runtime, streamClient);
        viewModel.Object = pod;
        viewModel.ContainerName = "app";

        await viewModel.Connect();

        await WaitForAsync(() => viewModel.Logs.LineCount == 10_000);

        viewModel.Logs.LineCount.ShouldBe(10_000);
        viewModel.Logs.Text.ShouldStartWith($"line-2{Environment.NewLine}");
        viewModel.Logs.Text.Contains($"line-1{Environment.NewLine}").ShouldBeFalse();
    }

    [AvaloniaFact]
    public async Task Connect_should_resolve_ephemeral_container_logs()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        V1Pod pod = CreatePod(
            name: "app",
            namespaceName: "default",
            uid: "pod-uid",
            containers: ["app"],
            ephemeralContainers: ["debug"]);
        await workspace.Runtime.AddOrUpdateResource(pod);
        await workspace.Runtime.SeedResource<V1Pod>(true);
        RecordingPodLogStreamClient streamClient = new(["debug line\n"]);
        using PodLogsViewModel viewModel = CreateViewModel(workspace.Runtime, streamClient);
        viewModel.Object = pod;
        viewModel.ContainerName = "debug";

        await viewModel.Connect();

        await WaitForAsync(() => streamClient.Requests.Count == 1 && viewModel.Logs.Text.Contains("debug line", StringComparison.Ordinal));

        viewModel.AvailableContainers.Select(container => container.Name).ShouldBe(["app", "debug"]);
        viewModel.AvailableContainers[1].DisplayName.ShouldBe("debug (ephemeral)");
        viewModel.AvailableContainers[1].IsEphemeralContainer.ShouldBeTrue();
        viewModel.ContainerSelectionItems[2].IsEphemeralContainer.ShouldBeTrue();
        streamClient.Requests[0].ContainerName.ShouldBe("debug");
    }

    [AvaloniaFact]
    public async Task Dispose_should_be_idempotent()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        using PodLogsViewModel viewModel = CreateViewModel(workspace.Runtime, new RecordingPodLogStreamClient());

        viewModel.Dispose();
        Should.NotThrow(() => viewModel.Dispose());
    }

    [AvaloniaFact]
    public async Task Selecting_all_while_connecting_should_queue_a_follow_up_reconnect()
    {
        using var workspace = await Application.Current.CreateClusterAsync();

        V1Pod olderPod = CreatePod(
            name: "app-7c9dd9f4f4-abcde",
            namespaceName: "default",
            uid: "old-pod-uid",
            ownerUid: "replicaset-uid",
            ownerName: "app-7c9dd9f4f4",
            ownerKind: "ReplicaSet",
            containers: ["app"],
            creationTimestamp: new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc));

        V1Pod newerPod = CreatePod(
            name: "app-7c9dd9f4f4-fghij",
            namespaceName: "default",
            uid: "new-pod-uid",
            ownerUid: "replicaset-uid",
            ownerName: "app-7c9dd9f4f4",
            ownerKind: "ReplicaSet",
            containers: ["app"],
            creationTimestamp: new DateTime(2026, 4, 1, 12, 5, 0, DateTimeKind.Utc));

        await workspace.Runtime.AddOrUpdateResource(olderPod);
        await workspace.Runtime.AddOrUpdateResource(newerPod);
        await workspace.Runtime.SeedResource<V1Pod>(true);

        BlockingPodLogStreamClient streamClient = new("initial line\n");
        using PodLogsViewModel viewModel = CreateViewModel(workspace.Runtime, streamClient);
        viewModel.Object = olderPod;
        viewModel.ContainerName = "app";

        Task connectTask = viewModel.Connect();
        await streamClient.WaitForFirstRequestAsync();

        viewModel.SelectedPodItems = new ObservableCollection<PodLogPodSelectionItem>([viewModel.PodSelectionItems[0]]);
        streamClient.ReleaseFirstRequest();

        await WaitForAsync(() => streamClient.Requests.Count == 3 && viewModel.SelectedPodItems.Count == 1 && viewModel.SelectedPodItems[0].IsAll);

        await connectTask;

        streamClient.Requests.Count.ShouldBe(3);
        streamClient.Requests[0].PodName.ShouldBe("app-7c9dd9f4f4-abcde");
        streamClient.Requests[1].PodName.ShouldBe("app-7c9dd9f4f4-fghij");
        streamClient.Requests[2].PodName.ShouldBe("app-7c9dd9f4f4-abcde");
        viewModel.SelectedPodItems.Count.ShouldBe(1);
        viewModel.SelectedPodItems[0].IsAll.ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task JumpToPresent_should_request_live_following()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        using PodLogsViewModel viewModel = CreateViewModel(workspace.Runtime, new RecordingPodLogStreamClient());

        viewModel.JumpToPresent();

        viewModel.AutoScrollToBottom.ShouldBeTrue();
        viewModel.JumpToPresentRequested.ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task Connect_should_disable_resource_names_in_single_pod_single_container_mode()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        V1Pod pod = CreatePod(
            name: "app-7c9dd9f4f4-abcde",
            namespaceName: "default",
            uid: "pod-uid",
            containers: ["app"]);

        await workspace.Runtime.AddOrUpdateResource(pod);
        await workspace.Runtime.SeedResource<V1Pod>(true);

        RecordingPodLogStreamClient streamClient = new(["line\n"]);
        using PodLogsViewModel viewModel = CreateViewModel(workspace.Runtime, streamClient);
        viewModel.Object = pod;
        viewModel.ContainerName = "app";

        await viewModel.Connect();

        await WaitForAsync(() => viewModel.Logs.Text == "line");
        viewModel.CanShowResourceNames.ShouldBeFalse();
        viewModel.ShowResourceNames.ShouldBeFalse();
        viewModel.Logs.Text.ShouldBe("line");
    }

    [AvaloniaFact]
    public async Task Selecting_more_than_one_container_should_enable_resource_names()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        V1Pod pod = CreatePod(
            name: "app-7c9dd9f4f4-abcde",
            namespaceName: "default",
            uid: "pod-uid",
            containers: ["app", "sidecar"]);

        await workspace.Runtime.AddOrUpdateResource(pod);
        await workspace.Runtime.SeedResource<V1Pod>(true);

        using PodLogsViewModel viewModel = CreateViewModel(workspace.Runtime, new RecordingPodLogStreamClient(["app line\n", "sidecar line\n"]));
        viewModel.Object = pod;
        viewModel.ContainerName = "app";

        await viewModel.Connect();
        await WaitForAsync(() => viewModel.ContainerSelectionItems.Count == 3);

        viewModel.SelectedContainerItems = new ObservableCollection<PodLogContainerSelectionItem>(
            [viewModel.ContainerSelectionItems[1], viewModel.ContainerSelectionItems[2]]);

        viewModel.SelectedContainerItems.Count.ShouldBe(2);
        viewModel.CanShowResourceNames.ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task Selecting_all_containers_should_not_throw_and_should_normalize_selection()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        V1Pod pod = CreatePod(
            name: "app-7c9dd9f4f4-abcde",
            namespaceName: "default",
            uid: "pod-uid",
            containers: ["app", "sidecar"]);

        await workspace.Runtime.AddOrUpdateResource(pod);
        await workspace.Runtime.SeedResource<V1Pod>(true);

        RecordingPodLogStreamClient streamClient = new(["line\n"]);
        using PodLogsViewModel viewModel = CreateViewModel(workspace.Runtime, streamClient);
        viewModel.Object = pod;
        viewModel.ContainerName = "app";

        await viewModel.Connect();

        await WaitForAsync(() => viewModel.ContainerSelectionItems.Count == 3);

        viewModel.SelectedContainerItems = new ObservableCollection<PodLogContainerSelectionItem>(
            [viewModel.ContainerSelectionItems[1]]);

        Should.NotThrow(() => viewModel.SelectedContainerItems.Add(viewModel.ContainerSelectionItems[0]));

        await WaitForAsync(() => viewModel.SelectedContainerItems.Count == 1 && viewModel.SelectedContainerItems[0].IsAll);

        viewModel.SelectedContainerItems.Count.ShouldBe(1);
        viewModel.SelectedContainerItems[0].IsAll.ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task Selecting_a_specific_container_should_uncheck_all_containers_and_stream_only_selected_containers()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        V1Pod pod = CreatePod(
            name: "app-7c9dd9f4f4-abcde",
            namespaceName: "default",
            uid: "pod-uid",
            containers: ["app", "sidecar"]);

        await workspace.Runtime.AddOrUpdateResource(pod);
        await workspace.Runtime.SeedResource<V1Pod>(true);

        RecordingPodLogStreamClient streamClient = new(["initial line\n", "all app line\n", "all sidecar line\n", "selected sidecar line\n"]);
        using PodLogsViewModel viewModel = CreateViewModel(workspace.Runtime, streamClient);
        viewModel.Object = pod;
        viewModel.ContainerName = "app";

        await viewModel.Connect();
        await WaitForAsync(() => viewModel.ContainerSelectionItems.Count == 3);

        viewModel.SelectedContainerItems.Clear();
        viewModel.SelectedContainerItems.Add(viewModel.ContainerSelectionItems[0]);
        await WaitForAsync(() => viewModel.SelectedContainerItems.Count == 1 && viewModel.SelectedContainerItems[0].IsAll);
        await WaitForAsync(() => streamClient.Requests.Count >= 3);

        viewModel.SelectedContainerItems.Add(viewModel.ContainerSelectionItems[2]);

        await WaitForAsync(() => viewModel.SelectedContainerItems.Count == 1 && !viewModel.SelectedContainerItems[0].IsAll);
        await WaitForAsync(() => streamClient.Requests.Count >= 4 && streamClient.Requests[^1].ContainerName == "sidecar");

        viewModel.SelectedContainerItems[0].Name.ShouldBe("sidecar");
        streamClient.Requests[^1].ContainerName.ShouldBe("sidecar");
    }

    [AvaloniaFact]
    public async Task Selecting_all_pods_should_select_only_the_all_item_and_stream_every_pod()
    {
        using var workspace = await Application.Current.CreateClusterAsync();

        V1Pod olderPod = CreatePod(
            name: "app-7c9dd9f4f4-abcde",
            namespaceName: "default",
            uid: "old-pod-uid",
            ownerUid: "replicaset-uid",
            ownerName: "app-7c9dd9f4f4",
            ownerKind: "ReplicaSet",
            containers: ["app"],
            creationTimestamp: new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc));

        V1Pod newerPod = CreatePod(
            name: "app-7c9dd9f4f4-fghij",
            namespaceName: "default",
            uid: "new-pod-uid",
            ownerUid: "replicaset-uid",
            ownerName: "app-7c9dd9f4f4",
            ownerKind: "ReplicaSet",
            containers: ["app"],
            creationTimestamp: new DateTime(2026, 4, 1, 12, 5, 0, DateTimeKind.Utc));

        await workspace.Runtime.AddOrUpdateResource(olderPod);
        await workspace.Runtime.AddOrUpdateResource(newerPod);
        await workspace.Runtime.SeedResource<V1Pod>(true);

        RecordingPodLogStreamClient streamClient = new(["newer line\n", "older line\n"]);
        using PodLogsViewModel viewModel = CreateViewModel(workspace.Runtime, streamClient);
        viewModel.Object = olderPod;
        viewModel.ContainerName = "app";

        await viewModel.Connect();
        await WaitForAsync(() => viewModel.PodSelectionItems.Count == 3);

        viewModel.SelectedPodItems.Add(viewModel.PodSelectionItems[0]);

        await WaitForAsync(() => viewModel.SelectedPodItems.Count == 1 && viewModel.SelectedPodItems[0].IsAll);
        await WaitForAsync(() => streamClient.Requests.Count == 3);

        viewModel.SelectedPodItems.Count.ShouldBe(1);
        viewModel.SelectedPodItems[0].IsAll.ShouldBeTrue();
        streamClient.Requests.Skip(1).Select(request => request.PodName).ShouldBe(
            [
                "app-7c9dd9f4f4-fghij",
                "app-7c9dd9f4f4-abcde",
            ]);
    }

    [AvaloniaFact]
    public async Task Selecting_a_specific_pod_should_uncheck_all_pods_and_stream_only_selected_pods()
    {
        using var workspace = await Application.Current.CreateClusterAsync();

        V1Pod olderPod = CreatePod(
            name: "app-7c9dd9f4f4-abcde",
            namespaceName: "default",
            uid: "old-pod-uid",
            ownerUid: "replicaset-uid",
            ownerName: "app-7c9dd9f4f4",
            ownerKind: "ReplicaSet",
            containers: ["app"],
            creationTimestamp: new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc));

        V1Pod newerPod = CreatePod(
            name: "app-7c9dd9f4f4-fghij",
            namespaceName: "default",
            uid: "new-pod-uid",
            ownerUid: "replicaset-uid",
            ownerName: "app-7c9dd9f4f4",
            ownerKind: "ReplicaSet",
            containers: ["app"],
            creationTimestamp: new DateTime(2026, 4, 1, 12, 5, 0, DateTimeKind.Utc));

        await workspace.Runtime.AddOrUpdateResource(olderPod);
        await workspace.Runtime.AddOrUpdateResource(newerPod);
        await workspace.Runtime.SeedResource<V1Pod>(true);

        RecordingPodLogStreamClient streamClient = new(["initial line\n", "selected line\n"]);
        using PodLogsViewModel viewModel = CreateViewModel(workspace.Runtime, streamClient);
        viewModel.Object = olderPod;
        viewModel.ContainerName = "app";

        await viewModel.Connect();
        await WaitForAsync(() => viewModel.PodSelectionItems.Count == 3);

        viewModel.SelectedPodItems.Clear();
        viewModel.SelectedPodItems.Add(viewModel.PodSelectionItems[0]);
        await WaitForAsync(() => viewModel.SelectedPodItems.Count == 1 && viewModel.SelectedPodItems[0].IsAll);

        viewModel.SelectedPodItems.Add(viewModel.PodSelectionItems[1]);

        await WaitForAsync(() => viewModel.SelectedPodItems.Count == 1 && !viewModel.SelectedPodItems[0].IsAll);
        await WaitForAsync(() => streamClient.Requests.Count >= 2);

        viewModel.SelectedPodItems[0].Pod!.Name().ShouldBe("app-7c9dd9f4f4-fghij");
        streamClient.Requests[^1].PodName.ShouldBe("app-7c9dd9f4f4-fghij");
    }

    [AvaloniaFact]
    public async Task DownloadLogs_should_export_the_current_buffer()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        RecordingPodLogExportService exportService = new();
        using PodLogsViewModel viewModel = CreateViewModel(workspace.Runtime, new RecordingPodLogStreamClient(), exportService);

        viewModel.Object = CreatePod(
            name: "app-7c9dd9f4f4-abcde",
            namespaceName: "default",
            uid: "pod-uid",
            containers: ["app"]);
        viewModel.ContainerName = "app";
        viewModel.Logs.Text = "alpha\nbeta\n";

        await viewModel.DownloadLogs();

        exportService.SuggestedFileName.ShouldBe("default-app-7c9dd9f4f4-abcde-app.log");
        exportService.Content.ShouldBe("alpha\nbeta\n");
    }

    [AvaloniaFact]
    public async Task JumpToControlledByLogs_should_enable_multi_pod_view_for_the_owner_group()
    {
        using var workspace = await Application.Current.CreateClusterAsync();

        V1Pod olderPod = CreatePod(
            name: "app-7c9dd9f4f4-abcde",
            namespaceName: "default",
            uid: "old-pod-uid",
            ownerUid: "replicaset-uid",
            ownerName: "app-7c9dd9f4f4",
            ownerKind: "ReplicaSet",
            containers: ["app", "sidecar"],
            initContainers: ["init-db"],
            creationTimestamp: new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc));

        V1Pod newerPod = CreatePod(
            name: "app-7c9dd9f4f4-fghij",
            namespaceName: "default",
            uid: "new-pod-uid",
            ownerUid: "replicaset-uid",
            ownerName: "app-7c9dd9f4f4",
            ownerKind: "ReplicaSet",
            containers: ["app", "sidecar"],
            initContainers: ["init-db"],
            creationTimestamp: new DateTime(2026, 4, 1, 12, 5, 0, DateTimeKind.Utc));

        await workspace.Runtime.AddOrUpdateResource(olderPod);
        await workspace.Runtime.AddOrUpdateResource(newerPod);
        await workspace.Runtime.SeedResource<V1Pod>(true);

        RecordingPodLogStreamClient streamClient = new(["initial line\n", "newer line\n", "older line\n"]);
        using PodLogsViewModel viewModel = CreateViewModel(workspace.Runtime, streamClient);
        viewModel.Object = olderPod;
        viewModel.ContainerName = "app";
        viewModel.ShowResourceNames = true;

        await viewModel.Connect();
        await WaitForAsync(() => streamClient.Requests.Count == 1);

        await viewModel.JumpToControlledByLogs();

        await WaitForAsync(() => streamClient.Requests.Count == 3);
        await WaitForAsync(() => viewModel.Logs.Text.Contains("[app-7c9dd9f4f4-fghij/app] newer line", StringComparison.Ordinal));

        streamClient.Requests.Count.ShouldBe(3);
        streamClient.Requests[1].PodName.ShouldBe("app-7c9dd9f4f4-fghij");
        streamClient.Requests[2].PodName.ShouldBe("app-7c9dd9f4f4-abcde");
        viewModel.SelectedContainerItems.Count.ShouldBe(1);
        viewModel.SelectedContainerItems[0].Name.ShouldBe("app");
        viewModel.CanShowResourceNames.ShouldBeTrue();
        viewModel.Logs.Text.ShouldContain("[app-7c9dd9f4f4-fghij/app] newer line");
        viewModel.Logs.Text.ShouldContain("[app-7c9dd9f4f4-abcde/app] older line");
    }

    [AvaloniaFact]
    public async Task Connect_should_leave_session_unresolved_when_no_log_session_can_be_resolved()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        V1Pod pod = CreatePod(
            name: "app-7c9dd9f4f4-abcde",
            namespaceName: "default",
            uid: "pod-uid",
            containers: ["app"]);

        RecordingPodLogStreamClient streamClient = new();
        using PodLogsViewModel viewModel = CreateViewModel(workspace.Runtime, streamClient);
        viewModel.Object = pod;
        viewModel.ContainerName = "app";

        await viewModel.Connect();

        streamClient.Requests.ShouldBeEmpty();
        viewModel.SessionResolution.ShouldBeNull();
        viewModel.PreviousLogsAvailable.ShouldBeFalse();
    }

    private static PodLogsViewModel CreateViewModel(
        IClusterRuntime runtime,
        IPodLogStreamClient streamClient,
        IPodLogExportService? exportService = null)
    {
        IServiceProvider services = Application.Current.GetTestServices();
        return new PodLogsViewModel(
            services.GetRequiredService<ILogger<PodLogsViewModel>>(),
            services.GetRequiredService<ISettingsService>(),
            exportService ?? new NoOpPodLogExportService(),
            new PodLogSessionResolver(),
            streamClient)
        {
            Cluster = runtime,
        };
    }

    private static V1Pod CreatePod(
        string name,
        string namespaceName,
        string uid,
        string? ownerUid = null,
        string? ownerName = null,
        string? ownerKind = null,
        string[]? containers = null,
        string[]? initContainers = null,
        string[]? ephemeralContainers = null,
        int restartCount = 0,
        DateTime? creationTimestamp = null)
    {
        var containerList = new List<V1Container>();
        if (containers is not null)
        {
            for (var i = 0; i < containers.Length; i++)
            {
                containerList.Add(new V1Container { Name = containers[i] });
            }
        }

        var ephemeralContainerList = new List<V1EphemeralContainer>();
        if (ephemeralContainers is not null)
        {
            for (var i = 0; i < ephemeralContainers.Length; i++)
            {
                ephemeralContainerList.Add(new V1EphemeralContainer { Name = ephemeralContainers[i] });
            }
        }

        var initContainerList = new List<V1Container>();
        if (initContainers is not null)
        {
            for (var i = 0; i < initContainers.Length; i++)
            {
                initContainerList.Add(new V1Container { Name = initContainers[i] });
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
                InitContainers = initContainerList,
                EphemeralContainers = ephemeralContainerList,
            },
            Status = new V1PodStatus
            {
                ContainerStatuses = containerStatuses,
            },
        };
    }

    private static V1ReplicaSet CreateOwnedReplicaSet(string name, string uid, V1Deployment deployment)
    {
        return new V1ReplicaSet
        {
            Metadata = new V1ObjectMeta
            {
                Name = name,
                NamespaceProperty = deployment.Namespace(),
                Uid = uid,
                OwnerReferences =
                [
                    new V1OwnerReference
                    {
                        Name = deployment.Name(),
                        Uid = deployment.Uid(),
                        Kind = V1Deployment.KubeKind,
                        Controller = true,
                    },
                ],
            },
        };
    }

    private static async Task WaitForAsync(Func<bool> predicate, int timeoutMs = 3000)
    {
        await TestWait.UntilAsync(
            predicate,
            timeoutMs,
            TestContext.Current.CancellationToken,
            () => Dispatcher.UIThread.RunJobs());
    }

    private sealed class RecordingPodLogStreamClient : IPodLogStreamClient
    {
        private readonly Queue<string> _payloads;

        public RecordingPodLogStreamClient(IEnumerable<string>? payloads = null)
        {
            _payloads = new Queue<string>(payloads ?? []);
        }

        public List<PodLogReadOptions> Requests { get; } = [];

        public Task<Stream> OpenAsync(IClusterRuntime cluster, PodLogReadOptions options, CancellationToken cancellationToken = default)
        {
            Requests.Add(options);

            var payload = _payloads.Count > 0 ? _payloads.Dequeue() : string.Empty;
            var bytes = Encoding.UTF8.GetBytes(payload.ReplaceLineEndings("\n"));
            return Task.FromResult<Stream>(new MemoryStream(bytes));
        }
    }

    private sealed class RestartingPodLogStreamClient : IPodLogStreamClient
    {
        private readonly string _firstPayload;
        private readonly string _secondPayload;

        public RestartingPodLogStreamClient(string firstPayload, string secondPayload)
        {
            _firstPayload = firstPayload;
            _secondPayload = secondPayload;
        }

        public List<PodLogReadOptions> Requests { get; } = [];

        public Task<Stream> OpenAsync(IClusterRuntime cluster, PodLogReadOptions options, CancellationToken cancellationToken = default)
        {
            Requests.Add(options);
            var payload = Requests.Count == 1 ? _firstPayload : _secondPayload;
            var bytes = Encoding.UTF8.GetBytes(payload.ReplaceLineEndings("\n"));
            return Task.FromResult<Stream>(Requests.Count == 1
                ? new NonSeekableMemoryStream(bytes)
                : new MemoryStream(bytes));
        }
    }

    private sealed class StatusChangingPodLogStreamClient : IPodLogStreamClient, IDisposable
    {
        private readonly BlockingReadStream _stream;
        private readonly byte[]? _reconnectPayload;

        public StatusChangingPodLogStreamClient(string payload, string? reconnectPayload = null)
        {
            _stream = new BlockingReadStream(Encoding.UTF8.GetBytes(payload.ReplaceLineEndings("\n")));
            _reconnectPayload = reconnectPayload is null
                ? null
                : Encoding.UTF8.GetBytes(reconnectPayload.ReplaceLineEndings("\n"));
        }

        public List<PodLogReadOptions> Requests { get; } = [];

        public Task<Stream> OpenAsync(IClusterRuntime cluster, PodLogReadOptions options, CancellationToken cancellationToken = default)
        {
            Requests.Add(options);
            return Task.FromResult<Stream>(Requests.Count == 1 || _reconnectPayload is null
                ? _stream
                : new MemoryStream(_reconnectPayload));
        }

        public void Release()
        {
            _stream.Release();
        }

        public void Dispose()
        {
            _stream.Dispose();
        }
    }

    private sealed class ReplayingPodLogStreamClient : IPodLogStreamClient
    {
        private int _requestCount;

        public List<PodLogReadOptions> Requests { get; } = [];

        public int RequestCount => Volatile.Read(ref _requestCount);

        public Task<Stream> OpenAsync(IClusterRuntime cluster, PodLogReadOptions options, CancellationToken cancellationToken = default)
        {
            Requests.Add(options);
            Interlocked.Increment(ref _requestCount);

            var payload = $"{options.ContainerName} line\n";
            var bytes = Encoding.UTF8.GetBytes(payload);
            return Task.FromResult<Stream>(new NonSeekableMemoryStream(bytes));
        }
    }

    private sealed class MultiContainerPodLogStreamClient : IPodLogStreamClient, IDisposable
    {
        private readonly List<Stream> _streams = [];

        public List<PodLogReadOptions> Requests { get; } = [];

        public Task<Stream> OpenAsync(IClusterRuntime cluster, PodLogReadOptions options, CancellationToken cancellationToken = default)
        {
            Requests.Add(options);
            Stream stream = options.ContainerName == "init-db"
                ? new NonSeekableMemoryStream(Encoding.UTF8.GetBytes("init line\n"))
                : new BlockingReadStream(Encoding.UTF8.GetBytes($"{options.ContainerName} line\n"));
            _streams.Add(stream);
            return Task.FromResult(stream);
        }

        public void Dispose()
        {
            for (var i = 0; i < _streams.Count; i++)
            {
                _streams[i].Dispose();
            }

            _streams.Clear();
        }
    }

    private sealed class BlockingReadStream : Stream
    {
        private readonly byte[] _buffer;
        private readonly TaskCompletionSource _release = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private int _position;

        public BlockingReadStream(byte[] buffer)
        {
            _buffer = buffer;
        }

        public override bool CanRead => true;

        public override bool CanSeek => false;

        public override bool CanWrite => false;

        public override long Length => _buffer.Length;

        public override long Position
        {
            get => _position;
            set => throw new NotSupportedException();
        }

        public override int Read(byte[] buffer, int offset, int count)
        {
            if (_position >= _buffer.Length)
            {
                _release.Task.GetAwaiter().GetResult();
                return 0;
            }

            var length = Math.Min(count, _buffer.Length - _position);
            Array.Copy(_buffer, _position, buffer, offset, length);
            _position += length;
            return length;
        }

        public override void Flush()
        {
        }

        public override ValueTask<int> ReadAsync(Memory<byte> buffer, CancellationToken cancellationToken = default)
        {
            if (_position < _buffer.Length)
            {
                var length = Math.Min(buffer.Length, _buffer.Length - _position);
                _buffer.AsMemory(_position, length).CopyTo(buffer);
                _position += length;
                return ValueTask.FromResult(length);
            }

            return WaitForReleaseAsync(cancellationToken);
        }

        public override Task<int> ReadAsync(byte[] buffer, int offset, int count, CancellationToken cancellationToken)
        {
            return ReadAsync(buffer.AsMemory(offset, count), cancellationToken).AsTask();
        }

        public void Release()
        {
            _release.TrySetResult();
        }

        public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();

        public override void SetLength(long value) => throw new NotSupportedException();

        public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();

        protected override void Dispose(bool disposing)
        {
            _release.TrySetResult();
            base.Dispose(disposing);
        }

        private async ValueTask<int> WaitForReleaseAsync(CancellationToken cancellationToken)
        {
            await _release.Task.WaitAsync(cancellationToken);
            return 0;
        }
    }

    private sealed class NonSeekableMemoryStream : MemoryStream
    {
        public NonSeekableMemoryStream(byte[] buffer)
            : base(buffer)
        {
        }

        public override bool CanSeek => false;

        public override long Length => throw new NotSupportedException();

        public override long Position
        {
            get => base.Position;
            set => throw new NotSupportedException();
        }

        public override long Seek(long offset, SeekOrigin loc)
        {
            throw new NotSupportedException();
        }
    }

    private sealed class BlockingPodLogStreamClient : IPodLogStreamClient
    {
        private readonly TaskCompletionSource _firstRequestStarted = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly TaskCompletionSource _releaseFirstRequest = new(TaskCreationOptions.RunContinuationsAsynchronously);
        private readonly string _payload;
        private int _requestCount;

        public BlockingPodLogStreamClient(string payload)
        {
            _payload = payload;
        }

        public List<PodLogReadOptions> Requests { get; } = [];

        public Task WaitForFirstRequestAsync()
        {
            return _firstRequestStarted.Task;
        }

        public void ReleaseFirstRequest()
        {
            _releaseFirstRequest.TrySetResult();
        }

        public async Task<Stream> OpenAsync(IClusterRuntime cluster, PodLogReadOptions options, CancellationToken cancellationToken = default)
        {
            Requests.Add(options);
            if (Interlocked.Increment(ref _requestCount) == 1)
            {
                _firstRequestStarted.TrySetResult();
                await _releaseFirstRequest.Task.WaitAsync(cancellationToken);
            }

            var bytes = Encoding.UTF8.GetBytes(_payload.ReplaceLineEndings("\n"));
            return new MemoryStream(bytes);
        }
    }

    private sealed class NoOpPodLogExportService : IPodLogExportService
    {
        public Task ExportAsync(string suggestedFileName, string content, CancellationToken cancellationToken = default)
        {
            return Task.CompletedTask;
        }
    }

    private sealed class RecordingPodLogExportService : IPodLogExportService
    {
        public string? SuggestedFileName { get; private set; }

        public string? Content { get; private set; }

        public Task ExportAsync(string suggestedFileName, string content, CancellationToken cancellationToken = default)
        {
            SuggestedFileName = suggestedFileName;
            Content = content;
            return Task.CompletedTask;
        }
    }
}
