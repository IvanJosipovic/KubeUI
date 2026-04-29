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

public sealed class PodLogsViewModelTests : AvaloniaTestBase
{
    [AvaloniaFact]
    public async Task Connect_should_retarget_to_the_newest_matching_pod_and_stream_logs()
    {
        TestCluster runtime = new();
        ClusterWorkspaceViewModel workspace = runtime.CreateWorkspace();

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

        await runtime.AddOrUpdateResource(replacementPod);

        RecordingPodLogStreamClient streamClient = new(["first line\nsecond line\n"]);
        PodLogsViewModel viewModel = CreateViewModel(workspace, streamClient);
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
        TestCluster runtime = new();
        ClusterWorkspaceViewModel workspace = runtime.CreateWorkspace();

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

        await runtime.AddOrUpdateResource(olderPod);
        await runtime.AddOrUpdateResource(newerPod);

        RecordingPodLogStreamClient streamClient = new(["newer line\n"]);
        PodLogsViewModel viewModel = CreateViewModel(workspace, streamClient);
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
        TestCluster runtime = new();
        ClusterWorkspaceViewModel workspace = runtime.CreateWorkspace();

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

        await runtime.AddOrUpdateResource(olderPod);
        await runtime.AddOrUpdateResource(newerPod);
        await runtime.AddOrUpdateResource(newestPod);

        RecordingPodLogStreamClient streamClient = new(["newer app line\n", "newer sidecar line\n", "older app line\n", "older sidecar line\n"]);
        PodLogsViewModel viewModel = CreateViewModel(workspace, streamClient);
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
    public async Task Show_resource_names_should_toggle_rendered_prefixes()
    {
        TestCluster runtime = new();
        ClusterWorkspaceViewModel workspace = runtime.CreateWorkspace();

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

        await runtime.AddOrUpdateResource(olderPod);
        await runtime.AddOrUpdateResource(newerPod);

        RecordingPodLogStreamClient streamClient = new(["newer app line\n", "older app line\n"]);
        PodLogsViewModel viewModel = CreateViewModel(workspace, streamClient);
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
        TestCluster runtime = new();
        ClusterWorkspaceViewModel workspace = runtime.CreateWorkspace();
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

        await runtime.AddOrUpdateResource(pod);

        RecordingPodLogStreamClient streamClient = new(["current line\n", "previous line\n"]);
        PodLogsViewModel viewModel = CreateViewModel(workspace, streamClient);
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
        TestCluster runtime = new();
        ClusterWorkspaceViewModel workspace = runtime.CreateWorkspace();
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

        await runtime.AddOrUpdateResource(pod);

        RecordingPodLogStreamClient streamClient = new(["initial line\n", "refreshed line\n"]);
        PodLogsViewModel viewModel = CreateViewModel(workspace, streamClient);
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
    public void Dispose_should_be_idempotent()
    {
        TestCluster runtime = new();
        ClusterWorkspaceViewModel workspace = runtime.CreateWorkspace();
        PodLogsViewModel viewModel = CreateViewModel(workspace, new RecordingPodLogStreamClient());

        viewModel.Dispose();
        Should.NotThrow(() => viewModel.Dispose());
    }

    [AvaloniaFact]
    public async Task Selecting_all_while_connecting_should_queue_a_follow_up_reconnect()
    {
        TestCluster runtime = new();
        ClusterWorkspaceViewModel workspace = runtime.CreateWorkspace();

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

        await runtime.AddOrUpdateResource(olderPod);
        await runtime.AddOrUpdateResource(newerPod);

        BlockingPodLogStreamClient streamClient = new("initial line\n");
        PodLogsViewModel viewModel = CreateViewModel(workspace, streamClient);
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
    public void JumpToPresent_should_request_live_following()
    {
        TestCluster runtime = new();
        ClusterWorkspaceViewModel workspace = runtime.CreateWorkspace();
        PodLogsViewModel viewModel = CreateViewModel(workspace, new RecordingPodLogStreamClient());

        viewModel.JumpToPresent();

        viewModel.AutoScrollToBottom.ShouldBeTrue();
        viewModel.JumpToPresentRequested.ShouldBeTrue();
    }

    [AvaloniaFact]
    public async Task Connect_should_disable_resource_names_in_single_pod_single_container_mode()
    {
        TestCluster runtime = new();
        ClusterWorkspaceViewModel workspace = runtime.CreateWorkspace();
        V1Pod pod = CreatePod(
            name: "app-7c9dd9f4f4-abcde",
            namespaceName: "default",
            uid: "pod-uid",
            containers: ["app"]);

        await runtime.AddOrUpdateResource(pod);

        RecordingPodLogStreamClient streamClient = new(["line\n"]);
        PodLogsViewModel viewModel = CreateViewModel(workspace, streamClient);
        viewModel.Object = pod;
        viewModel.ContainerName = "app";

        await viewModel.Connect();

        await WaitForAsync(() => viewModel.Logs.Text == "line");
        viewModel.CanShowResourceNames.ShouldBeFalse();
        viewModel.ShowResourceNames.ShouldBeFalse();
        viewModel.Logs.Text.ShouldBe("line");
    }

    [AvaloniaFact]
    public async Task Selecting_all_containers_should_not_throw_and_should_normalize_selection()
    {
        TestCluster runtime = new();
        ClusterWorkspaceViewModel workspace = runtime.CreateWorkspace();
        V1Pod pod = CreatePod(
            name: "app-7c9dd9f4f4-abcde",
            namespaceName: "default",
            uid: "pod-uid",
            containers: ["app", "sidecar"]);

        await runtime.AddOrUpdateResource(pod);

        RecordingPodLogStreamClient streamClient = new(["line\n"]);
        PodLogsViewModel viewModel = CreateViewModel(workspace, streamClient);
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
    public async Task DownloadLogs_should_export_the_current_buffer()
    {
        TestCluster runtime = new();
        ClusterWorkspaceViewModel workspace = runtime.CreateWorkspace();
        RecordingPodLogExportService exportService = new();
        PodLogsViewModel viewModel = CreateViewModel(workspace, new RecordingPodLogStreamClient(), exportService);

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
        TestCluster runtime = new();
        ClusterWorkspaceViewModel workspace = runtime.CreateWorkspace();

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

        await runtime.AddOrUpdateResource(olderPod);
        await runtime.AddOrUpdateResource(newerPod);

        RecordingPodLogStreamClient streamClient = new(["initial line\n", "newer line\n", "older line\n"]);
        PodLogsViewModel viewModel = CreateViewModel(workspace, streamClient);
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
    public async Task Connect_should_report_when_no_log_session_can_be_resolved()
    {
        TestCluster runtime = new();
        ClusterWorkspaceViewModel workspace = runtime.CreateWorkspace();
        V1Pod pod = CreatePod(
            name: "app-7c9dd9f4f4-abcde",
            namespaceName: "default",
            uid: "pod-uid",
            containers: ["app"]);

        RecordingPodLogStreamClient streamClient = new();
        PodLogsViewModel viewModel = CreateViewModel(workspace, streamClient);
        viewModel.Object = pod;
        viewModel.ContainerName = "app";

        await viewModel.Connect();

        streamClient.Requests.ShouldBeEmpty();
        viewModel.SessionResolution.ShouldBeNull();
        viewModel.PreviousLogsAvailable.ShouldBeFalse();
        viewModel.StatusMessage.ShouldNotBeNull();
    }

    private static PodLogsViewModel CreateViewModel(
        ClusterWorkspaceViewModel workspace,
        IPodLogStreamClient streamClient,
        IPodLogExportService? exportService = null)
    {
        IServiceProvider services = TestApp.CurrentServices ?? throw new InvalidOperationException("Test services are not initialized.");
        return new PodLogsViewModel(
            services.GetRequiredService<ILogger<PodLogsViewModel>>(),
            services.GetRequiredService<ISettingsService>(),
            exportService ?? new NoOpPodLogExportService(),
            new PodLogSessionResolver(),
            streamClient)
        {
            Cluster = workspace,
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

        List<V1Container> initContainerList = [];
        if (initContainers is not null)
        {
            for (int i = 0; i < initContainers.Length; i++)
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
            },
            Status = new V1PodStatus
            {
                ContainerStatuses = containerStatuses,
            },
        };
    }

    private static async Task WaitForAsync(Func<bool> predicate, int timeoutMs = 3000)
    {
        System.Diagnostics.Stopwatch stopwatch = System.Diagnostics.Stopwatch.StartNew();
        while (stopwatch.ElapsedMilliseconds < timeoutMs)
        {
            Dispatcher.UIThread.RunJobs();
            if (predicate())
            {
                return;
            }

            await Task.Delay(25);
        }

        Dispatcher.UIThread.RunJobs();
        predicate().ShouldBeTrue();
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

            string payload = _payloads.Count > 0 ? _payloads.Dequeue() : string.Empty;
            byte[] bytes = Encoding.UTF8.GetBytes(payload.ReplaceLineEndings("\n"));
            return Task.FromResult<Stream>(new MemoryStream(bytes));
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

            byte[] bytes = Encoding.UTF8.GetBytes(_payload.ReplaceLineEndings("\n"));
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
