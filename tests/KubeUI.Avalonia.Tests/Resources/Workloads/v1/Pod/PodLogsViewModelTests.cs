using System.Collections.ObjectModel;
using System.Net;
using System.Net.Http;
using System.Text;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using AvaloniaEdit.Document;
using Dock.Model.Core;
using k8s;
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
    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task Deployment_rollout_should_switch_logs_to_the_new_pod_without_refresh(KubernetesBackend backend)
    {
        const string deploymentName = "api";
        const string appLabel = "api";
        V1Deployment deployment = CreateKindDeployment(deploymentName, appLabel, "old pod line");
        V1ReplicaSet oldReplicaSet = CreateOwnedReplicaSet("api-old", "old-replicaset-uid", deployment);
        V1Pod oldPod = CreatePod(
            "api-old-pod", "default", "old-pod-uid", "old-replicaset-uid", "api-old", "ReplicaSet", ["app"],
            creationTimestamp: new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc));
        V1ReplicaSet newReplicaSet = CreateOwnedReplicaSet("api-new", "new-replicaset-uid", deployment);
        V1Pod newPod = CreatePod(
            "api-new-pod", "default", "new-pod-uid", "new-replicaset-uid", "api-new", "ReplicaSet", ["app"],
            creationTimestamp: new DateTime(2026, 4, 1, 12, 5, 0, DateTimeKind.Utc));
        oldReplicaSet.Metadata!.OwnerReferences![0].Uid = "deployment-uid";
        newReplicaSet.Metadata!.OwnerReferences![0].Uid = "deployment-uid";
        oldPod.Metadata!.Labels = new Dictionary<string, string> { ["app"] = appLabel };
        newPod.Metadata!.Labels = new Dictionary<string, string> { ["app"] = appLabel };
        oldPod.Status!.ContainerStatuses![0].State = new V1ContainerState { Running = new V1ContainerStateRunning() };
        newPod.Status!.ContainerStatuses![0].State = new V1ContainerState { Running = new V1ContainerStateRunning() };
        using var workspace = await Application.Current.CreateClusterAsync(config =>
        {
            config.Type = backend;
            config.HttpHandlerFactory = () => [new FakePodLogHandler(oldReplicaSet, oldPod, newReplicaSet, newPod, "old pod line\n", "new pod line\n")];
        });

        await workspace.Runtime.SeedResource<V1Deployment>(true);
        await workspace.Runtime.SeedResource<V1ReplicaSet>(true);
        await workspace.Runtime.SeedResource<V1Pod>(true);
        await workspace.Runtime.AddOrUpdateResource(deployment);
        await WaitForAsync(() => workspace.Runtime.GetResourceList<V1Pod>().Any(pod =>
            pod.Metadata?.Labels?.TryGetValue("app", out var value) == true && value == appLabel), 60000);
        await WaitForAsync(() => workspace.Runtime.GetResourceList<V1Pod>().Any(pod =>
            pod.Metadata?.Labels?.TryGetValue("app", out var value) == true
            && pod.Status?.ContainerStatuses?.Any(status =>
                status.Name == "app" && status.State?.Running is not null) == true), 60000);
        deployment = workspace.Runtime.GetResource<V1Deployment>("default", deploymentName).ShouldNotBeNull();

        IServiceProvider services = Application.Current.GetTestServices();
        using PodLogsViewModel viewModel = CreateViewModel(
            workspace.Runtime,
            services.GetRequiredService<IPodLogStreamClient>());
        viewModel.Object = deployment;
        viewModel.ContainerName = "app";

        await viewModel.Connect();
        await WaitForAsync(() => viewModel.Logs.Text.Contains("old pod line", StringComparison.Ordinal), 60000);

        var currentDeployment = workspace.Runtime.GetResource<V1Deployment>("default", deploymentName).ShouldNotBeNull();
        currentDeployment.Spec!.Template.Metadata ??= new V1ObjectMeta();
        currentDeployment.Spec.Template.Metadata.Annotations ??= new Dictionary<string, string>();
        currentDeployment.Spec.Template.Metadata.Annotations["podlogs.kubeui.dev/revision"] = "2";
        currentDeployment.Spec.Template.Spec!.Containers[0].Command = ["sh", "-c", "echo new pod line; sleep 300"];
        await workspace.Runtime.AddOrUpdateResource(currentDeployment);
        await WaitForAsync(() => workspace.Runtime.GetResourceList<V1Pod>().Count(pod =>
            pod.Metadata?.Labels?.TryGetValue("app", out var value) == true && value == appLabel) >= 2, 60000);
        newPod = workspace.Runtime.GetResourceList<V1Pod>().OrderByDescending(pod => pod.Metadata?.CreationTimestamp).First(pod => pod.Name() != oldPod.Name());

        await WaitForAsync(() => viewModel.Logs.Text.Contains("new pod line", StringComparison.Ordinal), 60000);
        viewModel.AvailablePods.Select(pod => pod.Name()).ShouldContain(newPod.Name());
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task Terminal_job_logs_should_not_reconnect_after_completion(KubernetesBackend backend)
    {
        const string jobName = "terminal-job";
        const string appLabel = "terminal-job";
        V1Job job = CreateTerminalJob(jobName, appLabel);
        V1Pod fakePod = CreatePod(
            "terminal-job-pod", "default", "terminal-pod-uid", "job-uid", jobName, "Job", ["job"]);
        fakePod.Metadata!.Labels = new Dictionary<string, string> { ["app"] = appLabel };
        fakePod.Status!.Phase = "Succeeded";
        fakePod.Status.ContainerStatuses![0].State = new V1ContainerState
        {
            Terminated = new V1ContainerStateTerminated { ExitCode = 0 },
        };

        using var workspace = await Application.Current.CreateClusterAsync(config =>
        {
            config.Type = backend;
            config.HttpHandlerFactory = () => [new FakeJobLogHandler(fakePod, "terminal line\n")];
        });
        await workspace.Runtime.SeedResource<V1Job>(true);
        await workspace.Runtime.SeedResource<V1Pod>(true);
        await workspace.Runtime.AddOrUpdateResource(job);
        await WaitForAsync(() => workspace.Runtime.GetResourceList<V1Pod>().Any(pod =>
            pod.Metadata?.Labels?.TryGetValue("app", out var value) == true && value == appLabel), 60000);
        await WaitForAsync(() => workspace.Runtime.GetResourceList<V1Pod>().Any(pod =>
            pod.Metadata?.Labels?.TryGetValue("app", out var value) == true
            && pod.Status?.ContainerStatuses?.Any(status =>
                status.Name == "job" && (status.State?.Running is not null || status.State?.Terminated is not null)) == true), 60000);
        job = workspace.Runtime.GetResource<V1Job>("default", jobName).ShouldNotBeNull();

        IServiceProvider services = Application.Current.GetTestServices();
        using PodLogsViewModel viewModel = CreateViewModel(
            workspace.Runtime,
            services.GetRequiredService<IPodLogStreamClient>());
        viewModel.Object = job;
        viewModel.ContainerName = "job";

        await viewModel.Connect();
        await WaitForAsync(() => viewModel.Logs.Text.Contains("terminal line", StringComparison.Ordinal), 60000);
        await WaitForAsync(() => viewModel.IsConnected == false, 10000);
        viewModel.Logs.Text.ShouldContain("terminal line");
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task Multi_container_logs_should_show_resource_names(KubernetesBackend backend)
    {
        const string podName = "multi-container-pod";
        V1Pod pod = CreatePod(podName, "default", null, containers: ["app", "sidecar"]);
        pod.Metadata!.Labels = new Dictionary<string, string> { ["app"] = podName };
        pod.Status!.Phase = "Running";
        pod.Status.ContainerStatuses =
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
        pod.Spec!.RestartPolicy = "Never";
        pod.Spec.Containers[0].Image = "busybox:1.36";
        pod.Spec.Containers[0].Command = ["sh", "-c", "echo app-line; sleep 30"];
        pod.Spec.Containers[1].Image = "busybox:1.36";
        pod.Spec.Containers[1].Command = ["sh", "-c", "echo sidecar-line; sleep 30"];

        using var workspace = await Application.Current.CreateClusterAsync(config =>
        {
            config.Type = backend;
            config.HttpHandlerFactory = () => [new FakeMultiContainerLogHandler()];
        });
        await workspace.Runtime.SeedResource<V1Pod>(true);
        await workspace.Runtime.AddOrUpdateResource(pod);
        await WaitForAsync(() => workspace.Runtime.GetResource<V1Pod>("default", podName)?.Spec?.Containers?.Count == 2, 60000);
        await WaitForAsync(() => workspace.Runtime.GetResource<V1Pod>("default", podName)?.Status?.ContainerStatuses?.Count(status =>
            status.State?.Running is not null) == 2, 60000);
        pod = workspace.Runtime.GetResourceList<V1Pod>().Single(item => item.Name() == podName);

        IServiceProvider services = Application.Current.GetTestServices();
        using PodLogsViewModel viewModel = CreateViewModel(
            workspace.Runtime,
            services.GetRequiredService<IPodLogStreamClient>());
        viewModel.Object = pod;
        viewModel.ContainerName = "app";

        await viewModel.Connect();
        await WaitForAsync(() => viewModel.ContainerSelectionItems.Count == 3, 60000);
        viewModel.SelectedContainerItems = new ObservableCollection<PodLogContainerSelectionItem>(
            [viewModel.ContainerSelectionItems[1], viewModel.ContainerSelectionItems[2]]);
        await WaitForAsync(() => viewModel.Logs.Text.Contains("app-line", StringComparison.Ordinal)
            && viewModel.Logs.Text.Contains("sidecar-line", StringComparison.Ordinal), 60000);

        viewModel.CanShowResourceNames.ShouldBeTrue();
        viewModel.ShowResourceNames = true;
        await WaitForAsync(() => viewModel.Logs.Text.Contains("[app] app-line", StringComparison.Ordinal)
            && viewModel.Logs.Text.Contains("[sidecar] sidecar-line", StringComparison.Ordinal), 60000);
        viewModel.ShowResourceNames.ShouldBeTrue();
        viewModel.Logs.Text.ShouldContain("app-line");
        viewModel.Logs.Text.ShouldContain("sidecar-line");
    }

    [AvaloniaFact]
    public async Task Connect_should_retarget_to_the_newest_matching_pod_and_stream_logs()
    {
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

        using var workspace = await Application.Current.CreateClusterAsync();
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

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task Connect_should_expose_related_pods_and_all_container_options(KubernetesBackend backend)
    {
        const string deploymentName = "related-pods-deployment";
        const string appLabel = "related-pods-deployment";
        V1Deployment deployment = CreateKindDeployment(deploymentName, appLabel, "unused");
        deployment.Spec!.Replicas = 0;

        V1Pod olderPod = CreatePod(
            name: "app-7c9dd9f4f4-abcde",
            namespaceName: "default",
            uid: null,
            ownerUid: "deployment-uid",
            ownerName: deploymentName,
            ownerKind: "Deployment",
            containers: ["app", "sidecar"],
            initContainers: ["init-db"],
            creationTimestamp: new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc));

        V1Pod newerPod = CreatePod(
            name: "app-7c9dd9f4f4-fghij",
            namespaceName: "default",
            uid: null,
            ownerUid: "deployment-uid",
            ownerName: deploymentName,
            ownerKind: "Deployment",
            containers: ["app", "sidecar"],
            initContainers: ["init-db"],
            creationTimestamp: new DateTime(2026, 4, 1, 12, 5, 0, DateTimeKind.Utc));

        foreach (var pod in new[] { olderPod, newerPod })
        {
            pod.Metadata!.Labels = new Dictionary<string, string> { ["app"] = appLabel };
            pod.Metadata.OwnerReferences![0].ApiVersion = "apps/v1";
            pod.Spec!.RestartPolicy = "Never";
            pod.Spec.InitContainers![0].Image = "busybox:1.36";
            pod.Spec.InitContainers[0].Command = ["sh", "-c", "sleep 1"];
            pod.Status!.Phase = "Running";
            pod.Status.ContainerStatuses = pod.Spec.Containers.Select(container => new V1ContainerStatus
            {
                Name = container.Name,
                State = new V1ContainerState { Running = new V1ContainerStateRunning() },
            }).ToList();
            foreach (var container in pod.Spec.Containers)
            {
                container.Image = "busybox:1.36";
                container.Command = ["sh", "-c", $"echo {pod.Name()} {container.Name} line; sleep 300"];
            }
        }

        using var workspace = await Application.Current.CreateClusterAsync(config =>
        {
            config.Type = backend;
            config.HttpHandlerFactory = () => [new FakeMultipleSelectedPodsLogHandler()];
        });
        await workspace.Runtime.SeedResource<V1Deployment>(true);
        await workspace.Runtime.SeedResource<V1Pod>(true);
        await workspace.Runtime.AddOrUpdateResource(deployment);
        await WaitForAsync(() => workspace.Runtime.GetResourceList<V1Deployment>().Any(item => item.Name() == deploymentName), 60000);
        deployment = workspace.Runtime.GetResourceList<V1Deployment>().Single(item => item.Name() == deploymentName);
        foreach (var pod in new[] { newerPod, olderPod })
        {
            pod.Metadata!.OwnerReferences![0].Uid = deployment.Uid();
            await workspace.Runtime.AddOrUpdateResource(pod);
        }
        await WaitForAsync(() => workspace.Runtime.GetResourceList<V1Pod>().Count(item =>
            item.Metadata?.Labels?.TryGetValue("app", out var value) == true && value == appLabel) == 2, 60000);
        await WaitForAsync(() => workspace.Runtime.GetResourceList<V1Pod>().Where(item =>
            item.Metadata?.Labels?.TryGetValue("app", out var value) == true && value == appLabel)
            .All(item => item.Status?.ContainerStatuses?.All(status => status.State?.Running is not null) == true), 60000);
        olderPod = workspace.Runtime.GetResourceList<V1Pod>().Single(item => item.Name() == olderPod.Name());
        newerPod = workspace.Runtime.GetResourceList<V1Pod>().Single(item => item.Name() == newerPod.Name());

        IServiceProvider services = Application.Current.GetTestServices();
        using PodLogsViewModel viewModel = CreateViewModel(
            workspace.Runtime,
            services.GetRequiredService<IPodLogStreamClient>());
        viewModel.Object = olderPod;
        viewModel.ContainerName = "app";

        await viewModel.Connect();

        viewModel.AvailablePods.Count.ShouldBe(2);
        viewModel.AvailablePods.Select(item => item.Name()).OrderBy(name => name).ShouldBe(
            ["app-7c9dd9f4f4-abcde", "app-7c9dd9f4f4-fghij"]);
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

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task Deployment_pod_should_expose_init_and_application_containers(KubernetesBackend backend)
    {
        const string deploymentName = "multi-container-deployment";
        const string appLabel = "multi-container-deployment";
        V1Deployment deployment = CreateMultiContainerDeployment(deploymentName, appLabel);
        V1ReplicaSet replicaSet = CreateOwnedReplicaSet("multi-container-deployment-rs", "multi-container-rs-uid", deployment);
        replicaSet.Metadata!.OwnerReferences![0].Uid = "deployment-uid";
        V1Pod pod = CreatePod(
            "multi-container-deployment-pod",
            "default",
            "multi-container-pod-uid",
            "multi-container-rs-uid",
            replicaSet.Name(),
            "ReplicaSet",
            ["app", "sidecar"],
            ["init-db"]);
        pod.Metadata!.Labels = new Dictionary<string, string> { ["app"] = appLabel };
        pod.Status!.Phase = "Running";
        pod.Status.ContainerStatuses =
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

        using var workspace = await Application.Current.CreateClusterAsync(config =>
        {
            config.Type = backend;
            config.HttpHandlerFactory = () => [new FakePodLogHandler(replicaSet, pod, null, null, "app line\n", "app line\n")];
        });
        await workspace.Runtime.SeedResource<V1Deployment>(true);
        await workspace.Runtime.SeedResource<V1ReplicaSet>(true);
        await workspace.Runtime.SeedResource<V1Pod>(true);
        await workspace.Runtime.AddOrUpdateResource(deployment);
        await WaitForAsync(() => workspace.Runtime.GetResourceList<V1Pod>().Any(item =>
            item.Metadata?.Labels?.TryGetValue("app", out var value) == true
            && value == appLabel), 60000);
        await WaitForAsync(() => workspace.Runtime.GetResourceList<V1Pod>().Any(item =>
            item.Metadata?.Labels?.TryGetValue("app", out var value) == true
            && item.Status?.ContainerStatuses?.Any(status =>
                status.Name == "app" && status.State?.Running is not null) == true), 60000);
        deployment = workspace.Runtime.GetResourceList<V1Deployment>().Single(item => item.Name() == deploymentName);

        IServiceProvider services = Application.Current.GetTestServices();
        using PodLogsViewModel viewModel = CreateViewModel(
            workspace.Runtime,
            services.GetRequiredService<IPodLogStreamClient>());
        viewModel.Object = deployment;
        viewModel.ContainerName = "app";

        await viewModel.Connect();
        await WaitForAsync(() => viewModel.Logs.Text.Contains("app line", StringComparison.Ordinal), 60000);

        viewModel.AvailablePods.Count.ShouldBe(1);
        viewModel.AvailableContainers.Count.ShouldBe(3);
        viewModel.AvailableContainers.Select(container => container.Name).ShouldBe(["init-db", "app", "sidecar"]);
        viewModel.AvailableContainers[0].DisplayName.ShouldBe("init-db (init)");
        viewModel.ContainerSelectionItems.Count.ShouldBe(4);
        viewModel.SelectedContainerItems.Count.ShouldBe(1);
        viewModel.SelectedContainerItems[0].Name.ShouldBe("app");
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task Connect_should_stream_multiple_selected_pods_and_containers(KubernetesBackend backend)
    {
        const string deploymentName = "multi-select-deployment";
        const string appLabel = "multi-select-deployment";
        V1Deployment deployment = CreateKindDeployment(deploymentName, appLabel, "unused");
        deployment.Spec!.Replicas = 0;

        V1Pod olderPod = CreatePod(
            name: "app-7c9dd9f4f4-abcde",
            namespaceName: "default",
            uid: null,
            ownerUid: "deployment-uid",
            ownerName: deploymentName,
            ownerKind: "Deployment",
            containers: ["app", "sidecar"],
            creationTimestamp: new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc));

        V1Pod newerPod = CreatePod(
            name: "app-7c9dd9f4f4-fghij",
            namespaceName: "default",
            uid: null,
            ownerUid: "deployment-uid",
            ownerName: deploymentName,
            ownerKind: "Deployment",
            containers: ["app", "sidecar"],
            creationTimestamp: new DateTime(2026, 4, 1, 12, 5, 0, DateTimeKind.Utc));

        V1Pod newestPod = CreatePod(
            name: "app-7c9dd9f4f4-klmno",
            namespaceName: "default",
            uid: null,
            ownerUid: "deployment-uid",
            ownerName: deploymentName,
            ownerKind: "Deployment",
            containers: ["app", "sidecar", "metrics"],
            creationTimestamp: new DateTime(2026, 4, 1, 12, 10, 0, DateTimeKind.Utc));

        foreach (var pod in new[] { olderPod, newerPod, newestPod })
        {
            pod.Metadata!.Labels = new Dictionary<string, string> { ["app"] = appLabel };
            pod.Metadata.OwnerReferences![0].ApiVersion = "apps/v1";
            pod.Spec!.RestartPolicy = "Never";
            pod.Status!.Phase = "Running";
            pod.Status.ContainerStatuses = [];
            foreach (var container in pod.Spec.Containers)
            {
                container.Image = "busybox:1.36";
                var podPrefix = pod == newerPod ? "newer" : "older";
                container.Command = ["sh", "-c", $"echo {podPrefix} {container.Name} line; sleep 300"];
                pod.Status.ContainerStatuses.Add(new V1ContainerStatus
                {
                    Name = container.Name,
                    State = new V1ContainerState { Running = new V1ContainerStateRunning() },
                });
            }
        }

        using var workspace = await Application.Current.CreateClusterAsync(config =>
        {
            config.Type = backend;
            config.HttpHandlerFactory = () => [new FakeMultipleSelectedPodsLogHandler()];
        });
        await workspace.Runtime.SeedResource<V1Deployment>(true);
        await workspace.Runtime.SeedResource<V1Pod>(true);
        await workspace.Runtime.AddOrUpdateResource(deployment);
        await WaitForAsync(() => workspace.Runtime.GetResourceList<V1Deployment>().Any(item => item.Name() == deploymentName), 60000);
        deployment = workspace.Runtime.GetResourceList<V1Deployment>().Single(item => item.Name() == deploymentName);
        foreach (var pod in new[] { olderPod, newerPod, newestPod })
        {
            pod.Metadata!.OwnerReferences![0].Uid = deployment.Uid();
            await workspace.Runtime.AddOrUpdateResource(pod);
        }
        await WaitForAsync(() => workspace.Runtime.GetResourceList<V1Pod>().Count(item =>
            item.Metadata?.Labels?.TryGetValue("app", out var value) == true && value == appLabel) == 3, 60000);
        await WaitForAsync(() => workspace.Runtime.GetResourceList<V1Pod>().Where(item =>
            item.Metadata?.Labels?.TryGetValue("app", out var value) == true && value == appLabel)
            .All(item => item.Status?.ContainerStatuses?.All(status => status.State?.Running is not null) == true), 60000);
        V1Pod[] currentPods = workspace.Runtime.GetResourceList<V1Pod>()
            .Where(item => item.Metadata?.Labels?.TryGetValue("app", out var value) == true && value == appLabel)
            .OrderBy(item => item.Metadata?.CreationTimestamp)
            .ToArray();
        olderPod = currentPods.Single(item => item.Name() == "app-7c9dd9f4f4-abcde");
        newerPod = currentPods.Single(item => item.Name() == "app-7c9dd9f4f4-fghij");
        newestPod = currentPods.Single(item => item.Name() == "app-7c9dd9f4f4-klmno");

        IServiceProvider services = Application.Current.GetTestServices();
        using PodLogsViewModel viewModel = CreateViewModel(
            workspace.Runtime,
            services.GetRequiredService<IPodLogStreamClient>());
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
        await WaitForAsync(() => viewModel.Logs.Text.Contains("older sidecar line", StringComparison.Ordinal)
            && viewModel.Logs.Text.Contains("newer app line", StringComparison.Ordinal), 60000);

        viewModel.SelectedPodItems.Count.ShouldBe(2);
        viewModel.SelectedContainerItems.Count.ShouldBe(2);
        viewModel.Logs.Text.ShouldContain("[app-7c9dd9f4f4-fghij/app] newer app line");
        viewModel.Logs.Text.ShouldContain("[app-7c9dd9f4f4-abcde/sidecar] older sidecar line");
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task Connect_should_not_open_logs_for_a_container_that_is_waiting_to_start(KubernetesBackend backend)
    {
        V1Pod pod = CreatePod(
            name: "app-7c9dd9f4f4-abcde",
            namespaceName: "default",
            uid: null,
            containers: ["app", "sidecar"]);
        pod.Spec!.RestartPolicy = "Never";
        pod.Spec.Containers[0].Image = "busybox:kubeui-never-pulled";
        pod.Spec.Containers[1].Image = "busybox:1.36";
        pod.Spec.Containers[1].Command = ["sh", "-c", "echo sidecar line; sleep 300"];
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

        using var workspace = await Application.Current.CreateClusterAsync(config =>
        {
            config.Type = backend;
            config.HttpHandlerFactory = () => [new FakeWaitingContainerLogHandler()];
        });
        await workspace.Runtime.AddOrUpdateResource(pod);
        await workspace.Runtime.SeedResource<V1Pod>(true);
        await WaitForAsync(() => workspace.Runtime.GetResourceList<V1Pod>().Any(item =>
            item.Name() == pod.Name()
            && item.Status?.ContainerStatuses?.Count == 2
            && item.Status.ContainerStatuses.Any(status => status.Name == "app" && status.State?.Waiting is not null)
            && item.Status.ContainerStatuses.Any(status => status.Name == "sidecar" && status.State?.Running is not null)), 60000);
        pod = workspace.Runtime.GetResourceList<V1Pod>().Single(item => item.Name() == pod.Name());

        IServiceProvider services = Application.Current.GetTestServices();
        using PodLogsViewModel viewModel = CreateViewModel(
            workspace.Runtime,
            services.GetRequiredService<IPodLogStreamClient>());
        viewModel.Object = pod;
        viewModel.ContainerName = "app";
        viewModel.SelectedContainerItems = new ObservableCollection<PodLogContainerSelectionItem>(
            [new PodLogContainerSelectionItem("app", "app", false, false)]);

        await viewModel.Connect();

        viewModel.IsConnected.ShouldBeFalse();
        viewModel.Logs.Text.ShouldBeEmpty();
        viewModel.Logs.Text.ShouldNotContain("app line");
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task Refresh_should_open_a_container_after_it_changes_from_waiting_to_running(KubernetesBackend backend)
    {
        V1Pod waitingPod = CreatePod(
            name: "app-7c9dd9f4f4-abcde",
            namespaceName: "default",
            uid: null,
            containers: ["app", "sidecar"]);
        waitingPod.Spec!.RestartPolicy = "Never";
        waitingPod.Spec.Containers[0].Image = "busybox:kubeui-never-pulled";
        waitingPod.Spec.Containers[0].Command = ["sh", "-c", "echo app line; sleep 300"];
        waitingPod.Spec.Containers[1].Image = "busybox:1.36";
        waitingPod.Spec.Containers[1].Command = ["sh", "-c", "echo sidecar line; sleep 300"];
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
            uid: null,
            containers: ["app", "sidecar"]);
        runningPod.Spec!.RestartPolicy = "Never";
        runningPod.Spec.Containers[0].Image = "busybox:1.36";
        runningPod.Spec.Containers[0].Command = ["sh", "-c", "echo app line; sleep 300"];
        runningPod.Spec.Containers[1].Image = "busybox:1.36";
        runningPod.Spec.Containers[1].Command = ["sh", "-c", "echo sidecar line; sleep 300"];
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

        using var workspace = await Application.Current.CreateClusterAsync(config =>
        {
            config.Type = backend;
            config.HttpHandlerFactory = () => [new FakeRefreshWaitingContainerLogHandler()];
        });
        await workspace.Runtime.AddOrUpdateResource(waitingPod);
        await workspace.Runtime.SeedResource<V1Pod>(true);
        await WaitForAsync(() => workspace.Runtime.GetResourceList<V1Pod>().Any(item =>
            item.Name() == waitingPod.Name()
            && item.Status?.ContainerStatuses?.Count == 2
            && item.Status.ContainerStatuses.Any(status => status.Name == "app" && status.State?.Waiting is not null)
            && item.Status.ContainerStatuses.Any(status => status.Name == "sidecar" && status.State?.Running is not null)), 60000);
        waitingPod = workspace.Runtime.GetResourceList<V1Pod>().Single(item => item.Name() == waitingPod.Name());
        runningPod = KubernetesJson.Deserialize<V1Pod>(KubernetesJson.Serialize(waitingPod));
        runningPod.Spec!.Containers[0].Image = "busybox:1.36";
        runningPod.Status!.ContainerStatuses!.First(status => status.Name == "app").State = new V1ContainerState
        {
            Running = new V1ContainerStateRunning(),
        };

        IServiceProvider services = Application.Current.GetTestServices();
        using PodLogsViewModel viewModel = CreateViewModel(
            workspace.Runtime,
            services.GetRequiredService<IPodLogStreamClient>());
        viewModel.Object = waitingPod;
        viewModel.ContainerName = "app";
        viewModel.SelectedContainerItems = new ObservableCollection<PodLogContainerSelectionItem>(
            [new PodLogContainerSelectionItem(string.Empty, "all", false, true)]);

        await viewModel.Connect();
        await WaitForAsync(() => viewModel.Logs.Text.Contains("sidecar line", StringComparison.Ordinal), 60000);
        viewModel.Logs.Text.ShouldNotContain("app line");

        await workspace.Runtime.AddOrUpdateResource(runningPod);
        await WaitForAsync(() => workspace.Runtime.GetResourceList<V1Pod>().Any(item =>
            item.Name() == runningPod.Name()
            && item.Status?.ContainerStatuses?.FirstOrDefault(status => status.Name == "app")?.State?.Running is not null), 60000);
        await viewModel.Refresh();

        await WaitForAsync(() => viewModel.Logs.Text.Contains("app line", StringComparison.Ordinal), 60000);
        viewModel.Logs.Text.ShouldContain("app line");
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task Show_resource_names_should_toggle_rendered_prefixes(KubernetesBackend backend)
    {
        const string deploymentName = "show-resource-names-deployment";
        const string appLabel = "show-resource-names-deployment";
        V1Deployment deployment = CreateKindDeployment(deploymentName, appLabel, "unused");
        deployment.Spec!.Replicas = 0;

        V1Pod olderPod = CreatePod(
            name: "app-7c9dd9f4f4-abcde",
            namespaceName: "default",
            uid: null,
            ownerUid: "deployment-uid",
            ownerName: deploymentName,
            ownerKind: "Deployment",
            containers: ["app", "sidecar"],
            creationTimestamp: new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc));

        V1Pod newerPod = CreatePod(
            name: "app-7c9dd9f4f4-fghij",
            namespaceName: "default",
            uid: null,
            ownerUid: "deployment-uid",
            ownerName: deploymentName,
            ownerKind: "Deployment",
            containers: ["app", "sidecar"],
            creationTimestamp: new DateTime(2026, 4, 1, 12, 5, 0, DateTimeKind.Utc));

        foreach (var pod in new[] { newerPod, olderPod })
        {
            pod.Metadata!.Labels = new Dictionary<string, string> { ["app"] = appLabel };
            pod.Metadata.OwnerReferences![0].ApiVersion = "apps/v1";
            pod.Spec!.RestartPolicy = "Never";
            pod.Status!.Phase = "Running";
            pod.Status.ContainerStatuses = [];
            foreach (var container in pod.Spec.Containers)
            {
                container.Image = "busybox:1.36";
                var podPrefix = pod == newerPod ? "newer" : "older";
                container.Command = ["sh", "-c", $"echo {podPrefix} {container.Name} line; sleep 300"];
                pod.Status.ContainerStatuses.Add(new V1ContainerStatus
                {
                    Name = container.Name,
                    State = new V1ContainerState { Running = new V1ContainerStateRunning() },
                });
            }
        }

        using var workspace = await Application.Current.CreateClusterAsync(config =>
        {
            config.Type = backend;
            config.HttpHandlerFactory = () => [new FakeMultipleSelectedPodsLogHandler()];
        });
        await workspace.Runtime.SeedResource<V1Deployment>(true);
        await workspace.Runtime.SeedResource<V1Pod>(true);
        await workspace.Runtime.AddOrUpdateResource(deployment);
        await WaitForAsync(() => workspace.Runtime.GetResourceList<V1Deployment>().Any(item => item.Name() == deploymentName), 60000);
        deployment = workspace.Runtime.GetResourceList<V1Deployment>().Single(item => item.Name() == deploymentName);
        foreach (var pod in new[] { newerPod, olderPod })
        {
            pod.Metadata!.OwnerReferences![0].Uid = deployment.Uid();
            await workspace.Runtime.AddOrUpdateResource(pod);
        }
        await WaitForAsync(() => workspace.Runtime.GetResourceList<V1Pod>().Count(item =>
            item.Metadata?.Labels?.TryGetValue("app", out var value) == true && value == appLabel) == 2, 60000);
        await WaitForAsync(() => workspace.Runtime.GetResourceList<V1Pod>().Where(item =>
            item.Metadata?.Labels?.TryGetValue("app", out var value) == true && value == appLabel)
            .All(item => item.Status?.ContainerStatuses?.All(status => status.State?.Running is not null) == true), 60000);
        olderPod = workspace.Runtime.GetResourceList<V1Pod>().Single(item => item.Name() == olderPod.Name());
        newerPod = workspace.Runtime.GetResourceList<V1Pod>().Single(item => item.Name() == newerPod.Name());

        IServiceProvider services = Application.Current.GetTestServices();
        using PodLogsViewModel viewModel = CreateViewModel(
            workspace.Runtime,
            services.GetRequiredService<IPodLogStreamClient>());
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
        viewModel.Logs.Text.ShouldContain("[app-7c9dd9f4f4-abcde/sidecar] older sidecar line");

        viewModel.ShowResourceNames = false;
        await WaitForAsync(() => !viewModel.Logs.Text.Contains("[app-7c9dd9f4f4-fghij/app]", StringComparison.Ordinal));
        viewModel.Logs.Text.ShouldContain("newer app line");
        viewModel.Logs.Text.ShouldNotContain("[app-7c9dd9f4f4-fghij/app] newer app line");
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task Changing_previous_should_restart_the_session_with_updated_log_options(KubernetesBackend backend)
    {
        V1Pod pod = CreatePod(
            name: "app-7c9dd9f4f4-abcde",
            namespaceName: "default",
            uid: null,
            containers: ["app"],
            creationTimestamp: new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc));
        pod.Spec!.RestartPolicy = "Always";
        pod.Spec.Containers[0].Image = "busybox:1.36";
        pod.Spec.Containers[0].Command =
        [
            "sh",
            "-c",
            "if [ -f /state/started ]; then echo current line; sleep 300; else touch /state/started; echo previous line; exit 1; fi",
        ];
        pod.Spec.Volumes =
        [
            new V1Volume
            {
                Name = "state",
                EmptyDir = new V1EmptyDirVolumeSource(),
            },
        ];
        pod.Spec.Containers[0].VolumeMounts =
        [
            new V1VolumeMount { Name = "state", MountPath = "/state" },
        ];
        pod.Status!.Phase = "Running";
        pod.Status.ContainerStatuses =
        [
            new V1ContainerStatus
            {
                Name = "app",
                RestartCount = 1,
                State = new V1ContainerState { Running = new V1ContainerStateRunning() },
            },
        ];

        using var workspace = await Application.Current.CreateClusterAsync(config =>
        {
            config.Type = backend;
            config.HttpHandlerFactory = () => [new FakePreviousLogsHandler()];
        });
        await workspace.Runtime.SeedResource<V1Pod>(true);
        await workspace.Runtime.AddOrUpdateResource(pod);
        await WaitForAsync(() => workspace.Runtime.GetResource<V1Pod>("default", pod.Name())?.Status?.ContainerStatuses?.Any(status =>
            status.RestartCount >= 1 && status.State?.Running is not null) == true, 60000);
        pod = workspace.Runtime.GetResource<V1Pod>("default", pod.Name()).ShouldNotBeNull();

        IServiceProvider services = Application.Current.GetTestServices();
        using PodLogsViewModel viewModel = CreateViewModel(
            workspace.Runtime,
            services.GetRequiredService<IPodLogStreamClient>());
        viewModel.Object = pod;
        viewModel.ContainerName = "app";

        await viewModel.Connect();
        await WaitForAsync(() => viewModel.Logs.Text.Contains("current line", StringComparison.Ordinal));

        viewModel.Previous = true;

        await WaitForAsync(() => viewModel.Logs.Text.Contains("previous line", StringComparison.Ordinal));

        viewModel.PreviousLogsAvailable.ShouldBeTrue();
        viewModel.SessionState.ShouldNotBeNull();
        viewModel.SessionState!.Previous.ShouldBeTrue();
        viewModel.Logs.Text.ShouldContain("previous line");
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task Refresh_should_restart_the_session_with_the_current_selection(KubernetesBackend backend)
    {
        using var workspace = await Application.Current.CreateClusterAsync(config =>
        {
            config.Type = backend;
            config.HttpHandlerFactory = () => [new FakeMultipleSelectedPodsLogHandler()];
        });
        V1Pod pod = CreatePod(
            name: "app-7c9dd9f4f4-abcde",
            namespaceName: "default",
            uid: null,
            containers: ["app"],
            restartCount: 1,
            creationTimestamp: new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc));
        ConfigureRunningContainer(pod, "app", "echo older app line; sleep 300");

        await workspace.Runtime.AddOrUpdateResource(pod);
        await workspace.Runtime.SeedResource<V1Pod>(true);
        await WaitForAsync(() => workspace.Runtime.GetResource<V1Pod>("default", pod.Name())?.Metadata?.Uid is not null, timeoutMs: 60000);
        await WaitForAsync(() => workspace.Runtime.GetResource<V1Pod>("default", pod.Name())?.Status?.ContainerStatuses?.Any(status =>
            status.Name == "app" && status.State?.Running is not null) == true, timeoutMs: 60000);
        pod = workspace.Runtime.GetResource<V1Pod>("default", pod.Name()).ShouldNotBeNull();

        using PodLogsViewModel viewModel = CreateViewModel(
            workspace.Runtime,
            Application.Current.GetTestServices().GetRequiredService<IPodLogStreamClient>());
        viewModel.Object = pod;
        viewModel.ContainerName = "app";

        await viewModel.Connect();
        await WaitForAsync(() => viewModel.Logs.Text.Contains("older app line", StringComparison.Ordinal), timeoutMs: 60000);

        await viewModel.Refresh();

        await WaitForAsync(() => viewModel.Logs.Text.Contains("older app line", StringComparison.Ordinal));

        viewModel.Object.Name().ShouldBe("app-7c9dd9f4f4-abcde");
        viewModel.ContainerName.ShouldBe("app");
        viewModel.Logs.Text.ShouldContain("older app line");
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task Refresh_should_include_a_new_related_pod_added_after_connect(KubernetesBackend backend)
    {
        const string deploymentName = "related-pod-deployment";
        const string appLabel = "related-pod-deployment";
        V1Deployment deployment = CreateKindDeployment(deploymentName, appLabel, "unused");
        deployment.Spec!.Replicas = 0;
        V1Pod originalPod = CreatePod(
            name: "app-7c9dd9f4f4-abcde",
            namespaceName: "default",
            uid: null,
            ownerUid: "deployment-uid",
            ownerName: deploymentName,
            ownerKind: "Deployment",
            containers: ["app"],
            creationTimestamp: new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc));
        originalPod.Metadata!.Labels = new Dictionary<string, string> { ["app"] = appLabel };
        V1Pod addedPod = CreatePod(
            name: "app-7c9dd9f4f4-fghij",
            namespaceName: "default",
            uid: null,
            ownerUid: "deployment-uid",
            ownerName: deploymentName,
            ownerKind: "Deployment",
            containers: ["app"],
            creationTimestamp: new DateTime(2026, 4, 1, 12, 5, 0, DateTimeKind.Utc));
        addedPod.Metadata!.Labels = new Dictionary<string, string> { ["app"] = appLabel };
        foreach (var pod in new[] { originalPod, addedPod })
        {
            pod.Metadata!.OwnerReferences![0].ApiVersion = "apps/v1";
            pod.Spec!.RestartPolicy = "Never";
            pod.Spec.Containers[0].Image = "busybox:1.36";
            var line = pod == addedPod ? "added line" : "original line";
            pod.Spec.Containers[0].Command = ["sh", "-c", $"echo {line}; sleep 300"];
            pod.Status!.Phase = "Running";
            pod.Status.ContainerStatuses![0].State = new V1ContainerState
            {
                Running = new V1ContainerStateRunning(),
            };
        }

        using var workspace = await Application.Current.CreateClusterAsync(config =>
        {
            config.Type = backend;
            config.HttpHandlerFactory = () => [new FakeAddedRelatedPodLogHandler()];
        });
        await workspace.Runtime.SeedResource<V1Deployment>(true);
        await workspace.Runtime.SeedResource<V1Pod>(true);
        await workspace.Runtime.AddOrUpdateResource(deployment);
        await WaitForAsync(() => workspace.Runtime.GetResourceList<V1Deployment>().Any(item => item.Name() == deploymentName), 60000);
        deployment = workspace.Runtime.GetResourceList<V1Deployment>().Single(item => item.Name() == deploymentName);
        foreach (var pod in new[] { originalPod, addedPod })
        {
            pod.Metadata!.OwnerReferences![0].Uid = deployment.Uid();
        }
        await workspace.Runtime.AddOrUpdateResource(originalPod);
        await WaitForAsync(() => workspace.Runtime.GetResourceList<V1Pod>().Any(item =>
            item.Name() == originalPod.Name()
            && item.Status?.ContainerStatuses?.FirstOrDefault(status => status.Name == "app")?.State?.Running is not null), 60000);
        originalPod = workspace.Runtime.GetResourceList<V1Pod>().Single(item => item.Name() == originalPod.Name());

        IServiceProvider services = Application.Current.GetTestServices();
        using PodLogsViewModel viewModel = CreateViewModel(
            workspace.Runtime,
            services.GetRequiredService<IPodLogStreamClient>());
        viewModel.Object = originalPod;
        viewModel.ContainerName = "app";

        await viewModel.Connect();
        await WaitForAsync(() => viewModel.Logs.Text.Contains("original line", StringComparison.Ordinal), 60000);

        await workspace.Runtime.AddOrUpdateResource(addedPod);
        await WaitForAsync(() => workspace.Runtime.GetResourceList<V1Pod>().Any(item =>
            item.Name() == addedPod.Name()
            && item.Status?.ContainerStatuses?.FirstOrDefault(status => status.Name == "app")?.State?.Running is not null), 60000);
        addedPod = workspace.Runtime.GetResourceList<V1Pod>().Single(item => item.Name() == addedPod.Name());
        await viewModel.Refresh();

        await WaitForAsync(() => viewModel.AvailablePods.Count == 2
            && viewModel.AvailablePods.Any(item => item.Name() == addedPod.Name()), 60000);

        viewModel.Object.Name().ShouldBe(originalPod.Name());
        viewModel.AvailablePods.Select(pod => pod.Name()).ShouldBe([addedPod.Name(), originalPod.Name()]);

        PodLogPodSelectionItem addedPodSelection = viewModel.PodSelectionItems.Single(item => item.Pod?.Name() == addedPod.Name());
        viewModel.SelectedPodItems = new ObservableCollection<PodLogPodSelectionItem>([addedPodSelection]);
        await WaitForAsync(() => viewModel.Logs.Text.Contains("added line", StringComparison.Ordinal), 60000);

        viewModel.SelectedPodItems.Count.ShouldBe(1);
        viewModel.SelectedPodItems[0].Pod!.Name().ShouldBe(addedPod.Name());
        viewModel.Logs.Text.ShouldContain("added line");
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task Refresh_should_fall_back_when_the_current_newest_pod_is_removed(KubernetesBackend backend)
    {
        const string deploymentName = "fallback-deployment";
        const string appLabel = "fallback-deployment";
        V1Deployment deployment = CreateKindDeployment(deploymentName, appLabel, "unused");
        deployment.Spec!.Replicas = 0;

        using var workspace = await Application.Current.CreateClusterAsync(config =>
        {
            config.Type = backend;
            config.HttpHandlerFactory = () => [new FakeFallbackPodLogHandler()];
        });
        await workspace.Runtime.SeedResource<V1Deployment>(true);
        await workspace.Runtime.SeedResource<V1Pod>(true);
        await workspace.Runtime.AddOrUpdateResource(deployment);
        await WaitForAsync(() => workspace.Runtime.GetResourceList<V1Deployment>().Any(item => item.Name() == deploymentName), 60000);
        deployment = workspace.Runtime.GetResourceList<V1Deployment>().Single(item => item.Name() == deploymentName);

        V1Pod remainingPod = CreatePod(
            name: "app-7c9dd9f4f4-abcde",
            namespaceName: "default",
            uid: null,
            ownerUid: deployment.Uid(),
            ownerName: deployment.Name(),
            ownerKind: "Deployment",
            containers: ["app"],
            creationTimestamp: new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc));
        remainingPod.Metadata!.Labels = new Dictionary<string, string> { ["app"] = appLabel };
        V1Pod removedPod = CreatePod(
            name: "app-7c9dd9f4f4-fghij",
            namespaceName: "default",
            uid: null,
            ownerUid: deployment.Uid(),
            ownerName: deployment.Name(),
            ownerKind: "Deployment",
            containers: ["app"],
            creationTimestamp: new DateTime(2026, 4, 1, 12, 5, 0, DateTimeKind.Utc));
        removedPod.Metadata!.Labels = new Dictionary<string, string> { ["app"] = appLabel };
        foreach (var pod in new[] { remainingPod, removedPod })
        {
            pod.Metadata!.OwnerReferences![0].ApiVersion = "apps/v1";
            pod.Spec!.RestartPolicy = "Never";
            pod.Spec.Containers[0].Image = "busybox:1.36";
            var line = pod == removedPod ? "newest line" : "fallback line";
            pod.Spec.Containers[0].Command = ["sh", "-c", $"echo {line}; sleep 300"];
            pod.Status!.Phase = "Running";
            pod.Status.ContainerStatuses![0].State = new V1ContainerState
            {
                Running = new V1ContainerStateRunning(),
            };
        }

        await workspace.Runtime.AddOrUpdateResource(remainingPod);
        await workspace.Runtime.AddOrUpdateResource(removedPod);
        await WaitForAsync(() => workspace.Runtime.GetResourceList<V1Pod>().Count(item =>
            item.Metadata?.Labels?.TryGetValue("app", out var value) == true && value == appLabel) == 2, 60000);
        await WaitForAsync(() => workspace.Runtime.GetResourceList<V1Pod>().Where(item =>
            item.Metadata?.Labels?.TryGetValue("app", out var value) == true && value == appLabel)
            .All(item => item.Status?.ContainerStatuses?.FirstOrDefault(status => status.Name == "app")?.State?.Running is not null), 60000);
        V1Pod[] relatedPods = workspace.Runtime.GetResourceList<V1Pod>()
            .Where(item => item.Metadata?.Labels?.TryGetValue("app", out var value) == true && value == appLabel)
            .OrderBy(item => item.Metadata?.CreationTimestamp)
            .ToArray();
        remainingPod = relatedPods[0];
        removedPod = relatedPods[1];
        removedPod.Metadata!.OwnerReferences![0].Uid.ShouldBe(remainingPod.Metadata!.OwnerReferences![0].Uid);

        IServiceProvider services = Application.Current.GetTestServices();
        using PodLogsViewModel viewModel = CreateViewModel(
            workspace.Runtime,
            services.GetRequiredService<IPodLogStreamClient>());
        viewModel.Object = removedPod;
        viewModel.ContainerName = "app";

        await viewModel.Connect();
        await WaitForAsync(() => viewModel.Logs.Text.Contains("newest line", StringComparison.Ordinal), 60000);

        await workspace.Runtime.DeleteResource(removedPod);
        await WaitForAsync(() => workspace.Runtime.GetResourceList<V1Pod>().All(item => item.Name() != removedPod.Name()), 60000);
        await workspace.Runtime.SeedResource<V1Pod>(true);
        await viewModel.Refresh();

        await WaitForAsync(() => viewModel.Object.Name() != removedPod.Name(), 60000);
        await WaitForAsync(() => viewModel.Logs.Text.Contains("fallback line", StringComparison.Ordinal), 60000);

        viewModel.Object.Name().ShouldBe(remainingPod.Name());
        viewModel.AvailablePods.Count.ShouldBe(1);
        viewModel.AvailablePods[0].Name().ShouldBe(remainingPod.Name());
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task Refresh_should_expose_a_new_ephemeral_container_added_to_an_existing_pod(KubernetesBackend backend)
    {
        using var workspace = await Application.Current.CreateClusterAsync(config =>
        {
            config.Type = backend;
            config.HttpHandlerFactory = () => [new FakeEphemeralContainerLogHandler()];
        });
        V1Pod pod = CreatePod(
            name: "app-7c9dd9f4f4-abcde",
            namespaceName: "default",
            uid: null,
            containers: ["app"]);
        pod.Spec!.Containers[0].Image = "busybox:1.36";
        pod.Spec.Containers[0].Command = ["sh", "-c", "echo app line; sleep 300"];
        pod.Status!.Phase = "Running";

        await workspace.Runtime.AddOrUpdateResource(pod);
        await workspace.Runtime.SeedResource<V1Pod>(true);
        await WaitForAsync(() => workspace.Runtime.GetResource<V1Pod>("default", pod.Name())?.Metadata?.Uid is not null);
        V1Pod currentPod = workspace.Runtime.GetResource<V1Pod>("default", pod.Name()).ShouldNotBeNull();

        IServiceProvider services = Application.Current.GetTestServices();
        using PodLogsViewModel viewModel = CreateViewModel(
            workspace.Runtime,
            services.GetRequiredService<IPodLogStreamClient>());
        viewModel.Object = currentPod;
        viewModel.ContainerName = "app";

        await viewModel.Connect();

        await workspace.Runtime.AddPodEphemeralDebugContainer(currentPod, "app", "busybox:1.36");
        await WaitForAsync(() => workspace.Runtime.GetResource<V1Pod>("default", pod.Name())?.Spec?.EphemeralContainers?.Count == 1);
        await viewModel.Refresh();

        await WaitForAsync(() => viewModel.ContainerSelectionItems.Count == 3);

        viewModel.AvailableContainers.Count.ShouldBe(2);
        viewModel.AvailableContainers[0].Name.ShouldBe("app");
        viewModel.AvailableContainers[1].IsEphemeralContainer.ShouldBeTrue();
        viewModel.ContainerSelectionItems.Count(item => item.IsEphemeralContainer).ShouldBe(1);
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task Current_follow_stream_ending_should_reconnect_to_continue_after_container_restart(KubernetesBackend backend)
    {
        V1Pod pod = CreatePod(
            name: "app-7c9dd9f4f4-abcde",
            namespaceName: "default",
            uid: null,
            containers: ["app"],
            creationTimestamp: new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc));
        pod.Spec!.RestartPolicy = "Always";
        pod.Spec.Containers[0].Image = "busybox:1.36";
        pod.Spec.Containers[0].Command =
        [
            "sh",
            "-c",
            "if [ -f /state/started ]; then echo after restart; sleep 300; else touch /state/started; echo before restart; sleep 5; exit 1; fi",
        ];
        pod.Spec.Volumes =
        [
            new V1Volume
            {
                Name = "state",
                EmptyDir = new V1EmptyDirVolumeSource(),
            },
        ];
        pod.Spec.Containers[0].VolumeMounts =
        [
            new V1VolumeMount { Name = "state", MountPath = "/state" },
        ];
        pod.Status!.Phase = "Running";

        using var workspace = await Application.Current.CreateClusterAsync(config =>
        {
            config.Type = backend;
            config.HttpHandlerFactory = () => [new FakeRestartingPodLogsHandler()];
        });
        await workspace.Runtime.SeedResource<V1Pod>(true);
        await workspace.Runtime.AddOrUpdateResource(pod);
        await WaitForAsync(() => workspace.Runtime.GetResource<V1Pod>("default", pod.Name())?.Status?.Phase == "Running", 60000);
        pod = workspace.Runtime.GetResource<V1Pod>("default", pod.Name()).ShouldNotBeNull();

        IServiceProvider services = Application.Current.GetTestServices();
        using PodLogsViewModel viewModel = CreateViewModel(
            workspace.Runtime,
            services.GetRequiredService<IPodLogStreamClient>());
        viewModel.Object = pod;
        viewModel.ContainerName = "app";

        await viewModel.Connect();

        await WaitForAsync(() => viewModel.Logs.Text.Contains("before restart", StringComparison.Ordinal), timeoutMs: 10000);
        await WaitForAsync(() => viewModel.Logs.Text.Contains("after restart", StringComparison.Ordinal), timeoutMs: 20000);

        viewModel.Logs.Text.ShouldContain("before restart");
        viewModel.Logs.Text.ShouldContain("after restart");
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task Terminal_pod_should_not_reconnect_after_follow_stream_ends(KubernetesBackend backend)
    {
        V1Pod pod = CreatePod(
            name: "job-completed",
            namespaceName: "default",
            uid: null,
            containers: ["job"]);
        pod.Spec!.RestartPolicy = "Never";
        pod.Spec.Containers[0].Image = "busybox:1.36";
        pod.Spec.Containers[0].Command = ["sh", "-c", "echo completed line"];
        pod.Status!.Phase = "Succeeded";
        pod.Status.ContainerStatuses![0].State = new V1ContainerState
        {
            Terminated = new V1ContainerStateTerminated { ExitCode = 0 },
        };

        using var workspace = await Application.Current.CreateClusterAsync(config =>
        {
            config.Type = backend;
            config.HttpHandlerFactory = () => [new FakeTerminalPodLogHandler()];
        });
        await workspace.Runtime.AddOrUpdateResource(pod);
        await workspace.Runtime.SeedResource<V1Pod>(true);
        await WaitForAsync(() => workspace.Runtime.GetResourceList<V1Pod>().Any(item =>
            item.Name() == pod.Name()
            && item.Status?.Phase == "Succeeded"
            && item.Status.ContainerStatuses?.FirstOrDefault(status => status.Name == "job")?.State?.Terminated is not null), 60000);
        pod = workspace.Runtime.GetResourceList<V1Pod>().Single(item => item.Name() == pod.Name());

        IServiceProvider services = Application.Current.GetTestServices();
        using PodLogsViewModel viewModel = CreateViewModel(
            workspace.Runtime,
            services.GetRequiredService<IPodLogStreamClient>());
        viewModel.Object = pod;
        viewModel.ContainerName = "job";

        await viewModel.Connect();
        await WaitForAsync(() => viewModel.Logs.Text.Contains("completed line", StringComparison.Ordinal), 60000);
        await WaitForAsync(() => viewModel.IsConnected == false, 10000);

        viewModel.IsConnected.ShouldBeFalse();
        viewModel.Logs.Text.ShouldContain("completed line");
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task Running_pod_becoming_succeeded_should_stop_follow_reconnects(KubernetesBackend backend)
    {
        V1Pod pod = CreatePod(
            name: "job-transition",
            namespaceName: "default",
            uid: null,
            containers: ["job"]);
        pod.Spec!.RestartPolicy = "Never";
        pod.Spec.Containers[0].Image = "busybox:1.36";
        pod.Spec.Containers[0].Command = ["sh", "-c", "echo running line; sleep 5; exit 0"];
        pod.Status!.Phase = "Running";
        pod.Status.ContainerStatuses![0].State = new V1ContainerState
        {
            Running = new V1ContainerStateRunning(),
        };

        using var workspace = await Application.Current.CreateClusterAsync(config =>
        {
            config.Type = backend;
            config.HttpHandlerFactory = () => [new FakeCompletingPodLogHandler(pod)];
        });
        await workspace.Runtime.SeedResource<V1Pod>(true);
        await workspace.Runtime.AddOrUpdateResource(pod);
        await WaitForAsync(() => workspace.Runtime.GetResource<V1Pod>("default", pod.Name())?.Status?.Phase == "Running", 60000);
        pod = workspace.Runtime.GetResource<V1Pod>("default", pod.Name()).ShouldNotBeNull();

        IServiceProvider services = Application.Current.GetTestServices();
        using PodLogsViewModel viewModel = CreateViewModel(
            workspace.Runtime,
            services.GetRequiredService<IPodLogStreamClient>());
        viewModel.Object = pod;
        viewModel.ContainerName = "job";

        await viewModel.Connect();
        await WaitForAsync(() => viewModel.Logs.Text.Contains("running line", StringComparison.Ordinal));

        await WaitForAsync(() => workspace.Runtime.GetResource<V1Pod>("default", pod.Name())?.Status?.Phase == "Succeeded", 60000);

        await WaitForAsync(() => viewModel.IsConnected == false, 10000);

        viewModel.IsConnected.ShouldBeFalse();
        viewModel.Logs.Text.ShouldContain("running line");
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task Ending_one_multi_container_stream_should_not_reconnect_all_streams(KubernetesBackend backend)
    {
        V1Pod pod = CreatePod(
            name: "app-7c9dd9f4f4-abcde",
            namespaceName: "default",
            uid: null,
            containers: ["app", "sidecar"],
            initContainers: ["init-db"]);
        pod.Spec!.RestartPolicy = "Never";
        pod.Spec.InitContainers![0].Image = "busybox:1.36";
        pod.Spec.InitContainers[0].Command = ["sh", "-c", "echo init line; exit 0"];
        pod.Spec.Containers[0].Image = "busybox:1.36";
        pod.Spec.Containers[0].Command = ["sh", "-c", "echo app line; exit 0"];
        pod.Spec.Containers[1].Image = "busybox:1.36";
        pod.Spec.Containers[1].Command = ["sh", "-c", "echo sidecar line; sleep 300"];
        pod.Status!.Phase = "Running";
        pod.Status.InitContainerStatuses =
        [
            new V1ContainerStatus
            {
                Name = "init-db",
                State = new V1ContainerState
                {
                    Terminated = new V1ContainerStateTerminated { ExitCode = 0 },
                },
            },
        ];
        pod.Status.ContainerStatuses =
        [
            new V1ContainerStatus
            {
                Name = "app",
                State = new V1ContainerState
                {
                    Terminated = new V1ContainerStateTerminated { ExitCode = 0 },
                },
            },
            new V1ContainerStatus
            {
                Name = "sidecar",
                State = new V1ContainerState { Running = new V1ContainerStateRunning() },
            },
        ];

        using var workspace = await Application.Current.CreateClusterAsync(config =>
        {
            config.Type = backend;
            config.HttpHandlerFactory = () => [new FakeMultiContainerEndingLogHandler()];
        });
        await workspace.Runtime.SeedResource<V1Pod>(true);
        await workspace.Runtime.AddOrUpdateResource(pod);
        await WaitForAsync(() => workspace.Runtime.GetResource<V1Pod>("default", pod.Name())?.Status?.ContainerStatuses?.Any(status =>
            status.Name == "sidecar" && status.State?.Running is not null) == true, 60000);
        pod = workspace.Runtime.GetResource<V1Pod>("default", pod.Name()).ShouldNotBeNull();

        IServiceProvider services = Application.Current.GetTestServices();
        using PodLogsViewModel viewModel = CreateViewModel(
            workspace.Runtime,
            services.GetRequiredService<IPodLogStreamClient>());
        viewModel.Object = pod;
        viewModel.ContainerName = "app";
        viewModel.SelectedContainerItems = new ObservableCollection<PodLogContainerSelectionItem>(
            [new PodLogContainerSelectionItem(string.Empty, "all", false, true)]);

        await viewModel.Connect();

        await WaitForAsync(() => viewModel.Logs.Text.Contains("init line", StringComparison.Ordinal)
            && viewModel.Logs.Text.Contains("app line", StringComparison.Ordinal)
            && viewModel.Logs.Text.Contains("sidecar line", StringComparison.Ordinal), 10000);
        await WaitForAsync(() => CountOccurrences(viewModel.Logs.Text, "init line") == 1
            && CountOccurrences(viewModel.Logs.Text, "app line") == 1
            && CountOccurrences(viewModel.Logs.Text, "sidecar line") == 1, 5000);

        viewModel.Logs.Text.ShouldContain("init line");
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
    public async Task Connect_should_not_create_undo_history()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        V1Pod pod = CreatePod(
            name: "app",
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

        await WaitForAsync(() => viewModel.Logs.Text.Contains("line", StringComparison.Ordinal));

        viewModel.Logs.UndoStack.CanUndo.ShouldBeFalse();
    }

    [AvaloniaFact]
    public async Task Dispose_should_clear_and_replace_the_log_document()
    {
        using var workspace = await Application.Current.CreateClusterAsync();
        V1Pod pod = CreatePod(
            name: "app",
            namespaceName: "default",
            uid: "pod-uid",
            containers: ["app"]);
        await workspace.Runtime.AddOrUpdateResource(pod);
        await workspace.Runtime.SeedResource<V1Pod>(true);
        RecordingPodLogStreamClient streamClient = new(["line\n"]);
        using PodLogsViewModel viewModel = CreateViewModel(workspace.Runtime, streamClient);
        viewModel.Object = pod;
        viewModel.ContainerName = "app";
        TextDocument previousLogs = viewModel.Logs;

        await viewModel.Connect();
        await WaitForAsync(() => viewModel.Logs.Text.Contains("line", StringComparison.Ordinal));

        viewModel.Dispose();

        previousLogs.Text.ShouldBeEmpty();
        viewModel.Logs.ShouldNotBeSameAs(previousLogs);
        viewModel.Logs.Text.ShouldBeEmpty();
        viewModel.Logs.UndoStack.CanUndo.ShouldBeFalse();
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

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task Connect_should_disable_resource_names_in_single_pod_single_container_mode(KubernetesBackend backend)
    {
        using var workspace = await Application.Current.CreateClusterAsync(config =>
        {
            config.Type = backend;
            config.HttpHandlerFactory = () => [new FakeMultipleSelectedPodsLogHandler()];
        });
        V1Pod pod = CreatePod(
            name: "app-7c9dd9f4f4-abcde",
            namespaceName: "default",
            uid: null,
            containers: ["app"]);
        ConfigureRunningContainer(pod, "app", "echo older app line; sleep 300");

        await workspace.Runtime.AddOrUpdateResource(pod);
        await workspace.Runtime.SeedResource<V1Pod>(true);
        await WaitForAsync(() => workspace.Runtime.GetResource<V1Pod>("default", pod.Name())?.Metadata?.Uid is not null, timeoutMs: 60000);
        await WaitForAsync(() => workspace.Runtime.GetResource<V1Pod>("default", pod.Name())?.Status?.ContainerStatuses?.Any(status =>
            status.Name == "app" && status.State?.Running is not null) == true, timeoutMs: 60000);
        pod = workspace.Runtime.GetResource<V1Pod>("default", pod.Name()).ShouldNotBeNull();

        using PodLogsViewModel viewModel = CreateViewModel(
            workspace.Runtime,
            Application.Current.GetTestServices().GetRequiredService<IPodLogStreamClient>());
        viewModel.Object = pod;
        viewModel.ContainerName = "app";

        await viewModel.Connect();

        await WaitForAsync(() => viewModel.Logs.Text.Contains("older app line", StringComparison.Ordinal), timeoutMs: 60000);
        viewModel.CanShowResourceNames.ShouldBeFalse();
        viewModel.ShowResourceNames.ShouldBeFalse();
        viewModel.Logs.Text.ShouldContain("older app line");
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task Selecting_more_than_one_container_should_enable_resource_names(KubernetesBackend backend)
    {
        using var workspace = await Application.Current.CreateClusterAsync(config =>
        {
            config.Type = backend;
            config.HttpHandlerFactory = () => [new FakeMultipleSelectedPodsLogHandler()];
        });
        V1Pod pod = CreatePod(
            name: "app-7c9dd9f4f4-abcde",
            namespaceName: "default",
            uid: null,
            containers: ["app", "sidecar"]);
        ConfigureRunningContainer(pod, "app", "echo older app line; sleep 300");
        ConfigureRunningContainer(pod, "sidecar", "echo older sidecar line; sleep 300");

        await workspace.Runtime.AddOrUpdateResource(pod);
        await workspace.Runtime.SeedResource<V1Pod>(true);
        await WaitForAsync(() => workspace.Runtime.GetResource<V1Pod>("default", pod.Name())?.Metadata?.Uid is not null, timeoutMs: 60000);
        await WaitForAsync(() => workspace.Runtime.GetResource<V1Pod>("default", pod.Name())?.Status?.ContainerStatuses?.Count == 2
            && workspace.Runtime.GetResource<V1Pod>("default", pod.Name())!.Status.ContainerStatuses.All(status => status.State?.Running is not null), timeoutMs: 60000);
        pod = workspace.Runtime.GetResource<V1Pod>("default", pod.Name()).ShouldNotBeNull();

        using PodLogsViewModel viewModel = CreateViewModel(
            workspace.Runtime,
            Application.Current.GetTestServices().GetRequiredService<IPodLogStreamClient>());
        viewModel.Object = pod;
        viewModel.ContainerName = "app";

        await viewModel.Connect();
        await WaitForAsync(() => viewModel.ContainerSelectionItems.Count == 3, timeoutMs: 60000);

        viewModel.SelectedContainerItems = new ObservableCollection<PodLogContainerSelectionItem>(
            [viewModel.ContainerSelectionItems[1], viewModel.ContainerSelectionItems[2]]);

        viewModel.SelectedContainerItems.Count.ShouldBe(2);
        viewModel.CanShowResourceNames.ShouldBeTrue();
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task Selecting_all_containers_should_not_throw_and_should_normalize_selection(KubernetesBackend backend)
    {
        using var workspace = await Application.Current.CreateClusterAsync(config =>
        {
            config.Type = backend;
            config.HttpHandlerFactory = () => [new FakeMultipleSelectedPodsLogHandler()];
        });
        V1Pod pod = CreatePod(
            name: "app-7c9dd9f4f4-abcde",
            namespaceName: "default",
            uid: null,
            containers: ["app", "sidecar"]);
        ConfigureRunningContainer(pod, "app", "echo older app line; sleep 300");
        ConfigureRunningContainer(pod, "sidecar", "echo older sidecar line; sleep 300");

        await workspace.Runtime.AddOrUpdateResource(pod);
        await workspace.Runtime.SeedResource<V1Pod>(true);
        await WaitForAsync(() => workspace.Runtime.GetResource<V1Pod>("default", pod.Name())?.Metadata?.Uid is not null, timeoutMs: 60000);
        await WaitForAsync(() => workspace.Runtime.GetResource<V1Pod>("default", pod.Name())?.Status?.ContainerStatuses?.Count == 2
            && workspace.Runtime.GetResource<V1Pod>("default", pod.Name())!.Status.ContainerStatuses.All(status => status.State?.Running is not null), timeoutMs: 60000);
        pod = workspace.Runtime.GetResource<V1Pod>("default", pod.Name()).ShouldNotBeNull();

        using PodLogsViewModel viewModel = CreateViewModel(
            workspace.Runtime,
            Application.Current.GetTestServices().GetRequiredService<IPodLogStreamClient>());
        viewModel.Object = pod;
        viewModel.ContainerName = "app";

        await viewModel.Connect();

        await WaitForAsync(() => viewModel.ContainerSelectionItems.Count == 3, timeoutMs: 60000);

        viewModel.SelectedContainerItems = new ObservableCollection<PodLogContainerSelectionItem>(
            [viewModel.ContainerSelectionItems[1]]);

        Should.NotThrow(() => viewModel.SelectedContainerItems.Add(viewModel.ContainerSelectionItems[0]));

        await WaitForAsync(() => viewModel.SelectedContainerItems.Count == 1 && viewModel.SelectedContainerItems[0].IsAll, timeoutMs: 60000);

        viewModel.SelectedContainerItems.Count.ShouldBe(1);
        viewModel.SelectedContainerItems[0].IsAll.ShouldBeTrue();
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task Selecting_a_specific_container_should_uncheck_all_containers_and_stream_only_selected_containers(KubernetesBackend backend)
    {
        using var workspace = await Application.Current.CreateClusterAsync(config =>
        {
            config.Type = backend;
            config.HttpHandlerFactory = () => [new FakeMultipleSelectedPodsLogHandler()];
        });
        V1Pod pod = CreatePod(
            name: "app-7c9dd9f4f4-abcde",
            namespaceName: "default",
            uid: null,
            containers: ["app", "sidecar"]);
        ConfigureRunningContainer(pod, "app", "echo older app line; sleep 300");
        ConfigureRunningContainer(pod, "sidecar", "echo older sidecar line; sleep 300");

        await workspace.Runtime.AddOrUpdateResource(pod);
        await workspace.Runtime.SeedResource<V1Pod>(true);

        using PodLogsViewModel viewModel = CreateViewModel(
            workspace.Runtime,
            Application.Current.GetTestServices().GetRequiredService<IPodLogStreamClient>());
        viewModel.Object = pod;
        viewModel.ContainerName = "app";

        await viewModel.Connect();
        await WaitForAsync(() => viewModel.ContainerSelectionItems.Count == 3, timeoutMs: 60000);

        viewModel.SelectedContainerItems.Clear();
        viewModel.SelectedContainerItems.Add(viewModel.ContainerSelectionItems[0]);
        await WaitForAsync(() => viewModel.SelectedContainerItems.Count == 1 && viewModel.SelectedContainerItems[0].IsAll, timeoutMs: 60000);
        viewModel.SelectedContainerItems.Add(viewModel.ContainerSelectionItems[2]);

        await WaitForAsync(() => viewModel.SelectedContainerItems.Count == 1 && !viewModel.SelectedContainerItems[0].IsAll, timeoutMs: 60000);

        viewModel.SelectedContainerItems[0].Name.ShouldBe("sidecar");
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task Selecting_all_pods_should_select_only_the_all_item_and_stream_every_pod(KubernetesBackend backend)
    {
        const string deploymentName = "select-all-pods-deployment";
        const string appLabel = "select-all-pods-deployment";
        V1Deployment deployment = CreateKindDeployment(deploymentName, appLabel, "unused");
        deployment.Spec!.Replicas = 0;

        V1Pod olderPod = CreatePod(
            name: "app-7c9dd9f4f4-abcde",
            namespaceName: "default",
            uid: null,
            ownerUid: "deployment-uid",
            ownerName: deploymentName,
            ownerKind: "Deployment",
            containers: ["app"],
            creationTimestamp: new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc));

        V1Pod newerPod = CreatePod(
            name: "app-7c9dd9f4f4-fghij",
            namespaceName: "default",
            uid: null,
            ownerUid: "deployment-uid",
            ownerName: deploymentName,
            ownerKind: "Deployment",
            containers: ["app"],
            creationTimestamp: new DateTime(2026, 4, 1, 12, 5, 0, DateTimeKind.Utc));

        foreach (var pod in new[] { olderPod, newerPod })
        {
            pod.Metadata!.Labels = new Dictionary<string, string> { ["app"] = appLabel };
            pod.Metadata.OwnerReferences![0].ApiVersion = "apps/v1";
            pod.Spec!.RestartPolicy = "Never";
            ConfigureRunningContainer(pod, "app", $"echo {(pod == newerPod ? "newer" : "older")} line; sleep 300");
        }

        using var workspace = await Application.Current.CreateClusterAsync(config =>
        {
            config.Type = backend;
            config.HttpHandlerFactory = () => [new FakeMultipleSelectedPodsLogHandler()];
        });
        await workspace.Runtime.SeedResource<V1Deployment>(true);
        await workspace.Runtime.SeedResource<V1Pod>(true);
        await workspace.Runtime.AddOrUpdateResource(deployment);
        await WaitForAsync(() => workspace.Runtime.GetResourceList<V1Deployment>().Any(item => item.Name() == deploymentName), 60000);
        deployment = workspace.Runtime.GetResourceList<V1Deployment>().Single(item => item.Name() == deploymentName);
        foreach (var pod in new[] { olderPod, newerPod })
        {
            pod.Metadata!.OwnerReferences![0].Uid = deployment.Uid();
            await workspace.Runtime.AddOrUpdateResource(pod);
        }
        await WaitForAsync(() => workspace.Runtime.GetResourceList<V1Pod>().Count(item =>
            item.Metadata?.Labels?.TryGetValue("app", out var value) == true && value == appLabel) == 2, 60000);
        await WaitForAsync(() => workspace.Runtime.GetResourceList<V1Pod>().Where(item =>
            item.Metadata?.Labels?.TryGetValue("app", out var value) == true && value == appLabel)
            .All(item => item.Status?.ContainerStatuses?.Any(status => status.State?.Running is not null) == true), 60000);
        olderPod = workspace.Runtime.GetResourceList<V1Pod>().Single(item => item.Name() == olderPod.Name());
        newerPod = workspace.Runtime.GetResourceList<V1Pod>().Single(item => item.Name() == newerPod.Name());

        using PodLogsViewModel viewModel = CreateViewModel(
            workspace.Runtime,
            Application.Current.GetTestServices().GetRequiredService<IPodLogStreamClient>());
        viewModel.Object = olderPod;
        viewModel.ContainerName = "app";

        await viewModel.Connect();
        await WaitForAsync(() => viewModel.PodSelectionItems.Count == 3, timeoutMs: 60000);

        viewModel.SelectedPodItems.Add(viewModel.PodSelectionItems[0]);

        await WaitForAsync(() => viewModel.SelectedPodItems.Count == 1 && viewModel.SelectedPodItems[0].IsAll, timeoutMs: 60000);

        viewModel.SelectedPodItems.Count.ShouldBe(1);
        viewModel.SelectedPodItems[0].IsAll.ShouldBeTrue();
    }

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task Selecting_a_specific_pod_should_uncheck_all_pods_and_stream_only_selected_pods(KubernetesBackend backend)
    {
        const string deploymentName = "select-specific-pod-deployment";
        const string appLabel = "select-specific-pod-deployment";
        V1Deployment deployment = CreateKindDeployment(deploymentName, appLabel, "unused");
        deployment.Spec!.Replicas = 0;

        V1Pod olderPod = CreatePod(
            name: "app-7c9dd9f4f4-abcde",
            namespaceName: "default",
            uid: null,
            ownerUid: "deployment-uid",
            ownerName: deploymentName,
            ownerKind: "Deployment",
            containers: ["app"],
            creationTimestamp: new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc));

        V1Pod newerPod = CreatePod(
            name: "app-7c9dd9f4f4-fghij",
            namespaceName: "default",
            uid: null,
            ownerUid: "deployment-uid",
            ownerName: deploymentName,
            ownerKind: "Deployment",
            containers: ["app"],
            creationTimestamp: new DateTime(2026, 4, 1, 12, 5, 0, DateTimeKind.Utc));

        foreach (var pod in new[] { olderPod, newerPod })
        {
            pod.Metadata!.Labels = new Dictionary<string, string> { ["app"] = appLabel };
            pod.Metadata.OwnerReferences![0].ApiVersion = "apps/v1";
            pod.Spec!.RestartPolicy = "Never";
            ConfigureRunningContainer(pod, "app", $"echo {(pod == newerPod ? "newer" : "older")} line; sleep 300");
        }

        using var workspace = await Application.Current.CreateClusterAsync(config =>
        {
            config.Type = backend;
            config.HttpHandlerFactory = () => [new FakeMultipleSelectedPodsLogHandler()];
        });
        await workspace.Runtime.SeedResource<V1Deployment>(true);
        await workspace.Runtime.SeedResource<V1Pod>(true);
        await workspace.Runtime.AddOrUpdateResource(deployment);
        await WaitForAsync(() => workspace.Runtime.GetResourceList<V1Deployment>().Any(item => item.Name() == deploymentName), 60000);
        deployment = workspace.Runtime.GetResourceList<V1Deployment>().Single(item => item.Name() == deploymentName);
        foreach (var pod in new[] { olderPod, newerPod })
        {
            pod.Metadata!.OwnerReferences![0].Uid = deployment.Uid();
            await workspace.Runtime.AddOrUpdateResource(pod);
        }
        await WaitForAsync(() => workspace.Runtime.GetResourceList<V1Pod>().Count(item =>
            item.Metadata?.Labels?.TryGetValue("app", out var value) == true && value == appLabel) == 2, 60000);
        await WaitForAsync(() => workspace.Runtime.GetResourceList<V1Pod>().Where(item =>
            item.Metadata?.Labels?.TryGetValue("app", out var value) == true && value == appLabel)
            .All(item => item.Status?.ContainerStatuses?.Any(status => status.State?.Running is not null) == true), 60000);
        olderPod = workspace.Runtime.GetResourceList<V1Pod>().Single(item => item.Name() == olderPod.Name());
        newerPod = workspace.Runtime.GetResourceList<V1Pod>().Single(item => item.Name() == newerPod.Name());

        using PodLogsViewModel viewModel = CreateViewModel(
            workspace.Runtime,
            Application.Current.GetTestServices().GetRequiredService<IPodLogStreamClient>());
        viewModel.Object = olderPod;
        viewModel.ContainerName = "app";

        await viewModel.Connect();
        await WaitForAsync(() => viewModel.PodSelectionItems.Count == 3, timeoutMs: 60000);

        viewModel.SelectedPodItems.Clear();
        viewModel.SelectedPodItems.Add(viewModel.PodSelectionItems[0]);
        await WaitForAsync(() => viewModel.SelectedPodItems.Count == 1 && viewModel.SelectedPodItems[0].IsAll, timeoutMs: 60000);

        viewModel.SelectedPodItems.Add(viewModel.PodSelectionItems.Single(item => item.Pod?.Name() == newerPod.Name()));

        await WaitForAsync(() => viewModel.SelectedPodItems.Count == 1 && !viewModel.SelectedPodItems[0].IsAll, timeoutMs: 60000);

        viewModel.SelectedPodItems[0].Pod!.Name().ShouldBe("app-7c9dd9f4f4-fghij");
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

    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task JumpToControlledByLogs_should_enable_multi_pod_view_for_the_owner_group(KubernetesBackend backend)
    {
        const string deploymentName = "jump-controlled-by-deployment";
        const string appLabel = "jump-controlled-by-deployment";
        V1Deployment deployment = CreateKindDeployment(deploymentName, appLabel, "unused");
        deployment.Spec!.Replicas = 0;

        V1Pod olderPod = CreatePod(
            name: "app-7c9dd9f4f4-abcde",
            namespaceName: "default",
            uid: null,
            ownerUid: "deployment-uid",
            ownerName: deploymentName,
            ownerKind: "Deployment",
            containers: ["app", "sidecar"],
            creationTimestamp: new DateTime(2026, 4, 1, 12, 0, 0, DateTimeKind.Utc));

        V1Pod newerPod = CreatePod(
            name: "app-7c9dd9f4f4-fghij",
            namespaceName: "default",
            uid: null,
            ownerUid: "deployment-uid",
            ownerName: deploymentName,
            ownerKind: "Deployment",
            containers: ["app", "sidecar"],
            creationTimestamp: new DateTime(2026, 4, 1, 12, 5, 0, DateTimeKind.Utc));

        foreach (var pod in new[] { olderPod, newerPod })
        {
            pod.Metadata!.Labels = new Dictionary<string, string> { ["app"] = appLabel };
            pod.Metadata.OwnerReferences![0].ApiVersion = "apps/v1";
            pod.Spec!.RestartPolicy = "Never";
            var prefix = pod == newerPod ? "newer" : "older";
            ConfigureRunningContainer(pod, "app", $"echo {prefix} app line; sleep 300");
            ConfigureRunningContainer(pod, "sidecar", $"echo {prefix} sidecar line; sleep 300");
        }

        using var workspace = await Application.Current.CreateClusterAsync(config =>
        {
            config.Type = backend;
            config.HttpHandlerFactory = () => [new FakeMultipleSelectedPodsLogHandler()];
        });
        await workspace.Runtime.SeedResource<V1Deployment>(true);
        await workspace.Runtime.SeedResource<V1Pod>(true);
        await workspace.Runtime.AddOrUpdateResource(deployment);
        await WaitForAsync(() => workspace.Runtime.GetResourceList<V1Deployment>().Any(item => item.Name() == deploymentName), 60000);
        deployment = workspace.Runtime.GetResourceList<V1Deployment>().Single(item => item.Name() == deploymentName);
        foreach (var pod in new[] { olderPod, newerPod })
        {
            pod.Metadata!.OwnerReferences![0].Uid = deployment.Uid();
            await workspace.Runtime.AddOrUpdateResource(pod);
        }
        await WaitForAsync(() => workspace.Runtime.GetResourceList<V1Pod>().Count(item =>
            item.Metadata?.Labels?.TryGetValue("app", out var value) == true && value == appLabel) == 2, 60000);
        await WaitForAsync(() => workspace.Runtime.GetResourceList<V1Pod>().Where(item =>
            item.Metadata?.Labels?.TryGetValue("app", out var value) == true && value == appLabel)
            .All(item => item.Status?.ContainerStatuses?.Count == 2
                && item.Status.ContainerStatuses.All(status => status.State?.Running is not null)), 60000);
        olderPod = workspace.Runtime.GetResourceList<V1Pod>().Single(item => item.Name() == olderPod.Name());
        newerPod = workspace.Runtime.GetResourceList<V1Pod>().Single(item => item.Name() == newerPod.Name());

        using PodLogsViewModel viewModel = CreateViewModel(
            workspace.Runtime,
            Application.Current.GetTestServices().GetRequiredService<IPodLogStreamClient>());
        viewModel.Object = olderPod;
        viewModel.ContainerName = "app";
        viewModel.ShowResourceNames = true;

        await viewModel.Connect();
        await WaitForAsync(() => viewModel.Logs.Text.Contains("older app line", StringComparison.Ordinal), timeoutMs: 60000);

        await viewModel.JumpToControlledByLogs();

        await WaitForAsync(() => viewModel.Logs.Text.Contains("[app-7c9dd9f4f4-fghij/app] newer app line", StringComparison.Ordinal)
            && viewModel.Logs.Text.Contains("[app-7c9dd9f4f4-abcde/app] older app line", StringComparison.Ordinal), timeoutMs: 60000);

        viewModel.SelectedContainerItems.Count.ShouldBe(1);
        viewModel.SelectedContainerItems[0].Name.ShouldBe("app");
        viewModel.CanShowResourceNames.ShouldBeTrue();
        viewModel.Logs.Text.ShouldContain("[app-7c9dd9f4f4-fghij/app] newer app line");
        viewModel.Logs.Text.ShouldContain("[app-7c9dd9f4f4-abcde/app] older app line");
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

    private static V1Deployment CreateKindDeployment(string name, string appLabel, string message)
    {
        return new V1Deployment
        {
            ApiVersion = "apps/v1",
            Kind = V1Deployment.KubeKind,
            Metadata = new V1ObjectMeta { Name = name, NamespaceProperty = "default" },
            Spec = new V1DeploymentSpec
            {
                Replicas = 1,
                Selector = new V1LabelSelector { MatchLabels = new Dictionary<string, string> { ["app"] = appLabel } },
                Template = new V1PodTemplateSpec
                {
                    Metadata = new V1ObjectMeta { Labels = new Dictionary<string, string> { ["app"] = appLabel } },
                    Spec = new V1PodSpec
                    {
                        Containers =
                        [
                            new V1Container
                            {
                                Name = "app",
                                Image = "busybox:1.36",
                                Command = ["sh", "-c", $"echo {message}; sleep 300"],
                            },
                        ],
                    },
                },
            },
        };
    }

    private static V1Deployment CreateMultiContainerDeployment(string name, string appLabel)
    {
        return new V1Deployment
        {
            ApiVersion = "apps/v1",
            Kind = V1Deployment.KubeKind,
            Metadata = new V1ObjectMeta { Name = name, NamespaceProperty = "default" },
            Spec = new V1DeploymentSpec
            {
                Replicas = 1,
                Selector = new V1LabelSelector { MatchLabels = new Dictionary<string, string> { ["app"] = appLabel } },
                Template = new V1PodTemplateSpec
                {
                    Metadata = new V1ObjectMeta { Labels = new Dictionary<string, string> { ["app"] = appLabel } },
                    Spec = new V1PodSpec
                    {
                        InitContainers =
                        [
                            new V1Container
                            {
                                Name = "init-db",
                                Image = "busybox:1.36",
                                Command = ["sh", "-c", "echo init line; sleep 1"],
                            },
                        ],
                        Containers =
                        [
                            new V1Container
                            {
                                Name = "app",
                                Image = "busybox:1.36",
                                Command = ["sh", "-c", "echo app line; sleep 300"],
                            },
                            new V1Container
                            {
                                Name = "sidecar",
                                Image = "busybox:1.36",
                                Command = ["sh", "-c", "echo sidecar line; sleep 300"],
                            },
                        ],
                    },
                },
            },
        };
    }

    private static V1Job CreateTerminalJob(string name, string appLabel)
    {
        return new V1Job
        {
            ApiVersion = "batch/v1",
            Kind = V1Job.KubeKind,
            Metadata = new V1ObjectMeta { Name = name, NamespaceProperty = "default" },
            Spec = new V1JobSpec
            {
                BackoffLimit = 0,
                Template = new V1PodTemplateSpec
                {
                    Metadata = new V1ObjectMeta { Labels = new Dictionary<string, string> { ["app"] = appLabel } },
                    Spec = new V1PodSpec
                    {
                        RestartPolicy = "Never",
                        Containers =
                        [
                            new V1Container
                            {
                                Name = "job",
                                Image = "busybox:1.36",
                                Command = ["sh", "-c", "echo terminal line"],
                            },
                        ],
                    },
                },
            },
        };
    }

    private static V1Pod CreatePod(
        string name,
        string namespaceName,
        string? uid,
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
            ApiVersion = "v1",
            Kind = V1Pod.KubeKind,
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

    private static void ConfigureRunningContainer(V1Pod pod, string containerName, string command)
    {
        V1Container container = pod.Spec!.Containers.Single(item => item.Name == containerName);
        container.Image = "busybox:1.36";
        container.Command = ["sh", "-c", command];
        pod.Status!.Phase = "Running";
        pod.Status.ContainerStatuses ??= [];
        V1ContainerStatus? status = pod.Status.ContainerStatuses.SingleOrDefault(item => item.Name == containerName);
        if (status is null)
        {
            pod.Status.ContainerStatuses.Add(new V1ContainerStatus { Name = containerName });
            status = pod.Status.ContainerStatuses[^1];
        }

        status.State = new V1ContainerState { Running = new V1ContainerStateRunning() };
    }

    private static V1ReplicaSet CreateOwnedReplicaSet(string name, string? uid, V1Deployment deployment)
    {
        return new V1ReplicaSet
        {
            ApiVersion = "apps/v1",
            Kind = V1ReplicaSet.KubeKind,
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

    private static int CountOccurrences(string text, string value)
    {
        var count = 0;
        var offset = 0;
        while ((offset = text.IndexOf(value, offset, StringComparison.Ordinal)) >= 0)
        {
            count++;
            offset += value.Length;
        }

        return count;
    }

    private sealed class FakeMultiContainerEndingLogHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Method == HttpMethod.Get
                && request.RequestUri?.AbsolutePath.EndsWith("/log", StringComparison.Ordinal) == true)
            {
                var container = request.RequestUri.Query.Contains("container=init-db", StringComparison.Ordinal)
                    ? "init-db"
                    : request.RequestUri.Query.Contains("container=sidecar", StringComparison.Ordinal)
                        ? "sidecar"
                        : "app";
                Stream content = container == "sidecar"
                    ? new BlockingReadStream(Encoding.UTF8.GetBytes("sidecar line\n"))
                    : new NonSeekableMemoryStream(Encoding.UTF8.GetBytes(container == "init-db" ? "init line\n" : "app line\n"));
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    RequestMessage = request,
                    Content = new StreamContent(content),
                });
            }

            return base.SendAsync(request, cancellationToken);
        }
    }

    private sealed class FakeCompletingPodLogHandler : DelegatingHandler
    {
        private readonly V1Pod _pod;
        private int _completed;

        public FakeCompletingPodLogHandler(V1Pod pod)
        {
            _pod = pod;
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Method == HttpMethod.Get
                && request.RequestUri?.AbsolutePath.EndsWith("/log", StringComparison.Ordinal) == true)
            {
                if (Interlocked.Exchange(ref _completed, 1) == 0 && InnerHandler is not null)
                {
                    V1Pod completedPod = KubernetesJson.Deserialize<V1Pod>(KubernetesJson.Serialize(_pod));
                    completedPod.Status ??= new V1PodStatus();
                    completedPod.Status.Phase = "Succeeded";
                    completedPod.Status.ContainerStatuses ??= [];
                    completedPod.Status.ContainerStatuses[0].State = new V1ContainerState
                    {
                        Terminated = new V1ContainerStateTerminated { ExitCode = 0 },
                    };
                    using HttpRequestMessage update = new(
                        HttpMethod.Put,
                        $"http://fake-kubernetes/api/v1/namespaces/{completedPod.Namespace()}/pods/{completedPod.Name()}")
                    {
                        Content = new StringContent(
                            KubernetesJson.Serialize(completedPod),
                            Encoding.UTF8,
                            "application/json"),
                    };
                    using HttpMessageInvoker invoker = new(InnerHandler, disposeHandler: false);
                    await invoker.SendAsync(update, cancellationToken).ConfigureAwait(false);
                }

                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    RequestMessage = request,
                    Content = new StreamContent(new NonSeekableMemoryStream(Encoding.UTF8.GetBytes("running line\n"))),
                };
            }

            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }

    private sealed class FakeRestartingPodLogsHandler : DelegatingHandler
    {
        private int _requestCount;

        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Method == HttpMethod.Get
                && request.RequestUri?.AbsolutePath.EndsWith("/log", StringComparison.Ordinal) == true)
            {
                var payload = Interlocked.Increment(ref _requestCount) == 1
                    ? "before restart\n"
                    : "after restart\n";
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    RequestMessage = request,
                    Content = new StreamContent(new NonSeekableMemoryStream(Encoding.UTF8.GetBytes(payload))),
                });
            }

            return base.SendAsync(request, cancellationToken);
        }
    }

    private sealed class FakePreviousLogsHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Method == HttpMethod.Get
                && request.RequestUri?.AbsolutePath.EndsWith("/log", StringComparison.Ordinal) == true)
            {
                var payload = request.RequestUri.Query.Contains("previous=true", StringComparison.Ordinal)
                    ? "previous line\n"
                    : "current line\n";
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    RequestMessage = request,
                    Content = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes(payload))),
                });
            }

            return base.SendAsync(request, cancellationToken);
        }
    }

    private sealed class FakeEphemeralContainerLogHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (request.Method == HttpMethod.Get
                && request.RequestUri?.AbsolutePath.EndsWith("/log", StringComparison.Ordinal) == true)
            {
                var container = request.RequestUri.Query.Contains("container=debug-", StringComparison.Ordinal)
                    ? "debug line\n"
                    : "app line\n";
                return Task.FromResult(new HttpResponseMessage(HttpStatusCode.OK)
                {
                    RequestMessage = request,
                    Content = new StringContent(container, Encoding.UTF8, "text/plain"),
                });
            }

            return base.SendAsync(request, cancellationToken);
        }
    }

    private sealed class FakeMultiContainerLogHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var path = request.RequestUri?.AbsolutePath ?? string.Empty;
            if (request.Method == HttpMethod.Post
                && path.EndsWith("/pods", StringComparison.Ordinal)
                && request.Content is not null)
            {
                var pod = KubernetesJson.Deserialize<V1Pod>(
                    await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false));
                pod.Metadata ??= new V1ObjectMeta();
                pod.Metadata.Uid = "multi-pod-uid";
                request.Content = new StringContent(
                    KubernetesJson.Serialize(pod),
                    Encoding.UTF8,
                    "application/json");
            }

            if (request.Method == HttpMethod.Get && path.EndsWith("/log", StringComparison.Ordinal))
            {
                var payload = request.RequestUri?.Query.Contains("container=sidecar", StringComparison.Ordinal) == true
                    ? "sidecar-line\n"
                    : "app-line\n";
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    RequestMessage = request,
                    Content = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes(payload))),
                };
            }

            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }

    private sealed class FakeWaitingContainerLogHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var path = request.RequestUri?.AbsolutePath ?? string.Empty;
            if (request.Method == HttpMethod.Post
                && path.EndsWith("/pods", StringComparison.Ordinal)
                && request.Content is not null)
            {
                var pod = KubernetesJson.Deserialize<V1Pod>(
                    await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false));
                pod.Metadata ??= new V1ObjectMeta();
                pod.Metadata.Uid = "pod-uid";
                request.Content = new StringContent(
                    KubernetesJson.Serialize(pod),
                    Encoding.UTF8,
                    "application/json");
            }

            if (request.Method == HttpMethod.Get && path.EndsWith("/log", StringComparison.Ordinal))
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    RequestMessage = request,
                    Content = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes("sidecar line\n"))),
                };
            }

            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }

    private sealed class FakeRefreshWaitingContainerLogHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var path = request.RequestUri?.AbsolutePath ?? string.Empty;
            if ((request.Method == HttpMethod.Post || request.Method == HttpMethod.Put)
                && path.Contains("/pods", StringComparison.Ordinal)
                && request.Content is not null)
            {
                var pod = KubernetesJson.Deserialize<V1Pod>(
                    await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false));
                pod.Metadata ??= new V1ObjectMeta();
                pod.Metadata.Uid ??= "pod-uid";
                request.Content = new StringContent(
                    KubernetesJson.Serialize(pod),
                    Encoding.UTF8,
                    "application/json");
            }

            if (request.Method == HttpMethod.Get && path.EndsWith("/log", StringComparison.Ordinal))
            {
                var payload = request.RequestUri?.Query.Contains("container=app", StringComparison.Ordinal) == true
                    ? "app line\n"
                    : "sidecar line\n";
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    RequestMessage = request,
                    Content = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes(payload))),
                };
            }

            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }

    private sealed class FakeTerminalPodLogHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var path = request.RequestUri?.AbsolutePath ?? string.Empty;
            if (request.Method == HttpMethod.Post
                && path.EndsWith("/pods", StringComparison.Ordinal)
                && request.Content is not null)
            {
                var pod = KubernetesJson.Deserialize<V1Pod>(
                    await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false));
                pod.Metadata ??= new V1ObjectMeta();
                pod.Metadata.Uid = "completed-pod-uid";
                request.Content = new StringContent(
                    KubernetesJson.Serialize(pod),
                    Encoding.UTF8,
                    "application/json");
            }

            if (request.Method == HttpMethod.Get && path.EndsWith("/log", StringComparison.Ordinal))
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    RequestMessage = request,
                    Content = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes("completed line\n"))),
                };
            }

            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }

    private sealed class FakeFallbackPodLogHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var path = request.RequestUri?.AbsolutePath ?? string.Empty;
            if ((request.Method == HttpMethod.Post || request.Method == HttpMethod.Put)
                && request.Content is not null)
            {
                if (path.Contains("/deployments", StringComparison.Ordinal))
                {
                    var deployment = KubernetesJson.Deserialize<V1Deployment>(
                        await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false));
                    deployment.Metadata ??= new V1ObjectMeta();
                    deployment.Metadata.Uid = "deployment-uid";
                    request.Content = new StringContent(
                        KubernetesJson.Serialize(deployment),
                        Encoding.UTF8,
                        "application/json");
                }
                else if (path.Contains("/pods", StringComparison.Ordinal))
                {
                    var pod = KubernetesJson.Deserialize<V1Pod>(
                        await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false));
                    pod.Metadata ??= new V1ObjectMeta();
                    pod.Metadata.Uid = $"{pod.Metadata.Name}-uid";
                    request.Content = new StringContent(
                        KubernetesJson.Serialize(pod),
                        Encoding.UTF8,
                        "application/json");
                }
            }

            if (request.Method == HttpMethod.Get && path.EndsWith("/log", StringComparison.Ordinal))
            {
                var payload = path.Contains("app-7c9dd9f4f4-fghij", StringComparison.Ordinal)
                    ? "newest line\n"
                    : "fallback line\n";
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    RequestMessage = request,
                    Content = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes(payload))),
                };
            }

            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }

    private sealed class FakeAddedRelatedPodLogHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var path = request.RequestUri?.AbsolutePath ?? string.Empty;
            if ((request.Method == HttpMethod.Post || request.Method == HttpMethod.Put)
                && request.Content is not null)
            {
                if (path.Contains("/deployments", StringComparison.Ordinal))
                {
                    var deployment = KubernetesJson.Deserialize<V1Deployment>(
                        await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false));
                    deployment.Metadata ??= new V1ObjectMeta();
                    deployment.Metadata.Uid = "deployment-uid";
                    request.Content = new StringContent(
                        KubernetesJson.Serialize(deployment),
                        Encoding.UTF8,
                        "application/json");
                }
                else if (path.Contains("/pods", StringComparison.Ordinal))
                {
                    var pod = KubernetesJson.Deserialize<V1Pod>(
                        await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false));
                    pod.Metadata ??= new V1ObjectMeta();
                    pod.Metadata.Uid = $"{pod.Metadata.Name}-uid";
                    request.Content = new StringContent(
                        KubernetesJson.Serialize(pod),
                        Encoding.UTF8,
                        "application/json");
                }
            }

            if (request.Method == HttpMethod.Get && path.EndsWith("/log", StringComparison.Ordinal))
            {
                var payload = path.Contains("app-7c9dd9f4f4-fghij", StringComparison.Ordinal)
                    ? "added line\n"
                    : "original line\n";
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    RequestMessage = request,
                    Content = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes(payload))),
                };
            }

            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }

    private sealed class FakeMultipleSelectedPodsLogHandler : DelegatingHandler
    {
        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var path = request.RequestUri?.AbsolutePath ?? string.Empty;
            if ((request.Method == HttpMethod.Post || request.Method == HttpMethod.Put)
                && request.Content is not null)
            {
                if (path.Contains("/deployments", StringComparison.Ordinal))
                {
                    var deployment = KubernetesJson.Deserialize<V1Deployment>(
                        await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false));
                    deployment.Metadata ??= new V1ObjectMeta();
                    deployment.Metadata.Uid = "deployment-uid";
                    request.Content = new StringContent(
                        KubernetesJson.Serialize(deployment),
                        Encoding.UTF8,
                        "application/json");
                }
                else if (path.Contains("/pods", StringComparison.Ordinal))
                {
                    var pod = KubernetesJson.Deserialize<V1Pod>(
                        await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false));
                    pod.Metadata ??= new V1ObjectMeta();
                    pod.Metadata.Uid = $"{pod.Metadata.Name}-uid";
                    request.Content = new StringContent(
                        KubernetesJson.Serialize(pod),
                        Encoding.UTF8,
                        "application/json");
                }
            }

            if (request.Method == HttpMethod.Get && path.EndsWith("/log", StringComparison.Ordinal))
            {
                var podPrefix = path.Contains("app-7c9dd9f4f4-fghij", StringComparison.Ordinal) ? "newer" : "older";
                var container = request.RequestUri?.Query.Contains("container=sidecar", StringComparison.Ordinal) == true
                    ? "sidecar"
                    : request.RequestUri?.Query.Contains("container=metrics", StringComparison.Ordinal) == true
                        ? "metrics"
                        : "app";
                var payload = $"{podPrefix} {container} line\n";
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    RequestMessage = request,
                    Content = new StreamContent(new MemoryStream(Encoding.UTF8.GetBytes(payload))),
                };
            }

            return await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }

    private sealed class FakeJobLogHandler : DelegatingHandler
    {
        private readonly V1Pod _pod;
        private readonly byte[] _payload;
        private int _jobWrites;

        public FakeJobLogHandler(V1Pod pod, string payload)
        {
            _pod = pod;
            _payload = Encoding.UTF8.GetBytes(payload.ReplaceLineEndings("\n"));
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var path = request.RequestUri?.AbsolutePath ?? string.Empty;
            var isJobWrite = path.Contains("/jobs", StringComparison.Ordinal)
                && (request.Method == HttpMethod.Post || request.Method == HttpMethod.Put);
            if (isJobWrite && Volatile.Read(ref _jobWrites) == 0 && request.Content is not null)
            {
                var job = KubernetesJson.Deserialize<V1Job>(
                    await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false));
                job.Metadata ??= new V1ObjectMeta();
                job.Metadata.Uid = "job-uid";
                request.Content = new StringContent(
                    KubernetesJson.Serialize(job),
                    Encoding.UTF8,
                    "application/json");
            }
            if (request.Method == HttpMethod.Get && path.EndsWith("/log", StringComparison.Ordinal))
            {
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    RequestMessage = request,
                    Content = new StreamContent(new MemoryStream(_payload, writable: false)),
                };
            }

            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if (isJobWrite && Interlocked.Increment(ref _jobWrites) == 1)
            {
                await SendResourceAsync(_pod, cancellationToken).ConfigureAwait(false);
            }

            return response;
        }

        private async Task SendResourceAsync(V1Pod pod, CancellationToken cancellationToken)
        {
            using HttpRequestMessage request = new(
                HttpMethod.Post,
                "http://fake-kubernetes/api/v1/namespaces/default/pods")
            {
                Content = new StringContent(KubernetesJson.Serialize(pod), Encoding.UTF8, "application/json"),
            };
            using HttpMessageInvoker invoker = new(InnerHandler!, disposeHandler: false);
            await invoker.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }

    private sealed class FakePodLogHandler : DelegatingHandler
    {
        private readonly V1ReplicaSet _oldReplicaSet;
        private readonly V1Pod _oldPod;
        private readonly V1ReplicaSet? _newReplicaSet;
        private readonly V1Pod? _newPod;
        private readonly byte[] _oldPayload;
        private readonly byte[] _newPayload;
        private int _deploymentWrites;

        public FakePodLogHandler(
            V1ReplicaSet oldReplicaSet,
            V1Pod oldPod,
            V1ReplicaSet? newReplicaSet,
            V1Pod? newPod,
            string oldPayload,
            string newPayload)
        {
            _oldReplicaSet = oldReplicaSet;
            _oldPod = oldPod;
            _newReplicaSet = newReplicaSet;
            _newPod = newPod;
            _oldPayload = Encoding.UTF8.GetBytes(oldPayload.ReplaceLineEndings("\n"));
            _newPayload = Encoding.UTF8.GetBytes(newPayload.ReplaceLineEndings("\n"));
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var path = request.RequestUri?.AbsolutePath ?? string.Empty;
            var isDeploymentWrite = path.Contains("/deployments", StringComparison.Ordinal)
                && (request.Method == HttpMethod.Post || request.Method == HttpMethod.Put);
            if (isDeploymentWrite && Volatile.Read(ref _deploymentWrites) == 0 && request.Content is not null)
            {
                var deployment = KubernetesJson.Deserialize<V1Deployment>(
                    await request.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false));
                deployment.Metadata ??= new V1ObjectMeta();
                deployment.Metadata.Uid = "deployment-uid";
                request.Content = new StringContent(
                    KubernetesJson.Serialize(deployment),
                    Encoding.UTF8,
                    "application/json");
            }
            if (request.Method == HttpMethod.Get && path.EndsWith("/log", StringComparison.Ordinal))
            {
                var podName = path.Split('/', StringSplitOptions.RemoveEmptyEntries)[^2];
                var payload = podName.Contains("new", StringComparison.Ordinal) ? _newPayload : _oldPayload;
                return new HttpResponseMessage(HttpStatusCode.OK)
                {
                    RequestMessage = request,
                    Content = new StreamContent(new MemoryStream(payload, writable: false)),
                };
            }

            var response = await base.SendAsync(request, cancellationToken).ConfigureAwait(false);
            if (isDeploymentWrite)
            {
                if (Interlocked.Increment(ref _deploymentWrites) == 1)
                {
                    await SendResourceAsync(_oldReplicaSet, cancellationToken).ConfigureAwait(false);
                    await SendResourceAsync(_oldPod, cancellationToken).ConfigureAwait(false);
                }
                else
                {
                    if (_newReplicaSet is not null && _newPod is not null)
                    {
                        await SendResourceAsync(_newReplicaSet, cancellationToken).ConfigureAwait(false);
                        await SendResourceAsync(_newPod, cancellationToken).ConfigureAwait(false);
                    }
                }
            }

            return response;
        }

        private async Task SendResourceAsync<T>(T resource, CancellationToken cancellationToken)
            where T : IKubernetesObject<V1ObjectMeta>
        {
            var isPod = resource is V1Pod;
            using HttpRequestMessage request = new(
                HttpMethod.Post,
                $"http://fake-kubernetes/{(isPod ? "api/v1" : "apis/apps/v1")}/namespaces/default/{(isPod ? "pods" : "replicasets")}")
            {
                Content = new StringContent(KubernetesJson.Serialize(resource), Encoding.UTF8, "application/json"),
            };
            using HttpMessageInvoker invoker = new(InnerHandler!, disposeHandler: false);
            await invoker.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }
    }

    private sealed class SynchronizedList<T> : IReadOnlyList<T>
    {
        private readonly object _gate = new();
        private readonly List<T> _items = [];

        public int Count
        {
            get
            {
                lock (_gate)
                {
                    return _items.Count;
                }
            }
        }

        public T this[int index]
        {
            get
            {
                lock (_gate)
                {
                    return _items[index];
                }
            }
        }

        public void Add(T item)
        {
            lock (_gate)
            {
                _items.Add(item);
            }
        }

        public IEnumerator<T> GetEnumerator()
        {
            lock (_gate)
            {
                return _items.ToArray().AsEnumerable().GetEnumerator();
            }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }

    private sealed class RecordingPodLogStreamClient : IPodLogStreamClient
    {
        private readonly Queue<string> _payloads;

        public RecordingPodLogStreamClient(IEnumerable<string>? payloads = null)
        {
            _payloads = new Queue<string>(payloads ?? []);
        }

        public SynchronizedList<PodLogReadOptions> Requests { get; } = [];

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

        public SynchronizedList<PodLogReadOptions> Requests { get; } = [];

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

        public SynchronizedList<PodLogReadOptions> Requests { get; } = [];

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

        public SynchronizedList<PodLogReadOptions> Requests { get; } = [];

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

        public SynchronizedList<PodLogReadOptions> Requests { get; } = [];

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

        public SynchronizedList<PodLogReadOptions> Requests { get; } = [];

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
