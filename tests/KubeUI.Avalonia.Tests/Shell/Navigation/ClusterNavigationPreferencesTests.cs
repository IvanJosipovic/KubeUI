using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using KubeUI.Avalonia.Shell.Navigation;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Shell.Navigation;

public sealed class ClusterNavigationPreferencesTests
{
    [AvaloniaFact]
    public void disabled_cluster_is_removed_from_navigation_and_restored_when_enabled()
    {
        var services = Application.Current.GetTestServices();
        var workspace = services.GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        using var vm = services.GetRequiredService<NavigationViewModel>();
        vm.Clusters.ShouldContain(node => node.Cluster == workspace);

        workspace.IsEnabled = false;
        vm.Clusters.ShouldNotContain(node => node.Cluster == workspace);

        workspace.IsEnabled = true;
        vm.Clusters.Single(node => node.Cluster == workspace).ToggleExpansionCommand.ShouldNotBeNull();
    }

    [AvaloniaFact]
    public void cluster_added_disabled_has_no_navigation_node()
    {
        var services = Application.Current.GetTestServices();
        var catalog = services.GetRequiredService<ClusterWorkspaceCatalog>();
        var workspace = catalog.Clusters.Single();
        using var vm = services.GetRequiredService<NavigationViewModel>();

        catalog.Clusters.Remove(workspace);
        workspace.IsEnabled = false;
        catalog.Clusters.Add(workspace);

        vm.Clusters.ShouldBeEmpty();
    }

    [AvaloniaFact]
    public async Task toggle_expansion_command_connects_then_toggles_expansion()
    {
        var services = Application.Current.GetTestServices();
        var workspace = services.GetRequiredService<ClusterWorkspaceCatalog>().Clusters.Single();
        using var vm = services.GetRequiredService<NavigationViewModel>();
        var node = vm.Clusters.Single(x => x.Cluster == workspace);
        node.ExpansionMenuHeader.ShouldBe(Assets.Resources.NavigationView_ContextMenu_Expand);

        await vm.ToggleClusterExpansionCommand.ExecuteAsync(null).WaitAsync(TestContext.Current.CancellationToken);
        await vm.ToggleClusterExpansionCommand.ExecuteAsync(node).WaitAsync(TestContext.Current.CancellationToken);
        await TestWait.UntilAsync(() => workspace.Runtime.Connected && node.IsExpanded, 5000, TestContext.Current.CancellationToken, () => Dispatcher.UIThread.RunJobs());
        node.ExpansionMenuHeader.ShouldBe(Assets.Resources.NavigationView_ContextMenu_Collapse);

        await vm.ToggleClusterExpansionCommand.ExecuteAsync(node).WaitAsync(TestContext.Current.CancellationToken);
        node.IsExpanded.ShouldBeFalse();
        node.ExpansionMenuHeader.ShouldBe(Assets.Resources.NavigationView_ContextMenu_Expand);

        await vm.ToggleClusterExpansionCommand.ExecuteAsync(node).WaitAsync(TestContext.Current.CancellationToken);
        node.IsExpanded.ShouldBeTrue();
    }
}
