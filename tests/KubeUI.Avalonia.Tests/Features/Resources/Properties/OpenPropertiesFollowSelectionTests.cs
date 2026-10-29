using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using Dock.Model.Controls;
using Dock.Model.Core;
using DynamicData;
using k8s;
using k8s.Models;
using KubernetesClient.Informer.Client;
using KubeUI.Avalonia.Infrastructure.Docking;
using KubeUI.Avalonia.Resources;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Features.Resources.Properties;

public sealed class OpenPropertiesFollowSelectionTests
{
    [AvaloniaFact]
    public async Task open_properties_follow_list_selection_in_place()
    {
        var (_, _, rightDock) = CreateLayout();
        using var cluster = await Application.Current.CreateClusterAsync();
        using var list = await CreatePodListAsync(cluster, Pod("api"), Pod("worker"));
        list.ResourceConfig.View(new[] { SelectByName(list, "api") });
        var properties = rightDock.VisibleDockables!.OfType<ResourcePropertiesViewModel<V1Pod>>().Single();

        var worker = SelectByName(list, "worker");
        await WaitForAsync(() => ReferenceEquals(properties.Object, worker));

        rightDock.VisibleDockables!.OfType<ResourcePropertiesViewModel<V1Pod>>().ShouldHaveSingleItem().ShouldBeSameAs(properties);
        properties.Cluster.ShouldBeSameAs(cluster);
    }

    [AvaloniaFact]
    public async Task auto_hidden_properties_follow_list_selection_without_expanding()
    {
        var (factory, root, rightDock) = CreateLayout();
        using var cluster = await Application.Current.CreateClusterAsync();
        using var list = await CreatePodListAsync(cluster, Pod("api"), Pod("worker"));
        list.ResourceConfig.View(new[] { SelectByName(list, "api") });
        var properties = rightDock.VisibleDockables!.OfType<ResourcePropertiesViewModel<V1Pod>>().Single();
        factory.PinDockable(properties);
        root.RightPinnedDockables.ShouldContain(properties);

        var worker = SelectByName(list, "worker");
        await WaitForAsync(() => ReferenceEquals(properties.Object, worker));

        root.RightPinnedDockables!.ShouldHaveSingleItem().ShouldBeSameAs(properties);
        rightDock.VisibleDockables.ShouldNotContain(properties);
        root.PinnedDock?.VisibleDockables?.ShouldNotContain(properties);
    }

    [AvaloniaFact]
    public async Task selection_does_not_open_properties_when_none_are_open()
    {
        var (_, root, rightDock) = CreateLayout();
        using var cluster = await Application.Current.CreateClusterAsync();
        using var list = await CreatePodListAsync(cluster, Pod("api"));

        SelectByName(list, "api");
        Dispatcher.UIThread.RunJobs();

        rightDock.VisibleDockables!.OfType<ResourcePropertiesViewModel<V1Pod>>().ShouldBeEmpty();
        (root.RightPinnedDockables ?? []).ShouldBeEmpty();
    }

    [AvaloniaFact]
    public async Task open_properties_keep_current_object_for_multi_selection()
    {
        var (_, _, rightDock) = CreateLayout();
        using var cluster = await Application.Current.CreateClusterAsync();
        using var list = await CreatePodListAsync(cluster, Pod("api"), Pod("worker"));
        var api = SelectByName(list, "api");
        list.ResourceConfig.View(new[] { api });
        var properties = rightDock.VisibleDockables!.OfType<ResourcePropertiesViewModel<V1Pod>>().Single();

        list.SelectionModel.Select(IndexOf(list, "worker"));
        await WaitForAsync(() => list.SelectedItems?.Count == 2);

        properties.Object.ShouldBeSameAs(api);
    }

    [AvaloniaFact]
    public async Task auto_hidden_properties_switch_to_another_resource_kind_and_stay_hidden()
    {
        var (factory, root, rightDock) = CreateLayout();
        using var cluster = await Application.Current.CreateClusterAsync();
        using var pods = await CreatePodListAsync(cluster, Pod("api"));
        pods.ResourceConfig.View(new[] { SelectByName(pods, "api") });
        var podProperties = rightDock.VisibleDockables!.OfType<ResourcePropertiesViewModel<V1Pod>>().Single();
        factory.PinDockable(podProperties);
        using var deployments = Application.Current.GetRequiredTestService<ResourceListViewModel<V1Deployment>>();
        deployments.Initialize(cluster);
        var deployment = new V1Deployment
        {
            ApiVersion = V1Deployment.KubeApiVersion,
            Kind = V1Deployment.KubeKind,
            Metadata = new V1ObjectMeta { Name = "web", NamespaceProperty = "team-a" },
        };
        deployments.Objects.AddOrUpdate(deployment);
        await WaitForAsync(() => deployments.ItemCount == 1);

        deployments.SelectionModel.Select(0);

        await WaitForAsync(() => root.RightPinnedDockables!.OfType<ResourcePropertiesViewModel<V1Deployment>>().Any());
        var deploymentProperties = root.RightPinnedDockables!.ShouldHaveSingleItem().ShouldBeOfType<ResourcePropertiesViewModel<V1Deployment>>();
        deploymentProperties.Object.ShouldBeSameAs(deployment);
        rightDock.VisibleDockables!.ShouldNotContain(deploymentProperties);
        podProperties.Cluster.ShouldBeNull();
    }

    [AvaloniaFact]
    public async Task open_properties_switch_to_another_resource_kind_in_place()
    {
        var (_, _, rightDock) = CreateLayout();
        using var cluster = await Application.Current.CreateClusterAsync();
        using var pods = await CreatePodListAsync(cluster, Pod("api"));
        pods.ResourceConfig.View(new[] { SelectByName(pods, "api") });
        var podProperties = rightDock.VisibleDockables!.OfType<ResourcePropertiesViewModel<V1Pod>>().Single();
        var index = rightDock.VisibleDockables!.IndexOf(podProperties);
        using var deployments = Application.Current.GetRequiredTestService<ResourceListViewModel<V1Deployment>>();
        deployments.Initialize(cluster);
        var deployment = new V1Deployment
        {
            ApiVersion = V1Deployment.KubeApiVersion,
            Kind = V1Deployment.KubeKind,
            Metadata = new V1ObjectMeta { Name = "web", NamespaceProperty = "team-a" },
        };
        deployments.Objects.AddOrUpdate(deployment);
        await WaitForAsync(() => deployments.ItemCount == 1);

        deployments.SelectionModel.Select(0);

        await WaitForAsync(() => rightDock.VisibleDockables!.OfType<ResourcePropertiesViewModel<V1Deployment>>().Any());
        var deploymentProperties = rightDock.VisibleDockables!.OfType<ResourcePropertiesViewModel<V1Deployment>>().Single();
        deploymentProperties.Object.ShouldBeSameAs(deployment);
        rightDock.VisibleDockables!.IndexOf(deploymentProperties).ShouldBe(index);
        rightDock.ActiveDockable.ShouldBeSameAs(deploymentProperties);
        rightDock.VisibleDockables.ShouldNotContain(podProperties);
        podProperties.Cluster.ShouldBeNull();
    }

    [AvaloniaFact]
    public async Task viewing_another_resource_releases_replaced_properties_view()
    {
        var (_, _, rightDock) = CreateLayout();
        using var cluster = await Application.Current.CreateClusterAsync();
        var config = (ResourceConfigBase<V1Pod>)cluster.GetResourceConfig(GroupApiVersionKind.From<V1Pod>());
        config.View(new[] { Pod("api") });
        var first = rightDock.VisibleDockables!.OfType<ResourcePropertiesViewModel<V1Pod>>().Single();

        config.View(new[] { Pod("worker") });

        first.Cluster.ShouldBeNull();
    }

    [AvaloniaFact]
    public async Task show_resource_moves_the_change_subscription_to_the_new_cluster()
    {
        using var first = await Application.Current.CreateClusterAsync();
        using var second = await Application.Current.CreateClusterAsync();
        using var properties = Application.Current.GetRequiredTestService<ResourcePropertiesViewModel<V1Pod>>();
        properties.Initialize(first, Pod("api"));

        var moved = Pod("worker");
        properties.ShowResource(second, moved);

        properties.Cluster.ShouldBeSameAs(second);
        properties.Object.ShouldBeSameAs(moved);
        properties.ResourceConfig.ShouldBeSameAs(second.GetResourceConfig(GroupApiVersionKind.From<V1Pod>()));
    }

    [AvaloniaFact]
    public async Task queued_update_for_previous_resource_is_ignored_after_retarget()
    {
        using var cluster = await Application.Current.CreateClusterAsync();
        using var properties = Application.Current.GetRequiredTestService<ResourcePropertiesViewModel<V1Pod>>();
        properties.Initialize(cluster, Pod("api"));
        var worker = Pod("worker");

        properties.Cluster_OnChange(WatchEventType.Modified, GroupApiVersionKind.From<V1Pod>(), Pod("api"));
        properties.ShowResource(cluster, worker);
        Dispatcher.UIThread.RunJobs();

        properties.Object.ShouldBeSameAs(worker);
    }

    [AvaloniaFact]
    public async Task queued_update_from_previous_cluster_is_ignored_after_retarget()
    {
        using var first = await Application.Current.CreateClusterAsync();
        using var second = await Application.Current.CreateClusterAsync();
        using var properties = Application.Current.GetRequiredTestService<ResourcePropertiesViewModel<V1Pod>>();
        properties.Initialize(first, Pod("api"));
        var sameNameInSecond = Pod("api");

        properties.Cluster_OnChange(WatchEventType.Modified, GroupApiVersionKind.From<V1Pod>(), Pod("api"));
        properties.ShowResource(second, sameNameInSecond);
        Dispatcher.UIThread.RunJobs();

        properties.Object.ShouldBeSameAs(sameNameInSecond);
    }

    [AvaloniaFact]
    public async Task restoring_unchanged_selection_updates_properties()
    {
        var (_, _, rightDock) = CreateLayout();
        using var cluster = await Application.Current.CreateClusterAsync();
        using var first = await CreatePodListAsync(cluster, Pod("api"), Pod("worker"));
        using var second = await CreatePodListAsync(cluster, Pod("api"), Pod("worker"));
        var api = SelectByName(first, "api");
        first.CaptureSelectionState();
        first.ResourceConfig.View(new[] { api });
        var properties = rightDock.VisibleDockables!.OfType<ResourcePropertiesViewModel<V1Pod>>().Single();
        var worker = SelectByName(second, "worker");
        properties.Object.ShouldBeSameAs(worker);

        first.RestoreSelectionState();

        properties.Object.ShouldBeSameAs(api);
    }

    [AvaloniaFact]
    public async Task replacing_auto_hidden_properties_preserves_focus()
    {
        var (factory, root, rightDock) = CreateLayout();
        using var cluster = await Application.Current.CreateClusterAsync();
        using var pods = await CreatePodListAsync(cluster, Pod("api"));
        pods.ResourceConfig.View(new[] { SelectByName(pods, "api") });
        var properties = rightDock.VisibleDockables!.OfType<ResourcePropertiesViewModel<V1Pod>>().Single();
        factory.PinDockable(properties);
        var documents = factory.GetDockable<IDocumentDock>("Documents")!;
        factory.InsertDockable(documents, pods, 0);
        factory.SetFocusedDockable(documents, pods);
        using var deployments = Application.Current.GetRequiredTestService<ResourceListViewModel<V1Deployment>>();
        deployments.Initialize(cluster);
        deployments.Objects.AddOrUpdate(new V1Deployment { ApiVersion = V1Deployment.KubeApiVersion, Kind = V1Deployment.KubeKind, Metadata = new V1ObjectMeta { Name = "web", NamespaceProperty = "team-a" } });
        await WaitForAsync(() => deployments.ItemCount == 1);

        deployments.SelectionModel.Select(0);

        root.FocusedDockable.ShouldBeSameAs(pods);
    }

    [AvaloniaFact]
    public async Task in_flight_callback_from_previous_cluster_is_ignored()
    {
        using var first = await Application.Current.CreateClusterAsync();
        using var second = await Application.Current.CreateClusterAsync();
        using var properties = Application.Current.GetRequiredTestService<ResourcePropertiesViewModel<V1Pod>>();
        properties.Initialize(first, Pod("api"));
        var current = Pod("api");
        properties.ShowResource(second, current);

        properties.OnClusterChange(first, Pod("api"));
        Dispatcher.UIThread.RunJobs();

        properties.Object.ShouldBeSameAs(current);
    }

    private static (IFactory Factory, IRootDock Root, IToolDock RightDock) CreateLayout()
    {
        var factory = Application.Current.GetTestServices().GetRequiredService<IFactory>();
        var layout = factory.CreateLayout();
        factory.InitLayout(layout);
        return (factory, factory.GetDockable<IRootDock>("Root")!, factory.GetDockable<IToolDock>("RightDock")!);
    }

    private static async Task<ResourceListViewModel<V1Pod>> CreatePodListAsync(ClusterWorkspace cluster, params V1Pod[] pods)
    {
        var list = Application.Current.GetRequiredTestService<ResourceListViewModel<V1Pod>>();
        list.Initialize(cluster);
        list.Objects.AddOrUpdate(pods);
        await WaitForAsync(() => list.ItemCount == pods.Length);
        return list;
    }

    private static V1Pod SelectByName(ResourceListViewModel<V1Pod> list, string name)
    {
        list.SelectionModel.Clear();
        list.SelectionModel.Select(IndexOf(list, name));
        return list.SelectedItem.ShouldNotBeNull();
    }

    private static int IndexOf(ResourceListViewModel<V1Pod> list, string name)
    {
        for (var i = 0; ; i++)
        {
            list.SelectionModel.Source.ShouldNotBeNull();
            var item = list.SelectionModel.Source.Cast<object>().ElementAt(i);
            if (item is V1Pod pod && pod.Metadata.Name == name)
            {
                return i;
            }
        }
    }

    private static V1Pod Pod(string name) => new()
    {
        ApiVersion = V1Pod.KubeApiVersion,
        Kind = V1Pod.KubeKind,
        Metadata = new V1ObjectMeta { Name = name, NamespaceProperty = "team-a" },
    };

    private static Task WaitForAsync(Func<bool> condition)
        => TestWait.UntilAsync(
            condition,
            5000,
            TestContext.Current.CancellationToken,
            () => Dispatcher.UIThread.RunJobs());
}
