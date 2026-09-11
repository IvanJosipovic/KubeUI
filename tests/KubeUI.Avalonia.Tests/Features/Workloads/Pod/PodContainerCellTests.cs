using System.Globalization;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Media;
using Avalonia.Threading;
using k8s.Models;
using KubeUI.Avalonia.Converters;
using KubeUI.Avalonia.Resources.Workloads.v1.Pod;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.Workloads.Pod;

public sealed class PodContainerCellTests
{
    [AvaloniaFact]
    public async Task pod_container_cell_resubscribes_after_recycling()
    {
        var cluster = await Application.Current.CreateClusterAsync();
        await cluster.Runtime.SeedResource<V1Pod>(true);

        var pod = new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "container-status-pod",
                NamespaceProperty = "default",
            },
            Spec = new V1PodSpec
            {
                Containers = [new V1Container { Name = "app", Image = "app:latest" }],
            },
            Status = new V1PodStatus
            {
                ContainerStatuses =
                [
                    new V1ContainerStatus
                    {
                        Name = "app",
                        State = new V1ContainerState { Running = new V1ContainerStateRunning() },
                    },
                ],
            },
        };
        await cluster.Runtime.AddOrUpdateResource(pod);

        var cell = new PodContainerCellView();
        cell.Initialize(cluster);
        cell.DataContext = pod;
        var window = new Window { Content = cell };
        window.Show();
        await TestApplicationExtensions.WaitForUiAsync();

        cell.ContainerStatuses.ShouldNotBeNull();
        cell.ContainerStatuses.Single().Status.ShouldBe("Running");

        window.Content = null;
        await TestApplicationExtensions.WaitForUiAsync();
        window.Content = cell;
        await TestApplicationExtensions.WaitForUiAsync();

        var updatedPod = new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = pod.Name(),
                NamespaceProperty = pod.Namespace(),
                Uid = pod.Metadata!.Uid,
            },
            Spec = pod.Spec,
            Status = new V1PodStatus
            {
                ContainerStatuses =
                [
                    new V1ContainerStatus
                    {
                        Name = "app",
                        State = new V1ContainerState
                        {
                            Waiting = new V1ContainerStateWaiting { Reason = "ImagePullBackOff" },
                        },
                    },
                ],
            },
        };
        await cluster.Runtime.AddOrUpdateResource(updatedPod);

        await TestWait.UntilAsync(
            () => cell.ContainerStatuses.Single().Status == "ImagePullBackOff",
            5000,
            TestContext.Current.CancellationToken,
            () => Dispatcher.UIThread.RunJobs());

        cell.ContainerStatuses.Single().Status.ShouldBe("ImagePullBackOff");
        window.Close();
    }

    [AvaloniaFact]
    public async Task pod_status_cell_resubscribes_after_recycling()
    {
        var cluster = await Application.Current.CreateClusterAsync();
        await cluster.Runtime.SeedResource<V1Pod>(true);

        var pod = new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "status-pod",
                NamespaceProperty = "default",
            },
            Status = new V1PodStatus
            {
                Conditions = [new V1PodCondition { Type = "Ready", Status = "True" }],
            },
        };
        await cluster.Runtime.AddOrUpdateResource(pod);

        var cell = new PodStatusCellView();
        cell.Initialize(cluster);
        cell.DataContext = pod;
        var window = new Window { Content = cell };
        window.Show();
        await TestApplicationExtensions.WaitForUiAsync();

        cell.Text.ShouldBe(Assets.Resources.PodStatusCell_Running);

        window.Content = null;
        await TestApplicationExtensions.WaitForUiAsync();
        window.Content = cell;
        await TestApplicationExtensions.WaitForUiAsync();

        var terminatingPod = new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = pod.Name(),
                NamespaceProperty = pod.Namespace(),
                Uid = pod.Metadata!.Uid,
                DeletionTimestamp = DateTime.UtcNow,
            },
            Status = pod.Status,
        };
        await cluster.Runtime.AddOrUpdateResource(terminatingPod);

        await TestWait.UntilAsync(
            () => cell.Text == Assets.Resources.PodStatusCell_Terminating,
            5000,
            TestContext.Current.CancellationToken,
            () => Dispatcher.UIThread.RunJobs());

        cell.Text.ShouldBe(Assets.Resources.PodStatusCell_Terminating);
        window.Close();
    }

    [AvaloniaFact]
    public void container_status_brush_resolves_from_application_theme_resources()
    {
        var brush = ContainerStatusToBrushConverter.Instance().Convert(
            new V1ContainerStatus { Name = "web", Ready = true, Started = true },
            typeof(IBrush),
            null,
            CultureInfo.InvariantCulture);

        brush.ShouldBeAssignableTo<IBrush>();
    }

    [AvaloniaFact]
    public async Task Tooltip_viewmodel_contains_type_status_restarts_and_image()
    {
        var pod = new V1Pod
        {
            Spec = new V1PodSpec
            {
                Containers = new List<V1Container>
                {
                    new() { Name = "normal1", Image = "normal:image" }
                },
                InitContainers = new List<V1Container>
                {
                    new() { Name = "init1", Image = "init:image" }
                },
                EphemeralContainers = new List<V1EphemeralContainer>
                {
                    new() { Name = "ephemeral1", Image = "ephemeral:image" }
                }
            },
            Status = new V1PodStatus
            {
                ContainerStatuses = new List<V1ContainerStatus>
                {
                    new() { Name = "normal1", Ready = true, Started = true, RestartCount = 2 }
                },
                InitContainerStatuses = new List<V1ContainerStatus>
                {
                    new() { Name = "init1", Ready = false, Started = false, State = new V1ContainerState { Waiting = new V1ContainerStateWaiting { Reason = "InitWaiting" } }, RestartCount = 0 }
                },
                EphemeralContainerStatuses = new List<V1ContainerStatus>
                {
                    new() { Name = "ephemeral1", Ready = false, Started = false, State = new V1ContainerState { Waiting = new V1ContainerStateWaiting { Reason = "CreateContainerConfigError", Message = "image will run as root" } }, RestartCount = 0 }
                }
            }
        };

        var view = new PodContainerCellView
        {
            DataContext = pod
        };

        var window = new Window { Content = view };
        try
        {
            window.Show();
            await TestApplicationExtensions.WaitForUiAsync();
            await TestApplicationExtensions.WaitForUiAsync();

            var items = view.ContainerStatuses;
            items.ShouldNotBeNull();
            items.Count.ShouldBe(3);

            var normal = items.First(i => i.Name == "normal1");
            normal.Type.ShouldBe("Normal");
            normal.Status.ShouldBe("Running");
            normal.Restarts.ShouldBe(2);
            normal.Image.ShouldBe("normal:image");

            var init = items.First(i => i.Name == "init1");
            init.Type.ShouldBe("Init");
            init.Status.ShouldBe("InitWaiting");
            init.Restarts.ShouldBe(0);
            init.Image.ShouldBe("init:image");

            var eph = items.First(i => i.Name == "ephemeral1");
            eph.Type.ShouldBe("Ephemeral");
            eph.Status.ShouldBe("CreateContainerConfigError");
            eph.Restarts.ShouldBe(0);
            eph.Image.ShouldBe("ephemeral:image");
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task refresh_keeps_a_stable_container_statuses_collection_instance()
    {
        var firstPod = new V1Pod
        {
            Status = new V1PodStatus
            {
                ContainerStatuses = new List<V1ContainerStatus>
                {
                    new() { Name = "normal1", Ready = true, Started = true, RestartCount = 1 }
                }
            }
        };

        var secondPod = new V1Pod
        {
            Status = new V1PodStatus
            {
                ContainerStatuses = new List<V1ContainerStatus>
                {
                    new() { Name = "normal2", Ready = false, Started = false, RestartCount = 0 }
                }
            }
        };

        var view = new PodContainerCellView
        {
            DataContext = firstPod
        };

        var window = new Window { Content = view };
        window.Show();
        await TestApplicationExtensions.WaitForUiAsync();

        var statusesBeforeRefresh = view.ContainerStatuses;
        statusesBeforeRefresh.ShouldNotBeNull();

        view.DataContext = secondPod;
        await TestApplicationExtensions.WaitForUiAsync();

        view.ContainerStatuses.ShouldBeSameAs(statusesBeforeRefresh);
        view.ContainerStatuses.Count.ShouldBe(1);
        view.ContainerStatuses[0].Name.ShouldBe("normal2");

        window.Content = null;
        window.Close();
    }
}
