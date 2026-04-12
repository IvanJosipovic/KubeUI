using System.Collections.Generic;
using System.Linq;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using k8s.Models;
using KubeUI.Avalonia.Resources.Workloads.v1.Pod.Controls;
using KubeUI.Avalonia.Tests.Infra;
using Shouldly;
using Xunit;

namespace KubeUI.Avalonia.Tests.Features.Workloads.Pod;

public sealed class PodContainerCellTests : AvaloniaTestBase
{
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

        var view = new PodContainerCell
        {
            DataContext = pod
        };

        var window = new Window { Content = view };
        window.Show();
        Dispatcher.UIThread.RunJobs();
        Dispatcher.UIThread.RunJobs();

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
}
