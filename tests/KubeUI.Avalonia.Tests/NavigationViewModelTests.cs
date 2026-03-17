using Avalonia;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using System.Linq;
using KubeUI.Avalonia.Tests.Infra;
using KubeUI.Client;
using KubeUI.ViewModels;
using Shouldly;

namespace KubeUI.Avalonia.Tests;

public class NavigationViewModelTests
{
    [AvaloniaFact]
    public async Task resource_navigation_items_populate_only_after_connect_starts()
    {
        var runtime = new TestCluster
        {
            Connected = false,
            Status = ClusterStatus.None,
        };

        var workspace = runtime.CreateWorkspace();

        var vm = Application.Current.GetRequiredService<NavigationViewModel>();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);

        clusterNode.NavigationItems.Count.ShouldBe(0);

        vm.TreeView_SelectionChanged(clusterNode);
        await Task.Delay(10);
        Dispatcher.UIThread.RunJobs();

        clusterNode.NavigationItems.Count.ShouldBeGreaterThan(0);
    }

    [AvaloniaFact]
    public async Task selecting_cluster_node_does_not_crash_when_connect_fails()
    {
        var runtime = new TestCluster
        {
            Connected = false,
            Status = ClusterStatus.None,
            ConnectBehavior = () => throw new InvalidOperationException("connect failed"),
        };

        var workspace = runtime.CreateWorkspace();

        var vm = Application.Current.GetRequiredService<NavigationViewModel>();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);

        vm.TreeView_SelectionChanged(clusterNode);
        await Task.Delay(10);
        Dispatcher.UIThread.RunJobs();

        clusterNode.NavigationItems.Count.ShouldBe(0);
    }

    [AvaloniaFact]
    public async Task cluster_node_expands_after_successful_connect()
    {
        var runtime = new TestCluster
        {
            Connected = false,
            Status = ClusterStatus.None,
        };

        var workspace = runtime.CreateWorkspace();

        var vm = Application.Current.GetRequiredService<NavigationViewModel>();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        clusterNode.IsExpanded.ShouldBeFalse();

        vm.TreeView_SelectionChanged(clusterNode);
        await Task.Delay(10);
        Dispatcher.UIThread.RunJobs();

        clusterNode.Cluster.Connected.ShouldBeTrue();
        clusterNode.IsExpanded.ShouldBeTrue();
    }

    [AvaloniaFact]
    public void navigation_items_rebuild_after_navigation_ready_pulse()
    {
        var runtime = new TestCluster
        {
            Connected = true,
            Status = ClusterStatus.Connected,
        };

        var workspace = runtime.CreateWorkspace();
        workspace.NavigationReady = false;

        var vm = Application.Current.GetRequiredService<NavigationViewModel>();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        workspace.NavigationReady = false;
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        var rebuilds = 0;
        clusterNode.NavigationItems.CollectionChanged += (_, _) => rebuilds++;

        workspace.NavigationReady = true;
        Dispatcher.UIThread.RunJobs();

        rebuilds.ShouldBeGreaterThan(0);
    }

    [AvaloniaFact]
    public void port_forwarders_is_under_network_category_not_top_level()
    {
        var runtime = new TestCluster();
        var workspace = runtime.CreateWorkspace();

        var vm = Application.Current.GetRequiredService<NavigationViewModel>();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);

        var networkCategory = clusterNode.NavigationItems.Single(x => x.Name == "Network");
        var portForwardersInNetwork = networkCategory.NavigationItems
            .OfType<NavigationLink>()
            .SingleOrDefault(x => x.ViewModelKey == NavigationTargets.PortForwarders);

        portForwardersInNetwork.ShouldNotBeNull();

        var topLevelPortForwarders = clusterNode.NavigationItems
            .OfType<NavigationLink>()
            .SingleOrDefault(x => x.ViewModelKey == NavigationTargets.PortForwarders);

        topLevelPortForwarders.ShouldBeNull();
    }

    [AvaloniaFact]
    public void port_forwarders_is_hidden_when_pod_portforward_is_not_allowed()
    {
        var runtime = new TestCluster
        {
            CanCreatePodPortForward = false,
        };

        var workspace = runtime.CreateWorkspace();

        var vm = Application.Current.GetRequiredService<NavigationViewModel>();
        vm.ClusterCatalog.Clusters.Add(workspace);
        Dispatcher.UIThread.RunJobs();

        var clusterNode = vm.Clusters.Single(x => x.Cluster == workspace);
        var networkCategory = clusterNode.NavigationItems.Single(x => x.Name == "Network");

        var portForwardersInNetwork = networkCategory.NavigationItems
            .OfType<NavigationLink>()
            .SingleOrDefault(x => x.ViewModelKey == NavigationTargets.PortForwarders);

        portForwardersInNetwork.ShouldBeNull();
    }
}
