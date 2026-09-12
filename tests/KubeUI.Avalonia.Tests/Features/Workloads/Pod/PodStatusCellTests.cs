using System.Reflection;
using Avalonia.Controls;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Kubernetes;
using KubeUI.Avalonia.Resources.Workloads.v1.Pod;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.Workloads.Pod;

public sealed class PodStatusCellTests
{
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
        await TestApplicationExtensions.WaitForUiAsync();
        cell.Text.ShouldBe(Assets.Resources.PodStatusCell_Running);

        window.Content = cell;
        await TestApplicationExtensions.WaitForUiAsync();
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
    public async Task pod_status_cell_refreshes_when_cluster_change_reuses_same_pod_instance()
    {
        var cluster = await Application.Current.CreateClusterAsync();
        await cluster.Runtime.SeedResource<V1Pod>(true);
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
        await cluster.Runtime.AddOrUpdateResource(pod);

        var cell = new PodStatusCellView { DataContext = pod };
        cell.Initialize(cluster);
        var window = new Window { Content = cell };

        try
        {
            window.Show();
            await TestApplicationExtensions.WaitForUiAsync();
            cell.Text.ShouldBe(Assets.Resources.PodStatusCell_Terminating);

            pod.Metadata.DeletionTimestamp = null;
            pod.Status.Conditions[0].Status = "True";
            var onChange = typeof(Cluster).GetField("OnChange", BindingFlags.Instance | BindingFlags.NonPublic)
                ?.GetValue(cluster.Runtime);
            var handler = onChange.ShouldBeAssignableTo<Action<WatchEventType, GroupApiVersionKind, IKubernetesObject<V1ObjectMeta>>>();
            handler(WatchEventType.Modified, GroupApiVersionKind.From<V1Pod>(), pod);
            await TestApplicationExtensions.WaitForUiAsync();

            cell.Text.ShouldBe(Assets.Resources.PodStatusCell_Running);
        }
        finally
        {
            window.Close();
        }
    }
}
