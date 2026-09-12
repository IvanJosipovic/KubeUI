using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using k8s.Models;
using KubeUI.Avalonia.Resources.Workloads.v1.Pod;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.Workloads.Pod;

public sealed class PodStatusCellTests
{
    [AvaloniaFact]
    public async Task pod_status_cell_updates_when_data_context_changes()
    {
        var runningPod = new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "status-pod-a",
                NamespaceProperty = "default",
            },
            Status = new V1PodStatus
            {
                Conditions = [new V1PodCondition { Type = "Ready", Status = "True" }],
            },
        };
        var terminatingPod = new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "status-pod-b",
                NamespaceProperty = "default",
                DeletionTimestamp = DateTime.UtcNow,
            },
            Status = runningPod.Status,
        };

        var cell = new PodStatusCellView();
        cell.DataContext = runningPod;
        var window = new Window { Content = cell };

        try
        {
            window.Show();
            await TestApplicationExtensions.WaitForUiAsync();

            cell.Text.ShouldBe(Assets.Resources.PodStatusCell_Running);
            cell.DataContext = terminatingPod;
            await TestApplicationExtensions.WaitForUiAsync();
            cell.Text.ShouldBe(Assets.Resources.PodStatusCell_Terminating);
        }
        finally
        {
            window.Close();
        }
    }

    [AvaloniaFact]
    public async Task pod_status_cell_refreshes_when_data_context_is_replaced()
    {
        var pod = new V1Pod
        {
            Metadata = new V1ObjectMeta
            {
                Name = "same-instance-pod",
                NamespaceProperty = "default",
                DeletionTimestamp = DateTime.UtcNow,
            },
            Status = new V1PodStatus
            {
                Conditions = [new V1PodCondition { Type = "Ready", Status = "False", Reason = "ContainersNotReady" }],
            },
        };
        var cell = new PodStatusCellView { DataContext = pod };
        var window = new Window { Content = cell };

        try
        {
            window.Show();
            await TestApplicationExtensions.WaitForUiAsync();
            cell.Text.ShouldBe(Assets.Resources.PodStatusCell_Terminating);

            cell.DataContext = new V1Pod
            {
                Metadata = new V1ObjectMeta { Name = pod.Name(), NamespaceProperty = pod.Namespace() },
                Status = new V1PodStatus
                {
                    Conditions = [new V1PodCondition { Type = "Ready", Status = "True" }],
                },
            };
            await TestApplicationExtensions.WaitForUiAsync();

            cell.Text.ShouldBe(Assets.Resources.PodStatusCell_Running);
        }
        finally
        {
            window.Close();
        }
    }
}
