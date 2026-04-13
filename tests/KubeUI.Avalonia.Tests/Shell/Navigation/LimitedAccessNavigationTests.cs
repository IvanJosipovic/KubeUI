using Avalonia;
using Avalonia.Headless.XUnit;
using Avalonia.Threading;
using KubeUI.Avalonia.Tests.Infra;
using KubeUI.Kubernetes.Tests.Infra;
using Microsoft.Extensions.DependencyInjection;
using Shouldly;

namespace KubeUI.Avalonia.Tests.Shell.Navigation;

public sealed class LimitedAccessNavigationTests : AvaloniaTestBase
{
    [AvaloniaFact]
    public async Task limited_access_with_namespace_fallback_shows_namespaced_resources_in_navigation()
    {
        await using var harness = new MockClusterScenarioHarness();
        await harness.InitializeAsync();

        var runtime = await harness.CreateLimitedAccessClusterAsync(includeNamespaceFallback: true);
        var services = TestApp.CurrentServices ?? throw new InvalidOperationException("Test services are not initialized.");
        var workspace = ActivatorUtilities.CreateInstance<ClusterWorkspaceViewModel>(services, runtime);
        var navigation = services.GetRequiredService<NavigationViewModel>();

        navigation.ClusterCatalog.Clusters.Add(workspace);
        await workspace.EnsureWorkspaceStateInitializedAsync();
        Dispatcher.UIThread.RunJobs();

        var clusterNode = navigation.Clusters.Single(x => x.Cluster == workspace);
        await navigation.TreeViewSelectionChangedAsync(clusterNode);

        await WaitForAsync(() =>
        {
            Dispatcher.UIThread.RunJobs();
            return FindResourceLink(clusterNode, "Pods") != null
                && FindResourceLink(clusterNode, "Deployments") != null;
        }, timeoutMs: 30000);

        workspace.GetResourceConfig<k8s.Models.V1Pod>().PermissionsLoaded.ShouldBeTrue();
        workspace.GetResourceConfig<k8s.Models.V1Pod>().CanListAndWatch.ShouldBeTrue();
        workspace.GetResourceConfig<k8s.Models.V1Deployment>().PermissionsLoaded.ShouldBeTrue();
        workspace.GetResourceConfig<k8s.Models.V1Deployment>().CanListAndWatch.ShouldBeTrue();
        FindResourceLink(clusterNode, "Pods").ShouldNotBeNull();
        FindResourceLink(clusterNode, "Deployments").ShouldNotBeNull();
    }

    private static async Task WaitForAsync(Func<bool> predicate, int timeoutMs)
    {
        var deadline = DateTime.UtcNow.AddMilliseconds(timeoutMs);

        while (DateTime.UtcNow < deadline)
        {
            if (predicate())
            {
                return;
            }

            await Task.Delay(100);
        }

        predicate().ShouldBeTrue();
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
