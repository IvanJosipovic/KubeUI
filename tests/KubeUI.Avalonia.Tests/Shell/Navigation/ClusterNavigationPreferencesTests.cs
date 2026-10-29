using Avalonia.Headless.XUnit;
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
        vm.Clusters.ShouldContain(node => node.Cluster == workspace);
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

}
