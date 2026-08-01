using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using KubeUI.Avalonia.Features.Clusters.Workspace;
using KubeUI.Avalonia.Shell.Navigation;
using KubeUI.Avalonia.Tests.Infra;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Shell.Navigation;

public sealed class LimitedAccessNavigationTests
{
    [AvaloniaTheory, KubernetesBackendData]
    [Trait("Category", "Kind")]
    public async Task limited_access_with_listable_namespace_shows_namespaced_resources_in_navigation(KubernetesBackend backend)
    {
        var services = Application.Current.GetTestServices();
        var workspace = await Application.Current.CreateClusterAsync(config =>
        {
            config.Type = backend;
            config.AuthenticatedUser = KubernetesRbac.ServiceAccountUser;
            config.InitialYaml = KubernetesTestData.LimitedAccessWithNamespacePermissions;
        });

        var navigation = services.GetRequiredService<NavigationViewModel>();

        var clusterNode = navigation.Clusters.Single(x => x.Cluster == workspace);
        await navigation.TreeViewSelectionChangedAsync(clusterNode);

        await TestWait.UntilAsync(
            () =>
            {
                Dispatcher.UIThread.RunJobs();
                return FindResourceLink(clusterNode, "Pods") != null
                    && FindResourceLink(clusterNode, "Deployments") != null;
            },
            30000,
            TestContext.Current.CancellationToken);

        workspace.GetResourceConfig<k8s.Models.V1Pod>().PermissionsLoaded.ShouldBeTrue();
        workspace.GetResourceConfig<k8s.Models.V1Pod>().CanListAndWatch.ShouldBeTrue();
        workspace.GetResourceConfig<k8s.Models.V1Deployment>().PermissionsLoaded.ShouldBeTrue();
        workspace.GetResourceConfig<k8s.Models.V1Deployment>().CanListAndWatch.ShouldBeTrue();

        var podsLink = FindResourceLink(clusterNode, "Pods");
        var deploymentsLink = FindResourceLink(clusterNode, "Deployments");
        podsLink.ShouldNotBeNull();
        deploymentsLink.ShouldNotBeNull();

        await navigation.TreeViewSelectionChangedAsync(podsLink).WaitAsync(TestContext.Current.CancellationToken);
        await navigation.TreeViewSelectionChangedAsync(deploymentsLink).WaitAsync(TestContext.Current.CancellationToken);
    }

    private static ResourceNavigationLink? FindResourceLink(ClusterNavigationNode root, string name)
    {
        return FindResourceLink(root.NavigationItems, name);
    }

    private static ResourceNavigationLink? FindResourceLink(IEnumerable<NavigationItem> items, string name)
    {
        foreach (var child in items)
        {
            if (child is ResourceNavigationLink resourceLink && string.Equals(resourceLink.Name, name, StringComparison.Ordinal))
            {
                return resourceLink;
            }

            var nested = FindResourceLink(child.NavigationItems, name);
            if (nested != null)
            {
                return nested;
            }
        }

        return null;
    }
}
